﻿using Tools.Unity;
using UnityEngine;
using container = HBP.Data.Container;

namespace HBP.UI.Experience.Dataset
{
    public class ElanDataContainerSubModifier : SubModifier<container.Elan>
    {
        #region Properties
        [SerializeField] FileSelector m_EEGFileSelector, m_POSFileSelector, m_NotesFileSelector;

        public override bool Interactable
        {
            get
            {
                return base.Interactable;
            }
            set
            {
                base.Interactable = value;
                m_EEGFileSelector.interactable = value;
                m_POSFileSelector.interactable = value;
                m_NotesFileSelector.interactable = value;
            }
        }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            base.Initialize();

            m_EEGFileSelector.onValueChanged.AddListener((eeg) => { Object.EEG = eeg; });
            m_POSFileSelector.onValueChanged.AddListener((pos) => { Object.POS = pos; });
            m_NotesFileSelector.onValueChanged.AddListener((notes) => { Object.Notes = notes; });
        }
        #endregion

        #region Protected Methods
        protected override void SetFields(container.Elan objectToDisplay)
        {
            m_EEGFileSelector.File = objectToDisplay.SavedEEG;
            m_POSFileSelector.File = objectToDisplay.SavedPOS;
            m_NotesFileSelector.File = objectToDisplay.SavedNotes;
        }
        #endregion
    }
}