﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CielaSpike;
using Tools.Unity;
using HBP.Data.General;
using System.Linq;

namespace HBP.UI
{
	public class OpenProject : Window 
	{
		#region Properties
		[SerializeField] FolderSelector m_LocationFolderSelector;
		[SerializeField] ProjectList m_ProjectList;
        [SerializeField] Button m_LoadingButton;

        public override bool Interactable
        {
            get
            {
                return base.Interactable;
            }

            set
            {
                base.Interactable = value;

                m_LocationFolderSelector.interactable = value;
                m_ProjectList.Interactable = value;
                m_LoadingButton.interactable = value;
            }
        }
        #endregion

        #region Public Methods
        public void Load(ProjectInfo info)
        {
            FindObjectOfType<ProjectLoaderSaver>().Load(info);
            base.Close();
        }
        public void Load()
		{
            if (ApplicationState.ProjectLoaded != null)
            {
                if (ApplicationState.ProjectLoaded.Visualizations.Any(v => v.IsOpen))
                {
                    ApplicationState.DialogBoxManager.Open(DialogBoxManager.AlertType.WarningMultiOptions, "Opened visualizations", "Some visualizations of the currently loaded project are opened. Loading another project will close any opened visualization.\n\nWould you like to load another project ?", () =>
                    {
                        ApplicationState.Module3D.RemoveAllScenes();
                        Load(m_ProjectList.ObjectsSelected[0]);
                    },
                    "Load project");
                }
                else
                {
                    Load(m_ProjectList.ObjectsSelected[0]);
                }
            }
            else
            {
                Load(m_ProjectList.ObjectsSelected[0]);
            }
		}
        #endregion

        #region Private Methods
        protected override void Initialize()
        {
            // Initialize project list.
            m_ProjectList.Initialize();
            m_ProjectList.OnSelectionChanged.AddListener(() => m_LoadingButton.interactable = m_ProjectList.ObjectsSelected.Length > 0);
            m_ProjectList.OnAction.AddListener((info, i) => Load(info));

            // Initialise location folder selector.
            m_LocationFolderSelector.onValueChanged.AddListener((value) => this.StartCoroutineAsync(c_DisplayProjects(value)));

            // Base method.
            base.Initialize();
        }
        protected override void SetFields()
        {
            // Base method.
            base.SetFields();

            // Set location folder selector.
            m_LocationFolderSelector.Folder = ApplicationState.UserPreferences.General.Project.DefaultLocation;
        }
        #endregion

        #region Coroutines
        IEnumerator c_DisplayProjects(string path)
        {
            yield return Ninja.JumpToUnity;
            m_LoadingButton.interactable = false;
            m_ProjectList.Objects = new ProjectInfo[0];
            yield return Ninja.JumpBack;
            string[] paths = HBP.Data.General.Project.GetProject(path).ToArray();
            foreach (string projectPath in paths)
            {
                ProjectInfo project = new ProjectInfo(projectPath);
                yield return Ninja.JumpToUnity;
                m_ProjectList.Add(project);
                yield return Ninja.JumpBack;
            }
            yield return Ninja.JumpToUnity;
            m_ProjectList.SortByName(ProjectList.Sorting.Descending);
        }
        #endregion
    }
}