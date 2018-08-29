﻿using System.ComponentModel;

namespace HBP.Data.Enums
{
    public enum AveragingType { Mean, Median }
    public enum NormalizationType { None, Trial, Bloc, Protocol }
    public enum SiteInfluenceByDistanceType { Constant, Linear, Quadratic }
    public enum BlocFormatType {[Description("Trial height")] TrialHeight, [Description("Trial ratio")] TrialRatio, [Description("Bloc ratio")] BlocRatio }
    public enum DisplayableError { LeftMeshEmpty, RightMeshEmpty, PreimplantationMRIEmpty, ImplantationEmpty }
    public enum SceneType { SinglePatient, MultiPatients };
    public enum CameraType { EEG, fMRI };
    public enum ColorType // For now, integers matter because of the link with the dll.
    {
        Grayscale = 0,
        Hot = 1,
        Winter = 2,
        Warm = 3,
        Surface = 4,
        Cool = 5,
        RedYellow = 6,
        BlueGreen = 7,
        ACTC = 8,
        Bone = 9,
        GEColor = 10,
        Gold = 11,
        XRain = 12,
        MatLab = 13,
        Default = 14,
        BrainColor = 15,
        White = 16,
        SoftGrayscale = 17
    }
    public enum SiteFilter
    {
        Site = 0,
        Electrode = 1,
        Patient = 2,
        Highlighted = 3,
        Unhighlighted = 4,
        All = 5,
        InRegionOfInterest = 6,
        OutOfRegionOfInterest = 7,
        Name = 8,
        MarsAtlas = 9,
        Broadman = 10,
        Suspicious = 11,
        Unsuspicious = 12
    }
    public enum SiteAction
    {
        Include = 0,
        Exclude = 1,
        Blacklist = 2,
        Unblacklist = 3,
        Highlight = 4,
        Unhighlight = 5,
        Mark = 6,
        Unmark = 7,
        Suspect = 8,
        Unsuspect = 9
    }
    public enum CutOrientation { Axial = 0, Coronal = 1, Sagital = 2, Custom = 3 } // Same as above
    public enum SiteInformationDisplayMode { Anatomy, IEEG, CCEP, Light }
}