﻿using HBP.Data.Anatomy;
using Tools.Unity.Lists;
using System.Linq;

namespace HBP.UI.Anatomy
{
    public class ImplantationList : SelectableListWithSave<Implantation>
    {
        #region Properties
        enum OrderBy { None, Name, DescendingName, Path, DescendingPath }
        OrderBy m_OrderBy = OrderBy.None;
        #endregion

        #region SortingMethods
        public void SortByName()
        {
            switch (m_OrderBy)
            {
                case OrderBy.Name:
                    m_ObjectsToItems = m_ObjectsToItems.OrderByDescending((elt) => elt.Key.Name).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.DescendingName;
                    break;
                default:
                    m_ObjectsToItems = m_ObjectsToItems.OrderBy((elt) => elt.Key.Name).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.Name;
                    break;
            }
            foreach (var item in m_ObjectsToItems.Values) item.transform.SetAsLastSibling();
        }
        public void SortByPath()
        {
            switch (m_OrderBy)
            {
                case OrderBy.Path:
                    m_ObjectsToItems = m_ObjectsToItems.OrderByDescending((elt) => elt.Key.Path).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.DescendingPath;
                    break;
                default:
                    m_ObjectsToItems = m_ObjectsToItems.OrderBy((elt) => elt.Key.Path).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.Path;
                    break;
            }
            foreach (var item in m_ObjectsToItems.Values) item.transform.SetAsLastSibling();
        }
        #endregion
    }
}