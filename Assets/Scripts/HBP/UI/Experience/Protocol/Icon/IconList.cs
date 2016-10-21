﻿using HBP.Data.Experience.Protocol;
using System.Linq;

namespace HBP.UI.Experience.Protocol
{
    public class IconList : Tools.Unity.Lists.SelectableListWithSave<Icon>
    {
        #region Attributs
        /// <summary>
        /// The name alphabetical sort.
        /// </summary>
        bool m_sortByName = false;

        /// <summary>
        /// The path sort.
        /// </summary>
        bool m_sortByPath = false;

        /// <summary>
        /// The Min Window sort.
        /// </summary>
        bool m_SortByMin = false;

        /// <summary>
        /// The Max Window sort.
        /// </summary>
        bool m_SortByMax = false;
        #endregion

        #region Public Methods
        /// <summary>
        /// Sort the Icons by name.
        /// </summary>
        public void SortByName()
        {
            if (m_sortByName)
            {
                m_objects.OrderByDescending(x => x.Name);
            }
            else
            {
                m_objects.OrderBy(x => x.Name);
            }
            m_sortByName = !m_sortByName;
            ApplySort();
        }

        /// <summary>
        /// Sort the Icons by path.
        /// </summary>
		public void SortByPath()
        {
            if (m_sortByPath)
            {
                m_objects.OrderByDescending(x => x.Image);
            }
            else
            {
                m_objects.OrderBy(x => x.Image);
            }
            m_sortByPath = !m_sortByPath;
            ApplySort();
        }

        /// <summary>
        /// Sort the Icons by min window.
        /// </summary>
        public void SortByMin()
        {
            if (m_SortByMin)
            {
                m_objects.OrderByDescending(x => x.Window.x);
            }
            else
            {
                m_objects.OrderBy(x => x.Window.x);
            }
            m_SortByMin = !m_SortByMin;
            ApplySort();
        }

        /// <summary>
        /// Sort the Icons by max window.
        /// </summary>
        public void SortByMax()
        {
            if (m_SortByMax)
            {
                m_objects.OrderByDescending(x => x.Window.y);
            }
            else
            {
                m_objects.OrderBy(x => x.Window.y);
            }
            m_SortByMax = !m_SortByMax;
            ApplySort();
        }
        #endregion
    }
}
