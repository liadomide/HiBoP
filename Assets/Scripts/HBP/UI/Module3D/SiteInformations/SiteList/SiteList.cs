﻿using Tools.Unity.Lists;
using UnityEngine;

namespace HBP.UI.Module3D
{
    /// <summary>
    /// List to display Module3D sites.
    /// </summary>
    public class SiteList : List<HBP.Module3D.Site>
    {
        #region Properties
        /// <summary>
        /// List of module3D sites.
        /// </summary>
        public System.Collections.Generic.List<HBP.Module3D.Site> ObjectsList
        {
            get
            {
                return m_Objects;
            }
            set
            {
                m_Objects = value;
                m_NumberOfObjects = value.Count;
                ScrollRect.content.sizeDelta = new Vector2(0, m_ItemHeight * m_NumberOfObjects);
                ScrollRect.content.hasChanged = true;
            }
        }
        #endregion
    }
}