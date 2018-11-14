﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using data = HBP.Data.TrialMatrix;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.UI;
using Tools.Unity;
using UnityEngine.EventSystems;

namespace HBP.UI.TrialMatrix
{
    public class Bloc : MonoBehaviour
    {
        #region Properties
        public data.Bloc Data { private set;  get; }

        List<SubBloc> m_SubBlocs = new List<SubBloc>();
        public ReadOnlyCollection<SubBloc> SubBlocs { get { return new ReadOnlyCollection<SubBloc>(m_SubBlocs); } }

        List<GameObject> m_Fillers = new List<GameObject>();
        public ReadOnlyCollection<GameObject> Fillers { get { return new ReadOnlyCollection<GameObject>(m_Fillers); } }

        string m_title;
        public string Title
        {
            get { return m_title; }
            set
            {
                m_title = value;
                OnChangeTitle.Invoke(value);
            }
        }
        public StringEvent OnChangeTitle;

        List<int> m_SelectedTrials = new List<int>();
        public ReadOnlyCollection<int> SelectedTrials { get { return new ReadOnlyCollection<int>(m_SelectedTrials); } }

        [SerializeField] GameObject m_SubBlocPrefab;
        [SerializeField] RectTransform m_SubBlocContainer;
        [SerializeField] GameObject m_SelectionMaskPrefab;
        [SerializeField] RectTransform m_SelectionMaskContainer;
        List<GameObject> m_SelectionMasks;
        RectTransform m_RectTransform;
        LayoutElement m_LayoutElement;
        int m_BeginDragTrial;
        bool m_Dragging;
        #endregion

        #region Public Methods
        public void Set(data.Bloc bloc, Texture2D colorMap, Vector2 limits, Dictionary<int,Tools.CSharp.Window> timeLimitsByColumn)
        {
            Data = bloc;
            Title = bloc.ProtocolBloc.Name;
            m_SelectionMasks = new List<GameObject>();

            Clear();
            IOrderedEnumerable<Data.Experience.Protocol.SubBloc> orderedSubBlocs = bloc.ProtocolBloc.OrderedSubBlocs;
            int mainSubBlocIndex = bloc.ProtocolBloc.MainSubBlocPosition;
            foreach (var pair in timeLimitsByColumn)
            {
                int index = pair.Key + mainSubBlocIndex;
                if(index >= 0 && index < orderedSubBlocs.Count())
                {
                    AddSubBloc(bloc.SubBlocs[index], colorMap, limits, pair.Value);
                }
                else
                {
                    AddFiller(pair.Value);
                }
            }

            SetSize();
        }
        public void SelectAllTrials()
        {
            int numberOfTrials = Data.SubBlocs[0].SubTrials.Length;
            int[] trialsToSelect = new int[numberOfTrials];
            for (int i = 0; i < numberOfTrials; i++)
            {
                trialsToSelect[i] = i;
            }
            SelectTrials(trialsToSelect);
        }
        public void SelectTrials(int[] trials,bool additive = false)
        {
            if (!additive) m_SelectedTrials.Clear();
            m_SelectedTrials.AddRange(trials.Where(index => !m_SelectedTrials.Contains(index)));
            ClearSelectionMask();

            int numberOfTrials = Data.SubBlocs[0].SubTrials.Length;
            List<int> maskedTrials = new List<int>();
            for (int i = 0; i < numberOfTrials; i++)
            {
                if (!m_SelectedTrials.Contains(i)) maskedTrials.Add(i);
            }

            List<int> group = new List<int>();
            int numberOfMaskedTrials = maskedTrials.Count;
            for (int i = 0; i < numberOfMaskedTrials; i++)
            {
                int trial = maskedTrials[i];
                if (group.Count != 0 && trial != group[group.Count - 1] + 1)
                {
                    AddSelectionMask(group.ToArray());
                    group.Clear();
                }
                group.Add(trial);
                if (i == numberOfMaskedTrials - 1) AddSelectionMask(group.ToArray());
            }
        }
        public void OnPointerDown()
        {
            m_BeginDragTrial = GetTrialAtPosition(Input.mousePosition);
        }
        public void OnPointerClick(BaseEventData p)
        {
            PointerEventData pointer = (PointerEventData)p;
            if (pointer.button == PointerEventData.InputButton.Left)
            {
                if (!m_Dragging)
                {
                    SelectTrials(new int[] { m_BeginDragTrial }, Input.GetKey(KeyCode.LeftShift));
                }
            }
            else if (pointer.button == PointerEventData.InputButton.Right)
            {
                int numberOfTrials = Data.SubBlocs[0].SubTrials.Length;
                int[] array = new int[numberOfTrials];
                for (int i = 0; i < numberOfTrials; i++) array[i] = i;
                SelectTrials(array, false);
            }
        }
        public void OnBeginDrag()
        {
            m_Dragging = true;
        }
        public void OnEndDrag()
        {
            int numberOfTrials = Data.SubBlocs[0].SubTrials.Length;
            int beginTrial = Mathf.Clamp(m_BeginDragTrial, 0, numberOfTrials - 1); ;
            int endTrial = Mathf.Clamp(GetTrialAtPosition(Input.mousePosition), 0, numberOfTrials - 1);
            List<int> selectedTrials = new List<int>(Mathf.Abs(endTrial - beginTrial));
            if (beginTrial > endTrial)
            {
                for (int i = endTrial; i <= beginTrial; i++)
                {
                    selectedTrials.Add(i);
                }
            }
            else
            {
                for (int i = beginTrial; i <= endTrial; i++)
                {
                    selectedTrials.Add(i);
                }
            }
            SelectTrials(selectedTrials.ToArray(), Input.GetKey(KeyCode.LeftShift));
            m_Dragging = false;
        }
        public void OnScroll()
        {
            int delta = Mathf.RoundToInt(Input.GetAxis("Mouse ScrollWheel") * 10);
            if (delta != 0)
            {
                int numberOfTrials = Data.SubBlocs[0].SubTrials.Length;
                IEnumerable<int> trialsToSelect = m_SelectedTrials.Select(t => Mathf.Clamp(t + delta, 0, numberOfTrials - 1)).Distinct();
                SelectTrials(trialsToSelect.ToArray(), Input.GetKey(KeyCode.LeftShift));  
            }
        }
        #endregion

