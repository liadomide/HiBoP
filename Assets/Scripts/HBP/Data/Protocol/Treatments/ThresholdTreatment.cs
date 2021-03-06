﻿using System.ComponentModel;
using System.Runtime.Serialization;
using Tools.CSharp;
using Tools.CSharp.EEG;

namespace HBP.Data.Experience.Protocol
{
    [DataContract, DisplayName("Threshold")]
    public class ThresholdTreatment : Treatment
    {
        #region Properties
        [DataMember] public bool UseMinTreshold { get; set; }
        [DataMember] public float Min { get; set; }
        [DataMember] public bool UseMaxTreshold { get; set; }
        [DataMember] public float Max { get; set; }
        #endregion

        #region Constructors
        public ThresholdTreatment(): base()
        {
            UseMinTreshold = false;
            UseMaxTreshold = false;
            Min = 0;
            Max = 1;
        }
        public ThresholdTreatment(string ID) : base(ID)
        {
            UseMinTreshold = false;
            UseMaxTreshold = false;
            Min = 0;
            Max = 1;
        }
        public ThresholdTreatment(bool useOnWindow, Window window, bool useOnBaseline, Window baseline, bool useMinTreshold, float min, bool useMaxTreshold, float max, int order, string ID) : base(useOnWindow, window, useOnBaseline, baseline, order, ID)
        {
            UseMinTreshold = useMinTreshold;
            Min = min;
            UseMaxTreshold = useMaxTreshold;
            Max = max;
        }
        #endregion

        #region Public Methods
        public override void Apply(ref float[] values, ref float[] baseline, int windowMainEventIndex, int baselineMainEventIndex, Frequency frequency)
        {
            if(UseOnWindow)
            {
                int startIndex = windowMainEventIndex + frequency.ConvertToCeiledNumberOfSamples(Window.Start);
                int endIndex = windowMainEventIndex + frequency.ConvertToFlooredNumberOfSamples(Window.End);
                if (UseMinTreshold && !UseMaxTreshold)
                {
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        if (values[i] > Min) values[i] = 0;
                    }
                }
                else if (!UseMinTreshold && UseMaxTreshold)
                {
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        if (values[i] < Max) values[i] = 0;
                    }
                }
                else if (UseMinTreshold && UseMaxTreshold)
                {
                    float value;
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        value = values[i];
                        if (value < Max && value > Min) values[i] = 0;
                    }
                }
            }

            if(UseOnBaseline)
            {
                int startIndex = baselineMainEventIndex + frequency.ConvertToCeiledNumberOfSamples(Baseline.Start);
                int endIndex = baselineMainEventIndex + frequency.ConvertToFlooredNumberOfSamples(Baseline.End);
                if (UseMinTreshold && !UseMaxTreshold)
                {
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        if (baseline[i] > Min) baseline[i] = 0;
                    }
                }
                else if (!UseMinTreshold && UseMaxTreshold)
                {
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        if (baseline[i] < Max) baseline[i] = 0;
                    }
                }
                else if (UseMinTreshold && UseMaxTreshold)
                {
                    float value;
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        value = baseline[i];
                        if (value < Max && value > Min) baseline[i] = 0;
                    }
                }
            }

        }
        #endregion

        #region Operators
        public override object Clone()
        {
            return new ThresholdTreatment(UseOnWindow, Window, UseOnBaseline, Baseline, UseMinTreshold, Min, UseMaxTreshold, Max, Order, ID);
        }
        public override void Copy(object copy)
        {
            base.Copy(copy);
            if(copy is ThresholdTreatment tresholdTreatment)
            {
                UseMinTreshold = tresholdTreatment.UseMinTreshold;
                Min = tresholdTreatment.Min;
                UseMaxTreshold = tresholdTreatment.UseMaxTreshold;
                Max = tresholdTreatment.Max;
            }
            if(copy is ClampTreatment clampTreatment)
            {
                UseMinTreshold = clampTreatment.UseMinClamp;
                Min = clampTreatment.Min;
                UseMaxTreshold = clampTreatment.UseMaxClamp;
                Max = clampTreatment.Max;
            }
        }
        #endregion
    }
}