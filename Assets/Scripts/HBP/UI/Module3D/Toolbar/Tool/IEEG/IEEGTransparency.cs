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

        public bool IsGlobal { get; set; }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            m_Slider.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                if (IsGlobal)
                {
                    foreach (HBP.Module3D.Column3DDynamic column in SelectedScene.ColumnManager.ColumnsIEEG)
                    {
                        column.DynamicParameters.AlphaMin = value;
                    }
                }
                else
                {
                    ((HBP.Module3D.Column3DDynamic)SelectedColumn).DynamicParameters.AlphaMin = value;
                    //((HBP.Module3D.Column3DIEEG)ApplicationState.Module3D.SelectedScene.ColumnManager.SelectedColumn).IEEGParameters.AlphaMax = value; // FIXME : Required / other value ?
                }
            });
        }

        public override void DefaultState()
        {
            m_Slider.value = 0.2f;
            m_Slider.interactable = false;
        }

        public override void UpdateInteractable()
        {
            bool isColumnDynamic = SelectedColumn is HBP.Module3D.Column3DDynamic;

            m_Slider.interactable = isColumnDynamic;
        }

        public override void UpdateStatus()
        {
            if (SelectedColumn is HBP.Module3D.Column3DDynamic dynamicColumn)
            {
                m_Slider.value = dynamicColumn.DynamicParameters.AlphaMin;
            }
            else
            {
                m_Slider.value = 0.2f;
            }
        }
        #endregion
    }
}