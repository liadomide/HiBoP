﻿using HBP.Module3D;
using Tools.Unity.ResizableGrid;
using Tools.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using NewTheme.Components;
using HBP.UI.Module3D;

public class View3DUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region Properties
    private const float MINIMIZED_THRESHOLD = 10.0f;

    [SerializeField] private ThemeElement m_ThemeElement;
    [SerializeField] private State m_MoveState;
    [SerializeField] private SelectionRing m_SelectionRing;

    /// <summary>
    /// Associated logical scene 3D
    /// </summary>
    private Base3DScene m_Scene;
    /// <summary>
    /// Associated logical column 3D
    /// </summary>
    private Column3D m_Column;
    /// <summary>
    /// Associated logical view 3D
    /// </summary>
    private View3D m_View;
    /// <summary>
    /// Parent resizable grid
    /// </summary>
    public ResizableGrid ParentGrid { get; set; }
    /// <summary>
    /// GameObject to hide a minimized view
    /// </summary>
    private GameObject m_MinimizedGameObject;
    /// <summary>
    /// Render camera texture
    /// </summary>
    private RawImage m_RawImage;
    /// <summary>
    /// Rect Transform
    /// </summary>
    private RectTransform m_RectTransform;
    /// <summary>
    /// True if the pointer in on the view ui
    /// </summary>
    private bool m_PointerIsInView;

    private bool m_UsingRenderTexture;
    /// <summary>
    /// True if we are using render textures for the cameras (instead of changing the viewport)
    /// </summary>
    public bool UsingRenderTexture
    {
        get
        {
            return m_UsingRenderTexture;
        }
        set
        {
            m_UsingRenderTexture = value;
            m_RawImage.enabled = value;
            m_RectTransform.hasChanged = true;
        }
    }

    public bool IsMinimizedHorizontally
    {
        get
        {
            return Mathf.Abs(m_RectTransform.rect.width - ParentGrid.MinimumViewWidth) <= MINIMIZED_THRESHOLD;
        }
    }
    public bool IsMinimzedVertically
    {
        get
        {
            return Mathf.Abs(m_RectTransform.rect.height - ParentGrid.MinimumViewHeight) <= MINIMIZED_THRESHOLD;
        }
    }
    /// <summary>
    /// Returns true if the view is minimized but the column is not
    /// </summary>
    public bool IsViewMinimizedAndColumnNotMinimized
    {
        get
        {
            return IsMinimzedVertically && !IsMinimizedHorizontally;
        }
    }
    public bool IsMinimized
    {
        get
        {
            return IsMinimizedHorizontally || IsMinimzedVertically;
        }
    }
    #endregion

    #region Events
    /// <summary>
    /// Event called when changing the minimized state of the view
    /// </summary>
    public UnityEvent OnChangeViewSize = new UnityEvent();
    #endregion

    #region Private Methods
    private void Awake()
    {
        ParentGrid = GetComponentInParent<ResizableGrid>();
        m_RectTransform = GetComponent<RectTransform>();
        m_RawImage = GetComponent<RawImage>();
        UsingRenderTexture = true;
    }
    private void Update()
    {
        if (m_RectTransform.hasChanged)
        {
            m_MinimizedGameObject.SetActive(IsViewMinimizedAndColumnNotMinimized);
            m_View.IsMinimized = IsMinimized;

            if (m_UsingRenderTexture)
            {
                UnityEngine.Profiling.Profiler.BeginSample("RenderTexture");
                if (m_RectTransform.rect.width > 0 && m_RectTransform.rect.height > 0)
                {
                    RenderTexture renderTexture = new RenderTexture((int)m_RectTransform.rect.width, (int)m_RectTransform.rect.height, 24);
                    renderTexture.antiAliasing = 1;
                    m_View.TargetTexture = renderTexture;
                    m_View.Aspect = m_RectTransform.rect.width / m_RectTransform.rect.height;
                    m_RawImage.texture = m_View.TargetTexture;
                }
                UnityEngine.Profiling.Profiler.EndSample();
            }
            else
            {
                Rect viewport = m_RectTransform.ToScreenSpace();
                m_View.SetViewport(viewport.x, viewport.y, viewport.width, viewport.height);
            }

            OnChangeViewSize.Invoke();
            m_RectTransform.hasChanged = false;
        }
        SendRayToScene();
    }
    /// <summary>
    /// Transform the mouse position to a ray and send it to the scene
    /// </summary>
    private void SendRayToScene()
    {
        Ray ray;
        if (CursorToRay(out ray))
        {
            m_Scene.PassiveRaycastOnScene(ray, m_Column);
        }
    }
    #endregion

    #region Public Methods
    public void OnPointerDown(PointerEventData data)
    {
        if (IsMinimized) return;

        m_View.IsSelected = true;
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            m_View.DisplayRotationCircles = true;
            m_ThemeElement.Set(m_MoveState);
        }
        else
        {
            if (CursorToRay(out Ray ray))
            {
                m_Scene.ClickOnScene(ray);
            }
        }
    }
    public void OnDrag(PointerEventData data)
    {
        if (IsMinimized) return;

        switch (data.button)
        {
            case PointerEventData.InputButton.Left:
                if (m_Scene.ROICreationMode)
                {
                    m_Column.MoveSelectedROISphere(m_View.Camera, data.delta);
                }
                break;
            case PointerEventData.InputButton.Right:
                m_View.RotateCamera(data.delta);
                break;
            case PointerEventData.InputButton.Middle:
                m_View.StrafeCamera(data.delta);
                break;
            default:
                break;
        }
    }
    public void OnEndDrag(PointerEventData data)
    {
        if (IsMinimized) return;

        m_View.DisplayRotationCircles = false;
        m_ThemeElement.Set();
    }
    public void OnPointerUp(PointerEventData data)
    {
        if (IsMinimized) return;

        m_View.DisplayRotationCircles = false;
        m_ThemeElement.Set();
    }
    public void OnScroll(PointerEventData data)
    {
        if (IsMinimized) return;

        ROI selectedROI = m_Column.SelectedROI;
        if (m_Scene.ROICreationMode && selectedROI)
        {
            if (selectedROI.SelectedSphereID != -1)
            {
                selectedROI.ChangeSelectedBubbleSize(data.scrollDelta.y);
            }
            else
            {
                m_View.ZoomCamera(data.scrollDelta.y);
            }
        }
        else
        {
            m_View.ZoomCamera(data.scrollDelta.y);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        m_PointerIsInView = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        m_PointerIsInView = false;
        ApplicationState.Module3D.OnDisplaySiteInformation.Invoke(new SiteInfo(null, false, Input.mousePosition));
    }
    /// <summary>
    /// Initialize this view
    /// </summary>
    public void Initialize(Base3DScene scene, Column3D column, View3D view)
    {
        m_Scene = scene;
        m_Column = column;
        m_View = view;
        ParentGrid = GetComponentInParent<ResizableGrid>();
        m_RectTransform = GetComponent<RectTransform>();
        m_RawImage = GetComponent<RawImage>();
        UsingRenderTexture = true;

        if (!m_UsingRenderTexture)
        {
            Rect viewport = m_RectTransform.ToScreenSpace();
            m_View.SetViewport(viewport.x, viewport.y, viewport.width, viewport.height);
        }
        else
        {
            if (m_RectTransform.rect.width > 0 && m_RectTransform.rect.height > 0)
            {
                RenderTexture renderTexture = new RenderTexture((int)m_RectTransform.rect.width, (int)m_RectTransform.rect.height, 24);
                renderTexture.antiAliasing = 1;
                m_View.TargetTexture = renderTexture;
                m_View.Aspect = m_RectTransform.rect.width / m_RectTransform.rect.height;
                m_RawImage.texture = m_View.TargetTexture;
            }
        }
        
        m_MinimizedGameObject = transform.Find("MinimizedImage").gameObject;
        m_MinimizedGameObject.GetComponentInChildren<Text>().text = "View " + view.LineID;
        m_MinimizedGameObject.SetActive(false);

        m_SelectionRing.ViewCamera = view.Camera;
        m_SelectionRing.Viewport = m_RectTransform;
        m_Column.OnSelectSite.AddListener((site) => { m_SelectionRing.Site = site; });
    }
    /// <summary>
    /// Create a ray corresponding to the mouse position in the viewport of the view
    /// </summary>
    /// <param name="ray">Ray to be created</param>
    /// <returns>True if the cursor is indeed in the view</returns>
    public bool CursorToRay(out Ray ray)
    {
        if (!m_PointerIsInView)
        {
            ray = new Ray();
            return false;
        }

        Vector2 localPosition = new Vector2();
        Vector2 position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, position, null, out localPosition);
        localPosition = new Vector2((localPosition.x / m_RectTransform.rect.width) + 0.5f, (localPosition.y / m_RectTransform.rect.height) + 0.5f);
        ray = m_View.Camera.ViewportPointToRay(localPosition);
        return localPosition.x >= 0 && localPosition.x <= 1 && localPosition.y >= 0 && localPosition.y <= 1;
    }
    #endregion
}
