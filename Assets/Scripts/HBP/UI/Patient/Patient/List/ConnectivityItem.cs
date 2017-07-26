﻿using UnityEngine;
using UnityEngine.UI;
using Tools.Unity.Lists;
using HBP.Data.Anatomy;

namespace HBP.UI.Anatomy
{
    public class ConnectivityItem : SavableItem<Data.Anatomy.Connectivity>
    {
        #region Properties
        [SerializeField] InputField m_NameInputField;
        [SerializeField] Tools.Unity.FileSelector m_FileSelector;
        public override Connectivity Object
        {
            get
            {
                return base.Object;
            }

            set
            {
                base.Object = value;
                m_NameInputField.text = value.Name;
                m_FileSelector.File = value.Path;
            }
        }
        #endregion

        #region Public Methods
        public override void Save()
        {
            Object.Name = m_NameInputField.text;
            Object.Path = m_FileSelector.File;
        }
        #endregion
    }
}