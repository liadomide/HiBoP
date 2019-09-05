﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class BrainColor : Tool
    {
        #region Properties
        [SerializeField]
        private Dropdown m_Dropdown;

        /// <summary>
        /// Correspondance between brain color dropdown options indices and color type
        /// </summary>
        private List<Data.Enums.ColorType> m_BrainColorIndices = new List<Data.Enums.ColorType>() { Data.Enums.ColorType.BrainColor, Data.Enums.ColorType.Default, Data.Enums.ColorType.White, Data.Enums.ColorType.Grayscale, Data.Enums.ColorType.SoftGrayscale };
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            m_Dropdown.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                SelectedScene.BrainColor = m_BrainColorIndices[value];
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
            m_Dropdown.value = m_BrainColorIndices.FindIndex((c) => c == SelectedScene.BrainColor);
        }
        #endregion
    }
}