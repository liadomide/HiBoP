﻿using CielaSpike;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Tools.CSharp;
using Tools.Unity;

namespace HBP.Data
{
    /// <summary>
    /// Contains all the data about a patient.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <listheader>
    /// <term>Data</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term><b>Name</b></term>
    /// <description>Name of the patient.</description>
    /// </item>
    /// <item>
    /// <term><b>Date</b></term>
    /// <description>Year in which the patient was implanted.</description>
    /// </item>
    /// <item>
    /// <term><b>Place</b></term>
    /// <description>Place where the patient had the operation.</description>
    /// </item>
    /// <item>
    /// <term><b>Meshes</b></term>
    /// <description>Meshes of the patient.</description>
    /// </item>
    /// <item>
    /// <term><b>MRIs</b></term>
    /// <description>MRI scans of the patient.</description>
    /// </item>
    /// <item>
    /// <term><b>Sites</b></term>
    /// <description>Sites of the patient.</description>
    /// </item>
    /// <item>
    /// <term><b>Tags</b></term>
    /// <description>Tags of the patient.</description>
    /// </item>
    /// </list>
    /// </remarks>
    [DataContract]
    public class Patient : BaseData, ILoadable<Patient>, ILoadableFromDatabase<Patient>, INameable
    {
        #region Properties
        /// <summary>
        /// Extension of patient files.
        /// </summary>
        public const string EXTENSION = ".patient";
        /// <summary>
        /// Name of the patient.
        /// </summary>
        [DataMember] public string Name { get; set; }
        /// <summary>
        /// Year in which the patient was implanted.
        /// </summary>
        [DataMember] public int Date { get; set; }
        /// <summary>
        /// Place where the patient had the operation.
        /// </summary>
        [DataMember] public string Place { get; set; }
        /// <summary>
        /// Meshes of the patient.
        /// </summary>
        [DataMember] public List<BaseMesh> Meshes { get; set; }
        /// <summary>
        /// MRI scans of the patient.
        /// </summary>
        [DataMember] public List<MRI> MRIs { get; set; }
        /// <summary>
        /// Sites of the patient.
        /// </summary>
        [DataMember] public List<Site> Sites { get; set; }
        /// <summary>
        /// Tags of the patient.
        /// </summary>
        [DataMember] public List<BaseTagValue> Tags { get; set; }
        /// <summary>
        /// Complete name of the patient. Name (Place-Date).
        /// </summary>
        [IgnoreDataMember] public string CompleteName { get { return Name + " (" + Place + " - " + Date + ")"; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Patient class.
        /// </summary>
        /// <param name="name">Name of the patient.</param>
        /// <param name="place">Place where the  patient had the operation.</param>
        /// <param name="date">Year in which the patient was implanted.</param>
        /// <param name="meshes">Meshes of the patient.</param>
        /// <param name="MRIs">MRI scans of the patient.</param>
        /// <param name="sites">Sites of the patient.</param>
        /// <param name="tags">Tags of the patient.</param>
        /// <param name="ID">Unique identifier to identify the patient.</param>
        public Patient(string name, string place, int date, IEnumerable<BaseMesh> meshes, IEnumerable<MRI> MRIs, IEnumerable<Site> sites, IEnumerable<BaseTagValue> tags, string ID) : base(ID)
        {
            Name = name;
            Place = place;
            Date = date;
            Meshes = meshes.ToList();
            this.MRIs = MRIs.ToList();
            Sites = sites.ToList();
            Tags = tags.ToList();
        }
        /// <summary>
        /// Initializes a new instance of the Patient class.
        /// </summary>
        /// <param name="name">Name of the patient.</param>
        /// <param name="place">Place where the  patient had the operation.</param>
        /// <param name="date">Year in which the patient was implanted.</param>
        /// <param name="meshes">Meshes of the patient.</param>
        /// <param name="MRIs">MRI scans of the patient.</param>
        /// <param name="sites">Sites of the patient.</param>
        /// <param name="tags">Tags of the patient.</param>
        public Patient(string name, string place, int date, IEnumerable<BaseMesh> meshes, IEnumerable<MRI> MRIs, IEnumerable<Site> sites, IEnumerable<BaseTagValue> tags) : base()
        {
            Name = name;
            Place = place;
            Date = date;
            Meshes = meshes.ToList();
            this.MRIs = MRIs.ToList();
            Sites = sites.ToList();
            Tags = tags.ToList();
        }
        /// <summary>
        /// Initializes a new instance of the Patient class.
        /// </summary>
        public Patient() : this("Unknown", "Unknown", 0, new BaseMesh[0], new MRI[0], new Site[0], new BaseTagValue[0])
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Generates  ID recursively.
        /// </summary>
        public override void GenerateID()
        {
            base.GenerateID();
            foreach (var mesh in Meshes) mesh.GenerateID();
            foreach (var mri in MRIs) mri.GenerateID();
            foreach (var site in Sites) site.GenerateID();
            foreach (var tag in Tags) tag.GenerateID();
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Gets the extension of the patient files.
        /// </summary>
        /// <returns></returns>
        public static string[] GetExtensions()
        {
            return new string[] { EXTENSION[0] == '.' ? EXTENSION.Substring(1) : EXTENSION };
        }
        /// <summary>
        /// Determines if the specified directory is a patient directory.
        /// </summary>
        /// <param name="path">The specified directory.</param>
        /// <returns><see langword="true"/> if the directory is a patient directory; otherwise, <see langword="false"/></returns>
        public static bool IsPatientDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists) return false;
            string[] nameElements = directory.Name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameElements.Length != 3) return false;
            DirectoryInfo[] directories = directory.GetDirectories();
            if (!directories.Any((dir) => dir.Name == "implantation") || !directories.Any((dir) => dir.Name == "t1mri")) return false;
            return true;
        }
        /// <summary>
        /// Loads patients from a directory
        /// </summary>
        /// <param name="path">The specified path of the patient directory.</param>
        /// <param name="result">The patient in the patient directory.</param>
        /// <returns><see langword="true"/> if the method worked successfully; otherwise, <see langword="false"/></returns>
        public static bool LoadFromDirectory(string path, out Patient result)
        {
            result = null;
            if (IsPatientDirectory(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                string[] directoryNameParts = directory.Name.Split(new char[1] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                int.TryParse(directoryNameParts[1], out int date);
                string name = directoryNameParts[2];
                string place = directoryNameParts[0];
                IEnumerable<BaseTag> tags = ApplicationState.ProjectLoaded.Preferences.GeneralTags.Concat(ApplicationState.ProjectLoaded.Preferences.PatientsTags);
                IntTag dateTag = tags.OfType<IntTag>().FirstOrDefault(t => t.Name == "Date");
                if (dateTag == null)
                {
                    dateTag = new IntTag("Date");
                    ApplicationState.ProjectLoaded.Preferences.PatientsTags.Add(dateTag);
                }
                StringTag placeTag = tags.OfType<StringTag>().FirstOrDefault(t => t.Name == "Place");
                if (placeTag == null)
                {
                    placeTag = new StringTag("Place");
                    ApplicationState.ProjectLoaded.Preferences.PatientsTags.Add(placeTag);
                }
                IntTagValue dateTagValue = new IntTagValue(dateTag, date);
                StringTagValue placeTagValue = new StringTagValue(placeTag, place);
                result = new Patient(name, place, date, BaseMesh.LoadFromDirectory(path), MRI.LoadFromDirectory(path), Site.LoadFromIntranatDirectory(path), new BaseTagValue[] { dateTagValue, placeTagValue }, directory.Name);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Loads patient from patient file.
        /// </summary>
        /// <param name="path">The specified path of the patient file.</param>
        /// <param name="result">The patient in the patient file.</param>
        /// <returns><see langword="true"/> if the method worked successfully; otherwise, <see langword="false"/></returns>
        public static bool LoadFromFile(string path, out Patient result)
        {
            result = null;
            try
            {
                result = ClassLoaderSaver.LoadFromJson<Patient>(path);
                return result != null;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw new CanNotReadPatientFileException(Path.GetFileNameWithoutExtension(path));
            }
        }
        /// <summary>
        /// Loads patients from a database. 
        /// </summary>
        /// <param name="path">The specified path of the database.</param>
        /// <param name="patients">The patients in the database.</param>
        /// <returns><see langword="true"/> if the method worked successfully; otherwise, <see langword="false"/></returns>
        public static bool LoadFromDatabase(string path, out Patient[] patients)
        {
            patients = new Patient[0];
            return true;
        }
        public static bool LoadFromIntranatDatabase(string path, out Patient[] patients)
        {
            patients = new Patient[0];
            if (string.IsNullOrEmpty(path)) return false;
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists) return false;
            IEnumerable<string> patientDirectories = (from dir in directory.GetDirectories() where IsPatientDirectory(dir.FullName) select dir.FullName).ToArray();
            List<Patient> patientsList = new List<Patient>(patientDirectories.Count());
            foreach (string dir in patientDirectories)
            {
                if (LoadFromDirectory(dir, out Patient patient))
                {
                    patientsList.Add(patient);
                }
            }
            patients = patientsList.ToArray();
            return true;
        }
        public static bool LoadFromBIDSDatabase(string path, out Patient[] patients)
        {
            patients = new Patient[0];
            if (string.IsNullOrEmpty(path)) return false;
            DirectoryInfo databaseDirectoryInfo = new DirectoryInfo(path);
            if (!databaseDirectoryInfo.Exists) return false;

            // Read participants.tsv.
            FileInfo participantsFileInfo = new FileInfo(databaseDirectoryInfo.FullName + "/participants.tsv");
            if (!participantsFileInfo.Exists) return false;
            Dictionary<string, Dictionary<string, string>> tagValuesBySubjectID = new Dictionary<string, Dictionary<string, string>>();
            using (StreamReader streamReader = new StreamReader(participantsFileInfo.FullName))
            {
                string[] lines = streamReader.ReadToEnd().Split(new string[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0) return true;
                string[] tags = lines[0].Split(new char[] { '\t' });
                for (int l = 1; l < lines.Length; l++)
                {
                    string[] values = lines[l].Split(new char[] { '\t' });
                    if (values.Length == tags.Length)
                    {
                        Dictionary<string, string> valueByTag = new Dictionary<string, string>();
                        for (int t = 1; t < tags.Length; t++)
                        {
                            valueByTag.Add(tags[t], values[t]);
                        }
                        tagValuesBySubjectID.Add(values[0], valueByTag);
                    }
                }
            }

            // Find mesh files.
            Regex meshRegex = new Regex(@"sub-([a-zA-Z0-9.]+)(_ses-([a-zA-Z0-9.]+))?(_acq-([a-zA-Z0-9.]+))?(_ce-([a-zA-Z0-9.]+))?(_rec-([a-zA-Z0-9.]+))?(_run-([a-zA-Z0-9.]+))?(_[a-zA-Z0-9.-]+)*(_hemi-([a-zA-Z0-9.-]))(_[a-zA-Z0-9.-]+)*_([a-zA-Z0-9.-]+)\.gii$");
            FileInfo[] meshFiles = databaseDirectoryInfo.GetFiles("*.gii", SearchOption.AllDirectories);
            Dictionary<string, List<BIDSMeshFile>> meshesFilesBySubjectID = new Dictionary<string, List<BIDSMeshFile>>();
            foreach (var file in meshFiles)
            {
                Match match = meshRegex.Match(file.FullName);
                if (match.Success)
                {
                    BIDSMeshFile meshFile = new BIDSMeshFile();
                    GroupCollection groups = match.Groups;
                    meshFile.Subject = groups[1].Value;
                    meshFile.Session = groups[3].Value;
                    meshFile.DataAcquisition = groups[5].Value;
                    meshFile.Contrast = groups[7].Value;
                    meshFile.Reconstruction = groups[9].Value;
                    if (int.TryParse(groups[11].Value, out int run)) meshFile.Run = run;
                    meshFile.Hemisphere = groups[14].Value;
                    meshFile.Name = groups[16].Value;
                    meshFile.Path = file.FullName;
                    if (meshesFilesBySubjectID.TryGetValue(meshFile.Subject, out List<BIDSMeshFile> files))
                    {
                        files.Add(meshFile);
                    }
                    else
                    {
                        meshesFilesBySubjectID[meshFile.Subject] = new List<BIDSMeshFile>() { meshFile };
                    }
                }
            }

            // Find MRI files.
            Regex mriRegex = new Regex(@"sub-(\w+)(_ses-(\w+))?(_acq-(\w+))?(_ce-(\w+))?(_rec-(\w+))?(_run-(\w+))?_(T1w|T2w|T1rho|T1map|T2map|T2star|FLAIR|FLASH|PD|PDmap|PDT2|inplaneT1|inplaneT2|angio)\.nii(\.gz)?$");
            FileInfo[] mriFiles = databaseDirectoryInfo.GetFiles("*.nii", SearchOption.AllDirectories);
            Dictionary<string, List<BIDSMRIFile>> mriFilesBySubjectID = new Dictionary<string, List<BIDSMRIFile>>();
            foreach (var file in mriFiles)
            {
                Match match = mriRegex.Match(file.FullName);
                if (match.Success)
                {
                    BIDSMRIFile mriFile = new BIDSMRIFile();
                    GroupCollection groups = match.Groups;
                    mriFile.Subject = groups[1].Value;
                    mriFile.Session = groups[3].Value;
                    mriFile.DataAcquisition = groups[5].Value;
                    mriFile.Contrast = groups[7].Value;
                    mriFile.Reconstruction = groups[9].Value;
                    if (int.TryParse(groups[11].Value, out int run)) mriFile.Run = run;
                    mriFile.Name = groups[12].Value;
                    mriFile.Path = file.FullName;
                    if (mriFilesBySubjectID.TryGetValue(mriFile.Subject, out List<BIDSMRIFile> files))
                    {
                        files.Add(mriFile);
                    }
                    else
                    {
                        mriFilesBySubjectID[mriFile.Subject] = new List<BIDSMRIFile>() { mriFile };
                    }
                }
            }

            // Find Electrodes files.
            Regex electrodesRegex = new Regex(@"sub-(\w+)(_ses-(\w+))?(_acq-(\w+))?(_ce-(\w+))?(_rec-(\w+))?(_run-(\w+))?(_space-(\w+))?_electrodes\.tsv?$");
            FileInfo[] electrodesFiles = databaseDirectoryInfo.GetFiles("*_electrodes.tsv", SearchOption.AllDirectories);
            Dictionary<string, List<BIDSElectrodeFile>> electrodesFilesBySubjectID = new Dictionary<string, List<BIDSElectrodeFile>>();
            foreach (var file in electrodesFiles)
            {
                Match match = mriRegex.Match(file.FullName);
                if (match.Success)
                {
                    BIDSElectrodeFile electrodeFile = new BIDSElectrodeFile();
                    GroupCollection groups = match.Groups;
                    electrodeFile.Subject = groups[1].Value;
                    electrodeFile.Session = groups[3].Value;
                    electrodeFile.DataAcquisition = groups[5].Value;
                    electrodeFile.Contrast = groups[7].Value;
                    electrodeFile.Reconstruction = groups[9].Value;
                    if (int.TryParse(groups[11].Value, out int run)) electrodeFile.Run = run;
                    electrodeFile.Space = groups[12].Value;
                    electrodeFile.Name = groups[12].Value;
                    electrodeFile.Path = file.FullName;
                    if (electrodesFilesBySubjectID.TryGetValue(electrodeFile.Subject, out List<BIDSElectrodeFile> files))
                    {
                        files.Add(electrodeFile);
                    }
                    else
                    {
                        electrodesFilesBySubjectID[electrodeFile.Subject] = new List<BIDSElectrodeFile>() { electrodeFile };
                    }
                }
            }

            // Create patients.
            List<Patient> patientsList = new List<Patient>(tagValuesBySubjectID.Count);
            foreach (var pair in tagValuesBySubjectID)
            {
                // Meshes.
                List<BaseMesh> meshes = new List<BaseMesh>();
                var subjectMeshFiles = meshesFilesBySubjectID[pair.Key];
                foreach (var meshFile in subjectMeshFiles)
                {
                    if (meshFile.Hemisphere == "L" || meshFile.Hemisphere == "l" || meshFile.Hemisphere == "left" || meshFile.Hemisphere == "Left")
                    {
                        var rightMeshFile = subjectMeshFiles.FirstOrDefault(f => f.Same(meshFile) && (f.Hemisphere == "R" || f.Hemisphere == "r" || f.Hemisphere == "right" || f.Hemisphere == "Right"));
                        if (rightMeshFile == null) rightMeshFile = new BIDSMeshFile();
                        meshes.Add(new LeftRightMesh(meshFile.Name, "", meshFile.Path, rightMeshFile.Path, "", ""));
                    }
                    else if (meshFile.Hemisphere == "R" || meshFile.Hemisphere == "r" || meshFile.Hemisphere == "right" || meshFile.Hemisphere == "Right")
                    {
                        var leftMeshFile = subjectMeshFiles.FirstOrDefault(f => f.Same(meshFile) && (f.Hemisphere == "L" || f.Hemisphere == "l" || f.Hemisphere == "left" || f.Hemisphere == "Left"));
                        if (leftMeshFile == null) leftMeshFile = new BIDSMeshFile();
                        meshes.Add(new LeftRightMesh(meshFile.Name, "", leftMeshFile.Path, meshFile.Path, "", ""));
                    }
                    else
                    {
                        meshes.Add(new SingleMesh(meshFile.Name, "", meshFile.Path, ""));
                    }
                }

                // MRIs.
                var mris = mriFilesBySubjectID[pair.Key].Select(f => new MRI(f.Name, f.Path));

                // Sites.
                Site[] sites = new Site[0]; // Site.LoadImplantationFromBIDSFile(;

                // Tags.
                BaseTagValue[] tags = null;
                Patient patient = new Patient(pair.Key, "", 0, meshes, mris, sites, tags);
                patientsList.Add(patient);
            }
            patients = patientsList.ToArray();
            return true;
        }
        public static IEnumerator c_LoadFromDatabase(string path, Action<float, float, LoadingText> OnChangeProgress, Action<IEnumerable<Patient>> result)
        {
            yield return Ninja.JumpBack;

            List<Patient> patients = new List<Patient>();
            if (!string.IsNullOrEmpty(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                if (directory.Exists)
                {
                    IEnumerable<DirectoryInfo> patientDirectories = directory.GetDirectories().Where(d => IsPatientDirectory(d.FullName));
                    int length = patientDirectories.Count();
                    int i = 0;
                    foreach (var dir in patientDirectories)
                    {
                        yield return Ninja.JumpToUnity;
                        OnChangeProgress.Invoke((float)i / length, 0, new LoadingText("Loading patient ", dir.Name, " [" + (i + 1) + "/" + length + "]"));
                        yield return Ninja.JumpBack;
                        if (LoadFromDirectory(dir.FullName, out Patient patient))
                        {
                            patients.Add(patient);
                        }
                        i++;
                    }
                }
            }

            yield return Ninja.JumpToUnity;
            OnChangeProgress.Invoke(1.0f, 0, new LoadingText("Patients loaded successfully"));
            result(patients);
        }
        #endregion

        #region Operators
        /// <summary>
        /// Clone the instance.
        /// </summary>
        /// <returns>object cloned.</returns>
        public override object Clone()
        {
            return new Patient(Name, Place, Date, Meshes.DeepClone(), MRIs.DeepClone(), Sites.DeepClone(), Tags.DeepClone(), ID);
        }
        /// <summary>
        /// Copy the instance.
        /// </summary>
        /// <param name="obj">instance to copy.</param>
        public override void Copy(object obj)
        {
            base.Copy(obj);
            if (obj is Patient patient)
            {
                Name = patient.Name;
                Date = patient.Date;
                Place = patient.Place;
                Meshes = patient.Meshes;
                MRIs = patient.MRIs;
                Sites = patient.Sites;
                Tags = patient.Tags;
            }
        }
        #endregion

        #region Interfaces
        string[] ILoadable<Patient>.GetExtensions()
        {
            return GetExtensions();
        }
        bool ILoadable<Patient>.LoadFromFile(string path, out Patient[] result)
        {
            bool success = LoadFromFile(path, out Patient patient);
            result = new Patient[] { patient };
            return success;
        }
        bool ILoadableFromDatabase<Patient>.LoadFromDatabase(string path, out Patient[] result)
        {
            return LoadFromDatabase(path, out result);
        }
        IEnumerator ILoadableFromDatabase<Patient>.LoadFromDatabase(string path, Action<float, float, LoadingText> OnChangeProgress, Action<IEnumerable<Patient>> result)
        {
            yield return Ninja.JumpToUnity;
            yield return ApplicationState.CoroutineManager.StartCoroutineAsync(c_LoadFromDatabase(path, OnChangeProgress, result));
            yield return Ninja.JumpBack;
        }
        #endregion

        class BIDSFile
        {
            public string Name;
            public string Subject;
            public string Session;
            public string DataAcquisition;
            public string Contrast;
            public string Reconstruction;
            public int Run;
            public string Path;

            public bool Same(BIDSFile file)
            {
                return file.Name == Name
                    && file.Subject == Subject
                    && file.Session == Session
                    && file.DataAcquisition == DataAcquisition
                    && file.Contrast == Contrast
                    && file.Reconstruction == Reconstruction
                    && file.Run == Run;
            }
        }
        class BIDSMeshFile : BIDSFile
        {
            public string Hemisphere;
        }
        class BIDSMRIFile : BIDSFile
        {

        }
        class BIDSElectrodeFile : BIDSFile
        {
            public string Space;
        }
    }
}