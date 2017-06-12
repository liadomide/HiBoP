﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class IEEGSitesParameters : Tool
    {
        #region Properties
        [SerializeField]
        private Slider m_Slider;

        [SerializeField]
        private InputField m_InputField;

        public GenericEvent<float, float> OnValueChanged = new GenericEvent<float, float>();
        #endregion

        #region Public Methods
        public override void AddListeners()
        {
            m_Slider.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                HBP.Module3D.Column3DIEEG column = (HBP.Module3D.Column3DIEEG)ApplicationState.Module3D.SelectedScene.ColumnManager.SelectedColumn;
                column.IEEGParameters.Gain = value;
                OnValueChanged.Invoke(value, column.IEEGParameters.MaximumInfluence);
            });

            m_InputField.onEndEdit.AddListener((value) =>
            {
                if (ListenerLock) return;

                float val = float.Parse(value);
                HBP.Module3D.Column3DIEEG column = (HBP.Module3D.Column3DIEEG)ApplicationState.Module3D.SelectedScene.ColumnManager.SelectedColumn;
                column.IEEGParameters.MaximumInfluence = val;
                m_InputField.text = column.IEEGParameters.MaximumInfluence.ToString("N2");
                OnValueChanged.Invoke(column.IEEGParameters.Gain, val);
            });
        }

        public override void DefaultState()
        {
            m_Slider.value = 0.5f;
            m_Slider.interactable = false;
            m_InputField.text = "15.00";
            m_InputField.interactable = false;
        }

        public override void UpdateInteractable()
        {
            switch (ApplicationState.Module3D.SelectedScene.ModesManager.CurrentModeID)
            {
                case HBP.Module3D.Mode.ModesId.NoPathDefined:
                    m_Slider.interactable = false;
                    m_InputField.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.MinPathDefined:
                    m_Slider.interactable = true;
                    m_InputField.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.AllPathDefined:
                    m_Slider.interactable = true;
                    m_InputField.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.ComputingAmplitudes:
                    m_Slider.interactable = false;
                    m_InputField.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.AmplitudesComputed:
                    m_Slider.interactable = true;
                    m_InputField.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.TriErasing:
                    m_Slider.interactable = false;
                    m_InputField.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.ROICreation:
                    m_Slider.interactable = false;
                    m_InputField.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.AmpNeedUpdate:
                    m_Slider.interactable = true;
                    m_InputField.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.Error:
                    m_Slider.interactable = false;
                    m_InputField.interactable = false;
                    break;
                default:
                    break;
            }
        }

        public override void UpdateStatus(Toolbar.UpdateToolbarType type)
        {
            if (type == Toolbar.UpdateToolbarType.Scene || type == Toolbar.UpdateToolbarType.Column)
            {
                HBP.Module3D.Column3DIEEG selectedColumn = (HBP.Module3D.Column3DIEEG)ApplicationState.Module3D.SelectedScene.ColumnManager.SelectedColumn;
                m_Slider.value = selectedColumn.IEEGParameters.Gain;
                m_InputField.text = selectedColumn.IEEGParameters.MaximumInfluence.ToString();
            }
        }
        #endregion
    }
}