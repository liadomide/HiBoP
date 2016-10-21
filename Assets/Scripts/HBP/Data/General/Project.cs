﻿using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HBP.Data.Patient;
using UnityEngine;
using HBP.Data.Settings;
using HBP.Data.Visualisation;
using HBP.Data.Experience.Dataset;
using HBP.Data.Experience.Protocol;

namespace HBP.Data.General
{
    /// <summary>
    /// Class which define a project, it's contains :
    ///     - Settings.
    ///     - Patients.
    ///     - Groups.
    ///     - Regions of interest.
    ///     - Protocols.
    ///     - Datasets.
    ///     - Visualisations.
    /// </summary>
    public class Project
    {
        #region Properties
        ProjectSettings settings;
        public ProjectSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        List<Patient.Patient> patients;
        public ReadOnlyCollection<Patient.Patient> Patients
        {
            get { return new ReadOnlyCollection<Patient.Patient>(patients); }
        }

        List<Group> groups;
        public ReadOnlyCollection<Group> Groups
        {
            get { return new ReadOnlyCollection<Group>(groups); }
        }

        List<Protocol> protocols;
        public ReadOnlyCollection<Protocol> Protocols
        {
            get { return new ReadOnlyCollection<Protocol>(protocols); }
        }

        List<Dataset> datasets;
        public ReadOnlyCollection<Dataset> Datasets
        {
            get { return new ReadOnlyCollection<Dataset>(datasets); }
        }

        List<SinglePatientVisualisation> singlePatientVisualisations;
        public ReadOnlyCollection<SinglePatientVisualisation> SinglePatientVisualisations
        {
            get { return new ReadOnlyCollection<SinglePatientVisualisation>(singlePatientVisualisations); }
        }

        List<MultiPatientsVisualisation> multiPatientsVisualisations;
        public ReadOnlyCollection<MultiPatientsVisualisation> MultiPatientsVisualisations
        {
            get { return new ReadOnlyCollection<MultiPatientsVisualisation>(multiPatientsVisualisations); }
        }
        #endregion

        #region Constructors
        public Project(ProjectSettings settings, Patient.Patient[] patients, Group[] groups, Protocol[] protocols, Dataset[] datasets, SinglePatientVisualisation[] singleVisualisations, MultiPatientsVisualisation[] multiVisualisations)
        {
            Settings = settings;
            SetPatients(patients);
            SetGroups(groups);
            SetProtocols(protocols);
            SetDatasets(datasets);
            SetSinglePatientVisualisations(singleVisualisations);
            SetMultiPatientsVisualisations(multiVisualisations);
        }
        public Project(ProjectSettings settings) : this(settings, new Patient.Patient[0], new Group[0], new Protocol[0], new Dataset[0] , new SinglePatientVisualisation[0], new MultiPatientsVisualisation[0])
        {
        }
        public Project(string name) : this(new ProjectSettings(name))
        {
        }
        public Project() : this(new ProjectSettings())
        {
        }
        #endregion

        #region Public Methods
        // Patients.
        public void SetPatients(Patient.Patient[] patients)
        {
            this.patients = new List<Patient.Patient>();
            AddPatient(patients);
        }
        public void AddPatient(Patient.Patient patient)
        {
            patients.Add(patient);
        }
        public void AddPatient(Patient.Patient[] patients)
        {
            foreach (Patient.Patient patient in patients)
            {
                AddPatient(patient);
            }
        }
        public void RemovePatient(Patient.Patient patient)
        {
            foreach(Group group in groups)
            {
                group.Remove(patient);
            }
            foreach (Dataset dataset in datasets)
            {
                dataset.Remove((from info in dataset.Data where info.Patient == patient select info).ToArray());
            }
            RemoveSinglePatientVisualisation((from singlePatientVisualisation in singlePatientVisualisations where singlePatientVisualisation.Patient == patient select singlePatientVisualisation).ToArray());
            foreach(MultiPatientsVisualisation multiPatientsVisualisation in multiPatientsVisualisations)
            {
                multiPatientsVisualisation.RemovePatient(patient);
            }
            patients.Remove(patient);
        }
        public void RemovePatient(Patient.Patient[] patients)
        {
            foreach (Patient.Patient patient in patients)
            {
                RemovePatient(patient);
            }
        }

