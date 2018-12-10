﻿using System.Collections.Generic;
using HBP.Data.Experience;
using HBP.Data.Experience.Dataset;
using HBP.Data.Experience.Protocol;
using System.Linq;
using Tools.CSharp;
using System;

public static class DataManager
{
    #region Properties
    // Data.
    static Dictionary<Request, Data> m_DataByRequest = new Dictionary<Request, Data>();
    static Dictionary<BlocRequest, BlocData> m_BlocDataByRequest = new Dictionary<BlocRequest, BlocData>();

    static Dictionary<ChannelRequest, ChannelData> m_ChannelDataByRequest = new Dictionary<ChannelRequest, ChannelData>();
    static Dictionary<BlocChannelRequest, BlocChannelData> m_BlocChannelDataByRequest = new Dictionary<BlocChannelRequest, BlocChannelData>();
    
    // Statistics.
    static Dictionary<ChannelRequest, ChannelStatistics> m_ChannelStatisticsByRequest = new Dictionary<ChannelRequest, ChannelStatistics>();
    static Dictionary<BlocChannelRequest, BlocChannelStatistics> m_BlocChannelStatisticsByRequest = new Dictionary<BlocChannelRequest, BlocChannelStatistics>();

    static Dictionary<Request, EventsStatistics> m_EventsStatisticsByRequest = new Dictionary<Request, EventsStatistics>();
    static Dictionary<BlocRequest, BlocEventsStatistics> m_BlocEventsStatisticsByRequest = new Dictionary<BlocRequest, BlocEventsStatistics>();

    static Dictionary<BlocRequest, HBP.Data.Enums.NormalizationType> m_NormalizeByRequest = new Dictionary<BlocRequest, HBP.Data.Enums.NormalizationType>();
    public static bool HasData
    {
        get
        {
            return m_DataByRequest.Count > 0
                || m_BlocDataByRequest.Count > 0
                || m_ChannelDataByRequest.Count > 0
                || m_BlocChannelDataByRequest.Count > 0
                || m_ChannelStatisticsByRequest.Count > 0
                || m_BlocChannelStatisticsByRequest.Count > 0
                || m_EventsStatisticsByRequest.Count > 0
                || m_BlocEventsStatisticsByRequest.Count > 0;
        }
    }
    #endregion

    #region Public Methods
    // General.
    public static void Load(DataInfo dataInfo)
    {
        Load(new Request(dataInfo));
    }
    public static void UnLoad(DataInfo dataInfo)
    {
        UnLoad(new Request(dataInfo));
    }
    public static void Reload(DataInfo dataInfo)
    {
        UnLoad(dataInfo);
        Load(dataInfo);
    }
    public static void Clear()
    {
        m_DataByRequest.Clear();
        m_BlocDataByRequest.Clear();

        m_ChannelDataByRequest.Clear();
        m_BlocChannelDataByRequest.Clear();

        m_ChannelStatisticsByRequest.Clear();
        m_BlocChannelStatisticsByRequest.Clear();

        m_EventsStatisticsByRequest.Clear();
        m_BlocEventsStatisticsByRequest.Clear();

        m_NormalizeByRequest.Clear();
    }

    // Data.
    public static Data GetData(DataInfo dataInfo)
    {
        return GetData(new Request(dataInfo));
    }
    public static BlocData GetData(DataInfo dataInfo,Bloc bloc)
    {
        return GetData(new BlocRequest(dataInfo, bloc));
    }
    public static ChannelData GetData(DataInfo dataInfo, string channel)
    {
        return GetData(new ChannelRequest(dataInfo, channel));
    }
    public static BlocChannelData GetData(DataInfo dataInfo, Bloc bloc, string channel)
    {
        return GetData(new BlocChannelRequest(dataInfo, bloc, channel));
    }

    // Statistics.
    public static ChannelStatistics GetStatistics(DataInfo dataInfo, string channel)
    {
        return GetStatistics(new ChannelRequest(dataInfo, channel));
    }
    public static BlocChannelStatistics GetStatistics(DataInfo dataInfo, Bloc bloc, string channel)
    {
        return GetStatistics(new BlocChannelRequest(dataInfo, bloc, channel));
    }
    public static EventsStatistics GetEventsStatistics(DataInfo dataInfo)
    {
        return GetEventsStatistics(new Request(dataInfo));
    }
    public static BlocEventsStatistics GetEventsStatistics(DataInfo dataInfo, Bloc bloc)
    {
        return GetEventsStatistics(new BlocRequest(dataInfo, bloc));
    }

