﻿using System.Collections.Generic;
using Tools.Unity.Components;
using UnityEngine;

namespace HBP.UI.Anatomy
{
    public class ConnectivityListGestion : ListGestion<Data.Anatomy.Connectivity>
    {
        #region Properties
        [SerializeField] new ConnectivityList List;
        public override List<Data.Anatomy.Connectivity> Items
        {
            get
            {
                return base.Items;
            }

            set
            {
                List.Initialize();
                base.Items = value;
                List.SortByName(ConnectivityList.Sorting.Descending);
            }
        }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            base.List = List;
            base.Initialize();
        }
        #endregion
    }
}