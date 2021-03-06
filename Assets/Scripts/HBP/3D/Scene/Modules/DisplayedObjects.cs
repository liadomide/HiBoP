﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HBP.Module3D
{
    /// <summary>
    /// List of all the objects displayed in the scene
    /// </summary>
    public class DisplayedObjects : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Parent scene
        /// </summary>
        [SerializeField] private Base3DScene m_Scene;

        [SerializeField]  private Transform m_BrainSurfaceMeshesParent;
        /// <summary>
        /// Parent of surface meshes
        /// </summary>
        public Transform BrainSurfaceMeshesParent
        {
            get
            {
                return m_BrainSurfaceMeshesParent;
            }
            set
            {
                m_BrainSurfaceMeshesParent = value;
            }
        }

        /// <summary>
        /// Parent of the invisible surface meshes
        /// </summary>
        [SerializeField] private Transform m_InvisibleBrainMeshesParent;
        /// <summary>
        /// Parent of the cut meshes
        /// </summary>
        [SerializeField] private Transform m_BrainCutMeshesParent;
        /// <summary>
        /// Parent of the sites
        /// </summary>
        [SerializeField] private Transform m_SitesMeshesParent;
        /// <summary>
        /// List of every patient parents for the sites
        /// </summary>
        public List<GameObject> SitesPatientParent { get; private set; }

        /// <summary>
        /// Meshes of the brain surface
        /// </summary>
        public List<GameObject> BrainSurfaceMeshes { get; private set; } = new List<GameObject>();
        /// <summary>
        /// Meshes of the cuts
        /// </summary>
        public List<GameObject> BrainCutMeshes { get; private set; } = new List<GameObject>();
        /// <summary>
        /// Meshes of the invisible surface
        /// </summary>
        public List<GameObject> InvisibleBrainSurfaceMeshes { get; private set; } = new List<GameObject>();
        /// <summary>
        /// Simplified brain
        /// </summary>
        public GameObject SimplifiedBrain { get; private set; }

        /// <summary>
        /// Parent for ROI GameObjects
        /// </summary>
        [SerializeField] protected Transform m_ROIParent;

        /// <summary>
        /// Prefab for the 3D brain mesh part
        /// </summary>
        [SerializeField] private GameObject m_BrainPrefab;
        /// <summary>
        /// Prefab for the 3D simplified brain mesh
        /// </summary>
        [SerializeField] private GameObject m_SimplifiedBrainPrefab;
        /// <summary>
        /// Prefab for the 3D invisible brain mesh
        /// </summary>
        [SerializeField] private GameObject m_InvisibleBrainPrefab;
        /// <summary>
        /// Prefab for the 3D cut
        /// </summary>
        [SerializeField] private GameObject m_CutPrefab;
        /// <summary>
        /// Prefab for the 3D site
        /// </summary>
        [SerializeField] private GameObject m_SitePrefab;
        /// <summary>
        /// Prefab for the ROI
        /// </summary>
        [SerializeField] private GameObject m_ROIPrefab;
        #endregion

        #region Private Methods
        private void Awake()
        {
            // Mark brain mesh as dynamic
            m_BrainPrefab.GetComponent<MeshFilter>().sharedMesh.MarkDynamic();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Instantiate the gameObjects responsible for the invisible brain
        /// </summary>
        /// <param name="visible">Is the mesh corresponding to the invisible brain visible at instantiation ?</param>
        public void InstantiateInvisibleMesh(bool visible)
        {
            // destroy previous GO
            if (InvisibleBrainSurfaceMeshes != null)
                for (int ii = 0; ii < InvisibleBrainSurfaceMeshes.Count; ++ii)
                    Destroy(InvisibleBrainSurfaceMeshes[ii]);

            // create new GO
            InvisibleBrainSurfaceMeshes = new List<GameObject>(BrainSurfaceMeshes.Count);
            for (int ii = 0; ii < BrainSurfaceMeshes.Count; ++ii)
            {
                GameObject invisibleBrainPart = Instantiate(m_InvisibleBrainPrefab, m_InvisibleBrainMeshesParent);
                invisibleBrainPart.name = "erased brain part " + ii;
                invisibleBrainPart.layer = LayerMask.NameToLayer(HBP3DModule.DEFAULT_MESHES_LAYER);
                invisibleBrainPart.transform.localScale = new Vector3(-1, 1, 1);
                invisibleBrainPart.transform.localPosition = new Vector3(0, 0, 0);
                invisibleBrainPart.SetActive(visible);
                InvisibleBrainSurfaceMeshes.Add(invisibleBrainPart);
            }
        }
        /// <summary>
        /// Instantiate all the gameObjects representing the sites on the scene
        /// </summary>
        /// <param name="implantation">Implantation to be instantiated</param>
        public void InstantiateImplantation(Implantation3D implantation)
        {
            foreach (Transform sitePatient in m_SitesMeshesParent)
            {
                Destroy(sitePatient.gameObject);
            }
            SitesPatientParent = new List<GameObject>();
            
            int siteIndex = 0;
            foreach (var patient in m_Scene.Visualization.Patients)
            {
                GameObject sitePatient = new GameObject(patient.ID);
                sitePatient.transform.SetParent(m_SitesMeshesParent);
                sitePatient.transform.localPosition = Vector3.zero;
                SitesPatientParent.Add(sitePatient);
                foreach (var siteInfo in implantation.GetSitesOfPatient(patient))
                {
                    GameObject siteGameObject = Instantiate(m_SitePrefab, sitePatient.transform);
                    siteGameObject.name = siteInfo.Name;

                    siteGameObject.transform.localPosition = siteInfo.UnityPosition;
                    siteGameObject.GetComponent<MeshFilter>().sharedMesh = SharedMeshes.Site;

                    siteGameObject.SetActive(true);
                    siteGameObject.layer = LayerMask.NameToLayer("Inactive");

                    Site site = siteGameObject.GetComponent<Site>();
                    site.Information.Patient = patient;
                    site.Information.Name = siteInfo.Name;
                    site.Information.Index = siteIndex++;
                    site.Information.SiteData = siteInfo.SiteData;
                    site.State.IsBlackListed = false;
                    site.State.IsHighlighted = false;
                    site.State.IsOutOfROI = true;
                    site.State.IsMasked = false;
                    site.State.Color = SiteState.DefaultColor;
                    site.IsActive = true;
                }
            }

            foreach (Column3D column in m_Scene.Columns)
            {
                column.UpdateSites(implantation, SitesPatientParent);
            }
            m_Scene.ROIManager.UpdateROIMasks();
        }
        /// <summary>
        /// Instantiate the splits for the brain mesh
        /// </summary>
        /// <param name="nbSplits">Number of splits to be instantiated</param>
        public void InstantiateSplits(int nbSplits)
        {
            for (int i = 0; i < BrainSurfaceMeshes.Count; i++)
            {
                Destroy(BrainSurfaceMeshes[i]);
            }
            if (SimplifiedBrain != null)
            {
                Destroy(SimplifiedBrain);
            }

            BrainSurfaceMeshes = new List<GameObject>(nbSplits);
            for (int i = 0; i < nbSplits; ++i)
            {
                BrainSurfaceMeshes.Add(Instantiate(m_BrainPrefab));
                BrainSurfaceMeshes[i].GetComponent<Renderer>().sharedMaterial = m_Scene.BrainMaterial;
                BrainSurfaceMeshes[i].name = "brain_" + i;
                BrainSurfaceMeshes[i].transform.parent = BrainSurfaceMeshesParent;
                BrainSurfaceMeshes[i].transform.localPosition = Vector3.zero;
                BrainSurfaceMeshes[i].layer = LayerMask.NameToLayer(HBP3DModule.HIDDEN_MESHES_LAYER);
                BrainSurfaceMeshes[i].SetActive(true);
            }
            SimplifiedBrain = Instantiate(m_SimplifiedBrainPrefab);
            SimplifiedBrain.GetComponent<Renderer>().sharedMaterial = m_Scene.SimplifiedBrainMaterial;
            SimplifiedBrain.transform.name = "brain_simplified";
            SimplifiedBrain.transform.parent = BrainSurfaceMeshesParent;
            SimplifiedBrain.transform.localPosition = Vector3.zero;
            SimplifiedBrain.layer = LayerMask.NameToLayer(HBP3DModule.HIDDEN_MESHES_LAYER);
            SimplifiedBrain.SetActive(true);
        }
        /// <summary>
        /// Instantiate the gameObject for a new cut in the scene
        /// </summary>
        public void InstantiateCut()
        {
            GameObject cutGameObject = Instantiate(m_CutPrefab);
            cutGameObject.GetComponent<Renderer>().sharedMaterial = m_Scene.CutMaterial;
            cutGameObject.name = "Cut";
            cutGameObject.transform.parent = m_BrainCutMeshesParent.transform;
            cutGameObject.AddComponent<MeshCollider>();
            cutGameObject.layer = LayerMask.NameToLayer(HBP3DModule.DEFAULT_MESHES_LAYER);
            cutGameObject.transform.localPosition = Vector3.zero;
            BrainCutMeshes.Add(cutGameObject);
            BrainCutMeshes.Last().layer = LayerMask.NameToLayer(HBP3DModule.DEFAULT_MESHES_LAYER);
        }
        /// <summary>
        /// Instantiate a ROI on the scene
        /// </summary>
        /// <returns>The ROI object that has been instantiated</returns>
        public ROI InstantiateROI()
        {
            return Instantiate(m_ROIPrefab, m_ROIParent).GetComponent<ROI>();
        }
        #endregion
    }
}