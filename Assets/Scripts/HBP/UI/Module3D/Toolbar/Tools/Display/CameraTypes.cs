﻿using HBP.Module3D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class CameraTypes : Tool
    {
        #region Properties
        [SerializeField]
        private Dropdown m_Dropdown;
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            m_Dropdown.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                SelectedScene.CameraType = (Data.Enums.CameraControl)value;
            });
        }
        public override void DefaultState()
        {
            m_Dropdown.value = 0;
            m_Dropdown.interactable = false;
        }
        public override void UpdateInteractable()
        {
            m_Dropdown.interactable = true;
        }
        public override void UpdateStatus()
        {
            m_Dropdown.value = (int)SelectedScene.CameraType;
        }
        #endregion
    }
}