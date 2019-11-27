﻿using HBP.Module3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class TriangleErasingMode : Tool
    {
        #region Properties
        [SerializeField]
        private Dropdown m_Dropdown;
        [SerializeField]
        private RectTransform m_InputFieldParent;
        [SerializeField]
        private InputField m_InputField;
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            m_Dropdown.onValueChanged.AddListener((value) =>
            {
                if (ListenerLock) return;

                Data.Enums.TriEraserMode mode = (Data.Enums.TriEraserMode)value;
                SelectedScene.TriangleEraser.CurrentMode = mode;
                UpdateInteractable();
            });

            m_InputField.onEndEdit.AddListener((value) =>
            {
                if (ListenerLock) return;

                int degrees = 30;
                int.TryParse(value, out degrees);
                SelectedScene.TriangleEraser.Degrees = degrees;
                m_InputField.text = degrees.ToString();
            });
        }
        public override void DefaultState()
        {
            m_Dropdown.interactable = false;
            m_Dropdown.value = 0;
            m_InputField.interactable = false;
            m_InputField.text = "30";
            m_InputFieldParent.gameObject.SetActive(false);
        }
        public override void UpdateInteractable()
        {
            bool isZoneModeEnabled = SelectedScene.TriangleEraser.CurrentMode == Data.Enums.TriEraserMode.Zone;

            m_InputFieldParent.gameObject.SetActive(isZoneModeEnabled);
            m_InputField.gameObject.SetActive(isZoneModeEnabled);
            m_Dropdown.interactable = true;
            m_InputField.interactable = isZoneModeEnabled;
        }
        public override void UpdateStatus()
        {
            m_Dropdown.value = (int)SelectedScene.TriangleEraser.CurrentMode;
            m_InputField.text = SelectedScene.TriangleEraser.Degrees.ToString();
            if (SelectedScene.TriangleEraser.CurrentMode != Data.Enums.TriEraserMode.Zone)
            {
                m_InputField.gameObject.SetActive(false);
                m_InputFieldParent.gameObject.SetActive(false);
            }
            else
            {
                m_InputField.gameObject.SetActive(true);
                m_InputFieldParent.gameObject.SetActive(true);
            }
        }
        #endregion
    }
}