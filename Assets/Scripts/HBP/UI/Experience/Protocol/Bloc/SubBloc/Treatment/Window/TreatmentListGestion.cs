﻿using HBP.Data.Experience.Protocol;
using System.Collections.Generic;
using System.Linq;
using Tools.Unity.Components;
using UnityEngine;

namespace HBP.UI.Experience.Protocol
{
    public class TreatmentListGestion : ListGestion<Treatment>
    {
        #region Properties
        [SerializeField] new TreatmentList List;
        public override List<Treatment> Objects
        {
            get
            {
                return base.Objects;
            }

            set
            {
                List.Initialize();
                base.Objects = value;
            }
        }

        Tools.CSharp.Window m_Window;
        public Tools.CSharp.Window Window
        {
            get
            {
                return m_Window;
            }
            set
            {
                m_Window = value;
                foreach (var modifier in SubWindows.OfType<TreatmentModifier>())
                {
                    modifier.Window = value;
                }
            }
        }

        Tools.CSharp.Window m_Baseline;
        public Tools.CSharp.Window Baseline
        {
            get
            {
                return m_Baseline;
            }
            set
            {
                m_Baseline = value;
                foreach (var modifier in SubWindows.OfType<TreatmentModifier>())
                {
                    modifier.Baseline = value;
                }
            }
        }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            base.List = List;
            base.Initialize();
        }
        protected override void OnSaveCreator(CreatorWindow creatorWindow)
        {
            Data.Enums.CreationType type = creatorWindow.Type;
            Treatment item = new ClampTreatment();
            switch (type)
            {
                case Data.Enums.CreationType.FromScratch:
                    OpenModifier(item, Interactable);
                    break;
                case Data.Enums.CreationType.FromExistingItem:
                    OpenSelector(Objects.ToArray());
                    break;
                case Data.Enums.CreationType.FromFile:
                    if (LoadFromFile(out item))
                    {
                        OpenModifier(item, Interactable);
                    }
                    break;
                case Data.Enums.CreationType.FromDatabase:
                    OpenDatabaseSelector();
                    break;
            }
        }
        protected override ItemModifier<Treatment> OpenModifier(Treatment item, bool interactable)
        {
            TreatmentModifier modifier = ApplicationState.WindowsManager.OpenModifier(item, interactable) as TreatmentModifier;
            modifier.Window = Window;
            modifier.Baseline = Baseline;
            modifier.OnClose.AddListener(() => OnCloseSubWindow(modifier));
            modifier.OnSave.AddListener(() => OnSaveModifier(modifier));
            OnOpenSavableWindow.Invoke(modifier);
            SubWindows.Add(modifier);
            return modifier;
        }
        #endregion
    }
}