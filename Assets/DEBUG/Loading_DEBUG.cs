﻿using HBP.Data.Experience.Dataset;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using HBP.Data.TrialMatrix.Grid;

public class Loading_DEBUG : MonoBehaviour
{
    public Texture2D colorMap;
    public HBP.UI.TrialMatrix.Grid.TrialMatrixGrid trialMatrixGrid;
    public HBP.UI.TrialMatrix.TrialMatrix trialMatrix;

    public void Load()
    {
        //Dataset[] datasets = ApplicationState.ProjectLoaded.Datasets.ToArray();
        //List<ChannelStruct> channels = new List<ChannelStruct>();
        //List<DataStruct> datas = new List<DataStruct>();
        //foreach (var dataset in datasets)
        //{
        //    foreach (var dataInfo in dataset.Data)
        //    {
        //        DataStruct data = new DataStruct(dataset, dataInfo.Name);
        //        if (!datas.Contains(data)) datas.Add(data);
        //        Elan.ElanFile eeg = new Elan.ElanFile(dataInfo.EEG, false);
        //        channels.AddRange(eeg.Channels.Select(c => new ChannelStruct(c.Label, dataInfo.Patient)).Where(i => !channels.Contains(i)));
        //    }
        //}
        //channels = channels.Take(4).ToList();
        //trialMatrixGrid.Display(channels.ToArray(), datas.ToArray());

        DataInfo dataInfo = ApplicationState.ProjectLoaded.Datasets.First(d => d.Name == "VISU").Data.First();
        Elan.ElanFile elanFile = new Elan.ElanFile(dataInfo.EEG, false);
        string channel = elanFile.Channels.First().Label;
        HBP.Data.TrialMatrix.TrialMatrix trialMatrixData = new HBP.Data.TrialMatrix.TrialMatrix(dataInfo, channel);
        trialMatrix.Set(trialMatrixData, colorMap);
    }
}
