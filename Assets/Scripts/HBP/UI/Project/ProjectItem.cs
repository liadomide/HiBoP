﻿using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI
{
	/// <summary>
	/// Manage the project item.
	/// </summary>
	public class ProjectItem : Tools.Unity.Lists.ActionnableItem<Data.ProjectInfo> 
	{
        #region Properties
        [SerializeField] Text m_NameText;

        [SerializeField] Text m_PatientsText;
        [SerializeField] Text m_GroupsText;
        [SerializeField] Text m_ProtocolsText;
        [SerializeField] Text m_DatasetsText;
        [SerializeField] Text m_VisualizationsText;

        [SerializeField] State m_EmptyState;

        public override Data.ProjectInfo Object
        {
            get
            {
                return base.Object;
            }

            set
            {
                base.Object = value;

                m_NameText.text = value.Settings.Name;
                m_PatientsText.SetIEnumerableFieldInItem(value.Patients, m_EmptyState);
                m_GroupsText.SetIEnumerableFieldInItem(value.Groups, m_EmptyState);
                m_ProtocolsText.SetIEnumerableFieldInItem(value.Protocols, m_EmptyState);
                m_DatasetsText.SetIEnumerableFieldInItem(value.Datasets, m_EmptyState);
                m_VisualizationsText.SetIEnumerableFieldInItem(value.Visualizations, m_EmptyState);
            }
        }
        #endregion
    }
}