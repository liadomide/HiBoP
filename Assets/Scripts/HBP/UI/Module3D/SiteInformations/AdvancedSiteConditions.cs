﻿using HBP.Module3D;
using System.Linq;
using Tools.CSharp.BooleanExpressionParser;
using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI.Module3D
{
    /// <summary>
    /// Class used to define a set of conditions to check on the sites from a string
    /// </summary>
    public class AdvancedSiteConditions : BaseSiteConditions
    {
        #region Properties
        public const string TRUE = "TRUE";
        public const string FALSE = "FALSE";
        public const string HIGHLIGHTED = "H";
        public const string BLACKLISTED = "B";
        public const string LABEL = "LABEL";
        public const string IN_ROI = "ROI";
        public const string IN_MESH = "MESH";
        public const string IN_LEFT_HEMISPHERE = "L";
        public const string IN_RIGHT_HEMISPHERE = "R";
        public const string ON_PLANE = "CUT";
        public const string NAME = "NAME";
        public const string PATIENT_NAME = "PAT_NAME";
        public const string PATIENT_PLACE = "PAT_PLACE";
        public const string PATIENT_DATE = "PAT_DATE";
        public const string TAG = "TAG";
        public const string MEAN = "MEAN";
        public const string MEDIAN = "MEDIAN";
        public const string MAX = "MAX";
        public const string MIN = "MIN";
        public const string STANDARD_DEVIATION = "STDEV";

        /// <summary>
        /// InputField used to write the string to be parsed as a set of conditions
        /// </summary>
        [SerializeField] InputField m_InputField;
        /// <summary>
        /// Boolean expression parsed from the string
        /// </summary>
        private BooleanExpression m_BooleanExpression;
        #endregion

        #region Private Methods
        /// <summary>
        /// Check all the set conditions for a specific site
        /// </summary>
        /// <param name="site">Site to check</param>
        /// <returns>True if the conditions are met</returns>
        protected override bool CheckConditions(Site site)
        {
            foreach (var booleanValue in m_BooleanExpression.GetAllBooleanValuesUnderThisOne())
            {
                booleanValue.SetBooleanValue((s) => ParseConditionAndCheckValue(site, s));
            }
            return m_BooleanExpression.Evaluate();
        }
        /// <summary>
        /// Parse the string containing the conditions and get the value 
        /// </summary>
        /// <param name="site">Site to check</param>
        /// <param name="s">String to be parsed</param>
        /// <returns>True if the site matches the set of conditions</returns>
        private bool ParseConditionAndCheckValue(Site site, string s)
        {
            s = s.ToUpper();
            if (s.Contains("=") || s.Contains(">") || s.Contains("<"))
            {
                string[] elements = s.Split('=', '<', '>');
                if (elements.Length == 2)
                {
                    string label = elements[0].Replace(" ", "").Replace("\"", "").Replace("[", "").Replace("]", "");
                    string value = elements[1].Replace("\"", "");
                    string deblankedValue = System.Text.RegularExpressions.Regex.Replace(value, "^\\s+", "");
                    deblankedValue = System.Text.RegularExpressions.Regex.Replace(deblankedValue, "\\s+$", "");
                    if (label == LABEL)
                    {
                        return CheckLabel(site, deblankedValue);
                    }
                    else if (label == NAME)
                    {
                        return CheckName(site, deblankedValue);
                    }
                    else if (label == PATIENT_NAME)
                    {
                        return CheckPatientName(site, deblankedValue);
                    }
                    else if (label == PATIENT_PLACE)
                    {
                        return CheckPatientPlace(site, deblankedValue);
                    }
                    else if (label == PATIENT_DATE)
                    {
                        return CheckPatientDate(site, deblankedValue);
                    }
                    else if (label == TAG)
                    {
                        string[] splits = deblankedValue.Split(':');
                        if (splits.Length == 2)
                        {
                            string tagName = System.Text.RegularExpressions.Regex.Replace(splits[0], "^\\s+", "");
                            tagName = System.Text.RegularExpressions.Regex.Replace(tagName, "\\s+$", "");
                            string tagValue = System.Text.RegularExpressions.Regex.Replace(splits[1], "^\\s+", "");
                            tagValue = System.Text.RegularExpressions.Regex.Replace(tagValue, "\\s+$", "");
                            Data.BaseTag tag = ApplicationState.ProjectLoaded.Preferences.SitesTags.FirstOrDefault(t => t.Name.ToUpper() == tagName);
                            if (tag == null) tag = ApplicationState.ProjectLoaded.Preferences.GeneralTags.FirstOrDefault(t => t.Name.ToUpper() == tagName);
                            return CheckTag(site, tag, tagValue);
                        }
                    }
                    else if (label == MEAN)
                    {
                        if (s.Contains("<"))
                        {
                            return CheckMean(site, false, deblankedValue);
                        }
                        else if (s.Contains(">"))
                        {
                            return CheckMean(site, true, deblankedValue);
                        }
                    }
                    else if (label == MEDIAN)
                    {
                        if (s.Contains("<"))
                        {
                            return CheckMedian(site, false, deblankedValue);
                        }
                        else if (s.Contains(">"))
                        {
                            return CheckMedian(site, true, deblankedValue);
                        }
                    }
                    else if (label == MAX)
                    {
                        if (s.Contains("<"))
                        {
                            return CheckMax(site, false, deblankedValue);
                        }
                        else if (s.Contains(">"))
                        {
                            return CheckMax(site, true, deblankedValue);
                        }
                    }
                    else if (label == MIN)
                    {
                        if (s.Contains("<"))
                        {
                            return CheckMin(site, false, deblankedValue);
                        }
                        else if (s.Contains(">"))
                        {
                            return CheckMin(site, true, deblankedValue);
                        }
                    }
                    else if (label == STANDARD_DEVIATION)
                    {
                        if (s.Contains("<"))
                        {
                            return CheckStandardDeviation(site, false, deblankedValue);
                        }
                        else if (s.Contains(">"))
                        {
                            return CheckStandardDeviation(site, true, deblankedValue);
                        }
                    }
                }
            }
            else
            {
                s = s.Replace(" ", "").Replace("\"", "").Replace("[", "").Replace("]", "");
                if (s == TRUE)
                {
                    return true;
                }
                else if (s == FALSE)
                {
                    return false;
                }
                else if (s == HIGHLIGHTED)
                {
                    return CheckHighlighted(site);
                }
                else if (s == BLACKLISTED)
                {
                    return CheckBlacklisted(site);
                }
                else if (s == IN_ROI)
                {
                    return CheckInROI(site);
                }
                else if (s == IN_MESH)
                {
                    return CheckInMesh(site);
                }
                else if (s == IN_LEFT_HEMISPHERE)
                {
                    return CheckInLeftHemisphere(site);
                }
                else if (s == IN_RIGHT_HEMISPHERE)
                {
                    return CheckInRightHemisphere(site);
                }
                else if (s == ON_PLANE)
                {
                    return CheckOnPlane(site);
                }
            }
            throw new InvalidAdvancedConditionException(s);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Parse the whole string and store it to a BooleanExpression object
        /// </summary>
        public void ParseConditions()
        {
            m_BooleanExpression = Parser.Parse(m_InputField.text);
        }
        #endregion
    }
}