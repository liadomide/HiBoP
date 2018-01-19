﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace HBP.UI.TrialMatrix
{
    public class ValuesLegend : MonoBehaviour
    {
        #region Properties
        [SerializeField] RawImage m_ColorMapRawImage;
        [SerializeField] RectTransform m_ValuesRect;
        [SerializeField] LimitsWindow m_LimitsWindow;
        Vector2 m_Limits;
        public GenericEvent<Vector2> OnChangeLimits { get { return m_LimitsWindow.OnUpdateLimits; } }
        public GenericEvent<bool> OnAutoLimits { get { return m_LimitsWindow.OnAutoLimits; } }
        #endregion

        #region Public Methods
        public void Set(Texture2D colorMap, Vector2 limits, int nbValue)
        {
            this.m_Limits = limits;
            SetColorMap(colorMap);
            SetValues(GenerateValues(limits.x, limits.y, nbValue));
        }
        public void OpenLimitsWindow()
        {
            m_LimitsWindow.Open(m_Limits);
        }
        public void CloseLimitsWindow()
        {
            m_LimitsWindow.Close();
        }
        #endregion

        #region Private Methods
        void SetColorMap(Texture2D colorMap)
        {
            m_ColorMapRawImage.texture = colorMap;
        }
        void SetValues(float[] values)
        {
            ClearValues();
            for (int i = 0; i < values.Length; i++)
            {
                AddValue(values[i], i, values.Length);
            }
        }
        void ClearValues()
        {
            foreach(Transform child in m_ValuesRect)
            {
                Destroy(child.gameObject);
            }
        }
        void AddValue(float value, int position, int max)
        {
            // Instantiate and add components needed
            GameObject l_gameObject = new GameObject();
            l_gameObject.transform.SetParent(m_ValuesRect);
            RectTransform l_rect = l_gameObject.AddComponent<RectTransform>();
            Text l_text = l_gameObject.AddComponent<Text>();
            int l_max = max - 1;

            // SetText
            l_text.text = value.ToString("n2");
            l_text.color = Color.white;
            l_text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            l_text.fontStyle = FontStyle.Normal;
            l_text.fontSize = 14;
            l_text.horizontalOverflow = HorizontalWrapMode.Wrap;

            // Set pivot, text alignment and name
            if (position == 0)
            {
                l_rect.pivot = new Vector2(0.5f, 0f);
                l_text.alignment = TextAnchor.LowerLeft;
                l_gameObject.name = "Min";
            }
            else if (position == l_max)
            {
                l_rect.pivot = new Vector2(0.5f, 1f);
                l_text.alignment = TextAnchor.UpperLeft;
                l_gameObject.name = "Max";
            }
            else
            {
                l_text.alignment = TextAnchor.MiddleLeft;
                l_gameObject.name = position.ToString() + "/" + l_max.ToString();
            }

            // SetPosition
            float l_position = (float)position / l_max;
            l_rect.anchorMin = new Vector2(0, l_position);
            l_rect.anchorMax = new Vector2(1, l_position);
            l_rect.anchoredPosition = new Vector2(0, 0);
            l_rect.sizeDelta = new Vector2(0, 30);
        }
        float[] GenerateValues(float min, float max, int nbValue)
        {
            float l_size = max - min;
            float[] l_result = new float[nbValue];
            for (int i = 0; i < nbValue; i++)
            {
                l_result[i] = min + l_size *(i / (float)(nbValue-1));
            }
            return l_result;
        }
        #endregion
    }
}