﻿using HBP.Data.Experience.Protocol;
using System.Linq;

namespace HBP.UI.Experience.Protocol
{
    public class IconList : Tools.Unity.Lists.SelectableListWithItemAction<Icon>
    {
        #region Properties
        enum OrderBy { None, Name, DescendingName, Path, DescendingPath, Start, DescendingStart, End, DescendingEnd}
        OrderBy m_OrderBy = OrderBy.None;

        public SortingDisplayer m_NameSortingDisplayer;
        public SortingDisplayer m_IllustrationSortingDisplayer;
        public SortingDisplayer m_StartSortingDisplayer;
        //public SortingDisplayer m_EndSortingDisplayer;
        #endregion

        #region Public Methods
        public void SortByName()
        {
            switch (m_OrderBy)
            {
                case OrderBy.DescendingName:
                    m_ObjectsToItems = m_ObjectsToItems.OrderByDescending((elt) => elt.Key.Name).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.Name;
                    m_NameSortingDisplayer.Sorting = SortingDisplayer.SortingType.Ascending;
                    break;
                default:
                    m_ObjectsToItems = m_ObjectsToItems.OrderBy((elt) => elt.Key.Name).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.DescendingName;
                    m_NameSortingDisplayer.Sorting = SortingDisplayer.SortingType.Descending;
                    break;
            }
            foreach (var item in m_ObjectsToItems.Values) item.transform.SetAsLastSibling();
            m_IllustrationSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_StartSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            //m_EndSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }

		public void SortByPath()
        {
            switch (m_OrderBy)
            {
                case OrderBy.DescendingPath:
                    m_ObjectsToItems = m_ObjectsToItems.OrderByDescending((elt) => elt.Key.IllustrationPath).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.Path;
                    m_IllustrationSortingDisplayer.Sorting = SortingDisplayer.SortingType.Ascending;
                    break;
                default:
                    m_ObjectsToItems = m_ObjectsToItems.OrderBy((elt) => elt.Key.IllustrationPath).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.DescendingPath;
                    m_IllustrationSortingDisplayer.Sorting = SortingDisplayer.SortingType.Descending;
                    break;
            }
            foreach (var item in m_ObjectsToItems.Values) item.transform.SetAsLastSibling();
            m_NameSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_StartSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            //m_EndSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }
        public void SortByStart()
        {
            switch (m_OrderBy)
            {
                case OrderBy.DescendingStart:
                    m_ObjectsToItems = m_ObjectsToItems.OrderByDescending((elt) => elt.Key.Window.Start).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.Start;
                    m_StartSortingDisplayer.Sorting = SortingDisplayer.SortingType.Ascending;
                    break;
                default:
                    m_ObjectsToItems = m_ObjectsToItems.OrderBy((elt) => elt.Key.Window.Start).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.DescendingStart;
                    m_StartSortingDisplayer.Sorting = SortingDisplayer.SortingType.Descending;
                    break;
            }
            foreach (var item in m_ObjectsToItems.Values) item.transform.SetAsLastSibling();
            m_NameSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_IllustrationSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            //m_EndSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }
        //public void SortByEnd()
        //{
        //    switch (m_OrderBy)
        //    {
        //        case OrderBy.DescendingEnd:
        //            m_ObjectsToItems = m_ObjectsToItems.OrderByDescending((elt) => elt.Key.Window.End).ToDictionary(k => k.Key, v => v.Value);
        //            m_OrderBy = OrderBy.End;
        //            m_EndSortingDisplayer.Sorting = SortingDisplayer.SortingType.Ascending;
        //            break;
        //        default:
        //            m_ObjectsToItems = m_ObjectsToItems.OrderBy((elt) => elt.Key.Window.End).ToDictionary(k => k.Key, v => v.Value);
        //            m_OrderBy = OrderBy.DescendingEnd;
        //            m_EndSortingDisplayer.Sorting = SortingDisplayer.SortingType.Descending;
        //            break;
        //    }
        //    foreach (var item in m_ObjectsToItems.Values) item.transform.SetAsLastSibling();
        //    m_NameSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        //    m_IllustrationSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        //    m_StartSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        //}
        #endregion
    }
}
