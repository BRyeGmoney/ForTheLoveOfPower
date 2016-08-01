/*! \cond PRIVATE */
using System;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    [Serializable]
    // ReSharper disable once CheckNamespace
    public class BusFadeInfo {
        public string NameOfBus;
        public GroupBus ActingBus;
        public float TargetVolume;
        public float VolumeStep;
        public bool IsActive = true;
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once RedundantNameQualifier
        public System.Action completionAction;
    }
}
/*! \endcond */