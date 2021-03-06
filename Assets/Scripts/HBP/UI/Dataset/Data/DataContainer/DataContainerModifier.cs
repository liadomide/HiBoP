﻿using System;
using Tools.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using container = HBP.Data.Container;

namespace HBP.UI.Experience.Dataset
{
    public class DataContainerModifier : SubModifier<container.DataContainer>
    {
        #region Properties
        [SerializeField] Dropdown m_ContainerTypeDropdown;
        [SerializeField] ElanDataContainerSubModifier m_ElanDataContainerSubModifier;
        [SerializeField] BrainVisionDataContainerSubModifier m_BrainVisionDataContainerSubModifier;
        [SerializeField] EDFDataContainerSubModifier m_EDFDataContainerSubModifier;
        [SerializeField] MicromedDataContainerSubModifier m_MicromedDataContainerSubModifier;

        Type[] m_Types;
        container.Elan m_ElanDataContainerTemp;
        container.EDF m_EDFDataContainerTemp;
        container.Micromed m_MicromedDataContainerTemp;
        container.BrainVision m_BrainVisionDataContainerTemp;

        DataAttribute m_DataAttribute;
        public DataAttribute DataAttribute
        {
            get
            {
                return m_DataAttribute;
            }
            set
            {
                m_DataAttribute = value;
                m_Types = m_ContainerTypeDropdown.Set(typeof(container.DataContainer), m_DataAttribute);
            }
        }

        public UnityEvent OnChangeDataType { get; } = new UnityEvent();

        public override bool Interactable
        {
            get
            {
                return base.Interactable;
            }
            set
            {
                base.Interactable = value;

                m_ContainerTypeDropdown.interactable = value;
                m_ElanDataContainerSubModifier.Interactable = value;
                m_EDFDataContainerSubModifier.Interactable = value;
                m_BrainVisionDataContainerSubModifier.Interactable = value;
                m_MicromedDataContainerSubModifier.Interactable = value;
            }
        }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            base.Initialize();
            m_ContainerTypeDropdown.onValueChanged.AddListener(ChangeDataInfoType);
        }
        #endregion

        #region Private Methods
        void ChangeDataInfoType(int i)
        {
            Type type = m_Types[i];
            if (type == typeof(container.Elan))
            {
                m_ElanDataContainerSubModifier.IsActive = true;
                m_BrainVisionDataContainerSubModifier.IsActive = false;
                m_EDFDataContainerSubModifier.IsActive = false;
                m_MicromedDataContainerSubModifier.IsActive = false;

                m_ElanDataContainerSubModifier.Object = m_ElanDataContainerTemp;
                m_Object = m_ElanDataContainerTemp;
            }
            else if (type == typeof(container.BrainVision))
            {
                m_ElanDataContainerSubModifier.IsActive = false;
                m_BrainVisionDataContainerSubModifier.IsActive = true;
                m_EDFDataContainerSubModifier.IsActive = false;
                m_MicromedDataContainerSubModifier.IsActive = false;

                m_BrainVisionDataContainerSubModifier.Object = m_BrainVisionDataContainerTemp;
                m_Object = m_BrainVisionDataContainerTemp;
            }
            else if (type == typeof(container.EDF))
            {
                m_ElanDataContainerSubModifier.IsActive = false;
                m_BrainVisionDataContainerSubModifier.IsActive = false;
                m_EDFDataContainerSubModifier.IsActive = true;
                m_MicromedDataContainerSubModifier.IsActive = false;

                m_EDFDataContainerSubModifier.Object = m_EDFDataContainerTemp;
                m_Object = m_EDFDataContainerTemp;
            }
            else if (type == typeof(container.Micromed))
            {
                m_ElanDataContainerSubModifier.IsActive = false;
                m_BrainVisionDataContainerSubModifier.IsActive = false;
                m_EDFDataContainerSubModifier.IsActive = false;
                m_MicromedDataContainerSubModifier.IsActive = true;

                m_MicromedDataContainerSubModifier.Object = m_MicromedDataContainerTemp;
                m_Object = m_MicromedDataContainerTemp;
            }
            else
            {
                m_ElanDataContainerSubModifier.IsActive = false;
                m_BrainVisionDataContainerSubModifier.IsActive = false;
                m_EDFDataContainerSubModifier.IsActive = false;
                m_MicromedDataContainerSubModifier.IsActive = false;
            }
            OnChangeDataType.Invoke();
        }
        #endregion

        #region Protected Methods
        protected override void SetFields(container.DataContainer objectToDisplay)
        {
            m_ElanDataContainerTemp = new container.Elan("", "", "", objectToDisplay.ID);
            m_EDFDataContainerTemp = new container.EDF("", objectToDisplay.ID);
            m_BrainVisionDataContainerTemp = new container.BrainVision("", objectToDisplay.ID);
            m_MicromedDataContainerTemp = new container.Micromed("", objectToDisplay.ID);

            if (objectToDisplay is container.Elan)
            {
                m_ElanDataContainerTemp = objectToDisplay as container.Elan;
            }
            else if (objectToDisplay is container.EDF)
            {
                m_EDFDataContainerTemp = objectToDisplay as container.EDF;
            }
            else if (objectToDisplay is container.Micromed)
            {
                m_MicromedDataContainerTemp = objectToDisplay as container.Micromed;
            }
            else if (objectToDisplay is container.BrainVision)
            {
                m_BrainVisionDataContainerTemp = objectToDisplay as container.BrainVision;
            }
            m_ContainerTypeDropdown.SetValue(Array.IndexOf(m_Types, Object.GetType()));
        }
        #endregion
    }
}