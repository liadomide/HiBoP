﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class AutoRotate : Tool
    {
        #region Properties
        [SerializeField]
        private Toggle m_Toggle;
        [SerializeField]
        private Slider m_Slider;

        public GenericEvent<bool, float> OnChangeValue = new GenericEvent<bool, float>();
        #endregion

        #region Public Methods
        public override void AddListeners()
        {
            m_Toggle.onValueChanged.AddListener((isOn) =>
            {
                if (ListenerLock) return;

                ApplicationState.Module3D.SelectedScene.AutomaticRotation = isOn;
                OnChangeValue.Invoke(isOn, m_Slider.value);
            });

            m_Slider.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                ApplicationState.Module3D.SelectedScene.AutomaticRotationSpeed = value;
                OnChangeValue.Invoke(m_Toggle.isOn, value);
            });
        }

        public override void DefaultState()
        {
            m_Toggle.isOn = false;
            m_Toggle.interactable = false;
            m_Slider.value = 30.0f;
            m_Slider.interactable = false;
        }

        public override void UpdateInteractable()
        {
            switch (ApplicationState.Module3D.SelectedScene.ModesManager.CurrentModeID)
            {
                case HBP.Module3D.Mode.ModesId.NoPathDefined:
                    m_Toggle.interactable = false;
                    m_Slider.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.MinPathDefined:
                    m_Toggle.interactable = true;
                    m_Slider.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.AllPathDefined:
                    m_Toggle.interactable = true;
                    m_Slider.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.ComputingAmplitudes:
                    m_Toggle.interactable = true;
                    m_Slider.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.AmplitudesComputed:
                    m_Toggle.interactable = true;
                    m_Slider.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.TriErasing:
                    m_Toggle.interactable = false;
                    m_Slider.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.ROICreation:
                    m_Toggle.interactable = false;
                    m_Slider.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.AmpNeedUpdate:
                    m_Toggle.interactable = true;
                    m_Slider.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.Error:
                    m_Toggle.interactable = false;
                    m_Slider.interactable = false;
                    break;
                default:
                    break;
            }
        }

        public override void UpdateStatus(Toolbar.UpdateToolbarType type)
        {
            if (type == Toolbar.UpdateToolbarType.Scene)
            {
                m_Toggle.isOn = ApplicationState.Module3D.SelectedScene.AutomaticRotation;
                m_Slider.value = ApplicationState.Module3D.SelectedScene.AutomaticRotationSpeed;
            }
        }
        #endregion
    }
}