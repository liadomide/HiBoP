﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.Unity
{
    public class TooltipManager : MonoBehaviour
    {
        #region Properties
        public const float TIME_TO_DISPLAY = 1.5f;

        private bool m_IsTooltipDisplayed = false;
        public bool IsTooltipDisplayed
        {
            get
            {
                return m_IsTooltipDisplayed;
            }
        }

        private float m_TimeBeforeHiding = TIME_TO_DISPLAY;
        private bool m_TooltipHasBeenDisplayedRecently = false;
        public bool TooltipHasBeenDisplayedRecently
        {
            get
            {
                return m_TooltipHasBeenDisplayedRecently;
            }
        }

        /// <summary>
        /// Canvas on which the tooltip is displayed
        /// </summary>
        [SerializeField]
        private RectTransform m_Canvas;
        /// <summary>
        /// Tooltip's RectTransform
        /// </summary>
        [SerializeField]
        private RectTransform m_Tooltip;
        /// <summary>
        /// Tooltip's Textfield
        /// </summary>
        [SerializeField]
        private Text m_TextField;
        #endregion

        #region Private Methods
        private void Update()
        {
            if (!m_IsTooltipDisplayed)
            {
                m_TimeBeforeHiding -= Time.deltaTime;
            }
            if (m_TimeBeforeHiding < 0)
            {
                m_TooltipHasBeenDisplayedRecently = false;
            }
        }
        private void ClampToCanvas()
        {
            Vector3 l_pos = m_Tooltip.localPosition;
            Vector3 l_minPosition = m_Canvas.rect.min - m_Tooltip.rect.min;
            Vector3 l_maxPosition = m_Canvas.rect.max - m_Tooltip.rect.max;

            l_minPosition = new Vector3(l_minPosition.x, l_minPosition.y, l_minPosition.z);
            l_maxPosition = new Vector3(l_maxPosition.x, l_maxPosition.y, l_maxPosition.z);

            l_pos.x = Mathf.Clamp(m_Tooltip.localPosition.x, l_minPosition.x, l_maxPosition.x);
            l_pos.y = Mathf.Clamp(m_Tooltip.localPosition.y, l_minPosition.y, l_maxPosition.y);

            m_Tooltip.localPosition = l_pos;
        }
        #endregion

        #region Public Methods
        public void ShowTooltip(string text, Vector3 position)
        {
            m_Tooltip.gameObject.SetActive(true);
            m_TextField.text = text;
            m_Tooltip.position = position;
            ClampToCanvas();

            m_IsTooltipDisplayed = true;
            m_TooltipHasBeenDisplayedRecently = true;
            m_TimeBeforeHiding = TIME_TO_DISPLAY;
        }
        public void HideTooltip()
        {
            m_Tooltip.gameObject.SetActive(false);
            m_IsTooltipDisplayed = false;
        }
        #endregion
    }
}