﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Tools.Unity.Graph
{
    public class TickMark : MonoBehaviour
    {
        #region Properties
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
        [SerializeField] protected ColorEvent m_OnChangeColor;
        public ColorEvent OnChangeColor
        {
            get
            {
                return m_OnChangeColor;
            }
        }
        #endregion

        #region Setters
        protected virtual void OnValidate()
        {
            SetColor();
        }
        protected virtual void SetColor()
        {
            m_OnChangeColor.Invoke(m_Color);
        }
        #endregion
    }
}