﻿using HBP.Module3D;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace HBP.UI.Module3D
{
    public class SiteToolbar : Toolbar
    {
        #region Properties
        /// <summary>
        /// Display the name of the selected site
        /// </summary>
        [SerializeField]
        private Tools.SelectedSite m_SelectedSite;
        /// <summary>
        /// Compare two sites
        /// </summary>
        [SerializeField]
        private Tools.CompareSite m_CompareSite;
        /// <summary>
        /// Load a single patient scene from a multi patient scene
        /// </summary>
        [SerializeField]
        private Tools.LoadPatient m_LoadPatient;
        /// <summary>
        /// Modify the state of some sites
        /// </summary>
        [SerializeField]
        private Tools.SiteModifier m_SiteModifier;
        /// <summary>
        /// Show or hide blacklisted sites
        /// </summary>
        [SerializeField]
        private Tools.BlacklistedSitesDisplay m_BlacklistedSitesDisplay;
        #endregion

        #region Private Methods
        /// <summary>
        /// Link elements to the toolbar
        /// </summary>
        /// <param name="parent">Transform of the toolbar</param>
        protected override void AddTools()
        {
            m_Tools.Add(m_SelectedSite);
            m_Tools.Add(m_CompareSite);
            m_Tools.Add(m_LoadPatient);
            m_Tools.Add(m_SiteModifier);
            m_Tools.Add(m_BlacklistedSitesDisplay);
        }
        #endregion
    }
}