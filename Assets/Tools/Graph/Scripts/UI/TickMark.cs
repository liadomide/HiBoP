﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Tools.Unity.Graph
{
    public class TickMark : MonoBehaviour
    {
        #region Properties
        [SerializeField] protected RectTransform m_ImageRectTransform;

        [SerializeField] protected float m_Lenght = 12.0f;
        public float Lenght
        {
            get
            {
                return m_Lenght;
            }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_Lenght, value))
                {
                    SetLenghtThicknessDirection();
                }
            }
        }

        [SerializeField] protected float m_Thickness = 2.0f;
        public float Thickness
        {
            get
            {
                return m_Thickness;
            }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_Thickness, value))
                {
                    SetLenghtThicknessDirection();
                }
            }
        }

        [SerializeField] protected Axe.DirectionEnum m_Direction;
        public Axe.DirectionEnum Direction
        {
            get
            {
                return m_Direction;
            }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_Direction, value))
                {
                    SetLenghtThicknessDirection();
                }
            }
        }

        [SerializeField] protected Color m_Color;
        public Color Color
        {
            get
            {
                return m_Color;
            }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_Color, value))
                {
                    SetColor();
                }
            }
        }

        [SerializeField,Range(0,1)] protected float m_Position;
        public float Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                if(SetPropertyUtility.SetStruct(ref m_Position, value))
                {
                    m_Position = Mathf.Clamp(m_Position, 0, 1);
                    SetPosition();
                }
            }
        }
        #endregion

        #region Setters
        protected virtual void OnValidate()
        {
            SetLenghtThicknessDirection();
            SetColor();
            SetPosition();
        }
        protected virtual void SetLenghtThicknessDirection()
        {
            RectTransform rectTransform = transform as RectTransform;
            switch (m_Direction)
            {
                case Axe.DirectionEnum.LeftToRight:
                case Axe.DirectionEnum.RightToLeft:
                    m_ImageRectTransform.anchorMin = new Vector2(0.5f, 1f);
                    m_ImageRectTransform.anchorMax = new Vector2(0.5f, 1f);
                    m_ImageRectTransform.pivot = new Vector2(0.5f, 0.5f);
                    m_ImageRectTransform.sizeDelta = new Vector2(m_Thickness, m_Lenght);
                    m_ImageRectTransform.localPosition = Vector3.zero;

                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0.5f, 1f);
                    rectTransform.sizeDelta = new Vector2(rectTransform.parent.GetComponent<RectTransform>().rect.width / 11.0f, 0);
                    break;
                case Axe.DirectionEnum.TopToBottom:
                case Axe.DirectionEnum.BottomToTop:
                    m_ImageRectTransform.anchorMin = new Vector2(1f, 0.5f);
                    m_ImageRectTransform.anchorMax = new Vector2(1f, 0.5f);
                    m_ImageRectTransform.pivot = new Vector2(0.5f, 0.5f);
                    m_ImageRectTransform.sizeDelta = new Vector2(m_Lenght, m_Thickness);
                    m_ImageRectTransform.localPosition = Vector3.zero;

                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(1f, 0.5f);
                    rectTransform.sizeDelta = new Vector2(0, rectTransform.parent.GetComponent<RectTransform>().rect.height / 11.0f);
                    break;
            }
        }
        protected virtual void SetDirection()
        {
            RectTransform rectTransform = transform as RectTransform;
            switch (m_Direction)
            {
                case Axe.DirectionEnum.LeftToRight:
                case Axe.DirectionEnum.RightToLeft:
                    m_ImageRectTransform.anchorMin = new Vector2(0.5f, 1f);
                    m_ImageRectTransform.anchorMax = new Vector2(0.5f, 1f);
                    m_ImageRectTransform.pivot = new Vector2(0.5f, 0.5f);
                    m_ImageRectTransform.localPosition = Vector3.zero;

                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0.5f, 1f);
                    rectTransform.sizeDelta = new Vector2(rectTransform.parent.GetComponent<RectTransform>().rect.width / 11.0f, 0);
                    break;
                case Axe.DirectionEnum.TopToBottom:
                case Axe.DirectionEnum.BottomToTop:
                    m_ImageRectTransform.anchorMin = new Vector2(1f, 0.5f);
                    m_ImageRectTransform.anchorMax = new Vector2(1f, 0.5f);
                    m_ImageRectTransform.pivot = new Vector2(0.5f, 0.5f);
                    m_ImageRectTransform.localPosition = Vector3.zero;

                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(1f, 0.5f);
                    rectTransform.sizeDelta = new Vector2(0, rectTransform.parent.GetComponent<RectTransform>().rect.height / 11.0f);
                    break;
            }
            SetLenght();
            SetPosition();
        }
        protected virtual void SetLenght()
        {
            switch (m_Direction)
            {
                case Axe.DirectionEnum.LeftToRight:
                case Axe.DirectionEnum.RightToLeft:
                    m_ImageRectTransform.sizeDelta = new Vector2(m_ImageRectTransform.sizeDelta.x, m_Lenght);
                    break;
                case Axe.DirectionEnum.BottomToTop:
                case Axe.DirectionEnum.TopToBottom:
                    m_ImageRectTransform.sizeDelta = new Vector2(m_Lenght, m_ImageRectTransform.sizeDelta.y);
                    break;
            }
        }
        protected virtual void SetThickness()
        {
            switch (m_Direction)
            {
                case Axe.DirectionEnum.LeftToRight:
                case Axe.DirectionEnum.RightToLeft:
                    m_ImageRectTransform.sizeDelta = new Vector2(m_Thickness, m_ImageRectTransform.sizeDelta.y);
                    break;
                case Axe.DirectionEnum.BottomToTop:
                case Axe.DirectionEnum.TopToBottom:
                    m_ImageRectTransform.sizeDelta = new Vector2(m_ImageRectTransform.sizeDelta.x, m_Thickness);
                    break;
            }
        }
        protected virtual void SetPosition()
        {
            RectTransform rectTransform = transform as RectTransform;
            switch (m_Direction)
            {
                case Axe.DirectionEnum.LeftToRight:
                case Axe.DirectionEnum.RightToLeft:
                    rectTransform.localPosition = m_Position * Vector3.right;
                    break;
                case Axe.DirectionEnum.BottomToTop:
                case Axe.DirectionEnum.TopToBottom:
                    rectTransform.localPosition = m_Position * Vector3.up;
                    break;
            }
        }
        protected virtual void SetColor()
        {
            m_ImageRectTransform.GetComponent<Image>().color = m_Color;
        }
        #endregion
    }
}