    public static void NormalizeData()
    {
        IEnumerable<DataInfo> dataInfoCollection = m_DataByRequest.Select((d) => d.Key.DataInfo).Distinct();
        foreach (var dataInfo in dataInfoCollection)
        {
            IEnumerable<BlocRequest> dataRequestCollection = m_BlocDataByRequest.Where((d) => d.Key.DataInfo == dataInfo).Select((d) => d.Key);
            switch (dataInfo.Normalization)
            {
                case DataInfo.NormalizationType.None:
                    foreach (var request in dataRequestCollection) if (m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.None) NormalizeByNone(request);
                    break;
                case DataInfo.NormalizationType.SubTrial:
                    foreach (var request in dataRequestCollection) if (m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.SubTrial) NormalizeBySubTrial(request);
                    break;
                case DataInfo.NormalizationType.Trial:
                    foreach (var request in dataRequestCollection) if (m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.Trial) NormalizeByTrial(request);
                    break;
                case DataInfo.NormalizationType.SubBloc:
                    foreach (var request in dataRequestCollection) if (m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.SubBloc) NormalizeBySubBloc(request);
                    break;
                case DataInfo.NormalizationType.Bloc:
                    foreach (var request in dataRequestCollection) if (m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.Bloc) NormalizeByBloc(request);
                    break;
                case DataInfo.NormalizationType.Protocol:
                    IEnumerable<Tuple<BlocRequest, bool>> dataRequestAndNeedToNormalize = from request in dataRequestCollection select new Tuple<BlocRequest, bool>(request, m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.Protocol);
                    if (dataRequestAndNeedToNormalize.Any((tuple) => tuple.Item2))
                    {
                        NormalizeByProtocol(dataRequestAndNeedToNormalize);
                    }
                    break;
                case DataInfo.NormalizationType.Auto:
                    switch (ApplicationState.UserPreferences.Data.EEG.Normalization)
                    {
                        case HBP.Data.Enums.NormalizationType.None:
                            foreach (var request in dataRequestCollection) if (m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.None) NormalizeByNone(request);
                            break;
                        case HBP.Data.Enums.NormalizationType.SubTrial:
                            foreach (var request in dataRequestCollection) if (m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.SubTrial) NormalizeBySubTrial(request);
                            break;
                        case HBP.Data.Enums.NormalizationType.Trial:
                            foreach (var request in dataRequestCollection) if (m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.Trial) NormalizeByTrial(request);
                            break;
                        case HBP.Data.Enums.NormalizationType.SubBloc:
                            foreach (var request in dataRequestCollection) if (m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.SubBloc) NormalizeBySubBloc(request);
                            break;
                        case HBP.Data.Enums.NormalizationType.Bloc:
                            foreach (var request in dataRequestCollection) if (m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.Bloc) NormalizeByBloc(request);
                            break;
                        case HBP.Data.Enums.NormalizationType.Protocol:
                            IEnumerable<Tuple<BlocRequest, bool>> dataRequestAndNeedToNormalize2 = from request in dataRequestCollection select new Tuple<BlocRequest, bool>(request, m_NormalizeByRequest[request] != HBP.Data.Enums.NormalizationType.Protocol);
                            if (dataRequestAndNeedToNormalize2.Any((tuple) => tuple.Item2))
                            {
                                NormalizeByProtocol(dataRequestAndNeedToNormalize2);
                            }
                            break;
                    }
                    break;
            }
        }
    }
    #endregion

    #region Private Methods
    static void Load(Request request)
    {
        if (request.IsValid && !m_DataByRequest.ContainsKey(request))
        {
            Data data = new Data(request.DataInfo);
            m_DataByRequest.Add(request, data);

            Protocol protocol = request.DataInfo.Dataset.Protocol;
            foreach (var bloc in protocol.Blocs)
            {
                m_BlocDataByRequest.Add(new BlocRequest(request.DataInfo, bloc), data.DataByBloc[bloc]);
                m_NormalizeByRequest.Add(new BlocRequest(request.DataInfo, bloc), HBP.Data.Enums.NormalizationType.None);
            }
        }
    }
    static void UnLoad(Request request)
    {
        if(request.IsValid && m_DataByRequest.ContainsKey(request))
        {
            m_DataByRequest.Remove(request);

            IEnumerable<ChannelRequest> channelDataRequestsToRemove = m_ChannelDataByRequest.Keys.Where(k => k.DataInfo == request.DataInfo);
            foreach (var channelDataRequest in channelDataRequestsToRemove)
            {
                m_ChannelDataByRequest.Remove(channelDataRequest);
            }

            IEnumerable<BlocChannelRequest> blocChannelDataRequestsToRemove = m_BlocChannelDataByRequest.Keys.Where(k => k.DataInfo == request.DataInfo);
            foreach (var blocChannelDataRequest in blocChannelDataRequestsToRemove)
            {
                m_BlocChannelDataByRequest.Remove(blocChannelDataRequest);
            }

            IEnumerable<BlocRequest> blocDataRequestsToRemove = m_BlocDataByRequest.Keys.Where(k => k.DataInfo == request.DataInfo);
            foreach (var blocDataRequest in blocDataRequestsToRemove)
            {
                m_BlocDataByRequest.Remove(blocDataRequest);
            }
        }
    }
    