        #region Private Methods    
        void Awake()
        {
            m_LayoutElement = GetComponent<LayoutElement>();
            m_RectTransform = GetComponent<RectTransform>();
        }
        void SetSize()
        {
            switch (ApplicationState.UserPreferences.Visualization.TrialMatrix.SubBlocFormat)
            {
                case HBP.Data.Enums.BlocFormatType.TrialHeight:
                    m_LayoutElement.preferredHeight = ApplicationState.UserPreferences.Visualization.TrialMatrix.TrialHeight * Data.SubBlocs[0].SubTrials.Length;
                    break;
                case HBP.Data.Enums.BlocFormatType.TrialRatio:
                    m_LayoutElement.preferredHeight = ApplicationState.UserPreferences.Visualization.TrialMatrix.TrialRatio * m_RectTransform.rect.width * Data.SubBlocs[0].SubTrials.Length;
                    break;
                case HBP.Data.Enums.BlocFormatType.BlocRatio:
                    m_LayoutElement.preferredHeight = ApplicationState.UserPreferences.Visualization.TrialMatrix.BlocRatio * m_RectTransform.rect.width;
                    break;
            }
        }
        void AddSubBloc(data.SubBloc data, Texture2D colorMap, Vector2 limits, Tools.CSharp.Window window)
        {
            SubBloc subBloc = (Instantiate(m_SubBlocPrefab, m_SubBlocContainer) as GameObject).GetComponent<SubBloc>();
            subBloc.Set(data, colorMap, limits, window);
            m_SubBlocs.Add(subBloc);
        }
        void AddFiller(Tools.CSharp.Window window)
        {
            GameObject filler = new GameObject("Filler");
            filler.transform.SetParent(m_SubBlocContainer);
            Image image = filler.AddComponent<Image>();
            image.sprite = null;
            image.color = Color.black;
            filler.AddComponent<LayoutElement>().flexibleWidth = window.End - window.Start;
            m_Fillers.Add(filler);
        }
        void Clear()
        {
            foreach (var subBloc in m_SubBlocs)
            {
                Destroy(subBloc.gameObject);
            }
            foreach (var filler in m_Fillers)
            {
                Destroy(filler);
            }
            m_SubBlocs = new List<SubBloc>();
            m_Fillers = new List<GameObject>();
        }
        void AddSelectionMask(int[] trials)
        {
            RectTransform rectTransform = Instantiate(m_SelectionMaskPrefab, m_SelectionMaskContainer).GetComponent<RectTransform>();
            int numberOfTrials = Data.SubBlocs[0].SubTrials.Length;
            float yMax = (float) (trials[trials.Length - 1] + 1) / numberOfTrials;
            float yMin = (float) trials[0] / numberOfTrials;
            rectTransform.anchorMin = new Vector2(0, yMin);
            rectTransform.anchorMax = new Vector2(1, yMax);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 0);
            m_SelectionMasks.Add(rectTransform.gameObject);
        }
        void ClearSelectionMask()
        {
            foreach (var selectionMask in m_SelectionMasks) Destroy(selectionMask);
            m_SelectionMasks.Clear();
        }
        int GetTrialAtPosition(Vector3 position)
        {
            Vector2 ratio = m_RectTransform.GetRatioPosition(position);
            return Mathf.FloorToInt((1-ratio.y) * Data.SubBlocs[0].SubTrials.Length);
        }
        #endregion
    }
}
