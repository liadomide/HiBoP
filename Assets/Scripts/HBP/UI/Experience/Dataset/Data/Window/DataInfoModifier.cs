﻿using UnityEngine.UI;
using HBP.Data.Experience.Dataset;
using Tools.Unity;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System;

namespace HBP.UI.Experience.Dataset
{
    public class DataInfoModifier : ItemModifier<DataInfo>
    {
        #region Properties
        public UnityEvent OnCanSave { get; set; } = new UnityEvent();
        public bool CanSave { get; set; }

        iEEGDataInfo m_IEEGDataInfoTemp;
        CCEPDataInfo m_CCEPDataInfoTemp;
        public new DataInfo ItemTemp { get { return itemTemp; } }
        Type[] m_Types; 
        [SerializeField] InputField m_NameInputField;
        [SerializeField] PatientDataInfoSubModifier m_PatientDataInfoGestion;
        [SerializeField] Dropdown m_TypeDropdown;
        [SerializeField] DataContainerModifier m_DataContainerModifier;
        [SerializeField] iEEGDataInfoSubModifier m_iEEGDataInfoSubModifier;
        [SerializeField] CCEPDataInfoSubModifier m_CCEPDataInfoSubModifier;

        public override bool Interactable
        {
            get
            {
                return base.Interactable;
            }
            set
            {
                base.Interactable = value;
                m_NameInputField.interactable = value;
                m_TypeDropdown.interactable = value;
                m_PatientDataInfoGestion.Interactable = value;
                m_iEEGDataInfoSubModifier.Interactable = value;
                m_CCEPDataInfoSubModifier.Interactable = value;
                m_DataContainerModifier.Interactable = value;
            }
        }
        #endregion

        #region Private Methods
        public override void Save()
        {
            OnCanSave.Invoke();
            if (CanSave)
            {
                Item = ItemTemp;
                OnSave.Invoke();
                base.Close();
            }
            else ApplicationState.DialogBoxManager.Open(DialogBoxManager.AlertType.Warning, "Data already exists", "A data for this patient with the same name already exists.");
        }
        protected override void SetFields(DataInfo objectToDisplay)
        {
            if (objectToDisplay is iEEGDataInfo iEEGDataInfo)
            {
                m_CCEPDataInfoTemp = new CCEPDataInfo(iEEGDataInfo.Name, iEEGDataInfo.DataContainer, iEEGDataInfo.Patient, "", iEEGDataInfo.ID);
                m_IEEGDataInfoTemp = iEEGDataInfo;
            }
            else if(objectToDisplay is CCEPDataInfo CCEPDataInfo)
            {
                m_CCEPDataInfoTemp = CCEPDataInfo;
                m_IEEGDataInfoTemp = new iEEGDataInfo(CCEPDataInfo.Name, CCEPDataInfo.DataContainer, CCEPDataInfo.Patient, iEEGDataInfo.NormalizationType.Auto, CCEPDataInfo.ID);
            }
            else if (objectToDisplay is PatientDataInfo patientDataInfo)
            {
                m_IEEGDataInfoTemp = new iEEGDataInfo(patientDataInfo.Name, new Data.Container.Elan(), patientDataInfo.Patient, iEEGDataInfo.NormalizationType.Auto, patientDataInfo.ID);
                m_CCEPDataInfoTemp = new CCEPDataInfo(patientDataInfo.Name, new Data.Container.Elan(), patientDataInfo.Patient, "", patientDataInfo.ID);
            }
            else
            {
                m_IEEGDataInfoTemp = new iEEGDataInfo(objectToDisplay.Name, new Data.Container.Elan(), ApplicationState.ProjectLoaded.Patients.FirstOrDefault(), iEEGDataInfo.NormalizationType.Auto, objectToDisplay.ID);
                m_CCEPDataInfoTemp = new CCEPDataInfo(objectToDisplay.Name, new Data.Container.Elan(), ApplicationState.ProjectLoaded.Patients.FirstOrDefault(), "", objectToDisplay.ID);
            }

            m_NameInputField.text = objectToDisplay.Name;
            m_TypeDropdown.SetValue(Array.IndexOf(m_Types, objectToDisplay.GetType()));
        }
        protected override void Initialize()
        {
            m_NameInputField.onValueChanged.AddListener((name) => ItemTemp.Name = name);
            m_TypeDropdown.onValueChanged.AddListener(ChangeDataInfoType);
            m_DataContainerModifier.OnChangeDataType.AddListener(OnChangeDataContainerType);
            m_Types = m_TypeDropdown.Set(typeof(DataInfo));
            base.Initialize();
        }
        void ChangeDataInfoType(int value)
        {
            Type type = m_Types[value];
            if(type == typeof(iEEGDataInfo))
            {
                if (itemTemp is CCEPDataInfo ccepDataInfo)
                {
                    m_IEEGDataInfoTemp.Name = ccepDataInfo.Name;
                    m_IEEGDataInfoTemp.Patient = ccepDataInfo.Patient;
                    m_IEEGDataInfoTemp.DataContainer = ccepDataInfo.DataContainer;
                }

                m_PatientDataInfoGestion.Object = m_IEEGDataInfoTemp;
                m_PatientDataInfoGestion.IsActive = true;

                m_iEEGDataInfoSubModifier.Object = m_IEEGDataInfoTemp;
                m_iEEGDataInfoSubModifier.IsActive = true;
                m_CCEPDataInfoSubModifier.IsActive = false;

                m_DataContainerModifier.DataAttribute = new iEEG();
                m_DataContainerModifier.Object = m_IEEGDataInfoTemp.DataContainer;

                itemTemp = m_IEEGDataInfoTemp;
            }
            else if(type == typeof(CCEPDataInfo))
            {
                if (itemTemp is iEEGDataInfo ieegDataInfo)
                {
                    m_CCEPDataInfoTemp.Name = ieegDataInfo.Name;
                    m_CCEPDataInfoTemp.Patient = ieegDataInfo.Patient;
                    m_CCEPDataInfoTemp.DataContainer = ieegDataInfo.DataContainer;
                }
                m_PatientDataInfoGestion.Object = m_CCEPDataInfoTemp;
                m_PatientDataInfoGestion.IsActive = true;

                m_CCEPDataInfoSubModifier.Object = m_CCEPDataInfoTemp;
                m_CCEPDataInfoSubModifier.IsActive = true;
                m_iEEGDataInfoSubModifier.IsActive = false;

                m_DataContainerModifier.DataAttribute = new CCEP();
                m_DataContainerModifier.Object = m_CCEPDataInfoTemp.DataContainer;

                itemTemp = m_CCEPDataInfoTemp;
            }
            else
            {
                m_PatientDataInfoGestion.IsActive = false;
                m_CCEPDataInfoSubModifier.IsActive = false;
                m_iEEGDataInfoSubModifier.IsActive = false;
            }
        }
        void OnChangeDataContainerType()
        {
            ItemTemp.DataContainer = m_DataContainerModifier.Object;
        }
        #endregion
    }
}