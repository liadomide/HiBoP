﻿using System.Collections.ObjectModel;
using System.Collections.Generic;
using Tools.Unity.Components;
using d = HBP.Data.Experience.Protocol;
using UnityEngine;

namespace HBP.UI.Experience.Protocol
{
    public class ProtocolListGestion : ListGestion<d.Protocol>
    {
        #region Properties
        [SerializeField] List<d.Protocol> m_ModifiedProtocols = new List<d.Protocol>();
        public ReadOnlyCollection<d.Protocol> ModifiedProtocols
        {
            get
            {
                return new ReadOnlyCollection<d.Protocol>(m_ModifiedProtocols);
            }
        }

        [SerializeField]protected ProtocolList m_List;
        public override Tools.Unity.Lists.ActionableList<d.Protocol> List => m_List;

        [SerializeField] protected ProtocolCreator m_ObjectCreator;
        public override ObjectCreator<d.Protocol> ObjectCreator => m_ObjectCreator;
        #endregion

        #region Public Methods
        protected override void OnSaveModifier(d.Protocol obj)
        {
            m_ModifiedProtocols.Add(obj);
            base.OnSaveModifier(obj);
        }
        protected override void OnObjectCreated(d.Protocol obj)
        {
            m_ModifiedProtocols.Add(obj);
            base.OnObjectCreated(obj);
        }
        #endregion
    }
}