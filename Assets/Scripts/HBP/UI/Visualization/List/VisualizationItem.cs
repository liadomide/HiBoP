﻿using UnityEngine;
using UnityEngine.UI;
using Tools.Unity.Lists;
using System.Linq;

namespace HBP.UI.Visualization
{
    public class VisualizationItem : ActionnableItem<Data.Visualization.Visualization>
    {
        #region Properties
        [SerializeField] Text m_NameText;
        [SerializeField] Text m_PatientsText;
        [SerializeField] Text m_ColumnsText;

        [SerializeField] State m_ErrorState;

        public override Data.Visualization.Visualization Object
        {
            get
            {
                return base.Object;
            }

            set
            {
                base.Object = value;

                m_NameText.text = value.Name;
                m_PatientsText.SetIEnumerableFieldInItem("Patients", value.Patients.Select(p => p.Name), m_ErrorState);
                m_ColumnsText.SetIEnumerableFieldInItem("Columns", value.Columns.Select(c => c.Name), m_ErrorState);
            }
        }
        #endregion
    }
}