﻿
/**
 * \file    Base3DScene.cs
 * \author  Lance Florian
 * \date    2015
 * \brief   Define Base3DScene and ComputeGeneratorsJob classes
 */

// system
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

// unity
using UnityEngine;
using UnityEngine.Events;
using HBP.UI.Module3D;
using UnityEngine.Rendering;
using System.Collections;
using CielaSpike;
using Tools.Unity;

namespace HBP.Module3D
{
    #region Struct
    /// <summary>
    /// Plot request to be send to the outside UI
    /// </summary>
    public struct SiteRequest
    {
        public bool spScene; /**< is a single patient scene ? */
        public int idSite1; /**< id of the first site */
        public int idSite2;  /**< id of second site */
        public string idPatient; /**< id of the patient corresponding to the first site */
        public string idPatient2;  /**< id of the patient corresponding to the second site*/
        public List<List<bool>> maskColumn; /**< masks of the sites :  dim[0] = columnds data nb, dim[1] = sites nb.  if true : the site is not excluded/blacklisted/column masked and is in a ROI (if there is at least one ROI, if not ROI defined, all the plots are considered inside a ROI) */

        public void display()
        {
            Debug.Log("plotRequest : " + spScene + " " + idSite1 + " " + idSite2 + " " + idPatient + " " + idPatient2 + " " + maskColumn.Count);
            if (maskColumn.Count > 0)
            {
                Debug.Log("size1 : " + maskColumn[0].Count);
                string mask = "";
                for (int ii = 0; ii < maskColumn[0].Count; ++ii)
                    mask += maskColumn[0][ii] + " ";
                Debug.Log("-> mask : " + mask);
            }
        }
    }

    /// <summary>
    /// Site info to be send to the UI
    /// </summary>
    public class SiteInfo
    {
        public SiteInfo(Site site, bool enabled, Vector3 position, SiteInformationDisplayMode mode = SiteInformationDisplayMode.IEEG, string name = "", string amplitude = "", string timeline = "", string height = "", string latency = "")
        {
            this.site = site;
            this.enabled = enabled;
            this.position = position;
            this.name = name;
            this.amplitude = amplitude;
            this.timeline = timeline;
            this.height = height;
            this.latency = latency;
            this.mode = mode;
        }

        public Site site = null;
        public bool enabled;
        public Vector3 position;
        public string name;
        public string amplitude;
        public string height;
        public string latency;
        public string timeline;
        public SiteInformationDisplayMode mode;
    }

    /// <summary>
    /// IRM cal values
    /// </summary>
    public struct MRICalValues
    {
        public float min;
        public float max;
        public float loadedCalMin;
        public float loadedCalMax;
        public float computedCalMin;
        public float computedCalMax;
    }

    /// <summary>
    /// IRMF data to be send to the UI
    /// </summary>
    public struct FMRIDataParameters
    {
        public bool singlePatient;
        public int columnId;
        public float alpha;
        public float calMin; 
        public float calMax;
        public MRICalValues calValues;
    }
    #endregion

    /// <summary>
    /// Class containing all the DLL data and the displayable Gameobjects of a 3D scene.
    /// </summary>
    [AddComponentMenu("Scenes/Base 3D Scene")]
    public abstract class Base3DScene : MonoBehaviour
    {
        #region Events
        public class EventManager
        {
            /// <summary>
            /// Event called when this scene is selected
            /// </summary>
            public GenericEvent<Base3DScene> OnSelectScene = new GenericEvent<Base3DScene>();
            /// <summary>
            /// Send the new selected id column to the UI (params : idColumn)
            /// </summary>
            public GenericEvent<int> OnSelectColumn = new GenericEvent<int>();
            /// <summary>
            /// Event called when a FMRI column is added
            /// </summary>
            public UnityEvent OnAddFMRIColumn = new UnityEvent();
            /// <summary>
            /// Event called when a FMRI column is removed
            /// </summary>
            public UnityEvent OnRemoveFMRIColumn = new UnityEvent();

            /// <summary>
            /// Event for updating cuts planes 
            /// </summary>
            public UnityEvent OnUpdatePlanes = new UnityEvent();
            /// <summary>
            /// Event for updating the planes cuts display in the cameras
            /// </summary>
            public UnityEvent OnModifyPlanesCuts = new UnityEvent();
            /// <summary>
            /// Event for updating the cuts images in the UI (params : textures, columnId, planeNb)
            /// </summary>
            public GenericEvent<List<Texture2D>, int, int> OnUpdateCutsInUI = new GenericEvent<List<Texture2D>, int, int>();
            
            /// <summary>
            /// Event for sending IRMF data parameters to UI (params : IRMFDataParameters)
            /// </summary>
            public GenericEvent<FMRIDataParameters> OnSendFMRIParameters = new GenericEvent<FMRIDataParameters>();

            /// <summary>
            /// Event called when changing the colors of the colormap
            /// </summary>
            public GenericEvent<ColorType> OnChangeColormap = new GenericEvent<ColorType>();
            /// <summary>
            /// Event for colormap values associated to a column id (params : minValue, middle, maxValue, id)
            /// </summary>
            public GenericEvent<float, float, float, Column3D> OnSendColorMapValues = new GenericEvent<float, float, float, Column3D>();

            /// <summary>
            /// Event for sending info in order to display a message in a scene screen (params : message, duration, width, height)
            /// </summary>
            public GenericEvent<string, float, int, int> OnDisplaySceneMessage = new GenericEvent<string, float, int, int>();
            /// <summary>
            /// Event for sending info in order to display a loadingbar in a scene screen (params : duration, width, height, value)
            /// </summary>
            public GenericEvent<float, int, int, float> OnDisplaySceneProgressBar = new GenericEvent<float, int, int, float>();

            /// <summary>
            /// UI event for sending a plot info request to the outside UI (params : plotRequest)
            /// </summary>
            public GenericEvent<SiteRequest> OnRequestSiteInformation = new GenericEvent<SiteRequest>();
            /// <summary>
            /// Occurs when a plot is clicked in the scene (params : id of the column, if = -1 use the current selected column id)
            /// </summary>
            public GenericEvent<int> OnClickSite = new GenericEvent<int>();
            /// <summary>
            /// Event called when clicking on a ROI
            /// </summary>
            public GenericEvent<int, int> OnSelectROI = new GenericEvent<int, int>();

            public GenericEvent<Vector3, int> OnCreateROIBubble = new GenericEvent<Vector3, int>();

            /// <summary>
            /// Event for updating time in the UI
            /// </summary>
            public UnityEvent OnUpdateTimeInUI = new UnityEvent();
            /// <summary>
            /// Ask the UI to udpate the ROI of all the scene columns
            /// </summary>
            public GenericEvent<int> OnAskRegionOfInterestUpdate = new GenericEvent<int>();
            /// <summary>
            /// Ask the camera manager to update the target for this scene
            /// </summary>
            public GenericEvent<Vector3> OnUpdateCameraTarget = new GenericEvent<Vector3>();
            /// <summary>
            /// Event for updating the IRM cal values in the UI
            /// </summary>
            public GenericEvent<MRICalValues> OnMRICalValuesUpdate = new GenericEvent<MRICalValues>();

            /// <summary>
            /// Event for sending mode specifications
            /// </summary>
            public GenericEvent<ModeSpecifications> OnSendModeSpecifications = new GenericEvent<ModeSpecifications>();
        }
        public EventManager Events = new EventManager();
        #endregion

        #region Properties
        /// <summary>
        /// Name of the scene
        /// </summary>
        public string Name
        {
            get
            {
                return Visualization.Name;
            }
        }
        /// <summary>
        /// Type of the scene (Single / Multi)
        /// </summary>
        public abstract SceneType Type { get; }
        
