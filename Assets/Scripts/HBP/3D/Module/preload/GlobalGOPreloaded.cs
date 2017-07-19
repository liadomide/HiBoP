﻿

/* \file GlobalGOPreloaded.cs
 * \author Lance Florian
 * \date    22/04/2016
 * \brief Define GlobalGOPreloaded
 */

// system
using System;
using System.Text;

// unity
using UnityEngine;

namespace HBP.Module3D
{
    /// <summary>
    /// Preloaded GO to be instancied later
    /// </summary>
    public class GlobalGOPreloaded : MonoBehaviour
    {
        static public GameObject ColormapDisplay = null;
        static public GameObject TimeDisplay = null;
        static public GameObject CutImageOverlay = null;
        static public GameObject IconeDisplay = null;
        static public GameObject MinimizeDisplay = null;
        static public GameObject SetPlanePanel = null;
        static public GameObject ChoosePlanePanel = null;
        static public GameObject Timeline = null;
        static public GameObject IEEGLeftMenu = null;
        static public GameObject globalLeftMenu = null;
        static public GameObject ROILeftMenu = null;
        static public GameObject fMRILeftMenu = null;
        static public GameObject SiteLeftMenu = null;
        static public GameObject SceneLeftMenu = null;
        static public GameObject Line = null;
        static public GameObject View = null;
        static public GameObject ScreenMessage = null;
        static public GameObject ScreenProgressBar = null;
        static public GameObject SinglePatientCamera = null;
        static public GameObject MultiPatientsCamera = null;

        void Awake()
        {
            // Colormap display
            ColormapDisplay = Instantiate(Resources.Load("Prefabs/ui/overlay/column colormap", typeof(GameObject))) as GameObject;
            InitializeGameObject(ColormapDisplay, "Colormap Display");

            // Colormap display
            TimeDisplay = Instantiate(Resources.Load("Prefabs/ui/overlay/time display", typeof(GameObject))) as GameObject;
            InitializeGameObject(TimeDisplay, "Time Display");
            
            // Cut image overlay
            CutImageOverlay = Instantiate(Resources.Load("Prefabs/ui/overlay/image_cut", typeof(GameObject))) as GameObject;
            InitializeGameObject(CutImageOverlay, "Image cut overlay");

            // Icone display
            IconeDisplay = Instantiate(Resources.Load("Prefabs/ui/overlay/icone", typeof(GameObject))) as GameObject;
            InitializeGameObject(IconeDisplay, "Icone display");

            // Minimize display
            MinimizeDisplay = Instantiate(Resources.Load("Prefabs/ui/overlay/minimize", typeof(GameObject))) as GameObject;
            InitializeGameObject(MinimizeDisplay, "Minimize display");

            // Set plane panel
            SetPlanePanel = Instantiate(Resources.Load("Prefabs/ui/overlay/set plane", typeof(GameObject))) as GameObject;
            InitializeGameObject(SetPlanePanel, "Set plane panel");

            // Choose plane panel
            ChoosePlanePanel = Instantiate(Resources.Load("Prefabs/ui/overlay/choose plane", typeof(GameObject))) as GameObject;
            InitializeGameObject(ChoosePlanePanel, "Choose plane panel");

            // Timeline
            Timeline = Instantiate(Resources.Load("Prefabs/ui/overlay/timeline", typeof(GameObject))) as GameObject;
            InitializeGameObject(Timeline, "Tmeline");

            // IEEG left menu
            IEEGLeftMenu = Instantiate(Resources.Load("Prefabs/ui/camera/iEEG left menu", typeof(GameObject))) as GameObject;
            InitializeGameObject(IEEGLeftMenu, "IEEG left menu");

            // General left menu
            SceneLeftMenu = Instantiate(Resources.Load("Prefabs/ui/camera/Scene left menu", typeof(GameObject))) as GameObject;
            InitializeGameObject(SceneLeftMenu, "Scene left menu");
            
            // Site left menu
            SiteLeftMenu = Instantiate(Resources.Load("Prefabs/ui/camera/Site left menu", typeof(GameObject))) as GameObject;
            InitializeGameObject(SiteLeftMenu, "Site left menu");            

            // ROI left menu
            ROILeftMenu = Instantiate(Resources.Load("Prefabs/ui/camera/ROI left menu", typeof(GameObject))) as GameObject;
            InitializeGameObject(ROILeftMenu, "ROI left menu");

            // fMRI left menu
            fMRILeftMenu = Instantiate(Resources.Load("Prefabs/ui/camera/fMRI left menu", typeof(GameObject))) as GameObject;
            InitializeGameObject(fMRILeftMenu, "fMRI left menu");

            // global left menu
            globalLeftMenu = Instantiate(Resources.Load("Prefabs/ui/camera/Global left menu", typeof(GameObject))) as GameObject;
            InitializeGameObject(globalLeftMenu, "Global left menu");

            // Line views panel
            Line = Instantiate(Resources.Load("Prefabs/ui/camera/Camera line views panel", typeof(GameObject))) as GameObject;
            InitializeGameObject(Line, "LineViews");

            // View panel
            View = Instantiate(Resources.Load("Prefabs/ui/camera/Camera view panel", typeof(GameObject))) as GameObject;
            InitializeGameObject(View, "View");

            // Screen message panel
            ScreenMessage = Instantiate(Resources.Load("Prefabs/ui/overlay/screen message panel", typeof(GameObject))) as GameObject;            
            InitializeGameObject(ScreenMessage, "Screen message");

            // Screen progress bar panel
            ScreenProgressBar = Instantiate(Resources.Load("Prefabs/ui/overlay/progressbar", typeof(GameObject))) as GameObject;
            InitializeGameObject(ScreenProgressBar, "Screen Progress Bar");
        }

        private void InitializeGameObject(GameObject baseGameObject, string objectName)
        {
            baseGameObject.name = objectName;
            baseGameObject.transform.SetParent(transform);
            baseGameObject.SetActive(false);
        }
    }
}