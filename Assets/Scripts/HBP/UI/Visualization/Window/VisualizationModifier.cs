﻿using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using HBP.Data;
using HBP.UI.Anatomy;
using HBP.Data.Visualization;

namespace HBP.UI.Visualization
{
    public class VisualizationModifier : ObjectModifier<Data.Visualization.Visualization>
    {
        #region Properties
        [SerializeField] InputField m_NameInputField;

        [SerializeField] PatientList m_PatientList;
        [SerializeField] Button m_AddPatientButton, m_RemovePatientButton, m_AddGroupButton, m_RemoveGroupButton;

        [SerializeField] TabManager m_TabGestion;
        [SerializeField] ColumnModifier m_ColumnModifier;

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

                m_AddPatientButton.interactable = value;
                m_RemovePatientButton.interactable = value;
                m_AddGroupButton.interactable = value;
                m_RemoveGroupButton.interactable = value;
                m_PatientList.Interactable = false;

                m_ColumnModifier.Interactable = value;
            }
        }
        #endregion

        #region Public Methods
        public override void Save()
        {
            if (Item.IsOpen)
            {
                ApplicationState.DialogBoxManager.Open(Tools.Unity.DialogBoxManager.AlertType.WarningMultiOptions, "Visualization already open", "The visualization you are trying to modify is already open. This visualization needs to be closed before saving the changes.\n\nWould you like to close it and save the changes ?", () =>
                {
                    ApplicationState.Module3D.RemoveScene(Item);
                    base.Save();
                },
                "Close & Save");
            }
            else
            {
                base.Save();
            }
        }
        public void AddPatients()
        {
            ObjectSelector<Patient> selector = ApplicationState.WindowsManager.OpenSelector(ApplicationState.ProjectLoaded.Patients.Where(p => !itemTemp.Patients.Contains(p)));
            selector.OnSave.AddListener(() => m_PatientList.Add(selector.ObjectsSelected));
            WindowsReferencer.Add(selector);
        }
        public void RemovePatients()
        {
            m_PatientList.Remove(m_PatientList.ObjectsSelected);
        }
        public void AddGroups()
        {
            ObjectSelector<Group> selector = ApplicationState.WindowsManager.OpenSelector(ApplicationState.ProjectLoaded.Groups);
            selector.OnSave.AddListener(() => AddGroups(selector.ObjectsSelected));
            WindowsReferencer.Add(selector);
        }
        public void RemoveGroups()
        {
            ObjectSelector<Group> selector = ApplicationState.WindowsManager.OpenSelector(ApplicationState.ProjectLoaded.Groups);
            selector.OnSave.AddListener(() => RemoveGroups(selector.ObjectsSelected));
            WindowsReferencer.Add(selector);
        }

        public void AddColumn()
        {
            Column column = new IEEGColumn("Column n°"+(ItemTemp.Columns.Count + 1), new BaseConfiguration(), ItemTemp.Patients);
            ItemTemp.Columns.Add(column);
            m_TabGestion.AddTab(column.Name, -1 , true);
            m_ColumnModifier.Object = column;
        }
        public void RemoveColumn()
        {
            int index = m_TabGestion.ActiveTabIndex;
            ItemTemp.Columns.RemoveAt(index);
            m_TabGestion.RemoveTab(index);
        }
        #endregion

        #region Private Methods
        protected override void Initialize()
        {
            base.Initialize();

            // General.
            m_NameInputField.onEndEdit.AddListener((value) => ItemTemp.Name = value);

            // Patients.
            m_PatientList.OnAddObject.AddListener(OnAddPatient);
            m_PatientList.OnRemoveObject.AddListener(OnRemovePatient);

            // Tabs.
            m_TabGestion.OnSwapColumns.AddListener((column1, column2) => ItemTemp.SwapColumns(column1, column2));
            m_TabGestion.OnActiveTabChanged.AddListener(SelectColumn);

            // Column Modifier.
            m_ColumnModifier.OnChangeName.AddListener(m_TabGestion.ChangeTabTitle);
            m_ColumnModifier.OnChangeColumn.AddListener(OnChangeColumnHandler);
        }
        protected override void SetFields(Data.Visualization.Visualization objectToDisplay)
        {
            m_NameInputField.text = ItemTemp.Name;

            m_PatientList.Set(itemTemp.Patients);

            if (objectToDisplay.Columns.Count > 0)
            {
                for (int i = 0; i < objectToDisplay.Columns.Count; i++)
                {
                    m_TabGestion.AddTab(objectToDisplay.Columns[i].Name);
                }
                m_TabGestion.ActiveTabIndex = 0;
            }
        }
        protected void AddGroups(IEnumerable<Group> groups)
        {
            m_PatientList.Add(groups.SelectMany(g => g.Patients).Distinct().Where(p => !m_PatientList.Objects.Contains(p)));
        }
        protected void RemoveGroups(IEnumerable<Group> groups)
        {
            m_PatientList.Remove(groups.SelectMany(g => g.Patients).Distinct().Where(p => m_PatientList.Objects.Contains(p)));
        }
        protected void SelectColumn()
        {
            int index = m_TabGestion.ActiveTabIndex;
            if (index >= 0)
            {
                if (!m_ColumnModifier.gameObject.activeSelf)
                {
                    m_ColumnModifier.gameObject.SetActive(true);
                }
                m_ColumnModifier.Patients = ItemTemp.Patients.ToArray();
                m_ColumnModifier.Object = ItemTemp.Columns[index];
            }
            else
            {
                if (m_ColumnModifier.gameObject.activeSelf)
                {
                    m_ColumnModifier.gameObject.SetActive(false);
                }
            }
        }
        protected void OnChangeColumnHandler(Column column)
        {
            if(ItemTemp != null)
            {
                ItemTemp.Columns[m_TabGestion.ActiveTabIndex] = column;
            }
        }
        protected void OnRemovePatient(Patient patient)
        {
            itemTemp.RemovePatient(patient);
            SelectColumn();
        }
        protected void OnAddPatient(Patient patient)
        {
            itemTemp.AddPatient(patient);
            SelectColumn();
        }
        #endregion
    }
}