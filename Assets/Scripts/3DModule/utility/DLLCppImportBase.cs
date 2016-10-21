﻿
/**
 * \file    CppDLLImportBase.cs
 * \author  Lance Florian
 * \date    10/02/2016
 * \brief   Define CppDLLImportBase
 */

// system
using System;
using System.Runtime.InteropServices;

namespace HBP.VISU3D.DLL
{
    /// <summary>
    /// Base class for creating C++ Dll import classes
    /// </summary>
    public abstract class CppDLLImportBase : IDisposable
    {
        #region members

        /// <summary>
        /// pointer to C+ dll class
        /// </summary>
        protected HandleRef _handle;

        #endregion members

        #region functions

        /// <summary>
        /// CppDLLImportBase default constructor
        /// </summary>
        public CppDLLImportBase()
        {
            createDLLClass();
        }

        /// <summary>
        /// CppDLLImportBase constructor with an already allocated dll class
        /// </summary>
        /// <param name="ptr"></param>
        public CppDLLImportBase(IntPtr ptr)
        {
            _handle = new HandleRef(this, ptr);
        }

        /// <summary>
        /// CppDLLImportBase Destructor
        /// </summary>
        ~CppDLLImportBase()
        {
            Cleanup();
        }

        /// <summary>
        /// Allocate DLL memory
        /// </summary>
        abstract protected void createDLLClass();

        /// <summary>
        /// Clean DLL memory
        /// </summary>
        abstract protected void deleteDLLClass();

        /// <summary>
        /// Force delete C++ DLL data (remove GC for this object)
        /// </summary>
        public virtual void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Delete C+ DLL data, and set handle to IntPtr.Zero
        /// </summary>
        private void Cleanup()
        {
            deleteDLLClass();
            _handle = new HandleRef(this, IntPtr.Zero);
        }

        /// <summary>
        /// Return pointer to C++ DLL
        /// </summary>
        /// <returns></returns>
        public HandleRef getHandle()
        {
            return _handle;
        }

        #endregion functions
    }
}