        // Groups.
        public void SetGroups(Group[] groups)
        {
            this.groups = new List<Group>();
            AddGroup(groups);
        }
        public void AddGroup(Group group)
        {
            groups.Add(group);
        }
        public void AddGroup(Group[] groups)
        {
            foreach (Group group in groups)
            {
                AddGroup(group);
            }
        }
        public void RemoveGroup(Group group)
        {
            groups.Remove(group);
        }
        public void RemoveGroup(Group[] groups)
        {
            foreach (Group group in groups)
            {
                RemoveGroup(group);
            }
        }

        // Protocols.
        public void SetProtocols(Protocol[] protocols)
        {
            this.protocols = new List<Protocol>();
            AddProtocol(protocols);
        }
        public void AddProtocol(Protocol protocol)
        {
            protocols.Add(protocol);
        }
        public void AddProtocol(Protocol[] protocols)
        {
            foreach (Protocol protocol in protocols)
            {
                AddProtocol(protocol);
            }
        }
        public void RemoveProtocol(Protocol protocol)
        {
            foreach (Dataset dataset in datasets)
            {
                dataset.Remove((from info in dataset.Data where info.Protocol == protocol select info).ToArray());
            }
            foreach (SinglePatientVisualisation singlePatientVisualisation in singlePatientVisualisations)
            {                
                singlePatientVisualisation.RemoveColumn((from column in singlePatientVisualisation.Columns where column.Protocol == protocol select column).ToArray());
            }
            foreach (MultiPatientsVisualisation multiPatientsVisualisation in multiPatientsVisualisations)
            {
                multiPatientsVisualisation.RemoveColumn((from column in multiPatientsVisualisation.Columns where column.Protocol == protocol select column).ToArray());
            }
            protocols.Remove(protocol);
        }
        public void RemoveProtocol(Protocol[] protocols)
        {
            foreach (Protocol protocol in protocols)
            {
                RemoveProtocol(protocol);
            }
        }

        // Datasets.
        public void SetDatasets(Dataset[] datasets)
        {
            this.datasets = new List<Dataset>();
            AddDataset(datasets);
        }
        public void AddDataset(Dataset dataset)
        {
            datasets.Add(dataset);
        }
        public void AddDataset(Dataset[] datasets)
        {
            foreach (Dataset dataset in datasets)
            {
                AddDataset(dataset);
            }
        }
        public void RemoveDataset(Dataset dataset)
        {
            foreach (SinglePatientVisualisation singlePatientVisualisation in singlePatientVisualisations)
            {
                singlePatientVisualisation.RemoveColumn((from column in singlePatientVisualisation.Columns where column.Dataset == dataset select column).ToArray());
            }
            foreach (MultiPatientsVisualisation multiPatientsVisualisation in multiPatientsVisualisations)
            {
                multiPatientsVisualisation.RemoveColumn((from column in multiPatientsVisualisation.Columns where column.Dataset == dataset select column).ToArray());
            }
            datasets.Remove(dataset);
        }
        public void RemoveDataset(Dataset[] datasets)
        {
            foreach (Dataset dataset in datasets)
            {
                RemoveDataset(dataset);
            }
        }

        // SinglePatientVisualisations.
        public void SetSinglePatientVisualisations(SinglePatientVisualisation[] singlePatientVisualisations)
        {
            this.singlePatientVisualisations = new List<SinglePatientVisualisation>();
            AddSinglePatientVisualisation(singlePatientVisualisations);
        }
        public void AddSinglePatientVisualisation(SinglePatientVisualisation singlePatientVisualisation)
        {
            singlePatientVisualisations.Add(singlePatientVisualisation);
        }
        public void AddSinglePatientVisualisation(SinglePatientVisualisation[] singlePatientVisualisations)
        {
            foreach(SinglePatientVisualisation singlePatientVisualisation in singlePatientVisualisations)
            {
                AddSinglePatientVisualisation(singlePatientVisualisation);
            }
        }
        public void RemoveSinglePatientVisualisation(SinglePatientVisualisation singlePatientVisualisation)
        {
            singlePatientVisualisations.Remove(singlePatientVisualisation);
        }
        public void RemoveSinglePatientVisualisation(SinglePatientVisualisation[] singlePatientVisualisations)
        {
            foreach (SinglePatientVisualisation singlePatientVisualisation in singlePatientVisualisations)
            {
                RemoveSinglePatientVisualisation(singlePatientVisualisation);
            }
        }

