﻿using HBP.Data.Experience.Dataset;
using System.Linq;

namespace HBP.Data.Visualization
{
    public class SiteData
    {
        #region Properties
        public SiteTrial[] Trials { get; set; }
        public string Unit { get; set; }
        #endregion

        #region constructors
        public SiteData(EpochedData epochedData, string site)
        {
            Trials = epochedData.Trials.Select(t => new SiteTrial(t, site)).ToArray();
        }
        public SiteData(SiteTrial[] trials, string unit)
        {
            Trials = trials;
            Unit = unit;
        }
        public SiteData() : this(new SiteTrial[0], "")
        {

        }
        #endregion
    }
}