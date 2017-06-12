﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class IEEGTransparency : Tool
    {
        #region Properties
        [SerializeField]
        private Slider m_Slider;

        public GenericEvent<float> OnChangeValue = new GenericEvent<float>();
        #endregion

        #region Public Methods
        public override void AddListeners()
        {
            m_Slider.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                ((HBP.Module3D.Column3DIEEG)ApplicationState.Module3D.SelectedScene.ColumnManager.SelectedColumn).IEEGParameters.AlphaMin = value;
                //((HBP.Module3D.Column3DIEEG)ApplicationState.Module3D.SelectedScene.ColumnManager.SelectedColumn).IEEGParameters.AlphaMax = value; // FIXME : Required / other value ?
                OnChangeValue.Invoke(value);
            });
        }

        public override void DefaultState()
        {
            m_Slider.value = 0.2f;
            m_Slider.interactable = false;
        }

        public override void UpdateInteractable()
        {
            switch (ApplicationState.Module3D.SelectedScene.ModesManager.CurrentModeID)
            {
                case HBP.Module3D.Mode.ModesId.NoPathDefined:
                    m_Slider.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.MinPathDefined:
                    m_Slider.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.AllPathDefined:
                    m_Slider.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.ComputingAmplitudes:
                    m_Slider.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.AmplitudesComputed:
                    m_Slider.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.TriErasing:
                    m_Slider.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.ROICreation:
                    m_Slider.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.AmpNeedUpdate:
                    m_Slider.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.Error:
                    m_Slider.interactable = false;
                    break;
                default:
                    break;
            }
        }

        public override void UpdateStatus(Toolbar.UpdateToolbarType type)
        {
            if (type == Toolbar.UpdateToolbarType.Scene || type == Toolbar.UpdateToolbarType.Column)
            {
                m_Slider.value = ((HBP.Module3D.Column3DIEEG)ApplicationState.Module3D.SelectedScene.ColumnManager.SelectedColumn).IEEGParameters.AlphaMin;
            }
        }
        #endregion
    }
}