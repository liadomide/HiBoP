﻿using HBP.Data.Experience.Dataset;
using HBP.Data.Localizer;
using System.Collections.Generic;
using System.Linq;

namespace HBP.Data.Visualization
{
    public class IEEGData
    {
        #region Properties
        public IconicScenario IconicScenario { get; set; }
        public Timeline Timeline { get; set; }
        public Dictionary<Patient, BlocEventsStatistics> EventStatisticsByPatient { get; set; } = new Dictionary<Patient, BlocEventsStatistics>();
        public Dictionary<string, BlocChannelData> DataByChannel { get; set; } = new Dictionary<string, BlocChannelData>();
        public Dictionary<string, BlocChannelStatistics> StatisticsByChannel { get; set; } = new Dictionary<string, BlocChannelStatistics>();
        
        private Dictionary<string, Frequency> m_FrequencyByChannel = new Dictionary<string, Frequency>();
        public List<Frequency> Frequencies = new List<Frequency>();
        #endregion

        #region Public Methods
        public void Load(IEnumerable<DataInfo> columnData, Experience.Protocol.Bloc bloc)
        {
            foreach (DataInfo dataInfo in columnData)
            {
                Experience.Dataset.Data data = DataManager.GetData(dataInfo);
                // Values
                foreach (var channel in data.UnitByChannel.Keys)
                {
                    if (!DataByChannel.ContainsKey(dataInfo.Patient.ID + "_" + channel)) DataByChannel.Add(dataInfo.Patient.ID + "_" + channel, DataManager.GetData(dataInfo, bloc, channel));
                    if (!StatisticsByChannel.ContainsKey(dataInfo.Patient.ID + "_" + channel)) StatisticsByChannel.Add(dataInfo.Patient.ID + "_" + channel, DataManager.GetStatistics(dataInfo, bloc, channel));
                    if (!m_FrequencyByChannel.ContainsKey(dataInfo.Patient.ID + "_" + channel)) m_FrequencyByChannel.Add(dataInfo.Patient.ID + "_" + channel, data.Frequency);
                    if (!Frequencies.Contains(data.Frequency)) Frequencies.Add(data.Frequency);
                }
                // Events
                if (!EventStatisticsByPatient.ContainsKey(dataInfo.Patient)) EventStatisticsByPatient.Add(dataInfo.Patient, DataManager.GetEventsStatistics(dataInfo, bloc));
            }
        }
        public void Unload()
        {
            EventStatisticsByPatient.Clear();
            DataByChannel.Clear();
            StatisticsByChannel.Clear();
            m_FrequencyByChannel.Clear();
            Frequencies.Clear();
            IconicScenario = null;
            Timeline = null;
        }
        public void SetTimeline(Frequency maxFrequency, Experience.Protocol.Bloc bloc)
        {
            Frequencies.Add(maxFrequency);
            Frequencies = Frequencies.GroupBy(f => f.Value).Select(g => g.First()).ToList();
            Dictionary<Experience.Protocol.SubBloc, List<SubBlocEventsStatistics>> eventStatisticsBySubBloc = new Dictionary<Experience.Protocol.SubBloc, List<SubBlocEventsStatistics>>();
            foreach (var subBloc in bloc.SubBlocs)
            {
                eventStatisticsBySubBloc.Add(subBloc, new List<SubBlocEventsStatistics>());
            }
            foreach (var blocEventStatistics in EventStatisticsByPatient.Values)
            {
                foreach (var subBlocEventStatistics in blocEventStatistics.EventsStatisticsBySubBloc)
                {
                    eventStatisticsBySubBloc[subBlocEventStatistics.Key].Add(subBlocEventStatistics.Value);
                }
            }
            Timeline = new Timeline(bloc, eventStatisticsBySubBloc, maxFrequency);
            IconicScenario = new IconicScenario(bloc, maxFrequency, Timeline);

            //// Resampling taking frequencies into account
            //if (Frequencies.Count > 1)
            //{
            //    //int maxSize = (from siteConfiguration in Configuration.ConfigurationBySite.Values select siteConfiguration.Values).Max(v => v.Length);
            //    foreach (var siteConfiguration in Configuration.ConfigurationBySite.Values)
            //    {
            //        int frequency;
            //        if (m_FrequencyBySiteConfiguration.TryGetValue(siteConfiguration, out frequency))
            //        {
            //            Timeline timeline = TimeLineByFrequency[frequency];
            //            int samplesBefore = (int)((timeline.Start.RawValue - TimeLine.Start.RawValue) / TimeLine.Step);
            //            int samplesAfter = (int)((TimeLine.End.RawValue - timeline.End.RawValue) / TimeLine.Step);
            //            siteConfiguration.ResizeValues(TimeLine.Lenght, samplesBefore, samplesAfter);
            //        }
            //    }
            //}
        }
        /// <summary>
        /// Standardize column.
        /// </summary>
        /// <param name="before">sample before</param>
        /// <param name="after">sample after</param>
        public void Standardize(int before, int after)
        {
            //int diffBefore = before - TimeLine.MainEvent.Position;
            //int diffAfter = after - (TimeLine.Lenght - TimeLine.MainEvent.Position);
            //TimeLine.Resize(diffBefore, diffAfter);
            //foreach (var siteConfiguration in IEEGConfiguration.ConfigurationBySite.Values)
            //{
            //    siteConfiguration.Resize(diffBefore, diffAfter);
            //}
        }
        #endregion
    }
}
