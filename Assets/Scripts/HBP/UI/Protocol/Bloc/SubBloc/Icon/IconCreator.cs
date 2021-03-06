﻿using HBP.Data.Experience.Protocol;
using System.Linq;
using Tools.Unity.Components;

namespace HBP.UI.Experience.Protocol
{
    public class IconCreator : ObjectCreator<Icon>
    {
        Tools.CSharp.Window m_Window;
        public Tools.CSharp.Window Window
        {
            get
            {
                return m_Window;
            }
            set
            {
                m_Window = value;
                foreach (var modifier in WindowsReferencer.Windows.OfType<TreatmentModifier>())
                {
                    modifier.Window = value;
                }
            }
        }

        public override void CreateFromScratch()
        {
            OpenModifier(new Icon("New Icon", "", Window));
        }

        protected override ObjectModifier<Icon> OpenModifier(Icon item)
        {
            IconModifier modifier = base.OpenModifier(item) as IconModifier;
            modifier.Window = Window;
            return modifier;
        }
    }
}