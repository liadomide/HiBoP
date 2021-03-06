﻿using System.Collections.Generic;
using System.Linq;

namespace HBP.Data.Experience.Dataset
{
    public struct SubBlocEventsStatistics
    {
        #region Properties
        public Dictionary<Protocol.Event, EventStatistics> StatisticsByEvent { get; set; }
        #endregion

        #region Constructors
        public SubBlocEventsStatistics(BlocData data, Protocol.SubBloc subBloc) : this(subBloc.Events.ToDictionary(e => e, e => data.Trials.Where(t => t.SubTrialBySubBloc[subBloc].Found).Select(t => t.SubTrialBySubBloc[subBloc].InformationsByEvent[e]).ToArray()))
        {
        }
        public SubBlocEventsStatistics(Dictionary<Protocol.Event, EventInformation[]> informationsByEvent)
        {
            StatisticsByEvent = informationsByEvent.ToDictionary(kv => kv.Key, kv => new EventStatistics(kv.Value, ApplicationState.UserPreferences.Data.Protocol.PositionAveraging));
        }
        #endregion

        #region Public Methods
        public void Clear()
        {
            StatisticsByEvent.Clear();
            StatisticsByEvent = new Dictionary<Protocol.Event, EventStatistics>();
        }
        #endregion
    }
}