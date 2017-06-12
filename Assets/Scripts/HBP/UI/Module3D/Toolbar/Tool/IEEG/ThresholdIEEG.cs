﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class ThresholdIEEG : Tool
    {
        #region Properties
        [SerializeField]
        private Button m_Button;

        [SerializeField]
        private Module3D.ThresholdIEEG m_ThresholdIEEG;

        public GenericEvent<float, float, float> OnChangeValues = new GenericEvent<float, float, float>();
        #endregion

        #region Public Methods
        public override void AddListeners()
        {
            m_ThresholdIEEG.OnChangeValues.AddListener((min, mid, max) =>
            {
                OnChangeValues.Invoke(min, mid, max);
            });
        }

        public override void DefaultState()
        {
            m_Button.interactable = false;
        }

        public override void UpdateInteractable()
        {
            switch (ApplicationState.Module3D.SelectedScene.ModesManager.CurrentModeID)
            {
                case HBP.Module3D.Mode.ModesId.NoPathDefined:
                    m_Button.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.MinPathDefined:
                    m_Button.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.AllPathDefined:
                    m_Button.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.ComputingAmplitudes:
                    m_Button.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.AmplitudesComputed:
                    m_Button.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.TriErasing:
                    m_Button.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.ROICreation:
                    m_Button.interactable = false;
                    break;
                case HBP.Module3D.Mode.ModesId.AmpNeedUpdate:
                    m_Button.interactable = true;
                    break;
                case HBP.Module3D.Mode.ModesId.Error:
                    m_Button.interactable = false;
                    break;
                default:
                    break;
            }
        }

        public override void UpdateStatus(Toolbar.UpdateToolbarType type)
        {
            if (type == Toolbar.UpdateToolbarType.Scene || type == Toolbar.UpdateToolbarType.Column) // maybe FIXME
            {
                HBP.Module3D.Column3D selectedColumn = ApplicationState.Module3D.SelectedScene.ColumnManager.SelectedColumn;
                if (selectedColumn is HBP.Module3D.Column3DIEEG)
                {
                    m_ThresholdIEEG.UpdateIEEGValues(((HBP.Module3D.Column3DIEEG)selectedColumn).IEEGParameters);
                }
            }
        }
        #endregion
    }
}