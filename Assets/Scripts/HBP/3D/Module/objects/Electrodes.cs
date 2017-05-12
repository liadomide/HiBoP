﻿
/**
 * \file    Electrodes.cs
 * \author  Lance Florian
 * \date    2015
 * \brief   Define Latencies, MarsAtlasIndex, RawSiteList and PatientElectrodesList classes
 */

// system
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// unity
using UnityEngine;

namespace HBP.Module3D
{
    /// <summary>
    /// CCEP latencies
    /// </summary>
    public class Latencies
    {
        public Latencies(int nbPlots, int[] latencies1D, float[] heights1D)
        {
            stimulationPlots = new bool[nbPlots];
            latencies = new int[nbPlots][];
            heights = new float[nbPlots][];
            transparencies = new float[nbPlots][];
            sizes = new float[nbPlots][];
            positiveHeight = new bool[nbPlots][];
            
            int id = 0;
            for (int ii = 0; ii < nbPlots; ++ii)
            {
                latencies[ii] = new int[nbPlots];
                heights[ii] = new float[nbPlots];
                transparencies[ii] = new float[nbPlots];
                sizes[ii] = new float[nbPlots];
                positiveHeight[ii] = new bool[nbPlots];

                int maxLatency = 0;
                float minHeight = float.MaxValue;
                float maxHeight = float.MinValue;
                for (int jj = 0; jj < nbPlots; ++jj, ++id)
                {
                    latencies[ii][jj] = latencies1D[id];
                    heights[ii][jj] = heights1D[id];

                    if (latencies1D[id] == 0)
                    {
                        stimulationPlots[ii] = true;
                    }
                    else if (latencies1D[id] != -1)
                    {
                        // update max latency for current source plot
                        if (maxLatency < latencies1D[id])
                            maxLatency = latencies1D[id];

                        // update positive height state array
                        positiveHeight[ii][jj] = (heights1D[id] >= 0);

                        // update min/max heights
                        if (heights1D[id] < minHeight)
                            minHeight = heights1D[id];

                        if (heights1D[id] > maxHeight)
                            maxHeight = heights1D[id];
                    }                    
                }

                float max;

                
                if (Math.Abs(minHeight) > Math.Abs(maxHeight)) {
                    max = Math.Abs(minHeight);
                }
                else {
                    max = Math.Abs(maxHeight);
                }

                // now computes transparencies and sizes values 
                for (int jj = 0; jj < nbPlots; ++jj)
                {
                    if (latencies[ii][jj] != 0 && latencies[ii][jj] != -1) {
                        transparencies[ii][jj] = 1f - (0.9f * latencies[ii][jj] / maxLatency);
                        sizes[ii][jj] = Math.Abs(heights[ii][jj]) / max;
                    }                    
                }
            }
        }

        public bool is_size_a_source(int idSite)
        {
            return stimulationPlots[idSite];
        }

        public bool is_site_responsive_for_source(int idSiteToTest, int idSourceSite)
        {
            return (latencies[idSourceSite][idSiteToTest] != -1 && latencies[idSourceSite][idSiteToTest] != 0);
        }

        public List<int> responsive_site_id(int idSourceSite)
        {
            if(!is_size_a_source(idSourceSite))
            {
                Debug.LogError("-ERROR : not a source site.");
                return new List<int>();
            }

            List<int> responsiveSites = new List<int>(latencies.Length);
            for(int ii = 0; ii < latencies.Length; ++ii)
            {
                int latency = latencies[idSourceSite][ii];
                if (latency != -1 && latency != 0)
                {
                    responsiveSites.Add(ii);
                }
            }
            return responsiveSites;
        }

        public string name;

        bool[] stimulationPlots = null;
        public int[][] latencies = null;
        public float[][] heights = null;

        public float[][] transparencies = null; // for latency
        public float[][] sizes = null;          // for height
        public bool[][] positiveHeight = null;  // for height
    }


    namespace DLL
    {
        /// <summary>
        /// Mars atlas index, used to identify sites mars IDs
        /// </summary>
        public class MarsAtlasIndex : CppDLLImportBase
        {
            #region functions

