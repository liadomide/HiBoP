﻿using UnityEngine;
using UnityEngine.Events;
using UnityStandardAssets.ImageEffects;

namespace HBP.Module3D
{
    public class View3D : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Camera 3D associated to the view
        /// </summary>
        private Camera3D m_Camera3D;
        /// <summary>
        /// Physical camera component of this view
        /// </summary>
        public Camera Camera
        {
            get
            {
                return m_Camera3D.Camera;
            }
        }
        
        /// <summary>
        /// True if any view of the column this view belongs to is selected
        /// </summary>
        public bool IsColumnSelected { get; set; }

        private bool m_IsSelected = false;
        /// <summary>
        /// True if this view is the last view in which the user clicked
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return m_IsSelected;
            }
            set
            {
                m_IsSelected = value;
                if (m_IsSelected)
                {
                    OnSelectView.Invoke(this);
                }
            }
        }

        private bool m_IsClicked = false;
        /// <summary>
        /// True if this view is the last object in which the user clicked
        /// </summary>
        public bool IsClicked
        {
            get
            {
                return m_IsClicked;
            }
            set
            {
                m_IsClicked = value;
                IsSelected = value;
            }
        }

        private bool m_IsMinimized = false;
        /// <summary>
        /// True if the view is minimized
        /// </summary>
        public bool IsMinimized
        {
            get
            {
                return m_IsMinimized;
            }
            set
            {
                m_IsMinimized = value;
                if (!m_IsMinimized)
                {
                    m_Camera3D.CullingMask = m_RegularCullingMask;
                }
                else
                {
                    m_Camera3D.CullingMask = m_MinimizedCullingMask;
                }
                m_Camera3D.Camera.enabled = !m_IsMinimized;
            }
        }

        /// <summary>
        /// Line in which the view belongs
        /// </summary>
        public int LineID { get; set; }

        /// <summary>
        /// Start or stop the automatic rotation of the camera of this view
        /// </summary>
        public bool AutomaticRotation
        {
            get
            {
                return m_Camera3D.AutomaticRotation;
            }
            set
            {
                m_Camera3D.AutomaticRotation = value;
            }
        }

        /// <summary>
        /// Automatic rotation speed
        /// </summary>
        public float AutomaticRotationSpeed
        {
            get
            {
                return m_Camera3D.AutomaticRotationSpeed;
            }
            set
            {
                m_Camera3D.AutomaticRotationSpeed = value;
            }
        }

        /// <summary>
        /// Set the edge mode
        /// </summary>
        public bool EdgeMode
        {
            get
            {
                return m_Camera3D.GetComponent<EdgeDetection>().enabled;
            }
            set
            {
                m_Camera3D.GetComponent<EdgeDetection>().enabled = value;
            }
        }

        /// <summary>
        /// Layer of the view
        /// </summary>
        public string Layer { get; set; }

        /// <summary>
        /// Set the texture on which the camera renders
        /// </summary>
        /// <param name="texture">Texture to be rendered on</param>
        public RenderTexture TargetTexture
        {
            get
            {
                return m_Camera3D.Camera.targetTexture;
            }
            set
            {
                if (m_Camera3D.Camera.targetTexture)
                {
                    m_Camera3D.Camera.targetTexture.Release();
                }
                m_Camera3D.Camera.targetTexture = value;
            }
        }

        /// <summary>
        /// Aspect ration of the camera
        /// </summary>
        public float Aspect
        {
            get
            {
                return m_Camera3D.Camera.aspect;
            }
            set
            {
                m_Camera3D.Camera.aspect = value;
            }
        }

        public bool DisplayRotationCircles
        {
            get
            {
                return m_Camera3D.DisplayRotationCircles;
            }
            set
            {
                m_Camera3D.DisplayRotationCircles = value;
            }
        }

        /// <summary>
        /// Default minimized culling mask value
        /// </summary>
        private int m_MinimizedCullingMask;
        /// <summary>
        /// Default regular culling mask value
        /// </summary>
        private int m_RegularCullingMask;

        /// <summary>
        /// Event called when we select this view
        /// </summary>
        public GenericEvent<View3D> OnSelectView = new GenericEvent<View3D>();
        /// <summary>
        /// Event called when the camera is moved (rotation, strafe, zoom)
        /// </summary>
        public UnityEvent OnMoveView = new UnityEvent();
        #endregion

        #region Private Methods
        private void Awake()
        {
            m_Camera3D = transform.GetComponentInChildren<Camera3D>();
        }
        private void Start()
        {
            int layer = 0;
            layer |= 1 << LayerMask.NameToLayer(Layer);
            switch (GetComponentInParent<Base3DScene>().Type)
            {
                case SceneType.SinglePatient:
                    layer |= 1 << LayerMask.NameToLayer("Default");
                    break;
                case SceneType.MultiPatients:
                    layer |= 1 << LayerMask.NameToLayer("Default");
                    break;
                default:
                    break;
            }
            m_RegularCullingMask = layer;
            m_MinimizedCullingMask = 0;

            if (!m_IsMinimized)
            {
                m_Camera3D.CullingMask = m_RegularCullingMask;
            }
            else
            {
                m_Camera3D.CullingMask = m_MinimizedCullingMask;
            }
        }
        private void Update()
        {
            m_Camera3D.Camera.backgroundColor = IsClicked ? ApplicationState.Theme.Color.ClickedViewColor : ((IsSelected || IsColumnSelected) ? ApplicationState.Theme.Color.SelectedViewColor : ApplicationState.Theme.Color.RegularViewColor);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Synchronize the camera of this view using the camera from a reference view
        /// </summary>
        /// <param name="reference"></param>
        public void SynchronizeCamera(View3D reference)
        {
            m_Camera3D.transform.position = reference.m_Camera3D.transform.position;
            m_Camera3D.transform.rotation = reference.m_Camera3D.transform.rotation;
            m_Camera3D.Target = reference.m_Camera3D.Target;
        }
        /// <summary>
        /// Set the viewport of the camera
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(float x, float y, float width, float height)
        {
            m_Camera3D.Camera.rect = new Rect(x / Screen.width, y / Screen.height, width / Screen.width, height / Screen.height);
        }
        /// <summary>
        /// Rotate the camera around
        /// </summary>
        /// <param name="amountX">Distance</param>
        public void RotateCamera(Vector2 amount)
        {
            m_Camera3D.HorizontalRotation(amount.x);
            m_Camera3D.VerticalRotation(amount.y);
            OnMoveView.Invoke();
        }
        /// <summary>
        /// Strafe the camera
        /// </summary>
        /// <param name="amount">Distance</param>
        public void StrafeCamera(Vector2 amount)
        {
            m_Camera3D.HorizontalStrafe(amount.x);
            m_Camera3D.VerticalStrafe(amount.y);
            OnMoveView.Invoke();
        }
        /// <summary>
        /// Zoom with the camera
        /// </summary>
        /// <param name="amount">Distance</param>
        public void ZoomCamera(float amount)
        {
            m_Camera3D.Zoom(3*amount);
            OnMoveView.Invoke();
        }
        #endregion
    }
}