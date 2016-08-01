/*! \cond PRIVATE */
using System;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    [Serializable]
    // ReSharper disable once CheckNamespace
    public class DuckGroupInfo {
        // ReSharper disable InconsistentNaming
        public string soundType = MasterAudio.NoGroupName;
        public float riseVolStart = .5f;
        public float unduckTime = 1f;
        public float duckedVolumeCut = -6f;
        // ReSharper restore InconsistentNaming
    }
}
/*! \endcond */