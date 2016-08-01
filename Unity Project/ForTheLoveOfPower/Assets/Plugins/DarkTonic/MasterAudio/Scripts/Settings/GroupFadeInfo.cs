/*! \cond PRIVATE */
using System;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    [Serializable]
    // ReSharper disable once CheckNamespace
    public class GroupFadeInfo {
        public MasterAudioGroup ActingGroup;
        public string NameOfGroup;
        public float TargetVolume;
        public float VolumeStep;
        public bool IsActive = true;
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once RedundantNameQualifier
        public System.Action completionAction;
    }
}
/*! \endcond */