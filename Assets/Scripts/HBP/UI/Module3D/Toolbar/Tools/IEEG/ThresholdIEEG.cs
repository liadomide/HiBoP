﻿using HBP.Module3D;
using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class ThresholdIEEG : Tool
    {
        #region Properties
        /// <summary>
        /// Button to open the threshold iEEG panel
        /// </summary>
        [SerializeField] private Button m_Button;
        /// <summary>
        /// Button to set the values automatically
        /// </summary>
        [SerializeField] private Button m_Auto;
        /// <summary>
        /// Module to handle the threshold iEEG
        /// </summary>
        [SerializeField] private Module3D.ThresholdIEEG m_ThresholdIEEG;
        /// <summary>
        /// Are the changes applied to all columns ?
        /// </summary>
        public bool IsGlobal { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize the toolbar
        /// </summary>
        public override void Initialize()
        {
            m_ThresholdIEEG.Initialize();
            m_ThresholdIEEG.OnChangeValues.AddListener((min, mid, max) =>
            {
                if (ListenerLock) return;

                foreach (var column in GetColumnsDependingOnTypeAndGlobal(IsGlobal))
                {
                    column.DynamicParameters.SetSpanValues(min, mid, max);
                }
            });
            m_Auto.onClick.AddListener(() =>
            {
                if (ListenerLock) return;

                foreach (var column in GetColumnsDependingOnTypeAndGlobal(IsGlobal))
                {
                    column.DynamicParameters.ResetSpanValues(column);
                    m_ThresholdIEEG.UpdateIEEGValues(column);
                }
            });
        }
        /// <summary>
        /// Set the default state of this tool
        /// </summary>
        public override void DefaultState()
        {
            m_Button.interactable = false;
        }
        /// <summary>
        /// Update the interactable state of the tool
        /// </summary>
        public override void UpdateInteractable()
        {
            bool isColumnIEEG = SelectedColumn is Column3DIEEG;
            bool isColumnCCEPAndSourceSelected = SelectedColumn is HBP.Module3D.Column3DCCEP ccepColumn && ccepColumn.IsSourceSelected;

            m_Button.interactable = isColumnIEEG || isColumnCCEPAndSourceSelected;
        }
        /// <summary>
        /// Update the status of the tool
        /// </summary>
        public override void UpdateStatus()
        {
            if (SelectedColumn is Column3DDynamic dynamicColumn)
            {
                m_ThresholdIEEG.UpdateIEEGValues(dynamicColumn);
            }
        }
        #endregion
    }
}