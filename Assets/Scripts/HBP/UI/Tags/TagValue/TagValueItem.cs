﻿using Tools.Unity.Lists;
using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI
{
    public class TagValueItem : ActionnableItem<Data.BaseTagValue>
    {
        #region Properties
        [SerializeField] Text m_NameText;
        [SerializeField] Text m_ValueText;

        public override Data.BaseTagValue Object
        {
            get
            {
                return base.Object;
            }
            set
            {
                base.Object = value;
                m_NameText.text = value.Tag.Name;
                m_ValueText.text = value.DisplayableValue;
            }
        }
        #endregion
    }
}