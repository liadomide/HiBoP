﻿using HBP.Module3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class DefaultView : Tool
    {
        #region Properties
        [SerializeField]
        private Button m_Button;
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            m_Button.onClick.AddListener(() =>
            {
                if (ListenerLock) return;

                int lineID = ApplicationState.Module3D.SelectedView.LineID;
                foreach (Column3D column in ApplicationState.Module3D.SelectedScene.ColumnManager.Columns)
                {
                    foreach (View3D view in column.Views)
                    {
                        if (view.LineID == lineID)
                        {
                            view.Default();
                        }
                    }
                }
            });
        }
        public override void DefaultState()
        {
            m_Button.interactable = false;
        }
        public override void UpdateInteractable()
        {
            switch (ApplicationState.Module3D.SelectedScene.ModesManager.CurrentMode.ID)
            {
                case Mode.ModesId.NoPathDefined:
                    m_Button.interactable = false;
                    break;
                case Mode.ModesId.MinPathDefined:
                    m_Button.interactable = true;
                    break;
                case Mode.ModesId.AllPathDefined:
                    m_Button.interactable = true;
                    break;
                case Mode.ModesId.ComputingAmplitudes:
                    m_Button.interactable = true;
                    break;
                case Mode.ModesId.AmplitudesComputed:
                    m_Button.interactable = true;
                    break;
                case Mode.ModesId.TriErasing:
                    m_Button.interactable = false;
                    break;
                case Mode.ModesId.ROICreation:
                    m_Button.interactable = false;
                    break;
                case Mode.ModesId.AmpNeedUpdate:
                    m_Button.interactable = true;
                    break;
                case Mode.ModesId.Error:
                    m_Button.interactable = false;
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}