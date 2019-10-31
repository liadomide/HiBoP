﻿using HBP.Data.Preferences;
using Tools.Unity;
using UnityEngine;
using UnityEngine.UI;
using d = HBP.Data.Experience.Protocol;

namespace HBP.UI.Experience.Protocol
{
    public class SubBlocModifier : ObjectModifier<d.SubBloc>
    {
        #region Properties
        [SerializeField] InputField m_NameInputField;
        [SerializeField] InputField m_OrderInputField;
        [SerializeField] Dropdown m_TypeDropdown;
        [SerializeField] RangeSlider m_WindowSlider;
        [SerializeField] RangeSlider m_BaselineSlider;

        [SerializeField] EventListGestion m_EventListGestion;
        [SerializeField] IconListGestion m_IconListGestion;
        [SerializeField] TreatmentListGestion m_TreatmentListGestion;

        public override bool Interactable
        {
            get
            {
                return base.Interactable;
            }

            set
            {
                base.Interactable = value;
                m_NameInputField.interactable = value;
                m_OrderInputField.interactable = value;
                m_TypeDropdown.interactable = value && ItemTemp.Type == Data.Enums.MainSecondaryEnum.Secondary;
                m_WindowSlider.interactable = value;
                m_BaselineSlider.interactable = value;
                m_EventListGestion.Interactable = value;
                m_IconListGestion.Interactable = value;
                m_TreatmentListGestion.Interactable = value;
            }
        }

        #endregion

        #region Private Methods
        protected override void Initialize()
        {
            base.Initialize();

            m_NameInputField.onEndEdit.AddListener((value) => ItemTemp.Name = value);
            m_OrderInputField.onEndEdit.AddListener((value) => ItemTemp.Order = int.Parse(value));
            m_TypeDropdown.onValueChanged.AddListener((value) => ItemTemp.Type = (Data.Enums.MainSecondaryEnum) value);

            m_WindowSlider.onValueChanged.AddListener(OnChangeWindow);
            m_BaselineSlider.onValueChanged.AddListener(OnChangeBaseline);

            m_EventListGestion.WindowsReferencer.OnOpenWindow.AddListener(window => WindowsReferencer.Add(window));
            m_IconListGestion.WindowsReferencer.OnOpenWindow.AddListener(window => WindowsReferencer.Add(window));
            m_TreatmentListGestion.WindowsReferencer.OnOpenWindow.AddListener(window => WindowsReferencer.Add(window));
        }
        protected override void SetFields(d.SubBloc objectToDisplay)
        {
            m_NameInputField.text = objectToDisplay.Name;
            m_OrderInputField.text = objectToDisplay.Order.ToString();
            m_TypeDropdown.Set(typeof(Data.Enums.MainSecondaryEnum), (int)objectToDisplay.Type);

            ProtocolPreferences preferences = ApplicationState.UserPreferences.Data.Protocol;
            m_WindowSlider.minLimit = preferences.MinLimit;
            m_WindowSlider.maxLimit = preferences.MaxLimit;
            m_WindowSlider.step = preferences.Step;

            m_WindowSlider.Values = objectToDisplay.Window.ToVector2();

            m_BaselineSlider.minLimit = preferences.MinLimit;
            m_BaselineSlider.maxLimit = preferences.MaxLimit;
            m_BaselineSlider.step = preferences.Step;
            m_BaselineSlider.Values = objectToDisplay.Baseline.ToVector2();

            m_EventListGestion.List.Set(objectToDisplay.Events);
            m_IconListGestion.List.Set(objectToDisplay.Icons);
            m_TreatmentListGestion.List.Set(objectToDisplay.Treatments);
        }
        void OnChangeWindow(float min, float max)
        {
            ItemTemp.Window = new Tools.CSharp.Window(Mathf.RoundToInt(min), Mathf.RoundToInt(max));
            m_IconListGestion.Window = itemTemp.Window;
            m_TreatmentListGestion.Window = itemTemp.Window;
            m_TreatmentListGestion.Baseline = itemTemp.Baseline;
        }
        void OnChangeBaseline(float min, float max)
        {
            ItemTemp.Baseline = new Tools.CSharp.Window(Mathf.RoundToInt(min), Mathf.RoundToInt(max));
            m_TreatmentListGestion.Baseline = itemTemp.Baseline;
        }
        #endregion
    }
}