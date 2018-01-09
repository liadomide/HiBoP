﻿/**
 * \file    Column3DView.cs
 * \author  Lance Florian
 * \date    21/03/2016
 * \brief   Define Column3DView class
 */

using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Events;
using System.Linq;
using System;
using System.IO;

namespace HBP.Module3D
{
    /// <summary>
    /// Column 3D view base class
    /// </summary>
    public class Column3D : MonoBehaviour
    {
        #region Properties
        public enum ColumnType
        {
            Base, FMRI, IEEG
        }
        public virtual ColumnType Type
        {
            get
            {
                return ColumnType.Base;
            }
        }
        public int ID { get; set; }

        [SerializeField, Candlelight.PropertyBackingField]
        protected string m_Label;
        public string Label
        {
            get { return m_Label; }
            set { m_Label = value; }
        }

        [SerializeField, Candlelight.PropertyBackingField]
        protected string m_Layer;
        public string Layer
        {
            get { return m_Layer; }
            set { m_Layer = value; }
        }

        private bool m_IsSelected;
        /// <summary>
        /// Is this column selected ?
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return m_IsSelected;
            }
            set
            {
                m_IsSelected = value;
                if (m_IsSelected)
                {
                    OnSelectColumn.Invoke(this);
                }
            }
        }

        private bool m_IsMinimized;
        public bool IsMinimized
        {
            get
            {
                return m_IsMinimized;
            }
            set
            {
                if (m_IsMinimized != value)
                {
                    m_IsMinimized = value;
                    OnChangeMinimizedState.Invoke();
                }
            }
        }

        private bool m_IsRenderingUpToDate = false;
        /// <summary>
        /// Does the column rendering need to be updated ?
        /// </summary>
        public bool IsRenderingUpToDate
        {
            get
            {
                return m_IsRenderingUpToDate;
            }
            set
            {
                m_IsRenderingUpToDate = value;
            }
        }

        [SerializeField]
        private Transform m_BrainSurfaceMeshesParent;
        [SerializeField]
        private GameObject m_BrainPrefab;
        private List<GameObject> m_BrainSurfaceMeshes = new List<GameObject>();
        public List<GameObject> BrainSurfaceMeshes
        {
            get
            {
                return m_BrainSurfaceMeshes;
            }
        }

        public GameObject ViewPrefab;
        protected List<View3D> m_Views = new List<View3D>();
        public ReadOnlyCollection<View3D> Views
        {
            get
            {
                return new ReadOnlyCollection<View3D>(m_Views);
            }
        }

        public View3D SelectedView
        {
            get
            {
                foreach (View3D view in Views)
                {
                    if (view.IsSelected)
                    {
                        return view;
                    }
                }
                return null;
            }
        }

        protected int m_SelectedSiteID = -1;
        public int SelectedSiteID
        {
            get
            {
                return m_SelectedSiteID;
            }
            set
            {
                m_SelectedSiteID = value;
                OnSelectSite.Invoke(SelectedSite);
                if (value >= 0)
                {
                    SelectedPatientID = SelectedSite.Information.PatientNumber;
                }
                else
                {
                    SelectedPatientID = -1;
                }
            }
        }
        public Site SelectedSite
        {
            get
            {
                return m_SelectedSiteID >= 0 ? Sites[m_SelectedSiteID] : null;
            }
            set
            {
                m_SelectedSiteID = Sites.FindIndex((site) => site == value);
                ApplicationState.Module3D.OnRequestUpdateInUI.Invoke();
                SelectedPatientID = value.Information.PatientNumber;
            }
        }
        public int SelectedPatientID { get; protected set; }

        protected DLL.RawSiteList m_RawElectrodes = null;  /**< raw format of the plots container dll */
        public DLL.RawSiteList RawElectrodes
        {
            get
            {
                return m_RawElectrodes;
            }
        }
        public List<List<List<GameObject>>> SitesGameObjects = null; /**< plots GO list with order : patient/electrode/plot */
        public List<Site> Sites = null; /**< plots list */
        public Dictionary<string, SiteState> SiteStateBySiteID = new Dictionary<string, SiteState>();

        // select plot
        protected SiteRing m_SelectRing = null;
        public SiteRing SelectRing { get { return m_SelectRing; } }

        // ROI
        [SerializeField]
        protected Transform m_ROIParent;
        protected List<ROI> m_ROIs = new List<ROI>();
        public ReadOnlyCollection<ROI> ROIs
        {
            get
            {
                return new ReadOnlyCollection<ROI>(m_ROIs);
            }
        }
        protected ROI m_SelectedROI = null;
        public ROI SelectedROI
        {
            get
            {
                return m_SelectedROI;
            }
            set
            {
                if (value == null)
                {
                    m_SelectedROI = null;
                }
                else
                {
                    if (m_SelectedROI != null)
                    {
                        m_SelectedROI.SetVisibility(false);
                    }

                    m_SelectedROI = value;
                    m_SelectedROI.SetVisibility(true);
                    m_SelectedROI.StartAnimation();
                }
                OnSelectROI.Invoke();
            }
        }
        public int SelectedROIID
        {
            get
            {
                return m_ROIs.FindIndex((roi) => roi == SelectedROI);
            }
            set
            {
                SelectedROI = value == -1 ? null : m_ROIs[value];
            }
        }
        [SerializeField]
        private GameObject m_ROIPrefab;

        // generators
        public List<DLL.MRITextureCutGenerator> DLLMRITextureCutGenerators = null;
        public List<DLL.MRIBrainGenerator> DLLBrainTextureGenerators = new List<DLL.MRIBrainGenerator>();

        //public DLL.Texture DLLBrainColorScheme = null;          /**< brain colorscheme dll texture */
        public DLL.Texture DLLCutColorScheme = null;            /**< cut colorscheme dll texture */
        public DLL.Texture DLLCutFMRIColorScheme = null;        /**< cut colorscheme dll texture */
        public List<DLL.Texture> DLLBrainCutTextures = null;    /**< list of cut DLL textures */
        public List<DLL.Texture> DLLGUIBrainCutTextures = null; /**< list of GUI DLL cut textures| */
        //  texture 2D
        //public DLL.Texture DLLBrainMainColor = null;            /**< main color dll texture of the brain mesh */
        public Texture2D BrainColorSchemeTexture = null;        /**< brain colorscheme unity 2D texture  */
        public List<Texture2D> BrainCutTextures = null;         /**< list of cut textures */
        public List<Texture2D> GUIBrainCutTextures = null;      /**< list of GUI cut textures */

            
        // latencies
        public bool SourceDefined { get { return SourceSelectedID != -1; } }
        public int SourceSelectedID = -1; /**< id of the selected source */
        public int CurrentLatencyFile = -1; /**< id of the current latency file */
        #endregion

        #region Events
        /// <summary>
        /// Event called when this column is selected
        /// </summary>
        public GenericEvent<Column3D> OnSelectColumn = new GenericEvent<Column3D>();
        /// <summary>
        /// Event called when a view is moved
        /// </summary>
        public GenericEvent<View3D> OnMoveView = new GenericEvent<View3D>();
        /// <summary>
        /// Event called when changing the number of ROIs of this column
        /// </summary>
        public UnityEvent OnChangeNumberOfROI = new UnityEvent();
        /// <summary>
        /// Event called when changing the number of volume in a ROI of this column
        /// </summary>
        public UnityEvent OnChangeNumberOfVolumeInROI = new UnityEvent();
        /// <summary>
        /// Event called when selecting a ROI
        /// </summary>
        public UnityEvent OnSelectROI = new UnityEvent();
        /// <summary>
        /// Event called when changing the radius of a volume in a ROI
        /// </summary>
        public UnityEvent OnChangeROIVolumeRadius = new UnityEvent();
        /// <summary>
        /// Event called when minimizing a column
        /// </summary>
        public UnityEvent OnChangeMinimizedState = new UnityEvent();
        /// <summary>
        /// Event called when selecting a site
        /// </summary>
        public GenericEvent<Site> OnSelectSite = new GenericEvent<Site>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Base init class of the column
        /// </summary>
        /// <param name="idColumn"></param>
        /// <param name="nbCuts"></param>
        /// <param name="sites"></param>
        /// <param name="plotsGO"></param>
        public virtual void Initialize(int idColumn, int nbCuts, DLL.PatientElectrodesList sites, List<GameObject> sitesPatientParent, List<GameObject> siteList)
        {
            // scene
            Layer = "Column" + idColumn;

            // select ring
            m_SelectRing = gameObject.GetComponentInChildren<SiteRing>();
            m_SelectRing.SetLayer(Layer);
            
            // sites
            UpdateSites(sites, sitesPatientParent, siteList);

            // generators dll
            DLLMRITextureCutGenerators = new List<DLL.MRITextureCutGenerator>(nbCuts);
            for (int ii = 0; ii < nbCuts; ++ii)
            {                
                DLLMRITextureCutGenerators.Add(new DLL.MRITextureCutGenerator());
            }

            // DLL textures
            DLLCutColorScheme = new DLL.Texture();
            DLLCutFMRIColorScheme = new DLL.Texture();
            DLLGUIBrainCutTextures = new List<DLL.Texture>(nbCuts);
            DLLBrainCutTextures = new List<DLL.Texture>(nbCuts);
            for (int ii = 0; ii < nbCuts; ++ii)
            {
                DLLGUIBrainCutTextures.Add(new DLL.Texture());
                DLLBrainCutTextures.Add(new DLL.Texture());
            }

            // textures 2D
            BrainColorSchemeTexture = Texture2Dutility.GenerateColorScheme();
            BrainCutTextures = new List<Texture2D>(nbCuts);
            GUIBrainCutTextures = new List<Texture2D>(nbCuts);
            for (int ii = 0; ii < nbCuts; ++ii)
            {
                BrainCutTextures.Add(Texture2Dutility.GenerateCut());
                GUIBrainCutTextures.Add(Texture2Dutility.GenerateGUI());
            }

            // view
            AddView();

            // update rendering
            IsRenderingUpToDate = false;
        }
        public virtual void UpdateSites(DLL.PatientElectrodesList sites, List<GameObject> sitesPatientParent, List<GameObject> siteList)
        {
            GameObject patientPlotsParent = transform.Find("Sites").gameObject;
            foreach (Transform patientSite in patientPlotsParent.transform)
            {
                Destroy(patientSite.gameObject);
            }

            m_RawElectrodes = new DLL.RawSiteList();
            sites.ExtractRawSiteList(m_RawElectrodes);

            SitesGameObjects = new List<List<List<GameObject>>>(sitesPatientParent.Count);
            Sites = new List<Site>(sites.TotalSitesNumber);
            for (int ii = 0; ii < sitesPatientParent.Count; ++ii)
            {
                // instantiate patient plots
                GameObject patientPlots = Instantiate(sitesPatientParent[ii]);
                patientPlots.transform.SetParent(patientPlotsParent.transform);
                patientPlots.transform.localPosition = Vector3.zero;
                patientPlots.name = sitesPatientParent[ii].name;

                SitesGameObjects.Add(new List<List<GameObject>>(patientPlots.transform.childCount));
                for (int jj = 0; jj < patientPlots.transform.childCount; ++jj)
                {
                    int nbPlots = patientPlots.transform.GetChild(jj).childCount;

                    SitesGameObjects[ii].Add(new List<GameObject>(nbPlots));
                    for (int kk = 0; kk < nbPlots; ++kk)
                    {
                        SitesGameObjects[ii][jj].Add(patientPlots.transform.GetChild(jj).GetChild(kk).gameObject);
                        SitesGameObjects[ii][jj][kk].layer = LayerMask.NameToLayer(Layer);
                        Sites.Add(patientPlots.transform.GetChild(jj).GetChild(kk).gameObject.GetComponent<Site>());

                        int id = Sites.Count - 1;
                        Site baseSite = siteList[id].GetComponent<Site>();
                        Sites[id].Information = baseSite.Information;
                        if (!SiteStateBySiteID.ContainsKey(baseSite.Information.FullID))
                        {
                            SiteStateBySiteID.Add(baseSite.Information.FullID, new SiteState(baseSite.State));
                        }
                        Sites[id].State = SiteStateBySiteID[baseSite.Information.FullID];
                        Sites[id].IsActive = true;
                    }
                }
            }
        }
        /// <summary>
        /// Set the meshes for this column
        /// </summary>
        /// <param name="brainMeshesParent"></param>
        public void InitializeColumnMeshes(GameObject brainMeshesParent)
        {
            m_BrainSurfaceMeshes = new List<GameObject>();
            foreach (Transform meshPart in brainMeshesParent.transform)
            {
                if (meshPart.GetComponent<MeshCollider>() == null || !HBP3DModule.UseSimplifiedMeshes) // if the gameobject does not have mesh collider
                {
                    GameObject brainPart = Instantiate(m_BrainPrefab, m_BrainSurfaceMeshesParent);
                    brainPart.GetComponent<Renderer>().sharedMaterial = meshPart.GetComponent<Renderer>().sharedMaterial;
                    brainPart.name = meshPart.name;
                    brainPart.transform.localPosition = Vector3.zero;
                    brainPart.layer = LayerMask.NameToLayer(Layer);
                    brainPart.GetComponent<MeshFilter>().mesh = Instantiate(meshPart.GetComponent<MeshFilter>().mesh);
                    brainPart.SetActive(true);
                    BrainSurfaceMeshes.Add(brainPart);
                }
            }
        }
        /// <summary>
        /// Update the meshes of this column
        /// </summary>
        /// <param name="brainMeshes"></param>
        public void UpdateColumnMeshes(List<GameObject> brainMeshes)
        {
            for (int i = 0; i < brainMeshes.Count; i++)
            {
                if (brainMeshes[i].GetComponent<MeshCollider>() == null || !HBP3DModule.UseSimplifiedMeshes) // if the gameobject does not have mesh collider
                {
                    DestroyImmediate(m_BrainSurfaceMeshes[i].GetComponent<MeshFilter>().sharedMesh);
                    m_BrainSurfaceMeshes[i].GetComponent<MeshFilter>().sharedMesh = Instantiate(brainMeshes[i].GetComponent<MeshFilter>().mesh);
                }
            }
        }

        public void ChangeMeshesLayer(int layer)
        {
            foreach (var mesh in m_BrainSurfaceMeshes)
            {
                mesh.layer = layer;
            }
        }
        /// <summary>
        ///  Clean all allocated data
        /// </summary>
        public virtual void Clear()
        {
            DLLCutColorScheme.Dispose();
            DLLCutFMRIColorScheme.Dispose();
            Destroy(BrainColorSchemeTexture);

            // DLL
            // generators
            for (int ii = 0; ii < DLLBrainTextureGenerators.Count; ++ii)
            {
                DLLBrainTextureGenerators[ii].Dispose();
            }
            for (int ii = 0; ii < DLLMRITextureCutGenerators.Count; ++ii)
            {
                DLLMRITextureCutGenerators[ii].Dispose();
            }

            // textures 2D
            for (int ii = 0; ii < BrainCutTextures.Count; ++ii)
                Destroy(BrainCutTextures[ii]);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nbSplits"></param>
        public void ResetSplitsNumber(int nbSplits)
        {
            // generators dll
            //      brain
            DLLBrainTextureGenerators = new List<DLL.MRIBrainGenerator>(nbSplits);
            for (int ii = 0; ii < nbSplits; ++ii)
                DLLBrainTextureGenerators.Add(new DLL.MRIBrainGenerator());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nbCuts"></param>
        public virtual void UpdateCutsPlanesNumber(int diffCuts)
        {
            if (diffCuts < 0)
            {
                for (int ii = 0; ii < -diffCuts; ++ii)
                {
                    // GO textures
                    BrainCutTextures.Add(Texture2Dutility.GenerateCut());                    
                    GUIBrainCutTextures.Add(Texture2Dutility.GenerateGUI());

                    // DLL textures
                    DLLBrainCutTextures.Add(new DLL.Texture());
                    DLLGUIBrainCutTextures.Add(new DLL.Texture());

                    // DLL generators
                    DLLMRITextureCutGenerators.Add(new DLL.MRITextureCutGenerator());
                }
            }
            else if (diffCuts > 0)
            {
                for (int ii = 0; ii < diffCuts; ++ii)
                {
                    // GO textures          
                    Destroy(BrainCutTextures[BrainCutTextures.Count - 1]);
                    BrainCutTextures.RemoveAt(BrainCutTextures.Count - 1);
                    Destroy(GUIBrainCutTextures[GUIBrainCutTextures.Count - 1]);
                    GUIBrainCutTextures.RemoveAt(GUIBrainCutTextures.Count - 1);
                    
                    // DLL textures
                    DLLBrainCutTextures.RemoveAt(DLLBrainCutTextures.Count - 1);
                    DLLGUIBrainCutTextures.RemoveAt(DLLGUIBrainCutTextures.Count - 1);

                    // DLL generators
                    DLLMRITextureCutGenerators[DLLMRITextureCutGenerators.Count - 1].Dispose();
                    DLLMRITextureCutGenerators.RemoveAt(DLLMRITextureCutGenerators.Count - 1);
                }
            }

            if (diffCuts != 0)
            {
                IsRenderingUpToDate = false;
            }
        }
        /// <summary>
        /// Set the sites visibility state
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetSitesVisibility(bool isVisible)
        {
            string layer;
            if (isVisible)
                layer = Layer;
            else
                layer = "Inactive";

            for(int ii = 0; ii < Sites.Count; ++ii)
            {
                Sites[ii].gameObject.layer = LayerMask.NameToLayer(layer);
            }
        }
        public virtual void UpdateSitesRendering(SceneStatesInfo data, Latencies latenciesFile)
        {
            UnityEngine.Profiling.Profiler.BeginSample("TEST-updatePlotsRendering");

            Vector3 noScale = new Vector3(0, 0, 0);
            Vector3 normalScale = new Vector3(1, 1, 1);
            MeshRenderer renderer = null;
            SiteType siteType;
            if (data.DisplayCCEPMode) // CCEP
            {
                for (int ii = 0; ii < Sites.Count; ++ii)
                {
                    //MaterialPropertyBlock props = new MaterialPropertyBlock();

                    bool activity = true;
                    bool highlight = Sites[ii].State.IsHighlighted;
                    float customAlpha = -1f;
                    renderer = Sites[ii].GetComponent<MeshRenderer>();

                    if (Sites[ii].State.IsBlackListed) // blacklisted plot
                    {
                        Sites[ii].transform.localScale = noScale;
                        siteType = SiteType.BlackListed;
                    }
                    else if (Sites[ii].State.IsExcluded) // excluded plot
                    {
                        Sites[ii].transform.localScale = normalScale;
                        siteType = SiteType.Excluded;
                    }
                    else if (latenciesFile != null) // latency file available
                    {
                        if (SourceSelectedID == -1) // no source selected
                        {
                            Sites[ii].transform.localScale = normalScale;
                            siteType = latenciesFile.IsSiteASource(ii) ? SiteType.Source : SiteType.NotASource;
                        }
                        else // source selected
                        {
                            if (ii == SourceSelectedID)
                            {
                                Sites[ii].transform.localScale = normalScale;
                                siteType = SiteType.Source;
                            }
                            else if (latenciesFile.IsSiteResponsiveForSource(ii, SourceSelectedID)) // data available
                            {
                                // set color
                                siteType = (latenciesFile.PositiveHeight[SourceSelectedID][ii]) ? SiteType.NonePos : SiteType.NoneNeg;

                                // set transparency
                                customAlpha = latenciesFile.Transparencies[SourceSelectedID][ii] - 0.25f;

                                if (Sites[ii].State.IsHighlighted)
                                    customAlpha = 1;

                                // set size
                                float size = latenciesFile.Sizes[SourceSelectedID][ii];
                                Sites[ii].transform.localScale = new Vector3(size, size, size);
                            }
                            else // no data available
                            {
                                Sites[ii].transform.localScale = normalScale;
                                siteType = SiteType.NoLatencyData;
                            }
                        }
                    }
                    else // no mask and no latency file available : all plots have the same size and color
                    {
                        Sites[ii].transform.localScale = normalScale;
                        siteType = Sites[ii].State.IsMarked ? SiteType.Marked : SiteType.Normal;
                    }

                    // select plot ring 
                    if (ii == SelectedSiteID)
                        m_SelectRing.SetSelectedSite(Sites[ii], Sites[ii].transform.localScale);

                    Material siteMaterial = SharedMaterials.SiteSharedMaterial(highlight, siteType);
                    if (customAlpha > 0f)
                    {
                        Color col = siteMaterial.color;
                        col.a = customAlpha;
                        siteMaterial.color = col;
                    }

                    renderer.sharedMaterial = siteMaterial;
                    Sites[ii].gameObject.SetActive(activity);
                }
            }
            else // iEEG
            {
                UnityEngine.Profiling.Profiler.BeginSample("TEST-updatePlotsRendering -1 ");


                for (int ii = 0; ii < Sites.Count; ++ii)
                {
                    bool activity;
                    if (!Sites[ii].IsInitialized)
                    {
                        Sites[ii].IsInitialized = true;
                        activity = Sites[ii].gameObject.activeSelf;
                    }
                    else
                        activity = Sites[ii].IsActive;


                    if (Sites[ii].State.IsMasked || Sites[ii].State.IsOutOfROI) // column mask : plot is not visible can't be clicked // ROI mask : plot is not visible, can't be clicked
                    {
                        if (activity)
                            Sites[ii].gameObject.SetActive(false);

                        Sites[ii].IsActive = false;
                        continue;
                    }


                    UnityEngine.Profiling.Profiler.BeginSample("TEST-updatePlotsRendering -2 ");

                    if (Sites[ii].State.IsBlackListed) // blacklist mask : plot is barely visible with another color, can be clicked
                    {
                        Sites[ii].transform.localScale = normalScale;
                        siteType = SiteType.BlackListed;
                        if (data.HideBlacklistedSites)
                        {
                            if (activity) Sites[ii].IsActive = false;
                            continue;
                        }
                    }
                    else if (Sites[ii].State.IsExcluded) // excluded mask : plot is a little visible with another color, can be clicked
                    {
                        Sites[ii].transform.localScale = normalScale;
                        siteType = SiteType.Excluded;
                    }
                    else // no mask and no amplitude computed : all plots have the same size and color
                    {
                        Sites[ii].transform.localScale = normalScale;
                        siteType = Sites[ii].State.IsMarked ? SiteType.Marked : SiteType.Normal;
                    }

                    UnityEngine.Profiling.Profiler.EndSample();
                    UnityEngine.Profiling.Profiler.BeginSample("TEST-updatePlotsRendering -3 ");

                    Sites[ii].GetComponent<MeshRenderer>().sharedMaterial = SharedMaterials.SiteSharedMaterial(Sites[ii].State.IsHighlighted, siteType);

                    if (!activity)
                        Sites[ii].gameObject.SetActive(true);

                    Sites[ii].IsActive = true;

                    UnityEngine.Profiling.Profiler.EndSample();
                }

                // select plot ring 
                if (SelectedSiteID >= 0 && SelectedSiteID < Sites.Count)
                    m_SelectRing.SetSelectedSite(Sites[SelectedSiteID], Sites[SelectedSiteID].transform.localScale);


                UnityEngine.Profiling.Profiler.EndSample();
            }

            if (SelectedSiteID == -1)
            {
                m_SelectRing.SetSelectedSite(null, new Vector3(0, 0, 0));
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }
        public void SaveSiteStates(string path)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("ID,Excluded,Blacklisted,Highlighted,Marked");
                    foreach (var site in SiteStateBySiteID)
                    {
                        sw.WriteLine("{0},{1},{2},{3},{4}", site.Key, site.Value.IsExcluded, site.Value.IsBlackListed, site.Value.IsHighlighted, site.Value.IsMarked);
                    }
                }
            }
            catch
            {
                ApplicationState.DialogBoxManager.Open(Tools.Unity.DialogBoxManager.AlertType.Error, "Can not save site states", "Please verify your rights.");
            }
        }
        public void LoadSiteStates(string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    // Find which column of the csv corresponds to which argument
                    string firstLine = sr.ReadLine();
                    string[] firstLineSplits = firstLine.Split(',');
                    int[] indices = new int[5];
                    for (int i = 0; i < 5; ++i)
                    {
                        string split = firstLineSplits[i];
                        indices[i] = split == "ID" ? 0 : split == "Excluded" ? 1 : split == "Blacklisted" ? 2 : split == "Highlighted" ? 3 : split == "Marked" ? 4 : i;
                    }
                    // Fill states
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] args = line.Split(',');
                        SiteState state = new SiteState();
                        bool stateValue;
                        if (bool.TryParse(args[indices[1]], out stateValue))
                        {
                            state.IsExcluded = stateValue;
                        }
                        else
                        {
                            state.IsExcluded = false;
                        }
                        if (bool.TryParse(args[indices[2]], out stateValue))
                        {
                            state.IsBlackListed = stateValue;
                        }
                        else
                        {
                            state.IsBlackListed = false;
                        }
                        if (bool.TryParse(args[indices[3]], out stateValue))
                        {
                            state.IsHighlighted = stateValue;
                        }
                        else
                        {
                            state.IsHighlighted = false;
                        }
                        if (bool.TryParse(args[indices[4]], out stateValue))
                        {
                            state.IsMarked = stateValue;
                        }
                        else
                        {
                            state.IsMarked = false;
                        }
                        SiteState existingState;
                        if (SiteStateBySiteID.TryGetValue(args[indices[0]], out existingState))
                        {
                            existingState.ApplyState(state);
                        }
                        else
                        {
                            SiteStateBySiteID.Add(args[indices[0]], state);
                        }
                    }
                }
            }
            catch
            {
                ApplicationState.DialogBoxManager.Open(Tools.Unity.DialogBoxManager.AlertType.Error, "Can not load site states", "Please verify your files and try again.");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="colormap"></param>
        /// <param name="colorBrainCut"></param>
        public void ResetColorSchemes(ColorType colormap, ColorType colorBrainCut)
        {
            DLLCutColorScheme = DLL.Texture.Generate2DColorTexture(colorBrainCut, colormap); 
            DLLCutFMRIColorScheme = DLL.Texture.Generate2DColorTexture(colorBrainCut, colormap);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometryGenerator"></param>
        /// <param name="volume"></param>
        /// <param name="indexCut"></param>
        /// <param name="MRICalMinFactor"></param>
        /// <param name="MRICalMaxFactor"></param>
        public void CreateMRITexture(DLL.MRIGeometryCutGenerator geometryGenerator, DLL.Volume volume, int indexCut, float MRICalMinFactor, float MRICalMaxFactor)
        {
            UnityEngine.Profiling.Profiler.BeginSample("TEST-Column3DView create_MRI_texture reset 0  ");
            DLL.MRITextureCutGenerator textureGenerator = DLLMRITextureCutGenerators[indexCut];
            textureGenerator.Reset(geometryGenerator);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("TEST-Column3DView create_MRI_texture fill_texture_with_volume 1  ");
            textureGenerator.FillTextureWithVolume(volume, DLLCutColorScheme, MRICalMinFactor, MRICalMaxFactor);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("TEST-Column3DView create_MRI_texture updateTexture 2  ");
            textureGenerator.UpdateTexture(DLLBrainCutTextures[indexCut]);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("TEST-Column3DView create_MRI_texture update_texture_2D 3  ");
            DLLBrainCutTextures[indexCut].UpdateTexture2D(BrainCutTextures[indexCut]); // update mesh cut 2D texture
            UnityEngine.Profiling.Profiler.EndSample();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="orientation"></param>
        /// <param name="flip"></param>
        /// <param name="cutPlanes"></param>
        /// <param name="drawLines"></param>
        public void CreateGUIMRITexture(int indexCut, string orientation, bool flip, List<Cut> cutPlanes, bool drawLines)
        {
            if (DLLBrainCutTextures[indexCut].TextureSize[0] > 0)
            { 
                DLLGUIBrainCutTextures[indexCut].CopyAndRotate(DLLBrainCutTextures[indexCut], orientation, flip, drawLines, indexCut, cutPlanes, DLLMRITextureCutGenerators[indexCut]);
                DLLGUIBrainCutTextures[indexCut].UpdateTexture2D(GUIBrainCutTextures[indexCut]);
            }
        }
        public void ResizeGUIMRITextures()
        {
            int max = 0;
            foreach (var texture in DLLGUIBrainCutTextures)
            {
                int textureMax = texture.TextureSize.Max();
                if (textureMax > max)
                {
                    max = textureMax;
                }
            }
            for (int i = 0; i < DLLGUIBrainCutTextures.Count; ++i)
            {
                DLLGUIBrainCutTextures[i].ResizeToSquare(max);
                DLLGUIBrainCutTextures[i].UpdateTexture2D(GUIBrainCutTextures[i]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void AddView()
        {
            View3D view = Instantiate(ViewPrefab, transform.Find("Views")).GetComponent<View3D>();
            view.gameObject.name = "View " + m_Views.Count;
            view.LineID = m_Views.Count;
            view.Layer = Layer;
            view.OnSelectView.AddListener((selectedView) =>
            {
                foreach (View3D v in m_Views)
                {
                    if (v != selectedView)
                    {
                        v.IsSelected = false;
                    }
                }
                IsSelected = true;
                ApplicationState.Module3D.OnSelectView.Invoke(selectedView);
            });
            view.OnMoveView.AddListener(() =>
            {
                OnMoveView.Invoke(view);
            });
            m_Views.Add(view);
        }
        /// <summary>
        /// 
        /// </summary>
        public void RemoveView(int lineID)
        {
            Destroy(m_Views[lineID].gameObject);
            m_Views.RemoveAt(lineID);
        }

        public ROI AddROI(string name = ROI.DEFAULT_ROI_NAME)
        {
            GameObject roiGameObject = Instantiate(m_ROIPrefab, m_ROIParent);
            ROI roi = roiGameObject.GetComponent<ROI>();
            roi.Name = name;
            roi.OnChangeNumberOfVolumeInROI.AddListener(() =>
            {
                OnChangeNumberOfVolumeInROI.Invoke();
            });
            roi.OnChangeROIVolumeRadius.AddListener(() =>
            {
                OnChangeROIVolumeRadius.Invoke();
            });
            m_ROIs.Add(roi);
            OnChangeNumberOfROI.Invoke();
            SelectedROI = m_ROIs.Last();

            return roi;
        }
        public void CopyROI(ROI roi)
        {
            ROI newROI = AddROI();
            newROI.Name = roi.Name;
            foreach (Sphere bubble in roi.Spheres)
            {
                newROI.AddBubble(Layer, "Bubble", bubble.Position, bubble.Radius);
            }
        }
        public void RemoveSelectedROI()
        {
            Destroy(m_SelectedROI.gameObject);
            m_ROIs.Remove(m_SelectedROI);
            OnChangeNumberOfROI.Invoke();

            if (m_ROIs.Count > 0)
            {
                SelectedROI = m_ROIs.Last();
            }
            else
            {
                SelectedROI = null;
            }
        }

        public void SetCurrentSiteAsSource()
        {
            SourceSelectedID = SelectedSiteID;
        }
        public void UndefineSource()
        {
            SourceSelectedID = -1;
        }
        #endregion
    }
}