        // MultiPatientsVisualisations.
        public void SetMultiPatientsVisualisations(MultiPatientsVisualisation[] multiPatientsVisualisations)
        {
            this.multiPatientsVisualisations = new List<MultiPatientsVisualisation>();
            AddMultiPatientsVisualisation(multiPatientsVisualisations);
        }
        public void AddMultiPatientsVisualisation(MultiPatientsVisualisation multiPatientsVisualisation)
        {
            multiPatientsVisualisations.Add(multiPatientsVisualisation);
        }
        public void AddMultiPatientsVisualisation(MultiPatientsVisualisation[] multiPatientsVisualisations)
        {
            foreach (MultiPatientsVisualisation multiPatientsVisualisation in multiPatientsVisualisations)
            {
                AddMultiPatientsVisualisation(multiPatientsVisualisation);
            }
        }
        public void RemoveMultiPatientsVisualisation(MultiPatientsVisualisation multiPatientsVisualisation)
        {
            multiPatientsVisualisations.Remove(multiPatientsVisualisation);
        }
        public void RemoveMultiPatientsVisualisation(MultiPatientsVisualisation[] multiPatientsVisualisations)
        {
            foreach (MultiPatientsVisualisation multiPatientsVisualisation in multiPatientsVisualisations)
            {
                RemoveMultiPatientsVisualisation(multiPatientsVisualisation);
            }
        }

        // Others.
        public static bool IsProject(string path)
        {
            bool l_isProject = false;
            string l_path = path;
            string l_name;
            if (Directory.Exists(l_path))
            {
                DirectoryInfo l_projectDirectory = new DirectoryInfo(l_path);
                l_name = l_projectDirectory.Name;
                if (Directory.Exists(l_path + Path.DirectorySeparatorChar + "Patients") && Directory.Exists(l_path + Path.DirectorySeparatorChar + "Groups") && Directory.Exists(l_path + Path.DirectorySeparatorChar + "Protocols") && Directory.Exists(l_path + Path.DirectorySeparatorChar + "ROI") && Directory.Exists(l_path + Path.DirectorySeparatorChar + "Datasets") && Directory.Exists(l_path + Path.DirectorySeparatorChar + "Visualisations") && Directory.Exists(l_path + Path.DirectorySeparatorChar + "Visualisations"+Path.DirectorySeparatorChar+"SinglePatient") && Directory.Exists(l_path + Path.DirectorySeparatorChar + "Visualisations" + Path.DirectorySeparatorChar + "MultiPatients") && File.Exists(l_path + Path.DirectorySeparatorChar + l_name + ".settings"))
                {
                    l_isProject = true;
                }
            }
            return l_isProject;
        }
        public static string[] GetProject(string path)
        {
            List<string> l_listPathProject = new List<string>();
            if (path != string.Empty && Directory.Exists(path))
            {
                string[] l_directories = Directory.GetDirectories(path);
                foreach (string l_directoryPath in l_directories)
                {
                    if (IsProject(l_directoryPath))
                    {
                        l_listPathProject.Add(l_directoryPath);
                    }
                }
            }
            return l_listPathProject.ToArray();
        }
        public string GetProject(string path, string ID)
        {
            if (path != string.Empty && Directory.Exists(path))
            {
                DirectoryInfo l_directory = new DirectoryInfo(path);
                DirectoryInfo[] l_directories = l_directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
                foreach (DirectoryInfo directory in l_directories)
                {
                    FileInfo[] l_files = directory.GetFiles("*" + FileExtension.Settings, SearchOption.TopDirectoryOnly);
                    foreach (FileInfo file in l_files)
                    {
                        ProjectSettings l_setting = ProjectSettings.LoadJson(file.FullName);
                        if (l_setting.ID == ID)
                        {
                            return directory.FullName;
                        }
                    }
                }

            }
            return string.Empty;
        }
        #endregion
    }
}
