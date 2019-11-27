﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class CutColor : Tool
    {
        #region Properties
        [SerializeField]
        private Dropdown m_Dropdown;

        /// <summary>
        /// Correspondance between cut color dropdown options indices and color type
        /// </summary>
        private List<Data.Enums.ColorType> m_CutColorIndices = new List<Data.Enums.ColorType>() { Data.Enums.ColorType.Default, Data.Enums.ColorType.Grayscale };
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            m_Dropdown.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                SelectedScene.CutColor = m_CutColorIndices[value];
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
            m_Dropdown.value = m_CutColorIndices.FindIndex((c) => c == SelectedScene.CutColor);
        }
        #endregion
    }
}