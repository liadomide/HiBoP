﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HBP.Module3D
{
    public class MRI3D : ICloneable
    {
        #region Properties
        public string Name { get; set; }
        public DLL.NIFTI NII { get; set; }
        public DLL.Volume Volume { get; set; }
        public bool IsLoaded
        {
            get
            {
                return NII.IsLoaded;
            }
        }
        #endregion

        #region Constructors
        public MRI3D(Data.Anatomy.MRI mri)
        {
            Name = mri.Name;
            NII = new DLL.NIFTI();
            Volume = new DLL.Volume();
            if (NII.LoadNIIFile(mri.File))
            {
                NII.ConvertToVolume(Volume);
            }
        }
        public MRI3D(string name, DLL.NIFTI nii, DLL.Volume volume)
        {
            Name = name;
            NII = nii;
            Volume = volume;
        }
        public MRI3D() { }
        #endregion

        #region Public Methods
        public object Clone()
        {
            MRI3D mri = new MRI3D();
            mri.Name = Name;
            mri.NII = NII;
            mri.Volume = Volume;
            return mri;
        }
        #endregion
    }
}