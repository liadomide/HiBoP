﻿using HBP.Module3D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class FMRIParameters : Tool
    {
        #region Properties
        [SerializeField]
        private Text m_MinText;

        [SerializeField]
        private Text m_MaxText;

        [SerializeField]
        private InputField m_CalMinInputField;

        [SerializeField]
        private Slider m_CalMinSlider;

        [SerializeField]
        private InputField m_CalMaxInputField;

        [SerializeField]
        private Slider m_CalMaxSlider;

        [SerializeField]
        private Slider m_AlphaSlider;
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            m_CalMinInputField.onEndEdit.AddListener((value) =>
            {
                if (ListenerLock) return;
                
                float floatValue;
                if (global::Tools.CSharp.NumberExtension.TryParseFloat(value, out floatValue))
                {
                    SelectedScene.FMRIManager.FMRICalMin = floatValue;
                }
                ListenerLock = true;
                UpdateStatus();
                ListenerLock = false;
            });
            m_CalMinSlider.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                SelectedScene.FMRIManager.FMRICalMinFactor = value;
                ListenerLock = true;
                UpdateStatus();
                ListenerLock = false;
            });
            m_CalMaxInputField.onEndEdit.AddListener((value) =>
            {
                if (ListenerLock) return;
                
                float floatValue;
                if (global::Tools.CSharp.NumberExtension.TryParseFloat(value, out floatValue))
                {
                    SelectedScene.FMRIManager.FMRICalMax = floatValue;
                }
                ListenerLock = true;
                UpdateStatus();
                ListenerLock = false;
            });
            m_CalMaxSlider.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                SelectedScene.FMRIManager.FMRICalMaxFactor = value;
                ListenerLock = true;
                UpdateStatus();
                ListenerLock = false;
            });
            m_AlphaSlider.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                SelectedScene.FMRIManager.FMRIAlpha = value;
                ListenerLock = true;
                UpdateStatus();
                ListenerLock = false;
            });
        }

        public override void DefaultState()
        {
            gameObject.SetActive(false);
        }

        public override void UpdateInteractable()
        {
            bool hasFMRI = SelectedScene.FMRIManager.FMRI != null;

            gameObject.SetActive(hasFMRI);
        }

        public override void UpdateStatus()
        {
            bool hasFMRI = SelectedScene.FMRIManager.FMRI != null;
            if (hasFMRI)
            {
                MRICalValues calValues = SelectedScene.FMRIManager.FMRI.Volume.ExtremeValues;
                m_MinText.text = calValues.ComputedCalMin.ToString("N2");
                m_MaxText.text = calValues.ComputedCalMax.ToString("N2");
                m_CalMinInputField.text = SelectedScene.FMRIManager.FMRICalMin.ToString("N2");
                m_CalMaxInputField.text = SelectedScene.FMRIManager.FMRICalMax.ToString("N2");
                m_CalMinSlider.value = SelectedScene.FMRIManager.FMRICalMinFactor;
                m_CalMaxSlider.value = SelectedScene.FMRIManager.FMRICalMaxFactor;
                m_AlphaSlider.value = SelectedScene.FMRIManager.FMRIAlpha;
            }
        }
        #endregion
    }
}