﻿using UnityEngine;
using Tools.Unity.Lists;
using System.Linq;

namespace HBP.UI
{
    public class AliasList : ActionableList<Data.Alias>
    {
        #region Properties
        enum OrderBy { None, Key, DescendingKey, Value, DescendingValue }
        OrderBy m_OrderBy = OrderBy.None;

        [SerializeField] SortingDisplayer m_KeySortingDisplayer;
        [SerializeField] SortingDisplayer m_ValueSortingDisplayer;
        #endregion

        #region Public Methods
        public override bool Add(Data.Alias objectToAdd)
        {
            SortByNone();
            return base.Add(objectToAdd);
        }
        #endregion

        #region Sorting Methods
        /// <summary>
        /// Sort by key.
        /// </summary>
        /// <param name="sorting">Sorting</param>
        public void SortByKey(Sorting sorting)
        {
            switch (sorting)
            {
                case Sorting.Ascending:
                    m_Objects = m_Objects.OrderByDescending((elt) => elt.Key).ToList();
                    m_OrderBy = OrderBy.Key;
                    m_KeySortingDisplayer.Sorting = SortingDisplayer.SortingType.Ascending;
                    break;
                case Sorting.Descending:
                    m_Objects = m_Objects.OrderBy((elt) => elt.Key).ToList();
                    m_OrderBy = OrderBy.DescendingKey;
                    m_KeySortingDisplayer.Sorting = SortingDisplayer.SortingType.Descending;
                    break;
            }
            Refresh();
            m_ValueSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }
        /// <summary>
        /// Sort by key.
        /// </summary>
        public void SortByKey()
        {
            switch (m_OrderBy)
            {
                case OrderBy.DescendingKey: SortByKey(Sorting.Ascending); break;
                default: SortByKey(Sorting.Descending); break;
            }
        }
        /// <summary>
        /// Sort by value.
        /// </summary>
        /// <param name="sorting">Sorting</param>
        public void SortByValue(Sorting sorting)
        {
            switch (sorting)
            {
                case Sorting.Ascending:
                    m_Objects = m_Objects.OrderBy((elt) => elt.Value).ToList();
                    m_OrderBy = OrderBy.Value;
                    m_ValueSortingDisplayer.Sorting = SortingDisplayer.SortingType.Ascending;
                    break;
                case Sorting.Descending:
                    m_Objects = m_Objects.OrderByDescending((elt) => elt.Value).ToList();
                    m_OrderBy = OrderBy.DescendingValue;
                    m_ValueSortingDisplayer.Sorting = SortingDisplayer.SortingType.Descending;
                    break;
            }
            Refresh();
            m_KeySortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }
        /// <summary>
        /// Sort by value.
        /// </summary>
        public void SortByValue()
        {
            switch (m_OrderBy)
            {
                case OrderBy.DescendingValue: SortByValue(Sorting.Ascending); break;
                default: SortByValue(Sorting.Descending); break;
            }
        }
        /// <summary>
        /// Sort by none.
        /// </summary>
        public void SortByNone()
        {
            m_OrderBy = OrderBy.None;
            m_KeySortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_ValueSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }
        #endregion
    }
}