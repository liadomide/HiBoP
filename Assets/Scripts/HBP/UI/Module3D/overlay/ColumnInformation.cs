﻿using System.Collections;
using System.Collections.Generic;
using HBP.Module3D;
using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI.Module3D
{
    public class ColumnInformation : ColumnOverlayElement
    {
        #region Properties
        [SerializeField] private Text m_Protocol;
        [SerializeField] private Text m_Bloc;
        [SerializeField] private Text m_Dataset;
        [SerializeField] private Text m_Data;
        #endregion

        #region Public Methods
        public override void Setup(Base3DScene scene, Column3D column, Column3DUI columnUI)
        {
            base.Setup(scene, column, columnUI);

            switch (column.Type)
            {
                case Data.Enums.ColumnType.Anatomic:
                    IsActive = false;
                    break;
                case Data.Enums.ColumnType.iEEG:
                    IsActive = true;
                    Column3DIEEG col = (Column3DIEEG)column;
                    m_Protocol.text = col.ColumnIEEGData.Dataset.Protocol.Name;
                    m_Bloc.text = col.ColumnIEEGData.Bloc.Name;
                    m_Dataset.text = col.ColumnIEEGData.Dataset.Name;
                    m_Data.text = col.ColumnIEEGData.DataName;
                    break;
            }
        }
        #endregion
    }
}