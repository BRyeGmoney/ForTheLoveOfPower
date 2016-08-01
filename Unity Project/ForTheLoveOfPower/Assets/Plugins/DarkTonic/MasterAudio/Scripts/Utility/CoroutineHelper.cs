/*! \cond PRIVATE */
using UnityEngine;
using System.Collections;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    // ReSharper disable once CheckNamespace
    public static class CoroutineHelper {
        public static IEnumerator WaitForActualSeconds(float time) {
            var start = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup < start + time) {
                yield return MasterAudio.EndOfFrameDelay;
            }
        }
    }
}
/*! \endcond */