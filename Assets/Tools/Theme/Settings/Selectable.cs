﻿using UnityEngine;
using UnityEngine.UI;

namespace NewTheme
{
    [CreateAssetMenu(menuName = "Theme/Settings/Selectable")]
    public class Selectable : Settings
    {
        #region Properties
        public UnityEngine.UI.Selectable.Transition Transition;
        public UnityEngine.UI.ColorBlock Colors;
        public SpriteState SpriteState;
        public AnimationTriggers AnimationTriggers;
        #endregion

        #region Public Methods
        public override void Set(GameObject gameObject)
        {
            UnityEngine.UI.Selectable selectable = gameObject.GetComponent<UnityEngine.UI.Selectable>();
            if (selectable)
            {
                selectable.colors = Colors;
                selectable.spriteState = SpriteState;
                selectable.animationTriggers = AnimationTriggers;
            }
        }
        #endregion

    }
}