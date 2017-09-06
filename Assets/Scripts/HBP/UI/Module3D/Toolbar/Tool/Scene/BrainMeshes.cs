﻿using HBP.Module3D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D.Tools
{
    public class BrainMeshes : Tool
    {
        #region Properties
        [SerializeField]
        private Toggle m_Left;

        [SerializeField]
        private Toggle m_Right;
        
        public GenericEvent<SceneStatesInfo.MeshPart> OnChangeValue = new GenericEvent<SceneStatesInfo.MeshPart>();
        #endregion

        #region Private Methods
        /// <summary>
        /// Return the mesh part according to which mesh is displayed
        /// </summary>
        /// <param name="left">Is the left mesh displayed ?</param>
        /// <param name="right">Is the right mesh displayed ?</param>
        /// <returns>Mesh part enum identifier</returns>
        private SceneStatesInfo.MeshPart GetMeshPart(bool left, bool right)
        {
            if (left && right) return SceneStatesInfo.MeshPart.Both;
            if (left && !right) return SceneStatesInfo.MeshPart.Left;
            if (!left && right) return SceneStatesInfo.MeshPart.Right;
            return SceneStatesInfo.MeshPart.None;
        }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            m_Left.onValueChanged.AddListener((display) =>
            {
                if (ListenerLock) return;

                if (m_Left.isOn || m_Right.isOn)
                {
                    SceneStatesInfo.MeshPart mesh = GetMeshPart(m_Left.isOn, m_Right.isOn);
                    ApplicationState.Module3D.SelectedScene.UpdateMeshPartToDisplay(mesh);
                    OnChangeValue.Invoke(mesh);
                }
                else
                {
                    m_Left.isOn = true;
                }
            });

            m_Right.onValueChanged.AddListener((display) =>
            {
                if (ListenerLock) return;

                if (m_Left.isOn || m_Right.isOn)
                {
                    SceneStatesInfo.MeshPart mesh = GetMeshPart(m_Left.isOn, m_Right.isOn);
                    ApplicationState.Module3D.SelectedScene.UpdateMeshPartToDisplay(mesh);
                    OnChangeValue.Invoke(mesh);
                }
                else
                {
                    m_Right.isOn = true;
                }
            });
        }
        public override void DefaultState()
        {
            m_Left.isOn = false;
            m_Left.interactable = false;
            m_Right.isOn = false;
            m_Right.interactable = false;
        }
        public override void UpdateInteractable()
        {
            bool isMeshLeftRight = ApplicationState.Module3D.SelectedScene.ColumnManager.SelectedMesh is LeftRightMesh3D;
            switch (ApplicationState.Module3D.SelectedScene.ModesManager.CurrentMode.ID)
            {
                case Mode.ModesId.NoPathDefined:
                    m_Left.interactable = false;
                    m_Right.interactable = false;
                    m_Left.gameObject.SetActive(false);
                    m_Right.gameObject.SetActive(false);
                    break;
                case Mode.ModesId.MinPathDefined:
                    m_Left.interactable = isMeshLeftRight;
                    m_Right.interactable = isMeshLeftRight;
                    m_Left.gameObject.SetActive(isMeshLeftRight);
                    m_Right.gameObject.SetActive(isMeshLeftRight);
                    break;
                case Mode.ModesId.AllPathDefined:
                    m_Left.interactable = isMeshLeftRight;
                    m_Right.interactable = isMeshLeftRight;
                    m_Left.gameObject.SetActive(isMeshLeftRight);
                    m_Right.gameObject.SetActive(isMeshLeftRight);
                    break;
                case Mode.ModesId.ComputingAmplitudes:
                    m_Left.interactable = false;
                    m_Right.interactable = false;
                    m_Left.gameObject.SetActive(false);
                    m_Right.gameObject.SetActive(false);
                    break;
                case Mode.ModesId.AmplitudesComputed:
                    m_Left.interactable = isMeshLeftRight;
                    m_Right.interactable = isMeshLeftRight;
                    m_Left.gameObject.SetActive(isMeshLeftRight);
                    m_Right.gameObject.SetActive(isMeshLeftRight);
                    break;
                case Mode.ModesId.TriErasing:
                    m_Left.interactable = false;
                    m_Right.interactable = false;
                    m_Left.gameObject.SetActive(false);
                    m_Right.gameObject.SetActive(false);
                    break;
                case Mode.ModesId.ROICreation:
                    m_Left.interactable = false;
                    m_Right.interactable = false;
                    m_Left.gameObject.SetActive(false);
                    m_Right.gameObject.SetActive(false);
                    break;
                case Mode.ModesId.AmpNeedUpdate:
                    m_Left.interactable = isMeshLeftRight;
                    m_Right.interactable = isMeshLeftRight;
                    m_Left.gameObject.SetActive(isMeshLeftRight);
                    m_Right.gameObject.SetActive(isMeshLeftRight);
                    break;
                case Mode.ModesId.Error:
                    m_Left.interactable = false;
                    m_Right.interactable = false;
                    m_Left.gameObject.SetActive(false);
                    m_Right.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
        public override void UpdateStatus(Toolbar.UpdateToolbarType type)
        {
            if (type == Toolbar.UpdateToolbarType.Scene)
            {
                switch (ApplicationState.Module3D.SelectedScene.SceneInformation.MeshPartToDisplay)
                {
                    case SceneStatesInfo.MeshPart.Left:
                        m_Left.isOn = true;
                        m_Right.isOn = false;
                        break;
                    case SceneStatesInfo.MeshPart.Right:
                        m_Left.isOn = false;
                        m_Right.isOn = true;
                        break;
                    case SceneStatesInfo.MeshPart.Both:
                        m_Left.isOn = true;
                        m_Right.isOn = true;
                        break;
                    case SceneStatesInfo.MeshPart.None:
                        m_Left.isOn = false;
                        m_Right.isOn = false;
                        break;
                    default:
                        break;
                }
            }
        }
        public void ChangeBrainTypeCallback()
        {
            Base3DScene selectedScene = ApplicationState.Module3D.SelectedScene;
            if (!(selectedScene.ColumnManager.SelectedMesh is LeftRightMesh3D) && selectedScene.SceneInformation.MeshPartToDisplay != SceneStatesInfo.MeshPart.Both)
            {
                m_Left.isOn = true;
                m_Right.isOn = true;
            }
        }
        #endregion
    }
}