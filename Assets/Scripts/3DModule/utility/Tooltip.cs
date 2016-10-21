﻿
/**
 * \file    Tooltip.cs
 * \author  Lance Florian
 * \date    27/04/2016
 * \brief   Define Tooltip
 */


// system
using System;
using System.Collections;
using System.Collections.Generic;

// unity
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace HBP.VISU3D
{

    /// <summary>
    /// Define a tooltip to display extra info when cursor is moving on the gameobject
    /// </summary>
    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string m_text = "";
        public float m_timeBeforeDisplay = 1f;

        private float m_time = 0f;
        private bool m_timerStarted = false;
        private bool m_tooltipDisplayed = false;
        private GameObject m_infoPanel = null;

        private Vector3 m_mousePosition;
        public int m_height = 50;
        public int m_width = 100;

        void Start()
        {
            m_infoPanel = StaticVisuComponents.CanvasOverlay.Find("others").Find("tooltip panel").gameObject;
        }

        void Update()
        {
            if (!m_timerStarted && !m_tooltipDisplayed)
                return;

            if (Input.mousePosition != m_mousePosition)
            {
                resetTimer();

                if (m_tooltipDisplayed)
                {
                    displayInfoState(false, new Vector3(0, 0, 0));
                }

                return;
            }

            if (Time.time - m_time > m_timeBeforeDisplay)
            {
                displayInfoState(true, Input.mousePosition + new Vector3(5, 5, 0));
            }
        }


        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            resetTimer();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            displayInfoState(false, new Vector3(0, 0, 0));
        }

        private void resetTimer()
        {
            m_time = Time.time;
            m_timerStarted = true;
            m_mousePosition = Input.mousePosition;            
        }

        void displayInfoState(bool state, Vector3 mousePosition)
        {
            m_tooltipDisplayed = state;
            m_infoPanel.SetActive(m_tooltipDisplayed);

            if (m_tooltipDisplayed)
            {
                m_infoPanel.transform.Find("info text").GetComponent<Text>().text = m_text;
                m_infoPanel.transform.Find("info text").GetComponent<RectTransform>().sizeDelta = new Vector3(m_width, m_height);
                m_infoPanel.transform.position = mousePosition;
                m_infoPanel.GetComponent<RectTransform>().sizeDelta = new Vector3(m_width, m_height);
            }

            m_timerStarted = false;
        }
    }
}