﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.Unity.Lists
{
    [RequireComponent(typeof(ScrollRect))]
    public class List<T> : MonoBehaviour
    {
        #region Properties
        public GameObject ItemPrefab;
        public float ItemHeight;

        protected Dictionary<T, Item<T>> m_ItemByObject;
        protected System.Collections.Generic.List<T> m_Objects = new System.Collections.Generic.List<T>();
        public virtual T[] Objects
        {
            get
            {
                return m_Objects.ToArray();
            }
            set
            {
                Remove(m_Objects.ToArray());
                Add(value);
            }
        }

        protected bool m_Interactable;
        public bool interactable
        {
            get
            {
                return m_Interactable;
            }
            set
            {
                m_Interactable = value;
                foreach (var item in m_ItemByObject.Values) item.interactable = value;
            }
        }

        protected ScrollRect m_ScrollRect;
        protected int m_NumberOfObjectsVisibleAtTheSameTime;
        protected int m_NumberOfObjects;
        protected int m_Start;
        protected int m_End;
        protected bool m_Initialized;
        #endregion

        #region Public Methods
        public virtual bool Add(T obj)
        {
            if (!m_Objects.Contains(obj))
            {
                m_Objects.Add(obj);
                m_NumberOfObjects++;
                m_ScrollRect.content.sizeDelta = new Vector2(m_ScrollRect.content.sizeDelta.x, m_ScrollRect.content.sizeDelta.y + ItemHeight);
                m_ScrollRect.content.hasChanged = true;
                return true;
            }
            return false;
        }
        public virtual bool Add(IEnumerable<T> objectsToAdd)
        {
            bool result = true;
            foreach (T obj in objectsToAdd) result &= Add(obj);
            return result;
        }
        public virtual bool Remove(T obj)
        {
            if (m_Objects.Contains(obj))
            {
                if (m_NumberOfObjects <= m_NumberOfObjectsVisibleAtTheSameTime)
                {
                    DestroyItem(-1);
                }
                m_NumberOfObjects--;
                m_Objects.Remove(obj);
                m_ScrollRect.content.sizeDelta = new Vector2(m_ScrollRect.content.sizeDelta.x, m_ScrollRect.content.sizeDelta.y - ItemHeight);
                m_ScrollRect.content.hasChanged = true;
                GetLimits(out m_Start, out m_End);
                Refresh();
                return true;
            }
            return false;
        }
        public virtual bool Remove(IEnumerable<T> objectsToRemove)
        {
            bool result = true;
            foreach (T obj in objectsToRemove) result &= Remove(obj);
            return result;
        }
        public virtual bool UpdateObject(T objectToUpdate)
        {
            Item<T> item;
            if (m_ItemByObject.TryGetValue(objectToUpdate, out item))
            {
                item.Object = objectToUpdate;
                return true;
            }
            return false;
        }
        public virtual void Refresh()
        {
            Item<T>[] items =  m_ItemByObject.Values.OrderByDescending((item) => item.transform.localPosition.y).ToArray();
            int itemsLength = items.Length;
            m_ItemByObject.Clear();
            for (int i = m_Start, j=0; i <= m_End && j < itemsLength; i++, j++)
            {
                items[j].Object = m_Objects[i];
                m_ItemByObject.Add(m_Objects[i], items[j]);
            }
        }
        public virtual bool Initialize()
        {
            if (!m_Initialized)
            {
                m_Objects = new System.Collections.Generic.List<T>();
                m_ItemByObject = new Dictionary<T, Item<T>>();
                m_ScrollRect = GetComponent<ScrollRect>();
                m_Initialized = true;
                return true;
            }
            return false;
        }
        #endregion

        #region Private Methods
        private void Awake()
        {
            Initialize();
        }
        void Update()
        {
            if (m_ScrollRect.viewport.hasChanged)
            {
                m_NumberOfObjectsVisibleAtTheSameTime = Mathf.CeilToInt(m_ScrollRect.viewport.rect.height / ItemHeight) + 1;
                m_ScrollRect.viewport.hasChanged = false;
            }
            if (m_ScrollRect.content.hasChanged)
            {
                Display();
                m_ScrollRect.content.hasChanged = false;
            }
        }
        void Display()
        {
            int start, end; GetLimits(out start, out end);

            // Resize viewport and list.
            int resize = (end - start) - (m_End - m_Start);
            if (resize >= 0) SpawnItem(resize);
            else if (resize < 0) DestroyItem(resize);

            // Move content.
            int deplacement = start - m_Start;
            if (deplacement > 0) MoveItemsDownwards(deplacement);
            else if (deplacement < 0) MoveItemsUpwards(deplacement);

            m_Start = start;
            m_End = end;
        }
        protected virtual void SpawnItem(int number)
        {
            int end = Mathf.Min(m_End + number, m_NumberOfObjects - 1);
            for (int i = m_Start; i <= end; i++)
            {
                T obj = m_Objects[i];
                if (!m_ItemByObject.ContainsKey(obj))
                {
                    Item<T> item = Instantiate(ItemPrefab, m_ScrollRect.content).GetComponent<Item<T>>();
                    RectTransform itemRectTransform = item.transform as RectTransform;
                    itemRectTransform.sizeDelta = new Vector2(0, itemRectTransform.sizeDelta.y);
                    itemRectTransform.localPosition = new Vector3(itemRectTransform.localPosition.x, -i * ItemHeight, itemRectTransform.localPosition.z);
                    m_ItemByObject.Add(obj, item);
                    item.Object = obj;
                }
            }
        }
        void DestroyItem(int number)
        {
            int end = m_End + 1 + number;
            for (int i = end; i <= m_End; i++)
            {
                T obj = m_Objects[i];
                Item<T> item = m_ItemByObject[obj];
                m_ItemByObject.Remove(obj);
                Destroy(item.gameObject);
            }
        }
        protected virtual void MoveItemsUpwards(int deplacement)
        {
            for (int i = 0; i > deplacement; i--)
            {
                T obj = m_Objects[m_End + i];
                Item<T> item = m_ItemByObject[obj];
                m_ItemByObject.Remove(obj);
                T newObj = m_Objects[m_Start - 1 + i];
                m_ItemByObject.Add(newObj, item);
                item.transform.localPosition = new Vector3(item.transform.localPosition.x, -(m_Start - 1 + i) * ItemHeight, item.transform.localPosition.z);
                item.Object = newObj;
            }
        }
        protected virtual void MoveItemsDownwards(int deplacement)
        {
            for (int i = 0; i < deplacement; i++)
            {
                T obj = m_Objects[m_Start + i];
                Item<T> item = m_ItemByObject[obj];
                m_ItemByObject.Remove(obj);
                T newObj = m_Objects[m_End + 1 + i];
                m_ItemByObject.Add(newObj, item);
                item.transform.localPosition = new Vector3(item.transform.localPosition.x, -(m_End + 1 + i) * ItemHeight, item.transform.localPosition.z);
                item.Object = newObj;
            }
        }
        void GetLimits(out int start, out int end)
        {
            int maxNumberOfItem = m_NumberOfObjects - m_NumberOfObjectsVisibleAtTheSameTime;
            start = Mathf.Clamp(Mathf.FloorToInt((m_ScrollRect.content.localPosition.y / m_ScrollRect.content.sizeDelta.y) * m_NumberOfObjects), 0, Mathf.Max(maxNumberOfItem, 0));
            end = Mathf.Clamp(start + m_NumberOfObjectsVisibleAtTheSameTime - 1, 0, m_NumberOfObjects > 0 ? m_NumberOfObjects - 1 : 0);
        }
        #endregion
    }
}
