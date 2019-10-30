﻿
/* \file SharedMaterials.cs
 * \author Lance Florian
 * \date    22/04/2016
 * \brief Define SharedMaterials
 */

using HBP.Data.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace HBP.Module3D
{
    /// <summary>
    /// Shared materials used by GO at runtime
    /// </summary>
    public class SharedMaterials : MonoBehaviour
    {
        #region Struct
        public struct ROI
        {
            public static Material Normal = null;
            public static Material Selected = null;
        }

        public struct Ring
        {
            public static Material Selected = null;
        }

        public struct Site
        {
            private static Dictionary<Color, Material> m_MaterialByColor = new Dictionary<Color, Material>();
            public static Material GetMaterial(Color baseColor, bool highlighted)
            {
                Color color = new Color(baseColor.r, baseColor.g, baseColor.b, highlighted ? 1 : 0.5f);
                if (!m_MaterialByColor.TryGetValue(color, out Material material))
                {
                    material = Instantiate(Basic) as Material;
                    material.color = color;
                    m_MaterialByColor.Add(color, material);
                }
                return material;
            }

            public static Material Basic = null;

            public static Material Negative = null;
            public static Material Positive = null;
            public static Material BlackListed = null;
            
            public static Material NegativeHighlighted = null;
            public static Material PositiveHighlighted = null;
            public static Material BlackListedHighlighted = null;

            public static Material Source = null;
            public static Material SourceHighlighted = null;
            public static Material NotASource = null;
            public static Material NotASourceHighlighted = null;
        }
        #endregion

        #region Private Methods
        void Awake()
        {
            // ROI
            ROI.Normal = Instantiate(Resources.Load("Materials/ROI/ROI", typeof(Material))) as Material;
            ROI.Selected = Instantiate(Resources.Load("Materials/ROI/ROISelected", typeof(Material))) as Material;

            // Site
            Site.BlackListed = Instantiate(Resources.Load("Materials/Sites/Blacklisted", typeof(Material))) as Material;
            Site.Negative = Instantiate(Resources.Load("Materials/Sites/Negative", typeof(Material))) as Material;
            Site.Basic = Instantiate(Resources.Load("Materials/Sites/Basic", typeof(Material))) as Material;
            Site.Positive = Instantiate(Resources.Load("Materials/Sites/Positive", typeof(Material))) as Material;
            Site.BlackListedHighlighted = Instantiate(Resources.Load("Materials/Sites/BlacklistedHighlighted", typeof(Material))) as Material;
            Site.NegativeHighlighted = Instantiate(Resources.Load("Materials/Sites/NegativeHighlighted", typeof(Material))) as Material;
            Site.PositiveHighlighted = Instantiate(Resources.Load("Materials/Sites/PositiveHighlighted", typeof(Material))) as Material;
            Site.Source = Instantiate(Resources.Load("Materials/Sites/Source", typeof(Material))) as Material;
            Site.SourceHighlighted = Instantiate(Resources.Load("Materials/Sites/SourceHighlighted", typeof(Material))) as Material;
            Site.NotASource = Instantiate(Resources.Load("Materials/Sites/NotASource", typeof(Material))) as Material;
            Site.NotASourceHighlighted = Instantiate(Resources.Load("Materials/Sites/NotASourceHighlighted", typeof(Material))) as Material;

            // Ring
            Ring.Selected = Instantiate(Resources.Load("Materials/Rings/Selected", typeof(Material))) as Material;
        }
        #endregion

        #region Public Methods
        public static Material SiteSharedMaterial(bool highlighted, SiteType siteType, Color baseColor)
        {
            switch (siteType)
            {
                case SiteType.Positive:
                    return highlighted ? Site.PositiveHighlighted : Site.Positive;
                case SiteType.Negative:
                    return highlighted ? Site.NegativeHighlighted : Site.Negative;
                case SiteType.Source:
                    return highlighted ? Site.SourceHighlighted : Site.Source;
                case SiteType.NotASource:
                    return highlighted ? Site.NotASourceHighlighted : Site.NotASource;
                case SiteType.BlackListed:
                    return highlighted ? Site.BlackListedHighlighted : Site.BlackListed;
                case SiteType.Normal:
                default:
                    return Site.GetMaterial(baseColor, highlighted);
            }
        }
        #endregion
    }
}