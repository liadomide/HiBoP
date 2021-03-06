﻿using UnityEngine;
using Tools.Unity;

namespace HBP.UI
{
    public class SingleMeshSubModifier : SubModifier<Data.SingleMesh>
    {
        #region Properties
        [SerializeField] FileSelector m_MeshFileSelector;
        [SerializeField] FileSelector m_MarsAtlasFileSelector;

        public override bool Interactable
        {
            get { return base.Interactable; }
            set
            {
                base.Interactable = value;

                m_MeshFileSelector.interactable = value;
                m_MarsAtlasFileSelector.interactable = value;
            }
        }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            base.Initialize();
            m_MeshFileSelector.onValueChanged.AddListener((path) => Object.Path = path);
            m_MarsAtlasFileSelector.onValueChanged.AddListener((marsAtlasPath) => Object.MarsAtlasPath = marsAtlasPath);
        }
        #endregion

        #region Protected Methods
        protected override void SetFields(Data.SingleMesh objectToDisplay)
        {
            m_MeshFileSelector.File = objectToDisplay.SavedPath;
            m_MarsAtlasFileSelector.File = objectToDisplay.SavedMarsAtlasPath;
        }
        #endregion
    }
}