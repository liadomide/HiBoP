﻿

/**
 * \file    Column3DViewFMRI.cs
 * \author  Lance Florian
 * \date    29/02/2016
 * \brief   Define Column3DViewFMRI class
 */

// system
using System.Collections.Generic;

// unity
using UnityEngine;

namespace HBP.Module3D
{
    /// <summary>
    /// A 3D column view class, containing all necessary data concerning an FMRI column
    /// </summary>
    public class Column3DFMRI : Column3D
    {
        #region Properties
        public override ColumnType Type
        {
            get
            {
                return ColumnType.FMRI;
            }
        }
        // textures
        public List<Texture2D> BrainCutWithFMRITextures = null;        
        public List<Texture2D> GUIBrainCutWithFMRITextures = null;

        public List<DLL.Texture> DLLBrainCutWithFMRITextures = null;
        public List<DLL.Texture> DLLGUIBrainCutWithFMRITextures = null;

        public float CalMin = 0.4f;
        public float CalMax = 0.6f;
        public float Alpha = 0.5f;
        #endregion

        #region Public Methods
        /// <summary>
        /// Init the IRMF column
        /// </summary>
        /// <param name="idColumn"></param>
        /// <param name="nbCuts"></param>
        /// <param name="plots"></param>
        /// <param name="plotsGO"></param>
        public override void Initialize(int idColumn, int nbCuts,  DLL.PatientElectrodesList plots, List<GameObject> PlotsPatientParent)
        {
            base.Initialize(idColumn, nbCuts, plots, PlotsPatientParent);

            // GO textures            
            BrainCutWithFMRITextures = new List<Texture2D>();
            GUIBrainCutWithFMRITextures = new List<Texture2D>();
            for (int ii = 0; ii < nbCuts; ++ii)
            {
                BrainCutWithFMRITextures.Add(Texture2Dutility.GenerateCut(1, 1));
                GUIBrainCutWithFMRITextures.Add(Texture2Dutility.GenerateGUI(1, 1));
            }
            
            // DLL textures
            DLLBrainCutWithFMRITextures = new List<DLL.Texture>(nbCuts);
            DLLGUIBrainCutWithFMRITextures = new List<DLL.Texture>(nbCuts);
            for (int jj = 0; jj < nbCuts; ++jj)
            {
                DLLBrainCutWithFMRITextures.Add(new DLL.Texture());
                DLLGUIBrainCutWithFMRITextures.Add(new DLL.Texture());
            }
        }
        /// <summary>
        /// Update the cut planes number of the 3D column view
        /// </summary>
        /// <param name="newCutsNb"></param>
        public new void UpdateCutsPlanesNumber(int diffCuts)
        {
            base.UpdateCutsPlanesNumber(diffCuts);

            // update number of cuts
            if (diffCuts < 0)
            {
                for (int ii = 0; ii < -diffCuts; ++ii)
                {
                    // GO textures 
                    BrainCutWithFMRITextures.Add(Texture2Dutility.GenerateCut());
                    GUIBrainCutWithFMRITextures.Add(Texture2Dutility.GenerateGUI());

                    // DLL textures
                    DLLBrainCutWithFMRITextures.Add(new DLL.Texture());
                    DLLGUIBrainCutWithFMRITextures.Add(new DLL.Texture());
                }
            }
            else if (diffCuts > 0)
            {                
                for (int ii = 0; ii < diffCuts; ++ii)
                {
                    // GO textures       
                    Destroy(BrainCutWithFMRITextures[BrainCutWithFMRITextures.Count - 1]);
                    BrainCutWithFMRITextures.RemoveAt(BrainCutWithFMRITextures.Count - 1);

                    Destroy(GUIBrainCutWithFMRITextures[GUIBrainCutWithFMRITextures.Count - 1]);
                    GUIBrainCutWithFMRITextures.RemoveAt(GUIBrainCutWithFMRITextures.Count - 1);

                    // DLL textures
                    DLLBrainCutWithFMRITextures.RemoveAt(DLLBrainCutWithFMRITextures.Count - 1);
                    DLLGUIBrainCutWithFMRITextures.RemoveAt(DLLGUIBrainCutWithFMRITextures.Count - 1);
                }
            }
        }
        /// <summary>
        ///  Clean all dll data and unity textures
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            // plots
            m_RawElectrodes.Dispose();

            // textures 2D
            for (int ii = 0; ii < BrainCutWithFMRITextures.Count; ++ii)
            {
                Destroy(BrainCutWithFMRITextures[ii]);
                Destroy(GUIBrainCutWithFMRITextures[ii]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void UpdateSitesVisibility(SceneStatesInfo data)
        {
            Vector3 normalScale = new Vector3(1, 1, 1);
            MeshRenderer renderer = null;
            SiteType siteType;

            for (int ii = 0; ii < Sites.Count; ++ii)
            {
                bool activity = true;
                bool highlight = Sites[ii].Information.IsHighlighted;
                renderer = Sites[ii].GetComponent<MeshRenderer>();

                if (Sites[ii].Information.IsMasked) // column mask : plot is not visible can't be clicked
                {
                    activity = false;
                    siteType = Sites[ii].Information.IsMarked ? SiteType.Marked : SiteType.Normal;
                }
                else if (Sites[ii].Information.IsInROI) // ROI mask : plot is not visible, can't be clicked
                {
                    activity = false;
                    siteType = Sites[ii].Information.IsMarked ? SiteType.Marked : SiteType.Normal;
                }
                else
                {
                    if (Sites[ii].Information.IsBlackListed) // blacklist mask : plot is barely visible with another color, can be clicked
                    {
                        Sites[ii].transform.localScale = normalScale;
                        siteType = SiteType.BlackListed;
                    }
                    else if(Sites[ii].Information.IsExcluded)
                    {
                        Sites[ii].transform.localScale = normalScale;
                        siteType = SiteType.Excluded;
                    }
                    else // no mask : all plots have the same size and color
                    {
                        Sites[ii].transform.localScale = normalScale;
                        siteType = Sites[ii].Information.IsMarked ? SiteType.Marked : SiteType.Normal;
                    }

                    // select site ring 
                    if (ii == SelectedSiteID)
                        m_SelectRing.SetSelectedSite(Sites[ii], Sites[ii].transform.localScale);

                    renderer.sharedMaterial = SharedMaterials.SiteSharedMaterial(highlight, siteType);
                }

                Sites[ii].gameObject.SetActive(activity);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexCut"></param>
        /// <param name="orientation"></param>
        /// <param name="flip"></param>
        /// <param name="cutPlanes"></param>
        /// <param name="drawLines"></param>
        public void CreateGUIFMRITexture(int indexCut, string orientation, bool flip, List<Cut> cutPlanes, bool drawLines)
        {
            if (DLLBrainCutTextures[indexCut].m_TextureSize[0] > 0)
            {
                DLLGUIBrainCutWithFMRITextures[indexCut].CopyAndRotate(DLLBrainCutWithFMRITextures[indexCut], orientation, flip, drawLines, indexCut, cutPlanes, DLLMRITextureCutGenerators[indexCut]);
                DLLGUIBrainCutWithFMRITextures[indexCut].UpdateTexture2D(GUIBrainCutWithFMRITextures[indexCut], false); // TODO: ;..
            }
        }
        #endregion
    }
}