﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tools.Unity.Lists
{
    public class SelectableList<T> : List<T>
    {
        #region Properties
        protected GenericEvent<T, bool> m_OnSelectionChanged = new GenericEvent<T, bool>();
        public virtual GenericEvent<T, bool> OnSelectionChanged
        {
            get { return m_OnSelectionChanged; }
        }
        public virtual T[] ObjectsSelected
        {
            get
            {
                return (from couple in m_SelectedStateByObject where couple.Value select couple.Key).ToArray();
            }
            set
            {
                Deselect(from obj in m_Objects.Where((elt) => !value.Contains(elt)) select obj);
                Select(from obj in m_Objects.Where((elt) => value.Contains(elt)) select obj);
            }
        }
        protected Dictionary<T, bool> m_SelectedStateByObject;
        [SerializeField, Candlelight.PropertyBackingField]
        protected bool m_MultiSelection;
        public virtual bool MultiSelection
        {
            get
            {
                return m_MultiSelection;
            }
            set
            {
                m_MultiSelection = value;
                if (!value)
                {
                    T[] objectSelected = ObjectsSelected;
                    for (int i = 1; i < objectSelected.Length; i++)
                    {
                        Deselect(objectSelected[i]);
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        public override bool Add(T obj)
        {
            if (base.Add(obj))
            {
                m_SelectedStateByObject.Add(obj, false);
                return true;
            }
            return false;
        }
        public override bool Remove(T obj)
        {
            if (base.Remove(obj))
            {
                m_SelectedStateByObject.Remove(obj);
                return true;
            }
            return false;
        }
        public virtual void SelectAll()
        {
            SelectAll(Toggle.ToggleTransition.None);
        }
        public virtual void SelectAll(Toggle.ToggleTransition transition)
        {
            Select(m_Objects, transition);
        }
        public virtual void DeselectAll()
        {
            DeselectAll(Toggle.ToggleTransition.None);
        }
        public virtual void DeselectAll(Toggle.ToggleTransition transition) 
        {
            Deselect(m_Objects, transition);
        }
        public virtual void Select(T objectToSelect, Toggle.ToggleTransition transition = Toggle.ToggleTransition.None)
        {
            if (!m_MultiSelection)
            {
                Deselect(m_Objects.Where((o) => !o.Equals(objectToSelect)));
            }
            if (m_SelectedStateByObject.ContainsKey(objectToSelect))
            {
                m_SelectedStateByObject[objectToSelect] = true;
            }
            Item<T> item;
            if (m_ItemByObject.TryGetValue(objectToSelect, out item))
            {
                (item as SelectableItem<T>).Select(true, transition);
            }
        }
        public virtual void Select(IEnumerable<T> objectsToSelect, Toggle.ToggleTransition transition = Toggle.ToggleTransition.None)
        {
            foreach (var obj in objectsToSelect) Select(obj, transition);
        }
        public virtual void Deselect(T objectToDeselect, Toggle.ToggleTransition transition = Toggle.ToggleTransition.None)
        {
            if (m_SelectedStateByObject.ContainsKey(objectToDeselect))
            {
                m_SelectedStateByObject[objectToDeselect] = false;
            }
            Item<T> item;
            if (m_ItemByObject.TryGetValue(objectToDeselect, out item))
            {
                (item as SelectableItem<T>).Select(false, transition);
            }
        }
        public virtual void Deselect(IEnumerable<T> objectsToDeselect, Toggle.ToggleTransition transition = Toggle.ToggleTransition.None)
        {
            foreach (var obj in objectsToDeselect) Deselect(obj, transition);
        }
        public override bool Initialize()
        {
            if (base.Initialize())
            {
                m_SelectedStateByObject = new Dictionary<T, bool>();
                return true;
            }
            return false;
        }
        #endregion

        #region Private Methods
        protected override void SpawnItem(int number)
        {
            int end = Mathf.Min(m_End + number, m_NumberOfObjects - 1);
            for (int i = m_Start; i <= end; i++)
            {
                T obj = m_Objects[i];
                if (!m_ItemByObject.ContainsKey(obj))
                {
                    SelectableItem<T> item = Instantiate(ItemPrefab, m_ScrollRect.content).GetComponent<SelectableItem<T>>();
                    RectTransform itemRectTransform = item.transform as RectTransform;
                    itemRectTransform.sizeDelta = new Vector2(0, itemRectTransform.sizeDelta.y);
                    itemRectTransform.localPosition = new Vector3(itemRectTransform.localPosition.x, -i * ItemHeight, itemRectTransform.localPosition.z);
                    m_ItemByObject.Add(obj, item);
                    item.OnChangeSelected.RemoveAllListeners();
                    item.Select(m_SelectedStateByObject[obj]);
                    item.OnChangeSelected.AddListener((selected) => OnSelection(obj, selected));
                    item.Object = obj;
                }
            };
        }
        protected override void MoveItemsDownwards(int deplacement)
        {
            for (int i = 0; i < deplacement; i++)
            {
                T obj = m_Objects[m_Start + i];
                SelectableItem<T> item = m_ItemByObject[obj] as SelectableItem<T>;
                m_ItemByObject.Remove(obj);
                T newObj = m_Objects[m_End + 1 + i];
                m_ItemByObject.Add(newObj, item);
                item.transform.localPosition = new Vector3(item.transform.localPosition.x, -(m_End + 1 + i) * ItemHeight, item.transform.localPosition.z);
                item.OnChangeSelected.RemoveAllListeners();
                item.Select(m_SelectedStateByObject[newObj]);
                item.OnChangeSelected.AddListener((selected) => OnSelection(newObj, selected));
                item.Object = newObj;
            }
        }
        protected override void MoveItemsUpwards(int deplacement)
        {
            for (int i = 0; i > deplacement; i--)
            {
                T obj = m_Objects[m_End + i];
                SelectableItem<T> item = m_ItemByObject[obj] as SelectableItem<T>;
                m_ItemByObject.Remove(obj);
                T newObj = m_Objects[m_Start - 1 + i];
                m_ItemByObject.Add(newObj, item);
                item.transform.localPosition = new Vector3(item.transform.localPosition.x, -(m_Start - 1 + i) * ItemHeight, item.transform.localPosition.z);
                item.OnChangeSelected.RemoveAllListeners();
                item.Select(m_SelectedStateByObject[newObj]);
                item.OnChangeSelected.AddListener((selected) => OnSelection(newObj, selected));
                item.Object = newObj;
            }
        }
        protected virtual void OnSelection(T obj, bool selected)
        {
            if (!m_MultiSelection)
            {
                Deselect(m_Objects.Where((o) => !o.Equals(obj)), Toggle.ToggleTransition.Fade);
            }
            if (m_SelectedStateByObject.ContainsKey(obj))
            {
                m_SelectedStateByObject[obj] = selected;
            }
            OnSelectionChanged.Invoke(obj, selected);
        }
        #endregion
    }
}
