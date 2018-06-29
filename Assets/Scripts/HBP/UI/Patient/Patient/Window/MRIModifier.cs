﻿using Tools.Unity;
using UnityEngine;
using UnityEngine.UI;
using HBP.Data.Anatomy;

namespace HBP.UI.Anatomy
{
    public class MRIModifier : ItemModifier<MRI>
    {
        #region Properties
        [SerializeField] InputField m_NameInputField;
        [SerializeField] FileSelector m_FileSelector;
        #endregion

        #region Private Methods
        protected override void SetFields(MRI objectToDisplay)
        {
            m_NameInputField.text = objectToDisplay.Name;
            m_FileSelector.File = objectToDisplay.File;
        }
        protected override void SetInteractable(bool interactable)
        {
            m_NameInputField.interactable = interactable;
            m_FileSelector.interactable = interactable;
        }
        protected override void Initialize()
        {
            m_NameInputField.onValueChanged.RemoveAllListeners();
            m_NameInputField.onValueChanged.AddListener((name) => ItemTemp.Name = name);

            m_FileSelector.onValueChanged.RemoveAllListeners();
            m_FileSelector.onValueChanged.AddListener((file) => ItemTemp.File = file);
        }
        #endregion
    }
}