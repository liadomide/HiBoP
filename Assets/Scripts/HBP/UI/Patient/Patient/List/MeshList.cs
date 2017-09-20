﻿using System.Linq;
using HBP.Data.Anatomy;

namespace HBP.UI.Anatomy
{
    public class MeshList : Tools.Unity.Lists.SelectableListWithItemAction<Data.Anatomy.Mesh>
    {
        #region Properties
        enum OrderBy { None, Name, DescendingName, Mesh, DescendingMesh, MarsAtlas, DescendingMarsAtlas, Transformation, DescendingTransformation }
        OrderBy m_OrderBy = OrderBy.None;

        public SortingDisplayer m_NameSortingDisplayer;
        public SortingDisplayer m_MeshSortingDisplayer;
        public SortingDisplayer m_MarsAtlasSortingDisplayer;
        public SortingDisplayer m_TransformationSortingDisplayer;
        #endregion

        #region SortingMethods
        public override void Add(Mesh objectToAdd)
        {
            base.Add(objectToAdd);
            SortByNone();
        }
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
            m_MeshSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_MarsAtlasSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_TransformationSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }
        public void SortByMesh()
        {
            switch (m_OrderBy)
            {
                case OrderBy.DescendingMesh:
                    m_ObjectsToItems = m_ObjectsToItems.OrderBy((elt) => elt.Key.HasMesh).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.Mesh;
                    m_MeshSortingDisplayer.Sorting = SortingDisplayer.SortingType.Ascending;
                    break;
                default:
                    m_ObjectsToItems = m_ObjectsToItems.OrderByDescending((elt) => elt.Key.HasMesh).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.DescendingMesh;
                    m_MeshSortingDisplayer.Sorting = SortingDisplayer.SortingType.Descending;

                    break;
            }
            foreach (var item in m_ObjectsToItems.Values) item.transform.SetAsLastSibling();
            m_NameSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_MarsAtlasSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_TransformationSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }
        public void SortByMarsAtlas()
        {
            switch (m_OrderBy)
            {
                case OrderBy.DescendingMarsAtlas:
                    m_ObjectsToItems = m_ObjectsToItems.OrderBy((elt) => elt.Key.HasMarsAtlas).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.MarsAtlas;
                    m_MarsAtlasSortingDisplayer.Sorting = SortingDisplayer.SortingType.Ascending;
                    break;
                default:
                    m_ObjectsToItems = m_ObjectsToItems.OrderByDescending((elt) => elt.Key.HasMarsAtlas).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.DescendingMarsAtlas;
                    m_MarsAtlasSortingDisplayer.Sorting = SortingDisplayer.SortingType.Descending;

                    break;
            }
            foreach (var item in m_ObjectsToItems.Values) item.transform.SetAsLastSibling();
            m_NameSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_MeshSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_TransformationSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }
        public void SortByTransformation()
        {
            switch (m_OrderBy)
            {
                case OrderBy.DescendingTransformation:
                    m_ObjectsToItems = m_ObjectsToItems.OrderBy((elt) => elt.Key.Transformation).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.Transformation;
                    m_TransformationSortingDisplayer.Sorting = SortingDisplayer.SortingType.Ascending;
                    break;
                default:
                    m_ObjectsToItems = m_ObjectsToItems.OrderByDescending((elt) => elt.Key.Transformation).ToDictionary(k => k.Key, v => v.Value);
                    m_OrderBy = OrderBy.DescendingTransformation;
                    m_TransformationSortingDisplayer.Sorting = SortingDisplayer.SortingType.Descending;
                    break;
            }
            foreach (var item in m_ObjectsToItems.Values) item.transform.SetAsLastSibling();
            m_NameSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_MeshSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_MarsAtlasSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }
        public void SortByNone()
        {
            m_OrderBy = OrderBy.None;
            m_NameSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_MeshSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_MarsAtlasSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
            m_TransformationSortingDisplayer.Sorting = SortingDisplayer.SortingType.None;
        }
        #endregion
    }
}