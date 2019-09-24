﻿using HBP.Module3D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

namespace HBP.UI.Module3D
{
    public class CutParametersController : MonoBehaviour
    {
        #region Properties
        public Cut Cut { get; private set; }
        private Base3DScene m_Scene;
        private bool m_IsUIUpdating = false;
        public bool AreControlsOpen { get; set; }
        public Texture2D Texture { get { return m_Image.sprite.texture; } }

        private bool m_ClickedOnMinus = false;
        private bool m_ClickedOnPlus = false;
        private float m_TimeSinceLastUpdate = 0.0f;
        private float m_TimeBetweenTwoUpdates = 0.2f;
        private bool m_Destroyed = false;

        /// <summary>
        /// Image of the cut
        /// </summary>
        [SerializeField] private Image m_Image;
        /// <summary>
        /// Dropdown to change the axis of the cut
        /// </summary>
        [SerializeField] private Dropdown m_Orientation;
        /// <summary>
        /// Slider to change the position of the cut
        /// </summary>
        [SerializeField] private Slider m_Position;
        /// <summary>
        /// Button to slightly change the position of the cut
        /// </summary>
        [SerializeField] private Button m_PlusPosition;
        /// <summary>
        /// Button to slightly change the position of the cut
        /// </summary>
        [SerializeField] private Button m_MinusPosition;
        /// <summary>
        /// Toggle to change the flip of the cut
        /// </summary>
        [SerializeField] private Toggle m_Flip;
        /// <summary>
        /// Button to remove the cut
        /// </summary>
        [SerializeField] private Button m_Remove;
        /// <summary>
        /// Rect Transform of the custom vector editor
        /// </summary>
        [SerializeField] private RectTransform m_CustomValues;
        /// <summary>
        /// X value for the custom normal
        /// </summary>
        [SerializeField] private InputField m_CustomX;
        /// <summary>
        /// Y value for the custom normal
        /// </summary>
        [SerializeField] private InputField m_CustomY;
        /// <summary>
        /// Z value for the custom normal
        /// </summary>
        [SerializeField] private InputField m_CustomZ;
        /// <summary>
        /// Text on the MRI for the direction of the cut
        /// </summary>
        [SerializeField] private Text m_PositionTitle;
        /// <summary>
        /// Text on the MRI for the value of the cut
        /// </summary>
        [SerializeField] private Text m_PositionValue;
        /// <summary>
        /// Gameobject showing information about the position of the cut
        /// </summary>
        [SerializeField] private GameObject m_PositionInformation;

        [SerializeField] private RectTransform m_SitesRectTransform;
        /// <summary>
        /// Prefab for the sites
        /// </summary>
        [SerializeField] private GameObject m_SitePrefab;

        [SerializeField] private RectTransform m_CutLinesRectTransform;
        /// <summary>
        /// Prefab for the cut lines
        /// </summary>
        [SerializeField] private GameObject m_CutLinePrefab;
        #endregion

        #region Events
        public UnityEvent OnOpenControls = new UnityEvent();
        public UnityEvent OnCloseControls = new UnityEvent();
        #endregion

