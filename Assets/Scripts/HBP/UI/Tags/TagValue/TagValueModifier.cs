﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tools.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI
{
    public class TagValueModifier : ObjectModifier<Data.BaseTagValue>
    {
        #region Properties
        [SerializeField] Dropdown m_TagDropdown;

        [SerializeField] EmptyTagValueSubModifier m_EmptyTagValueSubModifier;
        [SerializeField] BoolTagValueSubModifier m_BoolTagValueSubModifier;
        [SerializeField] FloatTagValueSubModifier m_FloatTagValueSubModifier;
        [SerializeField] IntTagValueSubModifier m_IntTagValueSubModifier;
        [SerializeField] EnumTagValueSubModifier m_EnumTagValueSubModifier;
        [SerializeField] StringTagValueSubModifier m_StringTagValueSubModifier;

        List<BaseSubModifier> m_SubModifiers;
        List<Data.BaseTagValue> m_TagValuesTemp;

        Data.BaseTag[] m_Tags = new Data.BaseTag[0];
        public Data.BaseTag[] Tags
        {
            get
            {
                return m_Tags;
            }
            set
            {
                m_Tags = value;
                if(m_Tags.Length == 0)
                {
                    m_TagDropdown.options = new List<Dropdown.OptionData> { new Dropdown.OptionData("None") };
                    m_TagDropdown.interactable = false;
                    m_OKButton.interactable = false;
                }
                else
                {
                    m_TagDropdown.options = Tags.Select(t => new Dropdown.OptionData(t.Name)).ToList();
                    m_TagDropdown.interactable = Interactable;
                    m_OKButton.interactable = Interactable;
                }
                SetFields(ItemTemp);
            }
        }

        public override bool Interactable
        {
            get
            {
                return base.Interactable;
            }
            set
            {
                base.Interactable = value;

                m_EmptyTagValueSubModifier.Interactable = value;
                m_BoolTagValueSubModifier.Interactable = value;
                m_FloatTagValueSubModifier.Interactable = value;
                m_IntTagValueSubModifier.Interactable = value;
                m_EnumTagValueSubModifier.Interactable = value;
                m_StringTagValueSubModifier.Interactable = value;
            }
        }
        #endregion

        #region Public Methods
        public override void OK()
        {
            item = ItemTemp;
            base.OK();
        }
        #endregion

        #region Protected Methods
        protected override void Initialize()
        {
            base.Initialize();

            m_TagDropdown.onValueChanged.AddListener(OnChangeTag);
            m_TagDropdown.options = Tags.Select(t => new Dropdown.OptionData(t.Name)).ToList();

            m_EmptyTagValueSubModifier.Initialize();
            m_BoolTagValueSubModifier.Initialize();
            m_FloatTagValueSubModifier.Initialize();
            m_IntTagValueSubModifier.Initialize();
            m_EnumTagValueSubModifier.Initialize();
            m_StringTagValueSubModifier.Initialize();

            m_SubModifiers = new List<BaseSubModifier>
            {
                m_EmptyTagValueSubModifier,
                m_BoolTagValueSubModifier,
                m_FloatTagValueSubModifier,
                m_IntTagValueSubModifier,
                m_EnumTagValueSubModifier,
                m_StringTagValueSubModifier
            };

            m_TagValuesTemp = new List<Data.BaseTagValue>
            {
                new Data.EmptyTagValue(),
                new Data.BoolTagValue(),
                new Data.FloatTagValue(),
                new Data.IntTagValue(),
                new Data.EnumTagValue(),
                new Data.StringTagValue()
            };
        }
        protected override void SetFields(Data.BaseTagValue objectToDisplay)
        {
            int index = m_TagValuesTemp.FindIndex(t => t.GetType() == objectToDisplay.GetType());
            m_TagValuesTemp[index] = objectToDisplay;

            m_TagDropdown.SetValue(Array.IndexOf(Tags, objectToDisplay.Tag));
        }
        protected void OnChangeTag(int value)
        {
            // Close old subModifier
            m_SubModifiers.Find(subModifier => subModifier.GetType().IsSubclassOf(typeof(SubModifier<>).MakeGenericType(itemTemp.GetType()))).IsActive = false;

            if (value >= 0 && value < Tags.Length)
            {
                Data.BaseTag tag = Tags[value];


                Data.BaseTagValue tagValue = null;
                if (tag is Data.EmptyTag emptyTag)
                {
                    tagValue = m_TagValuesTemp.Find(t => t.GetType() == typeof(Data.EmptyTagValue));
                    tagValue.Copy(itemTemp);
                    (tagValue as Data.EmptyTagValue).Tag = emptyTag;
                }
                else if (tag is Data.BoolTag boolTag)
                {
                    tagValue = m_TagValuesTemp.Find(t => t.GetType() == typeof(Data.BoolTagValue));
                    tagValue.Copy(itemTemp);
                    (tagValue as Data.BoolTagValue).Tag = boolTag;
                }
                else if (tag is Data.EnumTag enumTag)
                {
                    tagValue = m_TagValuesTemp.Find(t => t.GetType() == typeof(Data.EnumTagValue));
                    tagValue.Copy(itemTemp);
                    (tagValue as Data.EnumTagValue).Tag = enumTag;
                }
                else if (tag is Data.FloatTag floatTag)
                {
                    tagValue = m_TagValuesTemp.Find(t => t.GetType() == typeof(Data.FloatTagValue));
                    tagValue.Copy(itemTemp);
                    (tagValue as Data.FloatTagValue).Tag = floatTag;
                }
                else if (tag is Data.IntTag intTag)
                {
                    tagValue = m_TagValuesTemp.Find(t => t.GetType() == typeof(Data.IntTagValue));
                    tagValue.Copy(itemTemp);
                    (tagValue as Data.IntTagValue).Tag = intTag;
                }
                else if (tag is Data.StringTag stringTag)
                {
                    tagValue = m_TagValuesTemp.Find(t => t.GetType() == typeof(Data.StringTagValue));
                    tagValue.Copy(itemTemp);
                    (tagValue as Data.StringTagValue).Tag = stringTag;
                }

                itemTemp = tagValue;

                // Open new subModifier;
                BaseSubModifier newSubModifier = m_SubModifiers.Find(subModifier => subModifier.GetType().IsSubclassOf(typeof(SubModifier<>).MakeGenericType(tagValue.GetType())));
                newSubModifier.IsActive = true;
                newSubModifier.Object = itemTemp;
            }
        }
        #endregion
    }
}