    static Data GetData(Request request)
    {
        if (request.IsValid)
        {
            Data result;
            if (m_DataByRequest.TryGetValue(request, out result))
            {
                return result;
            }
            else
            {
                Load(request);
                return m_DataByRequest[request];
            }
        }
        else
        {
            return null;
        }
    }
    static BlocData GetData(BlocRequest request)
    {
        if (request.IsValid)
        {
            BlocData result;
            if (m_BlocDataByRequest.TryGetValue(request, out result))
            {
                return result;
            }
            else
            {
                Load(request.DataInfo);
                return m_BlocDataByRequest[request];
            }
        }
        else
        {
            return null;
        }
    }
    static ChannelData GetData(ChannelRequest request)
    {
        if (request.IsValid)
        {
            ChannelData result;
            if (m_ChannelDataByRequest.TryGetValue(request, out result))
            {
                return result;
            }
            else
            {
                Request dataRequest = new Request(request.DataInfo);
                if (!m_DataByRequest.ContainsKey(dataRequest))
                {
                    Load(dataRequest);
                }
                ChannelData channelData = new ChannelData(m_DataByRequest[dataRequest], request.Channel);
                m_ChannelDataByRequest.Add(request, channelData);
                return channelData;
            }
        }
        else
        {
            return null;
        }
    }
    static BlocChannelData GetData(BlocChannelRequest request)
    {
        if (request.IsValid)
        {
            BlocChannelData result;
            if (m_BlocChannelDataByRequest.TryGetValue(request, out result))
            {
                return result;
            }
            else
            {
                Request dataRequest = new Request(request.DataInfo);
                Data data = GetData(dataRequest);
                if (data != null)
                {
                    if(data.UnitByChannel.ContainsKey(request.Channel))
                    {
                        BlocRequest blocDataRequest = new BlocRequest(request.DataInfo, request.Bloc);
                        BlocChannelData blocChannelData = new BlocChannelData(m_BlocDataByRequest[blocDataRequest], request.Channel);
                        m_BlocChannelDataByRequest.Add(request, blocChannelData);
                        return blocChannelData;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        else
        {
            return null;
        }
    }

    // Statistics.
    static ChannelStatistics GetStatistics(ChannelRequest request)
    {
        if (request.IsValid)
        {
            ChannelStatistics result;
            if (m_ChannelStatisticsByRequest.TryGetValue(request, out result))
            {
                return result;
            }
            else
            {
                ChannelData channelData = GetData(request);
                ChannelStatistics channelStatistics = new ChannelStatistics(channelData);
                m_ChannelStatisticsByRequest.Add(request, channelStatistics);
                return channelStatistics;
            }
        }
        else
        {
            return null;
        }
    }
    static BlocChannelStatistics GetStatistics(BlocChannelRequest request)
    {
        if (request.IsValid)
        {
            BlocChannelStatistics result;
            if (m_BlocChannelStatisticsByRequest.TryGetValue(request, out result))
            {
                return result;
            }
            else
            {
                BlocChannelData blocChannelData = GetData(request);
                BlocChannelStatistics blocChannelStatistics = new BlocChannelStatistics(blocChannelData);
                m_BlocChannelStatisticsByRequest.Add(request, blocChannelStatistics);
                return blocChannelStatistics;
            }
        }
        else
        {
            return null;
        }
    }
    static EventsStatistics GetEventsStatistics(Request request)
    {
        if (request.IsValid)
        {
            EventsStatistics result;
            if (m_EventsStatisticsByRequest.TryGetValue(request, out result))
            {
                return result;
            }
            else
            {
                EventsStatistics eventsStatistics = new EventsStatistics(request.DataInfo);
                foreach (var pair in eventsStatistics.EventsStatisticsByBloc) m_BlocEventsStatisticsByRequest.Add(new BlocRequest(request.DataInfo, pair.Key), pair.Value);
                m_EventsStatisticsByRequest.Add(request, eventsStatistics);
                return eventsStatistics;
            }
        }
        else return null;
    }
    static BlocEventsStatistics GetEventsStatistics(BlocRequest request)
    {
        if (request.IsValid)
        {
            BlocEventsStatistics result;
            if (m_BlocEventsStatisticsByRequest.TryGetValue(request, out result))
            {
                return result;
            }
            else
            {
                BlocEventsStatistics blocEventsStatistics = new BlocEventsStatistics(request.DataInfo,request.Bloc);
                m_BlocEventsStatisticsByRequest.Add(request, blocEventsStatistics);
                return blocEventsStatistics;
            }
        }
        else return null;
    }

    static void NormalizeByNone(BlocRequest request)
    {
        BlocData blocData = GetData(request);
        foreach (var trial in blocData.Trials)
        {
            foreach (var subTrial in trial.SubTrialBySubBloc.Values)
            {
                subTrial.Normalize(0, 1);
            }
        }
        m_NormalizeByRequest[request] = HBP.Data.Enums.NormalizationType.None;
    }
    static void NormalizeBySubTrial(BlocRequest request)
    {
        BlocData blocData = GetData(request);
        foreach (var trial in blocData.Trials)
        {
            foreach (var subTrial in trial.SubTrialBySubBloc.Values)
            {
                foreach (var pair in subTrial.BaselineValuesByChannel)
                {
                    subTrial.Normalize(pair.Value.Mean(), pair.Value.StandardDeviation(), pair.Key);
                }
            }
        }
        m_NormalizeByRequest[request] = HBP.Data.Enums.NormalizationType.SubTrial;
    }
    static void NormalizeByTrial(BlocRequest request)
    {
        BlocData epochedData = GetData(request);
        foreach (var trial in epochedData.Trials)
        {
            Dictionary<string, List<float>> baselineByChannel = new Dictionary<string, List<float>>();
            foreach (var subTrial in trial.SubTrialBySubBloc.Values)
            {
                foreach (var channel in subTrial.BaselineValuesByChannel.Keys)
                {
                    if (!baselineByChannel.ContainsKey(channel)) baselineByChannel[channel] = new List<float>();
                    baselineByChannel[channel].AddRange(subTrial.BaselineValuesByChannel[channel]);
                }
            }

            float average, standardDeviation;
            foreach (var channel in baselineByChannel.Keys)
            {
                average = baselineByChannel[channel].ToArray().Mean();
                standardDeviation = baselineByChannel[channel].ToArray().StandardDeviation();
                foreach (var subTrial in trial.SubTrialBySubBloc.Values)
                {
                    subTrial.Normalize(average, standardDeviation, channel);
                }
            }
        }
        m_NormalizeByRequest[request] = HBP.Data.Enums.NormalizationType.Trial;
    }
    static void NormalizeBySubBloc(BlocRequest request)
    {
        Dictionary<string, List<float>> baselineByChannel;
        BlocData epochedData = GetData(request);
        foreach (var subBloc in request.Bloc.SubBlocs)
        {
            baselineByChannel = new Dictionary<string, List<float>>();
            foreach (var trial in epochedData.Trials)
            {
                SubTrial subTrial = trial.SubTrialBySubBloc[subBloc];
                foreach (var channel in subTrial.BaselineValuesByChannel.Keys)
                {
                    if (!baselineByChannel.ContainsKey(channel)) baselineByChannel[channel] = new List<float>();
                    baselineByChannel[channel].AddRange(subTrial.BaselineValuesByChannel[channel]);
                }
            }

            float average, standardDeviation;
            foreach (var channel in baselineByChannel.Keys)
            {
                average = baselineByChannel[channel].ToArray().Mean();
                standardDeviation = baselineByChannel[channel].ToArray().StandardDeviation();
                foreach (var trial in epochedData.Trials)
                {
                    SubTrial subTrial = trial.SubTrialBySubBloc[subBloc];
                    subTrial.Normalize(average, standardDeviation, channel);
                }
            }
        }
        m_NormalizeByRequest[request] = HBP.Data.Enums.NormalizationType.SubBloc;
    }
    static void NormalizeByBloc(BlocRequest request)
    {
        Dictionary<string, List<float>> baselineByChannel = new Dictionary<string, List<float>>();
        BlocData epochedData = m_BlocDataByRequest[request];
        foreach (var trial in epochedData.Trials)
        {
            foreach (var subTrial in trial.SubTrialBySubBloc.Values)
            {
                foreach (var channel in subTrial.BaselineValuesByChannel.Keys)
                {
                    if (!baselineByChannel.ContainsKey(channel)) baselineByChannel[channel] = new List<float>();
                    baselineByChannel[channel].AddRange(subTrial.BaselineValuesByChannel[channel]);
                }
            }
        }

        float average, standardDeviation;
        foreach (var channel in baselineByChannel.Keys)
        {
            average = baselineByChannel[channel].ToArray().Mean();
            standardDeviation = baselineByChannel[channel].ToArray().StandardDeviation();
            foreach (var trial in epochedData.Trials)
            {
                foreach (var subTrial in trial.SubTrialBySubBloc.Values)
                {
                    subTrial.Normalize(average, standardDeviation, channel);
                }
            }
        }
        m_NormalizeByRequest[request] = HBP.Data.Enums.NormalizationType.Bloc;
    }
    static void NormalizeByProtocol(IEnumerable<Tuple<BlocRequest, bool>> dataRequestAndNeedToNormalize)
    {
        Dictionary<string, List<float>> baselineByChannel = new Dictionary<string, List<float>>();

        foreach (var tuple in dataRequestAndNeedToNormalize)
        {
            BlocData epochedData = m_BlocDataByRequest[tuple.Item1];
            foreach (var trial in epochedData.Trials)
            {
                foreach (var subTrial in trial.SubTrialBySubBloc.Values)
                {
                    foreach (var channel in subTrial.BaselineValuesByChannel.Keys)
                    {
                        if (!baselineByChannel.ContainsKey(channel)) baselineByChannel[channel] = new List<float>();
                        baselineByChannel[channel].AddRange(subTrial.BaselineValuesByChannel[channel]);
                    }
                }
            }
        }

        float average, standardDeviation;
        foreach (var channel in baselineByChannel.Keys)
        {
            average = baselineByChannel[channel].ToArray().Mean();
            standardDeviation = baselineByChannel[channel].ToArray().StandardDeviation();
            foreach (var tuple in dataRequestAndNeedToNormalize)
            {
                if (tuple.Item2)
                {
                    BlocData epochedData = m_BlocDataByRequest[tuple.Item1];
                    foreach (var trial in epochedData.Trials)
                    {
                        foreach (var subTrial in trial.SubTrialBySubBloc.Values)
                        {
                            subTrial.Normalize(average, standardDeviation, channel);
                        }
                    }
                }
            }
        }

        foreach (var tuple in dataRequestAndNeedToNormalize)
        {
            if (tuple.Item2) m_NormalizeByRequest[tuple.Item1] = HBP.Data.Enums.NormalizationType.Protocol;
        }
    }
    #endregion

    #region Private struct
    struct Request
    {
        public DataInfo DataInfo { get; set; }
        public bool IsValid
        {
            get
            {
                return DataInfo != null && DataInfo.isOk;
            }
        }

        public Request(DataInfo dataInfo)
        {
            DataInfo = dataInfo;
        }
    }
    struct BlocRequest
    {
        public DataInfo DataInfo { get; set; }
        public Bloc Bloc { get; set; }
        public bool IsValid
        {
            get
            {
                return DataInfo.Dataset.Protocol.Blocs.Contains(Bloc);
            }
        }

        public BlocRequest(DataInfo dataInfo, Bloc bloc)
        {
            DataInfo = dataInfo;
            Bloc = bloc;
        }
    }
    struct ChannelRequest
    {
        public DataInfo DataInfo { get; set; }
        public string Channel { get; set; }
        public bool IsValid
        {
            get
            {
                return DataInfo != null && DataInfo.isOk; // AddTestOnChannel
            }
        }

        public ChannelRequest(DataInfo dataInfo, string channel)
        {
            DataInfo = dataInfo;
            Channel = channel;
        }
    }
    struct BlocChannelRequest
    {
        public DataInfo DataInfo { get; set; }
        public Bloc Bloc { get; set; }
        public string Channel { get; set; }
        public bool IsValid
        {
            get
            {
                return DataInfo.Dataset.Protocol.Blocs.Contains(Bloc); // AddTestOnChannel
            }
        }

        public BlocChannelRequest(DataInfo dataInfo, Bloc bloc, string channel)
        {
            DataInfo = dataInfo;
            Bloc = bloc;
            Channel = channel;
        }
    }
    #endregion
}