﻿using System;
using System.Runtime.InteropServices;

namespace Elan
{
    public class EEG
    {
        #region Properties
        public const string EXTENSION = ".eeg";
        public const string HEADER_EXTENSION = ".ent";

        public enum DataTypeEnum { Float, Double };

        // GENERAL
        public DataTypeEnum DataType
        {
            get
            {
                return (DataTypeEnum)GetDataType(_handle);
            }
            set
            {
                SetDataType((int)value, _handle);
            }
        }
        public int SampleNumber
        {
            get { return GetSampleNumber(_handle); }
            set { SetSampleNumber(value, _handle); }
        }
        public float SamplingFrequency
        {
            get { return GetSamplingFrequency(_handle); }
            set { SetSamplingFrequency(value, _handle); }
        }
        public bool Epoched
        {
            get
            {
                if (GetEpoched(_handle) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    SetEpoched(1, _handle);
                }
                else
                {
                    SetEpoched(0, _handle);
                }
            }
        }

        // EPOCHED SPECIFIC DATA
        public int PreStimSampleNumber
        {
            get
            {
                if (Epoched)
                {
                    return GetPreStimSampleNumber(_handle);
                }
                else
                {
                    Console.WriteLine("EEG is continuous : return 0 for PreStinSampleNumber.");
                    return 0;
                }
            }
            set
            {
                if (Epoched)
                {
                    SetPreStimSampleNumber(value, _handle);
                }
                {
                    Console.WriteLine("EEG is continuous : PreStimSampleNumber can't be set.");
                }
            }
        }
        public int PostStimSampleNumber
        {
            get
            {
                if (Epoched)
                {
                    return GetPostStimSampleNumber(_handle);
                }
                else
                {
                    Console.WriteLine("EEG is continuous : return 0 for PostStinSampleNumber.");
                    return 0;
                }
            }
            set
            {
                if (Epoched)
                {
                    SetPostStimSampleNumber(value, _handle);
                }
                {
                    Console.WriteLine("EEG is continuous : PostStimSampleNumber can't be set.");
                }
            }
        }
        public int EventCode
        {
            get
            {
                if (Epoched)
                {
                    return GetEpochedEventCode(_handle);
                }
                else
                {
                    Console.WriteLine("EEG is continuous : return 0 for EpochedEventCode.");
                    return 0;
                }
            }
            set
            {
                if (Epoched)
                {
                    SetEpochedEventCode(value, _handle);
                }
                {
                    Console.WriteLine("EEG is continuous : EpochedEventCode can't be set.");
                }
            }
        }
        public int EpochNumber
        {
            get
            {
                if (Epoched)
                {
                    return GetEpochNumber(_handle);
                }
                else
                {
                    Console.WriteLine("EEG is continuous : return 0 for EpochNumber.");
                    return 0;
                }
            }
            set
            {
                if (Epoched)
                {
                    SetEpochNumber(value, _handle);
                }
                {
                    Console.WriteLine("EEG is continuous : EpochNumber can't be set.");
                }
            }
        }

        // CONTINIOUS SPECIFIC DATA
        public int EventNumber
        {
            get
            {
                if (!Epoched)
                {
                    return GetEventNumber(_handle);
                }
                else
                {
                    Console.WriteLine("EEG is epoched : return 0 for EventNumber.");
                    return 0;
                }
            }
            private set
            {
                if (!Epoched)
                {
                    SetEventNumber(value, _handle);
                }
                else
                {
                    Console.WriteLine("EEG is epoched : eventNumber can't be set.");
                }
            }
        }
        public int[] EventCodes
        {
            get
            {
                if (!Epoched)
                {
                    int[] codes = new int[EventNumber];
                    GetContinuousEventCode(codes, _handle);
                    return codes;
                }
                else
                {
                    Console.WriteLine("EEG is epoched : return null EventCodes.");
                    return null;
                }
            }
            set
            {
                if (!Epoched)
                {
                    EventNumber = value.Length;
                    SetContinuousEventCode(value, _handle);
                }
                else
                {
                    Console.WriteLine("EEG is epoched : EventCodes can't be set.");
                }
            }
        }
        public int[] EventSamples
        {
            get
            {
                if (!Epoched)
                {
                    int[] samples = new int[EventNumber];
                    GetEventSample(samples, _handle);
                    return samples;
                }
                else
                {
                    Console.WriteLine("EEG is epoched : return null EventSamples.");
                    return null;
                }
            }
            set
            {
                if (!Epoched)
                {
                    EventNumber = value.Length;
                    SetEventSample(value, _handle);
                }
                else
                {
                    Console.WriteLine("EEG is epoched : EventSamples can't be set.");
                }
            }
        }