            public bool load_mars_atlas_index_file(string pathFile)
            {
                return load_MarsAtlasIndex(_handle, pathFile) == 1;
            }

            public string hemisphere(int label)
            {
                int length = 3;
                StringBuilder str = new StringBuilder();
                str.Append('?', length);
                hemisphere_MarsAtlasIndex(_handle, label, str, length);
                return str.ToString().Replace("?", string.Empty);
            }

            public string lobe(int label)
            {
                int length = 15;
                StringBuilder str = new StringBuilder();
                str.Append('?', length);
                lobe_MarsAtlasIndex(_handle, label, str, length);
                return str.ToString().Replace("?", string.Empty);
            }

            public string name_FS(int label)
            {
                int length = 30;
                StringBuilder str = new StringBuilder();
                str.Append('?', length);
                nameFS_MarsAtlasIndex(_handle, label, str, length);
                return str.ToString().Replace("?", string.Empty);
            }

            public string name(int label)
            {
                int length = 10;
                StringBuilder str = new StringBuilder();
                str.Append('?', length);
                name_MarsAtlasIndex(_handle, label, str, length);
                return str.ToString().Replace("?", string.Empty);
            }

            public string full_name(int label)
            {
                int length = 50;
                StringBuilder str = new StringBuilder();
                str.Append('?', length);
                fullName_MarsAtlasIndex(_handle, label, str, length);

                return str.ToString().Replace("?", string.Empty);
            }


            public string broadman_area(int label)
            {
                int length = 100;
                StringBuilder str = new StringBuilder();
                str.Append('?', length);
                BA_MarsAtlasIndex(_handle, label, str, length);
                return str.ToString().Replace("?", string.Empty);
            }

            #endregion functions

            #region memory_management

            /// <summary>
            /// Allocate DLL memory
            /// </summary>
            protected override void create_DLL_class()
            {
                _handle = new HandleRef(this, create_MarsAtlasIndex());
            }

            /// <summary>
            /// Clean DLL memory
            /// </summary>
            protected override void delete_DLL_class()
            {
                delete_MarsAtlasIndex(_handle);
            }

            #endregion memory_management

            #region DLLImport

