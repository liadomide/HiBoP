﻿using HBP.Module3D;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HBP.UI.Module3D
{
    public class ThresholdIEEG : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Texture to be applied to the image
        /// </summary>
        private Texture2D m_IEEGHistogram;
        /// <summary>
        /// Minimum Span value
        /// </summary>
        private float m_SpanMinFactor = 0.0f;
        private float SpanMin { get { return m_SpanMinFactor * m_Amplitude + m_MinAmplitude; } }
        /// <summary>
        /// Middle Span value
        /// </summary>
        private float m_MiddleFactor = 0.5f;
        private float Middle { get { return m_MiddleFactor * m_Amplitude + m_MinAmplitude; } }
        /// <summary>
        /// Maximum Span value
        /// </summary>
        private float m_SpanMaxFactor = 1.0f;
        private float SpanMax { get { return m_SpanMaxFactor * m_Amplitude + m_MinAmplitude; } }
        /// <summary>
        /// Minimum value
        /// </summary>
        private float m_MinAmplitude = float.MinValue;
        /// <summary>
        /// Maximum value
        /// </summary>
        private float m_MaxAmplitude = float.MaxValue;
        /// <summary>
        /// Amplitude
        /// </summary>
        private float m_Amplitude = 1.0f;
        /// <summary>
        /// Is the module initialized ?
        /// </summary>
        private bool m_Initialized = false;

        /// <summary>
        /// IEEG Histogram
        /// </summary>
        [SerializeField]
        private Image m_Histogram;
        /// <summary>
        /// Symmetry toggle
        /// </summary>
        [SerializeField]
        private Toggle m_SymmetryToggle;
        /// <summary>
        /// Text field for the min value
        /// </summary>
        [SerializeField]
        private Text m_MinText;
        /// <summary>
        /// Text field for the max value
        /// </summary>
        [SerializeField]
        private Text m_MaxText;
        /// <summary>
        /// Input field for the span min value
        /// </summary>
        [SerializeField]
        private InputField m_SpanMinInput;
        /// <summary>
        /// Input field for the middle value
        /// </summary>
        [SerializeField]
        private InputField m_MiddleInput;
        /// <summary>
        /// Input field for the span max value
        /// </summary>
        [SerializeField]
        private InputField m_SpanMaxInput;
        /// <summary>
        /// Input field for the amplitude
        /// </summary>
        [SerializeField]
        private InputField m_AmplitudeInput;
        /// <summary>
        /// Zone in which the handlers can move
        /// </summary>
        [SerializeField]
        private RectTransform m_HandlerZone;
        /// <summary>
        /// Handler responsible for the minimum value
        /// </summary>
        [SerializeField]
        private ThresholdHandler m_MinHandler;
        /// <summary>
        /// Handler responsible for the middle value
        /// </summary>
        [SerializeField]
        private ThresholdHandler m_MidHandler;
        /// <summary>
        /// Handler responsible for the maximum value
        /// </summary>
        [SerializeField]
        private ThresholdHandler m_MaxHandler;

        public GenericEvent<float, float, float> OnChangeValues = new GenericEvent<float, float, float>();
        #endregion

        #region Private Methods
        /// <summary>
        /// Update IEEG Histogram Texture
        /// </summary>
        private void UpdateIEEGHistogram()
        {
            UnityEngine.Profiling.Profiler.BeginSample("IEEG HISTOGRAM");
            float[] iEEGValues = ((Column3DIEEG)ApplicationState.Module3D.SelectedScene.ColumnManager.SelectedColumn).IEEGValuesForHistogram;
            if (!m_IEEGHistogram)
            {
                m_IEEGHistogram = new Texture2D(1, 1);
            }
            if(iEEGValues.Length > 0)
            {
                HBP.Module3D.DLL.Texture.GenerateDistributionHistogram(iEEGValues, 4 * 110, 4 * 110, m_MinAmplitude, m_MaxAmplitude).UpdateTexture2D(m_IEEGHistogram);
            }
            else
            {
                m_IEEGHistogram = Texture2D.blackTexture;
            }

            Destroy(m_Histogram.sprite);
            m_Histogram.sprite = Sprite.Create(m_IEEGHistogram, new Rect(0, 0, m_IEEGHistogram.width, m_IEEGHistogram.height), new Vector2(0.5f, 0.5f), 400f);
            UnityEngine.Profiling.Profiler.EndSample();
        }
        #endregion

        #region Public Methods
        public void Initialize()
        {
            m_IEEGHistogram = new Texture2D(1, 1);

            m_MinHandler.MinimumPosition = float.MinValue;
            m_MinHandler.MaximumPosition = 1.0f;
            m_MaxHandler.MinimumPosition = 0.0f;
            m_MaxHandler.MaximumPosition = float.MaxValue;
            m_MidHandler.MinimumPosition = float.MinValue;
            m_MidHandler.MaximumPosition = float.MaxValue;

            m_SpanMinInput.onEndEdit.AddListener((value) =>
            {
                float val = float.Parse(value);
                if (val > Middle) val = Middle;
                m_SpanMinInput.text = val.ToString("N3");
                m_SpanMinFactor = (val - m_MinAmplitude) / m_Amplitude;

                m_MinHandler.Position = m_SpanMinFactor;
                m_MidHandler.MinimumPosition = m_SpanMinFactor;
                m_MidHandler.ClampPosition();

                if (m_Initialized)
                {
                    OnChangeValues.Invoke(SpanMin, Middle, SpanMax);
                }
            });

            m_MiddleInput.onEndEdit.AddListener((value) =>
            {
                float val = float.Parse(value);
                val = Mathf.Clamp(val, SpanMin, SpanMax);
                m_MiddleInput.text = val.ToString("N3");
                m_MiddleFactor = (val - m_MinAmplitude) / m_Amplitude;

                m_MidHandler.Position = m_MiddleFactor;
                m_MinHandler.MaximumPosition = m_MiddleFactor;
                m_MinHandler.ClampPosition();
                m_MaxHandler.MinimumPosition = m_MiddleFactor;
                m_MaxHandler.ClampPosition();

                if (m_Initialized)
                {
                    OnChangeValues.Invoke(SpanMin, Middle, SpanMax);
                }
            });

            m_SpanMaxInput.onEndEdit.AddListener((value) =>
            {
                float val = float.Parse(value);
                if (val < Middle) val = Middle;
                m_SpanMaxInput.text = val.ToString("N3");
                m_SpanMaxFactor = (val - m_MinAmplitude) / m_Amplitude;

                m_MaxHandler.Position = m_SpanMaxFactor;
                m_MidHandler.MaximumPosition = m_SpanMaxFactor;
                m_MidHandler.ClampPosition();

                if (m_Initialized)
                {
                    OnChangeValues.Invoke(SpanMin, Middle, SpanMax);
                }
            });

            m_AmplitudeInput.onEndEdit.AddListener((value) =>
            {
                float val = float.Parse(value);
                m_AmplitudeInput.text = val.ToString("N3");
                float minVal = Middle - val;
                float maxVal = Middle + val;
                m_SpanMinInput.text = minVal.ToString("N3");
                m_SpanMaxInput.text = maxVal.ToString("N3");
                m_SpanMinInput.onEndEdit.Invoke(m_SpanMinInput.text);
                m_SpanMaxInput.onEndEdit.Invoke(m_SpanMaxInput.text);
            });

            m_MinHandler.OnChangePosition.AddListener((deplacement) =>
            {
                m_SpanMinFactor = m_MinHandler.Position;
                m_SpanMinInput.text = SpanMin.ToString("N3");
                m_SpanMinInput.onEndEdit.Invoke(m_SpanMinInput.text);
                if (m_SymmetryToggle.isOn)
                {
                    m_MaxHandler.Position = m_MidHandler.Position + (m_MidHandler.Position - m_MinHandler.Position);
                    m_SpanMaxFactor = m_MaxHandler.Position;
                    m_SpanMaxInput.text = SpanMax.ToString("N3");
                    m_SpanMaxInput.onEndEdit.Invoke(m_SpanMaxInput.text);
                    m_AmplitudeInput.text = (SpanMax - SpanMin).ToString("N3");
                }
            });

            m_MidHandler.OnChangePosition.AddListener((deplacement) =>
            {
                m_MiddleFactor = m_MidHandler.Position;
                m_MiddleInput.text = Middle.ToString("N3");
                if (m_SymmetryToggle.isOn)
                {
                    m_MinHandler.Position += deplacement;
                    m_SpanMinFactor = m_MinHandler.Position;
                    m_SpanMinInput.text = SpanMin.ToString("N3");
                    m_SpanMinInput.onEndEdit.Invoke(m_SpanMinInput.text);

                    m_MaxHandler.Position += deplacement;
                    m_SpanMaxFactor = m_MaxHandler.Position;
                    m_SpanMaxInput.text = SpanMax.ToString("N3");
                    m_SpanMaxInput.onEndEdit.Invoke(m_SpanMaxInput.text);
                }
                m_MiddleInput.onEndEdit.Invoke(m_MiddleInput.text);
            });

            m_MaxHandler.OnChangePosition.AddListener((deplacement) =>
            {
                m_SpanMaxFactor = m_MaxHandler.Position;
                m_SpanMaxInput.text = SpanMax.ToString("N3");
                m_SpanMaxInput.onEndEdit.Invoke(m_SpanMaxInput.text);
                if (m_SymmetryToggle.isOn)
                {
                    m_MinHandler.Position = m_MidHandler.Position - (m_MaxHandler.Position - m_MidHandler.Position);
                    m_SpanMinFactor = m_MinHandler.Position;
                    m_SpanMinInput.text = SpanMin.ToString("N3");
                    m_SpanMinInput.onEndEdit.Invoke(m_SpanMinInput.text);
                    m_AmplitudeInput.text = (SpanMax - SpanMin).ToString("N3");
                }
            });
        }
        /// <summary>
        /// Update IEEG values
        /// </summary>
        /// <param name="values">IEEG data values</param>
        public void UpdateIEEGValues(Column3DIEEG.IEEGDataParameters values)
        {
            m_Initialized = false;

            m_MinAmplitude = values.MinimumAmplitude;
            m_MaxAmplitude = values.MaximumAmplitude;
            m_Amplitude = m_MaxAmplitude - m_MinAmplitude;
            m_SpanMinFactor = (values.SpanMin - m_MinAmplitude) / m_Amplitude;
            m_MiddleFactor = (values.Middle - m_MinAmplitude) / m_Amplitude;
            m_SpanMaxFactor = (values.SpanMax - m_MinAmplitude) / m_Amplitude;

            m_MinText.text = m_MinAmplitude.ToString("N3");
            m_MaxText.text = m_MaxAmplitude.ToString("N3");
            m_SpanMinInput.text = values.SpanMin.ToString("N3");
            m_MiddleInput.text = values.Middle.ToString("N3");
            m_SpanMaxInput.text = values.SpanMax.ToString("N3");
            m_AmplitudeInput.text = ((values.SpanMax - values.SpanMin) / 2).ToString("N3");

            m_SpanMinInput.onEndEdit.Invoke(m_SpanMinInput.text);
            m_MiddleInput.onEndEdit.Invoke(m_MiddleInput.text);
            m_SpanMaxInput.onEndEdit.Invoke(m_SpanMaxInput.text);

            m_MinHandler.MaximumPosition = m_MidHandler.Position;
            m_MidHandler.MinimumPosition = m_MinHandler.Position;
            m_MidHandler.MaximumPosition = m_MaxHandler.Position;
            m_MaxHandler.MinimumPosition = m_MidHandler.Position;

            UpdateIEEGHistogram();

            m_Initialized = true;
        }
        #endregion
    }
}