﻿using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HBP.Data.Visualization;
using HBP.Data.Experience.Dataset;
using HBP.Data.Experience.Protocol;
using CielaSpike;
using Tools.Unity;
using UnityEngine.Events;
using Ionic.Zip;
using Tools.CSharp;

namespace HBP.Data.General
{
    /**
    * \class Project
    * \author Adrien Gannerie
    * \version 1.0
    * \date 12 janvier 2017
    * \brief Class which define a HiBoP project.
    * 
    * \details Class which define a HiBoP project, it's contains :
    *     - Settings.
    *     - Patients.
    *     - Groups.
    *     - Regions of interest.(To Add)
    *     - Protocols.
    *     - Datasets.
    *     - Visualizations.
    */
    public class Project
    {
        #region Properties
        /// <summary>
        /// Project extension.
        /// </summary>
        public const string EXTENSION = ".hibop";
        /// <summary>
        /// Project file
        /// </summary>
        public string FileName
        {
            get
            {
                return Settings.Name + EXTENSION;
            }
        }

        /// <summary>
        /// Settings of the project.
        /// </summary>
        public ProjectSettings Settings { get; set; }

        List<Patient> m_Patients = new List<Patient>();
        /// <summary>
        /// Patients of the project.
        /// </summary>
        public ReadOnlyCollection<Patient> Patients
        {
            get { return new ReadOnlyCollection<Patient>(m_Patients); }
        }

        List<Group> m_Groups = new List<Group>();
        /// <summary>
        /// Patient groups of the project.
        /// </summary>
        public ReadOnlyCollection<Group> Groups
        {
            get { return new ReadOnlyCollection<Group>(m_Groups); }
        }

        List<Protocol> m_Protocols = new List<Protocol>();
        /// <summary>
        /// Protocols of the project.
        /// </summary>
        public ReadOnlyCollection<Protocol> Protocols
        {
            get { return new ReadOnlyCollection<Protocol>(m_Protocols); }
        }

        List<Dataset> m_Datasets = new List<Dataset>();
        /// <summary>
        /// Datasets of the project.
        /// </summary>
        public ReadOnlyCollection<Dataset> Datasets
        {
            get { return new ReadOnlyCollection<Dataset>(m_Datasets); }
        }

