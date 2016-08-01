using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    /// <summary>
    /// This class is used to configure and create temporary per-Scene Sound Groups and Buses
    /// </summary>
    // ReSharper disable once CheckNamespace
    public class DynamicSoundGroupCreator : MonoBehaviour {
        /*! \cond PRIVATE */
        public const int ExtraHardCodedBusOptions = 1;

        // ReSharper disable InconsistentNaming
        public SystemLanguage previewLanguage = SystemLanguage.English;
        public MasterAudio.DragGroupMode curDragGroupMode = MasterAudio.DragGroupMode.OneGroupPerClip;
        public GameObject groupTemplate;
        public GameObject variationTemplate;
        public bool errorOnDuplicates = false;
        public bool createOnAwake = true;
        public bool soundGroupsAreExpanded = true;
        public bool removeGroupsOnSceneChange = true;
        public CreateItemsWhen reUseMode = CreateItemsWhen.FirstEnableOnly;
        public bool showCustomEvents = true;
        public MasterAudio.AudioLocation bulkVariationMode = MasterAudio.AudioLocation.Clip;
        public List<CustomEvent> customEventsToCreate = new List<CustomEvent>();
        public string newEventName = "my event";
        public bool showMusicDucking = true;
        public List<DuckGroupInfo> musicDuckingSounds = new List<DuckGroupInfo>();
        public List<GroupBus> groupBuses = new List<GroupBus>();
        public bool playListExpanded = false;
        public bool playlistEditorExp = true;
        public List<MasterAudio.Playlist> musicPlaylists = new List<MasterAudio.Playlist>();
        public List<GameObject> audioSourceTemplates = new List<GameObject>(10);
        public string audioSourceTemplateName = "Max Distance 500";
        public bool groupByBus = false;

        public bool itemsCreatedEventExpanded = false;
        public string itemsCreatedCustomEvent = string.Empty;

        public bool showUnityMixerGroupAssignment = true;
        // ReSharper restore InconsistentNaming

        private bool _hasCreated;
        private readonly List<Transform> _groupsToRemove = new List<Transform>();
        private Transform _trans;

        public enum CreateItemsWhen {
            FirstEnableOnly,
            EveryEnable
        }
        /*! \endcond */

        private readonly List<DynamicSoundGroup> _groupsToCreate = new List<DynamicSoundGroup>();

        // ReSharper disable once UnusedMember.Local
        private void Awake() {
            _trans = transform;
            _hasCreated = false;
            var aud = GetComponent<AudioSource>();
            if (aud != null) {
                Destroy(aud);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable() {
            CreateItemsIfReady(); // create in Enable event if it's all ready
        }

        // ReSharper disable once UnusedMember.Local
        private void Start() {
            CreateItemsIfReady(); // if it wasn't ready in Enable, create everything in Start
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable() {
            if (MasterAudio.AppIsShuttingDown) {
                return;
            }

            // scene changing
            if (!removeGroupsOnSceneChange) {
                // nothing to do.
                return;
            }

            if (MasterAudio.SafeInstance != null) {
                RemoveItems();
            }
        }

        private void CreateItemsIfReady() {
            if (MasterAudio.SafeInstance == null) { 
				return;
			}

			if (createOnAwake && MasterAudio.SoundsReady && !_hasCreated) {
                CreateItems();
            }
        }

        /// <summary>
        /// This method will remove the Sound Groups, Variations, buses, ducking triggers and Playlist objects specified in the Dynamic Sound Group Creator's Inspector. It is called automatically if you check the "Auto-remove Items" checkbox, otherwise you will need to call this method manually.
        /// </summary>
        public void RemoveItems() {
            // delete any buses we created too
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < groupBuses.Count; i++) {
                var aBus = groupBuses[i];

                if (aBus.isExisting) {
                    continue; // don't delete!
                }

                MasterAudio.DeleteBusByName(aBus.busName);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _groupsToRemove.Count; i++) {
                MasterAudio.DeleteSoundGroup(_groupsToRemove[i].name);
            }
            _groupsToRemove.Clear();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < customEventsToCreate.Count; i++) {
                var anEvent = customEventsToCreate[i];
                MasterAudio.DeleteCustomEvent(anEvent.EventName);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < musicPlaylists.Count; i++) {
                var aPlaylist = musicPlaylists[i];
                MasterAudio.DeletePlaylist(aPlaylist.playlistName);
            }

            if (reUseMode == CreateItemsWhen.EveryEnable) {
                _hasCreated = false;
            }
        }

        /// <summary>
        /// This method will create the Sound Groups, Variations, buses, ducking triggers and Playlist objects specified in the Dynamic Sound Group Creator's Inspector. It is called automatically if you check the "Auto-create Items" checkbox, otherwise you will need to call this method manually.
        /// </summary>
        public void CreateItems() {
            if (_hasCreated) {
                Debug.LogWarning("DynamicSoundGroupCreator '" + transform.name +
                                 "' has already created its items. Cannot create again.");
                return;
            }

            var ma = MasterAudio.Instance;
            if (ma == null) {
                return;
            }

            PopulateGroupData();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < groupBuses.Count; i++) {
                var aBus = groupBuses[i];

                if (aBus.isExisting) {
                    var confirmBus = MasterAudio.GrabBusByName(aBus.busName);
                    if (confirmBus == null) {
                        MasterAudio.LogWarning("Existing bus '" + aBus.busName +
                                               "' was not found, specified in prefab '" + name + "'.");
                    }
                    continue; // already exists.
                }

                if (!MasterAudio.CreateBus(aBus.busName, errorOnDuplicates)) {
                    continue;
                }

                var createdBus = MasterAudio.GrabBusByName(aBus.busName);
                if (createdBus == null) {
                    continue;
                }

                var busVol = PersistentAudioSettings.GetBusVolume(aBus.busName);
                if (!busVol.HasValue) {
                    createdBus.volume = aBus.volume;
                    createdBus.OriginalVolume = createdBus.volume;
                }
                createdBus.voiceLimit = aBus.voiceLimit;
                createdBus.stopOldest = aBus.stopOldest;
#if UNITY_5
                createdBus.forceTo2D = aBus.forceTo2D;
                createdBus.mixerChannel = aBus.mixerChannel;
#endif
                createdBus.isUsingOcclusion = aBus.isUsingOcclusion;
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _groupsToCreate.Count; i++) {
                var aGroup = _groupsToCreate[i];

                var busName = string.Empty;
                var selectedBusIndex = aGroup.busIndex == -1 ? 0 : aGroup.busIndex;
                if (selectedBusIndex >= HardCodedBusOptions) {
                    var selectedBus = groupBuses[selectedBusIndex - HardCodedBusOptions];
                    busName = selectedBus.busName;
                }
                aGroup.busName = busName;

                var groupTrans = MasterAudio.CreateSoundGroup(aGroup, _trans.name, errorOnDuplicates);

                // remove fx components
                // ReSharper disable ForCanBeConvertedToForeach
                for (var v = 0; v < aGroup.groupVariations.Count; v++) {
                    // ReSharper restore ForCanBeConvertedToForeach
                    var aVar = aGroup.groupVariations[v];
                    if (aVar.LowPassFilter != null) {
                        Destroy(aVar.LowPassFilter);
                    }
                    if (aVar.HighPassFilter != null) {
                        Destroy(aVar.HighPassFilter);
                    }
                    if (aVar.DistortionFilter != null) {
                        Destroy(aVar.DistortionFilter);
                    }
                    if (aVar.ChorusFilter != null) {
                        Destroy(aVar.ChorusFilter);
                    }
                    if (aVar.EchoFilter != null) {
                        Destroy(aVar.EchoFilter);
                    }
                    if (aVar.ReverbFilter != null) {
                        Destroy(aVar.ReverbFilter);
                    }
                }

                if (groupTrans == null) {
                    continue;
                }

                _groupsToRemove.Add(groupTrans);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < musicDuckingSounds.Count; i++) {
                var aDuck = musicDuckingSounds[i];
                if (aDuck.soundType == MasterAudio.NoGroupName) {
                    continue;
                }

                MasterAudio.AddSoundGroupToDuckList(aDuck.soundType, aDuck.riseVolStart, aDuck.duckedVolumeCut, aDuck.unduckTime);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < customEventsToCreate.Count; i++) {
                var anEvent = customEventsToCreate[i];
                MasterAudio.CreateCustomEvent(anEvent.EventName, anEvent.eventReceiveMode, anEvent.distanceThreshold,
                    errorOnDuplicates);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < musicPlaylists.Count; i++) {
                var aPlaylist = musicPlaylists[i];
                MasterAudio.CreatePlaylist(aPlaylist, errorOnDuplicates);
            }

            _hasCreated = true;

            if (itemsCreatedEventExpanded) {
                MasterAudio.FireCustomEvent(itemsCreatedCustomEvent, _trans.position);
            }
        }

        /*! \cond PRIVATE */
        public void PopulateGroupData() {
            if (_trans == null) {
                _trans = transform;
            }
            _groupsToCreate.Clear();

            for (var i = 0; i < _trans.childCount; i++) {
                var aGroup = _trans.GetChild(i).GetComponent<DynamicSoundGroup>();
                if (aGroup == null) {
                    continue;
                }

                aGroup.groupVariations.Clear();

                for (var c = 0; c < aGroup.transform.childCount; c++) {
                    var aVar = aGroup.transform.GetChild(c).GetComponent<DynamicGroupVariation>();
                    if (aVar == null) {
                        continue;
                    }

                    aGroup.groupVariations.Add(aVar);
                }

                _groupsToCreate.Add(aGroup);
            }
        }

        public static int HardCodedBusOptions {
            get { return MasterAudio.HardCodedBusOptions + ExtraHardCodedBusOptions; }
        }
        /*! \endcond */

        /// <summary>
        /// This property can be used to read and write the Dynamic Sound Groups.
        /// </summary>	
        public List<DynamicSoundGroup> GroupsToCreate {
            get { return _groupsToCreate; }
        }

        /*! \cond PRIVATE */
        public bool ShouldShowUnityAudioMixerGroupAssignments {
            get {
#if UNITY_5
                return showUnityMixerGroupAssignment;
#else
                return false;
#endif
            }
        }
        /*! \endcond */
    }
}