﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HBP.Module3D
{
    public class Implantation3D
    {
        #region Properties
        public string Name { get; set; }
        public DLL.PatientElectrodesList PatientElectrodesList { get; set; }
        public bool IsLoaded { get; set; }

        public bool AreLatenciesLoaded { get; set; }
        public DLL.RawSiteList RawSiteList { get; set; }
        public List<Latencies> Latencies { get; set; }
        #endregion

        #region Constructors
        public Implantation3D(string name, IEnumerable<string> pts, IEnumerable<string> patientIDs)
        {
            Name = name;
            PatientElectrodesList = new DLL.PatientElectrodesList();
            IsLoaded = PatientElectrodesList.LoadPTSFiles(pts.ToList(), patientIDs.ToList(), ApplicationState.Module3D.MarsAtlasIndex);
        }
        #endregion

        #region Public Methods
        public void LoadLatencies(Data.Patient patient)
        {
            RawSiteList = new DLL.RawSiteList();
            PatientElectrodesList.ExtractRawSiteList(RawSiteList);
            foreach (Data.Anatomy.Connectivity connectivity in patient.Brain.Connectivities)
            {
                if (!connectivity.isUsable) continue;
                Latencies latencies = connectivity.HasConnectivity ? RawSiteList.UpdateLatenciesWithFile(connectivity.File) : RawSiteList.GenerateDummyLatencies();
                latencies.Name = connectivity.Name;
                Latencies.Add(latencies);
                AreLatenciesLoaded = true;
            }
        }
        #endregion
    }
}