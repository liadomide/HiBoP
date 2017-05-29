﻿/**
 * \file    Column3DViewManager.cs
 * \author  Lance Florian
 * \date    2015
 * \brief   Define Column3DViewManager class
 */
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HBP.Module3D
{
    /// <summary>
    /// A class for managing all the columns data from a specific scene
    /// </summary>
    public class Column3DManager : MonoBehaviour
    {
        #region Properties
        // mani (debug)
        public Color[] ColorsSites = null;

        public int SelectedPatientID = 0; /**< id of the selected patient for Multi patient scene */
        public View3D SelectedView
        {
            get
            {
                foreach (Column3D column in Columns)
                {
                    foreach (View3D view in column.Views)
                    {
                        if (view.IsSelected)
                        {
                            return view;
                        }
                    }
                }
                return null;
            }
        }
        public View3D ClickedView
        {
            get
            {
                foreach (Column3D column in Columns)
                {
                    foreach (View3D view in column.Views)
                    {
                        if (view.IsClicked)
                        {
                            return view;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// ID of the seleccted column
        /// </summary>
        public int SelectedColumnID 
        {
            get
            {
                return m_Columns.FindIndex((c) => c.IsSelected);
            }
        }
        /// <summary>
        /// Selected column
        /// </summary>
        public Column3D SelectedColumn
        {
            get
            {
                return m_Columns.Find((c) => c.IsSelected);
            }
        }

        public GenericEvent<Column3DManager> OnSelectColumnManager = new GenericEvent<Column3DManager>();

        List<Column3D> m_Columns = new List<Column3D>();
        public ReadOnlyCollection<Column3D> Columns { get { return m_Columns != null ? new ReadOnlyCollection<Column3D>(m_Columns) : new ReadOnlyCollection<Column3D>(new List<Column3D>(0)); } }
        public ReadOnlyCollection<Column3DIEEG> ColumnsIEEG { get { return m_Columns != null ? new ReadOnlyCollection<Column3DIEEG>((from column in m_Columns where column is Column3DIEEG select (Column3DIEEG)column).ToArray()) : new ReadOnlyCollection<Column3DIEEG>(new List<Column3DIEEG>(0)); } }
        public ReadOnlyCollection<Column3DFMRI> ColumnsFMRI { get { return m_Columns != null ? new ReadOnlyCollection<Column3DFMRI>((from column in m_Columns where column is Column3DFMRI select (Column3DFMRI)column).ToArray()) : new ReadOnlyCollection<Column3DFMRI>(new List<Column3DFMRI>(0)); } }

        /// <summary>
        /// Maximum number of view in a column
        /// </summary>
        public int ViewNumber
        {
            get
            {
                int viewNumber = 0;
                foreach (Column3D column in m_Columns)
                {
                    if (column.Views.Count > viewNumber)
                    {
                        viewNumber = column.Views.Count;
                    }
                }
                return viewNumber;
            }
        }

        // plots
        public DLL.RawSiteList DLLLoadedRawSitesList = null;
        public DLL.PatientElectrodesList DLLLoadedPatientsElectrodes = null;
        public List<GameObject> SitesList = new List<GameObject>();
        public List<GameObject> SitesPatientParent = new List<GameObject>(); /**< plots patient parents of the scene */
        public List<List<GameObject>> SitesElectrodesParent = new List<List<GameObject>>(); /**< plots electrodes parents of the scene */

        // latency        
        public bool LatencyFilesDefined = false;
        public bool LatencyFileAvailable = false; /**< latency file is available */
        public List<Latencies> LatenciesFiles = new List<Latencies>(); /*< list of latency files */

        // timelines 
        public bool GlobalTimeline = true;  /**< is global timeline enabled */
        public float CommonTimelineValue = 0f; /**< commmon value of the timelines */

        // textures
        public List<Vector2[]> UVNull = null;                   /**< null uv vectors */ // // new List<Vector2[]>(); 
        public Color NotInBrainColor = Color.black;


        public List<DLL.MRIBrainGenerator> DLLCommonBrainTextureGeneratorList = null; /**< common generators for each brain part  */

        // Common columns cut textures 
        //  textures 2D
        public List<Texture2D> RightGUICutTextures = null;                  /**< list of rotated cut textures| */

        //  generator DLL
        public List<DLL.MRIGeometryCutGenerator> DLLMRIGeometryCutGeneratorList = null; /**< ... */        
        

        // niftii 
        public DLL.NIFTI DLLNii = null;
        // surface 
        public int MeshSplitNumber = 1;
        public DLL.Surface LHemi = null; /**< left hemi mesh */
        public DLL.Surface RHemi = null; /**< right hemi mesh */
        public DLL.Surface BothHemi = null; /**< fustion left/right hemi mesh */
        public DLL.Surface LWhite = null; /**< left white mesh */
        public DLL.Surface RWhite = null; /**< right white mesh */
        public DLL.Surface BothWhite = null; /**< fustion left/right white mesh */

        public List<DLL.Surface> DLLCutsList = null;
        public List<DLL.Surface> DLLSplittedMeshesList = null;
        public List<DLL.Surface> DLLSplittedWhiteMeshesList = null;

        // volume
        public float MRICalMinFactor = 0f;
        public float MRICalMaxFactor = 1f;
        public DLL.Volume DLLVolume = null;
        public List<DLL.Volume> DLLVolumeFMriList = null;

        // planes
        public List<Cut> PlanesCutsCopy = new List<Cut>();

        // UV coordinates
        public List<Vector2[]> UVCoordinatesSplits = null; // uv coordinates for each brain mesh split

        //
        public bool[] CommonMask = null;


        // textures
        public ColorType BrainColor = ColorType.BrainColor;
        public ColorType BrainCutColor = ColorType.Default;
        public ColorType Colormap = ColorType.MatLab; // TO move
        public Texture2D BrainColorMapTexture = null;
        public Texture2D BrainColorTexture = null;

        // Column 3D Prefabs
        public GameObject Column3DViewIEEGPrefab;
        public GameObject Column3DViewFMRIPrefab;
        #endregion

        #region Private Methods
        private void Awake()
        {
            //Initialize(0);
            //UpdateColumnsNumber(0, 0, 0);

            BrainColorMapTexture = Texture2Dutility.GenerateColorScheme();
            BrainColorTexture = Texture2Dutility.GenerateColorScheme();
        }
        private void LateUpdate()
        {
            SynchronizeViewsToSelectedView();
        }
        /// <summary>
        /// 
        /// </summary>
        private void AddIEEGColumn()
        {
            Column3DIEEG column = Instantiate(Column3DViewIEEGPrefab, transform.Find("Columns")).GetComponent<Column3DIEEG>();
            column.gameObject.name = "Column IEEG " + ColumnsIEEG.Count;
            column.OnSelectColumn.AddListener((selectedColumn) =>
            {
                Debug.Log("OnSelectColumn");
                foreach (Column3D c in m_Columns)
                {
                    if (c != selectedColumn)
                    {
                        c.IsSelected = false;
                        foreach (View3D v in c.Views)
                        {
                            v.IsSelected = false;
                            v.IsColumnSelected = false;
                        }
                    }
                }
                foreach (View3D v in selectedColumn.Views)
                {
                    v.IsColumnSelected = true;
                }
                OnSelectColumnManager.Invoke(this);
            });
            m_Columns.Add(column);
            ApplicationState.Module3D.OnAddColumn.Invoke();
        }
        /// <summary>
        /// 
        /// </summary>
        private void AddFMRIColumn()
        {
            Column3DFMRI column = Instantiate(Column3DViewFMRIPrefab, transform.Find("Columns")).GetComponent<Column3DFMRI>();
            column.gameObject.name = "Column FMRI " + ColumnsFMRI.Count;
            column.OnSelectColumn.AddListener((selectedColumn) =>
            {
                Debug.Log("OnSelectColumn");
                foreach (Column3D c in m_Columns)
                {
                    if (c != selectedColumn)
                    {
                        c.IsSelected = false;
                        foreach (View3D v in c.Views)
                        {
                            v.IsSelected = false;
                            v.IsColumnSelected = false;
                        }
                    }
                }
                foreach (View3D v in selectedColumn.Views)
                {
                    v.IsColumnSelected = true;
                }
                OnSelectColumnManager.Invoke(this);
            });
            m_Columns.Add(column);
            ApplicationState.Module3D.OnAddColumn.Invoke();
        }
        /// <summary>
        /// 
        /// </summary>
        private void RemoveIEEGColumn()
        {
            if (ColumnsIEEG.Count > 0)
            {
                Column3DIEEG column = ColumnsIEEG[ColumnsIEEG.Count - 1];
                int columnID = m_Columns.IndexOf(column);
                Destroy(m_Columns[columnID]);
                m_Columns.RemoveAt(columnID);
                ApplicationState.Module3D.OnRemoveColumn.Invoke(column);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void RemoveFMRIColumn()
        {
            if (ColumnsIEEG.Count > 0)
            {
                Column3DFMRI column = ColumnsFMRI[ColumnsFMRI.Count - 1];
                int columnID = m_Columns.IndexOf(column);
                Destroy(m_Columns[columnID]);
                m_Columns.RemoveAt(columnID);
                ApplicationState.Module3D.OnRemoveColumn.Invoke(column);
            }
        }
        /// <summary>
        /// Synchronize all the cameras from the same view line
        /// </summary>
        private void SynchronizeViewsToSelectedView()
        {
            if (ClickedView != null)
            {
                foreach (Column3D column in Columns)
                {
                    foreach (View3D view in column.Views)
                    {
                        if (view.LineID == ClickedView.LineID)
                        {
                            view.SynchronizeCamera(SelectedView);
                        }
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Reset all data.
        /// </summary>
        public void Initialize(int cutPlanesNb)
        {
            // init DLL objects
            //      nii loader;
            DLLNii = new DLL.NIFTI();

            // surfaces
            LHemi = new DLL.Surface();
            RHemi = new DLL.Surface();
            LWhite = new DLL.Surface();
            RWhite = new DLL.Surface();
            BothHemi = new DLL.Surface();
            BothWhite = new DLL.Surface();
            DLLCutsList = new List<DLL.Surface>();

            // volume
            if (DLLVolume != null)
                DLLVolume.Dispose();
            DLLVolume = new DLL.Volume();

            if (DLLVolumeFMriList != null)
                for (int ii = 0; ii < DLLVolumeFMriList.Count; ++ii)
                    DLLVolumeFMriList[ii].Dispose();
            DLLVolumeFMriList = new List<DLL.Volume>();

            // electrodes
            DLLLoadedPatientsElectrodes = new DLL.PatientElectrodesList();

            // cuts
            //  textures 2D            
            RightGUICutTextures = new List<Texture2D>(cutPlanesNb);

            //  DLL generators
            DLLMRIGeometryCutGeneratorList = new List<DLL.MRIGeometryCutGenerator>(cutPlanesNb);
            for (int ii = 0; ii < cutPlanesNb; ++ii)
            {                
                RightGUICutTextures.Add(new Texture2D(1, 1));                
                DLLMRIGeometryCutGeneratorList.Add(new DLL.MRIGeometryCutGenerator());
            }

            if (m_Columns != null)
            {
                for (int c = 0; c < m_Columns.Count; c++)
                {
                    m_Columns[c].Clear();
                    Destroy(m_Columns[c].gameObject);
                }
                m_Columns.Clear();
            }
            else m_Columns = new List<Column3D>();

            ResetSplitsNumber(1);
        }
        /// <summary>
        /// Reset the number of meshes splits for the brain
        /// </summary>
        /// <param name="nbSplits"></param>
        public void ResetSplitsNumber(int nbSplits)
        {
            DLLSplittedMeshesList = new List<DLL.Surface>(MeshSplitNumber);
            DLLSplittedWhiteMeshesList = new List<DLL.Surface>(MeshSplitNumber);

            // uv coordinates
            UVCoordinatesSplits = new List<Vector2[]>(Enumerable.Repeat(new Vector2[0], MeshSplitNumber));

            // brain
            //  generators
            DLLCommonBrainTextureGeneratorList = new List<DLL.MRIBrainGenerator>(MeshSplitNumber);
            for (int ii = 0; ii < MeshSplitNumber; ++ii)
                DLLCommonBrainTextureGeneratorList.Add(new DLL.MRIBrainGenerator());

            for (int c = 0; c < m_Columns.Count; c++)
            {
                m_Columns[c].ResetSplitsNumber(nbSplits);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        public void UpdateColormap(ColorType color)
        {
            Colormap = color;
            DLL.Texture tex = DLL.Texture.Generate1DColorTexture(Colormap);
            tex.UpdateTexture2D(BrainColorMapTexture);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        public void UpdateBrainCutColor(ColorType color)
        {
            BrainCutColor = color;
        }
        /// <summary>
        /// 
        /// </summary>
        public void ResetColors()
        {
            for (int ii = 0; ii < m_Columns.Count; ++ii)
                Columns[ii].ResetColorSchemes(Colormap, BrainCutColor);
        }
        /// <summary>
        /// Update the number of cut planes for every column
        /// </summary>
        /// <param name="nbCuts"></param>
        public void UpdateCutNumber(int nbCuts)
        {
            // update common
            int diffCuts = DLLMRIGeometryCutGeneratorList.Count - nbCuts;
            if (diffCuts < 0)
            {
                // textures 2D
                for (int ii = 0; ii < -diffCuts; ++ii)
                {
                    // GO textures        
                    RightGUICutTextures.Add(new Texture2D(1, 1));
                    int id = RightGUICutTextures.Count - 1;
                    RightGUICutTextures[id].filterMode = FilterMode.Trilinear; // TODO : test performances with this parameter
                    RightGUICutTextures[id].wrapMode = TextureWrapMode.Clamp;
                    RightGUICutTextures[id].anisoLevel = 9; // TODO : test performances with this parameter
                    RightGUICutTextures[id].mipMapBias = -2; // never superior to -1 (colorscheme 8 texture glitch)    

                    // DLL generators       
                    DLLMRIGeometryCutGeneratorList.Add(new DLL.MRIGeometryCutGenerator());
                }
            }
            else if (diffCuts > 0)
            {                
                for (int ii = 0; ii < diffCuts; ++ii)
                {
                    // GO textures
                    Destroy(RightGUICutTextures[RightGUICutTextures.Count - 1]);
                    RightGUICutTextures.RemoveAt(RightGUICutTextures.Count - 1);

                    // DLL generators
                    DLLMRIGeometryCutGeneratorList[DLLMRIGeometryCutGeneratorList.Count - 1].Dispose();
                    DLLMRIGeometryCutGeneratorList.RemoveAt(DLLMRIGeometryCutGeneratorList.Count - 1);
                }
            }

            // update columns
            for (int c = 0; c < m_Columns.Count; c++)
            {
                m_Columns[c].UpdateCutsPlanesNumber(diffCuts);
            }
        }
        /// <summary>
        /// Update the number of columns and set the number of cut planes for each ones
        /// </summary>
        /// <param name="nbIEEGColumns"></param>
        /// /// <param name="nbIRMFColumns"></param>
        /// <param name="nbCuts"></param>
        public void UpdateColumnsNumber(int nbIEEGColumns, int nbIRMFColumns, int nbCuts) //FIXME : rework this function (make it disappear and only use add columns methods)
        {            
            // clean data columns if changes in data columns nb
            if (nbIEEGColumns != ColumnsIEEG.Count)
            {
                foreach (var column in ColumnsIEEG) column.Clear();
            }

            // clean IRMF columns if changes in IRMF columns nb
            if (nbIRMFColumns != ColumnsFMRI.Count)
            {
                foreach (var column in ColumnsFMRI) column.Clear();
            }

            // resize the data GO list            
            int diffIEEGColumns = ColumnsIEEG.Count - nbIEEGColumns;
            if (diffIEEGColumns < 0)
            {
                for (int ii = 0; ii < -diffIEEGColumns; ++ii)
                {
                    AddIEEGColumn();
                }
            }
            else if (diffIEEGColumns > 0)
            {
                for (int ii = 0; ii < diffIEEGColumns; ++ii)
                {
                    RemoveIEEGColumn();
                }
            }

            // resize the IRMF GO list      
            int diffIRMFColumns = ColumnsFMRI.Count - nbIRMFColumns;
            if (diffIRMFColumns < 0)
            {
                for (int ii = 0; ii < -diffIRMFColumns; ++ii)
                {
                    // add column
                    DLLVolumeFMriList.Add(new DLL.Volume());
                    AddFMRIColumn();
                }
            }
            else if (diffIRMFColumns > 0)
            {
                for (int ii = 0; ii < diffIRMFColumns; ++ii)
                {
                    // destroy column
                    int idColumn = ColumnsFMRI.Count - 1;
                    RemoveFMRIColumn();

                    DLLVolumeFMriList[idColumn].Dispose();
                    DLLVolumeFMriList.RemoveAt(idColumn);
                }
            }

            // init new columns IEEG            
            if (diffIEEGColumns != 0)
            {
                for (int ii = 0; ii < nbIEEGColumns; ++ii)
                {
                    ColumnsIEEG[ii].Initialize(ii, nbCuts, DLLLoadedPatientsElectrodes, SitesPatientParent);
                    ColumnsIEEG[ii].ResetSplitsNumber(MeshSplitNumber);

                    if (LatencyFilesDefined)
                        ColumnsIEEG[ii].CurrentLatencyFile = 0;
                }
            }

            // init new columns IRMF
            if (diffIRMFColumns != 0)
            {
                // update IRMF columns mask
                bool[] maskColumnsOR = new bool[DLLLoadedPatientsElectrodes.TotalSitesNumber];
                for (int ii = 0; ii < SitesList.Count; ++ii)
                {
                    bool mask = false;

                    for (int jj = 0; jj < ColumnsIEEG.Count; ++jj)
                    {
                        mask = mask || ColumnsIEEG[jj].Sites[ii].Information.IsMasked;
                    }

                    maskColumnsOR[ii] = mask;
                }

                for (int ii = 0; ii < ColumnsFMRI.Count; ++ii)
                {
                    for (int jj = 0; jj < SitesList.Count; ++jj)
                    {
                        ColumnsFMRI[ii].Sites[jj].Information.IsMasked = maskColumnsOR[jj];
                    }
                }
                
                for (int ii = 0; ii < nbIRMFColumns; ++ii)
                {
                    ColumnsFMRI[ColumnsFMRI.Count - 1].Initialize(ColumnsIEEG.Count + ii, nbCuts, DLLLoadedPatientsElectrodes, SitesPatientParent);
                    ColumnsFMRI[ColumnsFMRI.Count - 1].ResetSplitsNumber(MeshSplitNumber);

                    for (int jj = 0; jj < SitesList.Count; ++jj)
                    {
                        ColumnsFMRI[ColumnsFMRI.Count - 1].Sites[jj].Information.IsMasked = maskColumnsOR[jj];
                    }
                }
            }


            CommonMask = new bool[DLLLoadedPatientsElectrodes.TotalSitesNumber];

            if (SelectedColumnID >= m_Columns.Count && SelectedColumnID > 0)
                m_Columns.Last().IsSelected = true;


            for (int ii = 0; ii < m_Columns.Count; ++ii)
                Columns[ii].ResetColorSchemes(Colormap, BrainCutColor);
        }
        /// <summary>
        /// Define the single patient and associated data
        /// </summary>
        /// <param name="patient"></param>
        /// <param name="columnDataList"></param>
        public void SetTimelineData(Data.Patient patient, List<Data.Visualization.Column> columnDataList)
        {
            for (int c = 0; c < ColumnsIEEG.Count; c++)
            {
                ColumnsIEEG[c].SetColumnData(columnDataList[c]);
            }
        }
        /// <summary>
        /// Define the mp patients list and associated data
        /// </summary>
        /// <param name="patientList"></param>
        /// <param name="columnDataList"></param>
        /// <param name="ptsPathFileList"></param>
        public void SetTimelineData(List<Data.Patient> patientList, List<Data.Visualization.Column> columnDataList)
        {
            for (int c = 0; c < ColumnsIEEG.Count; c++)
            {
                ColumnsIEEG[c].SetColumnData(columnDataList[c]);
            }
        }
        /// <summary>
        /// Create the cut mesh texture dll and texture2D
        /// </summary>
        /// <param name="indexCut"></param>
        public void CreateMRITexture(int indexCut, int indexColumn)
        {
            UnityEngine.Profiling.Profiler.BeginSample("create_MRI_texture");
                Columns[indexColumn].CreateMRITexture(DLLMRIGeometryCutGeneratorList[indexCut], DLLVolume, indexCut, MRICalMinFactor, MRICalMaxFactor);
            UnityEngine.Profiling.Profiler.EndSample();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="indexColumn"></param>
        public void CreateGUIMRITexture(int indexCut, int indexColumn)
        {
            string orientation = "";
            switch (PlanesCutsCopy[indexCut].Orientation)
            {
                case CutOrientation.Axial:
                    orientation = "Axial";
                    break;
                case CutOrientation.Coronal:
                    orientation = "Coronal";
                    break;
                case CutOrientation.Sagital:
                    orientation = "Sagital";
                    break;
                case CutOrientation.Custom:
                    orientation = "custom";
                    break;
                default:
                    orientation = "custom";
                    break;
            }

            Columns[indexColumn].CreateGUIMRITexture(indexCut, orientation, PlanesCutsCopy[indexCut].Flip, PlanesCutsCopy, orientation != "custom");            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="indexColumn"></param>
        public void CreateGUIIEEGTexture(int indexCut, int indexColumn)
        {
            string orientation = "";
            switch (PlanesCutsCopy[indexCut].Orientation)
            {
                case CutOrientation.Axial:
                    orientation = "Axial";
                    break;
                case CutOrientation.Coronal:
                    orientation = "Coronal";
                    break;
                case CutOrientation.Sagital:
                    orientation = "Sagital";
                    break;
                case CutOrientation.Custom:
                    orientation = "custom";
                    break;
                default:
                    orientation = "custom";
                    break;
            }

            ((Column3DIEEG)Columns[indexColumn]).CreateGUIIEEGTexture(indexCut, orientation, PlanesCutsCopy[indexCut].Flip, PlanesCutsCopy, orientation != "custom");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="indexColumn"></param>
        public void CreateGUIFMRITexture(int indexCut, int indexColumn)
        {
            string orientation = "";
            switch (PlanesCutsCopy[indexCut].Orientation)
            {
                case CutOrientation.Axial:
                    orientation = "Axial";
                    break;
                case CutOrientation.Coronal:
                    orientation = "Coronal";
                    break;
                case CutOrientation.Sagital:
                    orientation = "Sagital";
                    break;
                case CutOrientation.Custom:
                    orientation = "custom";
                    break;
                default:
                    orientation = "custom";
                    break;
            }

            ((Column3DFMRI)Columns[indexColumn]).CreateGUIFMRITexture(indexCut, orientation, PlanesCutsCopy[indexCut].Flip, PlanesCutsCopy, orientation != "custom");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexColumn"></param>
        /// <param name="indexCut"></param>
        /// <param name="thresholdInfluence"></param>
        public void ColorCutsTexturesWithIEEG(int indexColumn, int indexCut)
        {
            Column3DIEEG column = ColumnsIEEG[indexColumn];            
            DLL.MRITextureCutGenerator generator = column.DLLMRITextureCutGenerators[indexCut];        
            generator.FillTextureWithIEEG(column, column.DLLCutColorScheme, NotInBrainColor);

            DLL.Texture cutTexture = column.DLLBrainCutWithIEEGTextures[indexCut];
            generator.UpdateTextureWithIEEG(cutTexture);
            cutTexture.UpdateTexture2D(column.BrainCutWithIEEGTextures[indexCut]); // update mesh cut 2D texture
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexColumn"></param>
        /// <param name="indexCut"></param>
        public void ColorCutsTexturesWithFMRI(int indexColumn, int indexCut)
        {
            Column3DFMRI column = ColumnsFMRI[indexColumn];
            DLL.MRITextureCutGenerator generator = column.DLLMRITextureCutGenerators[indexCut];
            generator.FillTextureWithFMRI(column, DLLVolumeFMriList[indexColumn]);

            DLL.Texture cutTexture = column.DLLBrainCutWithFMRITextures[indexCut];
            generator.UpdateTextureWithFMRI(cutTexture);
            cutTexture.UpdateTexture2D(column.BrainCutWithFMRITextures[indexCut]); // update mesh cut 2D texture
        }
        /// <summary>
        /// Compute the amplitudes textures coordinates for the brain mesh
        /// When to call ? changes in IEEGColumn.currentTimeLineID, IEEGColumn.alphaMin, IEEGColumn.alphaMax
        /// </summary>
        /// <param name="whiteInflatedMeshes"></param>
        /// <param name="indexColumn"></param>
        /// <param name="thresholdInfluence"></param>
        /// <param name="alphaMin"></param>
        /// <param name="alphaMax"></param>
        public bool ComputeSurfaceBrainUVWithIEEG(bool whiteInflatedMeshes, int indexColumn)
        {
            for (int ii = 0; ii < MeshSplitNumber; ++ii)
                if(!ColumnsIEEG[indexColumn].DLLBrainTextureGenerators[ii].ComputeSurfaceUVIEEG(whiteInflatedMeshes ? DLLSplittedWhiteMeshesList[ii] : DLLSplittedMeshesList[ii], ColumnsIEEG[indexColumn]))
                    return false;

            return true;
        }
        /// <summary>
        /// Update the plot rendering parameters for all columns
        /// </summary>
        public void UpdateAllColumnsSitesRendering(SceneStatesInfo data)
        {
            for (int ii = 0; ii < ColumnsIEEG.Count; ++ii)
            {
                Latencies latencyFile = null;
                if (ColumnsIEEG[ii].CurrentLatencyFile != -1)
                    latencyFile = LatenciesFiles[ColumnsIEEG[ii].CurrentLatencyFile];

                ColumnsIEEG[ii].UpdateSitesSizeAndColorForIEEG(); // TEST
                ColumnsIEEG[ii].UpdateSitesRendering(data, latencyFile);
            }

            for (int ii = 0; ii < ColumnsFMRI.Count; ++ii)
                ColumnsFMRI[ii].UpdateSitesVisibility(data);
        }
        /// <summary>
        /// Update the visiblity of the ROI for all columns
        /// </summary>
        /// <param name="visible"></param>
        public void UpdateROIVisibility(bool visible)
        {
            // disable all ROI render
            for(int ii = 0; ii < m_Columns.Count; ++ii)
                if (m_Columns[ii].SelectedROI != null)
                    m_Columns[ii].SelectedROI.SetRenderingState(false);

            if(SelectedColumn != null)
                if(SelectedColumn.SelectedROI != null)
                SelectedColumn.SelectedROI.SetRenderingState(visible);
        }
        /// <summary>
        /// Update the visiblity of the plots for all columns
        /// </summary>
        /// <param name="visible"></param>
        public void UpdateSitesVisibility(bool visible)
        {
            foreach (var column in m_Columns) column.SetSitesVisibility(visible);
        }
        /// <summary>
        /// Add a view to every columns
        /// </summary>
        public void AddViewLine()
        {
            foreach (Column3D column in m_Columns)
            {
                column.AddView();
            }
            ApplicationState.Module3D.OnAddViewLine.Invoke();
        }
        /// <summary>
        /// Remove a view from every columns
        /// </summary>
        /// <param name="lineID">ID of the line of the view to be removed</param>
        public void RemoveViewLine(int lineID)
        {
            foreach (Column3D column in m_Columns)
            {
                column.RemoveView(lineID);
            }
            ApplicationState.Module3D.OnRemoveViewLine.Invoke(ViewNumber);
        }
        #endregion
    }
}