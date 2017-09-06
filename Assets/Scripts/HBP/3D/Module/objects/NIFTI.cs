﻿
/**
 * \file    NIFTI.cs
 * \author  Lance Florian
 * \date    2015
 * \brief   Define NIFTI class
 */

// system
using System;
using System.Runtime.InteropServices;

namespace HBP.Module3D.DLL
{
    /// <summary>
    /// A DLL wrapper NIFTI class, use to convert nii files into volumes
    /// </summary>
    public class NIFTI : CppDLLImportBase
    {
        #region Properties
        public bool IsLoaded { get; private set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathNiftiFile"></param>
        /// <returns></returns>
        public bool LoadNIIFile(string pathNiftiFile)
        {
            IsLoaded = (loadNiiFile_NIFTI(_handle, pathNiftiFile) == 1);
            return IsLoaded;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="volume"></param>
        public void ConvertToVolume(Volume volume)
        {
            convertToVolume_NIFTI(_handle, volume.getHandle());
        }
        #endregion

        #region Memory Management
        /// <summary>
        /// Allocate DLL memory
        /// </summary>
        protected override void create_DLL_class()
        {
            _handle = new HandleRef(this, create_NIFTI());
        }
        /// <summary>
        /// Clean DLL memory
        /// </summary>
        protected override void delete_DLL_class()
        {
            delete_NIFTI(_handle);
        }
        #endregion

        #region DLLImport

        // memory management
        [DllImport("hbp_export", EntryPoint = "create_NIFTI", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr create_NIFTI();

        [DllImport("hbp_export", EntryPoint = "delete_NIFTI", CallingConvention = CallingConvention.Cdecl)]
        static private extern void delete_NIFTI(HandleRef handleNii);

        // save / load
        [DllImport("hbp_export", EntryPoint = "loadNiiFile_NIFTI", CallingConvention = CallingConvention.Cdecl)]
        static private extern int loadNiiFile_NIFTI(HandleRef handleNii, string pathFile);

        // actions
        [DllImport("hbp_export", EntryPoint = "convertToVolume_NIFTI", CallingConvention = CallingConvention.Cdecl)]
        static private extern void convertToVolume_NIFTI(HandleRef handleNii, HandleRef handleVolume);

        #endregion
    }
}