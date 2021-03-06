﻿using HBP.Module3D;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI.Module3D
{
    /// <summary>
    /// Overlay element to display the used colormap and the corresponding minimum, middle and maximum values
    /// </summary>
    public class Colormap : ColumnOverlayElement
    {
        #region Properties
        /// <summary>
        /// Image containing the colormap sprite
        /// </summary>
        [SerializeField] private Image m_ColormapImage;

        /// <summary>
        /// Displays the minimum value for the colormap
        /// </summary>
        [SerializeField] private Text m_Min;
        /// <summary>
        /// Displays the middle value for the colormap
        /// </summary>
        [SerializeField] private Text m_Mid;
        /// <summary>
        /// Displays the maximum value for the colormap
        /// </summary>
        [SerializeField] private Text m_Max;

        /// <summary>
        /// Links between the type of a color and its sprite
        /// </summary>
        private Dictionary<Data.Enums.ColorType, Sprite> m_SpriteByColorType = new Dictionary<Data.Enums.ColorType, Sprite>();
        #endregion

        #region Private Methods
        private void Awake()
        {
            foreach (var colormap in System.Enum.GetValues(typeof(Data.Enums.ColorType)).Cast<Data.Enums.ColorType>())
            {
                m_SpriteByColorType.Add(colormap, Resources.Load<Sprite>(System.IO.Path.Combine("Colormaps", string.Format("colormap_{0}", ((int)colormap).ToString()))) as Sprite);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Setup the overlay element
        /// </summary>
        /// <param name="scene">Associated 3D scene</param>
        /// <param name="column">Associated 3D column</param>
        /// <param name="columnUI">Parent UI column</param>
        public override void Setup(Base3DScene scene, Column3D column, Column3DUI columnUI)
        {
            base.Setup(scene, column, columnUI);
            IsActive = false;

            scene.OnUpdateGeneratorState.AddListener((value) =>
            {
                if (column is Column3DDynamic)
                {
                    IsActive = value;
                }
            });

            scene.OnChangeColormap.AddListener((color) => m_ColormapImage.sprite = m_SpriteByColorType[color]);

            if (column is Column3DDynamic dynamicColumn)
            {
                dynamicColumn.DynamicParameters.OnUpdateSpanValues.AddListener(() =>
                {
                    m_Min.text = dynamicColumn.DynamicParameters.SpanMin.ToString("0.00");
                    m_Mid.text = dynamicColumn.DynamicParameters.Middle.ToString("0.00");
                    m_Max.text = dynamicColumn.DynamicParameters.SpanMax.ToString("0.00");
                });
            }
        }
        #endregion
    }
}