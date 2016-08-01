/*! \cond PRIVATE */
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    // ReSharper disable once CheckNamespace
    public static class DTMonoHelper {
        /// <summary>
        /// This is a cross-Unity-version method to tell you if a GameObject is active in the Scene.
        /// </summary>
        /// <param name="go">The GameObject you're asking about.</param>
        /// <returns>True or false</returns>
        public static bool IsActive(GameObject go) {
            return go.activeInHierarchy;
        }

        /// <summary>
        /// This is a cross-Unity-version method to set a GameObject to active in the Scene.
        /// </summary>
        /// <param name="go">The GameObject you're setting to active or inactive</param>
        /// <param name="isActive">True to set the object to active, false to set it to inactive.</param>
        public static void SetActive(GameObject go, bool isActive) {
            go.SetActive(isActive);
        }

    }
}
/*! \endcond */