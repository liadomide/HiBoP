﻿using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace HBP.Module3D.IBC
{
    /// <summary>
    /// Global information for IBC functional atlas (correspondance table between files name and 
    /// </summary>
    public class IBCInformation
    {
        #region Structs
        /// <summary>
        /// Structure containing the labels for a contrast
        /// </summary>
        public struct Labels
        {
            #region Propreties
            public string PrettyName { get; private set; }
            public string ControlCondition { get; private set; }
            public string TargetCondition { get; private set; }
            #endregion

            #region Constructors
            public Labels(string prettyName, string controlCondition, string targetCondition)
            {
                PrettyName = prettyName;
                ControlCondition = controlCondition;
                TargetCondition = targetCondition;
            }
            #endregion
        }
        #endregion

        #region Properties
        /// <summary>
        /// Correspondance table between raw name (from the file name) and the corresponding <see cref="Labels"/> object
        /// </summary>
        private Dictionary<string, Labels> m_LabelsByRawName = new Dictionary<string, Labels>();
        #endregion

        #region Constructors
        public IBCInformation(string csvFile)
        {
            Regex csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            if (new FileInfo(csvFile).Exists)
            {
                using (StreamReader sr = new StreamReader(csvFile))
                {
                    string line = sr.ReadLine();
                    while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                    {
                        string[] splits = csvParser.Split(line);
                        string rawName = splits.Length > 1 ? splits[1].TrimStart(' ', '"').TrimEnd('"') : "";
                        string prettyName = splits.Length > 2 ? splits[2].TrimStart(' ', '"').TrimEnd('"') : "";
                        string controlCondition = splits.Length > 3 ? splits[3].TrimStart(' ', '"').TrimEnd('"') : "";
                        string targetCondition = splits.Length > 4 ? splits[4].TrimStart(' ', '"').TrimEnd('"') : "";
                        m_LabelsByRawName.Add(rawName, new Labels(prettyName, controlCondition, targetCondition));
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the <see cref="Labels"/> object corresponding to the raw file name
        /// </summary>
        /// <param name="rawName">Raw name (from the file name)</param>
        /// <returns>Corresponding Labels object</returns>
        public Labels GetLabels(string rawName)
        {
            if (m_LabelsByRawName.TryGetValue(rawName, out Labels labels))
            {
                return labels;
            }
            return new Labels(rawName, "Unknown Condition", "Unknown Target");
        }
        #endregion
    }
}