            [DllImport("hbp_export", EntryPoint = "create_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr create_MarsAtlasIndex();
            [DllImport("hbp_export", EntryPoint = "delete_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern void delete_MarsAtlasIndex(HandleRef marsAtlasIndex);
            // load
            [DllImport("hbp_export", EntryPoint = "load_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern int load_MarsAtlasIndex(HandleRef marsAtlasIndex, string pathFile);
            // retrieve data
            [DllImport("hbp_export", EntryPoint = "hemisphere_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern void hemisphere_MarsAtlasIndex(HandleRef marsAtlasIndex, int label, StringBuilder hemisphere, int length);
            [DllImport("hbp_export", EntryPoint = "lobe_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern void lobe_MarsAtlasIndex(HandleRef marsAtlasIndex, int label, StringBuilder lobe, int length);
            [DllImport("hbp_export", EntryPoint = "nameFS_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern void nameFS_MarsAtlasIndex(HandleRef marsAtlasIndex, int label, StringBuilder nameFS, int length);
            [DllImport("hbp_export", EntryPoint = "name_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern void name_MarsAtlasIndex(HandleRef marsAtlasIndex, int label, StringBuilder name, int length);
            [DllImport("hbp_export", EntryPoint = "fullName_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern void fullName_MarsAtlasIndex(HandleRef marsAtlasIndex, int label, StringBuilder fullName, int length);
            [DllImport("hbp_export", EntryPoint = "BA_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern void BA_MarsAtlasIndex(HandleRef marsAtlasIndex, int label, StringBuilder BA, int length);

            #endregion DLLImport

        }

        /// <summary>
        ///  A raw version of PatientElectrodesList, means to be used in the C++ DLL
        /// </summary>
        public class RawSiteList : CppDLLImportBase
        {
            #region functions

            /// <summary>
            /// Save the raw plot list to an obj file
            /// </summary>
            /// <param name="pathObjNameFile"></param>
            /// <returns>true if success else false </returns>
            public bool save_to_obj(string pathObjNameFile)
            {
                bool success = saveToObj_RawPlotList(_handle, pathObjNameFile) == 1;
                StaticComponents.DLLDebugManager.check_error();
                return success;
            }

            public int sites_nb()
            {
                return sites_nb_RawSiteList(_handle);
            }

            /// <summary>
            /// Update the mask of the site corresponding to the input id
            /// </summary>
            /// <param name="idSite"></param>
            /// <param name="mask"></param>
            public void update_mask(int idSite, bool mask)
            {
                update_mask_RawSiteList(_handle, idSite, mask ? 1 : 0);
            }

            public Latencies update_latencies_with_file(string latencyFilePath)
            {
                int nbPlots = sites_nb();
                int[] latencies = new int[nbPlots * nbPlots];
                float[] heights = new float[nbPlots * nbPlots];

                bool success = update_latencies_with_file_RawSiteList(_handle, latencyFilePath, latencies, heights) == 1;                
                if (!success)
                    return null;

                Latencies PatientLatencies = new Latencies(nbPlots, latencies, heights);
                return PatientLatencies;
            }

            /// <summary>
            /// Generate dummy latencies for debug purposes
            /// </summary>
            /// <returns></returns>
            public Latencies generate_dummy_latencies()
            {
                int nbPlots = sites_nb();
                int[] latencies = new int[nbPlots * nbPlots];
                float[] heights = new float[nbPlots * nbPlots];

                update_with_dummy_latencies_RawSiteList(_handle, latencies, heights);

                Latencies PatientLatencies = new Latencies(nbPlots, latencies, heights);
                return PatientLatencies;
            }

            #endregion functions

            #region memory_management

            /// <summary>
            /// Allocate DLL memory
            /// </summary>
            protected override void create_DLL_class()
            {
                _handle = new HandleRef(this, create_RawSiteList());
            }

            /// <summary>
            /// Clean DLL memory
            /// </summary>
            protected override void delete_DLL_class()
            {
                delete_RawSiteList(_handle);
            }

            #endregion memory_management

            #region DLLImport

            //  memory management
            [DllImport("hbp_export", EntryPoint = "create_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr create_RawSiteList();

            [DllImport("hbp_export", EntryPoint = "delete_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void delete_RawSiteList(HandleRef handleRawSiteLst);

            // actions
            [DllImport("hbp_export", EntryPoint = "update_mask_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void update_mask_RawSiteList(HandleRef handleRawSiteLst, int plotId, int mask);

            [DllImport("hbp_export", EntryPoint = "update_latencies_with_file_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int update_latencies_with_file_RawSiteList(HandleRef handleRawSiteLst, string pathLatencyFile, int[] latencies, float[] heights);

            [DllImport("hbp_export", EntryPoint = "update_with_dummy_latencies_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void update_with_dummy_latencies_RawSiteList(HandleRef handleRawSiteLst, int[] latencies, float[] heights);

            // save
            [DllImport("hbp_export", EntryPoint = "saveToObj_RawPlotList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int saveToObj_RawPlotList(HandleRef handleRawSiteLst, string pathObjNameFile);

            // retrieve data
            [DllImport("hbp_export", EntryPoint = "sites_nb_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int sites_nb_RawSiteList(HandleRef handleRawSiteLst);

            //  memory management
            //delegate IntPtr create_RawPlotList();
            //delegate void delete_RawPlotList(HandleRef handleRawPlotLst);
            //// actions
            //delegate void updateMask_RawPlotList(HandleRef handleRawPlotLst, int plotId, int mask);
            //delegate int updateLatenciesWithFile_RawPlotList(HandleRef handleRawPlotLst, string pathLatencyFile, int[] latencies, float[] heights);
            //delegate void updateWithDummyLatencies_RawPlotList(HandleRef handleRawPlotLst, int[] latencies, float[] heights);
            //// save
            //delegate int saveToObj_RawPlotList(HandleRef handleRawPlotLst, string pathObjNameFile);
            //// load
            //delegate int loadColors_RawPlotList(string pathColorFile, float[] r, float[] g, float[] b);
            //// retrieve data
            //delegate int getNumberPlot_RawPlotList(HandleRef handleRawPlotLst);

            #endregion DLLImport
        }

        /// <summary>
        ///  A PatientElectrodes container, for managing severals patients, more useful for data visualization, means to be used with unity scripts
        /// </summary>
        public class PatientElectrodesList : CppDLLImportBase, ICloneable
        {
            #region functions

            /// <summary>
            /// Load a list of pts files add fill ElectrodesPatientMultiList with data
            /// </summary>
            /// <param name="ptsFilesPath"></param>
            /// <returns> true if sucess else false</returns>
            public bool load_pts_files(List<string> ptsFilesPath, List<string> names, MarsAtlasIndex marsAtlasIndex)
            {
                string ptsFilesPathStr = "", namesStr = "";
                for (int ii = 0; ii < ptsFilesPath.Count; ++ii)
                {
                    ptsFilesPathStr += ptsFilesPath[ii];

                    if (ii < ptsFilesPath.Count - 1)
                        ptsFilesPathStr += '?';
                }

                for (int ii = 0; ii < names.Count; ++ii)
                {
                    namesStr += names[ii];

                    if (ii < names.Count - 1)
                        namesStr += '?';
                }

                // load in the DLL
                bool fileLoaded = load_Pts_files_PatientElectrodesList(_handle, ptsFilesPathStr, namesStr, marsAtlasIndex.getHandle()) == 1;
                StaticComponents.DLLDebugManager.check_error();

                if (!fileLoaded)
                {
                    Debug.LogError("PatientElectrodesList : Error during loading. " + ptsFilesPathStr);
                    return false;
                }

                return true;
            }

            public int total_sites_nb()
            {
                return sites_nb_PatientElectrodesList(_handle);
            }

            public int patients_nb()
            {
                return patients_nb_PatientElectrodesList(_handle);
            }

            /// <summary>
            /// Return the number of electrode for the input patient id 
            /// </summary>
            /// <param name="patientId"></param>
            /// <returns></returns>
            public int electrodes_nb(int patientId)
            {
                return electrodes_nb_PatientElectrodesList(_handle, patientId);
            }

            /// <summary>
            /// Return the eletrode number of plots for the input patient id
            /// </summary>
            /// <param name="patientId"></param>
            /// <param name="electrodeId"></param>
            /// <returns></returns>
            public int electrode_sites_nb(int patientId, int electrodeId)
            { 
                return electrode_sites_nb_PatientElectrodesList(_handle, patientId, electrodeId);
            }

            /// <summary>
            /// Return the patient number of sites.
            /// </summary>
            /// <param name="patientId"></param>
            /// <returns></returns>
            public int patient_sites_nb(int patientId)
            {
                return patient_sites_nb_PatientElectrodesList(_handle, patientId);
            }

            /// <summary>
            /// Set the new state for a patient mask 
            /// </summary>
            /// <param name="state"></param>
            /// <param name="patientId"></param>
            public void set_patient_mask(bool state, int patientId)
            {
                set_mask_patient_PatientElectrodesList(_handle, state ? 1 : 0, patientId);
            }

            /// <summary>
            /// Set the new state for an electrode mask 
            /// </summary>
            /// <param name="state"></param>
            /// <param name="patientId"></param>
            /// <param name="electrodeId"></param>
            public void set_electrode_mask(bool state, int patientId, int electrodeId)
            {
                set_mask_electrode_PatientElectrodesList(_handle, state ? 1 : 0, patientId, electrodeId);
            }

            /// <summary>
            /// Set the new state for a site mask 
            /// </summary>
            /// <param name="state"></param>
            /// <param name="patientId"></param>
            /// <param name="electrodeId"></param>
            /// <param name="siteId"></param>
            public void set_site_mask(bool state, int patientId, int electrodeId, int siteId)
            {                
                set_mask_site_PatientElectrodesList(_handle, state ? 1 : 0, patientId, electrodeId, siteId);
            }

            /// <summary>
            /// Return the site mask value
            /// </summary>
            /// <param name="patientId"></param>
            /// <param name="electrodeId"></param>
            /// <param name="siteId"></param>
            /// <returns></returns>
            public bool site_mask(int patientId, int electrodeId, int siteId)
            {
                return site_mask_PatientElectrodesList(_handle, patientId, electrodeId, siteId) == 1;
            }

            /// <summary>
            /// Return the site position
            /// </summary>
            /// <param name="patientId"></param>
            /// <param name="electrodeId"></param>
            /// <param name="siteId"></param>
            /// <returns></returns>
            public Vector3 site_pos(int patientId, int electrodeId, int siteId)
            {
                float[] pos = new float[3];                
                site_pos_PatientElectrodesList(_handle, patientId, electrodeId, siteId, pos);
                return new Vector3(pos[0], pos[1], pos[2]);
            }


            /// <summary>
            /// Return the site name (electrode name + plot id )
            /// </summary>
            /// <param name="patientId"></param>
            /// <param name="electrodeId"></param>
            /// <param name="siteId"></param>
            /// <returns></returns>
            public string site_name(int patientId, int electrodeId, int siteId)
            {
                int length = 8;
                StringBuilder str = new StringBuilder();
                str.Append('?', length);
                site_name_PatientElectrodesList(_handle, patientId, electrodeId, siteId, str, length);

                return str.ToString().Replace("?", string.Empty);
            }

            /// <summary>
            /// Reset the input raw site list with PatientElectrodesList data
            /// </summary>
            /// <param name="rawPlotList"></param>
            public void extract_raw_site_list(RawSiteList rawSiteList)
            {
                extract_raw_site_list_PatientElectrodesList(_handle, rawSiteList.getHandle());
            }

            /// <summary>
            /// Update the mask of the input rawSiteList
            /// </summary>
            /// <param name="rawSiteList"></param>
            public void update_raw_site_list_mask(RawSiteList rawSiteList)
            {
                update_raw_site_list_mask_PatientElectrodesList(_handle, rawSiteList.getHandle());
            }

            /// <summary>
            /// Return the patient name
            /// </summary>
            /// <param name="patientId"></param>
            /// <returns></returns>
            public string patient_name(int patientId)
            {
                int length = 30;
                StringBuilder str = new StringBuilder();
                str.Append('?', length);
                patient_name_PatientElectrodesList(_handle, patientId, str, length);

                return str.ToString().Replace("?", string.Empty);
            }

            /// <summary>
            /// Return the electrode name
            /// </summary>
            /// <param name="patientId"></param>
            /// <param name="electrodeId"></param>
            /// <returns></returns>
            public string electrode_name(int patientId, int electrodeId)
            {
                int length = 30;
                StringBuilder str = new StringBuilder();
                str.Append('?', length);
                electrode_name_PatientElectrodesList(_handle, patientId, electrodeId, str, length);

                return str.ToString().Replace("?", string.Empty);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="patientId"></param>
            /// <param name="electrodeId"></param>
            /// <param name="siteId"></param>
            /// <returns></returns>
            public int site_mars_atlas_label(int patientId, int electrodeId, int siteId)
            {
                return site_mars_atlas_label_PatientElectrodesList(_handle, patientId, electrodeId, siteId);
            }

            #endregion functions        

            #region memory_management

            /// <summary>
            /// ElectrodesPatientMultiList default constructor
            /// </summary>
            public PatientElectrodesList() : base() { }

            /// <summary>
            /// ElectrodesPatientMultiList constructor with an already allocated dll ElectrodesPatientMultiList
            /// </summary>
            /// <param name="electrodesPatientMultiListHandle"></param>
            private PatientElectrodesList(IntPtr electrodesPatientMultiListHandle) : base(electrodesPatientMultiListHandle) { }

            /// <summary>
            /// ElectrodesPatientMultiList_dll copy constructor
            /// </summary>
            /// <param name="other"></param>
            public PatientElectrodesList(PatientElectrodesList other) : base(clone_PatientElectrodesList(other.getHandle())) { }

            /// <summary>
            /// Clone the surface
            /// </summary>
            /// <returns></returns>
            public object Clone()
            {
                return new PatientElectrodesList(this);
            }

            /// <summary>
            /// Allocate DLL memory
            /// </summary>
            protected override void create_DLL_class()
            {
                _handle = new HandleRef(this, create_PatientElectrodesList());
            }

            /// <summary>
            /// Clean DLL memory
            /// </summary>
            protected override void delete_DLL_class()
            {
                delete_PatientElectrodesList(_handle);
            }

            #endregion memory_management

            #region DLLImport

            // memory management
            [DllImport("hbp_export", EntryPoint = "create_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr create_PatientElectrodesList();

            [DllImport("hbp_export", EntryPoint = "delete_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void delete_PatientElectrodesList(HandleRef handlePatientElectrodesList);

            // memory management
            [DllImport("hbp_export", EntryPoint = "clone_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr clone_PatientElectrodesList(HandleRef handlePatientElectrodesListToBeCloned);

            // load
            [DllImport("hbp_export", EntryPoint = "load_Pts_files_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int load_Pts_files_PatientElectrodesList(HandleRef handlePatientElectrodesList, string pathFiles, string names, HandleRef handleMarsAtlasIndex);

            // actions          
            [DllImport("hbp_export", EntryPoint = "extract_raw_site_list_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void extract_raw_site_list_PatientElectrodesList(HandleRef handlePatientElectrodesList, HandleRef handleRawSiteList);

            [DllImport("hbp_export", EntryPoint = "set_mask_patient_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void set_mask_patient_PatientElectrodesList(HandleRef handlePatientElectrodesList, int state, int patientId);

            [DllImport("hbp_export", EntryPoint = "set_mask_electrode_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void set_mask_electrode_PatientElectrodesList(HandleRef handlePatientElectrodesList, int state, int patientId, int electrodeId);

            [DllImport("hbp_export", EntryPoint = "set_mask_site_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void set_mask_site_PatientElectrodesList(HandleRef handlePatientElectrodesList, int state, int patientId, int electrodeId, int siteId);

            [DllImport("hbp_export", EntryPoint = "update_raw_site_list_mask_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void update_raw_site_list_mask_PatientElectrodesList(HandleRef handlePatientElectrodesList, HandleRef handleRawSiteList);

            [DllImport("hbp_export", EntryPoint = "update_all_sites_mask_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void update_all_sites_mask_PatientElectrodesList(HandleRef handlePatientElectrodesList, int[] maskArray);

            // retrieve data
            [DllImport("hbp_export", EntryPoint = "site_name_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int site_name_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, int electrodeId, int siteId, StringBuilder name, int length);

            [DllImport("hbp_export", EntryPoint = "site_pos_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void site_pos_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, int electrodeId, int siteId, float[] position);

            [DllImport("hbp_export", EntryPoint = "sites_nb_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int sites_nb_PatientElectrodesList(HandleRef handlePatientElectrodesList);

            [DllImport("hbp_export", EntryPoint = "site_mask_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int site_mask_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, int electrodeId, int siteId);

            [DllImport("hbp_export", EntryPoint = "patients_nb_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int patients_nb_PatientElectrodesList(HandleRef handlePatientElectrodesList);

            [DllImport("hbp_export", EntryPoint = "patient_sites_nb_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int patient_sites_nb_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId);

            [DllImport("hbp_export", EntryPoint = "electrodes_nb_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int electrodes_nb_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId);

            [DllImport("hbp_export", EntryPoint = "electrode_sites_nb_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int electrode_sites_nb_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, int electrodeId);

            [DllImport("hbp_export", EntryPoint = "patient_name_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void patient_name_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, StringBuilder name, int length);

            [DllImport("hbp_export", EntryPoint = "electrode_name_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void electrode_name_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, int electrodeId, StringBuilder name, int length);

            [DllImport("hbp_export", EntryPoint = "site_mars_atlas_label_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int site_mars_atlas_label_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, int electrodeId, int siteId);

            #endregion DLLImport
        }
    }
}