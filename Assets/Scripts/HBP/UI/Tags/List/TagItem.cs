﻿using HBP.Data.Tags;
using Tools.CSharp;
using Tools.Unity.Lists;
using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI.Tags
{
    public class TagItem : ActionnableItem<Tag>
    {
        #region Properties
        [SerializeField] Text m_NameText;
        [SerializeField] Text m_TypeText;

        public override Tag Object
        {
            get
            {
                return base.Object;
            }
            set
            {
                base.Object = value;
                m_NameText.text = value.Name;
                m_TypeText.text = value.GetType().GetDisplayName();
            }
        }
        #endregion
    }
}