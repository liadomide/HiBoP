﻿using HBP.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools.CSharp;
using UnityEngine;
using UnityEngine.Events;

namespace Tools.Unity.Components
{
    [Serializable]
    public class ObjectCreator<T> : MonoBehaviour where T : ICloneable, ICopiable, new()
    {
        #region Properties
        [SerializeField] List<T> m_ExistingItems = new List<T>();
        public List<T> ExistingItems
        {
            get
            {
                return m_ExistingItems;
            }
            set
            {
                m_ExistingItems = value;
            }
        }

        [SerializeField] protected SubWindowsManager m_SubWindowsManager = new SubWindowsManager();
        public virtual SubWindowsManager SubWindowsManager { get => m_SubWindowsManager; }

        public UnityEvent<T> OnObjectCreated { get; protected set; } = new GenericEvent<T>();
        #endregion

        #region Public Methods
        public virtual void Create()
        {
            CreatorWindow creatorWindow = ApplicationState.WindowsManager.Open<CreatorWindow>("Creator window", true);
            creatorWindow.IsLoadableFromFile = typeof(T).GetInterfaces().Contains(typeof(ILoadable<T>));
            creatorWindow.IsLoadableFromDatabase = typeof(T).GetInterfaces().Contains(typeof(ILoadableFromDatabase<T>));
            creatorWindow.OnSave.AddListener(() => OnSaveCreator(creatorWindow));
            SubWindowsManager.Add(creatorWindow);
        }
        public virtual void CreateFromScratch()
        {
            OpenModifier(new T());
        }
        public virtual void CreateFromExistingItems()
        {
            OpenSelector(ExistingItems);
        }
        public virtual void CreateFromFile()
        {
            if (LoadFromFile(out T item))
            {
                OpenModifier(item);
            }
        }
        public virtual void CreateFromDatabase()
        {
            SelectDatabase();
        }
        #endregion

        #region Private Methods
        protected virtual void OnSaveCreator(CreatorWindow creatorWindow)
        {
            switch (creatorWindow.Type)
            {
                case HBP.Data.Enums.CreationType.FromScratch:
                    CreateFromScratch();
                    break;
                case HBP.Data.Enums.CreationType.FromExistingItem:
                    CreateFromExistingItems();
                    break;
                case HBP.Data.Enums.CreationType.FromFile:
                    CreateFromFile();
                    break;
                case HBP.Data.Enums.CreationType.FromDatabase:
                    CreateFromDatabase();
                    break;
            }
        }
        protected virtual void OpenSelector(IEnumerable<T> objects, bool multiSelection = false, bool openSelected = true, bool generateNewIDs = true)
        {
            ObjectSelector<T> selector = ApplicationState.WindowsManager.OpenSelector<T>();
            selector.OnSave.AddListener(() => OnSaveSelector(selector, generateNewIDs));
            selector.Objects = objects.ToArray();
            selector.MultiSelection = multiSelection;
            selector.OpenModifierWhenSave = openSelected;
            SubWindowsManager.Add(selector);
        }
        protected virtual void OnSaveSelector(ObjectSelector<T> selector, bool generateNewIDs = true)
        {
            foreach (var obj in selector.ObjectsSelected)
            {
                T clone = (T)obj.Clone();
                if (generateNewIDs)
                {
                    if (typeof(T).GetInterfaces().Contains(typeof(IIdentifiable)))
                    {
                        IIdentifiable identifiable = clone as IIdentifiable;
                        identifiable.GenerateID();
                    }
                }
                if (clone != null)
                {
                    if (selector.OpenModifierWhenSave)
                    {
                        OpenModifier(clone);
                    }
                    else
                    {
                        OnObjectCreated.Invoke(obj);
                    }
                }
            }
        }

        protected virtual ItemModifier<T> OpenModifier(T item)
        {
            ItemModifier<T> modifier = ApplicationState.WindowsManager.OpenModifier(item, true);
            modifier.OnSave.AddListener(() => OnSaveModifier(modifier));
            SubWindowsManager.Add(modifier);
            return modifier;
        }
        protected virtual void OnSaveModifier(ItemModifier<T> modifier)
        {
            OnObjectCreated.Invoke(modifier.Item);
        }

        protected virtual bool LoadFromFile(out T result)
        {
            result = new T();
            ILoadable<T> loadable = result as ILoadable<T>;
            string path = FileBrowser.GetExistingFileName(new string[] { loadable.GetExtension() }).StandardizeToPath();
            if (path != string.Empty)
            {
                result = ClassLoaderSaver.LoadFromJson<T>(path);
                if (typeof(T).GetInterfaces().Contains(typeof(IIdentifiable)))
                {
                    IIdentifiable identifiable = result as IIdentifiable;
                    if (identifiable.ID == "xxxxxxxxxxxxxxxxxxxxxxxxx")
                    {

                        identifiable.ID = Guid.NewGuid().ToString();
                    }
                }
                return true;
            }
            return false;
        }
        protected virtual void SelectDatabase()
        {
            string path = FileBrowser.GetExistingDirectoryName();
            if (path != null)
            {
                ILoadableFromDatabase<T> loadable = new T() as ILoadableFromDatabase<T>;
                GenericEvent<float, float, LoadingText> onChangeProgress = new GenericEvent<float, float, LoadingText>();
                ApplicationState.LoadingManager.Load(loadable.LoadFromDatabase(path, (progress, duration, text) => onChangeProgress.Invoke(progress, duration, text), (result) => OnEndLoadFromDatabase(result.ToArray())), onChangeProgress);
            }
        }
        protected virtual void OnEndLoadFromDatabase(T[] result)
        {
            if (result.Length > 0)
            {
                OpenSelector(result, true, false, false);
            }
        }
        #endregion
    }
}