        HandleRef _handle;
        bool[,] readed;
        #endregion

        #region Constructor
        public EEG(bool[,] readed, HandleRef _handle)
        {
            this._handle = _handle;
            this.readed = readed;
        }
        #endregion

        #region Public Methods
        public float[,][] GetFloatData()
        {
            float[,][] data = new float[readed.GetLength(0),readed.GetLength(1)][];
            for (int m = 0; m < data.GetLength(0); m++)
            {
                for (int c = 0; c < data.GetLength(1); c++)
                {
                    data[m,c] = GetFloatData(new Track(m, c));
                }
            }
            return data;
        }
        public float[][] GetFloatData(Track[] tracks)
        {
            float[][] data = new float[tracks.Length][];
            for (int i = 0; i < tracks.Length; i++)
            {
                data[i] = GetFloatData(tracks[i]);
            }
            return data;
        }
        public float[] GetFloatData(Track track)
        {
            float[] data = new float[SampleNumber];
            if(DataType == DataTypeEnum.Float)
            { 
                if (readed[track.Measure, track.Channel])
                {
                    GetFloatData(data, track.Measure, track.Channel, _handle);
                }
            }
            return data;
        }

        public double[,][] GetDoubleData()
        {
            double[,][] data = new double[readed.GetLength(0), readed.GetLength(1)][];
            for (int m = 0; m < data.GetLength(0); m++)
            {
                for (int c = 0; c < data.GetLength(1); c++)
                {
                    data[m, c] = GetDoubleData(new Track(m, c));
                }
            }
            return data;
        }
        public double[][] GetDoubleData(Track[] tracks)
        {
            double[][] data = new double[tracks.Length][];
            for (int i = 0; i < tracks.Length; i++)
            {
                data[i] = GetDoubleData(tracks[i]);
            }
            return data;
        }
        public double[] GetDoubleData(Track track)
        {
            double[] data = new double[SampleNumber];
            if (DataType == DataTypeEnum.Double)
            {
                if (readed[track.Measure, track.Channel])
                {
                    GetDoubleData(data, track.Measure, track.Channel, _handle);
                }
            }
            return data;
        }
        #endregion

        #region DLLImport
        [DllImport("elan", EntryPoint = "EEG_GetDataType", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetDataType(HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetDataType", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetDataType(int dataType, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetSampleNumber", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetSampleNumber(HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetSampleNumber", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetSampleNumber(int sampleNumber, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetSamplingFrequency", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern float GetSamplingFrequency(HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetSamplingFrequency", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetSamplingFrequency(float samplingFrequency, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetEpoched", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetEpoched(HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetEpoched", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetEpoched(int epoched, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetPreStimSampleNumber", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetPreStimSampleNumber(HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetPreStimSampleNumber", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetPreStimSampleNumber(int preStimSampleNumber, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetPostStimSampleNumber", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetPostStimSampleNumber(HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetPostStimSampleNumber", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetPostStimSampleNumber(int postStimSampleNumber, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetEpochedEventCode", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetEpochedEventCode(HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetEpochedEventCode", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetEpochedEventCode(int eventCode, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetEpochNumber", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetEpochNumber(HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetEpochNumber", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetEpochNumber(int epochNumber, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetEventNumber", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetEventNumber(HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetEventNumber", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetEventNumber(int eventNumber, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetContinuousEventCode", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetContinuousEventCode([Out] int[] eventCodes, HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetContinuousEventCode", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetContinuousEventCode([In] int[] eventCodes, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetEventSample", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void GetEventSample([Out] int[] eventSample, HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetEventSample", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetEventSample([In] int[] eventSample, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetFloatData", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void GetFloatData([Out] float[] data, HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetFloatData", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetFloatData([In] float[] data, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetFloatDataChannel", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void GetFloatData([Out] float[] data,int measure,int channel, HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetFloatDataChannel", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetFloatData([In] float[] data, int measure,int channel, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetDoubleData", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void GetDoubleData([Out] double[] data, HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetDoubleData", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetDoubleData([In] double[] data, HandleRef cppStruct);

        [DllImport("elan", EntryPoint = "EEG_GetDoubleDataChannel", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetDoubleData([Out] double[] data,int measure, int channel, HandleRef cppStruct);
        [DllImport("elan", EntryPoint = "EEG_SetDoubleDataChannel", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetDoubleData([In] double[] data, int measure, int channel, HandleRef cppStruct);
        #endregion
    }
}