        #region Private Methods
        private void Update()
        {
            if (!Input.GetMouseButton(0))
            {
                m_ClickedOnMinus = false;
                m_ClickedOnPlus = false;
            }
            else if (m_ClickedOnPlus || m_ClickedOnMinus)
            {
                m_TimeSinceLastUpdate += Time.deltaTime;
                if (m_TimeSinceLastUpdate > m_TimeBetweenTwoUpdates)
                {
                    m_TimeSinceLastUpdate = 0;
                    if (m_ClickedOnPlus)
                    {
                        m_Position.value += 1.0f / Cut.NumberOfCuts;
                    }
                    else if (m_ClickedOnMinus)
                    {
                        m_Position.value -= 1.0f / Cut.NumberOfCuts;
                    }
                }
            }
            
        }
        private void AddListeners()
        {
            Cut.OnUpdateGUITextures.AddListener((column) =>
            {
                Destroy(m_Image.sprite);
                Texture2D texture = column.CutTextures.GUIBrainCutTextures[Cut.ID];
                m_Image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                m_Image.sprite.texture.filterMode = FilterMode.Trilinear;
                m_Image.sprite.texture.anisoLevel = 9;
            });
            m_Scene.OnUpdateCuts.AddListener(() =>
            {
                if (m_Destroyed) return;

                UpdateUI();
                ShowSites();
                DrawLines();
            });
            Cut.OnRemoveCut.AddListener(() =>
            {
                Destroy(gameObject);
                m_Destroyed = true;
            });

            m_Position.onValueChanged.AddListener((value) =>
            {
                if (m_IsUIUpdating) return;

                Cut.Position = value;
                m_Scene.UpdateCutPlane(Cut, true);
            });
            m_MinusPosition.onClick.AddListener(() =>
            {
                if (m_IsUIUpdating) return;

                m_Position.value -= 1.0f / Cut.NumberOfCuts;
                m_ClickedOnMinus = true;
            });
            m_PlusPosition.onClick.AddListener(() =>
            {
                if (m_IsUIUpdating) return;

                m_Position.value += 1.0f / Cut.NumberOfCuts;
                m_ClickedOnPlus = true;
            });
            m_Orientation.onValueChanged.AddListener((value) =>
            {
                if (m_IsUIUpdating) return;

                Cut.Orientation = (Data.Enums.CutOrientation)value;
                if (Cut.Orientation == Data.Enums.CutOrientation.Custom)
                {
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomX.text, out float x);
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomY.text, out float y);
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomZ.text, out float z);
                    Cut.Normal = new Vector3(x, y, z);
                }
                m_Scene.UpdateCutPlane(Cut, true);
            });
            m_Flip.onValueChanged.AddListener((isOn) =>
            {
                if (m_IsUIUpdating) return;

                Cut.Flip = isOn;
                Cut.Position = 1.0f - Cut.Position;
                m_Scene.UpdateCutPlane(Cut, true);
            });
            m_Remove.onClick.AddListener(() =>
            {
                if (m_IsUIUpdating) return;

                m_Scene.RemoveCutPlane(Cut);
            });
            m_CustomX.onEndEdit.AddListener((value) =>
            {
                if (m_IsUIUpdating) return;

                if (Cut.Orientation == Data.Enums.CutOrientation.Custom)
                {
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomX.text, out float x);
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomY.text, out float y);
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomZ.text, out float z);
                    Cut.Normal = new Vector3(x, y, z);
                }
                m_Scene.UpdateCutPlane(Cut, true);
            });
            m_CustomY.onEndEdit.AddListener((value) =>
            {
                if (m_IsUIUpdating) return;

                if (Cut.Orientation == Data.Enums.CutOrientation.Custom)
                {
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomX.text, out float x);
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomY.text, out float y);
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomZ.text, out float z);
                    Cut.Normal = new Vector3(x, y, z);
                }
                m_Scene.UpdateCutPlane(Cut, true);
            });
            m_CustomZ.onEndEdit.AddListener((value) =>
            {
                if (m_IsUIUpdating) return;

                if (Cut.Orientation == Data.Enums.CutOrientation.Custom)
                {
                    float x = 1, y = 0, z = 0;
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomX.text, out x);
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomY.text, out y);
                    global::Tools.CSharp.NumberExtension.TryParseFloat(m_CustomZ.text, out z);
                    Cut.Normal = new Vector3(x, y, z);
                }
                m_Scene.UpdateCutPlane(Cut, true);
            });
            m_Image.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (AreControlsOpen)
                {
                    CloseControls();
                }
                else
                {
                    OpenControls();
                }
            });
            m_Scene.OnSitesRenderingUpdated.AddListener(() =>
            {
                if (m_Destroyed) return;

                ShowSites();
            });
        }
        private void UpdateUI()
        {
            m_IsUIUpdating = true;
            if (AreControlsOpen)
            {
                m_Position.value = Cut.Position;
                m_Orientation.value = (int)Cut.Orientation;
                m_Orientation.RefreshShownValue();
                m_Flip.isOn = Cut.Flip;
                m_CustomX.text = Cut.Normal.x.ToString();
                m_CustomY.text = Cut.Normal.y.ToString();
                m_CustomZ.text = Cut.Normal.z.ToString();
                m_Remove.gameObject.SetActive(true);
                m_Orientation.gameObject.SetActive(true);
                m_Position.transform.parent.gameObject.SetActive(true);

                m_Flip.gameObject.SetActive(Cut.Orientation != Data.Enums.CutOrientation.Custom);
                m_CustomValues.gameObject.SetActive(Cut.Orientation == Data.Enums.CutOrientation.Custom);
            }
            else
            {
                m_Remove.gameObject.SetActive(false);
                m_Orientation.gameObject.SetActive(false);
                m_Position.transform.parent.gameObject.SetActive(false);
                m_Flip.gameObject.SetActive(false);
                m_CustomValues.gameObject.SetActive(false);
            }
            m_PositionInformation.SetActive(Cut.Orientation != Data.Enums.CutOrientation.Custom);
            switch (Cut.Orientation)
            {
                case Data.Enums.CutOrientation.Axial:
                    m_PositionTitle.text = "Z";
                    m_PositionValue.text = Mathf.RoundToInt(Cut.Point.z).ToString();
                    break;
                case Data.Enums.CutOrientation.Coronal:
                    m_PositionTitle.text = "Y";
                    m_PositionValue.text = Mathf.RoundToInt(Cut.Point.y).ToString();
                    break;
                case Data.Enums.CutOrientation.Sagital:
                    m_PositionTitle.text = "X";
                    m_PositionValue.text = Mathf.RoundToInt(Cut.Point.x).ToString();
                    break;
            }
            m_IsUIUpdating = false;
        }
        #endregion

        #region Public Methods
        public void Initialize(Base3DScene scene, Cut cut)
        {
            m_Scene = scene;
            Cut = cut;
            m_Image.GetComponent<global::Tools.Unity.Components.ImageRatio>().Type = global::Tools.Unity.Components.ImageRatio.ControlType.WidthControlsHeight;
            m_Orientation.options = new List<Dropdown.OptionData>();
            foreach (var orientation in Enum.GetNames(typeof(Data.Enums.CutOrientation)))
            {
                m_Orientation.options.Add(new Dropdown.OptionData(orientation));
            }
            UpdateUI();
            AddListeners();
        }
        public void OpenControls()
        {
            AreControlsOpen = true;
            UpdateUI();
            OnOpenControls.Invoke();
        }
        public void CloseControls()
        {
            AreControlsOpen = false;
            UpdateUI();
            OnCloseControls.Invoke();
        }
        public void ShowSites()
        {
            foreach (Transform child in m_SitesRectTransform) Destroy(child.gameObject);
            if (Cut.Orientation == Data.Enums.CutOrientation.Custom) return;
            
            List<Site> sites = new List<Site>();
            m_Scene.SelectedColumn.RawElectrodes.GetSitesOnPlane(Cut, 1.0f, out int[] result);
            foreach (var site in m_Scene.SelectedColumn.Sites)
            {
                if (result[site.Information.GlobalID] == 1 && site.IsActive)
                {
                    sites.Add(site);
                }
            }

            foreach (var site in sites)
            {
                Vector2 ratio = m_Scene.DLLMRIGeometryCutGeneratorList[Cut.ID].GetPositionRatioOnTexture(site.transform.localPosition);
                float horizontalRatio = 0, verticalRatio = 0;
                switch (Cut.Orientation)
                {
                    case Data.Enums.CutOrientation.Axial:
                        horizontalRatio = Cut.Flip ? 1.0f - ratio.x : ratio.x;
                        verticalRatio = ratio.y;
                        break;
                    case Data.Enums.CutOrientation.Coronal:
                        horizontalRatio = Cut.Flip ? 1.0f - ratio.x : ratio.x;
                        verticalRatio = Cut.Flip ? 1.0f - ratio.y : ratio.y;
                        break;
                    case Data.Enums.CutOrientation.Sagital:
                        horizontalRatio = Cut.Flip ? 1.0f - ratio.y : ratio.y;
                        verticalRatio = Cut.Flip ? ratio.x : 1.0f - ratio.x;
                        break;
                }

                CutSite cutSite = Instantiate(m_SitePrefab, m_SitesRectTransform).GetComponent<CutSite>();
                cutSite.Initialize(m_Scene, site, new Vector2(horizontalRatio, verticalRatio));
            }
        }
        public void DrawLines()
        {
            foreach (Transform child in m_CutLinesRectTransform) Destroy(child.gameObject);
            if (Cut.Orientation == Data.Enums.CutOrientation.Custom || !ApplicationState.UserPreferences.Visualization.Cut.ShowCutLines) return;

            HBP.Module3D.DLL.BBox boundingBox = m_Scene.DLLMRIGeometryCutGeneratorList[Cut.ID].BoundingBox;
            if (boundingBox != null)
            {
                Vector3 min = boundingBox.Min;
                Vector3 max = boundingBox.Max;

                foreach (var cut in m_Scene.Cuts)
                {
                    if (cut == Cut || cut.Orientation == Data.Enums.CutOrientation.Custom) continue;

                    List<Vector3> points = boundingBox.IntersectionSegmentBetweenTwoPlanes(Cut, cut);
                    List<Vector2> linePoints = new List<Vector2>();
                    foreach (var point in points)
                    {
                        Vector2 ratio = m_Scene.DLLMRIGeometryCutGeneratorList[Cut.ID].GetPositionRatioOnTexture(new Vector3(-point.x, point.y, point.z));
                        float horizontalRatio = 0, verticalRatio = 0;
                        switch (Cut.Orientation)
                        {
                            case Data.Enums.CutOrientation.Axial:
                                horizontalRatio = Cut.Flip ? 1.0f - ratio.x : ratio.x;
                                verticalRatio = ratio.y;
                                break;
                            case Data.Enums.CutOrientation.Coronal:
                                horizontalRatio = Cut.Flip ? 1.0f - ratio.x : ratio.x;
                                verticalRatio = Cut.Flip ? 1.0f - ratio.y : ratio.y;
                                break;
                            case Data.Enums.CutOrientation.Sagital:
                                horizontalRatio = Cut.Flip ? 1.0f - ratio.y : ratio.y;
                                verticalRatio = Cut.Flip ? ratio.x : 1.0f - ratio.x;
                                break;
                        }
                        linePoints.Add(new Vector2(horizontalRatio, verticalRatio));
                    }
                    UnityEngine.UI.Extensions.UILineRenderer lineRenderer = Instantiate(m_CutLinePrefab, m_CutLinesRectTransform).GetComponent<UnityEngine.UI.Extensions.UILineRenderer>();
                    RectTransform lineRectTransform = lineRenderer.GetComponent<RectTransform>();
                    lineRectTransform.anchorMin = Vector2.zero;
                    lineRectTransform.anchorMax = Vector2.one;
                    lineRectTransform.anchoredPosition = Vector2.zero;
                    lineRectTransform.sizeDelta = Vector2.zero;
                    lineRenderer.Points = linePoints.ToArray();
                }
            }
        }
        #endregion
    }
}