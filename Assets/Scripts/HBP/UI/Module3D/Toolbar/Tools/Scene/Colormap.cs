﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class Colormap : Tool
    {
        #region Properties
        /// <summary>
        /// Dropdown to select the colormap
        /// </summary>
        [SerializeField] private Dropdown m_Dropdown;
        /// <summary>
        /// Correspondance between colormap dropdown options indices and color type
        /// </summary>
        private List<Data.Enums.ColorType> m_ColormapIndices = new List<Data.Enums.ColorType>() { Data.Enums.ColorType.Grayscale, Data.Enums.ColorType.Hot, Data.Enums.ColorType.Winter, Data.Enums.ColorType.Warm, Data.Enums.ColorType.Surface, Data.Enums.ColorType.Cool, Data.Enums.ColorType.RedYellow, Data.Enums.ColorType.BlueGreen, Data.Enums.ColorType.ACTC, Data.Enums.ColorType.Bone, Data.Enums.ColorType.GEColor, Data.Enums.ColorType.Gold, Data.Enums.ColorType.XRain, Data.Enums.ColorType.MatLab };
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize the toolbar
        /// </summary>
        public override void Initialize()
        {
            m_Dropdown.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                SelectedScene.Colormap = m_ColormapIndices[value];
            });
        }
        /// <summary>
        /// Set the default state of this tool
        /// </summary>
        public override void DefaultState()
        {
            m_Dropdown.value = 13;
            m_Dropdown.interactable = false;
        }
        /// <summary>
        /// Update the interactable state of the tool
        /// </summary>
        public override void UpdateInteractable()
        {
            m_Dropdown.interactable = true;
        }
        /// <summary>
        /// Update the status of the tool
        /// </summary>
        public override void UpdateStatus()
        {
            m_Dropdown.value = m_ColormapIndices.FindIndex((c) => c == SelectedScene.Colormap);
        }
        #endregion
    }
}