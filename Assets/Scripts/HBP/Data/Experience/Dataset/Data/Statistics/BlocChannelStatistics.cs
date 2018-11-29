﻿namespace HBP.Data.Experience.Dataset
{
    public class BlocChannelStatistics
    {
        #region Properties
        public ChannelTrialStat Trial { get; set; }
        #endregion

        #region Constructors
        public BlocChannelStatistics(BlocChannelData data)
        {
            Trial = new ChannelTrialStat(data.Trials, ApplicationState.UserPreferences.Data.EEG.Averaging);
        }
        #endregion
    }
}