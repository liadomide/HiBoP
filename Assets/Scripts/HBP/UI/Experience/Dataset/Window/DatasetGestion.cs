﻿using System.Linq;
using d = HBP.Data.Experience.Dataset;

namespace HBP.UI.Experience.Dataset
{
	public class DatasetGestion : ItemGestion<d.Dataset>
    {
		#region Public Methods
		public override void Save()
		{
            ApplicationState.ProjectLoaded.SetDatasets(Items.ToArray());
            base.Save();
        }
        #endregion

        #region Private Methods
        protected override void SetWindow()
		{
            list = transform.FindChild("Content").FindChild("List").FindChild("List").FindChild("Viewport").FindChild("Content").GetComponent<DatasetList>();
            (list as DatasetList).ActionEvent.AddListener((dataset, i) => OpenModifier(dataset,true));
            AddItem(ApplicationState.ProjectLoaded.Datasets.ToArray());
        }
        #endregion
    }
}