        private bool m_IsSelected;
        /// <summary>
        /// Is this scene selected ?
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return m_IsSelected;
            }
            set
            {
                bool wasSelected = m_IsSelected;
                m_IsSelected = value;
                if (m_IsSelected && !wasSelected)
                {
                    Events.OnSelectScene.Invoke(this);
                }
            }
        }
        
        /// <summary>
        /// List of the patients in this scene
        /// </summary>
        public ReadOnlyCollection<Data.Patient> Patients
        {
            get
            {
                return new ReadOnlyCollection<Data.Patient>(Visualization.Patients);
            }
        }

        /// <summary>
        /// Space between scenes in world space
        /// </summary>
        protected const int SPACE_BETWEEN_SCENES = 3000;

        protected Data.Visualization.Visualization m_Visualization;
        /// <summary>
        /// Visualization associated to this scene
        /// </summary>
        public virtual Data.Visualization.Visualization Visualization
        {
            get
            {
                return m_Visualization;
            }
            set
            {
                m_Visualization = value;
            }
        }

        protected List<Cut> m_Cuts = new List<Cut>();
        /// <summary>
        /// Cuts planes list
        /// </summary>
        public List<Cut> Cuts { get { return m_Cuts; } }

        /// <summary>
        /// Information about the scene
        /// </summary>
        public SceneStatesInfo SceneInformation { get; set; }

        protected ModesManager m_ModesManager = null;
        /// <summary>
        /// Modes of the scene
        /// </summary>
        public ModesManager ModesManager
        {
            get { return m_ModesManager; }
        }

        protected DisplayedObjects3DView m_DisplayedObjects = null;
        /// <summary>
        /// Displayable objects of the scene
        /// </summary>
        public DisplayedObjects3DView DisplayedObjects
        {
            get
            {
                return m_DisplayedObjects;
            }
        }

        protected Column3DManager m_ColumnManager = null;
        /// <summary>
        /// Column data manager
        /// </summary>
        public Column3DManager ColumnManager { get { return m_ColumnManager; } }
        
        /// <summary>
        /// ID of the selected column of this scene
        /// </summary>
        public int SelectedColumnID
        {
            get
            {
                return m_ColumnManager.SelectedColumnID;
            }
        }
        /// <summary>
        /// Are cut holes in MRI enabled ?
        /// </summary>
        public bool CutHolesEnabled
        {
            get
            {
                return SceneInformation.CutHolesEnabled;
            }
            set
            {
                if (!SceneInformation.MeshesLoaded || !SceneInformation.MRILoaded) return;

                SceneInformation.CutHolesEnabled = value;
                SceneInformation.CutMeshGeometryNeedsUpdate = true;
                SceneInformation.IsIEEGOutdated = true;
                foreach (Column3D column in m_ColumnManager.Columns)
                {
                    column.IsRenderingUpToDate = false;
                }
                m_ModesManager.UpdateMode(Mode.FunctionsId.UpdatePlane);
            }
        }
        /// <summary>
        /// Is the latency mode enabled ?
        /// </summary>
        public bool IsLatencyModeEnabled
        {
            get
            {
                return SceneInformation.DisplayCCEPMode;
            }
        }

        /// <summary>
        /// Handles triangle erasing
        /// </summary>
        protected TriEraser m_TriEraser = new TriEraser();
        /// <summary>
        /// Is the triangle eraser enabled ?
        /// </summary>
        public bool IsTriangleErasingEnabled
        {
            get
            {
                return m_TriEraser.IsEnabled;
            }
            set
            {
                m_TriEraser.IsEnabled = value;
            }
        }
        /// <summary>
        /// Mode of the triangle eraser
        /// </summary>
        public TriEraser.Mode TriangleErasingMode
        {
            get
            {
                return m_TriEraser.CurrentMode;
            }
            set
            {
                TriEraser.Mode previousMode = m_TriEraser.CurrentMode;
                m_TriEraser.CurrentMode = value;

                if (value == TriEraser.Mode.Expand || value == TriEraser.Mode.Invert)
                {
                    m_TriEraser.EraseTriangles(new Vector3(), new Vector3());
                    for (int ii = 0; ii < m_ColumnManager.DLLSplittedMeshesList.Count; ++ii)
                        m_ColumnManager.DLLSplittedMeshesList[ii].UpdateMeshFromDLL(m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshFilter>().mesh);
                    m_TriEraser.CurrentMode = previousMode;
                }
            }
        }
        /// <summary>
        /// Degrees limit when selecting a zone with the triangle eraser
        /// </summary>
        public float TriangleErasingZoneDegrees
        {
            get
            {
                return m_TriEraser.Degrees;
            }
            set
            {
                m_TriEraser.Degrees = value;
            }
        }

        /// <summary>
        /// Are Mars Atlas colors displayed ?
        /// </summary>
        public bool IsMarsAtlasEnabled
        {
            get
            {
                return SceneInformation.MarsAtlasModeEnabled;
            }
            set
            {
                SceneInformation.MarsAtlasModeEnabled = value;
                m_DisplayedObjects.BrainSurfaceMeshes[0].GetComponent<Renderer>().sharedMaterial.SetInt("_MarsAtlas", SceneInformation.MarsAtlasModeEnabled ? 1 : 0);
            }
        }
        
        private bool m_EdgeMode = false;
        /// <summary>
        /// Are the edges displayed ?
        /// </summary>
        public bool EdgeMode
        {
            get
            {
                return m_EdgeMode;
            }
            set
            {
                m_EdgeMode = value;
                foreach (Column3D column in m_ColumnManager.Columns)
                {
                    foreach (View3D view in column.Views)
                    {
                        view.EdgeMode = m_EdgeMode;
                    }
                }
            }
        }

        private bool m_AutomaticRotation = false;
        /// <summary>
        /// Are the brains automatically rotating ?
        /// </summary>
        public bool AutomaticRotation
        {
            get
            {
                return m_AutomaticRotation;
            }
            set
            {
                m_AutomaticRotation = value;
                foreach (Column3D column in m_ColumnManager.Columns)
                {
                    foreach (View3D view in column.Views)
                    {
                        view.AutomaticRotation = m_AutomaticRotation;
                    }
                }
            }
        }

        private float m_AutomaticRotationSpeed = 30.0f;
        /// <summary>
        /// Automatic rotation speed
        /// </summary>
        public float AutomaticRotationSpeed
        {
            get
            {
                return m_AutomaticRotationSpeed;
            }
            set
            {
                m_AutomaticRotationSpeed = value;
                foreach (Column3D column in m_ColumnManager.Columns)
                {
                    foreach (View3D view in column.Views)
                    {
                        view.AutomaticRotationSpeed = m_AutomaticRotationSpeed;
                    }
                }
            }
        }

        /// <summary>
        /// Ambient mode (rendering)
        /// </summary>
        public AmbientMode AmbientMode = AmbientMode.Flat;
        /// <summary>
        /// Ambient intensity (rendering)
        /// </summary>
        public float AmbientIntensity = 1;
        /// <summary>
        /// Ambient light (rendering)
        /// </summary>
        public Color AmbientLight = new Color(0.2f, 0.2f, 0.2f, 1);

        /// <summary>
        /// Is ROI creation mode activated ?
        /// </summary>
        public bool ROICreation
        {
            get
            {
                return SceneInformation.IsROICreationModeEnabled;
            }
            set
            {
                SceneInformation.IsROICreationModeEnabled = value;
                ColumnManager.UpdateROIVisibility(value);
            }
        }

        [SerializeField]
        public GameObject m_CutPrefab;
        #endregion

        #region Private Methods
        protected void Awake()
        {         
            int idScript = TimeExecution.ID;
            TimeExecution.StartAwake(idScript, TimeExecution.ScriptsId.Base3DScene);
            
            m_DisplayedObjects = new DisplayedObjects3DView();
            SceneInformation = new SceneStatesInfo();
            m_ColumnManager = GetComponent<Column3DManager>();

            // Init materials
            SharedMaterials.Brain.AddSceneMaterials(this);

            // set meshes layer
            switch (Type)
            {
                case SceneType.SinglePatient:
                    SceneInformation.MeshesLayerName = "Default";
                    break;
                case SceneType.MultiPatients:
                    SceneInformation.MeshesLayerName = "Default";
                    break;
            }

            // init modes            
            m_ModesManager = transform.Find("Modes").gameObject.GetComponent<ModesManager>();
            m_ModesManager.Initialize(this);
            m_ModesManager.SendModeSpecifications.AddListener((specs) =>
            {
                Events.OnSendModeSpecifications.Invoke(specs);

                // update scene visibility (useless)
                //UnityEngine.Profiling.Profiler.BeginSample("TEST-Base3DScene-SendModeSpecifications update_scene_items_visibility");
                //    update_scene_items_visibility(specs.itemMaskDisplay[0], specs.itemMaskDisplay[1], specs.itemMaskDisplay[2]);
                //UnityEngine.Profiling.Profiler.EndSample();
            });
            
            AddListeners();
            // init GO
            InitializeSceneGameObjects();

            TimeExecution.EndAwake(idScript, TimeExecution.ScriptsId.Base3DScene, gameObject);
        }
        protected void Update()
        {
            UnityEngine.Profiling.Profiler.BeginSample("TEST-Base3DScene-Update: set_current_mode_specifications");
            SetCurrentModeSpecifications();
            UnityEngine.Profiling.Profiler.EndSample();

            if (m_ModesManager.CurrentModeID == Mode.ModesId.NoPathDefined)
                return;

            UnityEngine.Profiling.Profiler.BeginSample("TEST-Base3DScene-Update");

            // TEMP : useless
            for (int ii = 0; ii < m_Cuts.Count; ++ii)
                m_Cuts[ii].RemoveFrontPlane = 0;

            // check if we must perform new cuts of the brain            
            if (SceneInformation.CutMeshGeometryNeedsUpdate)
            {
                SceneInformation.IsGeometryUpToDate = false;
                ColumnManager.PlanesCutsCopy = m_Cuts;

                UnityEngine.Profiling.Profiler.BeginSample("TEST-Base3DScene-Update compute_meshes_cuts 1");
                ComputeMeshesCut();
                UnityEngine.Profiling.Profiler.EndSample();
                UnityEngine.Profiling.Profiler.BeginSample("TEST-Base3DScene-Update compute_MRI_textures 1");

                // create textures 
                ComputeMRITextures();

                ComputeFMRITextures(-1, -1);

                SceneInformation.IsGeometryUpToDate = true;

                UnityEngine.Profiling.Profiler.EndSample();
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
        /// <summary>
        /// Add every listeners required for the scene
        /// </summary>
        private void AddListeners()
        {
            ApplicationState.Module3D.OnSelectColumn.AddListener((column) =>
            {
                // force mode to update UI
                m_ModesManager.SetCurrentModeSpecifications(true);

                ComputeGUITextures(-1, m_ColumnManager.SelectedColumnID);
                UpdateGUITextures();
            });
            ApplicationState.Module3D.OnChangeNumberOfROI.AddListener(() =>
            {
                UpdateCurrentRegionOfInterest(m_ColumnManager.SelectedColumn);
            });
            ApplicationState.Module3D.OnChangeROIVolumeRadius.AddListener(() =>
            {
                UpdateCurrentRegionOfInterest(m_ColumnManager.SelectedColumn);
            });
            ApplicationState.Module3D.OnChangeNumberOfVolumeInROI.AddListener(() =>
            {
                UpdateCurrentRegionOfInterest(m_ColumnManager.SelectedColumn);
            });
            ApplicationState.Module3D.OnSelectROI.AddListener(() =>
            {
                UpdateCurrentRegionOfInterest(m_ColumnManager.SelectedColumn);
            });
            m_ColumnManager.OnUpdateMRICalValues.AddListener(() =>
            {
                if (!SceneInformation.IsGeometryUpToDate)
                    return;

                SceneInformation.IsGeneratorUpToDate = false;
                SceneInformation.IsIEEGOutdated = true;

                { //TEST
                  // recompute UV
                    for (int ii = 0; ii < m_ColumnManager.MeshSplitNumber; ++ii)
                        m_ColumnManager.DLLCommonBrainTextureGeneratorList[ii].ComputeUVMainWithVolume(m_ColumnManager.DLLSplittedMeshesList[ii], m_ColumnManager.DLLVolume, m_ColumnManager.MRICalMinFactor, m_ColumnManager.MRICalMaxFactor);

                    // update brain mesh object mesh filter (TODO update only UV)
                    for (int ii = 0; ii < m_ColumnManager.MeshSplitNumber; ++ii)
                        m_ColumnManager.DLLSplittedMeshesList[ii].UpdateMeshFromDLL(m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshFilter>().mesh);
                }

                ComputeMRITextures();
                m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
                
                m_ModesManager.UpdateMode(Mode.FunctionsId.UpdateMiddle);
            });
            m_ColumnManager.OnUpdateIEEGSpan.AddListener(() =>
            {
                if (!SceneInformation.IsGeometryUpToDate) return;

                SceneInformation.IsGeneratorUpToDate = false;
                SceneInformation.IsIEEGOutdated = true;
                UpdateGUITextures();
                m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
                
                m_ModesManager.UpdateMode(Mode.FunctionsId.UpdateMiddle);
            });
            m_ColumnManager.OnUpdateIEEGAlpha.AddListener(() =>
            {
                if (SceneInformation.IsGeometryUpToDate && !SceneInformation.IsIEEGOutdated)
                    ComputeIEEGTextures(-1, -1, true, true, false);
            });
            m_ColumnManager.OnUpdateIEEGGain.AddListener(() =>
            {
                m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
            });
            m_ColumnManager.OnUpdateIEEGMaximumInfluence.AddListener(() =>
            {
                SceneInformation.IsGeneratorUpToDate = false;
                SceneInformation.IsIEEGOutdated = true;
                m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
                
                m_ModesManager.UpdateMode(Mode.FunctionsId.UpdateMiddle);
            });
            m_ColumnManager.OnUpdateColumnTimelineID.AddListener((column) =>
            {
                ComputeIEEGTexturesOfColumn(column);
                m_ColumnManager.UpdateColumnIEEGSitesRendering(column, SceneInformation);
            });
            SceneInformation.OnUpdateGeneratorState.AddListener((value) =>
            {
                if (!value)
                {
                    foreach (Column3DIEEG column in m_ColumnManager.ColumnsIEEG)
                    {
                        column.IsLooping = false;
                    }
                }
            });
        }
        /// <summary>
        /// 
        /// When to call ?  changes in DLLCutColorScheme, MRICalMinFactor, MRICalMaxFactor
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="indexColumn"></param>
        private void ComputeMRITextures(int indexCut = -1, int indexColumn = -1)
        {
            if (SceneInformation.MeshTypeToDisplay == SceneStatesInfo.MeshType.Inflated)
                return;

            bool allColumns = indexColumn == -1;
            bool allCuts = indexCut == -1;

            int[] columnsIndexes = allColumns ? Enumerable.Range(0, m_ColumnManager.Columns.Count).ToArray() : new int[1] { indexColumn };
            int[] cutsIndexes = allCuts ? Enumerable.Range(0, m_Cuts.Count).ToArray() : new int[1] { indexCut };

            UnityEngine.Profiling.Profiler.BeginSample("TEST-Base3DScene compute_MRI_textures 0 create_MRI_texture ");

            for (int ii = 0; ii < columnsIndexes.Length; ++ii)
                for (int jj = 0; jj < cutsIndexes.Length; ++jj)
                    m_ColumnManager.CreateMRITexture(cutsIndexes[jj], columnsIndexes[ii]);

            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.BeginSample("TEST-Base3DScene compute_MRI_textures 1 compute_GUI_textures");
            ComputeGUITextures(indexCut, m_ColumnManager.SelectedColumnID);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("TEST-Base3DScene compute_MRI_textures 2 update_GUI_textures");
            UpdateGUITextures();
            UnityEngine.Profiling.Profiler.EndSample();
        }
        /// <summary>
        /// 
        /// When to call ? changes in IEEGColumn.currentTimeLineID, IEEGColumn.alphaMin, IEEGColumn.alphaMax / DLLCutColorScheme
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="indexColumn"></param>
        private void ComputeIEEGTextures(int indexCut = -1, int indexColumn = -1, bool surface = true, bool cuts = true, bool plots = true)
        {
            UnityEngine.Profiling.Profiler.BeginSample("TEST-compute_IEEG_textures");

            UnityEngine.Profiling.Profiler.BeginSample("TEST-compute_IEEG_textures 0");
            bool allColumns = indexColumn == -1;
            bool allCuts = indexCut == -1;

            int[] columnsIndexes = allColumns ? Enumerable.Range(0, m_ColumnManager.Columns.Count).ToArray() : new int[1] { indexColumn };
            int[] cutsIndexes = allCuts ? Enumerable.Range(0, m_Cuts.Count).ToArray() : new int[1] { indexCut };

            for (int ii = 0; ii < columnsIndexes.Length; ++ii)
            {
                if (ColumnManager.Columns[columnsIndexes[ii]].Type == Column3D.ColumnType.FMRI)
                    return;

                Column3DIEEG currCol = (Column3DIEEG)ColumnManager.Columns[columnsIndexes[ii]];

                // brain surface
                if (surface)
                    if (!m_ColumnManager.ComputeSurfaceBrainUVWithIEEG((SceneInformation.MeshTypeToDisplay == SceneStatesInfo.MeshType.Inflated), m_ColumnManager.ColumnsIEEG[columnsIndexes[ii]]))
                        return;

                // brain cuts
                if (cuts)
                    for (int jj = 0; jj < cutsIndexes.Length; ++jj)
                        m_ColumnManager.ColorCutsTexturesWithIEEG(m_ColumnManager.ColumnsIEEG[columnsIndexes[ii]], cutsIndexes[jj]);

                if (plots)
                {
                    currCol.UpdateSitesSizeAndColorForIEEG();
                    currCol.UpdateSitesRendering(SceneInformation, null);
                }

                ColumnManager.Columns[columnsIndexes[ii]].IsRenderingUpToDate = false;
            }
            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.BeginSample("TEST-compute_IEEG_textures 1 compute_GUI_textures");
            ComputeGUITextures(indexCut, m_ColumnManager.SelectedColumnID);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("TEST-compute_IEEG_textures 2 update_GUI_textures");
            UpdateGUITextures();
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.EndSample();
        }
        private void ComputeIEEGTexturesOfColumn(Column3DIEEG column)
        {
            UnityEngine.Profiling.Profiler.BeginSample("compute_IEEG_textures");

            UnityEngine.Profiling.Profiler.BeginSample("compute_IEEG_textures 0");

            if (!m_ColumnManager.ComputeSurfaceBrainUVWithIEEG((SceneInformation.MeshTypeToDisplay == SceneStatesInfo.MeshType.Inflated), column)) return;

            for (int jj = 0; jj < m_Cuts.Count; ++jj)
            {
                m_ColumnManager.ColorCutsTexturesWithIEEG(column, jj);
            }

            column.UpdateSitesSizeAndColorForIEEG();
            column.UpdateSitesRendering(SceneInformation, null);

            column.IsRenderingUpToDate = false;

            UnityEngine.Profiling.Profiler.EndSample();

            if (column == m_ColumnManager.SelectedColumn)
            {
                UnityEngine.Profiling.Profiler.BeginSample("compute_IEEG_textures 1 compute_GUI_textures");
                ComputeGUITextures(-1, m_ColumnManager.SelectedColumnID);
                UnityEngine.Profiling.Profiler.EndSample();

                UnityEngine.Profiling.Profiler.BeginSample("compute_IEEG_textures 2 update_GUI_textures");
                UpdateGUITextures();
                UnityEngine.Profiling.Profiler.EndSample();
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }
        /// <summary>
        /// When to call ? changes in FMRIColumn.alpha / calMin/ calMax/ DLLCutColorScheme
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="indexColumn"></param>
        /// <param name="surface"></param>
        /// <param name="cuts"></param>
        private void ComputeFMRITextures(int indexCut = -1, int indexColumn = -1, bool surface = true, bool cuts = true)
        {
            bool allColumns = indexColumn == -1;
            bool allCuts = indexCut == -1;

            int[] columnsIndexes = allColumns ? Enumerable.Range(0, m_ColumnManager.ColumnsFMRI.Count).ToArray() : new int[1] { indexColumn };
            int[] cutsIndexes = allCuts ? Enumerable.Range(0, m_Cuts.Count).ToArray() : new int[1] { indexCut };

            for (int ii = 0; ii < columnsIndexes.Length; ++ii)
            {
                //Column3DViewFMRI currCol = Column3DViewManager.FMRI_col(columnsIndexes[ii]);

                // brain cuts
                if (cuts)
                    for (int jj = 0; jj < cutsIndexes.Length; ++jj)
                        m_ColumnManager.ColorCutsTexturesWithFMRI(columnsIndexes[ii], cutsIndexes[jj]);
            }

            ComputeGUITextures(indexCut, indexColumn);

            UpdateGUITextures();
        }
        /// <summary>
        /// Compute textures to be displayed on the UI
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="indexColumn"></param>
        private void ComputeGUITextures(int indexCut = -1, int indexColumn = -1)
        {
            bool allColumns = indexColumn == -1;
            bool allCuts = indexCut == -1;

            int[] columnsIndexes = allColumns ? Enumerable.Range(0, m_ColumnManager.Columns.Count).ToArray() : new int[1] { indexColumn };
            int[] cutsIndexes = allCuts ? Enumerable.Range(0, m_Cuts.Count).ToArray() : new int[1] { indexCut };

            for (int ii = 0; ii < columnsIndexes.Length; ++ii)
            {
                Column3D currCol = m_ColumnManager.Columns[columnsIndexes[ii]];

                for (int jj = 0; jj < Cuts.Count; ++jj)
                {
                    switch (currCol.Type)
                    {
                        case Column3D.ColumnType.FMRI:
                            m_ColumnManager.CreateGUIFMRITexture(cutsIndexes[jj], columnsIndexes[ii]);
                            break;
                        case Column3D.ColumnType.IEEG:
                            if (!SceneInformation.IsGeneratorUpToDate)
                                m_ColumnManager.CreateGUIMRITexture(cutsIndexes[jj], columnsIndexes[ii]);
                            else
                                m_ColumnManager.CreateGUIIEEGTexture(cutsIndexes[jj], columnsIndexes[ii]);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Update textures displayed on the UI
        /// </summary>
        /// <param name="indexCut"></param>
        private void UpdateGUITextures()
        {
            Column3D currCol = m_ColumnManager.SelectedColumn;
            if (!currCol) return;

            List<Texture2D> texturesToDisplay = null;
            switch (currCol.Type)
            {
                case Column3D.ColumnType.FMRI:
                    texturesToDisplay = ((Column3DFMRI)currCol).GUIBrainCutWithFMRITextures;
                    break;
                case Column3D.ColumnType.IEEG:
                    if (!SceneInformation.IsGeneratorUpToDate)
                        texturesToDisplay = currCol.GUIBrainCutTextures;
                    else
                        texturesToDisplay = ((Column3DIEEG)currCol).GUIBrainCutWithIEEGTextures;
                    break;
                default:
                    break;
            }

            foreach (Cut cut in m_Cuts)
            {
                cut.OnUpdateGUITextures.Invoke(texturesToDisplay[cut.ID]);
            }
        }
        /// <summary>
        /// Finalize Generators Computing
        /// </summary>
        private void FinalizeGeneratorsComputing()
        {
            // generators are now up to date
            SceneInformation.IsGeneratorUpToDate = true;
            SceneInformation.IsIEEGOutdated = false;

            // send inf values to overlays
            for (int ii = 0; ii < m_ColumnManager.ColumnsIEEG.Count; ++ii)
            {
                float maxValue = Math.Max(Math.Abs(m_ColumnManager.ColumnsIEEG[ii].SharedMinInf), Math.Abs(m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf));
                float minValue = -maxValue;
                minValue += m_ColumnManager.ColumnsIEEG[ii].IEEGParameters.Middle;
                maxValue += m_ColumnManager.ColumnsIEEG[ii].IEEGParameters.Middle;
                Events.OnSendColorMapValues.Invoke(minValue, m_ColumnManager.ColumnsIEEG[ii].IEEGParameters.Middle, maxValue, m_ColumnManager.ColumnsIEEG[ii]);
                m_ColumnManager.ColumnsIEEG[ii].OnUpdateCurrentTimelineID.Invoke();
            }

            // amplitudes are not displayed yet
            for (int ii = 0; ii < m_ColumnManager.ColumnsIEEG.Count; ++ii)
                m_ColumnManager.ColumnsIEEG[ii].UpdateIEEG = true;

            //####### CHECK ACCESS
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.PostUpdateGenerators))
            {
                Debug.LogError("-ERROR : Base3DScene::post_updateGenerators -> no acess for mode : " + m_ModesManager.CurrentModeName);
            }
            //##################

            // update plots visibility
            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);

            DisplayScreenMessage("Computing finished !", 1f, 250, 40);
            DisplayProgressbar(1f, 1f, 250, 40);

            //####### UDPATE MODE
            m_ModesManager.UpdateMode(Mode.FunctionsId.PostUpdateGenerators);
            //##################
        }
        /// <summary>
        /// Load FMRI dialog
        /// </summary>
        /// <param name="path">Path of the FMRI file</param>
        /// <returns></returns>
        private bool LoadFMRIDialog(out string path)
        {
            bool loaded = true;
            string[] filters = new string[] { "nii", "img" };
            path = "";
            path = DLL.QtGUI.GetExistingFileName(filters, "Select an fMRI file");

            if (!string.IsNullOrEmpty(path))
            {
                bool result = LoadFMRIFile(path);
                if (!result)
                {
                    Debug.LogError("-ERROR : ScenesManager::addIRMF -> can't load FMRI");
                    loaded = false;
                }
            }
            else
            {
                loaded = false;
            }
            return loaded;
        }
        /// <summary>
        /// Load an FMRI column
        /// </summary>
        /// <param name="fMRIPath"></param>
        /// <returns></returns>
        private bool LoadFMRIFile(string fMRIPath)
        {
            if (m_ColumnManager.DLLNii.LoadNIIFile(fMRIPath))
                return true;

            Debug.LogError("-ERROR : Base3DScene::load_FMRI_file -> load NII file failed. " + fMRIPath);
            return false;
        }
        /// <summary>
        /// Load a FMRI column
        /// </summary>
        /// <param name="fmriLabel"></param>
        /// <returns></returns>
        private bool LoadFMRIColumn(string fmriLabel)
        {
            // Check access
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.AddFMRIColumn))
            {
                Debug.LogError("-ERROR : Base3DScene::add_FMRI_column -> no acess for mode : " + m_ModesManager.CurrentModeName);
                return false;
            }

            // Update column number
            int newFMRIColumnNumber = m_ColumnManager.ColumnsFMRI.Count + 1;
            m_ColumnManager.UpdateColumnsNumber(m_ColumnManager.ColumnsIEEG.Count, newFMRIColumnNumber, m_Cuts.Count);

            // Update label
            int newFMRIColumnID = newFMRIColumnNumber - 1;
            m_ColumnManager.ColumnsFMRI[newFMRIColumnID].Label = fmriLabel;

            // Update sites visibility
            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);

            // Convert to volume            
            m_ColumnManager.DLLNii.ConvertToVolume(m_ColumnManager.DLLVolumeFMriList[newFMRIColumnID]);

            if (Type == SceneType.SinglePatient)
                Events.OnAskRegionOfInterestUpdate.Invoke(m_ColumnManager.ColumnsIEEG.Count + newFMRIColumnID);

            // send parameters to UI
            //IRMCalValues calValues = m_CM.DLLVolumeIRMFList[idCol].retrieveExtremeValues();

            FMRIDataParameters fmriParams = new FMRIDataParameters();
            fmriParams.calValues = m_ColumnManager.DLLVolumeFMriList[newFMRIColumnID].ExtremeValues;
            fmriParams.columnId = newFMRIColumnID;
            fmriParams.alpha = m_ColumnManager.ColumnsFMRI[newFMRIColumnID].Alpha;
            fmriParams.calMin = m_ColumnManager.ColumnsFMRI[newFMRIColumnID].CalMin;
            fmriParams.calMax = m_ColumnManager.ColumnsFMRI[newFMRIColumnID].CalMax;
            fmriParams.singlePatient = Type == SceneType.SinglePatient;

            m_ColumnManager.ColumnsFMRI[newFMRIColumnID].CalMin = fmriParams.calValues.computedCalMin;
            m_ColumnManager.ColumnsFMRI[newFMRIColumnID].CalMax = fmriParams.calValues.computedCalMax;

            // Update camera
            Events.OnUpdateCameraTarget.Invoke(m_ColumnManager.BothHemi.BoundingBox.Center);

            ComputeMRITextures(-1, -1);

            Events.OnSendFMRIParameters.Invoke(fmriParams);
            ComputeFMRITextures(-1, -1);

            m_ModesManager.UpdateMode(Mode.FunctionsId.AddFMRIColumn);
            return true;
        }
        /// <summary>
        /// Unload the last FMRI column
        /// </summary>
        private void UnloadLastFMRIColumn()
        {
            // Check access
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.RemoveLastFMRIColumn))
            {
                Debug.LogError("-ERROR : Base3DScene::remove_last_FMRI_column -> no acess for mode : " + m_ModesManager.CurrentModeName);
                return;
            }

            // Update columns number
            m_ColumnManager.UpdateColumnsNumber(m_ColumnManager.ColumnsIEEG.Count, m_ColumnManager.ColumnsFMRI.Count - 1, m_Cuts.Count);

            // Update plots visibility
            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);

            ComputeMRITextures(-1, -1);
            ComputeFMRITextures(-1, -1);
            UpdateGUITextures();

            m_ModesManager.UpdateMode(Mode.FunctionsId.RemoveLastFMRIColumn);
        }
        /// <summary>
        /// Actions to perform when clicking on a site
        /// </summary>
        private void ClickOnSiteCallback()
        {
            if (m_ColumnManager.SelectedColumn.SelectedSiteID == -1) return;
            if (SceneInformation.IsComparingSites)
            {
                SceneInformation.IsComparingSites = false;
                //SendAdditionalSiteInfoRequest(LAST_SELECTED_SITE); //FIXME : retrieve last selected site
            }
            else
            {
                if (((Column3DIEEG)m_ColumnManager.SelectedColumn).SendInformation)
                {
                    SendAdditionalSiteInfoRequest();
                }
            }
        }
        /// <summary>
        /// Init gameobjects of the scene
        /// </summary>
        protected void InitializeSceneGameObjects()
        {
            // init parents 
            m_DisplayedObjects.SitesMeshesParent = transform.Find("Sites").gameObject;
            m_DisplayedObjects.BrainSurfaceMeshesParent = transform.Find("Meshes").Find("Brains").gameObject;
            m_DisplayedObjects.BrainCutMeshesParent = transform.Find("Meshes").Find("Cuts").gameObject;

            // init lights
            m_DisplayedObjects.SharedDirectionalLight = transform.parent.Find("Global Light").gameObject;

            // init default planes
            m_Cuts = new List<Cut>();
            m_DisplayedObjects.BrainCutMeshes = new List<GameObject>();

            UpdateBrainSurfaceColor(ColorType.BrainColor);
            UpdateColormap(ColorType.MatLab, false);
            UpdateBrainCutColor(ColorType.Default, true);
        }
        #endregion

        #region Public Methods

        #region Display
        /// <summary>
        /// Update the IEEG colormap for this scene
        /// </summary>
        /// <param name="color">Color of the colormap</param>
        /// <param name="updateColors">Do the colors need to be reset ?</param>
        public void UpdateColormap(ColorType color, bool updateColors = true)
        {
            ColumnManager.Colormap = color;
            if (updateColors)
                ColumnManager.ResetColors();

            SharedMaterials.Brain.BrainMaterials[this].SetTexture("_ColorTex", ColumnManager.BrainColorMapTexture);

            if (SceneInformation.IsGeometryUpToDate && !SceneInformation.IsIEEGOutdated)
                ComputeIEEGTextures();

            Events.OnChangeColormap.Invoke(color);
        }
        /// <summary>
        /// Update the color of the surface of the brain for this scene
        /// </summary>
        /// <param name="color">Color of the brain</param>
        public void UpdateBrainSurfaceColor(ColorType color)
        {
            ColumnManager.BrainColor = color;
            DLL.Texture tex = DLL.Texture.Generate1DColorTexture(ColumnManager.BrainColor);
            tex.UpdateTexture2D(ColumnManager.BrainColorTexture);

            SharedMaterials.Brain.BrainMaterials[this].SetTexture("_MainTex", ColumnManager.BrainColorTexture);
        }
        /// <summary>
        /// Update the color of the cuts for this scene
        /// </summary>
        /// <param name="color">Color of the cuts</param>
        /// <param name="updateColors">Do the colors need to be reset ?</param>
        public void UpdateBrainCutColor(ColorType color, bool updateColors = true)
        {
            ColumnManager.BrainCutColor = color;
            if (updateColors)
                ColumnManager.ResetColors();

            SceneInformation.CutMeshGeometryNeedsUpdate = true;
            SceneInformation.IsIEEGOutdated = true;
            foreach (Column3D column in m_ColumnManager.Columns)
            {
                column.IsRenderingUpToDate = false;
            }

            //####### UDPATE MODE
            m_ModesManager.UpdateMode(Mode.FunctionsId.UpdatePlane); // TEMP
            //##################
        }
        /// <summary>
        /// Set the mesh part to be displayed in the scene
        /// </summary>
        /// <param name="meshPartToDisplay"></param>
        public void UpdateMeshPartToDisplay(SceneStatesInfo.MeshPart meshPartToDisplay)
        {
            // Check access
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.SetDisplayedMesh))
            {
                Debug.LogError("-ERROR : Base3DScene::setDisplayedMesh -> no acess for mode : " + m_ModesManager.CurrentModeName);
                return;
            }

            if (!SceneInformation.IsGeometryUpToDate) return;

            SceneInformation.MeshPartToDisplay = meshPartToDisplay;
            SceneInformation.CutMeshGeometryNeedsUpdate = true;
            SceneInformation.IsIEEGOutdated = true;
            //m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
            foreach (Column3D column in m_ColumnManager.Columns)
            {
                column.IsRenderingUpToDate = false;
            }

            // Update Mode
            m_ModesManager.UpdateMode(Mode.FunctionsId.SetDisplayedMesh);
        }
        /// <summary>
        /// Set the mesh type to be displayed in the scene
        /// </summary>
        /// <param name="meshTypeToDisplay"></param>
        public void UpdateMeshTypeToDisplay(SceneStatesInfo.MeshType meshTypeToDisplay)
        {
            // Check access
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.SetDisplayedMesh))
            {
                Debug.LogError("-ERROR : Base3DScene::setDisplayedMesh -> no acess for mode : " + m_ModesManager.CurrentModeName);
                return;
            }

            if (!SceneInformation.IsGeometryUpToDate) return;

            switch (meshTypeToDisplay)
            {
                case SceneStatesInfo.MeshType.Grey:
                    if (!SceneInformation.GreyMeshesAvailables) return;
                    break;
                case SceneStatesInfo.MeshType.White:
                    if (!SceneInformation.WhiteMeshesAvailables) return;
                    break;
                case SceneStatesInfo.MeshType.Inflated:
                    if (!SceneInformation.WhiteInflatedMeshesAvailables) return;
                    break;
            }

            SceneInformation.MeshTypeToDisplay = meshTypeToDisplay;
            SceneInformation.CutMeshGeometryNeedsUpdate = true;
            SceneInformation.IsIEEGOutdated = true;
            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
            foreach (Column3D column in m_ColumnManager.Columns)
            {
                column.IsRenderingUpToDate = false;
            }

            // Update mode
            m_ModesManager.UpdateMode(Mode.FunctionsId.SetDisplayedMesh);
        }
        #endregion

        #region Cuts
        /// <summary>
        /// Add a new cut plane
        /// </summary>
        public Cut AddCutPlane()
        {
            // Check access
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.AddNewPlane))
            {
                Debug.LogError("-ERROR : Base3DScene::addNewPlane -> no acess for mode : " + m_ModesManager.CurrentModeName);
                return null;
            }

            // Add new cut
            Cut cut = new Cut(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
            switch (m_Cuts.Count)
            {
                case 0:
                    cut.Orientation = CutOrientation.Axial;
                    cut.Flip = false;
                    cut.RemoveFrontPlane = 0;
                    cut.Position = 0.5f;
                    break;
                case 1:
                    cut.Orientation = CutOrientation.Coronal;
                    cut.Flip = false;
                    cut.RemoveFrontPlane = 0;
                    cut.Position = 0.5f;
                    break;
                case 2:
                    cut.Orientation = CutOrientation.Sagital;
                    cut.Flip = false;
                    cut.RemoveFrontPlane = 0;
                    cut.Position = 0.5f;
                    break;
                default:
                    cut.Orientation = CutOrientation.Axial;
                    cut.Flip = false;
                    cut.RemoveFrontPlane = 0;
                    cut.Position = 0.5f;
                    break;
            }
            m_Cuts.Add(cut);

            // Update IDs
            for (int i = 0; i < m_Cuts.Count; i++)
            {
                m_Cuts[i].ID = i;
            }

            // Add new cut GameObject
            GameObject cutGameObject = Instantiate(m_CutPrefab);
            cutGameObject.GetComponent<Renderer>().sharedMaterial = SharedMaterials.Brain.CutMaterials[this];
            cutGameObject.name = "cut_" + (m_Cuts.Count - 1);
            cutGameObject.transform.parent = m_DisplayedObjects.BrainCutMeshesParent.transform;
            cutGameObject.AddComponent<MeshCollider>();
            cutGameObject.layer = LayerMask.NameToLayer(SceneInformation.MeshesLayerName);
            cutGameObject.transform.localPosition = Vector3.zero;
            m_DisplayedObjects.BrainCutMeshes.Add(cutGameObject);
            m_DisplayedObjects.BrainCutMeshes.Last().layer = LayerMask.NameToLayer(SceneInformation.MeshesLayerName);

            // update columns manager
            m_ColumnManager.UpdateCutNumber(m_DisplayedObjects.BrainCutMeshes.Count);

            // update plots visibility
            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);

            SceneInformation.CutMeshGeometryNeedsUpdate = true;
            SceneInformation.IsIEEGOutdated = true;

            // Update mode
            m_ModesManager.UpdateMode(Mode.FunctionsId.AddNewPlane);

            UpdateCutPlane(cut);

            return cut;
        }
        /// <summary>
        /// Remove the last cut plane
        /// </summary>
        public void RemoveCutPlane(Cut cut)
        {
            // Check access
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.RemoveLastPlane))
            {
                Debug.LogError("-ERROR : Base3DScene::removeLastPlane -> no acess for mode : " + m_ModesManager.CurrentModeName);
                return;
            }

            m_Cuts.Remove(cut);
            for (int i = 0; i < m_Cuts.Count; i++)
            {
                m_Cuts[i].ID = i;
            }

            Destroy(m_DisplayedObjects.BrainCutMeshes[cut.ID]);
            m_DisplayedObjects.BrainCutMeshes.RemoveAt(cut.ID);

            // update columns manager
            m_ColumnManager.UpdateCutNumber(m_DisplayedObjects.BrainCutMeshes.Count);

            // update plots visibility
            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);

            SceneInformation.CutMeshGeometryNeedsUpdate = true;
            SceneInformation.IsIEEGOutdated = true;

            // Update mode
            m_ModesManager.UpdateMode(Mode.FunctionsId.RemoveLastPlane);
        }
        /// <summary>
        /// Update a cut plane
        /// </summary>
        /// <param name="orientationID"></param>
        /// <param name="flip"></param>
        /// <param name="removeFrontPlane"></param>
        /// <param name="customNormal"></param>
        /// <param name="idPlane"></param>
        /// <param name="position"></param>
        public void UpdateCutPlane(Cut cut)
        {
            // Check access
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.UpdatePlane))
            {
                Debug.LogError("-ERROR : Base3DScene::updatePlane -> no acess for mode : " + m_ModesManager.CurrentModeName);
                return;
            }

            if (cut.Orientation == CutOrientation.Custom || !SceneInformation.MRILoaded)
            {
                if (cut.Normal.x == 0 && cut.Normal.y == 0 && cut.Normal.z == 0)
                {
                    cut.Normal = new Vector3(1, 0, 0);
                }
            }
            else
            {
                Plane plane = new Plane(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
                m_ColumnManager.DLLVolume.SetPlaneWithOrientation(plane, cut.Orientation, cut.Flip);
                cut.Normal = plane.Normal;
            }

            SceneInformation.LastPlaneModifiedID = cut.ID;

            // Cuts base on the mesh
            float offset;
            if (SceneInformation.MeshToDisplay != null)
            {
                offset = SceneInformation.MeshToDisplay.SizeOffsetCutPlane(cut, cut.NumberOfCuts);
                offset *= 1.05f; // upsize a little bit the bbox for planes
            }
            else
                offset = 0.1f;

            cut.Point = SceneInformation.MeshCenter + cut.Normal * (cut.Position - 0.5f) * offset * cut.NumberOfCuts;

            SceneInformation.CutMeshGeometryNeedsUpdate = true;
            SceneInformation.IsIEEGOutdated = true;

            // update sites visibility
            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);

            // update cameras cuts display
            Events.OnModifyPlanesCuts.Invoke();

            // Update mode
            m_ModesManager.UpdateMode(Mode.FunctionsId.UpdatePlane);
        }
        #endregion

        /// <summary>
        /// Reset the gameobjects of the scene
        /// </summary>
        public void ResetSceneGameObjects()
        {
            // destroy meshes
            for (int ii = 0; ii < m_DisplayedObjects.BrainSurfaceMeshes.Count; ++ii)
            {
                Destroy(m_DisplayedObjects.BrainSurfaceMeshes[ii]);
            }
            for (int ii = 0; ii < m_DisplayedObjects.BrainCutMeshes.Count; ++ii)
            {
                m_DisplayedObjects.BrainCutMeshes[ii].SetActive(false);
            }
        }
        /// <summary>
        /// Reset the number of splits of the brain mesh
        /// </summary>
        /// <param name="nbSplits">Number of splits</param>
        public void ResetSplitsNumber(int nbSplits)
        {
            if (m_ColumnManager.MeshSplitNumber == nbSplits) return;

            m_ColumnManager.MeshSplitNumber = nbSplits;

            if(m_DisplayedObjects.BrainSurfaceMeshes.Count > 0)
                for (int ii = 0; ii < m_DisplayedObjects.BrainSurfaceMeshes.Count; ++ii)
                    Destroy(m_DisplayedObjects.BrainSurfaceMeshes[ii]);

            // reset meshes
            m_DisplayedObjects.BrainSurfaceMeshes = new List<GameObject>(m_ColumnManager.MeshSplitNumber);
            for (int ii = 0; ii < m_ColumnManager.MeshSplitNumber; ++ii)
            {
                m_DisplayedObjects.BrainSurfaceMeshes.Add(Instantiate(GlobalGOPreloaded.Brain));
                m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<Renderer>().sharedMaterial = SharedMaterials.Brain.BrainMaterials[this];
                m_DisplayedObjects.BrainSurfaceMeshes[ii].name = "brain_" + ii;
                m_DisplayedObjects.BrainSurfaceMeshes[ii].transform.parent = m_DisplayedObjects.BrainSurfaceMeshesParent.transform;
                m_DisplayedObjects.BrainSurfaceMeshes[ii].transform.localPosition = Vector3.zero;
                m_DisplayedObjects.BrainSurfaceMeshes[ii].layer = LayerMask.NameToLayer(SceneInformation.MeshesLayerName);
                m_DisplayedObjects.BrainSurfaceMeshes[ii].AddComponent<MeshCollider>();
                m_DisplayedObjects.BrainSurfaceMeshes[ii].SetActive(true);
            }

            m_ColumnManager.ResetSplitsNumber(nbSplits);
        }
        /// <summary>
        /// Set UI screen space/overlays layers mask settings corresponding to the current mode of the scene
        /// </summary>
        public void SetCurrentModeSpecifications(bool force = false)
        {
            m_ModesManager.SetCurrentModeSpecifications(force);
        }
        /// <summary>
        /// Update the sites masks
        /// </summary>
        /// <param name="allColumns">Do we apply the action on all the columns ?</param>
        /// <param name="siteGameObject">GameObject of the site on which we apply the action</param>
        /// <param name="action"> 0 : excluded / 1 : included / 2 : blacklisted / 3 : unblacklist / 4 : highlight / 5 : unhighlight / 6 : marked / 7 : unmarked </param>
        /// <param name="range"> 0 : a plot / 1 : all plots from an electrode / 2 : all plots from a patient / 3 : all highlighted / 4 : all unhighlighted 
        /// / 5 : all plots / 6 : in ROI / 7 : not in ROI / 8 : names filter / 9 : mars filter / 10 : broadman filter </param>
        public void UpdateSitesMasks(bool allColumns, SiteAction action = SiteAction.Exclude, SiteFilter filter = SiteFilter.Site, string nameFilter = "")
        {
            GameObject siteGameObject = null;
            if (m_ColumnManager.SelectedColumn.SelectedSite)
            {
                siteGameObject = m_ColumnManager.SelectedColumn.SelectedSite.gameObject;
            }
            // Check access
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.UpdateMaskPlot))
            {
                Debug.LogError("-ERROR : Base3DScene::updateMaskPlot -> no acess for mode : " + m_ModesManager.CurrentModeName);
                return;
            }

            List<Column3D> columns = new List<Column3D>(); // List of columns we inspect
            if (allColumns)
            {
                columns = m_ColumnManager.Columns.ToList();
            }
            else
            {
                columns.Add(m_ColumnManager.SelectedColumn);
            }

            List<int> sitesID = new List<int>();
            // Build the list of the sites on which we apply actions
            foreach (Column3D column in columns)
            {
                switch(filter)
                {
                    case SiteFilter.Site:
                        {
                            sitesID.Add(siteGameObject.GetComponent<Site>().Information.GlobalID);
                        }
                        break;
                    case SiteFilter.Electrode:
                        {
                            Transform parentElectrode = siteGameObject.transform.parent;
                            for (int jj = 0; jj < parentElectrode.childCount; ++jj)
                            {
                                sitesID.Add(parentElectrode.GetChild(jj).gameObject.GetComponent<Site>().Information.GlobalID);
                            }
                        }
                        break;
                    case SiteFilter.Patient:
                        {
                            Transform parentPatient = siteGameObject.transform.parent.parent;
                            for (int jj = 0; jj < parentPatient.childCount; ++jj)
                            {
                                Transform parentElectrode = parentPatient.GetChild(jj);
                                for (int kk = 0; kk < parentElectrode.childCount; kk++)
                                {
                                    sitesID.Add(parentElectrode.GetChild(kk).gameObject.GetComponent<Site>().Information.GlobalID);
                                }
                            }
                        }
                        break;
                    case SiteFilter.Highlighted:
                        {
                            for (int jj = 0; jj < column.Sites.Count; ++jj)
                            {                                
                                if (column.Sites[jj].Information.IsHighlighted)
                                    sitesID.Add(jj);
                            }
                        }
                        break;
                    case SiteFilter.Unhighlighted:
                        {                            
                            for (int jj = 0; jj < column.Sites.Count; ++jj)
                            {
                                if (!column.Sites[jj].Information.IsHighlighted)
                                    sitesID.Add(jj);
                            }
                        }
                        break;
                    case SiteFilter.All:
                        {
                            for (int jj = 0; jj < column.Sites.Count; ++jj)
                            {
                                sitesID.Add(jj);
                            }
                        }
                        break;
                    case SiteFilter.InRegionOfInterest:
                        {
                            for (int jj = 0; jj < column.Sites.Count; ++jj)
                            {
                                if (!column.Sites[jj].Information.IsOutOfROI)
                                    sitesID.Add(jj);
                            }
                        }
                        break;
                    case SiteFilter.OutOfRegionOfInterest:
                        {
                            for (int jj = 0; jj < column.Sites.Count; ++jj)
                            {
                                if (column.Sites[jj].Information.IsOutOfROI)
                                    sitesID.Add(jj);
                            }
                        }
                        break;
                    case SiteFilter.Name:
                        {
                            for (int jj = 0; jj < column.Sites.Count; ++jj)
                            {
                                if (column.Sites[jj].Information.FullName.ToLower().Contains(nameFilter.ToLower()))
                                    sitesID.Add(jj);
                            }
                        }
                        break;
                    case SiteFilter.MarsAtlas:
                        {                            
                            for (int jj = 0; jj < column.Sites.Count; ++jj)
                            {
                                if (ApplicationState.Module3D.MarsAtlasIndex.FullName(column.Sites[jj].Information.MarsAtlasIndex).ToLower().Contains(nameFilter.ToLower()))
                                    sitesID.Add(jj);      
                            }
                        }
                        break;
                    case SiteFilter.Broadman:
                        {
                            for (int jj = 0; jj < column.Sites.Count; ++jj)
                            {
                                if (ApplicationState.Module3D.MarsAtlasIndex.BroadmanArea(column.Sites[jj].Information.MarsAtlasIndex).ToLower().Contains(nameFilter.ToLower()))
                                    sitesID.Add(jj);
                            }
                        }
                        break;
                }
            }

            // Apply action
            foreach (Column3D column in columns)
            {
                for (int ii = 0; ii < sitesID.Count; ++ii)
                {
                    switch (action)
                    {
                        case SiteAction.Include:
                            column.Sites[sitesID[ii]].Information.IsExcluded = false;
                            break;
                        case SiteAction.Exclude:
                            column.Sites[sitesID[ii]].Information.IsExcluded = true;
                            break;
                        case SiteAction.Blacklist:
                            column.Sites[sitesID[ii]].Information.IsBlackListed = true;
                            break;
                        case SiteAction.Unblacklist:
                            column.Sites[sitesID[ii]].Information.IsBlackListed = false;
                            break;
                        case SiteAction.Highlight:
                            column.Sites[sitesID[ii]].Information.IsHighlighted = true;
                            break;
                        case SiteAction.Unhighlight:
                            column.Sites[sitesID[ii]].Information.IsHighlighted = false;
                            break;
                        case SiteAction.Mark:
                            column.Sites[sitesID[ii]].Information.IsMarked = true;
                            break;
                        case SiteAction.Unmark:
                            column.Sites[sitesID[ii]].Information.IsMarked = false;
                            break;
                        default:
                            break;
                    }
                }
            }

            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);

            // Update Mode
            m_ModesManager.UpdateMode(Mode.FunctionsId.UpdateMaskPlot);
        }
        public void SelectDefaultView()
        {
            m_ColumnManager.Columns[0].Views[0].IsSelected = true;
            m_ModesManager.SetCurrentModeSpecifications(true);
            ComputeGUITextures(-1, m_ColumnManager.SelectedColumnID);
            UpdateGUITextures();
        }
        /// <summary>
        /// Update the data render corresponding to the column
        /// </summary>
        /// <param name="indexColumn"></param>
        /// <returns></returns>
        public bool UpdateColumnRendering(Column3D column)
        {
            if (!SceneInformation.IsGeometryUpToDate || column.IsRenderingUpToDate)
                return false;

            UnityEngine.Profiling.Profiler.BeginSample("TEST-updateColumnRender");

            // TODO : un mesh pour chaque column

            // update cuts textures
            if ((SceneInformation.MeshTypeToDisplay != SceneStatesInfo.MeshType.Inflated))
            {
                for (int ii = 0; ii < Cuts.Count; ++ii)
                {
                    switch (column.Type)
                    {
                        case Column3D.ColumnType.FMRI:
                            m_DisplayedObjects.BrainCutMeshes[ii].GetComponent<Renderer>().material.mainTexture = ((Column3DFMRI)column).BrainCutWithFMRITextures[ii];
                            break;
                        case Column3D.ColumnType.IEEG:
                            if (!SceneInformation.IsGeneratorUpToDate)
                            {
                                UnityEngine.Profiling.Profiler.BeginSample("TEST-updateColumnRender-BrainCutTextures");
                                m_DisplayedObjects.BrainCutMeshes[ii].GetComponent<Renderer>().material.mainTexture = column.BrainCutTextures[ii];
                                UnityEngine.Profiling.Profiler.EndSample();
                            }
                            else
                            {
                                UnityEngine.Profiling.Profiler.BeginSample("TEST-updateColumnRender-BrainCutTexturesWithEEG");
                                m_DisplayedObjects.BrainCutMeshes[ii].GetComponent<Renderer>().material.mainTexture = ((Column3DIEEG)column).BrainCutWithIEEGTextures[ii];
                                UnityEngine.Profiling.Profiler.EndSample();
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            // update meshes splits UV
            for (int ii = 0; ii < m_ColumnManager.MeshSplitNumber; ++ii)
            {
                // uv 1 (main)
                //go_.brainSurfaceMeshes[ii].GetComponent<MeshFilter>().mesh.uv = m_CM.UVCoordinatesSplits[ii];

                if (column.Type == Column3D.ColumnType.FMRI || !SceneInformation.IsGeneratorUpToDate || SceneInformation.DisplayCCEPMode)
                {
                    // uv 2 (alpha) 
                    UnityEngine.Profiling.Profiler.BeginSample("TEST-updateColumnRender-UV2-Alpha");
                    m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshFilter>().mesh.uv2 = m_ColumnManager.UVNull[ii];
                    UnityEngine.Profiling.Profiler.EndSample();
                    // uv 3 (color map)
                    UnityEngine.Profiling.Profiler.BeginSample("TEST-updateColumnRender-UV3-EEG");
                    m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshFilter>().mesh.uv3 = m_ColumnManager.UVNull[ii];
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                else
                {
                    // uv 2 (alpha)
                    UnityEngine.Profiling.Profiler.BeginSample("TEST-updateColumnRender-UV2-Alpha");
                    m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshFilter>().mesh.uv2 = ((Column3DIEEG)column).DLLBrainTextureGenerators[ii].AlphaUV;
                    UnityEngine.Profiling.Profiler.EndSample();
                    // uv 3 (color map)
                    UnityEngine.Profiling.Profiler.BeginSample("TEST-updateColumnRender-UV3-EEG");
                    m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshFilter>().mesh.uv3 = ((Column3DIEEG)column).DLLBrainTextureGenerators[ii].IEEGUV;
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
            
            UnityEngine.Profiling.Profiler.EndSample();
            //column.IsRenderingUpToDate = true; // FIXME : uncomment this when updating to multiple meshes
            return true;
        }
        /// <summary>
        /// Update the brain and the cuts meshes colliders
        /// </summary>
        public void UpdateMeshesColliders()
        {
            if (!SceneInformation.MeshesLoaded || !SceneInformation.MRILoaded || SceneInformation.CollidersUpdated)
                return;

            // update splits colliders
            for(int ii = 0; ii < m_DisplayedObjects.BrainSurfaceMeshes.Count; ++ii)
            {
                m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshCollider>().sharedMesh = null;
                m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshCollider>().sharedMesh = m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshFilter>().mesh;
            }

            // update cuts colliders
            for (int ii = 0; ii < m_DisplayedObjects.BrainCutMeshes.Count; ++ii)
            {
                m_DisplayedObjects.BrainCutMeshes[ii].GetComponent<MeshCollider>().sharedMesh = null;
                m_DisplayedObjects.BrainCutMeshes[ii].GetComponent<MeshCollider>().sharedMesh = m_DisplayedObjects.BrainCutMeshes[ii].GetComponent<MeshFilter>().mesh;
            }

            SceneInformation.CollidersUpdated = true;
        }
        /// <summary>
        /// Update the textures generator
        /// </summary>
        public void UpdateGenerators()
        {
            // Check access
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.PreUpdateGenerators))
            {
                Debug.LogError("-ERROR : Base3DScene::pre_updateGenerators -> no acess for mode : " + m_ModesManager.CurrentModeName);
                return;
            }

            if (SceneInformation.CutMeshGeometryNeedsUpdate || !SceneInformation.IsGeometryUpToDate) // if update cut plane is pending, cancel action
                return;

            // Update mode
            m_ModesManager.UpdateMode(Mode.FunctionsId.PreUpdateGenerators);

            SceneInformation.IsGeneratorUpToDate = false;
            this.StartCoroutineAsync(c_ComputeGenerators());
        }
        /// <summary>
        /// Update the displayed amplitudes on the brain and the cuts with the slider position.
        /// </summary>
        /// <param name="columnID"></param>
        /// <param name="slider"></param>
        /// <param name="globalTimeline"> if globaltime is true, update all columns with the same slider, else udapte only current selected column </param>
        public void UpdateIEEGTimeline(int value, bool global)
        {
            if (global)
            {
                foreach (Column3DIEEG column in m_ColumnManager.ColumnsIEEG)
                {
                    column.CurrentTimeLineID = value;
                }
            }
            else
            {
                ((Column3DIEEG)m_ColumnManager.SelectedColumn).CurrentTimeLineID = value;
            }

            ComputeIEEGTextures();
            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
        }
        /// <summary>
        /// Reset invisible brain and triangle eraser
        /// </summary>
        /// <param name="updateGO"></param>
        public void ResetTriangleErasing(bool updateGO = true)
        {
            // destroy previous GO
            if (m_DisplayedObjects.InvisibleBrainSurfaceMeshes != null)
                for (int ii = 0; ii < m_DisplayedObjects.InvisibleBrainSurfaceMeshes.Count; ++ii)
                    Destroy(m_DisplayedObjects.InvisibleBrainSurfaceMeshes[ii]);

            // create new GO
            m_DisplayedObjects.InvisibleBrainSurfaceMeshes = new List<GameObject>(m_DisplayedObjects.BrainSurfaceMeshes.Count);
            for (int ii = 0; ii < m_DisplayedObjects.BrainSurfaceMeshes.Count; ++ii)
            {
                GameObject invisibleBrainPart = Instantiate(GlobalGOPreloaded.InvisibleBrain);
                invisibleBrainPart.name = "erased brain part " + ii;
                invisibleBrainPart.transform.SetParent(transform.Find("Meshes").Find("Erased Brains"));
                switch(Type)
                {
                    case SceneType.SinglePatient:
                        invisibleBrainPart.layer = LayerMask.NameToLayer("Default");
                        break;
                    case SceneType.MultiPatients:
                        invisibleBrainPart.layer = LayerMask.NameToLayer("Default");
                        break;
                }
                invisibleBrainPart.AddComponent<MeshFilter>();
                invisibleBrainPart.transform.localScale = new Vector3(-1, 1, 1);
                invisibleBrainPart.transform.localPosition = new Vector3(0, 0, 0);
                invisibleBrainPart.SetActive(m_TriEraser.IsEnabled);
                m_DisplayedObjects.InvisibleBrainSurfaceMeshes.Add(invisibleBrainPart);
            }

            m_TriEraser.Reset(m_DisplayedObjects.InvisibleBrainSurfaceMeshes, m_ColumnManager.DLLCutsList[0], m_ColumnManager.DLLSplittedMeshesList);

            if(updateGO)
                for (int ii = 0; ii < m_ColumnManager.DLLSplittedMeshesList.Count; ++ii)
                    m_ColumnManager.DLLSplittedMeshesList[ii].UpdateMeshFromDLL(m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshFilter>().mesh);
        }
        /// <summary>
        /// CTRL+Z on triangle eraser
        /// </summary>
        public void CancelLastTriangleErasingAction()
        {
            m_TriEraser.CancelLastAction();
            for (int ii = 0; ii < m_ColumnManager.DLLSplittedMeshesList.Count; ++ii)
                m_ColumnManager.DLLSplittedMeshesList[ii].UpdateMeshFromDLL(m_DisplayedObjects.BrainSurfaceMeshes[ii].GetComponent<MeshFilter>().mesh);
        }
        /// <summary>
        /// Update the cal min value of the input FMRI column
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columnId"> id of the FMRI columns</param>
        public void UpdateFMRICalMin(float value, int columnId)
        {
            m_ColumnManager.ColumnsFMRI[columnId].CalMin = value;
            ComputeFMRITextures(-1, -1);
        }
        /// <summary>
        /// Update the cal max value of the input FMRI column
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columnId"> id of the FMRI columns</param>
        public void UpdateFMRICalMax(float value, int columnId)
        {
            m_ColumnManager.ColumnsFMRI[columnId].CalMax = value;
            ComputeFMRITextures(-1, -1);
        }
        /// <summary>
        /// Update the alpha value of the input FMRI column
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columnId"> id of the FMRI columns</param>
        public void UpdateFMRIAlpha(float value, int columnId)
        {
            m_ColumnManager.ColumnsFMRI[columnId].Alpha = value;
            ComputeFMRITextures(-1, -1);
        }
        /// <summary>
        /// Add a FMRI column to this scene
        /// </summary>
        /// <returns></returns>
        public bool AddFMRIColumn()
        {
            string fMRIPath;
            if (!LoadFMRIDialog(out fMRIPath)) return false;

            string[] split = fMRIPath.Split(new Char[] { '/', '\\' });
            string fMRILabel = split[split.Length - 1];

            bool result = LoadFMRIColumn(fMRILabel);
            if (!result)
            {
                Debug.LogError("-ERROR : ScenesManager::addIRMF -> can't add IRMF column");
            }
            else
            {
                Events.OnAddFMRIColumn.Invoke();
            }
            return result;
        }
        /// <summary>
        /// Remove the last FMRI column from this scene
        /// </summary>
        public void RemoveLastFMRIColumn()
        {
            if (m_ColumnManager.ColumnsFMRI.Count > 0)
            {
                UnloadLastFMRIColumn();
                Events.OnRemoveFMRIColumn.Invoke();
            }
        }
        /// <summary>
        /// Send FMRI parameters to menu via event
        /// </summary>
        public void SendFMRIParametersToMenu() // TODO / FIXME
        {
            for (int ii = 0; ii < m_ColumnManager.ColumnsIEEG.Count; ++ii)
            {
                FMRIDataParameters FMRIDataParams;
                FMRIDataParams.alpha = m_ColumnManager.ColumnsFMRI[ii].Alpha;
                FMRIDataParams.calMin = m_ColumnManager.ColumnsFMRI[ii].CalMin;
                FMRIDataParams.calMax = m_ColumnManager.ColumnsFMRI[ii].CalMax;
                FMRIDataParams.columnId = ii;

                FMRIDataParams.calValues = m_ColumnManager.DLLVolumeFMriList[ii].ExtremeValues; 
                FMRIDataParams.singlePatient = Type == SceneType.SinglePatient;

                Events.OnSendFMRIParameters.Invoke(FMRIDataParams);
            }
        }
        /// <summary>
        /// Send an event for displaying a message on the scene screen
        /// </summary>
        /// <param name="message"></param>
        /// <param name="duration"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DisplayScreenMessage(string message, float duration, int width, int height)
        {
            Events.OnDisplaySceneMessage.Invoke(message, duration, width , height);
        }
        /// <summary>
        /// Send an event for displaying a progressbar on the scene screen
        /// </summary>
        /// <param name="value"></param>
        /// <param name="duration"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DisplayProgressbar(float value, float duration, int width, int height)
        {
            Events.OnDisplaySceneProgressBar.Invoke(duration, width, height, value);
        }
        /// <summary>
        /// Begin the comparison between two sites
        /// </summary>
        public void CompareSites()
        {
            DisplayScreenMessage("Select site to compare ", 3f, 200, 40);
            SceneInformation.IsComparingSites = true;
        }
        /// <summary>
        /// Unselect the site of the corresponding column
        /// </summary>
        /// <param name="columnId"></param>
        public void UnselectSite(Column3D column)
        {
            column.SelectedSiteID = -1; // unselect current site
            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
            ApplicationState.Module3D.OnSelectSite.Invoke(null);
            Events.OnClickSite.Invoke(-1); // update menu
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idColumn"></param>
        public void UpdateCurrentRegionOfInterest(Column3D column)
        {
            if (column.SelectedROI == null)
            {
                foreach (Site site in column.Sites)
                {
                    site.Information.IsOutOfROI = false;
                }
            }
            else
            {
                bool[] maskROI = new bool[m_ColumnManager.SitesList.Count];

                // update mask ROI
                for (int ii = 0; ii < maskROI.Length; ++ii)
                    maskROI[ii] = column.Sites[ii].Information.IsOutOfROI;

                column.SelectedROI.UpdateMask(column.RawElectrodes, maskROI);
                for (int ii = 0; ii < column.Sites.Count; ++ii)
                    column.Sites[ii].Information.IsOutOfROI = maskROI[ii];
            }
            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
        }
        /// <summary>
        /// Manage the mouse movments event in the scene
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="mousePosition"></param>
        /// /// <param name="idColumn"></param>
        public void PassiveRaycastOnScene(Ray ray, Column3D column)
        {
            if (!SceneInformation.MRILoaded) return;

            // update colliders if necessary
            UpdateMeshesColliders();

            int layerMask = 0;
            layerMask |= 1 << LayerMask.NameToLayer(column.Layer);
            layerMask |= 1 << LayerMask.NameToLayer("Default");

            RaycastHit hit;
            bool isCollision = Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask);
            if (!isCollision)
            {
                ApplicationState.Module3D.OnDisplaySiteInformation.Invoke(new SiteInfo(null, false, Input.mousePosition));
                return;
            }

            Site site = hit.collider.GetComponent<Site>();
            if (!site)
            {
                ApplicationState.Module3D.OnDisplaySiteInformation.Invoke(new SiteInfo(null, false, Input.mousePosition));
                return;
            }

            switch (column.Type)
            {
                case Column3D.ColumnType.FMRI:
                    ApplicationState.Module3D.OnDisplaySiteInformation.Invoke(new SiteInfo(site, true, Input.mousePosition, SiteInformationDisplayMode.FMRI, site.Information.FullName));
                    break;
                case Column3D.ColumnType.IEEG:
                    Column3DIEEG columnIEEG = column as Column3DIEEG;
                    int siteID = site.Information.SitePatientID;

                    float amplitude = 0;
                    if (columnIEEG.IEEGValuesBySiteID.Length > 0)
                    {
                        amplitude = columnIEEG.IEEGValuesBySiteID[siteID][columnIEEG.CurrentTimeLineID];
                    }
                    string timeline = columnIEEG.CurrentTimeLineID.ToString() + " (" + columnIEEG.CurrentTimeLine.ToString("N2") + columnIEEG.TimeLineUnite + ")";
                    switch (Type)
                    {
                        case SceneType.SinglePatient:
                            string latency = "none", height = "none";
                            if (columnIEEG.CurrentLatencyFile != -1)
                            {
                                Latencies latencyFile = m_ColumnManager.LatenciesFiles[columnIEEG.CurrentLatencyFile];

                                if (columnIEEG.SourceSelectedID == -1) // no source selected
                                {
                                    latency = "...";
                                    height = "no source selected";
                                }
                                else if (columnIEEG.SourceSelectedID == siteID) // site is the source
                                {
                                    latency = "0";
                                    height = "source";
                                }
                                else
                                {
                                    if (latencyFile.IsSiteResponsiveForSource(siteID, columnIEEG.SourceSelectedID))
                                    {
                                        latency = "" + latencyFile.LatenciesValues[columnIEEG.SourceSelectedID][siteID];
                                        height = "" + latencyFile.LatenciesValues[columnIEEG.SourceSelectedID][siteID];
                                    }
                                    else
                                    {
                                        latency = "No data";
                                        height = "No data";
                                    }
                                }
                            }
                            ApplicationState.Module3D.OnDisplaySiteInformation.Invoke(new SiteInfo(site, true, Input.mousePosition, SceneInformation.DisplayCCEPMode ? SiteInformationDisplayMode.CCEP : SiteInformationDisplayMode.IEEG, site.Information.FullName, "" + amplitude, timeline, height, latency));
                            break;
                        case SceneType.MultiPatients:
                            ApplicationState.Module3D.OnDisplaySiteInformation.Invoke(new SiteInfo(site, true, Input.mousePosition, SiteInformationDisplayMode.IEEG, site.Information.FullName, "" + amplitude, timeline));
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Manage the clicks on the scene
        /// </summary>
        /// <param name="ray"></param>
        public void ClickOnScene(Ray ray)
        {
            if (!SceneInformation.MRILoaded) return;

            int layerMask = 0;
            layerMask |= 1 << LayerMask.NameToLayer(m_ColumnManager.SelectedColumn.Layer);
            layerMask |= 1 << LayerMask.NameToLayer("Default");

            RaycastHit hit;
            bool isCollision = Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask);
            if (!isCollision)
            {
                UnselectSite(m_ColumnManager.SelectedColumn);
                return;
            }

            // FIXME : maybe create a component instead of checking the name of the parent
            bool cutHit = hit.transform.parent.gameObject.name == "Cuts";
            bool meshHit = hit.transform.parent.gameObject.name == "Brains" || hit.transform.parent.gameObject.name == "Erased Brains";
            bool siteHit = hit.collider.GetComponent<Site>() != null;
            bool roiHit = hit.collider.GetComponent<Bubble>() != null;

            if (siteHit)
            {
                Site site = hit.collider.gameObject.GetComponent<Site>();
                m_ColumnManager.SelectedColumn.SelectedSiteID = site.Information.GlobalID;
                m_ColumnManager.SelectedColumn.SelectedPatientID = site.Information.PatientID;
                if (m_ColumnManager.SelectedColumn.Type == Column3D.ColumnType.IEEG && Type == SceneType.SinglePatient)
                {
                    Column3DIEEG columnIEEG = (Column3DIEEG)m_ColumnManager.SelectedColumn;
                    m_ColumnManager.LatencyFileAvailable = (columnIEEG.CurrentLatencyFile != -1);
                    columnIEEG.SourceDefined = false;
                    columnIEEG.IsSiteASource = false;
                    columnIEEG.SiteLatencyData = false;
                    if (m_ColumnManager.LatencyFileAvailable)
                    {
                        columnIEEG.SourceDefined = (columnIEEG.SourceSelectedID != -1);
                        if (m_ColumnManager.LatenciesFiles[columnIEEG.CurrentLatencyFile].IsSiteASource(columnIEEG.SelectedSiteID))
                        {
                            columnIEEG.IsSiteASource = true;
                        }
                        if (columnIEEG.SourceDefined)
                        {
                            columnIEEG.SiteLatencyData = m_ColumnManager.LatenciesFiles[columnIEEG.CurrentLatencyFile].IsSiteResponsiveForSource(columnIEEG.SelectedSiteID, columnIEEG.SourceSelectedID);
                        }
                    }
                }
                ClickOnSiteCallback();
                m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
                ApplicationState.Module3D.OnSelectSite.Invoke(site);
            }
            else
            {
                if (SceneInformation.IsROICreationModeEnabled)
                {
                    ROI selectedROI = m_ColumnManager.SelectedColumn.SelectedROI;
                    if (selectedROI)
                    {
                        if (roiHit)
                        {
                            if (m_ColumnManager.SelectedColumn.SelectedROI.CheckCollision(ray))
                            {
                                int bubbleID = m_ColumnManager.SelectedColumn.SelectedROI.CollidedClosestBubbleID(ray);
                                selectedROI.SelectBubble(bubbleID);
                            }
                        }
                        else if (meshHit || cutHit)
                        {
                            selectedROI.AddBubble(m_ColumnManager.SelectedColumn.Layer, "Bubble", hit.point, 5.0f);
                            m_ColumnManager.UpdateAllColumnsSitesRendering(SceneInformation);
                        }
                        else
                        {
                            selectedROI.UnselectBubble();
                        }
                    }
                }
                else
                {
                    if (meshHit)
                    {
                        if (m_TriEraser.IsEnabled && m_TriEraser.IsClickAvailable)
                        {
                            m_TriEraser.EraseTriangles(ray.direction, hit.point);
                            for (int i = 0; i < m_ColumnManager.DLLSplittedMeshesList.Count; i++)
                            {
                                m_ColumnManager.DLLSplittedMeshesList[i].UpdateMeshFromDLL(m_DisplayedObjects.BrainSurfaceMeshes[i].GetComponent<MeshFilter>().mesh);
                            }
                        }
                    }
                }
            }

            if (!siteHit)
            {
                UnselectSite(m_ColumnManager.SelectedColumn);
            }
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// 
        /// </summary>
        public abstract void ComputeMeshesCut();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="previousPlot"></param>
        public abstract void SendAdditionalSiteInfoRequest(Site previousPlot = null);
        #endregion

        #region Coroutines
        private IEnumerator c_ComputeGenerators()
        {
            yield return Ninja.JumpToUnity;
            LoadingCircle loadingCircle = ApplicationState.LoadingManager.Open();
            GenericEvent<float, float, string> OnChangeLoadingProgress = new GenericEvent<float, float, string>();
            OnChangeLoadingProgress.AddListener((progress, time, message) => loadingCircle.ChangePercentage(progress, time, message));
            Task loadingTask;
            yield return this.StartCoroutineAsync(c_LoadIEEG(OnChangeLoadingProgress), out loadingTask);
            switch (loadingTask.State)
            {
                case TaskState.Done:
                    yield return new WaitForSeconds(0.5f);
                    break;
                case TaskState.Error:
                    Exception exception = loadingTask.Exception;
                    ApplicationState.DialogBoxManager.Open(DialogBoxManager.AlertType.Error, exception.ToString(), exception.Message);
                    break;
            }
            FinalizeGeneratorsComputing();
            ComputeIEEGTextures();
            loadingCircle.Close();
        }
        private IEnumerator c_LoadIEEG(GenericEvent<float, float, string> onChangeProgress = null)
        {
            float progress = 0.0f;
            float progressStep = 1.0f / ColumnManager.ColumnsIEEG.Count;

            bool useMultiCPU = true;
            bool addValues = false;
            bool ratioDistances = true;

            // copy from main generators
            for (int ii = 0; ii < m_ColumnManager.ColumnsIEEG.Count; ++ii)
            {
                for (int jj = 0; jj < m_ColumnManager.MeshSplitNumber; ++jj)
                {
                    m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj] = (DLL.MRIBrainGenerator)m_ColumnManager.DLLCommonBrainTextureGeneratorList[jj].Clone();
                }
            }
            float offsetState = 1f / (2 * m_ColumnManager.ColumnsIEEG.Count);

            // Do your threaded task. DON'T use the Unity API here
            if (SceneInformation.MeshTypeToDisplay != SceneStatesInfo.MeshType.Inflated)
            {
                for (int ii = 0; ii < m_ColumnManager.ColumnsIEEG.Count; ++ii)
                {
                    yield return Ninja.JumpToUnity;
                    progress += progressStep;
                    onChangeProgress.Invoke(progress, 1.0f, "Computing IEEG for column n°" + ii);
                    yield return Ninja.JumpBack;
                    
                    float currentMaxDensity, currentMinInfluence, currentMaxInfluence;
                    float maxDensity = 1;

                    m_ColumnManager.ColumnsIEEG[ii].SharedMinInf = float.MaxValue;
                    m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf = float.MinValue;

                    // update raw electrodes
                    m_ColumnManager.ColumnsIEEG[ii].UpdateDLLSitesMask();

                    // splits
                    for (int jj = 0; jj < m_ColumnManager.MeshSplitNumber; ++jj)
                    {
                        m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].InitializeOctree(m_ColumnManager.ColumnsIEEG[ii].RawElectrodes);


                        if (!m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].ComputeDistances(m_ColumnManager.ColumnsIEEG[ii].IEEGParameters.MaximumInfluence, true))
                        {
                            Debug.LogError("Abort computing"); // useless
                            yield return null;
                        }

                        if (!m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].ComputeInfluences(m_ColumnManager.ColumnsIEEG[ii], useMultiCPU, addValues, ratioDistances))
                        {
                            Debug.LogError("Abort computing"); // useless
                            yield return null;
                        }
                        currentMaxDensity = m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].MaximumDensity;
                        currentMinInfluence = m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].MinimumInfluence;
                        currentMaxInfluence = m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].MaximumInfluence;

                        if (currentMaxDensity > maxDensity)
                            maxDensity = currentMaxDensity;

                        if (currentMinInfluence < m_ColumnManager.ColumnsIEEG[ii].SharedMinInf)
                            m_ColumnManager.ColumnsIEEG[ii].SharedMinInf = currentMinInfluence;

                        if (currentMaxInfluence > m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf)
                            m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf = currentMaxInfluence;

                    }

                    // cuts
                    for (int jj = 0; jj < m_ColumnManager.PlanesCutsCopy.Count; ++jj)
                    {
                        m_ColumnManager.ColumnsIEEG[ii].DLLMRITextureCutGenerators[jj].InitializeOctree(m_ColumnManager.ColumnsIEEG[ii].RawElectrodes);
                        m_ColumnManager.ColumnsIEEG[ii].DLLMRITextureCutGenerators[jj].ComputeDistances(m_ColumnManager.ColumnsIEEG[ii].IEEGParameters.MaximumInfluence, true);

                        if (!m_ColumnManager.ColumnsIEEG[ii].DLLMRITextureCutGenerators[jj].ComputeInfluences(m_ColumnManager.ColumnsIEEG[ii], useMultiCPU, addValues, ratioDistances))
                        {
                            Debug.LogError("Abort computing");
                            yield return null;
                        }

                        currentMaxDensity = m_ColumnManager.ColumnsIEEG[ii].DLLMRITextureCutGenerators[jj].MaximumDensity;
                        currentMinInfluence = m_ColumnManager.ColumnsIEEG[ii].DLLMRITextureCutGenerators[jj].MinimumInfluence;
                        currentMaxInfluence = m_ColumnManager.ColumnsIEEG[ii].DLLMRITextureCutGenerators[jj].MaximumInfluence;

                        if (currentMaxDensity > maxDensity)
                            maxDensity = currentMaxDensity;

                        if (currentMinInfluence < m_ColumnManager.ColumnsIEEG[ii].SharedMinInf)
                            m_ColumnManager.ColumnsIEEG[ii].SharedMinInf = currentMinInfluence;

                        if (currentMaxInfluence > m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf)
                            m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf = currentMaxInfluence;
                    }

                    // synchronize max density
                    for (int jj = 0; jj < m_ColumnManager.MeshSplitNumber; ++jj)
                        m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].SynchronizeWithOthersGenerators(maxDensity, m_ColumnManager.ColumnsIEEG[ii].SharedMinInf, m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf);
                    for (int jj = 0; jj < m_ColumnManager.PlanesCutsCopy.Count; ++jj)
                        m_ColumnManager.ColumnsIEEG[ii].DLLMRITextureCutGenerators[jj].SynchronizeWithOthersGenerators(maxDensity, m_ColumnManager.ColumnsIEEG[ii].SharedMinInf, m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf);

                    for (int jj = 0; jj < m_ColumnManager.MeshSplitNumber; ++jj)
                        m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].AdjustInfluencesToColormap();
                    for (int jj = 0; jj < m_ColumnManager.PlanesCutsCopy.Count; ++jj)
                        m_ColumnManager.ColumnsIEEG[ii].DLLMRITextureCutGenerators[jj].AdjustInfluencesToColormap();
                }
            }
            else // if inflated white mesh is displayed, we compute only on the complete white mesh
            {
                for (int ii = 0; ii < m_ColumnManager.ColumnsIEEG.Count; ++ii)
                {
                    yield return Ninja.JumpToUnity;
                    progress += progressStep;
                    onChangeProgress.Invoke(progress, 1.0f, "Loading IEEG for column " + ii);
                    yield return Ninja.JumpBack;
                    
                    float currentMaxDensity, currentMinInfluence, currentMaxInfluence;
                    float maxDensity = 1;

                    m_ColumnManager.ColumnsIEEG[ii].SharedMinInf = float.MaxValue;
                    m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf = float.MinValue;

                    // update raw electrodes
                    m_ColumnManager.ColumnsIEEG[ii].UpdateDLLSitesMask();

                    // splits
                    for (int jj = 0; jj < m_ColumnManager.MeshSplitNumber; ++jj)
                    {
                        m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].Reset(m_ColumnManager.DLLSplittedWhiteMeshesList[jj], m_ColumnManager.DLLVolume); // TODO : ?
                        m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].InitializeOctree(m_ColumnManager.ColumnsIEEG[ii].RawElectrodes);

                        if (!m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].ComputeDistances(m_ColumnManager.ColumnsIEEG[ii].IEEGParameters.MaximumInfluence, true))
                        {
                            Debug.LogError("Abort computing");
                            yield return null;
                        }

                        if (!m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].ComputeInfluences(m_ColumnManager.ColumnsIEEG[ii], useMultiCPU, addValues, ratioDistances))
                        {
                            Debug.LogError("Abort computing");
                            yield return null;
                        }

                        currentMaxDensity = m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].MaximumDensity;
                        currentMinInfluence = m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].MinimumInfluence;
                        currentMaxInfluence = m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].MaximumInfluence;

                        if (currentMaxDensity > maxDensity)
                            maxDensity = currentMaxDensity;

                        if (currentMinInfluence < m_ColumnManager.ColumnsIEEG[ii].SharedMinInf)
                            m_ColumnManager.ColumnsIEEG[ii].SharedMinInf = currentMinInfluence;

                        if (currentMaxInfluence > m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf)
                            m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf = currentMaxInfluence;
                    }

                    // synchronize max density
                    for (int jj = 0; jj < m_ColumnManager.MeshSplitNumber; ++jj)
                        m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].SynchronizeWithOthersGenerators(maxDensity, m_ColumnManager.ColumnsIEEG[ii].SharedMinInf, m_ColumnManager.ColumnsIEEG[ii].SharedMaxInf);

                    for (int jj = 0; jj < m_ColumnManager.MeshSplitNumber; ++jj)
                        m_ColumnManager.ColumnsIEEG[ii].DLLBrainTextureGenerators[jj].AdjustInfluencesToColormap();
                }
            }
        }
        /// <summary>
        /// Reset the volume of the scene
        /// </summary>
        /// <param name="pathNIIBrainVolumeFile"></param>
        /// <returns></returns>
        protected IEnumerator c_LoadNiftiBrainVolume(string pathNIIBrainVolumeFile)
        {
            // Check access
            if (!m_ModesManager.FunctionAccess(Mode.FunctionsId.ResetNIIBrainVolumeFile))
            {
                throw new ModeAccessException(m_ModesManager.CurrentModeName);
            }

            SceneInformation.MRILoaded = false;

            // checks parameter
            if (pathNIIBrainVolumeFile.Length == 0)
            {
                throw new EmptyFilePathException("NII");
            }

            // load volume
            bool loadingSuccess = m_ColumnManager.DLLNii.LoadNIIFile(pathNIIBrainVolumeFile);
            if (loadingSuccess)
            {
                m_ColumnManager.DLLNii.ConvertToVolume(m_ColumnManager.DLLVolume);
                SceneInformation.VolumeCenter = m_ColumnManager.DLLVolume.Center;
            }
            else
            {
                throw new CanNotLoadNIIFile(pathNIIBrainVolumeFile);
            }

            SceneInformation.MRILoaded = loadingSuccess;
            Events.OnUpdatePlanes.Invoke();

            // send cal values to the UI
            Events.OnMRICalValuesUpdate.Invoke(m_ColumnManager.DLLVolume.ExtremeValues);

            // Update mode
            m_ModesManager.UpdateMode(Mode.FunctionsId.ResetNIIBrainVolumeFile);

            yield return SceneInformation.MRILoaded;
        }
        #endregion
    }
}