        List<Visualization.Visualization> m_Visualizations = new List<Visualization.Visualization>();
        /// <summary>
        /// Visualizations of the project.
        /// </summary>
        public ReadOnlyCollection<Visualization.Visualization> Visualizations
        {
            get { return new ReadOnlyCollection<Visualization.Visualization>(m_Visualizations); }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new project instance.
        /// </summary>
        /// <param name="settings">Settings of the project.</param>
        /// <param name="patients">Patients of the project.</param>
        /// <param name="groups">Groups of the project.</param>
        /// <param name="protocols">Protocols of the project.</param>
        /// <param name="datasets">Datasets of the project.</param>
        /// <param name="visualizations">Single patient visualizations of the project.</param>
        /// <param name="multiVisualizations">Multi patients visualizations of the project.</param>
        public Project(ProjectSettings settings, IEnumerable<Patient> patients, IEnumerable<Group> groups, IEnumerable<Protocol> protocols, IEnumerable<Dataset> datasets, IEnumerable<Visualization.Visualization> visualizations)
        {
            Settings = settings;
            SetPatients(patients);
            SetGroups(groups);
            SetProtocols(protocols);
            SetDatasets(datasets);
            SetVisualizations(visualizations);
        }
        /// <summary>
        /// Create a new project with only the settings.
        /// </summary>
        /// <param name="settings">Settings of the project.</param>
        public Project(ProjectSettings settings) : this(settings, new Patient[0], new Group[0], new Protocol[0], new Dataset[0], new Visualization.Visualization[0])
        {
        }
        /// <summary>
        /// Create a empty project with a name.
        /// </summary>
        /// <param name="name">Name of the project.</param>
        public Project(string name) : this(new ProjectSettings(name))
        {
        }
        /// <summary>
        /// Create a empty project with default values.
        /// </summary>
        public Project() : this(new ProjectSettings())
        {
        }
        #endregion

        #region Getter/Setter
        // Patients.
        /// <summary>
        /// Set the patients of the project.
        /// </summary>
        /// <param name="patients"></param>
        public void SetPatients(IEnumerable<Patient> patients)
        {
            this.m_Patients = new List<Patient>();
            AddPatient(patients);
            foreach (Dataset dataset in m_Datasets)
            {
                dataset.RemoveData(from data in dataset.GetPatientDataInfos() where !m_Patients.Any(p => p == data.Patient) select data);
            }
            foreach (Visualization.Visualization visualization in m_Visualizations)
            {
                visualization.RemovePatient(from patient in visualization.Patients where !m_Patients.Contains(patient) select patient);
            }
            foreach (Group _group in m_Groups)
            {
                _group.RemovePatient((from patient in _group.Patients where !m_Patients.Contains(patient) select patient).ToArray());
            }
        }
        public void AddPatient(Patient patient)
        {
            m_Patients.Add(patient);
        }
        public void AddPatient(IEnumerable<Patient> patients)
        {
            foreach (Patient patient in patients)
            {
                AddPatient(patient);
            }
        }
        public void RemovePatient(Patient patient)
        {
            foreach (Group group in m_Groups)
            {
                group.RemovePatient(patient);
            }
            foreach (Dataset dataset in m_Datasets)
            {
                dataset.RemoveData(from data in dataset.GetPatientDataInfos() where data.Patient == patient select data);
            }
            foreach (Visualization.Visualization visualization in m_Visualizations)
            {
                visualization.RemovePatient(patient);
            }
            m_Patients.Remove(patient);
        }
        public void RemovePatient(IEnumerable<Patient> patients)
        {
            foreach (Patient patient in patients)
            {
                RemovePatient(patient);
            }
        }
        // Groups.
        public void SetGroups(IEnumerable<Group> groups)
        {
            this.m_Groups = new List<Group>();
            AddGroup(groups);
        }
        public void AddGroup(Group group)
        {
            m_Groups.Add(group);
        }
        public void AddGroup(IEnumerable<Group> groups)
        {
            foreach (Group group in groups)
            {
                AddGroup(group);
            }
        }
        public void RemoveGroup(Group group)
        {
            m_Groups.Remove(group);
        }
        public void RemoveGroup(IEnumerable<Group> groups)
        {
            foreach (Group group in groups)
            {
                RemoveGroup(group);
            }
        }
        // Protocols.
        public void SetProtocols(IEnumerable<Protocol> protocols)
        {
            m_Protocols = new List<Protocol>();
            AddProtocol(protocols);
            RemoveDataset((from dataset in m_Datasets where !m_Protocols.Any(p => p == dataset.Protocol) select dataset).ToArray());
            foreach (Visualization.Visualization visualization in m_Visualizations)
            {
                IEEGColumn[] columnsToRemove = visualization.Columns.Where(c => c is IEEGColumn).Select(c => c as IEEGColumn).Where(c => !m_Protocols.Any(p => p == c.Dataset.Protocol)).ToArray();
                foreach (Column column in columnsToRemove)
                {
                    visualization.Columns.Remove(column);
                }
            }
        }
        public void AddProtocol(Protocol protocol)
        {
            m_Protocols.Add(protocol);
        }
        public void AddProtocol(IEnumerable<Protocol> protocols)
        {
            foreach (Protocol protocol in protocols)
            {
                AddProtocol(protocol);
            }
        }
        public void RemoveProtocol(Protocol protocol)
        {
            m_Datasets.RemoveAll((d) => d.Protocol == protocol);
            foreach (Visualization.Visualization visualization in m_Visualizations)
            {
                visualization.Columns.RemoveAll((column) => (column is IEEGColumn) && (column as IEEGColumn).Dataset.Protocol == protocol);
            }
            m_Protocols.Remove(protocol);
        }
        public void RemoveProtocol(IEnumerable<Protocol> protocols)
        {
            foreach (Protocol protocol in protocols)
            {
                RemoveProtocol(protocol);
            }
        }
        // Datasets.
        public void SetDatasets(IEnumerable<Dataset> datasets)
        {
            m_Datasets = new List<Dataset>();
            AddDataset(datasets);
            foreach (Visualization.Visualization visualization in m_Visualizations)
            {
                Column[] columnsToRemove = visualization.Columns.Where(column => column is IEEGColumn && !m_Datasets.Any(d => d == (column as IEEGColumn).Dataset)).ToArray();
                foreach (Column column in columnsToRemove)
                {
                    visualization.Columns.Remove(column);
                }
            }
        }
        public void AddDataset(Dataset dataset)
        {
            m_Datasets.Add(dataset);
        }
        public void AddDataset(IEnumerable<Dataset> datasets)
        {
            foreach (Dataset dataset in datasets)
            {
                AddDataset(dataset);
            }
        }
        public void RemoveDataset(Dataset dataset)
        {
            foreach (Visualization.Visualization visualization in m_Visualizations)
            {
                visualization.Columns.RemoveAll((column) => (column is IEEGColumn) && (column as IEEGColumn).Dataset == dataset);
            }
            m_Datasets.Remove(dataset);
        }
        public void RemoveDataset(IEnumerable<Dataset> datasets)
        {
            foreach (Dataset dataset in datasets)
            {
                RemoveDataset(dataset);
            }
        }
        // Visualizations.
        public void SetVisualizations(IEnumerable<Visualization.Visualization> visualizations)
        {
            this.m_Visualizations = new List<Visualization.Visualization>();
            AddVisualization(visualizations);
        }
        public void AddVisualization(Visualization.Visualization visualization)
        {
            m_Visualizations.Add(visualization);
        }
        public void AddVisualization(IEnumerable<Visualization.Visualization> visualizations)
        {
            foreach (Visualization.Visualization visualization in visualizations)
            {
                AddVisualization(visualization);
            }
        }
        public void RemoveVisualization(Visualization.Visualization visualization)
        {
            m_Visualizations.Remove(visualization);
        }
        public void RemoveVisualization(IEnumerable<Visualization.Visualization> visualizations)
        {
            foreach (Visualization.Visualization visualization in visualizations)
            {
                RemoveVisualization(visualization);
            }
        }
        #endregion

        #region Public Methods
        public static bool IsProject(string path)
        {
            return new FileInfo(path).Extension == EXTENSION;
        }
        public static IEnumerable<string> GetProject(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                if (directory.Exists)
                {
                    FileInfo[] files = directory.GetFiles("*" + EXTENSION);
                    return from file in files where IsProject(file.FullName) select file.FullName;
                }
            }
            return new string[0];
        }
        public string GetProject(string path, string ID)
        {
            IEnumerable<string> projectsDirectories = GetProject(path);
            foreach (var directoryPaths in projectsDirectories)
            {
                ProjectInfo projectInfo = new ProjectInfo(directoryPaths);
            }
            return projectsDirectories.FirstOrDefault((project) => new ProjectInfo(project).Settings.ID == ID);
        }

