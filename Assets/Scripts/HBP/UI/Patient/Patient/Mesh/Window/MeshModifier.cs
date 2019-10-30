﻿using UnityEngine.UI;
using Tools.Unity;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace HBP.UI.Anatomy
{
    public class MeshModifier : ItemModifier<Data.Anatomy.Mesh>
{
        #region Properties
        [SerializeField] InputField m_NameInputField;
        [SerializeField] Dropdown m_TypeDropdown;
        [SerializeField] FileSelector m_TransformationFileSelector;

        [SerializeField] SingleMeshSubModifier m_SingleMeshSubModifier;
        [SerializeField] LeftRightMeshSubModifier m_LeftRightMeshSubModifier;

        Type[] m_Types;
        List<BaseSubModifier> m_SubModifiers;
        List<Data.Anatomy.Mesh> m_MeshesTemp;

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
                m_TransformationFileSelector.interactable = value;

                m_SingleMeshSubModifier.Interactable = value;
                m_LeftRightMeshSubModifier.Interactable = value;
            }
        }
        #endregion

        #region Public Methods
        public override void Save()
        {
            item = ItemTemp;
            item.RecalculateUsable();
            base.Save();
        }
        #endregion

        #region Private Methods
        protected override void Initialize()
        {
            base.Initialize();

            m_NameInputField.onValueChanged.AddListener(OnChangeName);
            m_TransformationFileSelector.onValueChanged.AddListener(OnChangeTransformation);

            m_TypeDropdown.onValueChanged.AddListener(OnChangeType);
            m_Types = m_TypeDropdown.Set(typeof(Data.Anatomy.Mesh));

            m_SingleMeshSubModifier.Initialize();
            m_LeftRightMeshSubModifier.Initialize();

            m_SubModifiers = new List<BaseSubModifier>();
            m_SubModifiers.Add(m_SingleMeshSubModifier);
            m_SubModifiers.Add(m_LeftRightMeshSubModifier);

            m_MeshesTemp = new List<Data.Anatomy.Mesh>();
            m_MeshesTemp.Add(new Data.Anatomy.SingleMesh());
            m_MeshesTemp.Add(new Data.Anatomy.LeftRightMesh());

        }
        protected override void SetFields(Data.Anatomy.Mesh objectToDisplay)
        {
            int index = m_MeshesTemp.FindIndex(t => t.GetType() == ItemTemp.GetType());
            m_MeshesTemp[index] = ItemTemp;

            m_NameInputField.text = objectToDisplay.Name;

            m_TypeDropdown.SetValue(Array.IndexOf(m_Types, objectToDisplay.GetType()));

            m_TransformationFileSelector.File = objectToDisplay.SavedTransformation;
        }
        protected void OnChangeType(int value)
        {
            Type type = m_Types[value];

            // Close old subModifier
            m_SubModifiers.Find(subModifier => subModifier.GetType().IsSubclassOf(typeof(SubModifier<>).MakeGenericType(itemTemp.GetType()))).IsActive = false;

            Data.Anatomy.Mesh mesh = m_MeshesTemp.Find(t => t.GetType() == type);
            mesh.Copy(itemTemp);
            itemTemp = mesh;

            // Open new subModifier;
            BaseSubModifier newSubModifier = m_SubModifiers.Find(subModifier => subModifier.GetType().IsSubclassOf(typeof(SubModifier<>).MakeGenericType(type)));
            newSubModifier.IsActive = true;
            newSubModifier.Object = itemTemp;
        }
        protected void OnChangeName(string name)
        {
            ItemTemp.Name = name;
        }
        protected void OnChangeTransformation(string path)
        {
            ItemTemp.Transformation = path;
        }
        #endregion
    }
}