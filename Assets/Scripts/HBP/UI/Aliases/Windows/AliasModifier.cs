﻿using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI.Alias
{
    public class AliasModifier : ObjectModifier<Data.Preferences.Alias>
    {
        #region Properties
        [SerializeField] InputField m_KeyInputField;
        [SerializeField] InputField m_ValueInputField;

        public override bool Interactable
        {
            get => base.Interactable;
            set
            {
                base.Interactable = value;
                m_KeyInputField.interactable = value;
                m_ValueInputField.interactable = value;
            }
        }
        #endregion

        #region Protected Methods
        protected override void Initialize()
        {
            base.Initialize();

            m_KeyInputField.onValueChanged.AddListener(key => ItemTemp.Key = key);
            m_ValueInputField.onValueChanged.AddListener(value => ItemTemp.Value = value);
        }
        protected override void SetFields(Data.Preferences.Alias objectToDisplay)
        {
            m_KeyInputField.text = objectToDisplay.Key;
            m_ValueInputField.text = objectToDisplay.Value;
        }
        #endregion
    }
}

