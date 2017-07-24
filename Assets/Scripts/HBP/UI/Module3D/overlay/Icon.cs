﻿using HBP.Module3D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HBP.UI.Module3D
{
    public class Icon : OverlayElement
    {
        #region Properties
        [SerializeField]
        private Image m_Image;
        [SerializeField]
        private Text m_Text;

        private Data.Visualization.Icon m_CurrentIcon;
        #endregion

        #region Public Methods
        public void Initialize(Base3DScene scene, Column3D column, Column3DUI columnUI)
        {
            m_ColumnUI = columnUI;

            scene.SceneInformation.OnUpdateGeneratorState.AddListener((value) =>
            {
                IsActive = value;
            });

            if (column is Column3DFMRI)
            {
                IsActive = false;
            }
            else
            {
                Column3DIEEG col = (Column3DIEEG)column;
                col.OnUpdateCurrentTimelineID.AddListener(() =>
                {
                    if (!scene.SceneInformation.IsGeneratorUpToDate) return;

                    List<Data.Visualization.Icon> icons = col.ColumnData.IconicScenario.Icons.OrderByDescending((i) => i.StartPosition).ToList();
                    Data.Visualization.Icon icon = icons.DefaultIfEmpty(null).FirstOrDefault((i) => i.StartPosition <= col.CurrentTimeLineID && i.EndPosition >= col.CurrentTimeLineID);
                    if (icon != m_CurrentIcon)
                    {
                        if (icon == null)
                        {
                            IsActive = false;
                        }
                        else
                        {
                            IsActive = true;
                            Texture2D iconTexture = Texture2Dutility.GenerateIcon();
                            HBP.Module3D.DLL.Texture.Load(icon.IllustrationPath).UpdateTexture2D(iconTexture);
                            m_Image.sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
                            m_Text.text = icon.Label;
                        }
                    }
                });
            }
        }
        #endregion
    }
}