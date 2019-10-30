﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HBP.Module3D
{
    namespace DLL
    {
        /// <summary>
        /// Mars atlas index, used to identify sites mars IDs and areas on the brain
        /// </summary>
        public class MarsAtlasIndex : Tools.DLL.CppDLLImportBase
        {
            #region Constructors
            public MarsAtlasIndex(string path) : base()
            {
                if (load_MarsAtlasIndex(_handle, path) != 1)
                {
                    Debug.LogError("Can't load mars atlas index.");
                }
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Return the name of the hemisphere given a mars atlas label ID
            /// </summary>
            /// <param name="id">ID of mars atlas label</param>
            /// <returns>Name of the hemipshere</returns>
            public string Hemisphere(int id)
            {
                if (id < 0) return "not found";

                IntPtr result = hemisphere_MarsAtlasIndex(_handle, id);
                return Marshal.PtrToStringAnsi(result);
            }
            /// <summary>
            /// Return the name of the lobe given a mars atlas label ID
            /// </summary>
            /// <param name="id">ID of mars atlas label</param>
            /// <returns>Name of the lobe</returns>
            public string Lobe(int label)
            {
                if (label < 0) return "not found";

                IntPtr result = lobe_MarsAtlasIndex(_handle, label);
                return Marshal.PtrToStringAnsi(result);
            }
            /// <summary>
            /// Return the name of the name fs given a mars atlas label ID
            /// </summary>
            /// <param name="id">ID of mars atlas label</param>
            /// <returns>Name of the name fs</returns>
            public string NameFS(int label)
            {
                if (label < 0) return "not found";

                IntPtr result = nameFS_MarsAtlasIndex(_handle, label);
                return Marshal.PtrToStringAnsi(result);
            }
            /// <summary>
            /// Return the name of a mars atlas area given a mars atlas label ID
            /// </summary>
            /// <param name="id">ID of mars atlas label</param>
            /// <returns>Name of the mars atlas area</returns>
            public string Name(int label)
            {
                if (label < 0) return "not found";

                IntPtr result = name_MarsAtlasIndex(_handle, label);
                return Marshal.PtrToStringAnsi(result);
            }
            /// <summary>
            /// Return the full name of a mars atlas area given a mars atlas label ID
            /// </summary>
            /// <param name="id">ID of mars atlas label</param>
            /// <returns>Full name of the mars atlas area</returns>
            public string FullName(int label)
            {
                if (label < 0) return "not found";

                IntPtr result = fullName_MarsAtlasIndex(_handle, label);
                return Marshal.PtrToStringAnsi(result);
            }
            /// <summary>
            /// Return the name of the brodmann area given a mars atlas label ID
            /// </summary>
            /// <param name="id">ID of mars atlas label</param>
            /// <returns>Name of the brodmann area</returns>
            public string BrodmannArea(int label)
            {
                if (label < 0) return "not found";

                IntPtr result = BA_MarsAtlasIndex(_handle, label);
                return Marshal.PtrToStringAnsi(result);
            }
            #endregion

            #region Memory Management
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
            #endregion

            #region DLLImport
            [DllImport("hbp_export", EntryPoint = "create_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr create_MarsAtlasIndex();
            [DllImport("hbp_export", EntryPoint = "delete_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern void delete_MarsAtlasIndex(HandleRef marsAtlasIndex);
            [DllImport("hbp_export", EntryPoint = "load_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern int load_MarsAtlasIndex(HandleRef marsAtlasIndex, string pathFile);
            [DllImport("hbp_export", EntryPoint = "hemisphere_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr hemisphere_MarsAtlasIndex(HandleRef marsAtlasIndex, int label);
            [DllImport("hbp_export", EntryPoint = "lobe_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr lobe_MarsAtlasIndex(HandleRef marsAtlasIndex, int label);
            [DllImport("hbp_export", EntryPoint = "nameFS_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr nameFS_MarsAtlasIndex(HandleRef marsAtlasIndex, int label);
            [DllImport("hbp_export", EntryPoint = "name_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr name_MarsAtlasIndex(HandleRef marsAtlasIndex, int label);
            [DllImport("hbp_export", EntryPoint = "fullName_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr fullName_MarsAtlasIndex(HandleRef marsAtlasIndex, int label);
            [DllImport("hbp_export", EntryPoint = "BA_MarsAtlasIndex", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr BA_MarsAtlasIndex(HandleRef marsAtlasIndex, int label);
            #endregion

        }

        /// <summary>
        /// A raw version of <see cref="PatientElectrodesList"/>, meant to be used in the C++ DLL
        /// </summary>
        /// <remarks>
        /// This class is used to perform simple actions regarding all sites in the scene
        /// It contains the list of the sites in the DLL ordered by global site ID
        /// </remarks>
        public class RawSiteList : Tools.DLL.CppDLLImportBase
        {
            #region Properties
            /// <summary>
            /// Number of sites
            /// </summary>
            public int NumberOfSites
            {
                get
                {
                    return sites_nb_RawSiteList(_handle);
                }
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Save the raw site list to an obj file
            /// </summary>
            /// <param name="pathObjNameFile">Path to the obj file to be saved</param>
            /// <returns>True if the obj file has been correctly saved</returns>
            public bool SaveToObj(string pathObjNameFile)
            {
                bool success = saveToObj_RawPlotList(_handle, pathObjNameFile) == 1;
                ApplicationState.DLLDebugManager.check_error();
                return success;
            }
            /// <summary>
            /// Update the mask of a given site in the list
            /// </summary>
            /// <param name="idSite">Global ID of the site in the list</param>
            /// <param name="mask">True if the site has to be masked</param>
            public void UpdateMask(int idSite, bool mask)
            {
                update_mask_RawSiteList(_handle, idSite, mask ? 1 : 0);
            }
            /// <summary>
            /// Get an array containing bool values telling if a site is on a plane or not considering a specific precision
            /// </summary>
            /// <param name="plane">Plane on which to perform the check</param>
            /// <param name="precision">Precision of the check</param>
            /// <param name="result">Result array containing a bool for each site in the order of the raw site list: true if the site is on the plane, false otherwise</param>
            public void GetSitesOnPlane(Plane plane, float precision, out int[] result)
            {
                result = new int[NumberOfSites];
                float[] planeV = new float[6];
                for (int ii = 0; ii < 3; ++ii)
                {
                    planeV[ii] = plane.Point[ii];
                    planeV[ii + 3] = plane.Normal[ii];
                }
                sites_on_plane_RawSiteList(_handle, planeV, precision, result);
            }
            /// <summary>
            /// Check if a site is on any plane given a list of planes
            /// </summary>
            /// <param name="site">Site to check</param>
            /// <param name="planes">Planes on which to perform the check</param>
            /// <param name="precision">Precision of the check</param>
            /// <returns>True if the site is on any plane</returns>
            public bool IsSiteOnAnyPlane(Site site, IEnumerable<Plane> planes, float precision)
            {
                bool result = false;
                foreach (var plane in planes)
                {
                    float[] planeV = new float[6];
                    for (int ii = 0; ii < 3; ++ii)
                    {
                        planeV[ii] = plane.Point[ii];
                        planeV[ii + 3] = plane.Normal[ii];
                    }
                    result |= is_site_on_plane_RawSiteList(_handle, site.Information.GlobalID, planeV, precision) == 1;
                }
                return result;
            }
            #endregion

            #region Memory Management
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
            #endregion

            #region DLLImport
            [DllImport("hbp_export", EntryPoint = "create_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr create_RawSiteList();
            [DllImport("hbp_export", EntryPoint = "delete_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void delete_RawSiteList(HandleRef handleRawSiteLst);
            [DllImport("hbp_export", EntryPoint = "update_mask_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void update_mask_RawSiteList(HandleRef handleRawSiteLst, int plotId, int mask);
            [DllImport("hbp_export", EntryPoint = "saveToObj_RawPlotList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int saveToObj_RawPlotList(HandleRef handleRawSiteLst, string pathObjNameFile);
            [DllImport("hbp_export", EntryPoint = "sites_nb_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int sites_nb_RawSiteList(HandleRef handleRawSiteLst);
            [DllImport("hbp_export", EntryPoint = "is_site_on_plane_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int is_site_on_plane_RawSiteList(HandleRef handleRawSiteLst, int siteID, float[] planeV, float precision);
            [DllImport("hbp_export", EntryPoint = "sites_on_plane_RawSiteList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void sites_on_plane_RawSiteList(HandleRef handleRawSiteLst, float[] planeV, float precision, int[] result);
            #endregion
        }

        /// <summary>
        /// The default electrodes container
        /// </summary>
        /// <remarks>
        /// This class contains all the data for the sites on the DLL side
        /// It has information about patients, positions, mars atlas etc. for each site
        /// </remarks>
        public class PatientElectrodesList : Tools.DLL.CppDLLImportBase, ICloneable
        {
            #region Properties
            /// <summary>
            /// Number of sites
            /// </summary>
            public int NumberOfSites
            {
                get
                {
                    return sites_nb_PatientElectrodesList(_handle);
                }
            }
            /// <summary>
            /// Number of patients
            /// </summary>
            public int NumberOfPatients
            {
                get
                {
                    return patients_nb_PatientElectrodesList(_handle);
                }
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Load a list of files to fill the list of electrodes
            /// </summary>
            /// <param name="ptsFilesPath">List of the pts files describing the position of the sites</param>
            /// <param name="marsAtlas">List of the mars atlas files describing the mars atlas labels of the sites</param>
            /// <param name="names">List of the names of the patients</param>
            /// <param name="marsAtlasIndex">Reference to the mars atlas index</param>
            /// <returns>True if the files have been correctly loaded</returns>
            public bool LoadPTSFiles(string[] ptsFilesPath, string[] marsAtlas, string[] names, MarsAtlasIndex marsAtlasIndex)
            {
                string ptsFilesPathStr = string.Join("?", ptsFilesPath);
                string marsAtlasStr = string.Join("?", marsAtlas);
                string namesStr = string.Join("?", names);

                // load in the DLL
                bool fileLoaded = load_Pts_files_PatientElectrodesList(_handle, ptsFilesPathStr, marsAtlasStr, namesStr, marsAtlasIndex.getHandle()) == 1;
                ApplicationState.DLLDebugManager.check_error();

                if (!fileLoaded)
                {
                    Debug.LogError("PatientElectrodesList : Error during loading. " + ptsFilesPathStr);
                    return false;
                }

                return true;
            }
            /// <summary>
            /// Return the number of electrode given a patient index 
            /// </summary>
            /// <param name="patientIndex">Index of the patient to get the number of electrodes from</param>
            /// <returns>Number of electrodes of this patient</returns>
            public int NumberOfElectrodesInPatient(int patientIndex)
            {
                return electrodes_nb_PatientElectrodesList(_handle, patientIndex);
            }
            /// <summary>
            /// Return the number of sites given a patient index and an electrode index
            /// </summary>
            /// <param name="patientIndex">Index of the patient to get the number of sites from</param>
            /// <param name="electrodeIndex">Index of the electrode to get the number of sites from</param>
            /// <returns>Number of sites of this electrode</returns>
            public int NumberOfSitesInElectrode(int patientIndex, int electrodeIndex)
            { 
                return electrode_sites_nb_PatientElectrodesList(_handle, patientIndex, electrodeIndex);
            }
            /// <summary>
            /// Return the number of sites given a patient index
            /// </summary>
            /// <param name="patientIndex">Index of the patient to get the number of sites from</param>
            /// <returns>Number of sites of this patient</returns>
            public int NumberOfSitesInPatient(int patientIndex)
            {
                return patient_sites_nb_PatientElectrodesList(_handle, patientIndex);
            }
            /// <summary>
            /// Set the mask for the sites of a patient
            /// </summary>
            /// <param name="state">True if masked</param>
            /// <param name="patientIndex">Index of the patient</param>
            public void SetPatientMask(bool state, int patientIndex)
            {
                set_mask_patient_PatientElectrodesList(_handle, state ? 1 : 0, patientIndex);
            }
            /// <summary>
            /// Set the mask for the sites of an electrode
            /// </summary>
            /// <param name="state">True if masked</param>
            /// <param name="patientIndex">Index of the patient</param>
            /// <param name="electrodeIndex">Index of the electrode</param>
            public void SetElectrodeMask(bool state, int patientIndex, int electrodeIndex)
            {
                set_mask_electrode_PatientElectrodesList(_handle, state ? 1 : 0, patientIndex, electrodeIndex);
            }
            /// <summary>
            /// Set the mask for a site
            /// </summary>
            /// <param name="state">True if masked</param>
            /// <param name="patientIndex">Index of the patient</param>
            /// <param name="electrodeIndex">Index of the electrode</param>
            /// <param name="siteIndex">Index of the site</param>
            public void SetSiteMask(bool state, int patientIndex, int electrodeIndex, int siteIndex)
            {                
                set_mask_site_PatientElectrodesList(_handle, state ? 1 : 0, patientIndex, electrodeIndex, siteIndex);
            }
            /// <summary>
            /// Get the mask of a site
            /// </summary>
            /// <param name="patientIndex">Index of the patient</param>
            /// <param name="electrodeIndex">Index of the electrode</param>
            /// <param name="siteIndex">Index of the site</param>
            /// <returns>True if the site is masked</returns>
            public bool SiteMask(int patientIndex, int electrodeIndex, int siteIndex)
            {
                return site_mask_PatientElectrodesList(_handle, patientIndex, electrodeIndex, siteIndex) == 1;
            }
            /// <summary>
            /// Get the position of a site (in DLL reference)
            /// </summary>
            /// <param name="patientIndex">Index of the patient</param>
            /// <param name="electrodeIndex">Index of the electrode</param>
            /// <param name="siteIndex">Index of the site</param>
            /// <returns>Position of the site</returns>
            public Vector3 SitePosition(int patientIndex, int electrodeIndex, int siteIndex)
            {
                float[] pos = new float[3];                
                site_pos_PatientElectrodesList(_handle, patientIndex, electrodeIndex, siteIndex, pos);
                return new Vector3(pos[0], pos[1], pos[2]);
            }
            /// <summary>
            /// Get the name of the site
            /// </summary>
            /// <param name="patientIndex">Index of the patient</param>
            /// <param name="electrodeIndex">Index of the electrode</param>
            /// <param name="siteIndex">Index of the site</param>
            /// <returns>Name of the site</returns>
            public string SiteName(int patientIndex, int electrodeIndex, int siteIndex)
            {
                IntPtr result = site_name_PatientElectrodesList(_handle, patientIndex, electrodeIndex, siteIndex);
                return Marshal.PtrToStringAnsi(result);
            }
            /// <summary>
            /// Fill the <see cref="RawSiteList"/> using the full electrodes list
            /// </summary>
            /// <param name="rawSiteList">RawSiteList to be filled</param>
            public void ExtractRawSiteList(RawSiteList rawSiteList)
            {
                extract_raw_site_list_PatientElectrodesList(_handle, rawSiteList.getHandle());
            }
            /// <summary>
            /// Update the mask of the sites in the <see cref="RawSiteList"/> using the full electrodes list
            /// </summary>
            /// <param name="rawSiteList">RawSiteList to be filled</param>
            public void UpdateRawSiteListMask(RawSiteList rawSiteList)
            {
                update_raw_site_list_mask_PatientElectrodesList(_handle, rawSiteList.getHandle());
            }
            /// <summary>
            /// Get the name of the patient given its index
            /// </summary>
            /// <param name="patientIndex">Index of the patient</param>
            /// <returns>Name of the patient</returns>
            public string PatientName(int patientIndex)
            {
                IntPtr result = patient_name_PatientElectrodesList(_handle, patientIndex);
                return Marshal.PtrToStringAnsi(result);
            }
            /// <summary>
            /// Get the name of the electrode
            /// </summary>
            /// <param name="patientIndex">Index of the patient</param>
            /// <param name="electrodeIndex">Index of the electrode</param>
            /// <returns>Name of the electrode</returns>
            public string ElectrodeName(int patientIndex, int electrodeIndex)
            {
                IntPtr result = electrode_name_PatientElectrodesList(_handle, patientIndex, electrodeIndex);
                return Marshal.PtrToStringAnsi(result);
            }
            /// <summary>
            /// Get the mars atlas label of a site
            /// </summary>
            /// <param name="patientIndex">Index of the patient</param>
            /// <param name="electrodeIndex">Index of the electrode</param>
            /// <param name="siteIndex">Index of the site</param>
            /// <returns>Index of the mars atlas label</returns>
            public int MarsAtlasLabelOfSite(int patientIndex, int electrodeIndex, int siteIndex)
            {
                return site_mars_atlas_label_PatientElectrodesList(_handle, patientIndex, electrodeIndex, siteIndex);
            }
            /// <summary>
            /// Get the freesurfer index of a site
            /// </summary>
            /// <param name="patientIndex">Index of the patient</param>
            /// <param name="electrodeIndex">Index of the electrode</param>
            /// <param name="siteIndex">Index of the site</param>
            /// <returns></returns>
            public string FreesurferLabelOfSite(int patientIndex, int electrodeIndex, int siteIndex)
            {
                IntPtr result = site_freesurfer_label_PatientElectrodesList(_handle, patientIndex, electrodeIndex, siteIndex);
                return Marshal.PtrToStringAnsi(result);
            }
            public object Clone()
            {
                return new PatientElectrodesList(this);
            }
            #endregion

            #region Memory Management
            public PatientElectrodesList() : base() { }
            private PatientElectrodesList(IntPtr electrodesPatientMultiListHandle) : base(electrodesPatientMultiListHandle) { }
            public PatientElectrodesList(PatientElectrodesList other) : base(clone_PatientElectrodesList(other.getHandle())) { }
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
            #endregion

            #region DLLImport
            [DllImport("hbp_export", EntryPoint = "create_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr create_PatientElectrodesList();
            [DllImport("hbp_export", EntryPoint = "delete_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern void delete_PatientElectrodesList(HandleRef handlePatientElectrodesList);
            [DllImport("hbp_export", EntryPoint = "clone_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr clone_PatientElectrodesList(HandleRef handlePatientElectrodesListToBeCloned);
            [DllImport("hbp_export", EntryPoint = "load_Pts_files_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int load_Pts_files_PatientElectrodesList(HandleRef handlePatientElectrodesList, string pathFiles, string marsAtlas, string names, HandleRef handleMarsAtlasIndex);
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
            [DllImport("hbp_export", EntryPoint = "site_name_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr site_name_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, int electrodeId, int siteId);
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
            static private extern IntPtr patient_name_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId);
            [DllImport("hbp_export", EntryPoint = "electrode_name_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr electrode_name_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, int electrodeId);
            [DllImport("hbp_export", EntryPoint = "site_mars_atlas_label_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern int site_mars_atlas_label_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, int electrodeId, int siteId);
            [DllImport("hbp_export", EntryPoint = "site_freesurfer_label_PatientElectrodesList", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr site_freesurfer_label_PatientElectrodesList(HandleRef handlePatientElectrodesList, int patientId, int electrodeId, int siteId);
            #endregion
        }
    }
}