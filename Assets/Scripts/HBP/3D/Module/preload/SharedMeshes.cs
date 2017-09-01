﻿
/* \file SharedMeshes.cs
 * \author Lance Florian
 * \date    22/04/2016
 * \brief Define SharedMeshes
 */

// unity
using UnityEngine;

namespace HBP.Module3D
{
    /// <summary>
    /// Shared procedural meshes used at runtime
    /// </summary>
    public class SharedMeshes : MonoBehaviour
    {
        #region Properties
        static public Mesh ROISphere = null;
        static public Mesh Site = null;
        static public Mesh HighlightedSite = null;
        static public Mesh SiteSelection = null;

        static public Mesh SiteLOD0 = null;
        static public Mesh SiteLOD1 = null;
        static public Mesh SiteLOD2 = null;
        #endregion

        #region Private Methods
        void Awake()
        {
            ROISphere = Geometry.CreateSphereMesh(1f, 48, 32);
            Site = Geometry.CreateSphereMesh(1, 12, 8);
            HighlightedSite = Geometry.CreateTube();// (3f);
            SiteSelection = Geometry.CreateTube();// (3f);

            // TODO: use level of details for sites
            SiteLOD0 = Geometry.CreateSphereMesh(1, 16, 12);
            SiteLOD1 = Geometry.CreateSphereMesh(1, 12, 8);
            SiteLOD2 = Geometry.CreateSphereMesh(1, 4, 3);
        }
        #endregion
    }
}