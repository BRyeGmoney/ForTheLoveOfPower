using System;
using System.Collections.Generic;

#if UNITY_5
    using UnityEngine.Audio;
#endif

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    /// <summary>
    /// This class is used to control 1 or more Sound Groups at once, for muting, volume, and other purposes. Sound Groups using the Bug are routed through it, and Bus output can be assigned to a Unity Mixer Group.
    /// </summary>
    [Serializable]
    // ReSharper disable once CheckNamespace
    public class GroupBus {
        /*! \cond PRIVATE */
        // ReSharper disable InconsistentNaming
        public string busName;
        public float volume = 1.0f;
        public bool isSoloed = false;
        public bool isMuted = false;
        public int voiceLimit = -1;
        public bool stopOldest = false;
        public bool isExisting = false; // for Dynamic Sound Group - referenced Buses
        public bool isUsingOcclusion = false;

#if UNITY_5
        public AudioMixerGroup mixerChannel = null;
        public bool forceTo2D = false;
#else
		public bool forceTo2D = false;
#endif
        // ReSharper restore InconsistentNaming
        private readonly List<int> _activeAudioSourcesIds = new List<int>(50);
        private float _originalVolume = 1;

        public void AddActiveAudioSourceId(int id) {
            if (_activeAudioSourcesIds.Contains(id)) {
                return;
            }

            _activeAudioSourcesIds.Add(id);
        }

        public void RemoveActiveAudioSourceId(int id) {
            _activeAudioSourcesIds.Remove(id);
        }
        /*! \endcond */

        /// <summary>
        /// This property returns the number of active voices playing through the bus
        /// </summary>
        public int ActiveVoices {
            get { return _activeAudioSourcesIds.Count; }
        }

        /// <summary>
        /// This property returns whether or not the bus Active Voice limit has been reached
        /// </summary>
        public bool BusVoiceLimitReached {
            get {
                if (voiceLimit <= 0) {
                    return false; // no limit set
                }

                return _activeAudioSourcesIds.Count >= voiceLimit;
            }
        }

        /// <summary>
        /// This property will return the original volume of the Sound Group.
        /// </summary>
        public float OriginalVolume {
            get {
                // ReSharper disable once PossibleInvalidOperationException
                return _originalVolume;
            }
            set {
                _originalVolume = value;
            }
        }
    }
}
