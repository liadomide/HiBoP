﻿using UnityEngine.UI;
using HBP.Data.Anatomy;
using Tools.Unity;
using UnityEngine;

namespace HBP.UI.Anatomy
{
    public class MeshModifier : ItemModifier<Data.Anatomy.Mesh>
{
        #region Properties
        enum Type { Single, LeftRight}
        [SerializeField] InputField m_NameInputField;
        [SerializeField] Dropdown m_TypeDropdown;
        [SerializeField] SingleMeshSubModifier m_SingleMeshSubModifier;
        [SerializeField] LeftRightMeshSubModifier m_LeftRightMeshSubModifier;
        [SerializeField] FileSelector m_TransformationFileSelector;

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
                m_SingleMeshSubModifier.Interactable = value;
                m_LeftRightMeshSubModifier.Interactable = value;
                m_TransformationFileSelector.interactable = value;
            }
        }
        #endregion

        #region Public Methods
        public override void Save()
        {
            if (m_TypeDropdown.value == (int)Type.Single)
            {
                SingleMesh mesh = (SingleMesh) ItemTemp;
                Item = new SingleMesh(mesh.Name, mesh.Transformation, mesh.ID, mesh.Path, mesh.MarsAtlasPath);
            }
            else if (m_TypeDropdown.value == (int)Type.LeftRight)
            {
                LeftRightMesh mesh = (LeftRightMesh)ItemTemp;
                Item = new LeftRightMesh(mesh.Name, mesh.Transformation, mesh.ID, mesh.LeftHemisphere, mesh.RightHemisphere, mesh.LeftMarsAtlasHemisphere, mesh.RightMarsAtlasHemisphere);
            }
            Item.RecalculateUsable();
            OnSave.Invoke();
            base.Close();
        }
        #endregion

        #region Private Methods
        protected override void SetFields(Data.Anatomy.Mesh objectToDisplay)
        {
            m_NameInputField.text = objectToDisplay.Name;
            m_TypeDropdown.Set(typeof(Type), objectToDisplay is LeftRightMesh ? 1 : 0);
            m_TransformationFileSelector.File = objectToDisplay.SavedTransformation;
        }
        protected override void Initialize()
        {
            base.Initialize();

            m_NameInputField.onValueChanged.RemoveAllListeners();
            m_NameInputField.onValueChanged.AddListener((name) => ItemTemp.Name = name);

            m_TypeDropdown.onValueChanged.RemoveAllListeners();
            m_TypeDropdown.onValueChanged.AddListener(ChangeMeshType);

            m_TransformationFileSelector.onValueChanged.RemoveAllListeners();
            m_TransformationFileSelector.onValueChanged.AddListener((path) => ItemTemp.Transformation = path);
        }
        void ChangeMeshType(int i)
        {
            if(i == 0)
            {
                if(!(ItemTemp is SingleMesh))
                {
                    LeftRightMesh leftRightMesh = ItemTemp as LeftRightMesh;
                    itemTemp = new SingleMesh(leftRightMesh.Name, leftRightMesh.Transformation, leftRightMesh.ID, leftRightMesh.LeftHemisphere, leftRightMesh.LeftMarsAtlasHemisphere);
                }
                m_LeftRightMeshSubModifier.IsActive = false;
                m_SingleMeshSubModifier.Object = ItemTemp as SingleMesh;
                m_SingleMeshSubModifier.IsActive = true;
            }
            else if(i == 1)
            {
                if(!(ItemTemp is LeftRightMesh))
                {
                    SingleMesh singleMesh = ItemTemp as SingleMesh;
                    itemTemp = new LeftRightMesh(singleMesh.Name, singleMesh.Transformation, singleMesh.ID, singleMesh.Path, singleMesh.Path, singleMesh.MarsAtlasPath, singleMesh.MarsAtlasPath);
                }
                m_SingleMeshSubModifier.IsActive = false;
                m_LeftRightMeshSubModifier.Object = ItemTemp as LeftRightMesh;
                m_LeftRightMeshSubModifier.IsActive = true;
            }
        }
        #endregion
    }
}