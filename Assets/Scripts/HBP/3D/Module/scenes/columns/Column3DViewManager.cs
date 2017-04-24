﻿/**
 * \file    Column3DViewManager.cs
 * \author  Lance Florian
 * \date    2015
 * \brief   Define Column3DViewManager class
 */
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HBP.Module3D
{
    public class OnChangeSelectedColumn : UnityEngine.Events.UnityEvent<Column3DView> { }
    /// <summary>
    /// A class for managing all the columns data from a specific scene
    /// </summary>
    public class Column3DViewManager : MonoBehaviour
    {
        #region Properties
        // mani (debug)
        public Color[] colorsPlots = null;

        // columns
        public int idSelectedPatient = 0; /**< id of the selected patient for Multi patient scene */

        int m_SelectedColumnID = 0; /**< id of the selected column */
        public int SelectedColumnID 
        {
            get { return m_SelectedColumnID; }
            set
            {
                m_SelectedColumnID = value;
                OnChangeSelectedColumn.Invoke(m_Columns[value]);
            }
        }
        public Column3DView SelectedColumn
        {
            get { return m_Columns[SelectedColumnID]; }
            set
            {
                int index = m_Columns.FindIndex((elmt) => elmt == value);
                if (index != -1) SelectedColumnID = index;
                else Debug.LogError("Column3DViewManager didn't contain this column.");
            }
        }
        public OnChangeSelectedColumn OnChangeSelectedColumn = new OnChangeSelectedColumn();

        List<Column3DView> m_Columns = null;
        public ReadOnlyCollection<Column3DView> Columns { get { return new ReadOnlyCollection<Column3DView>(m_Columns); } }
        public ReadOnlyCollection<Column3DViewIEEG> ColumnsIEEG { get { return new ReadOnlyCollection<Column3DViewIEEG>((from column in m_Columns where column is Column3DViewIEEG select (Column3DViewIEEG)column).ToArray()); } }
        public ReadOnlyCollection<Column3DViewFMRI> ColumnsFMRI { get { return new ReadOnlyCollection<Column3DViewFMRI>((from column in m_Columns where column is Column3DViewFMRI select (Column3DViewFMRI)column).ToArray()); } }

        // plots
        public DLL.RawSiteList DLLLoadedRawPlotsList = null;
        public DLL.PatientElectrodesList DLLLoadedPatientsElectrodes = null;
        public List<GameObject> SitesList = new List<GameObject>();
        public List<GameObject> PlotsPatientParent = new List<GameObject>(); /**< plots patient parents of the scene */
        public List<List<GameObject>> PlotsElectrodesParent = new List<List<GameObject>>(); /**< plots electrodes parents of the scene */

        // latency        
        public bool latencyFilesDefined = false;
        public bool latencyFileAvailable = false; /**< latency file is available */
        public List<Latencies> latenciesFiles = new List<Latencies>(); /*< list of latency files */

        // timelines 
        public bool globalTimeline = true;  /**< is global timeline enabled */
        public float commonTimelineValue = 0f; /**< commmon value of the timelines */

        // textures
        public List<Vector2[]> uvNull = null;                   /**< null uv vectors */ // // new List<Vector2[]>(); 
        public Color notInBrainColor = Color.black;


        public List<DLL.MRIBrainGenerator> DLLCommonBrainTextureGeneratorList = null; /**< common generators for each brain part  */

        // Common columns cut textures 
        //  textures 2D
        public List<Texture2D> rightGUICutTextures = null;                  /**< list of rotated cut textures| */

        //  generator DLL
        public List<DLL.MRIGeometryCutGenerator> DLLMRIGeometryCutGeneratorList = null; /**< ... */        
        

        // niftii 
        public DLL.NIFTI DLLNii = null;
        // surface 
        public int meshSplitNb = 1;
        public DLL.Surface DLLTriErasingMesh = null; // inused
        public DLL.Surface DLLTriErasingPointMesh = null; // inused
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
        public List<Plane> planesCutsCopy = new List<Plane>();  /**< cut planes copied before the cut job */        
        public List<int> idPlanesOrientationList = new List<int>();     /**< id orientation of the cuts planes */
        public List<bool> planesOrientationFlipList = new List<bool>(); /**< flip state of the cuts plantes orientation */

        // UV coordinates
        public List<Vector2[]> UVCoordinatesSplits = null; // uv coordinates for each brain mesh split

        //
        public bool[] commonMask = null;


        // textures
        public int m_idBrainColor = 15;
        public int m_idBrainCutColor = 14;
        public int m_idColormap = 13; // TO move
        public Texture2D brainColorMapTexture = null;
        public Texture2D brainColorTexture = null;


        #endregion members

        #region mono_behaviour

        public void Awake()
        {
            reset(3);
            update_columns_nb(1, 0, 3);
        }

        #endregion mono_behaviour

        #region functions
        /// <summary>
        /// Reset all data.
        /// </summary>
        public void reset(int cutPlanesNb)
        {
            // init DLL objects
            //      nii loader;
            DLLNii = new DLL.NIFTI();

            // surfaces
            DLLTriErasingMesh = new DLL.Surface(); // inused
            DLLTriErasingPointMesh = new DLL.Surface(); // inused
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
            rightGUICutTextures = new List<Texture2D>(cutPlanesNb);

            //  DLL generators
            DLLMRIGeometryCutGeneratorList = new List<DLL.MRIGeometryCutGenerator>(cutPlanesNb);
            for (int ii = 0; ii < cutPlanesNb; ++ii)
            {                
                rightGUICutTextures.Add(new Texture2D(1, 1));                
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
            else m_Columns = new List<Column3DView>();

            brainColorMapTexture = Texture2Dutility.generate_color_scheme();
            brainColorTexture = Texture2Dutility.generate_color_scheme();

            reset_splits_nb(1);
        }

        /// <summary>
        /// Reset the number of meshes splits for the brain
        /// </summary>
        /// <param name="nbSplits"></param>
        public void reset_splits_nb(int nbSplits)
        {
            DLLSplittedMeshesList = new List<DLL.Surface>(meshSplitNb);
            DLLSplittedWhiteMeshesList = new List<DLL.Surface>(meshSplitNb);

            // uv coordinates
            UVCoordinatesSplits = new List<Vector2[]>(Enumerable.Repeat(new Vector2[0], meshSplitNb));

            // brain
            //  generators
            DLLCommonBrainTextureGeneratorList = new List<DLL.MRIBrainGenerator>(meshSplitNb);
            for (int ii = 0; ii < meshSplitNb; ++ii)
                DLLCommonBrainTextureGeneratorList.Add(new DLL.MRIBrainGenerator());

            for (int c = 0; c < m_Columns.Count; c++)
            {
                m_Columns[c].reset_splits_nb(nbSplits);
            }
        }

        public void update_colormap(int idColormap)
        {
            m_idColormap = idColormap;
            DLL.Texture tex = DLL.Texture.generate_1D_color_texture(m_idColormap);
            tex.update_texture_2D(brainColorMapTexture);
        }

        public void update_brain_cut_color(int idBrainCutColor)
        {
            m_idBrainCutColor = idBrainCutColor;
        }

        public void reset_colors()
        {
            for (int ii = 0; ii < m_Columns.Count; ++ii)
                Columns[ii].reset_color_schemes(m_idColormap, m_idBrainCutColor);
        }

        /// <summary>
        /// Update the number of cut planes for every column
        /// </summary>
        /// <param name="nbCuts"></param>
        public void update_cuts_nb(int nbCuts)
        {
            // update common
            int diffCuts = DLLMRIGeometryCutGeneratorList.Count - nbCuts;
            if (diffCuts < 0)
            {
                // textures 2D
                for (int ii = 0; ii < -diffCuts; ++ii)
                {
                    // GO textures        
                    rightGUICutTextures.Add(new Texture2D(1, 1));
                    int id = rightGUICutTextures.Count - 1;
                    rightGUICutTextures[id].filterMode = FilterMode.Trilinear; // TODO : test performances with this parameter
                    rightGUICutTextures[id].wrapMode = TextureWrapMode.Clamp;
                    rightGUICutTextures[id].anisoLevel = 9; // TODO : test performances with this parameter
                    rightGUICutTextures[id].mipMapBias = -2; // never superior to -1 (colorscheme 8 texture glitch)    

                    // DLL generators       
                    DLLMRIGeometryCutGeneratorList.Add(new DLL.MRIGeometryCutGenerator());
                }
            }
            else if (diffCuts > 0)
            {                
                for (int ii = 0; ii < diffCuts; ++ii)
                {
                    // GO textures
                    Destroy(rightGUICutTextures[rightGUICutTextures.Count - 1]);
                    rightGUICutTextures.RemoveAt(rightGUICutTextures.Count - 1);

                    // DLL generators
                    DLLMRIGeometryCutGeneratorList[DLLMRIGeometryCutGeneratorList.Count - 1].Dispose();
                    DLLMRIGeometryCutGeneratorList.RemoveAt(DLLMRIGeometryCutGeneratorList.Count - 1);
                }
            }

            // update columns
            for (int c = 0; c < m_Columns.Count; c++)
            {
                m_Columns[c].update_cuts_planes_nb(nbCuts);
            }
        }

        /// <summary>
        /// Update the number of columns and set the number of cut planes for each ones
        /// </summary>
        /// <param name="nbIEEGColumns"></param>
        /// /// <param name="nbIRMFColumns"></param>
        /// <param name="nbCuts"></param>
        public void update_columns_nb(int nbIEEGColumns, int nbIRMFColumns, int nbCuts)
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
                    // add column
                    columnsIeegGo.Add(new GameObject("column IEEG " + columnsIeegGo.Count + 1));
                    columnsIeegGo[columnsIeegGo.Count - 1].AddComponent<Column3DViewIEEG>();
                    columnsIeegGo[columnsIeegGo.Count - 1].transform.SetParent(this.transform);
                }
            }
            else if (diffIEEGColumns > 0)
            {
                for (int ii = 0; ii < diffIEEGColumns; ++ii)
                {
                    // destroy column
                    int idColumn = columnsIeegGo.Count - 1;
                    Destroy(columnsIeegGo[idColumn]);
                    columnsIeegGo.RemoveAt(idColumn);
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

                    columnsFmriGo.Add(new GameObject("column FMRI " + columnsFmriGo.Count + 1));
                    columnsFmriGo[columnsFmriGo.Count - 1].AddComponent<Column3DViewFMRI>();
                    columnsFmriGo[columnsFmriGo.Count - 1].transform.SetParent(this.transform);
                }
            }
            else if (diffIRMFColumns > 0)
            {
                for (int ii = 0; ii < diffIRMFColumns; ++ii)
                {
                    // destroy column
                    int idColumn = columnsFmriGo.Count - 1;
                    Destroy(columnsFmriGo[idColumn]);
                    columnsFmriGo.RemoveAt(idColumn);

                    DLLVolumeFMriList[idColumn].Dispose();
                    DLLVolumeFMriList.RemoveAt(idColumn);
                }
            }

            // init new columns IEEG            
            if (nbIEEGColumns != columnsIEEG.Count)
            {
                columnsIEEG = new List<Column3DViewIEEG>();
                for (int ii = 0; ii < nbIEEGColumns; ++ii)
                {
                    columnsIEEG.Add(columnsIeegGo[ii].GetComponent<Column3DViewIEEG>());
                    columnsIEEG[ii].init(ii, nbCuts, DLLLoadedPatientsElectrodes, PlotsPatientParent);
                    columnsIEEG[ii].reset_splits_nb(meshSplitNb);

                    if (latencyFilesDefined)
                        columnsIEEG[ii].currentLatencyFile = 0;
                }
            }

            // init new columns IRMF
            if (nbIRMFColumns != ColumnsFMRI.Count)
            {
                // update IRMF columns mask
                bool[] maskColumnsOR = new bool[DLLLoadedPatientsElectrodes.total_sites_nb()];
                for (int ii = 0; ii < SitesList.Count; ++ii)
                {
                    bool mask = false;

                    for (int jj = 0; jj < columnsIEEG.Count; ++jj)
                    {
                        mask = mask || columnsIEEG[jj].Sites[ii].Information.IsMasked;
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

                olumnsFMRI = new List<Column3DViewFMRI>();
                for (int ii = 0; ii < nbIRMFColumns; ++ii)
                {
                    ColumnsFMRI.Add(ColumnsFMRI[ii].GetComponent<Column3DViewFMRI>());
                    ColumnsFMRI[ColumnsFMRI.Count - 1].Initialize(ColumnsIEEG.Count + ii, nbCuts, DLLLoadedPatientsElectrodes, PlotsPatientParent);
                    ColumnsFMRI[ColumnsFMRI.Count - 1].reset_splits_nb(meshSplitNb);

                    for (int jj = 0; jj < SitesList.Count; ++jj)
                    {
                        ColumnsFMRI[ColumnsFMRI.Count - 1].Sites[jj].Information.IsMasked = maskColumnsOR[jj];
                    }
                }
            }


            commonMask = new bool[DLLLoadedPatientsElectrodes.total_sites_nb()];

            if (SelectedColumnID >= m_Columns.Count)
                SelectedColumnID = m_Columns.Count - 1;


            for (int ii = 0; ii < m_Columns.Count; ++ii)
                Columns[ii].reset_color_schemes(m_idColormap, m_idBrainCutColor);
        }

        /// <summary>
        /// Define the single patient and associated data
        /// </summary>
        /// <param name="patient"></param>
        /// <param name="columnDataList"></param>
        public void set_SP_timeline_data(Data.Patient patient, List<Data.Visualisation.ColumnData> columnDataList)
        {
            spPatient = patient;
            for (int c = 0; c < ColumnsIEEG.Count; c++)
            {
                ColumnsIEEG[c].set_column_data(columnDataList[c]);
            }
        }


        /// <summary>
        /// Define the mp patients list and associated data
        /// </summary>
        /// <param name="patientList"></param>
        /// <param name="columnDataList"></param>
        /// <param name="ptsPathFileList"></param>
        public void set_MP_timeline_data(List<Data.Patient> patientList, List<Data.Visualisation.ColumnData> columnDataList, List<string> ptsPathFileList)
        {
            mpPatients = patientList;
            for (int c = 0; c < ColumnsIEEG.Count; c++)
            {
                ColumnsIEEG[c].set_column_data(columnDataList[c]);
            }
        }


        /// <summary>
        /// Create the cut mesh texture dll and texture2D
        /// </summary>
        /// <param name="indexCut"></param>
        public void create_MRI_texture(int indexCut, int indexColumn)
        {
            UnityEngine.Profiling.Profiler.BeginSample("create_MRI_texture");
                Columns[indexColumn].create_MRI_texture(DLLMRIGeometryCutGeneratorList[indexCut], DLLVolume, indexCut, MRICalMinFactor, MRICalMaxFactor);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="indexColumn"></param>
        public void create_GUI_MRI_texture(int indexCut, int indexColumn)
        {
            // retrieve orientation to apply
            string orientation = "custom";
            if (idPlanesOrientationList[indexCut] == 0)
                orientation = "Axial";
            else if (idPlanesOrientationList[indexCut] == 1)
                orientation = "Coronal";
            else if (idPlanesOrientationList[indexCut] == 2)
                orientation = "Sagital";

            Columns[indexColumn].create_GUI_MRI_texture(indexCut, orientation, planesOrientationFlipList[indexCut], planesCutsCopy, orientation != "custom");            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="indexColumn"></param>
        public void create_GUI_IEEG_texture(int indexCut, int indexColumn)
        {
            // retrieve orientation to apply
            string orientation = "custom";
            if (idPlanesOrientationList[indexCut] == 0)
                orientation = "Axial";
            else if (idPlanesOrientationList[indexCut] == 1)
                orientation = "Coronal";
            else if (idPlanesOrientationList[indexCut] == 2)
                orientation = "Sagital";

            ((Column3DViewIEEG)Columns[indexColumn]).create_GUI_IEEG_texture(indexCut, orientation, planesOrientationFlipList[indexCut], planesCutsCopy, orientation != "custom");
        }


        public void create_GUI_FMRI_texture(int indexCut, int indexColumn)
        {
            // retrieve orientation to apply
            string orientation = "custom";
            if (idPlanesOrientationList[indexCut] == 0)
                orientation = "Axial";
            else if (idPlanesOrientationList[indexCut] == 1)
                orientation = "Coronal";
            else if (idPlanesOrientationList[indexCut] == 2)
                orientation = "Sagital";

            ((Column3DViewFMRI)Columns[indexColumn]).create_GUI_FMRI_texture(indexCut, orientation, planesOrientationFlipList[indexCut], planesCutsCopy, orientation != "custom");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexColumn"></param>
        /// <param name="indexCut"></param>
        /// <param name="thresholdInfluence"></param>
        public void color_cuts_textures_with_IEEG(int indexColumn, int indexCut)
        {
            Column3DViewIEEG column = ColumnsIEEG[indexColumn];            
            DLL.MRITextureCutGenerator generator = column.DLLMRITextureCutGeneratorList[indexCut];        
            generator.fill_texture_with_IEEG(column, column.DLLCutColorScheme, notInBrainColor);

            DLL.Texture cutTexture = column.dllBrainCutWithIEEGTextures[indexCut];
            generator.update_texture_with_IEEG(cutTexture);
            cutTexture.update_texture_2D(column.brainCutWithIEEGTextures[indexCut]); // update mesh cut 2D texture
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexColumn"></param>
        /// <param name="indexCut"></param>
        public void color_cuts_textures_with_FMRI(int indexColumn, int indexCut)
        {
            Column3DViewFMRI column = ColumnsFMRI[indexColumn];
            DLL.MRITextureCutGenerator generator = column.DLLMRITextureCutGeneratorList[indexCut];
            generator.fill_texture_with_FMRI(column, DLLVolumeFMriList[indexColumn]);

            DLL.Texture cutTexture = column.dllBrainCutWithFMRITextures[indexCut];
            generator.update_texture_with_FMRI(cutTexture);
            cutTexture.update_texture_2D(column.brainCutWithFMRITextures[indexCut]); // update mesh cut 2D texture
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
        public bool compute_surface_brain_UV_with_IEEG(bool whiteInflatedMeshes, int indexColumn)
        {
            for (int ii = 0; ii < meshSplitNb; ++ii)
                if(!ColumnsIEEG[indexColumn].DLLBrainTextureGeneratorList[ii].compute_surface_UV_IEEG(whiteInflatedMeshes ? DLLSplittedWhiteMeshesList[ii] : DLLSplittedMeshesList[ii], ColumnsIEEG[indexColumn]))
                    return false;

            return true;
        }

        /// <summary>
        /// Update the plot rendering parameters for all columns
        /// </summary>
        public void update_all_columns_sites_rendering(SceneStatesInfo data)
        {
            for (int ii = 0; ii < ColumnsIEEG.Count; ++ii)
            {
                Latencies latencyFile = null;
                if (ColumnsIEEG[ii].currentLatencyFile != -1)
                    latencyFile = latenciesFiles[ColumnsIEEG[ii].currentLatencyFile];

                ColumnsIEEG[ii].update_sites_size_and_color_arrays_for_IEEG(); // TEST
                ColumnsIEEG[ii].update_sites_rendering(data, latencyFile);
            }

            for (int ii = 0; ii < ColumnsFMRI.Count; ++ii)
                ColumnsFMRI[ii].update_plots_visiblity(data);
        }

        /// <summary>
        /// Update the visiblity of the ROI for all columns
        /// </summary>
        /// <param name="visible"></param>
        public void update_ROI_visibility(bool visible)
        {
            // disable all ROI render
            for(int ii = 0; ii < m_Columns.Count; ++ii)
                if (m_Columns[ii].SelectedROI != null)
                    m_Columns[ii].SelectedROI.set_rendering_state(false);

            if(SelectedColumn != null)
                if(SelectedColumn.SelectedROI != null)
                SelectedColumn.SelectedROI.set_rendering_state(visible);
        }

        /// <summary>
        /// Update the visiblity of the plots for all columns
        /// </summary>
        /// <param name="visible"></param>
        public void update_sites_visibiliy(bool visible)
        {
            foreach (var column in m_Columns) column.set_visible_sites(visible);
        }


        void AddIEEGColumn()
        {

        }
        void AddFMRIColumn()
        {

        }
        #endregion functions
    }
}