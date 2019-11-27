﻿using System.Linq;
using Tools.Unity.Components;
using d = HBP.Data.Experience.Protocol;
using UnityEngine;

namespace HBP.UI.Experience.Protocol
{
    public class EventListGestion : ListGestion<d.Event>
    {
        #region Properties
        [SerializeField] protected EventList m_List;
        public override Tools.Unity.Lists.ActionableList<d.Event> List => m_List;

        [SerializeField] protected EventCreator m_ObjectCreator;
        public override ObjectCreator<d.Event> ObjectCreator => m_ObjectCreator;
        #endregion

        #region Protected Methods
        protected override void Initialize()
        {
            base.Initialize();
            List.OnRemoveObject.AddListener(OnRemoveObject);
        }
        protected void OnRemoveObject(d.Event obj)
        {
            if (!List.Objects.Any((e) => e.Type == Data.Enums.MainSecondaryEnum.Main))
            {
                d.Event firstEvent = List.Objects.FirstOrDefault();
                if (firstEvent != null) firstEvent.Type = Data.Enums.MainSecondaryEnum.Main;
                List.UpdateObject(firstEvent);
            }
        }
        protected override void OnSaveModifier(d.Event obj)
        {
            if(obj.Type == Data.Enums.MainSecondaryEnum.Main)
            {
                foreach (var item in List.Objects.Where(e => e != obj))
                {
                    item.Type = Data.Enums.MainSecondaryEnum.Secondary;
                }
            }
            base.OnSaveModifier(obj);
        }
        #endregion
    }
}