        public IEnumerator c_Load(ProjectInfo projectInfo, Action<float, float, LoadingText> OnChangeProgress)
        {
            // Initialize progress.
            float steps = 1 + projectInfo.Patients + projectInfo.Groups + projectInfo.Protocols + 5 * projectInfo.Patients * projectInfo.Datasets + projectInfo.Visualizations;
            float progress = 0.0f;

            float settingsProgress = 1 / steps;
            float patientsProgress = projectInfo.Patients / steps;
            float groupsProgress = projectInfo.Groups / steps;
            float protocolsProgress = projectInfo.Protocols / steps;
            float datasetsProgress = 5 * projectInfo.Patients * projectInfo.Datasets / steps;
            float visualizationsProgress = projectInfo.Visualizations / steps;

            yield return Ninja.JumpToUnity;
            OnChangeProgress.Invoke(progress, 0, new LoadingText("Loading project"));
            yield return Ninja.JumpBack;

            // Unzipping
            if (Directory.Exists(ApplicationState.ProjectLoadedTMPFullPath)) Directory.Delete(ApplicationState.ProjectLoadedTMPFullPath, true);
            using (ZipFile zip = ZipFile.Read(projectInfo.Path))
            {
                zip.ExtractAll(ApplicationState.ProjectLoadedTMPFullPath, ExtractExistingFileAction.OverwriteSilently);
            }

            if (!File.Exists(projectInfo.Path)) throw new FileNotFoundException(projectInfo.Path); // Test if the file exists.
            if (!IsProject(projectInfo.Path)) throw new FileNotFoundException(projectInfo.Path); // Test if the file is a project.
            DirectoryInfo projectDirectory = new DirectoryInfo(ApplicationState.ProjectLoadedTMPFullPath);

            yield return Ninja.JumpToUnity;

            // Load Settings.
            yield return ApplicationState.CoroutineManager.StartCoroutineAsync(c_LoadSettings(projectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * settingsProgress, duration, text)));
            progress += settingsProgress;

            // Load Patients.
            yield return ApplicationState.CoroutineManager.StartCoroutineAsync(c_LoadPatients(projectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * patientsProgress, duration, text)));
            progress += patientsProgress;

