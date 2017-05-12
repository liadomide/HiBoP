﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HBP.Data.Visualization
{
    /**
    * \class ColumnConfiguration
    * \author Adrien Gannerie
    * \version 1.0
    * \date 28 avril 2017
    * \brief Configuration of a column.
    * 
    * \details ColumnConfiguration§ is a class which define the configuration of a patient and contains:
    *   - \a Unique ID.
    *   - \a Color of the patient.
    *   - \a Configurations of the patient electrodes.
    */
    [DataContract]
    public class ColumnConfiguration : ICloneable
    {
        #region Properties
        [DataMember]
        Dictionary<string, PatientConfiguration> configurationByPatientID;
        [IgnoreDataMember]
        /// <summary>
        /// Configuration by patient.
        /// </summary>
        public Dictionary<Patient, PatientConfiguration> ConfigurationByPatient { get; set; }

        /// <summary>
        /// Region of interest.
        /// </summary>
        [DataMember]
        public List<RegionOfInterest> RegionOfInterest { get; set; }
        #endregion

        #region Constructor
        public ColumnConfiguration(Dictionary<Patient,PatientConfiguration> configurationByPatient, IEnumerable<RegionOfInterest> regionOfInterest)
        {
            ConfigurationByPatient = configurationByPatient;
            RegionOfInterest = regionOfInterest.ToList();
        }
        public ColumnConfiguration() : this (new Dictionary<Patient, PatientConfiguration>(), new RegionOfInterest[0])
        { 
        }
        #endregion

        #region Public Methods
        public object Clone()
        {
            Dictionary<Patient, PatientConfiguration> configurationByPatientClone = new Dictionary<Patient, PatientConfiguration>();
            foreach (var item in ConfigurationByPatient) configurationByPatientClone.Add(item.Key, item.Value.Clone() as PatientConfiguration);
            return new ColumnConfiguration(configurationByPatientClone, from ROI in RegionOfInterest select ROI.Clone() as RegionOfInterest);
        }
        #endregion

        #region Serialization
        [OnSerializing]
        void OnSerializing(StreamingContext streamingContext)
        {
            configurationByPatientID = new Dictionary<string, PatientConfiguration>();
            foreach (var item in ConfigurationByPatient)
            {
                configurationByPatientID.Add(item.Key.ID, item.Value);
            }
        }
        [OnSerialized]
        void OnSerialized(StreamingContext streamingContext)
        {
            configurationByPatientID = null;
        }
        [OnDeserialized]
        void OnDeserialized(StreamingContext streamingContext)
        {
            foreach (var item in configurationByPatientID)
            {
                ConfigurationByPatient.Add(ApplicationState.ProjectLoaded.Patients.First((elmt) => elmt.ID == item.Key), item.Value);
            }
        }
        #endregion
    }
}