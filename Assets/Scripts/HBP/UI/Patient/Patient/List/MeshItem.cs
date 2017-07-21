﻿using UnityEngine;
using UnityEngine.UI;
using Tools.Unity.Lists;

namespace HBP.UI.Anatomy
{
    public class MeshItem : SavableItem<Data.Anatomy.Mesh>
    {
        #region Properties
        [SerializeField] InputField m_NameInputField;
        public override bool interactable
        {
            get
            {
                return base.interactable;
            }

            set
            {
                base.interactable = value;
                m_NameInputField.interactable = value;
            }
        }
        #endregion

        #region Public Methods
        public override void Save()
        {
            Object.Name = m_NameInputField.text;
        }
        #endregion

        #region Protected Methods
        protected override void SetObject(Data.Anatomy.Mesh objectToSet)
        {
            m_NameInputField.text = Object.Name;
        }
        #endregion
    }
}