            // Load Groups.
            yield return ApplicationState.CoroutineManager.StartCoroutineAsync(c_LoadGroups(projectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * groupsProgress, duration, text)));
            progress += groupsProgress;

            // Load Protocols.
            yield return ApplicationState.CoroutineManager.StartCoroutineAsync(c_LoadProtocols(projectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * protocolsProgress, duration, text)));
            progress += protocolsProgress;

            // Load Datasets.
            yield return ApplicationState.CoroutineManager.StartCoroutineAsync(c_LoadDatasets(projectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * datasetsProgress, duration, text)));
            progress += datasetsProgress;

            // Load Visualizations.
            yield return ApplicationState.CoroutineManager.StartCoroutineAsync(c_LoadVisualizations(projectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * visualizationsProgress, duration, text)));
            progress += visualizationsProgress;

            yield return Ninja.JumpToUnity;
            OnChangeProgress.Invoke(1.0f , 0, new LoadingText("Project loaded successfully."));
        }
        public IEnumerator c_Save(string path, Action<float, float, LoadingText> OnChangeProgress)
        {
            // Initialize progress.
            float steps = 3 + m_Patients.Count + m_Groups.Count + m_Protocols.Count + m_Datasets.Count + m_Visualizations.Count;
            float progress = 0.0f;

            float initializationProgress = 1 / steps;
            float settingsProgress = 1 / steps;
            float patientsProgress = m_Patients.Count / steps;
            float groupsProgress = m_Groups.Count / steps;
            float protocolsProgress = m_Protocols.Count / steps;
            float datasetsProgress = m_Datasets.Count / steps;
            float visualizationsProgress = m_Visualizations.Count / steps;
            float finalizationProgress = 1 / steps;

            // Initialization.
            yield return Ninja.JumpToUnity;
            OnChangeProgress.Invoke(progress, 0, new LoadingText("Initialization"));
            yield return Ninja.JumpBack;

            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(path);
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);
            DirectoryInfo tmpProjectDirectory = Directory.CreateDirectory(ApplicationState.ProjectLoadedTMPFullPath + "-temp");
            string oldTMPProjectDirectory = ApplicationState.ProjectLoadedTMPFullPath;
            progress += initializationProgress;

            yield return Ninja.JumpToUnity;
            OnChangeProgress.Invoke(progress, 0, new LoadingText("Saving project"));
            yield return Ninja.JumpBack;

            // Save Settings.
            yield return c_SaveSettings(tmpProjectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * settingsProgress, duration, text));
            progress += settingsProgress;

            // Save Patients
            yield return c_SavePatients(tmpProjectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * patientsProgress, duration, text));
            progress += patientsProgress;

            // Save Groups.
            yield return c_SaveGroups(tmpProjectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * groupsProgress, duration, text));
            progress += groupsProgress;

            // Save Protocols.
            yield return c_SaveProtocols(tmpProjectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * protocolsProgress, duration, text));
            progress += protocolsProgress;

            // Save Datasets
            yield return c_SaveDatasets(tmpProjectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * datasetsProgress, duration, text));
            progress += datasetsProgress;

            // Save Visualizations.
            yield return c_SaveVisualizations(tmpProjectDirectory, (localProgress, duration, text) => OnChangeProgress.Invoke(progress + localProgress * visualizationsProgress, duration, text));
            progress += visualizationsProgress;

            // Copy Icons.
            CopyIcons(Path.Combine(oldTMPProjectDirectory, "Protocols", "Icons"), Path.Combine(tmpProjectDirectory.FullName, "Protocols", "Icons"));

            // Deleting old directories.
            yield return Ninja.JumpToUnity;
            OnChangeProgress.Invoke(progress, 0, new LoadingText("Finalizing."));
            yield return Ninja.JumpBack;

            if (Directory.Exists(oldTMPProjectDirectory))
            {
                try
                {
                    Directory.Delete(oldTMPProjectDirectory, true);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotDeleteOldProjectDirectory(oldTMPProjectDirectory);
                }
            }

            try
            {
                tmpProjectDirectory.MoveTo(oldTMPProjectDirectory);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw new CanNotRenameProjectDirectory();
            }
            progress += finalizationProgress;

            // Zipping
            string filePath = Path.Combine(path, FileName);
            if (File.Exists(filePath)) File.Delete(filePath);
            using (ZipFile zip = new ZipFile(filePath))
            {
                zip.AddDirectory(oldTMPProjectDirectory);
                zip.Save();
            }

            yield return Ninja.JumpToUnity;
            OnChangeProgress.Invoke(1, 0, new LoadingText("Project saved successfully."));
        }

        public IEnumerator c_CheckDatasets(IEnumerable<Protocol> protocols, Action<float, float, LoadingText> OnChangeProgress)
        {
            yield return Ninja.JumpBack;

            IEnumerable<Dataset> datasets = m_Datasets.Where(d => protocols.Contains(d.Protocol));
            int count = 0;
            int length = datasets.SelectMany(d => d.Data).Count();
            foreach (var dataset in datasets)
            {
                for (int j = 0; j < dataset.Data.Length; ++j, ++count)
                {
                    DataInfo data = dataset.Data[j];
                    data.GetErrors(dataset.Protocol);

                    string message;
                    if (data is PatientDataInfo patientDataInfo) message = patientDataInfo.Name + " | " + dataset.Protocol.Name + " | " + patientDataInfo.Patient.Name;
                    else message = data.Name + " | " + dataset.Protocol.Name;

                    yield return Ninja.JumpToUnity;
                    OnChangeProgress.Invoke((float) count / length, 0, new LoadingText("Checking ", message, " [" + (count+1) + "/" + length + "]"));
                    yield return Ninja.JumpBack;
                }
            }
        }
        public IEnumerable c_CheckPatientTagValues(IEnumerable<Tags.Tag> tags, Action<float, float, LoadingText> OnChangeProgress)
        {
            yield return Ninja.JumpBack;

            // Test patient TagValues;
            IEnumerable<Tags.BaseTagValue> patientsTagValues = m_Patients.SelectMany(p => p.Tags);
            int count = 0;
            int length = patientsTagValues.Count();
            foreach (var tag in tags)
            {
                IEnumerable<Tags.BaseTagValue> tagValues = patientsTagValues.Where(t => t.Tag == tag);
                foreach (var tagValue in tagValues)
                {
                    tagValue.UpdateValue();
                }
                count++;

                yield return Ninja.JumpToUnity;
                OnChangeProgress.Invoke((float)count / length, 0, new LoadingText("Checking ", tag.Name, " [" + count + "/" + length + "]"));
                yield return Ninja.JumpBack;
            }
        }
        #endregion

        #region Private Methods
        IEnumerator c_LoadSettings(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            yield return Ninja.JumpBack;
            OnChangeProgress.Invoke(0, 0, new LoadingText("Loading settings"));

            FileInfo[] settingsFiles = projectDirectory.GetFiles("*" + ProjectSettings.EXTENSION, SearchOption.TopDirectoryOnly);
            if (settingsFiles.Length == 0) throw new SettingsFileNotFoundException(); // Test if settings files found.
            else if (settingsFiles.Length > 1) throw new MultipleSettingsFilesFoundException(); // Test if multiple settings files found.
            try
            {
                Settings = ClassLoaderSaver.LoadFromJson<ProjectSettings>(settingsFiles[0].FullName);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw new CanNotReadSettingsFileException(settingsFiles[0].Name);
            }
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Settings loaded successfully"));
        }
        IEnumerator c_LoadPatients(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            yield return Ninja.JumpBack;

            // Load patients.
            List<Patient> patients = new List<Patient>();
            DirectoryInfo patientDirectory = projectDirectory.GetDirectories("Patients", SearchOption.TopDirectoryOnly)[0];
            FileInfo[] patientFiles = patientDirectory.GetFiles("*" + Patient.EXTENSION, SearchOption.TopDirectoryOnly);
            for (int i = 0; i < patientFiles.Length; ++i)
            {
                FileInfo patientFile = patientFiles[i];
                OnChangeProgress.Invoke((float)i / patientFiles.Length, 0, new LoadingText("Loading patient ", Path.GetFileNameWithoutExtension(patientFile.Name), " [" + (i + 1).ToString() + "/" + patientFiles.Length + "]"));
                try
                {
                    patients.Add(ClassLoaderSaver.LoadFromJson<Patient>(patientFile.FullName));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotReadPatientFileException(Path.GetFileNameWithoutExtension(patientFile.Name));
                }
            }
            SetPatients(patients.ToArray());
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Patients loaded successfully"));
        }
        IEnumerator c_LoadGroups(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            yield return Ninja.JumpBack;

            // Load groups.
            List<Group> groups = new List<Group>();
            DirectoryInfo groupDirectory = projectDirectory.GetDirectories("Groups", SearchOption.TopDirectoryOnly)[0];
            FileInfo[] groupFiles = groupDirectory.GetFiles("*" + Group.EXTENSION, SearchOption.TopDirectoryOnly);
            for (int i = 0; i < groupFiles.Length; ++i)
            {
                FileInfo groupFile = groupFiles[i];
                OnChangeProgress.Invoke((float)i / groupFiles.Length, 0, new LoadingText("Loading group ", Path.GetFileNameWithoutExtension(groupFile.Name), " [" + (i + 1).ToString() + "/" + groupFiles.Length + "]"));
                try
                {
                    groups.Add(ClassLoaderSaver.LoadFromJson<Group>(groupFile.FullName));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotReadGroupFileException(Path.GetFileNameWithoutExtension(groupFile.Name));
                }
            }
            SetGroups(groups.ToArray());
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Groups loaded successfully"));
        }
        IEnumerator c_LoadProtocols(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            yield return Ninja.JumpBack;
            //Load Protocols
            List<Protocol> protocols = new List<Protocol>();
            DirectoryInfo protocolDirectory = projectDirectory.GetDirectories("Protocols", SearchOption.TopDirectoryOnly)[0];
            FileInfo[] protocolFiles = protocolDirectory.GetFiles("*" + Protocol.EXTENSION, SearchOption.TopDirectoryOnly);
            for (int i = 0; i < protocolFiles.Length; ++i)
            {
                FileInfo protocolFile = protocolFiles[i];
                OnChangeProgress.Invoke((float)i / protocolFiles.Length, 0, new LoadingText("Loading protocol ", Path.GetFileNameWithoutExtension(protocolFile.Name), " [" + (i + 1).ToString() + "/" + protocolFiles.Length + "]"));
                try
                {
                    protocols.Add(ClassLoaderSaver.LoadFromJson<Protocol>(protocolFile.FullName));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotReadProtocolFileException(Path.GetFileNameWithoutExtension(protocolFile.Name));
                }
            }
            SetProtocols(protocols.ToArray());
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Protocols loaded successfully"));
        }
        IEnumerator c_LoadDatasets(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            const float LOADING_TIME = 0.2f;
            const float CHECKING_TIME = 0.8f;
            yield return Ninja.JumpBack;
            //Load Datasets
            List<Dataset> datasets = new List<Dataset>();
            DirectoryInfo datasetDirectory = projectDirectory.GetDirectories("Datasets", SearchOption.TopDirectoryOnly)[0];
            FileInfo[] datasetFiles = datasetDirectory.GetFiles("*" + Dataset.EXTENSION, SearchOption.TopDirectoryOnly);
            for (int i = 0; i < datasetFiles.Length; ++i)
            {
                FileInfo datasetFile = datasetFiles[i];
                OnChangeProgress.Invoke((float)i / datasetFiles.Length * LOADING_TIME, 0, new LoadingText("Loading dataset ", Path.GetFileNameWithoutExtension(datasetFile.Name), " [" + (i + 1).ToString() + "/" + datasetFiles.Length + "]"));
                try
                {
                    datasets.Add(ClassLoaderSaver.LoadFromJson<Dataset>(datasetFile.FullName));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotReadDatasetFileException(Path.GetFileNameWithoutExtension(datasetFile.Name));
                }
            }
            SetDatasets(datasets.ToArray());
            yield return c_CheckDatasets(m_Protocols, (localProgress, duration, text) => OnChangeProgress.Invoke(LOADING_TIME + localProgress * CHECKING_TIME, duration, text));
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Datasets loaded successfully"));
        }
        IEnumerator c_LoadVisualizations(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            yield return Ninja.JumpBack;
            //Load Visualizations
            DirectoryInfo visualizationsDirectory = projectDirectory.GetDirectories("Visualizations", SearchOption.TopDirectoryOnly)[0];

            List<Visualization.Visualization> visualizations = new List<Visualization.Visualization>();
            FileInfo[] visualizationFiles = visualizationsDirectory.GetFiles("*" + Visualization.Visualization.EXTENSION, SearchOption.TopDirectoryOnly);
            for (int i = 0; i < visualizationFiles.Length; ++i)
            {
                FileInfo visualizationFile = visualizationFiles[i];
                OnChangeProgress.Invoke((float)i / visualizationFiles.Length, 0, new LoadingText("Loading visualization ", Path.GetFileNameWithoutExtension(visualizationFile.Name), " [" + (i + 1).ToString() + "/" + visualizationFiles.Length + "]"));
                try
                {
                    visualizations.Add(ClassLoaderSaver.LoadFromJson<Visualization.Visualization>(visualizationFile.FullName));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotReadVisualizationFileException(Path.GetFileNameWithoutExtension(visualizationFile.Name));
                }
            }
            SetVisualizations(visualizations.ToArray());
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Visualizations loaded successfully"));
        }

        IEnumerator c_SaveSettings(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            // Save settings
            yield return Ninja.JumpToUnity;
            OnChangeProgress.Invoke(0, 0, new LoadingText("Saving settings"));
            yield return Ninja.JumpBack;

            try
            {
                ClassLoaderSaver.SaveToJSon(Settings, Path.Combine(projectDirectory.FullName, Settings.Name + ProjectSettings.EXTENSION));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw new CanNotSaveSettingsException();
            }
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Settings saved successfully"));
        }
        IEnumerator c_SavePatients(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            DirectoryInfo patientDirectory = Directory.CreateDirectory(Path.Combine(projectDirectory.FullName, "Patients"));
            // Save patients

            int count = 0;
            int length = m_Patients.Count();
            foreach (Patient patient in m_Patients)
            {
                yield return Ninja.JumpToUnity;
                OnChangeProgress.Invoke((float)count / length, 0, new LoadingText("Saving patient ", patient.ID, " [" + (count + 1) + "/" + length + "]"));
                yield return Ninja.JumpBack;

                try
                {
                    ClassLoaderSaver.SaveToJSon(patient, Path.Combine(patientDirectory.FullName, patient.ID + Patient.EXTENSION));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotSaveSettingsException();
                }
                count++;
            }
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Patients saved successfully"));
        }
        IEnumerator c_SaveGroups(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            // Save groups
            DirectoryInfo groupDirectory = Directory.CreateDirectory(Path.Combine(projectDirectory.FullName, "Groups"));

            int count = 0;
            int length = m_Patients.Count();
            foreach (Group group in m_Groups)
            {
                yield return Ninja.JumpToUnity;
                OnChangeProgress.Invoke((float)count / length, 0, new LoadingText("Saving group ", group.Name, " [" + (count + 1) + "/" + length + "]"));
                yield return Ninja.JumpBack;

                try
                {
                    ClassLoaderSaver.SaveToJSon(group, Path.Combine(groupDirectory.FullName, group.Name + Group.EXTENSION));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotSaveSettingsException();
                }
                count++;
            }
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Groups saved successfully"));
        }
        IEnumerator c_SaveProtocols(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            // Save protocols
            DirectoryInfo protocolDirectory = Directory.CreateDirectory(Path.Combine(projectDirectory.FullName, "Protocols"));
            int count = 0;
            int length = m_Protocols.Count();
            foreach (Protocol protocol in m_Protocols)
            {
                yield return Ninja.JumpToUnity;
                OnChangeProgress.Invoke((float)count / length, 0, new LoadingText("Saving protocol ", protocol.Name, " [" + (count + 1).ToString() + "/" + length + "]"));
                yield return Ninja.JumpBack;

                try
                {
                    ClassLoaderSaver.SaveToJSon(protocol, Path.Combine(protocolDirectory.FullName, protocol.Name + Protocol.EXTENSION));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotSaveSettingsException();
                }
                count++;
            }
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Protocols saved successfully"));
        }
        IEnumerator c_SaveDatasets(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            //Save datasets
            DirectoryInfo datasetDirectory = Directory.CreateDirectory(Path.Combine(projectDirectory.FullName, "Datasets"));

            int count = 0;
            int length = m_Datasets.Count();
            foreach (Dataset dataset in m_Datasets)
            {
                yield return Ninja.JumpToUnity;
                OnChangeProgress.Invoke((float)count / length, 0, new LoadingText("Saving dataset ", dataset.Name, " [" + (count + 1).ToString() + "/" + length + "]"));
                yield return Ninja.JumpBack;

                try
                {
                    ClassLoaderSaver.SaveToJSon(dataset, Path.Combine(datasetDirectory.FullName, dataset.Name + Dataset.EXTENSION));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotSaveSettingsException();
                }
                count++;
            }
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Datasets saved successfully"));
        }
        IEnumerator c_SaveVisualizations(DirectoryInfo projectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            DirectoryInfo visualizationDirectory = Directory.CreateDirectory(Path.Combine(projectDirectory.FullName, "Visualizations"));

            //Save singleVisualizations
            int count = 0;
            int length = m_Visualizations.Count();
            foreach (Visualization.Visualization visualization in m_Visualizations)
            {
                yield return Ninja.JumpToUnity;
                OnChangeProgress.Invoke((float)count / length, 0, new LoadingText("Saving visualization ", visualization.Name, " [" + (count + 1) + "/" + length + "]"));
                yield return Ninja.JumpBack;

                try
                {
                    ClassLoaderSaver.SaveToJSon(visualization, Path.Combine(visualizationDirectory.FullName, visualization.Name + Visualization.Visualization.EXTENSION));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw new CanNotSaveSettingsException();
                }
                count++;
            }
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Visualizations saved successfully"));
        }

        void CopyIcons(string oldIconsDirectoryPath, string newIconsDirectoryPath)
        {
            new DirectoryInfo(oldIconsDirectoryPath).CopyFilesRecursively(new DirectoryInfo(newIconsDirectoryPath));
        }
        IEnumerator c_EmbedDataIntoProjectFile(DirectoryInfo projectDirectory, string oldProjectDirectory, Action<float, float, LoadingText> OnChangeProgress)
        {
            DirectoryInfo dataDirectory = Directory.CreateDirectory(Path.Combine(projectDirectory.FullName, "Data"));

            float progress = 0.0f;
            float progressStep = 1.0f / (Patients.Count + Datasets.Count);

            yield return Ninja.JumpToUnity;
            OnChangeProgress.Invoke(progress, 0, new LoadingText("Copying data"));
            yield return Ninja.JumpBack;

            // Save Patient Data
            if (Patients.Count > 0)
            {
                DirectoryInfo patientsDirectory = Directory.CreateDirectory(Path.Combine(dataDirectory.FullName, "Anatomy"));
                foreach (var patient in Patients)
                {
                    yield return Ninja.JumpToUnity;
                    progress += progressStep;
                    OnChangeProgress.Invoke(progress, 0, new LoadingText("Copying ", patient.Name, " anatomical data"));
                    yield return Ninja.JumpBack;

                    DirectoryInfo patientDirectory = Directory.CreateDirectory(Path.Combine(patientsDirectory.FullName, patient.ID));
                    if (patient.Meshes.Count > 0)
                    {
                        DirectoryInfo meshesDirectory = Directory.CreateDirectory(Path.Combine(patientDirectory.FullName, "Meshes"));
                        foreach (var mesh in patient.Meshes)
                        {
                            if (mesh is Anatomy.SingleMesh)
                            {
                                Anatomy.SingleMesh singleMesh = mesh as Anatomy.SingleMesh;
                                singleMesh.Path = singleMesh.Path.CopyToDirectory(meshesDirectory).Replace(projectDirectory.FullName, oldProjectDirectory);
                                singleMesh.MarsAtlasPath = singleMesh.MarsAtlasPath.CopyToDirectory(meshesDirectory).Replace(projectDirectory.FullName, oldProjectDirectory);
                            }
                            else if (mesh is Anatomy.LeftRightMesh)
                            {
                                Anatomy.LeftRightMesh singleMesh = mesh as Anatomy.LeftRightMesh;
                                singleMesh.LeftHemisphere = singleMesh.LeftHemisphere.CopyToDirectory(meshesDirectory).Replace(projectDirectory.FullName, oldProjectDirectory);
                                singleMesh.RightHemisphere = singleMesh.RightHemisphere.CopyToDirectory(meshesDirectory).Replace(projectDirectory.FullName, oldProjectDirectory);
                                singleMesh.LeftMarsAtlasHemisphere = singleMesh.LeftMarsAtlasHemisphere.CopyToDirectory(meshesDirectory).Replace(projectDirectory.FullName, oldProjectDirectory);
                                singleMesh.RightMarsAtlasHemisphere = singleMesh.RightMarsAtlasHemisphere.CopyToDirectory(meshesDirectory).Replace(projectDirectory.FullName, oldProjectDirectory);
                            }
                            mesh.Transformation = mesh.Transformation.CopyToDirectory(meshesDirectory).Replace(projectDirectory.FullName, oldProjectDirectory);
                        }
                    }
                    if (patient.MRIs.Count > 0)
                    {
                        DirectoryInfo mriDirectory = Directory.CreateDirectory(Path.Combine(patientDirectory.FullName, "MRIs"));
                        foreach (var mri in patient.MRIs)
                        {
                            mri.File = mri.File.CopyToDirectory(mriDirectory).Replace(projectDirectory.FullName, oldProjectDirectory);
                        }
                    }
                    if (patient.Implantations.Count > 0)
                    {
                        DirectoryInfo implantationsDirectory = Directory.CreateDirectory(Path.Combine(patientDirectory.FullName, "Implantations"));
                        foreach (var implantation in patient.Implantations)
                        {
                            implantation.File = implantation.File.CopyToDirectory(implantationsDirectory).Replace(projectDirectory.FullName, oldProjectDirectory);
                            implantation.MarsAtlas = implantation.MarsAtlas.CopyToDirectory(implantationsDirectory).Replace(projectDirectory.FullName, oldProjectDirectory);
                        }
                    }
                }
            }
            // Save Localizer Data
            if (Datasets.Count > 0)
            {
                DirectoryInfo localizersDirectory = Directory.CreateDirectory(Path.Combine(dataDirectory.FullName, "Functional"));
                foreach (var dataset in Datasets)
                {
                    if (dataset.Data.Length > 0)
                    {
                        yield return Ninja.JumpToUnity;
                        progress += progressStep;
                        OnChangeProgress.Invoke(progress, 0, new LoadingText("Copying ", dataset.Name));
                        yield return Ninja.JumpBack;

                        DirectoryInfo datasetDirectory = Directory.CreateDirectory(Path.Combine(localizersDirectory.FullName, dataset.Name));
                        foreach (var data in dataset.Data)
                        {
                            DirectoryInfo dataInfoDirectory = new DirectoryInfo(Path.Combine(datasetDirectory.FullName, data.Name));
                            if (!dataInfoDirectory.Exists) dataInfoDirectory = Directory.CreateDirectory(dataInfoDirectory.FullName);
                            data.DataContainer.CopyDataToDirectory(dataInfoDirectory, projectDirectory.FullName, oldProjectDirectory);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
