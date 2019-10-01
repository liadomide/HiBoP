﻿using HBP.Data.Enums;
using HBP.Data.Visualization;
using HBP.Module3D.DLL;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HBP.Module3D
{
    /// <summary>
    /// Class containing the CCEP data for the column
    /// </summary>
    public class Column3DCCEP : Column3DDynamic
    {
        #region Properties
        /// <summary>
        /// CCEP data of this column (contains information about what to display)
        /// </summary>
        public CCEPColumn ColumnCCEPData
        {
            get
            {
                return ColumnData as CCEPColumn;
            }
        }
        /// <summary>
        /// Timeline of this column (contains information about the length, the number of samples, the events etc.)
        /// </summary>
        public override Timeline Timeline
        {
            get
            {
                return ColumnCCEPData.Data.Timeline;
            }
        }

        /// <summary>
        /// List of all possible sources for this column
        /// </summary>
        public List<Site> Sources { get; private set; } = new List<Site>();
        private Site m_SelectedSource = null;
        /// <summary>
        /// Currently selected source
        /// </summary>
        public Site SelectedSource
        {
            get
            {
                return m_SelectedSource;
            }
            set
            {
                if (m_SelectedSource != value)
                {
                    m_SelectedSource = value;
                    OnSelectSource.Invoke();
                    SetActivityData();
                }
            }
        }
        /// <summary>
        /// Is a source selected in this column ?
        /// </summary>
        public bool IsSourceSelected
        {
            get
            {
                return m_SelectedSource != null;
            }
        }

        /// <summary>
        /// Latencies of the first spike of each site for the selected source
        /// </summary>
        public float[] Latencies { get; private set; }
        /// <summary>
        /// Amplitudes of the first spike of each site for the selected source
        /// </summary>
        public float[] Amplitudes { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Event called when selecting a source for this column
        /// </summary>
        public UnityEvent OnSelectSource = new UnityEvent();
        #endregion

        #region Private Methods
        protected override void Update()
        {
            //if (Input.GetKeyDown(KeyCode.A)) SelectedSource = SelectedSite;
            base.Update();
        }
        /// <summary>
        /// Set activity data for each site
        /// </summary>
        protected override void SetActivityData()
        {
            if (ColumnCCEPData == null) return;
            
            int timelineLength = Timeline.Length;
            int sitesCount = Sites.Count;
            // Construct sites value array the old way, and set sites masks // maybe FIXME
            ActivityValuesBySiteID = new float[sitesCount][];
            for (int i = 0; i < sitesCount; i++)
            {
                ActivityValuesBySiteID[i] = new float[timelineLength];
            }
            ActivityUnitsBySiteID = new string[sitesCount];
            Latencies = new float[sitesCount];
            Amplitudes = new float[sitesCount];
            int numberOfSitesWithValues = 0;

            if (!IsSourceSelected)
            {
                foreach (var site in Sites)
                {
                    site.State.IsMasked = false;// !ColumnCCEPData.Data.ProcessedValuesByChannelIDByStimulatedChannelID.ContainsKey(site.Information.FullCorrectedID);
                }
                return;
            }

            // Retrieve values
            if (!ColumnCCEPData.Data.ProcessedValuesByChannelIDByStimulatedChannelID.TryGetValue(SelectedSource.Information.FullCorrectedID, out Dictionary<string, float[]> processedValuesByChannel)) return;
            if (!ColumnCCEPData.Data.UnityByChannelIDByStimulatedChannelID.TryGetValue(SelectedSource.Information.FullCorrectedID, out Dictionary<string, string> unitByChannel)) return;
            if (!ColumnCCEPData.Data.DataByChannelIDByStimulatedChannelID.TryGetValue(SelectedSource.Information.FullCorrectedID, out Dictionary<string, Data.Experience.Dataset.BlocChannelData> dataByChannel)) return;
            if (!ColumnCCEPData.Data.StatisticsByChannelIDByStimulatedChannelID.TryGetValue(SelectedSource.Information.FullCorrectedID, out Dictionary<string, Data.Experience.Dataset.BlocChannelStatistics> statisticsByChannel)) return;

            foreach (Site site in Sites)
            {
                if (processedValuesByChannel.TryGetValue(site.Information.FullCorrectedID, out float[] values))
                {
                    if (values.Length > 0)
                    {
                        numberOfSitesWithValues++;
                        ActivityValuesBySiteID[site.Information.GlobalID] = values;
                        site.State.IsMasked = false;
                    }
                    else
                    {
                        ActivityValuesBySiteID[site.Information.GlobalID] = new float[timelineLength];
                        site.State.IsMasked = true;
                    }
                }
                else
                {
                    ActivityValuesBySiteID[site.Information.GlobalID] = new float[timelineLength];
                    site.State.IsMasked = true;
                }
                if (unitByChannel.TryGetValue(site.Information.FullCorrectedID, out string unit))
                {
                    ActivityUnitsBySiteID[site.Information.GlobalID] = unit;
                }
                else
                {
                    ActivityUnitsBySiteID[site.Information.GlobalID] = "";
                }
                if (dataByChannel.TryGetValue(site.Information.FullCorrectedID, out Data.Experience.Dataset.BlocChannelData blocChannelData))
                {
                    site.Data = blocChannelData;
                }
                if (statisticsByChannel.TryGetValue(site.Information.FullCorrectedID, out Data.Experience.Dataset.BlocChannelStatistics blocChannelStatistics))
                {
                    site.Statistics = blocChannelStatistics;
                    Data.Experience.Dataset.ChannelSubTrialStat trial = blocChannelStatistics.Trial.ChannelSubTrialBySubBloc[ColumnCCEPData.Bloc.MainSubBloc];
                    SubTimeline mainSubTimeline = Timeline.SubTimelinesBySubBloc[ColumnCCEPData.Bloc.MainSubBloc];
                    for (int i = mainSubTimeline.StatisticsByEvent[ColumnCCEPData.Bloc.MainSubBloc.MainEvent].RoundedIndexFromStart + 2; i < mainSubTimeline.Length - 2; i++)
                    {
                        if (trial.Values[i - 1] > trial.Values[i - 2] && trial.Values[i] > trial.Values[i - 1] && trial.Values[i] > trial.Values[i + 1] && trial.Values[i + 1] > trial.Values[i + 2]) // Maybe FIXME: method to compute amplitude and latency
                        {
                            Amplitudes[site.Information.GlobalID] = trial.Values[i];
                            Latencies[site.Information.GlobalID] = mainSubTimeline.Frequency.ConvertNumberOfSamplesToMilliseconds(i - mainSubTimeline.StatisticsByEvent[ColumnCCEPData.Bloc.MainSubBloc.MainEvent].RoundedIndexFromStart);
                            break;
                        }
                    }
                }
            }
            if (numberOfSitesWithValues == 0)
            {
                throw new NoMatchingSitesException();
            }

            DynamicParameters.MinimumAmplitude = float.MaxValue;
            DynamicParameters.MaximumAmplitude = float.MinValue;

            int length = timelineLength * sitesCount;
            ActivityValues = new float[length];
            List<float> iEEGNotMasked = new List<float>();
            for (int s = 0; s < sitesCount; ++s)
            {
                for (int t = 0; t < timelineLength; ++t)
                {
                    float val = ActivityValuesBySiteID[s][t];
                    ActivityValues[t * sitesCount + s] = val;
                }
                if (!Sites[s].State.IsMasked)
                {
                    for (int t = 0; t < timelineLength; ++t)
                    {
                        float val = ActivityValuesBySiteID[s][t];
                        iEEGNotMasked.Add(val);

                        //update min/ max values
                        if (val > DynamicParameters.MaximumAmplitude)
                            DynamicParameters.MaximumAmplitude = val;
                        else if (val < DynamicParameters.MinimumAmplitude)
                            DynamicParameters.MinimumAmplitude = val;
                    }
                }
            }
            ActivityValuesOfUnmaskedSites = iEEGNotMasked.ToArray();
        }
        /// <summary>
        /// Update sites sizes and colors arrays depending on the activity (to be called before the rendering update)
        /// </summary>
        /// <param name="showAllSites">Display sites that are not in a ROI</param>
        protected override void UpdateSitesSizeAndColorOfSites(bool showAllSites)
        {
            if (IsSourceSelected)
            {
                base.UpdateSitesSizeAndColorOfSites(showAllSites);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Update the sites of this column (when changing the implantation of the scene)
        /// </summary>
        /// <param name="sites">List of the sites in the DLL</param>
        /// <param name="sceneSitePatientParent">List of the patient parent of the sites as instantiated in the scene</param>
        public override void UpdateSites(PatientElectrodesList sites, List<GameObject> sceneSitePatientParent)
        {
            base.UpdateSites(sites, sceneSitePatientParent);
            Sources = Sites.Where(s => ColumnCCEPData.Data.ProcessedValuesByChannelIDByStimulatedChannelID.Keys.Contains(s.Information.FullCorrectedID)).ToList();
        }
        /// <summary>
        /// Update the visibility, the size and the color of the sites depending on their state
        /// </summary>
        /// <param name="showAllSites">Do we show sites that are not in a ROI ?</param>
        /// <param name="hideBlacklistedSites">Do we hide blacklisted sites ?</param>
        /// <param name="isGeneratorUpToDate">Is the activity generator up to date ?</param>
        public override void UpdateSitesRendering(bool showAllSites, bool hideBlacklistedSites, bool isGeneratorUpToDate)
        {
            UpdateSitesSizeAndColorOfSites(showAllSites);

            for (int i = 0; i < Sites.Count; ++i)
            {
                Site site = Sites[i];
                bool activity = site.IsActive;
                SiteType siteType;
                if (site.State.IsMasked || (site.State.IsOutOfROI && !showAllSites) || !site.State.IsFiltered)
                {
                    if (activity) site.IsActive = false;
                    continue;
                }
                else if (site.State.IsBlackListed)
                {
                    site.transform.localScale = Vector3.one;
                    siteType = SiteType.BlackListed;
                    if (hideBlacklistedSites)
                    {
                        if (activity) site.IsActive = false;
                        continue;
                    }
                }
                else if (isGeneratorUpToDate)
                {
                    site.transform.localScale = m_ElectrodesSizeScale[i];
                    siteType = m_ElectrodesPositiveColor[i] ? SiteType.Positive : SiteType.Negative;
                }
                else if (!IsSourceSelected)
                {
                    site.transform.localScale = Vector3.one;
                    siteType = ColumnCCEPData.Data.ProcessedValuesByChannelIDByStimulatedChannelID.ContainsKey(site.Information.FullCorrectedID) ? SiteType.Source : SiteType.NotASource;
                }
                else
                {
                    site.transform.localScale = Vector3.one;
                    siteType = SiteType.Normal;
                }
                if (!activity) site.IsActive = true;
                site.GetComponent<MeshRenderer>().sharedMaterial = SharedMaterials.SiteSharedMaterial(site.State.IsHighlighted, siteType, site.State.Color);
                site.transform.localScale *= DynamicParameters.Gain;
            }
        }
        /// <summary>
        /// Load the column configuration from the column data
        /// </summary>
        /// <param name="firstCall">Has this method not been called by another load method ?</param>
        public override void LoadConfiguration(bool firstCall = true)
        {
            if (firstCall) ResetConfiguration();
            DynamicParameters.Gain = ColumnCCEPData.DynamicConfiguration.Gain;
            DynamicParameters.InfluenceDistance = ColumnCCEPData.DynamicConfiguration.MaximumInfluence;
            DynamicParameters.AlphaMin = ColumnCCEPData.DynamicConfiguration.Alpha;
            DynamicParameters.SetSpanValues(ColumnCCEPData.DynamicConfiguration.SpanMin, ColumnCCEPData.DynamicConfiguration.Middle, ColumnCCEPData.DynamicConfiguration.SpanMax);
            base.LoadConfiguration(false);
        }
        /// <summary>
        /// Save the configuration of this column to the data column
        /// </summary>
        public override void SaveConfiguration()
        {
            ColumnCCEPData.DynamicConfiguration.Gain = DynamicParameters.Gain;
            ColumnCCEPData.DynamicConfiguration.MaximumInfluence = DynamicParameters.InfluenceDistance;
            ColumnCCEPData.DynamicConfiguration.Alpha = DynamicParameters.AlphaMin;
            ColumnCCEPData.DynamicConfiguration.SpanMin = DynamicParameters.SpanMin;
            ColumnCCEPData.DynamicConfiguration.Middle = DynamicParameters.Middle;
            ColumnCCEPData.DynamicConfiguration.SpanMax = DynamicParameters.SpanMax;
            base.SaveConfiguration();
        }
        /// <summary>
        /// Reset the configuration of this column
        /// </summary>
        public override void ResetConfiguration()
        {
            DynamicParameters.Gain = 1.0f;
            DynamicParameters.InfluenceDistance = 15.0f;
            DynamicParameters.AlphaMin = 0.8f;
            DynamicParameters.ResetSpanValues(this);
            base.ResetConfiguration();
        }
        #endregion
    }
}