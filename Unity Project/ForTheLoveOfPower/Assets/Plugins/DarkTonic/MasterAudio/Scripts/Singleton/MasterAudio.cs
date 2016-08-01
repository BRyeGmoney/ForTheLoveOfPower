using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5
using UnityEngine.Audio;
// ReSharper disable ForCanBeConvertedToForeach
#endif

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
	 
    /// <summary>
    /// This class contains the heart of the Master Audio API. There are also convenience methods here for Playlist Controllers, even though you can call those methods on the Playlist Controller itself as well.
    /// </summary>
    // ReSharper disable once CheckNamespace
    [AudioScriptOrder(-50)]
    public class MasterAudio : MonoBehaviour {
        /*! \cond PRIVATE */
        #region Constants

#pragma warning disable 1591
        public const string MasterAudioDefaultFolder = "Assets/Plugins/DarkTonic/MasterAudio";
        public const string PreviewText = "Fading & random settings are ignored by preview in edit mode.";
        public const float SemiTonePitchFactor = 1.05946f;
        public const float SpatialBlend_2DValue = 0f;
        public const float SpatialBlend_3DValue = 1f;
        public const float MaxCrossFadeTimeSeconds = 120;
        public const float DefaultDuckVolCut = -6f;

        public const string StoredLanguageNameKey = "~MA_Language_Key~";

        public static readonly YieldInstruction EndOfFrameDelay = new WaitForEndOfFrame();

        /// <summary>
        /// Subscribe to this event to be notified when the number of Audio Sources being used by Master Audio changes.
        /// </summary>
        // ReSharper disable once RedundantNameQualifier
        public static System.Action NumberOfAudioSourcesChanged;

        public const string GizmoFileName = "MasterAudio/MasterAudio Icon.png";
        public const int HardCodedBusOptions = 2;
        public const string AllBusesName = "[All]";
        public const string NoGroupName = "[None]";
        public const string DynamicGroupName = "[Type In]";
        public const string NoPlaylistName = "[No Playlist]";
        public const string NoVoiceLimitName = "[NO LMT]";
        public const string OnlyPlaylistControllerName = "~only~";
        public const float InnerLoopCheckInterval = .1f;

        #endregion

        #region Public Variables

        // ReSharper disable InconsistentNaming
        public AudioLocation bulkLocationMode = AudioLocation.Clip;
        public string groupTemplateName = "Default Single";
        public string audioSourceTemplateName = "Max Distance 500";
        public bool showGroupCreation = true;
        public bool useGroupTemplates = false;
        public DragGroupMode curDragGroupMode = DragGroupMode.OneGroupPerClip;
        public List<GameObject> groupTemplates = new List<GameObject>(10);
        public List<GameObject> audioSourceTemplates = new List<GameObject>(10);

        public bool mixerMuted;
        public bool playlistsMuted;

        public LanguageMode langMode = LanguageMode.UseDeviceSetting;
        public SystemLanguage testLanguage = SystemLanguage.English;
        public SystemLanguage defaultLanguage = SystemLanguage.English;

        public List<SystemLanguage> supportedLanguages = new List<SystemLanguage>()
        {
            SystemLanguage.English
        };

        public string busFilter = string.Empty;
        public bool useTextGroupFilter = false;
        public string textGroupFilter = string.Empty;
        public bool resourceClipsPauseDoNotUnload = false;
        public bool resourceClipsAllLoadAsync = true;
        public Transform playlistControllerPrefab;
        public bool persistBetweenScenes = false;
        public bool areGroupsExpanded = true;
        public Transform soundGroupTemplate;
        public Transform soundGroupVariationTemplate;
        public List<GroupBus> groupBuses = new List<GroupBus>();
        public bool groupByBus = true;
        public bool showGizmos = true;
        public bool showAdvancedSettings = true;
        public bool showLocalization = true;

        public bool playListExpanded = true;
        public bool playlistsExpanded = true;

#if UNITY_5
        public AllMusicSpatialBlendType musicSpatialBlendType = AllMusicSpatialBlendType.ForceAllTo2D;
        public float musicSpatialBlend = 0f;

        public AllMixerSpatialBlendType mixerSpatialBlendType = AllMixerSpatialBlendType.ForceAllTo3D;
        public float mixerSpatialBlend = 1f;

        public ItemSpatialBlendType newGroupSpatialType = ItemSpatialBlendType.ForceTo3D;
        public float newGroupSpatialBlend = 1f;
#endif

        public List<Playlist> musicPlaylists = new List<Playlist>()
        {
            new Playlist()
        };

        public float _masterAudioVolume = 1.0f;
        public bool ignoreTimeScale = false;
        public bool followAudioListener = true;
        public bool useGaplessPlaylists = false;
        public bool saveRuntimeChanges = false;
        public bool prioritizeOnDistance = false;
        public int rePrioritizeEverySecIndex = 1;

        public bool useOcclusion = false;
        public int reOccludeEverySecIndex = 1;
        public float occlusionMaxCutoffFreq = AudioUtil.DefaultMaxOcclusionCutoffFrequency;
        public float occlusionMinCutoffFreq = AudioUtil.DefaultMinOcclusionCutoffFrequency;
        public OcclusionSelectionType occlusionSelectType = OcclusionSelectionType.AllGroups;
        public bool occlusionUseLayerMask;
        public LayerMask occlusionLayerMask;
        public bool occlusionShowRaycasts = true;
        public bool occlusionShowCategories = false;
        public RaycastMode occlusionRaycastMode = RaycastMode.Physics3D;
        public bool occlusionIncludeStartRaycast2DCollider = true;

        public bool visualAdvancedExpanded = true;
        public bool logAdvancedExpanded = true;

        public bool showFadingSettings = false;
        public bool stopZeroVolumeVariations = false;
        public bool stopZeroVolumeGroups = false;
        public bool stopZeroVolumeBuses = false;
        public bool stopZeroVolumePlaylists = false;

        public bool resourceAdvancedExpanded = true;
        public bool useClipAgePriority = false;
        public bool LogSounds;
        public bool logCustomEvents = false;
        public bool disableLogging = false;
        public bool showMusicDucking = false;
        public bool enableMusicDucking = true;
        public List<DuckGroupInfo> musicDuckingSounds = new List<DuckGroupInfo>();
        public float defaultRiseVolStart = .5f;
        public float defaultUnduckTime = 1f;
        public float defaultDuckedVolumeCut = DefaultDuckVolCut;
        public float crossFadeTime = 1f;
        public float _masterPlaylistVolume = 1f;
        public bool showGroupSelect = false;
        public bool hideGroupsWithNoActiveVars = false;

        public string newEventName = "my event";
        public bool showCustomEvents = true;
        public List<CustomEvent> customEvents = new List<CustomEvent>();

        public Dictionary<string, DuckGroupInfo> duckingBySoundType = new Dictionary<string, DuckGroupInfo>();
        // populated at runtime

        public int frames;

        public bool showUnityMixerGroupAssignment = true;
        // ReSharper restore InconsistentNaming

        private Transform _trans;
        private bool _soundsLoaded;
        private bool _warming;

        #endregion

        #region Private Variables

        private static readonly Dictionary<string, AudioGroupInfo> AudioSourcesBySoundType =
            new Dictionary<string, AudioGroupInfo>();

        private static Dictionary<string, List<int>> _randomizer = new Dictionary<string, List<int>>();
        private static Dictionary<string, List<int>> _randomizerLeftovers = new Dictionary<string, List<int>>();

        private static Dictionary<string, List<int>> _clipsPlayedBySoundTypeOldestFirst =
            new Dictionary<string, List<int>>();

        private static readonly List<MasterAudioGroup> SoloedGroups = new List<MasterAudioGroup>();
        private static readonly List<BusFadeInfo> BusFades = new List<BusFadeInfo>();
        private static readonly List<GroupFadeInfo> GroupFades = new List<GroupFadeInfo>();
        private static readonly List<AudioSource> AllAudioSources = new List<AudioSource>(100);

        private static readonly Dictionary<string, Dictionary<ICustomEventReceiver, Transform>> ReceiversByEventName =
            new Dictionary<string, Dictionary<ICustomEventReceiver, Transform>>();

        private static readonly Dictionary<string, PlaylistController> PlaylistControllersByName =
            new Dictionary<string, PlaylistController>();

        private static readonly Dictionary<string, SoundGroupRefillInfo> LastTimeSoundGroupPlayed =
            new Dictionary<string, SoundGroupRefillInfo>();

        private static readonly List<GameObject> OcclusionSourcesInRange = new List<GameObject>(32);
        private static readonly List<GameObject> OcclusionSourcesOutOfRange = new List<GameObject>(32);
        private static readonly List<GameObject> OcclusionSourcesBlocked = new List<GameObject>(32);

        private static MasterAudio _instance;
        private static float _repriTime = -1f;
        private static List<string> _groupsToRemove;
        private static string _prospectiveMAFolder = string.Empty;
        private static Transform _listenerTrans;

        private static YieldInstruction _innerLoopDelay;
        private static readonly PlaySoundResult AndForgetSuccessResult = new PlaySoundResult {
            SoundPlayed = true
        };

        #endregion

        #region Master Audio enums
        public enum OcclusionSelectionType {
            AllGroups,
            TurnOnPerBusOrGroup
        }

        public enum RaycastMode {
            Physics3D,
            Physics2D
        }

#if UNITY_5
        public enum AllMusicSpatialBlendType {
            ForceAllTo2D,
            ForceAllTo3D,
            ForceAllToCustom,
            AllowDifferentPerController
        }

        public enum AllMixerSpatialBlendType {
            ForceAllTo2D,
            ForceAllTo3D,
            ForceAllToCustom,
            AllowDifferentPerGroup
        }

        public enum ItemSpatialBlendType {
            ForceTo2D,
            ForceTo3D,
            ForceToCustom,
            UseCurveFromAudioSource
        }
#endif

        public enum InternetFileLoadStatus {
            Loading,
            Loaded,
            Failed
        }

        public enum MixerWidthMode {
            Narrow,
            Normal,
            Wide
        }

        public enum CustomEventReceiveMode {
            Always,
            WhenDistanceLessThan,
            WhenDistanceMoreThan,
            Never
        }

        public enum AudioLocation {
            Clip,
            ResourceFile,
            FileOnInternet
        }

        public enum BusCommand {
            None,
            FadeToVolume,
            Mute,
            Pause,
            Solo,
            Unmute,
            Unpause,
            Unsolo,
            Stop,
            ChangeBusPitch,
            ToggleMute
        }

        public enum DragGroupMode {
            OneGroupPerClip,
            OneGroupWithVariations
        }

        public enum EventSoundFunctionType {
            PlaySound,
            GroupControl,
            BusControl,
            PlaylistControl,
            CustomEventControl,
            GlobalControl,
#if UNITY_5
            UnityMixerControl,
#endif
            PersistentSettingsControl
        }

        public enum LanguageMode {
            UseDeviceSetting,
            SpecificLanguage,
            DynamicallySet
        }

#if UNITY_5
        public enum UnityMixerCommand {
            None,
            TransitionToSnapshot,
            TransitionToSnapshotBlend
        }
#endif

        public enum PlaylistCommand {
            None,
            ChangePlaylist, // by name
            FadeToVolume,
            PlayClip, // by name
            PlayRandomSong,
            PlayNextSong,
            Pause,
            Resume,
            Stop,
            Mute,
            Unmute,
            ToggleMute,
            Restart,
            Start,
            StopLoopingCurrentSong
        }

        public enum CustomEventCommand {
            None,
            FireEvent
        }

        public enum GlobalCommand {
            None,
            PauseMixer,
            UnpauseMixer,
            StopMixer,
            StopEverything,
            PauseEverything,
            UnpauseEverything,
            MuteEverything,
            UnmuteEverything,
            SetMasterMixerVolume,
            SetMasterPlaylistVolume
        }

        public enum SoundGroupCommand {
            None,
            FadeToVolume,
            FadeOutAllOfSound,
            Mute,
            Pause,
            Solo,
            StopAllOfSound,
            Unmute,
            Unpause,
            Unsolo,
            StopAllSoundsOfTransform,
            PauseAllSoundsOfTransform,
            UnpauseAllSoundsOfTransform,
            StopSoundGroupOfTransform,
            PauseSoundGroupOfTransform,
            UnpauseSoundGroupOfTransform,
            FadeOutSoundGroupOfTransform,
            RefillSoundGroupPool,
            RouteToBus
        }

        public enum PersistentSettingsCommand {
            None,
            SetBusVolume,
            SetGroupVolume,
            SetMixerVolume,
            SetMusicVolume,
            MixerMuteToggle,
            MusicMuteToggle
        }
		 
        public enum SongFadeInPosition {
            NewClipFromBeginning = 1,
            NewClipFromLastKnownPosition = 3,
            SynchronizeClips = 5,
        }

        public enum SoundSpawnLocationMode {
            MasterAudioLocation,
            CallerLocation,
            AttachToCaller
        }

        public enum VariationCommand {
            None = 0,
            Stop = 1,
            Pause = 2,
            Unpause = 3
        }

        public static readonly List<SoundGroupCommand> GroupCommandsWithNoGroupSelector = new List<SoundGroupCommand> {
            SoundGroupCommand.None,
            SoundGroupCommand.PauseAllSoundsOfTransform,
            SoundGroupCommand.StopAllSoundsOfTransform,
            SoundGroupCommand.UnpauseAllSoundsOfTransform
        };

        public static readonly List<SoundGroupCommand> GroupCommandsWithNoAllGroupSelector = new List<SoundGroupCommand> {
            SoundGroupCommand.None,
            SoundGroupCommand.FadeOutSoundGroupOfTransform,
            SoundGroupCommand.PauseSoundGroupOfTransform,
            SoundGroupCommand.UnpauseSoundGroupOfTransform,
            SoundGroupCommand.StopSoundGroupOfTransform
        };

        #endregion

        #region Inner classes
        [Serializable]
        public class AudioGroupInfo {
            public List<AudioInfo> Sources;
            public int LastFramePlayed;
            public float LastTimePlayed;
            public MasterAudioGroup Group;
            public bool PlayedForWarming;

            public AudioGroupInfo(List<AudioInfo> sources, MasterAudioGroup groupScript) {
                Sources = sources;
                LastFramePlayed = -50;
                LastTimePlayed = -50;
                Group = groupScript;
                PlayedForWarming = false;
            }
        }

        [Serializable]
        public class AudioInfo {
            public AudioSource Source;
            public float OriginalVolume;
            public float LastPercentageVolume;
            public float LastRandomVolume;
            public SoundGroupVariation Variation;

            public AudioInfo(SoundGroupVariation variation, AudioSource source, float origVol) {
                Variation = variation;
                Source = source;
                OriginalVolume = origVol;
                LastPercentageVolume = 1f;
                LastRandomVolume = 0f;
            }
        }

        [Serializable]
        public class Playlist {
            // ReSharper disable InconsistentNaming
            public bool isExpanded = true;
            public string playlistName = "new playlist";
            public SongFadeInPosition songTransitionType = SongFadeInPosition.NewClipFromBeginning;
            public List<MusicSetting> MusicSettings;
            public AudioLocation bulkLocationMode = AudioLocation.Clip;
            public CrossfadeTimeMode crossfadeMode = CrossfadeTimeMode.UseMasterSetting;
            public float crossFadeTime = 1f;
            public bool fadeInFirstSong = false;
            public bool fadeOutLastSong = false;
            public bool resourceClipsAllLoadAsync = true;
            // ReSharper restore InconsistentNaming

            public enum CrossfadeTimeMode {
                UseMasterSetting,
                Override
            }

            public Playlist() {
                MusicSettings = new List<MusicSetting>();
            }
        }

        [Serializable]
        public class SoundGroupRefillInfo {
            public float LastTimePlayed;
            public float InactivePeriodSeconds;

            public SoundGroupRefillInfo(float lastTimePlayed, float inactivePeriodSeconds) {
                LastTimePlayed = lastTimePlayed;
                InactivePeriodSeconds = inactivePeriodSeconds;
            }
        }
        /*! \endcond */
        #endregion

        #region MonoDevelop events and Helpers

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once FunctionComplexityOverflow
        private void Awake() {
            if (FindObjectsOfType(typeof(MasterAudio)).Length > 1) {
                Destroy(gameObject);
                Debug.Log("More than one Master Audio prefab exists in this Scene. Destroying the newer one called '" +
                          name + "'. You may wish to set up a Bootstrapper Scene so this does not occur.");
                return;
            }

            useGUILayout = false;
            _soundsLoaded = false;

            _innerLoopDelay = new WaitForSeconds(InnerLoopCheckInterval);

            var aud = GetComponent<AudioSource>();
            if (aud != null) {
                // delete the previewer
                // ReSharper disable once ArrangeStaticMemberQualifier
                GameObject.Destroy(aud);
            }

            AudioSourcesBySoundType.Clear();
            PlaylistControllersByName.Clear();
            LastTimeSoundGroupPlayed.Clear();

            AllAudioSources.Clear();
            OcclusionSourcesInRange.Clear();
            OcclusionSourcesOutOfRange.Clear();
            OcclusionSourcesBlocked.Clear();

            var plNames = new List<string>();
            AudioResourceOptimizer.ClearAudioClips();

            PlaylistController.Instances = null; // clear the cache
            var playlists = PlaylistController.Instances;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlists.Count; i++) {
                var aList = playlists[i];

                if (plNames.Contains(aList.name)) {
                    Debug.LogError("You have more than 1 Playlist Controller with the name '" + aList.name +
                                   "'. You must name them all uniquely or the same-named ones will be deleted once they awake.");
                    continue;
                }

                plNames.Add(aList.name);

                PlaylistControllersByName.Add(aList.name, aList);
                if (persistBetweenScenes) {
                    DontDestroyOnLoad(aList);
                }
            }

            // start up Objects!
            if (persistBetweenScenes) {
                DontDestroyOnLoad(gameObject);
            }

            var playedStatuses = new List<int>();

            // ReSharper disable TooWideLocalVariableScope
            Transform parentGroup;
            List<AudioInfo> sources;
            AudioSource source;
            AudioGroupInfo group;
            MasterAudioGroup groupScript;
            string soundType;
            // ReSharper restore TooWideLocalVariableScope

            _randomizer = new Dictionary<string, List<int>>();
            _randomizerLeftovers = new Dictionary<string, List<int>>();
            _clipsPlayedBySoundTypeOldestFirst = new Dictionary<string, List<int>>();

            var firstGroupName = string.Empty;

            var allVars = new List<SoundGroupVariation>();

            _groupsToRemove = new List<string>(Trans.childCount);

            for (var k = 0; k < Trans.childCount; k++) {
                parentGroup = Trans.GetChild(k);

                sources = new List<AudioInfo>();

                groupScript = parentGroup.GetComponent<MasterAudioGroup>();

                if (groupScript == null) {
                    Debug.LogError("MasterAudio could not find 'MasterAudioGroup' script for group '" + parentGroup.name +
                                   "'. Skipping this group.");
                    continue;
                }

                soundType = parentGroup.name;

                if (string.IsNullOrEmpty(firstGroupName)) {
                    firstGroupName = soundType;
                }

                var newWeightedChildren = new List<Transform>();

                // ReSharper disable TooWideLocalVariableScope
                SoundGroupVariation variation;
                SoundGroupVariation childVariation;
                Transform child;
                // ReSharper restore TooWideLocalVariableScope

                for (var i = 0; i < parentGroup.childCount; i++) {
                    child = parentGroup.GetChild(i);
                    variation = child.GetComponent<SoundGroupVariation>();
                    source = child.GetComponent<AudioSource>();

                    var weight = variation.weight;

                    for (var j = 0; j < weight; j++) {
                        if (j > 0) {
                            // ReSharper disable once ArrangeStaticMemberQualifier
                            var extraChild =
                                (GameObject)
                                    GameObject.Instantiate(child.gameObject, parentGroup.transform.position,
                                        Quaternion.identity);
                            extraChild.transform.name = child.gameObject.name;
                            childVariation = extraChild.GetComponent<SoundGroupVariation>();
                            childVariation.weight = 1;

                            newWeightedChildren.Add(extraChild.transform);
                            source = extraChild.GetComponent<AudioSource>();

                            sources.Add(new AudioInfo(childVariation, source, source.volume));
                            allVars.Add(childVariation);

                            switch (childVariation.audLocation) {
                                case AudioLocation.ResourceFile:
                                    AudioResourceOptimizer.AddTargetForClip(childVariation.resourceFileName, source);
                                    break;
                                case AudioLocation.FileOnInternet:
                                    AudioResourceOptimizer.AddTargetForClip(childVariation.internetFileUrl, source);
                                    break;
                            }
                        } else {
                            sources.Add(new AudioInfo(variation, source, source.volume));
                            allVars.Add(variation);

                            switch (variation.audLocation) {
                                case AudioLocation.ResourceFile:
                                    var resFileName =
                                        AudioResourceOptimizer.GetLocalizedFileName(variation.useLocalization,
                                            variation.resourceFileName);
                                    AudioResourceOptimizer.AddTargetForClip(resFileName, source);
                                    break;
                                case AudioLocation.FileOnInternet:
                                    AudioResourceOptimizer.AddTargetForClip(variation.internetFileUrl, source);
                                    break;
                            }
                        }
                    }
                }

                // attach extra children from weight property.
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < newWeightedChildren.Count; i++) {
                    newWeightedChildren[i].parent = parentGroup;
                }

                group = new AudioGroupInfo(sources, groupScript);
                if (groupScript.isSoloed) {
                    SoloedGroups.Add(groupScript);
                }

                if (AudioSourcesBySoundType.ContainsKey(soundType)) {
                    Debug.LogError("You have more than one SoundGroup named '" + soundType +
                                   "'. Ignoring the 2nd one. Please rename it.");
                    continue;
                }

                group.Group.OriginalVolume = group.Group.groupMasterVolume;
                // added code for persistent group volume
                var persistentVolume = PersistentAudioSettings.GetGroupVolume(soundType);
                if (persistentVolume.HasValue) {
                    group.Group.groupMasterVolume = persistentVolume.Value;
                }

                AddRuntimeGroupInfo(soundType, group);

                for (var i = 0; i < sources.Count; i++) {
                    playedStatuses.Add(i);
                }

                if (group.Group.curVariationSequence == MasterAudioGroup.VariationSequence.Randomized) {
                    ArrayListUtil.SortIntArray(ref playedStatuses);
                }

                _randomizer.Add(soundType, playedStatuses);
                _randomizerLeftovers.Add(soundType, new List<int>(playedStatuses.Count));
                // fill leftovers pool.
                _randomizerLeftovers[soundType].AddRange(playedStatuses);
                _clipsPlayedBySoundTypeOldestFirst.Add(soundType, new List<int>());

                playedStatuses = new List<int>();
            }

            BusFades.Clear();
            GroupFades.Clear();

            // initialize persistent bus volumes 
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < groupBuses.Count; i++) {
                var aBus = groupBuses[i];

                aBus.OriginalVolume = aBus.volume;
                var busName = aBus.busName;
                var busVol = PersistentAudioSettings.GetBusVolume(busName);

                if (!busVol.HasValue) {
                    continue;
                }

                SetBusVolumeByName(busName, busVol.Value);
            }

            // populate ducking sounds dictionary
            duckingBySoundType.Clear();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < musicDuckingSounds.Count; i++) {
                var aDuck = musicDuckingSounds[i];
                if (duckingBySoundType.ContainsKey(aDuck.soundType)) {
                    continue;
                }
                duckingBySoundType.Add(aDuck.soundType, aDuck);
            }

            _soundsLoaded = true;

            _warming = true;

            // pre-warm the code so the first sound played for real doesn't have to JIT and be slow.
            if (!string.IsNullOrEmpty(firstGroupName)) {
                var result = PlaySound3DFollowTransform(firstGroupName, Trans, 0f);
                if (result != null && result.SoundPlayed) {
                    result.ActingVariation.Stop();
                }
            }

            FireCustomEvent("FakeEvent", _trans.position);

            // ReSharper disable once ForCanBeConvertedToForeach
            // Reset stuff for people who use "Save runtime changes".
            for (var i = 0; i < customEvents.Count; i++) {
                customEvents[i].frameLastFired = -1;
            }
            frames = 0;

            // Event Sounds warmer
            // ReSharper disable once ArrangeStaticMemberQualifier
            var evts = GameObject.FindObjectsOfType(typeof(EventSounds));
            if (evts.Length > 0) {
                var evt = evts[0] as EventSounds;
                evt.PlaySounds(evt.particleCollisionSound, EventSounds.EventType.UserDefinedEvent);
            }

            // done warming
            _warming = false;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < allVars.Count; i++) {
                allVars[i].DisableUpdater();
            }

            // fixed: make sure this happens before Playlists start or the volume won't be right.
            PersistentAudioSettings.RestoreMasterSettings();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start() {
            // wait for Playlist Controller to initialize!
            if (musicPlaylists.Count > 0
                && musicPlaylists[0].MusicSettings != null
                && musicPlaylists[0].MusicSettings.Count > 0
                && musicPlaylists[0].MusicSettings[0].clip != null
                && PlaylistControllersByName.Count == 0) {

                Debug.Log("No Playlist Controllers exist in the Scene. Music will not play.");
            }
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable() {
            var sources = GetComponentsInChildren<AudioSource>().ToList();
            StopTrackingRuntimeAudioSources(sources);
        }

        // ReSharper disable once UnusedMember.Local
        void Update() {
            frames++;

            UpdateLocation();

            // adjust for Inspector realtime slider.
            PerformBusFades();
            PerformGroupFades();
            RefillInactiveGroupPools();
        }

        /*! \cond PRIVATE */
        public void UpdateLocation() {
            if (ListenerTrans != null && followAudioListener) {
                Trans.position = ListenerTrans.position;
            }
        }

        private static void UpdateRefillTime(string sType, float inactivePeriodSeconds) {
            if (!LastTimeSoundGroupPlayed.ContainsKey(sType)) {
                LastTimeSoundGroupPlayed.Add(sType,
                    new SoundGroupRefillInfo(Time.realtimeSinceStartup, inactivePeriodSeconds));
            } else {
                LastTimeSoundGroupPlayed[sType].LastTimePlayed = Time.realtimeSinceStartup;
            }
        }

        private static void RefillInactiveGroupPools() {
            var groups = LastTimeSoundGroupPlayed.GetEnumerator();

            if (_groupsToRemove == null) { // re-init for compile-time changes.
                _groupsToRemove = new List<string>();
            }
            _groupsToRemove.Clear();

            while (groups.MoveNext()) {
                var grp = groups.Current;
                if (!(grp.Value.LastTimePlayed + grp.Value.InactivePeriodSeconds < Time.realtimeSinceStartup)) {
                    continue;
                }

                RefillSoundGroupPool(grp.Key);
                _groupsToRemove.Add(grp.Key);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _groupsToRemove.Count; i++) {
                LastTimeSoundGroupPlayed.Remove(_groupsToRemove[i]);
            }
        }

        private void PerformBusFades() {
            // ReSharper disable TooWideLocalVariableScope
            BusFadeInfo aFader;
            GroupBus aBus;
            // ReSharper restore TooWideLocalVariableScope

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < BusFades.Count; i++) {
                aFader = BusFades[i];
                if (!aFader.IsActive) {
                    continue;
                }

                aBus = aFader.ActingBus;
                if (aBus == null) {
                    Debug.Log("Could not find bus named '" + aFader.NameOfBus + "' to fade it one step.");
                    aFader.IsActive = false;
                    continue;
                }

                var newVolume = aBus.volume + aFader.VolumeStep;

                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (aFader.VolumeStep > 0f) {
                    newVolume = Math.Min(newVolume, aFader.TargetVolume);
                } else {
                    newVolume = Math.Max(newVolume, aFader.TargetVolume);
                }

                SetBusVolumeByName(aBus.busName, newVolume);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (newVolume != aFader.TargetVolume) {
                    continue;
                }

                aFader.IsActive = false;

                if (stopZeroVolumeBuses && aFader.TargetVolume == 0f) {
                    StopBus(aFader.NameOfBus);
                }

                if (aFader.completionAction != null) {
                    aFader.completionAction();
                }
            }

            BusFades.RemoveAll(delegate(BusFadeInfo obj) {
                return obj.IsActive == false;
            });
        }

        private void PerformGroupFades() {
            // ReSharper disable TooWideLocalVariableScope
            GroupFadeInfo aFader;
            MasterAudioGroup aGroup;
            // ReSharper restore TooWideLocalVariableScope

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < GroupFades.Count; i++) {
                aFader = GroupFades[i];
                if (!aFader.IsActive) {
                    continue;
                }

                aGroup = aFader.ActingGroup;
                if (aGroup == null) {
                    Debug.Log("Could not find Sound Group named '" + aFader.NameOfGroup + "' to fade it one step.");
                    aFader.IsActive = false;
                    continue;
                }

                var newVolume = aGroup.groupMasterVolume + aFader.VolumeStep;

                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (aFader.VolumeStep > 0f) {
                    newVolume = Math.Min(newVolume, aFader.TargetVolume);
                } else {
                    newVolume = Math.Max(newVolume, aFader.TargetVolume);
                }

                SetGroupVolume(aGroup.GameObjectName, newVolume);

                if (newVolume != aFader.TargetVolume) {
                    continue;
                }
                aFader.IsActive = false;

                if (stopZeroVolumeGroups && aFader.TargetVolume == 0f) {
                    StopAllOfSound(aFader.NameOfGroup);
                }

                if (aFader.completionAction != null) {
                    aFader.completionAction();
                }
            }

            GroupFades.RemoveAll(delegate(GroupFadeInfo obj) {
                return obj.IsActive == false;
            });
        }

        // ReSharper disable once UnusedMember.Local
        private void OnApplicationQuit() {
            AppIsShuttingDown = true;
            // very important!! Dont' take this out, false debug info may show up when you stop the Player
        }
        /*! \endcond */

        #endregion

        #region Sound Playing / Stopping Methods

        /// <summary>
        /// This method allows you to play a sound in a Sound Group in the location of the Master Audio prefab. Returns nothing.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <returns>boolean- true indicating that the sound was either played or scheduled, false otherwise.</returns>
        public static bool PlaySoundAndForget(string sType, float volumePercentage = 1f, float? pitch = null,
            float delaySoundTime = 0f, string variationName = null) {
            if (!SceneHasMasterAudio) {
                return false;
            }

            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
                return false;
            }

            var psr = PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, pitch, null, variationName, false, delaySoundTime);

            return psr != null && (psr.SoundPlayed || psr.SoundScheduled);
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group in the location of the Master Audio prefab. Returns a PlaySoundResult object.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <param name="isChaining"><b>Don't ever specify this</b> - used to control number of loops for Chained Loop Groups. MasterAudio will pass this parameter when it needs it. Never specify this param.</param>
        /// <param name="isSingleSubscribedPlay"><b>Don't ever specify this</b> - MasterAudio will pass this parameter when it needs it. Never specify this param.</param>
        /// <returns>PlaySoundResult - this object can be used to read if the sound played or not and also gives access to the Variation object that was used.</returns>
        public static PlaySoundResult PlaySound(string sType, float volumePercentage = 1f, float? pitch = null,
            float delaySoundTime = 0f, string variationName = null, bool isChaining = false,
            bool isSingleSubscribedPlay = false) {
            if (!SceneHasMasterAudio) {
                return null;
            }

            if (SoundsReady) {
                return PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, pitch, null, variationName, false,
                    delaySoundTime, false, true, isChaining, isSingleSubscribedPlay);
            }

            Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
            return null;
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group from a specific Vector 3 position. Returns nothing.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="sourcePosition">The position you want the sound to eminate from. Required.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <returns>boolean- true indicating that the sound was either played or scheduled, false otherwise.</returns>
        public static bool PlaySound3DAtVector3AndForget(string sType, Vector3 sourcePosition,
            float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null) {
            if (!SceneHasMasterAudio) {
                return false;
            }

            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
                return false;
            }

            var psr = PlaySoundAtVolume(sType, volumePercentage, sourcePosition, pitch, null, variationName, false, delaySoundTime,
                true);

            return psr != null && (psr.SoundPlayed || psr.SoundScheduled);
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group from a specific Vector3 position. Returns a PlaySoundResult object.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="sourcePosition">The position you want the sound to eminate from. Required.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <returns>PlaySoundResult - this object can be used to read if the sound played or not and also gives access to the Variation object that was used.</returns>
        public static PlaySoundResult PlaySound3DAtVector3(string sType, Vector3 sourcePosition,
            float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null) {
            if (!SceneHasMasterAudio) {
                return null;
            }

            if (SoundsReady) {
                return PlaySoundAtVolume(sType, volumePercentage, sourcePosition, pitch, null, variationName, false,
                    delaySoundTime, true, true);
            }

            Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
            return null;
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group from a specific position - the position of a Transform you pass in. Returns nothing.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="sourceTrans">The Transform whose position you want the sound to eminate from. Pass null if you want to play the sound 2D.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <returns>boolean- true indicating that the sound was either played or scheduled, false otherwise.</returns>
        public static bool PlaySound3DAtTransformAndForget(string sType, Transform sourceTrans = null,
            float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null) {
            if (!SceneHasMasterAudio) {
                return false;
            }

            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
                return false;
            }

            var psr = PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, pitch, sourceTrans, variationName, false,
                delaySoundTime);

            return psr != null && (psr.SoundPlayed || psr.SoundScheduled);
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group from a specific position - the position of a Transform you pass in.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="sourceTrans">The Transform whose position you want the sound to eminate from. Pass null if you want to play the sound 2D.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <param name="isChaining"><b>Don't ever specify this</b> - used to control number of loops for Chained Loop Groups. MasterAudio will pass this parameter when it needs it. Never specify this param.</param>
        /// <param name="isSingleSubscribedPlay"><b>Don't ever specify this</b> - MasterAudio will pass this parameter when it needs it. Never specify this param.</param>
        /// <returns>PlaySoundResult - this object can be used to read if the sound played or not and also gives access to the Variation object that was used.</returns>
        public static PlaySoundResult PlaySound3DAtTransform(string sType, Transform sourceTrans = null,
            float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null,
            bool isChaining = false, bool isSingleSubscribedPlay = false) {
            if (!SceneHasMasterAudio) {
                return null;
            }

            if (SoundsReady) {
                return PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, pitch, sourceTrans, variationName, false,
                    delaySoundTime, false, true, isChaining, isSingleSubscribedPlay);
            }

            Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
            return null;
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group from a specific position - a Transform you pass in. Returns nothing.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="sourceTrans">The Transform whose position you want the sound to eminate from. Pass null if you want to play the sound 2D.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <returns>boolean- true indicating that the sound was either played or scheduled, false otherwise.</returns>
        public static bool PlaySound3DFollowTransformAndForget(string sType, Transform sourceTrans = null,
            float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null) {
            if (!SceneHasMasterAudio) {
                return false;
            }

            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
                return false;
            }

            var psr = PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, pitch, sourceTrans, variationName, true,
                delaySoundTime);

            return psr != null && (psr.SoundPlayed || psr.SoundScheduled);
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group from a specific position - a Transform you pass in, and it will follow the Transform if it moves. Returns a PlaySoundResult.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="sourceTrans">The Transform whose position you want the sound to eminate from. Pass null if you want to play the sound 2D.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <param name="isChaining"><b>Don't ever specify this</b> - used to control number of loops for Chained Loop Groups. MasterAudio will pass this parameter when it needs it. Never specify this param.</param>
        /// <param name="isSingleSubscribedPlay"><b>Don't ever specify this</b> - MasterAudio will pass this parameter when it needs it. Never specify this param.</param>
        /// <returns>PlaySoundResult - this object can be used to read if the sound played or not and also gives access to the Variation object that was used.</returns>
        public static PlaySoundResult PlaySound3DFollowTransform(string sType, Transform sourceTrans = null,
            float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null,
            bool isChaining = false, bool isSingleSubscribedPlay = false) {
            if (!SceneHasMasterAudio) {
                return null;
            }

            if (SoundsReady) {
                return PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, pitch, sourceTrans, variationName, true,
                    delaySoundTime, false, true, isChaining, isSingleSubscribedPlay);
            }
            Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
            return null;
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group from a specific position - a Transform you pass in. Returns nothing.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <returns>boolean- true indicating that the sound was either played or scheduled, false otherwise.</returns>
        public static bool PlaySound3DAndForget(string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null) {
            if (!SceneHasMasterAudio) {
                return false;
            }

            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
                return false;
            }

            var psr = PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, pitch, null, variationName, false,
                delaySoundTime);

            return psr != null && (psr.SoundPlayed || psr.SoundScheduled);
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group from the location of Master Audio. This method will not return until the sound is finished (or cannot play) to continue execution. You need to call this with StartCoroutine. The sound will not be played looped, since that could cause a Coroutine that would never end.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <param name="completedAction"><b>Optional</b> - Code to execute when the sound is finished.</param>
        public static IEnumerator PlaySoundAndWaitUntilFinished(string sType, float volumePercentage = 1f,
            // ReSharper disable once RedundantNameQualifier
            float? pitch = null, float delaySoundTime = 0f, string variationName = null, System.Action completedAction = null) {
            if (!SceneHasMasterAudio) {
                yield break;
            }

            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
                yield break;
            }

            var sound = PlaySound(sType, volumePercentage, pitch, delaySoundTime, variationName, false, true);
            var done = false;

            if (sound == null || sound.ActingVariation == null) {
                yield break; // sound was "busy" or couldn't play for some reason.
            }
            sound.ActingVariation.SoundFinished += delegate {
                done = true;
            };

            while (!done) {
                yield return EndOfFrameDelay;
            }

            if (completedAction != null) {
                completedAction();
            }
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group from a specific position - a Transform you pass in. This method will not return until the sound is finished (or cannot play) to continue execution. You need to call this with StartCoroutine. The sound will not be played looped, since that could cause a Coroutine that would never end.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="sourceTrans">The Transform whose position you want the sound to eminate from. Pass null if you want to play the sound 2D.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <param name="completedAction"><b>Optional</b> - Code to execute when the sound is finished.</param>
        public static IEnumerator PlaySound3DAtTransformAndWaitUntilFinished(string sType, Transform sourceTrans,
            // ReSharper disable once RedundantNameQualifier
            float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, System.Action completedAction = null) {
            if (!SceneHasMasterAudio) {
                yield break;
            }

            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
                yield break;
            }

            var sound = PlaySound3DAtTransform(sType, sourceTrans, volumePercentage, pitch, delaySoundTime,
                variationName, false, true); 
            var done = false;

            if (sound == null || sound.ActingVariation == null) {
                yield break; // sound was "busy" or couldn't play for some reason.
            }
            sound.ActingVariation.SoundFinished += delegate {
                done = true;
            };

            while (!done) {
                yield return EndOfFrameDelay;
            }

            if (completedAction != null) {
                completedAction();
            }
        }

        /// <summary>
        /// This method allows you to play a sound in a Sound Group from a specific position - a Transform you pass in, and it will follow the Transform if it moves. This method will not return until the sound is finished (or cannot play) to continue execution. You need to call this with StartCoroutine. The sound will not be played looped, since that could cause a Coroutine that would never end.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to trigger a sound from.</param>
        /// <param name="sourceTrans">The Transform whose position you want the sound to eminate from. Pass null if you want to play the sound 2D.</param>
        /// <param name="volumePercentage"><b>Optional</b> - used if you want to play the sound at a reduced volume (between 0 and 1).</param>
        /// <param name="pitch"><b>Optional</b> - used if you want to play the sound at a specific pitch. If you do, it will override the pich and random pitch in the variation.</param>
        /// <param name="delaySoundTime"><b>Optional</b> - used if you want to play the sound X seconds from now instead of immediately.</param>
        /// <param name="variationName"><b>Optional</b> - used if you want to play a specific variation by name. Otherwise a random variation is played.</param>
        /// <param name="completedAction"><b>Optional</b> - Code to execute when the sound is finished.</param>
        public static IEnumerator PlaySound3DFollowTransformAndWaitUntilFinished(string sType, Transform sourceTrans,
            // ReSharper disable once RedundantNameQualifier
            float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, System.Action completedAction = null) {
            if (!SceneHasMasterAudio) {
                yield break;
            }

            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
                yield break;
            }

            var sound = PlaySound3DFollowTransform(sType, sourceTrans, volumePercentage, pitch, delaySoundTime,
                variationName, false, true);
            var done = false;

            if (sound == null || sound.ActingVariation == null) {
                yield break; // sound was "busy" or couldn't play for some reason.
            }
            sound.ActingVariation.SoundFinished += delegate {
                done = true;
            };

            while (!done) {
                yield return EndOfFrameDelay;
            }

            if (completedAction != null) {
                completedAction();
            }
        }

        // ReSharper disable once FunctionComplexityOverflow
        private static PlaySoundResult PlaySoundAtVolume(string sType,
            float volumePercentage,
            Vector3 sourcePosition,
            float? pitch = null,
            Transform sourceTrans = null,
            string variationName = null,
            bool attachToSource = false,
            float delaySoundTime = 0f,
            bool useVector3 = false,
            bool makePlaySoundResult = false,
            bool isChaining = false,
            bool isSingleSubscribedPlay = false,
            bool triggeredAsChildGroup = false) {

            if (!SceneHasMasterAudio) {
                // No MA
                return null;
            }

            if (!SoundsReady || sType == string.Empty || sType == NoGroupName) {
                return null; // not awake yet
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                var msg = "MasterAudio could not find sound: " + sType +
                          ". If your Scene just changed, this could happen when an OnDisable or OnInvisible event sound happened to a per-scene sound, which is expected.";
                if (sourceTrans != null) {
                    msg += " Triggered by prefab: " + (sourceTrans.name);
                }

                LogWarning(msg);
                return null;
            }

            var group = AudioSourcesBySoundType[sType];
            var maGroup = group.Group;

            var loggingEnabledForGrp = LoggingEnabledForGroup(maGroup);

            if (group.Group.childGroupMode == MasterAudioGroup.ChildGroupMode.TriggerLinkedGroupsWhenRequested &&
                !triggeredAsChildGroup) {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < group.Group.childSoundGroups.Count; i++) {
                    var childGrpName = group.Group.childSoundGroups[i];
                    PlaySoundAtVolume(childGrpName, volumePercentage, sourcePosition, pitch, sourceTrans, null,
                        attachToSource, delaySoundTime, useVector3, false, false, false, true);
                }
            }

            if (Instance.mixerMuted) {
                if (loggingEnabledForGrp) {
                    LogMessage("MasterAudio skipped playing sound: " + sType + " because the Mixer is muted.");
                }
                return null;
            }
            if (maGroup.isMuted) {
                if (loggingEnabledForGrp) {
                    LogMessage("MasterAudio skipped playing sound: " + sType + " because the Group is muted.");
                }
                return null;
            }
            if (SoloedGroups.Count > 0 && !SoloedGroups.Contains(maGroup)) {
                if (loggingEnabledForGrp) {
                    LogMessage("MasterAudio skipped playing sound: " + sType +
                               " because there are one or more Groups soloed. This one is not.");
                }
                return null;
            }

            group.PlayedForWarming = IsWarming;
            if (maGroup.curVariationMode == MasterAudioGroup.VariationMode.Normal) {
                switch (maGroup.limitMode) {
                    case MasterAudioGroup.LimitMode.TimeBased:
                        if (maGroup.minimumTimeBetween > 0) {
                            if (Time.realtimeSinceStartup < (group.LastTimePlayed + maGroup.minimumTimeBetween)) {
                                if (loggingEnabledForGrp) {
                                    LogMessage("MasterAudio skipped playing sound: " + sType +
                                               " due to Group's Min Seconds Between setting.");
                                }
                                return null;
                            }
                        }
                        break;
                    case MasterAudioGroup.LimitMode.FrameBased:
                        if (Time.frameCount - group.LastFramePlayed < maGroup.limitPerXFrames) {
                            if (loggingEnabledForGrp) {
                                LogMessage("Master Audio skipped playing sound: " + sType +
                                           " due to Group's Per Frame Limit.");
                            }
                            return null;
                        }

                        break;
                    case MasterAudioGroup.LimitMode.None:
                        break;
                }
            }

            SetLastPlayed(group);

            var sources = group.Sources;
            var isNonSpecific = string.IsNullOrEmpty(variationName);

            if (sources.Count == 0) {
                if (loggingEnabledForGrp) {
                    LogMessage("Sound Group {" + sType + "} has no active Variations.");
                }
                return null;
            }

            if (maGroup.curVariationMode == MasterAudioGroup.VariationMode.Normal) {
                if (group.Group.limitPolyphony) {
                    var maxVoices = group.Group.voiceLimitCount;
                    var busyVoices = 0;
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < group.Sources.Count; i++) {
                        if (group.Sources[i].Source == null) {
                            continue;
                        }
                        if (!group.Sources[i].Source.isPlaying) {
                            continue;
                        }

                        busyVoices++;
                        if (busyVoices < maxVoices) {
                            continue;
                        }

                        if (loggingEnabledForGrp) {
                            LogMessage("Polyphony limit of group: " + @group.Group.GameObjectName +
                                       " exceeded. Will not play this sound for this instance.");
                        }
                        return null;
                    }
                }
            }

            var groupBus = group.Group.BusForGroup;
            if (groupBus != null) {
                if (groupBus.BusVoiceLimitReached) {
                    if (!groupBus.stopOldest) {
                        if (loggingEnabledForGrp) {
                            LogMessage("Bus voice limit has been reached. Cannot play the sound: " +
                                       group.Group.GameObjectName +
                                       " until one voice has stopped playing. You can turn on the 'Stop Oldest' option for the bus to change ");
                        }
                        return null;
                    }

                    StopOldestSoundOnBus(groupBus);
                }
            }

            AudioInfo randomSource = null;

            var isSingleVarLoop = false;

            if (sources.Count == 1) {
                if (loggingEnabledForGrp) {
                    LogMessage("Cueing only child of " + sType);
                }
                randomSource = sources[0];

                if (maGroup.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain) {
                    isSingleVarLoop = true;
                }
            }

            List<int> choices = null;
            int? randomIndex = null;
            List<int> otherChoices = null;

            var pickedChoice = -1;

            if (randomSource == null) {
                // we must get a non-busy random source!
                if (!_randomizer.ContainsKey(sType)) {
                    Debug.Log("Sound Group {" + sType + "} has no active Variations.");
                    return null;
                }

                if (isNonSpecific) {
                    choices = _randomizer[sType];

                    randomIndex = 0;
                    pickedChoice = choices[randomIndex.Value];
                    randomSource = sources[pickedChoice];

                    // fill list with other random sources not used yet in case the first is busy.
                    otherChoices = _randomizerLeftovers[sType];
                    otherChoices.Remove(pickedChoice);

                    if (loggingEnabledForGrp) {
                        LogMessage(string.Format("Cueing child {0} of {1}",
                            choices[randomIndex.Value],
                            sType));
                    }
                } else {
                    // find source by name
                    var isFound = false;
                    var matchesFound = 0;
                    for (var i = 0; i < sources.Count; i++) {
                        var aSource = sources[i];
                        if (aSource.Source.name != variationName) {
                            continue;
                        }

                        matchesFound++;
                        if (!aSource.Variation.IsAvailableToPlay) {
                            continue;
                        }

                        randomSource = aSource;
                        isFound = true;
                        pickedChoice = i;
                        break;
                    }

                    if (!isFound) {
                        if (matchesFound == 0) {
                            if (loggingEnabledForGrp) {
                                LogMessage("Can't find variation {" + variationName + "} of " + sType);
                            }
                        } else {
                            if (loggingEnabledForGrp) {
                                LogMessage("Can't find non-busy variation {" + variationName + "} of " + sType);
                            }
                        }

                        return null;
                    }

                    if (loggingEnabledForGrp) {
                        LogMessage(string.Format("Cueing child named '{0}' of {1}",
                            variationName,
                            sType));
                    }
                }
            }

            if (randomSource.Variation == null) {
                // scene ending
                return null;
            }

            if (randomSource.Variation.audLocation == AudioLocation.Clip && randomSource.Variation.VarAudio.clip == null) {
                if (loggingEnabledForGrp) {
                    LogMessage(
                        string.Format("Child named '{0}' of {1} has no audio assigned to it so nothing will be played.",
                            randomSource.Variation.name,
                            sType));
                }

                RemoveClipAndRefillIfEmpty(group, isNonSpecific, randomIndex, choices, sType, pickedChoice,
                    loggingEnabledForGrp, false);

                return null;
                // nothing played, it's silent. Don't take up voices because that could silence a Dialog Group that already has a real sound playing.
            }

            if (group.Group.curVariationMode == MasterAudioGroup.VariationMode.Dialog) {
                if (group.Group.useDialogFadeOut) {
                    FadeOutAllOfSound(group.Group.GameObjectName, group.Group.dialogFadeOutTime);
                } else {
                    StopAllOfSound(group.Group.GameObjectName);
                }
            }

            PlaySoundResult playedState;
            var playedSound = false;
            var forgetSoundPlayedOrScheduled = false;

            bool soundSuccess;
            // ReSharper disable TooWideLocalVariableScope
            bool makePsRsuccess;
            bool doNotMakePsRsuccess;
            // ReSharper restore TooWideLocalVariableScope

            do {
                playedState = PlaySoundIfAvailable(randomSource, sourcePosition, volumePercentage,
                    ref forgetSoundPlayedOrScheduled, pitch, group, sourceTrans, attachToSource, delaySoundTime,
                    useVector3, makePlaySoundResult, isChaining, isSingleSubscribedPlay);

                makePsRsuccess = makePlaySoundResult && 
                    (playedState != null && (playedState.SoundPlayed || playedState.SoundScheduled));
                
                doNotMakePsRsuccess = !makePlaySoundResult && forgetSoundPlayedOrScheduled;

                soundSuccess = makePsRsuccess || doNotMakePsRsuccess;

                if (soundSuccess) {
                    playedSound = true;

                    if (!IsWarming) {
                        RemoveClipAndRefillIfEmpty(group, isNonSpecific, randomIndex, choices, sType, pickedChoice,
                            loggingEnabledForGrp, isSingleVarLoop);
                    }
                } else if (isNonSpecific) {
                    // try the other ones
                    if (otherChoices == null || otherChoices.Count <= 0) {
                        continue;
                    }
                    randomSource = sources[otherChoices[0]];
                    if (loggingEnabledForGrp) {
                        LogMessage("Child was busy. Cueing child {" + sources[otherChoices[0]].Source.name + "} of " +
                                   sType);
                    }
                    otherChoices.RemoveAt(0);
                } else {
                    if (loggingEnabledForGrp) {
                        LogMessage("Child was busy. Since you wanted a named Variation, no others to try. Aborting.");
                    }
                    if (otherChoices != null) {
                        otherChoices.Clear();
                    }
                }
            } while (!playedSound && otherChoices != null && otherChoices.Count > 0);
            // repeat until you've either played the sound or exhausted all possibilities.

            if (!soundSuccess) {
                if (loggingEnabledForGrp) {
                    LogMessage("All children of " + sType + " were busy. Will not play this sound for this instance.");
                }
            } else {
                // ReSharper disable once InvertIf
                if (group.Group.childGroupMode == MasterAudioGroup.ChildGroupMode.TriggerLinkedGroupsWhenPlayed &&
                    !triggeredAsChildGroup && !IsWarming) {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < group.Group.childSoundGroups.Count; i++) {
                        var childGrpName = group.Group.childSoundGroups[i];
                        
                        PlaySoundAtVolume(childGrpName, volumePercentage, sourcePosition, pitch, sourceTrans, null,
                            attachToSource, delaySoundTime, useVector3, false, false, false, true);
                    }
                }

                if (group.Group.soundPlayedEventActive) {
                    FireCustomEvent(group.Group.soundPlayedCustomEvent, Instance._trans.position);
                }
            }

            if (!makePlaySoundResult && soundSuccess) {
                return AndForgetSuccessResult;
            }

            return playedState;
        }

        private static void SetLastPlayed(AudioGroupInfo grp) {
            grp.LastTimePlayed = Time.realtimeSinceStartup;
            grp.LastFramePlayed = Time.frameCount;
        }

        private static void RemoveClipAndRefillIfEmpty(AudioGroupInfo grp, bool isNonSpecific, int? randomIndex,
            List<int> choices, string sType, int pickedChoice, bool loggingEnabledForGrp, bool isSingleVarLoop) {

            if (isSingleVarLoop) {
                grp.Group.ChainLoopCount++; // this doesn't call Refill Sound Group where this normally occurs so this part needs to happen separately
                return;
            }

            if (isNonSpecific && randomIndex.HasValue) {
                // only if successfully played!
                choices.RemoveAt(randomIndex.Value);
                _clipsPlayedBySoundTypeOldestFirst[sType].Add(pickedChoice);

                if (choices.Count == 0) {
                    if (loggingEnabledForGrp) {
                        LogMessage("Refilling Variation pool: " + sType);
                    }

                    RefillSoundGroupPool(sType);
                }
            }

            if (grp.Group.curVariationSequence == MasterAudioGroup.VariationSequence.TopToBottom &&
                grp.Group.useInactivePeriodPoolRefill) {
                UpdateRefillTime(sType, grp.Group.inactivePeriodSeconds);
            }
        }

        // ReSharper disable once FunctionComplexityOverflow
        private static PlaySoundResult PlaySoundIfAvailable(AudioInfo info,
            Vector3 sourcePosition,
            float volumePercentage,
            ref bool forgetSoundPlayed,
            float? pitch = null,
            AudioGroupInfo audioGroup = null,
            Transform sourceTrans = null,
            bool attachToSource = false,
            float delaySoundTime = 0f,
            bool useVector3 = false,
            bool makePlaySoundResult = false,
            bool isChaining = false,
            bool isSingleSubscribedPlay = false) {

            if (info.Source == null) {
                // this avoids false errors when stopping the game (from became "invisible" event callers)
                return null;
            }

            // ReSharper disable once PossibleNullReferenceException
            var maGroup = audioGroup.Group;

            if (maGroup.curVariationMode == MasterAudioGroup.VariationMode.Normal && info.Source.isPlaying) {
                var playedPercentage = AudioUtil.GetAudioPlayedPercentage(info.Source);
                var retriggerPercent = maGroup.retriggerPercentage;

                if (playedPercentage < retriggerPercent) {
                    return null; // wait for this to stop playing or play further.
                }
            }

            info.Variation.Stop();
            info.Variation.ObjectToFollow = null;

            var shouldUseClipAgePriority = Instance.prioritizeOnDistance &&
                                           (Instance.useClipAgePriority || info.Variation.ParentGroup.useClipAgePriority);

            if (useVector3) {
                info.Source.transform.position = sourcePosition;
                if (Instance.prioritizeOnDistance) {
                    AudioPrioritizer.Set3DPriority(info.Variation, shouldUseClipAgePriority);
                }
            } else if (sourceTrans != null) {
                if (attachToSource) {
                    info.Variation.ObjectToFollow = sourceTrans;
                } else {
                    info.Source.transform.position = sourceTrans.position;
                    info.Variation.ObjectToTriggerFrom = sourceTrans;
                }

                if (Instance.prioritizeOnDistance) {
                    AudioPrioritizer.Set3DPriority(info.Variation, shouldUseClipAgePriority);
                }
            } else {
                // "2d manner" - from Master Audio location
                if (Instance.prioritizeOnDistance) {
                    AudioPrioritizer.Set2DSoundPriority(info.Source);
                }
                info.Source.transform.localPosition = Vector3.zero;
                // put it back in MA prefab position after being detached.
            }

            var groupVolume = maGroup.groupMasterVolume;
            var busVolume = GetBusVolume(maGroup);

            var varVol = info.OriginalVolume;

            var randomVol = 0f;
            if (info.Variation.useRandomVolume) {
                // random volume
                randomVol = UnityEngine.Random.Range(info.Variation.randomVolumeMin, info.Variation.randomVolumeMax);

                switch (info.Variation.randomVolumeMode) {
                    case SoundGroupVariation.RandomVolumeMode.AddToClipVolume:
                        varVol += randomVol;
                        break;
                    case SoundGroupVariation.RandomVolumeMode.IgnoreClipVolume:
                        varVol = randomVol;
                        break;
                }
            }

            var calcVolume = varVol * groupVolume * busVolume * Instance._masterAudioVolume;

            // set volume to percentage.
            var volume = calcVolume * volumePercentage;
            var targetVolume = volume;

            info.Source.volume = targetVolume;

            // save these for on the fly adjustments afterward
            info.LastPercentageVolume = volumePercentage;
            info.LastRandomVolume = randomVol;

            // ReSharper disable once JoinDeclarationAndInitializer
            bool isActive;

            isActive = info.Variation.GameObj.activeInHierarchy;

            if (!isActive) {
                return null;
            }

            PlaySoundResult result = null;

            if (makePlaySoundResult) {
                result = new PlaySoundResult { ActingVariation = info.Variation };

                if (delaySoundTime > 0f) {
                    result.SoundScheduled = true;
                } else {
                    result.SoundPlayed = true;
                }
            } else {
                forgetSoundPlayed = true;
            }

            var soundType = maGroup.GameObjectName;
            var isChainLoop = maGroup.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain;

            if (isChainLoop) {
                if (!isChaining) {
                    maGroup.ChainLoopCount = 0;
                }

                var objFollow = info.Variation.ObjectToFollow;

                // make sure there isn't 2 chains going, ever!
                if (maGroup.ActiveVoices > 0 && !isChaining) {
                    StopAllOfSound(soundType);
                }

                // restore this because it is lost from the Stop above;
                info.Variation.ObjectToFollow = objFollow;
            }

            info.Variation.Play(pitch, targetVolume, soundType, volumePercentage, targetVolume, pitch, sourceTrans,
                attachToSource, delaySoundTime, isChaining, isSingleSubscribedPlay);

            return result;
        }

        /*! \cond PRIVATE */
        public static void DuckSoundGroup(string soundGroupName, AudioSource aSource) {
            var ma = Instance;

            if (!ma.EnableMusicDucking || !ma.duckingBySoundType.ContainsKey(soundGroupName) || aSource.clip == null) {
                return;
            }

            var matchingDuck = ma.duckingBySoundType[soundGroupName];

            // duck music
            var duckLength = aSource.clip.length;
            var duckPitch = aSource.pitch;

            var pcs = PlaylistController.Instances;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < pcs.Count; i++) {
                pcs[i].DuckMusicForTime(duckLength, matchingDuck.unduckTime, duckPitch, matchingDuck.riseVolStart, matchingDuck.duckedVolumeCut);
            }
        }
        /*! \endcond */

        private static void StopPauseOrUnpauseSoundsOfTransform(Transform trans, List<AudioInfo> varList,
            VariationCommand varCmd) {
            MasterAudioGroup grp = null;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var v = 0; v < varList.Count; v++) {
                var variation = varList[v].Variation;
                if (!variation.WasTriggeredFromTransform(trans)) {
                    continue;
                }

                if (grp == null) {
                    var sType = variation.ParentGroup.GameObjectName;
                    grp = GrabGroup(sType);
                }

                var stopEndDetector = grp != null && grp.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain;

                // matched, stop or pause the audio.
                switch (varCmd) {
                    case VariationCommand.Stop:
                        variation.Stop(stopEndDetector);
                        break;
                    case VariationCommand.Pause:
                        variation.Pause();
                        break;
                    case VariationCommand.Unpause:
                        if (AudioUtil.IsAudioPaused(variation.VarAudio)) {
                            variation.VarAudio.Play();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// This method allows you to abruptly stop all sounds triggered by or following a Transform.
        /// </summary>
        /// <param name="trans">The Transform the sound was triggered to follow or use the position of.</param>
        public static void StopAllSoundsOfTransform(Transform trans) {
            if (!SceneHasMasterAudio) {
                // No MA
                return;
            }

            foreach (var key in AudioSourcesBySoundType.Keys) {
                var varList = AudioSourcesBySoundType[key].Sources;
                StopPauseOrUnpauseSoundsOfTransform(trans, varList, VariationCommand.Stop);
            }
        }

        /// <summary>
        /// This method allows you to abruptly stop all sounds of a particular Sound Group triggered by or following a Transform.
        /// </summary>
        /// <param name="trans">The Transform the sound was triggered to follow or use the position of.</param>
        /// <param name="sType">The name of the Sound Group to stop.</param>
        public static void StopSoundGroupOfTransform(Transform trans, string sType) {
            if (!SceneHasMasterAudio) {
                // No MA
                return;
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                Debug.LogWarning("Could not locate group '" + sType + "'.");
                return;
            }

            var varList = AudioSourcesBySoundType[sType].Sources;
            StopPauseOrUnpauseSoundsOfTransform(trans, varList, VariationCommand.Stop);
        }

        /// <summary>
        /// This method allows you to pause all sounds triggered by or following a Transform.
        /// </summary>
        /// <param name="trans">The Transform the sound was triggered to follow or use the position of.</param>
        public static void PauseAllSoundsOfTransform(Transform trans) {
            if (!SceneHasMasterAudio) {
                // No MA
                return;
            }

            foreach (var key in AudioSourcesBySoundType.Keys) {
                var varList = AudioSourcesBySoundType[key].Sources;
                StopPauseOrUnpauseSoundsOfTransform(trans, varList, VariationCommand.Pause);
            }
        }

        /// <summary>
        /// This method allows you to pause all sounds of a particular Sound Group triggered by or following a Transform.
        /// </summary>
        /// <param name="trans">The Transform the sound was triggered to follow or use the position of.</param>
        /// <param name="sType">The name of the Sound Group to stop.</param>
        public static void PauseSoundGroupOfTransform(Transform trans, string sType) {
            if (!SceneHasMasterAudio) {
                // No MA
                return;
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                Debug.LogWarning("Could not locate group '" + sType + "'.");
                return;
            }

            var varList = AudioSourcesBySoundType[sType].Sources;
            StopPauseOrUnpauseSoundsOfTransform(trans, varList, VariationCommand.Pause);
        }

        /// <summary>
        /// This method allows you to unpause all sounds triggered by or following a Transform.
        /// </summary>
        /// <param name="trans">The Transform the sound was triggered to follow or use the position of.</param>
        public static void UnpauseAllSoundsOfTransform(Transform trans) {
            if (!SceneHasMasterAudio) {
                // No MA
                return;
            }

            foreach (var key in AudioSourcesBySoundType.Keys) {
                var varList = AudioSourcesBySoundType[key].Sources;
                StopPauseOrUnpauseSoundsOfTransform(trans, varList, VariationCommand.Unpause);
            }
        }

        /// <summary>
        /// This method allows you to unpause all sounds of a particular Sound Group triggered by or following a Transform.
        /// </summary>
        /// <param name="trans">The Transform the sound was triggered to follow or use the position of.</param>
        /// <param name="sType">The name of the Sound Group to stop.</param>
        public static void UnpauseSoundGroupOfTransform(Transform trans, string sType) {
            if (!SceneHasMasterAudio) {
                // No MA
                return;
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                Debug.LogWarning("Could not locate group '" + sType + "'.");
                return;
            }

            var varList = AudioSourcesBySoundType[sType].Sources;
            StopPauseOrUnpauseSoundsOfTransform(trans, varList, VariationCommand.Unpause);
        }

        /// <summary>
        /// This method allows you to fade out all sounds of a particular Sound Group triggered by or following a Transform for X seconds.
        /// </summary>
        /// <param name="trans">The Transform the sound was triggered to follow or use the position of.</param>
        /// <param name="sType">The name of the Sound Group.</param>
        /// <param name="fadeTime">The amount of seconds the fading will take.</param>
        public static void FadeOutSoundGroupOfTransform(Transform trans, string sType, float fadeTime) {
            if (!SceneHasMasterAudio) {
                // No MA
                return;
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                Debug.LogWarning("Could not locate group '" + sType + "'.");
                return;
            }

            var varList = AudioSourcesBySoundType[sType].Sources;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var v = 0; v < varList.Count; v++) {
                var variation = varList[v].Variation;
                if (!variation.WasTriggeredFromTransform(trans)) {
                    continue;
                }
                variation.FadeOutNow(fadeTime);
            }
        }

        /// <summary>
        /// This method allows you to abruptly stop all sounds in a specified Sound Group.
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        public static void StopAllOfSound(string sType) {
            if (!SceneHasMasterAudio) {
                // No MA
                return;
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                Debug.LogWarning("Could not locate group '" + sType + "'.");
                return;
            }

            var sources = AudioSourcesBySoundType[sType].Sources;

            var grp = GrabGroup(sType);

            var stopEndDetector = grp != null && grp.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain;

            foreach (var audio in sources) {
                audio.Variation.Stop(stopEndDetector);
            }
        }

        /// <summary>
        /// This method allows you to fade out all sounds in a specified Sound Group for X seconds.
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        /// <param name="fadeTime">The amount of seconds the fading will take.</param>
        public static void FadeOutAllOfSound(string sType, float fadeTime) {
            if (!SceneHasMasterAudio) {
                // No MA
                return;
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                Debug.LogWarning("Could not locate group '" + sType + "'.");
                return;
            }

            var sources = AudioSourcesBySoundType[sType].Sources;

            foreach (var audio in sources) {
                audio.Variation.FadeOutNow(fadeTime);
            }
        }

        #endregion

        #region Variation methods

        /// <summary>
        /// Returns a list of all Variation scripts that are currently playing a sound.
        /// </summary>
        /// <returns>List of SoundGroupVariation</returns>
        public static List<SoundGroupVariation> GetAllPlayingVariations() {
            var playingVars = new List<SoundGroupVariation>(32);

            foreach (var key in AudioSourcesBySoundType.Keys) {
                var varList = AudioSourcesBySoundType[key].Sources;

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < varList.Count; i++) {
                    var aVar = varList[i].Variation;
                    if (!aVar.IsPlaying) {
                        continue;
                    }

                    playingVars.Add(aVar);
                }
            }

            return playingVars;
        }

        /// <summary>
        /// Returns a list of all Variation scripts that are currently playing through a bus.
        /// </summary>
        /// <param name="busName">The name of the bus to query.</param>
        /// <returns>List of SoundGroupVariation</returns>
        public static List<SoundGroupVariation> GetAllPlayingVariationsInBus(string busName) {
            var playingVars = new List<SoundGroupVariation>(32);

            var busIndex = GetBusIndex(busName, false);

            if (busIndex < 0) {
                return playingVars;
            }

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < aInfo.Sources.Count; i++) {
                    var aVar = aInfo.Sources[i].Variation;
                    if (!aVar.IsPlaying) {
                        continue;
                    }

                    playingVars.Add(aVar);
                }
            }

            return playingVars;
        }

        /// <summary>
        /// This method will add the variation to a Sound Group during runtime.
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        /// <param name="clip">The Audio Clip of the variation.</param>
        /// <param name="variationName">Use this to specify a the variation's name.</param>
        /// <param name="volume">Use this to specify a the variation's volume.</param>
        /// <param name="pitch">Use this to specify a the variation's pitch.</param>
        public static void CreateGroupVariationFromClip(string sType, AudioClip clip, string variationName,
            float volume = 1f, float pitch = 1f) {
            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot create change variation clip yet.");
                return;
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                Debug.LogWarning("Could not locate group '" + sType + "'.");
                return;
            }

            var grp = AudioSourcesBySoundType[sType];

            var matchingNameFound = false;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < grp.Sources.Count; i++) {
                var aVar = grp.Sources[i];
                if (aVar.Variation.name != variationName) {
                    continue;
                }
                matchingNameFound = true;
                break;
            }

            if (matchingNameFound) {
                LogWarning("You already have a Variation for this Group named '" + variationName +
                           "'. \n\nPlease rename these Variations when finished to be unique, or you may not be able to play them by name if you have a need to.");
            }

            // ReSharper disable once ArrangeStaticMemberQualifier
            var newVar =
                (GameObject)
                    GameObject.Instantiate(Instance.soundGroupVariationTemplate.gameObject, grp.Group.transform.position,
                        Quaternion.identity);

            newVar.transform.name = variationName;
            newVar.transform.parent = grp.Group.transform;

            var audSrc = newVar.GetComponent<AudioSource>();
            audSrc.clip = clip;
            audSrc.pitch = pitch;

            var newVariation = newVar.GetComponent<SoundGroupVariation>();
            newVariation.DisableUpdater();

            var newInfo = new AudioInfo(newVariation, newVariation.VarAudio, volume);

            grp.Sources.Add(newInfo);

            if (!_randomizer.ContainsKey(sType)) {
                return; // sanity check
            }

            _randomizer[sType].Add(grp.Sources.Count - 1);
            _randomizerLeftovers[sType].Add(grp.Sources.Count - 1);
        }


        /// <summary>
        /// This method will change the pitch of a variation or all variations in a Sound Group.
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        /// <param name="changeAllVariations">Whether to change all variations in the Sound Group or just one.</param>
        /// <param name="variationName">Use this to specify a certain variation's name. Only that variation will be changes if you haven't passed changeAllVariations as true.</param>
        /// <param name="pitch">The new pitch of the variation.</param>
        public static void ChangeVariationPitch(string sType, bool changeAllVariations, string variationName,
            float pitch) {
            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot change variation clip yet.");
                return;
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                Debug.LogWarning("Could not locate group '" + sType + "'.");
                return;
            }

            var grp = AudioSourcesBySoundType[sType];

            var iChanged = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < grp.Sources.Count; i++) {
                var aVar = grp.Sources[i];
                if (!changeAllVariations && aVar.Source.transform.name != variationName) {
                    continue;
                }
                aVar.Variation.original_pitch = pitch;
                var aud = aVar.Variation.VarAudio;
                if (aud != null) {
                    aud.pitch = pitch;
                }
                iChanged++;
            }

            if (iChanged == 0 && !changeAllVariations) {
                Debug.Log("Could not find any matching variations of Sound Group '" + sType +
                          "' to change the pitch of.");
            }
        }

        /// <summary>
        /// This method will change the volume of a variation or all variations in a Sound Group.
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        /// <param name="changeAllVariations">Whether to change all variations in the Sound Group or just one.</param>
        /// <param name="variationName">Use this to specify a certain variation's name. Only that variation will be changes if you haven't passed changeAllVariations as true.</param>
        /// <param name="volume">The new volume of the variation.</param>
        public static void ChangeVariationVolume(string sType, bool changeAllVariations, string variationName,
            float volume) {
            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot change variation clip yet.");
                return;
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                Debug.LogWarning("Could not locate group '" + sType + "'.");
                return;
            }

            var grp = AudioSourcesBySoundType[sType];

            var iChanged = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < grp.Sources.Count; i++) {
                var aVar = grp.Sources[i];
                if (!changeAllVariations && aVar.Source.transform.name != variationName) {
                    continue;
                }
                aVar.OriginalVolume = volume;
                iChanged++;
            }

            if (iChanged == 0 && !changeAllVariations) {
                Debug.Log("Could not find any matching variations of Sound Group '" + sType +
                          "' to change the volume of.");
            }
        }

        /// <summary>
        /// This method will change the Audio Clip used by a variation into one named from a Resource file.
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        /// <param name="changeAllVariations">Whether to change all variations in the Sound Group or just one.</param>
        /// <param name="variationName">Use this to specify a certain variation's name. Only that variation will be changes if you haven't passed changeAllVariations as true.</param>
        /// <param name="resourceFileName">The name of the file in the Resource.</param>
        public static void ChangeVariationClipFromResources(string sType, bool changeAllVariations, string variationName,
            string resourceFileName) {
            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot create change variation clip yet.");
                return;
            }

            var aClip = Resources.Load(resourceFileName) as AudioClip;

            if (aClip == null) {
                LogWarning("Resource file '" + resourceFileName + "' could not be located.");
                return;
            }

            ChangeVariationClip(sType, changeAllVariations, variationName, aClip);
        }

        /// <summary>
        /// This method will change the Audio Clip used by a variation into one you specify.
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        /// <param name="changeAllVariations">Whether to change all variations in the Sound Group or just one.</param>
        /// <param name="variationName">Use this to specify a certain variation's name. Only that variation will be changes if you haven't passed changeAllVariations as true.</param>
        /// <param name="clip">The Audio Clip to replace the old one with.</param>
        public static void ChangeVariationClip(string sType, bool changeAllVariations, string variationName,
            AudioClip clip) {
            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot create change variation clip yet.");
                return;
            }

            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                Debug.LogWarning("Could not locate group '" + sType + "'.");
                return;
            }

            var grp = AudioSourcesBySoundType[sType];

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < grp.Sources.Count; i++) {
                var aVar = grp.Sources[i];
                if (changeAllVariations || aVar.Source.transform.name == variationName) {
                    aVar.Source.clip = clip;
                }
            }
        }

        #endregion

        #region Sound Group methods

        /// <summary>
        /// Change the bus of a Sound Group.
        /// </summary>
        /// <param name="sType">Sound Group name</param>
        /// <param name="busName">The new bus name. Use null for "route to [No Bus]"</param>
        public static void RouteGroupToBus(string sType, string busName) {
            var grp = GrabGroup(sType);

            if (grp == null) {
                LogError("Could not find Sound Group '" + sType + "'");
                return;
            }

            var newBusIndex = 0;

            if (busName != null) {
                var busIndex = GroupBuses.FindIndex(x => x.busName == busName);
                if (busIndex < 0) {
                    LogError("Could not find bus '" + busName + "' to assign to Sound Group '" + sType + "'");
                    return;
                }

                newBusIndex = HardCodedBusOptions + busIndex;
            }

            var oldBus = GetBusByIndex(grp.busIndex);

            grp.busIndex = newBusIndex;
            GroupBus newBus = null;

            if (newBusIndex > 0) {
                newBus = GroupBuses.Find(x => x.busName == busName);
                if (newBus.isMuted) {
                    MuteGroup(grp.name);
                } else if (newBus.isSoloed) {
                    SoloGroup(grp.name);
                }
            }

            var hasVoicesPlaying = false;

            // update active voice count on the new and old bus.
            var sources = AudioSourcesBySoundType[sType].Sources;
            for (var i = 0; i < sources.Count; i++) {
                var aVar = sources[i].Variation;

#if UNITY_5
                aVar.SetMixerGroup();
                aVar.SetSpatialBlend(); // set the spatial blend, including any overrides in the bus.
#endif
                if (!aVar.IsPlaying) {
                    continue;
                }

                if (newBus != null) { // could be "no bus"
                    newBus.AddActiveAudioSourceId(aVar.InstanceId);
                }

                if (oldBus != null) { // could be "no bus"
                    oldBus.RemoveActiveAudioSourceId(aVar.InstanceId);
                }
                hasVoicesPlaying = true;
            }

            if (hasVoicesPlaying) { // update the moved Variations to use new bus volume
                SetBusVolume(newBus, newBus != null ? newBus.volume : 0);
            }
        }

        /// <summary>
        /// This method will return the length in seconds of a Variation in a Sound Group.
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        /// <param name="variationName">Use this to specify a certain variation's name. The first match will be used</param>
        /// <returns>The time length of the Variation, taking pitch into account. If it cannot find the Variation, it returns -1 and logs the reason to the console.</returns>
        public static float GetVariationLength(string sType, string variationName) {
            var grp = GrabGroup(sType);
            if (grp == null) {
                return -1f;
            }

            SoundGroupVariation match = null;

            foreach (var sgv in grp.groupVariations) {
                if (sgv.name != variationName) {
                    continue;
                }
                match = sgv;
                break;
            }

            if (match == null) {
                LogError("Could not find Variation '" + variationName + "' in Sound Group '" + sType + "'.");
                return -1f;
            }

            if (match.audLocation == AudioLocation.ResourceFile) {
                LogError("Variation '" + variationName + "' in Sound Group '" + sType +
                         "' length cannot be determined because it's a Resource Files.");
                return -1f;
            }

            if (match.audLocation == AudioLocation.FileOnInternet) {
                LogError("Variation '" + variationName + "' in Sound Group '" + sType +
                         "' length cannot be determined because it's an Internet File.");
                return -1f;
            }

            var clip = match.VarAudio.clip;
            if (clip == null) {
                LogError("Variation '" + variationName + "' in Sound Group '" + sType + "' has no Audio Clip.");
                return -1f;
            }

            if (!(match.VarAudio.pitch <= 0f)) {
                return clip.length / match.VarAudio.pitch;
            }

            LogError("Variation '" + variationName + "' in Sound Group '" + sType +
                     "' has negative or zero pitch. Cannot compute length.");
            return -1f;
        }

        /// <summary>
        /// This method allows you to refill the pool of the Variation sounds for a Sound Group. That way you don't have to wait for all remaining random (or top to bottom) sounds to be played before it refills.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to refill the pool of.</param>
        public static void RefillSoundGroupPool(string sType) {
            var grp = GrabGroup(sType, false);
            if (grp == null) {
                return;
            }

            var choices = _randomizer[sType];

            var played = _clipsPlayedBySoundTypeOldestFirst[sType];
            if (choices.Count > 0) {
                played.AddRange(choices); // add any not played yet.
                choices.Clear();
            }

            if (grp.curVariationSequence == MasterAudioGroup.VariationSequence.Randomized) {
                int? lastIndexPlayed = null;

                if (grp.UsesNoRepeat) {
                    if (played.Count > 0) {
                        lastIndexPlayed = played[played.Count - 1];
                    }
                }

                ArrayListUtil.SortIntArray(ref played);

                if (lastIndexPlayed.HasValue && lastIndexPlayed.Value == played[0]) {
                    // would be a repeat of the last random choice! exchange
                    var firstIndex = played[0];
                    played.RemoveAt(0);
                    played.Insert(UnityEngine.Random.Range(1, played.Count), firstIndex);
                }
            }

            choices.AddRange(played);
            // refill leftovers pool.
            _randomizerLeftovers[sType].AddRange(played);

            played.Clear();

            if (grp.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain) {
                grp.ChainLoopCount++;
            }
        }

        /// <summary>
        /// This method allows you to check if a Sound Group exists.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to check.</param>
        /// <returns>Whether or not the Sound Group exists.</returns>
        public static bool SoundGroupExists(string sType) {
            var aGroup = GrabGroup(sType, false);
            return aGroup != null;
        }

        /// <summary>
        /// This method allows you to pause all Audio Sources in a Sound Group.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to pause.</param>
        public static void PauseSoundGroup(string sType) {
            var aGroup = GrabGroup(sType);

            if (aGroup == null) {
                return;
            }

            // ReSharper disable once TooWideLocalVariableScope
            SoundGroupVariation aVar;

            var sources = AudioSourcesBySoundType[sType].Sources;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < sources.Count; i++) {
                aVar = sources[i].Variation;

                aVar.Pause();
            }
        }

#if UNITY_5
        public static void SetGroupSpatialBlend(string sType) {
            var aGroup = GrabGroup(sType);

            if (aGroup == null) {
                return;
            }

            // ReSharper disable once TooWideLocalVariableScope
            SoundGroupVariation aVar;

            var sources = AudioSourcesBySoundType[sType].Sources;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < sources.Count; i++) {
                aVar = sources[i].Variation;

                aVar.SetSpatialBlend();
            }
        }

        public static void RouteGroupToUnityMixerGroup(string sType, AudioMixerGroup mixerGroup) {
            if (!Application.isPlaying) {
                return;
            }

            var aGroup = GrabGroup(sType, false);

            if (aGroup == null) {
                return;
            }

            // ReSharper disable once TooWideLocalVariableScope
            SoundGroupVariation aVar;

            var sources = AudioSourcesBySoundType[sType].Sources;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < sources.Count; i++) {
                aVar = sources[i].Variation;
                aVar.VarAudio.outputAudioMixerGroup = mixerGroup;
            }
        }
#endif

        /// <summary>
        /// This method allows you to unpause all Audio Sources in a Sound Group.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to unpause.</param>
        public static void UnpauseSoundGroup(string sType) {
            var aGroup = GrabGroup(sType);

            if (aGroup == null) {
                return;
            }

            // ReSharper disable once TooWideLocalVariableScope
            SoundGroupVariation aVar;

            var sources = AudioSourcesBySoundType[sType].Sources;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < sources.Count; i++) {
                aVar = sources[i].Variation;

                if (!AudioUtil.IsAudioPaused(aVar.VarAudio)) {
                    continue;
                }

                aVar.VarAudio.Play();
            }
        }

        /// <summary>
        /// This method allows you to fade the volume of a Sound Group over X seconds.
        /// </summary>
        /// <param name="sType">The name of the Sound Group to fade.</param>
        /// <param name="newVolume">The target volume of the Sound Group.</param>
        /// <param name="fadeTime">The amount of time the fade will take.</param>
        /// <param name="completionCallback">(Optional) - a method to execute when the fade has completed.</param>
        // ReSharper disable once RedundantNameQualifier
        public static void FadeSoundGroupToVolume(string sType, float newVolume, float fadeTime,
            // ReSharper disable once RedundantNameQualifier
            System.Action completionCallback = null) {
            if (newVolume < 0f || newVolume > 1f) {
                Debug.LogError("Illegal volume passed to FadeSoundGroupToVolume: '" + newVolume +
                               "'. Legal volumes are between 0 and 1");
                return;
            }

            if (fadeTime <= InnerLoopCheckInterval) {
                SetGroupVolume(sType, newVolume); // time really short, just do it at once.

                if (completionCallback != null) {
                    completionCallback();
                }

                return;
            }

            var aGroup = GrabGroup(sType);

            if (aGroup == null) {
                return;
            }

            if (newVolume < 0f || newVolume > 1f) {
                Debug.Log("Cannot fade Sound Group '" + sType +
                          "'. Invalid volume specified. Volume should be between 0 and 1.");
                return;
            }

            // make sure no other group fades for this group are happenning.
            var matchingFade = GroupFades.Find(delegate(GroupFadeInfo obj) {
                return obj.NameOfGroup == sType;
            });

            if (matchingFade != null) {
                matchingFade.IsActive = false; // start with a new one, delete old.
            }

            var volStep = (newVolume - aGroup.groupMasterVolume) / (fadeTime / AudioUtil.FrameTime);

            var groupFade = new GroupFadeInfo() {
                NameOfGroup = sType,
                ActingGroup = aGroup,
                VolumeStep = volStep,
                TargetVolume = newVolume
            };

            if (completionCallback != null) {
                groupFade.completionAction = completionCallback;
            }

            GroupFades.Add(groupFade);
        }

        /// <summary>
        /// This method will delete a Sound Group, and all variations from the current Scene's Master Audio object. 
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        public static void DeleteSoundGroup(string sType) {
            if (SafeInstance == null) {
                return;
            }

            var grp = GrabGroup(sType);
            if (grp == null) {
                return;
            }

            StopAllOfSound(sType); // unload Resources if any.

            var groupTrans = grp.transform;

            var ma = Instance;

            if (ma.duckingBySoundType.ContainsKey(sType)) {
                ma.duckingBySoundType.Remove(sType);
            }

            _randomizer.Remove(sType);
            _randomizerLeftovers.Remove(sType);
            _clipsPlayedBySoundTypeOldestFirst.Remove(sType);
            RemoveRuntimeGroupInfo(sType);
            LastTimeSoundGroupPlayed.Remove(sType);

            // ReSharper disable TooWideLocalVariableScope
            AudioSource aSource;
            SoundGroupVariation aVar;
            Transform aChild;
            // ReSharper restore TooWideLocalVariableScope

            // delete resource file pointers to Audio Sources being deleted
            for (var i = 0; i < groupTrans.childCount; i++) {
                aChild = groupTrans.GetChild(i);
                aSource = aChild.GetComponent<AudioSource>();
                aVar = aChild.GetComponent<SoundGroupVariation>();

                switch (aVar.audLocation) {
                    case AudioLocation.ResourceFile:
                        AudioResourceOptimizer.DeleteAudioSourceFromList(aVar.resourceFileName, aSource);
                        break;
                    case AudioLocation.FileOnInternet:
                        AudioResourceOptimizer.DeleteAudioSourceFromList(aVar.internetFileUrl, aSource);
                        break;
                }

            }

            groupTrans.parent = null;
            // ReSharper disable once ArrangeStaticMemberQualifier
            GameObject.Destroy(groupTrans.gameObject);
        }

        /// <summary>
        /// This method will create a new Sound Group from the Audio Clips you pass in.
        /// </summary>
        /// <param name="aGroup">The object containing all variations and group info.</param>
        /// <param name="creatorObjectName">The name of the object creating this group (for debug).</param>
        /// <param name="errorOnExisting">Whether to log an error if the Group already exists (same name).</param>
        /// <returns>Whether or not the Sound Group was created.</returns>
        public static Transform CreateSoundGroup(DynamicSoundGroup aGroup, string creatorObjectName,
            bool errorOnExisting = true) {
            if (!SceneHasMasterAudio) {
                return null;
            }

            if (!SoundsReady) {
                Debug.LogError("MasterAudio not finished initializing sounds. Cannot create new group yet.");
                return null;
            }

            var groupName = aGroup.transform.name;

            var ma = Instance;

            if (Instance.Trans.FindChild(groupName) != null) {
                if (errorOnExisting) {
                    Debug.LogError("Cannot add a new Sound Group named '" + groupName +
                                   "' because there is already a Sound Group of that name.");
                }
                return null;
            }

            // ReSharper disable once ArrangeStaticMemberQualifier
            var newGroup = (GameObject)GameObject.Instantiate(
                ma.soundGroupTemplate.gameObject, ma.Trans.position, Quaternion.identity);

            var groupTrans = newGroup.transform;
            groupTrans.name = UtilStrings.TrimSpace(groupName);
            groupTrans.parent = Instance.Trans;

            SoundGroupVariation variation;
            // ReSharper disable TooWideLocalVariableScope
            DynamicGroupVariation aVariation;
            AudioClip clip;
            // ReSharper restore TooWideLocalVariableScope

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < aGroup.groupVariations.Count; i++) {
                aVariation = aGroup.groupVariations[i];

                for (var j = 0; j < aVariation.weight; j++) {
                    // ReSharper disable once ArrangeStaticMemberQualifier
                    var newVariation = (GameObject)GameObject.Instantiate(aVariation.gameObject, groupTrans.position, Quaternion.identity);
                    newVariation.transform.parent = groupTrans;

                    // remove dynamic group variation script.
                    // ReSharper disable once ArrangeStaticMemberQualifier
                    GameObject.Destroy(newVariation.GetComponent<DynamicGroupVariation>());

                    newVariation.AddComponent<SoundGroupVariation>();
                    variation = newVariation.GetComponent<SoundGroupVariation>();

                    var clipName = variation.name;
                    // ReSharper disable once StringIndexOfIsCultureSpecific.1
                    var cloneIndex = clipName.IndexOf("(Clone)");
                    if (cloneIndex >= 0) {
                        clipName = clipName.Substring(0, cloneIndex);
                    }

                    var aVarAudio = aVariation.GetComponent<AudioSource>();

                    switch (aVariation.audLocation) {
                        case AudioLocation.Clip:
                            clip = aVarAudio.clip;
                            variation.VarAudio.clip = clip;
                            break;
                        case AudioLocation.ResourceFile:
                            var resourceFileName = AudioResourceOptimizer.GetLocalizedFileName(aVariation.useLocalization, aVariation.resourceFileName);
                            AudioResourceOptimizer.AddTargetForClip(resourceFileName, variation.VarAudio);
                            variation.resourceFileName = aVariation.resourceFileName;
                            variation.useLocalization = aVariation.useLocalization;
                            break;
                        case AudioLocation.FileOnInternet:
                            AudioResourceOptimizer.AddTargetForClip(aVariation.internetFileUrl, variation.VarAudio);
                            variation.internetFileUrl = aVariation.internetFileUrl;
                            break;
                    }

                    variation.audLocation = aVariation.audLocation;

                    variation.original_pitch = aVarAudio.pitch;
                    variation.transform.name = clipName;
                    variation.isExpanded = aVariation.isExpanded;

                    variation.useRandomPitch = aVariation.useRandomPitch;
                    variation.randomPitchMode = aVariation.randomPitchMode;
                    variation.randomPitchMin = aVariation.randomPitchMin;
                    variation.randomPitchMax = aVariation.randomPitchMax;

                    variation.useRandomVolume = aVariation.useRandomVolume;
                    variation.randomVolumeMode = aVariation.randomVolumeMode;
                    variation.randomVolumeMin = aVariation.randomVolumeMin;
                    variation.randomVolumeMax = aVariation.randomVolumeMax;

                    variation.useFades = aVariation.useFades;
                    variation.fadeInTime = aVariation.fadeInTime;
                    variation.fadeOutTime = aVariation.fadeOutTime;

                    variation.useIntroSilence = aVariation.useIntroSilence;
                    variation.introSilenceMin = aVariation.introSilenceMin;
                    variation.introSilenceMax = aVariation.introSilenceMax;
                    variation.fxTailTime = aVariation.fxTailTime;

                    variation.useRandomStartTime = aVariation.useRandomStartTime;
                    variation.randomStartMinPercent = aVariation.randomStartMinPercent;
                    variation.randomStartMaxPercent = aVariation.randomStartMaxPercent;

                    // remove unused filter FX
                    if (variation.LowPassFilter != null && !variation.LowPassFilter.enabled) {
                        // ReSharper disable once ArrangeStaticMemberQualifier
                        GameObject.Destroy(variation.LowPassFilter);
                    }
                    if (variation.HighPassFilter != null && !variation.HighPassFilter.enabled) {
                        // ReSharper disable once ArrangeStaticMemberQualifier
                        GameObject.Destroy(variation.HighPassFilter);
                    }
                    if (variation.DistortionFilter != null && !variation.DistortionFilter.enabled) {
                        // ReSharper disable once ArrangeStaticMemberQualifier
                        GameObject.Destroy(variation.DistortionFilter);
                    }
                    if (variation.ChorusFilter != null && !variation.ChorusFilter.enabled) {
                        // ReSharper disable once ArrangeStaticMemberQualifier
                        GameObject.Destroy(variation.ChorusFilter);
                    }
                    if (variation.EchoFilter != null && !variation.EchoFilter.enabled) {
                        // ReSharper disable once ArrangeStaticMemberQualifier
                        GameObject.Destroy(variation.EchoFilter);
                    }
                    if (variation.ReverbFilter != null && !variation.ReverbFilter.enabled) {
                        // ReSharper disable once ArrangeStaticMemberQualifier
                        GameObject.Destroy(variation.ReverbFilter);
                    }
                }
            }
            // added to Hierarchy!

            // populate sounds for playing!
            var groupScript = newGroup.GetComponent<MasterAudioGroup>();
            // populate other properties.
            groupScript.retriggerPercentage = aGroup.retriggerPercentage;

            var persistentGrpVol = PersistentAudioSettings.GetGroupVolume(aGroup.name);
            groupScript.OriginalVolume = aGroup.groupMasterVolume;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (persistentGrpVol.HasValue) {
                groupScript.groupMasterVolume = persistentGrpVol.Value;
            } else {
                groupScript.groupMasterVolume = aGroup.groupMasterVolume;
            }

            groupScript.limitMode = aGroup.limitMode;
            groupScript.limitPerXFrames = aGroup.limitPerXFrames;
            groupScript.minimumTimeBetween = aGroup.minimumTimeBetween;
            groupScript.limitPolyphony = aGroup.limitPolyphony;
            groupScript.voiceLimitCount = aGroup.voiceLimitCount;
            groupScript.curVariationSequence = aGroup.curVariationSequence;
            groupScript.useInactivePeriodPoolRefill = aGroup.useInactivePeriodPoolRefill;
            groupScript.inactivePeriodSeconds = aGroup.inactivePeriodSeconds;
            groupScript.curVariationMode = aGroup.curVariationMode;
            groupScript.useNoRepeatRefill = aGroup.useNoRepeatRefill;
            groupScript.useDialogFadeOut = aGroup.useDialogFadeOut;
            groupScript.dialogFadeOutTime = aGroup.dialogFadeOutTime;
            groupScript.isUsingOcclusion = aGroup.isUsingOcclusion;

            groupScript.chainLoopDelayMin = aGroup.chainLoopDelayMin;
            groupScript.chainLoopDelayMax = aGroup.chainLoopDelayMax;
            groupScript.chainLoopMode = aGroup.chainLoopMode;
            groupScript.chainLoopNumLoops = aGroup.chainLoopNumLoops;

            groupScript.childGroupMode = aGroup.childGroupMode;
            groupScript.childSoundGroups = aGroup.childSoundGroups;

            groupScript.soundPlayedEventActive = aGroup.soundPlayedEventActive;
            groupScript.soundPlayedCustomEvent = aGroup.soundPlayedCustomEvent;

            groupScript.targetDespawnedBehavior = aGroup.targetDespawnedBehavior;
            groupScript.despawnFadeTime = aGroup.despawnFadeTime;

            groupScript.resourceClipsAllLoadAsync = aGroup.resourceClipsAllLoadAsync;
            groupScript.logSound = aGroup.logSound;
            groupScript.alwaysHighestPriority = aGroup.alwaysHighestPriority;

#if UNITY_5
            groupScript.spatialBlendType = aGroup.spatialBlendType;
            groupScript.spatialBlend = aGroup.spatialBlend;
#endif

            var sources = new List<AudioInfo>();
            // ReSharper disable TooWideLocalVariableScope
            Transform aChild;
            AudioSource aSource;
            // ReSharper restore TooWideLocalVariableScope

            var playedStatuses = new List<int>();

            for (var i = 0; i < newGroup.transform.childCount; i++) {
                playedStatuses.Add(i);
                aChild = newGroup.transform.GetChild(i);
                aSource = aChild.GetComponent<AudioSource>();
                variation = aChild.GetComponent<SoundGroupVariation>();
                sources.Add(new AudioInfo(variation, aSource, aSource.volume));

                variation.DisableUpdater();
            }

            AddRuntimeGroupInfo(groupName, new AudioGroupInfo(sources, groupScript));

            if (groupScript.curVariationSequence == MasterAudioGroup.VariationSequence.Randomized) {
                ArrayListUtil.SortIntArray(ref playedStatuses);
            }

            // fill up randomizer
            _randomizer.Add(groupName, playedStatuses);
            _randomizerLeftovers.Add(groupName, new List<int>(playedStatuses.Count));
            // fill leftovers
            _randomizerLeftovers[groupName].AddRange(playedStatuses);
            _clipsPlayedBySoundTypeOldestFirst.Add(groupName, new List<int>(playedStatuses.Count));

            if (string.IsNullOrEmpty(aGroup.busName)) {
                return groupTrans;
            }

            groupScript.busIndex = GetBusIndex(aGroup.busName, true);
            if (groupScript.BusForGroup != null && groupScript.BusForGroup.isMuted) {
                MuteGroup(groupScript.name);
            }

            return groupTrans;
        }

        /// <summary>
        /// This will return the volume of a Sound Group.
        /// </summary>
        /// <param name="sType">The name of the Sound Group</param>
        /// <returns>The volume of the Sound Group</returns>
        public static float GetGroupVolume(string sType) {
            var aGroup = GrabGroup(sType);
            if (aGroup == null) {
                return 0f;
            }

            return aGroup.groupMasterVolume;
        }

        /// <summary>
        /// This method will set the volume of a Sound Group.
        /// </summary>
        /// <param name="sType">The name of the Sound Group</param>
        /// <param name="volumeLevel">The new volume level.</param>
        public static void SetGroupVolume(string sType, float volumeLevel) {
            var aGroup = GrabGroup(sType, Application.isPlaying);
            if (aGroup == null || AppIsShuttingDown) {
                return;
            }

            aGroup.groupMasterVolume = volumeLevel;

            // ReSharper disable TooWideLocalVariableScope
            AudioInfo aInfo;
            AudioSource aSource;
            // ReSharper restore TooWideLocalVariableScope

            var theGroup = AudioSourcesBySoundType[sType];

            var busVolume = GetBusVolume(aGroup);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < theGroup.Sources.Count; i++) {
                aInfo = theGroup.Sources[i];
                aSource = aInfo.Source;

                if (aSource == null) {
                    continue;
                }

                float newVol;
                if (aInfo.Variation.randomVolumeMode == SoundGroupVariation.RandomVolumeMode.AddToClipVolume) {
                    newVol = (aInfo.OriginalVolume * aInfo.LastPercentageVolume * aGroup.groupMasterVolume * busVolume *
                              Instance._masterAudioVolume) + aInfo.LastRandomVolume;
                } else {
                    // ignore original volume
                    newVol = (aInfo.OriginalVolume * aInfo.LastPercentageVolume * aGroup.groupMasterVolume * busVolume *
                              Instance._masterAudioVolume);
                }
                aSource.volume = newVol;
            }
        }

        /// <summary>
        /// This method will mute all variations in a Sound Group.
        /// </summary>
        /// <param name="sType">The name of the Sound Group</param>
        public static void MuteGroup(string sType) {
            var aGroup = GrabGroup(sType);
            if (aGroup == null) {
                return;
            }

            SoloedGroups.Remove(aGroup);
            aGroup.isSoloed = false;

            SetGroupMuteStatus(aGroup, sType, true);
        }

        /// <summary>
        /// This method will unmute all variations in a Sound Group
        /// </summary>
        /// <param name="sType">The name of the Sound Group</param>
        public static void UnmuteGroup(string sType) {
            var aGroup = GrabGroup(sType);
            if (aGroup == null) {
                return;
            }

            SetGroupMuteStatus(aGroup, sType, false);
        }

        private static void AddRuntimeGroupInfo(string groupName, AudioGroupInfo groupInfo) {
            AudioSourcesBySoundType.Add(groupName, groupInfo);

            var sources = new List<AudioSource>(groupInfo.Sources.Count);
            // ReSharper disable ForCanBeConvertedToForeach
            for (var i = 0; i < groupInfo.Sources.Count; i++) {
                // ReSharper restore ForCanBeConvertedToForeach
                sources.Add(groupInfo.Sources[i].Source);
            }

            TrackRuntimeAudioSources(sources);
        }

        /*! \cond PRIVATE */

        private static void FireAudioSourcesNumberChangedEvent() {
            if (NumberOfAudioSourcesChanged != null) {
                NumberOfAudioSourcesChanged();
            }
        }

        /// <summary>
        /// This method is used internally by Master Audio. You should never need to call them.
        /// </summary>
        /// <param name="sources"></param>
        public static void TrackRuntimeAudioSources(List<AudioSource> sources) {
            var wasListChanged = false;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < sources.Count; i++) {
                var src = sources[i];
                if (AllAudioSources.Contains(src)) {
                    continue;
                }

                AllAudioSources.Add(src);
                wasListChanged = true;
            }

            if (wasListChanged) {
                FireAudioSourcesNumberChangedEvent();
            }
        }

        /// <summary>
        /// This method is used internally by Master Audio. You should never need to call them.
        /// </summary>
        /// <param name="sources"></param>
        public static void StopTrackingRuntimeAudioSources(List<AudioSource> sources) {
            var wasListChanged = false;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < sources.Count; i++) {
                var src = sources[i];

                if (!AllAudioSources.Contains(src)) {
                    continue;
                }

                AllAudioSources.Remove(src);
                wasListChanged = true;
            }

            if (wasListChanged) {
                FireAudioSourcesNumberChangedEvent();
            }
        }

        private static void RemoveRuntimeGroupInfo(string groupName) {
            var groupInfo = GrabGroup(groupName);

            if (groupInfo != null) {
                // ReSharper disable once ForCanBeConvertedToForeach
                var sources = new List<AudioSource>(groupInfo.groupVariations.Count);

                for (var i = 0; i < groupInfo.groupVariations.Count; i++) {
                    sources.Add(groupInfo.groupVariations[i].VarAudio);
                }

                StopTrackingRuntimeAudioSources(sources);
            }

            AudioSourcesBySoundType.Remove(groupName);
        }

        /*! \endcond */

        private static void RescanChildren(MasterAudioGroup group) {
            var newChildren = new List<SoundGroupVariation>();

            var childNames = new List<string>();

            for (var i = 0; i < group.transform.childCount; i++) {
                var child = group.transform.GetChild(i);

                if (childNames.Contains(child.name)) {
                    continue;
                }

                childNames.Add(child.name);

                var variation = child.GetComponent<SoundGroupVariation>();

                newChildren.Add(variation);
            }

            group.groupVariations = newChildren;
        }

        private static void SetGroupMuteStatus(MasterAudioGroup aGroup, string sType, bool isMute) {
            aGroup.isMuted = isMute;

            var theGroup = AudioSourcesBySoundType[sType];
            // ReSharper disable TooWideLocalVariableScope
            AudioInfo aInfo;
            AudioSource aSource;
            // ReSharper restore TooWideLocalVariableScope

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < theGroup.Sources.Count; i++) {
                aInfo = theGroup.Sources[i];
                aSource = aInfo.Source;

                aSource.mute = isMute;
            }
        }

        /// <summary>
        /// This method will solo a Sound Group. If anything is soloed, only soloed Sound Groups will be heard.
        /// </summary>
        /// <param name="sType">The name of the Sound Group</param>
        public static void SoloGroup(string sType) {
            var aGroup = GrabGroup(sType);
            if (aGroup == null) {
                return;
            }

            aGroup.isMuted = false;
            aGroup.isSoloed = true;

            SoloedGroups.Add(aGroup);

            SetGroupMuteStatus(aGroup, sType, false);
        }

        /// <summary>
        /// This method will unsolo a Sound Group. 
        /// </summary>
        /// <param name="sType">The name of the Sound Group</param>
        public static void UnsoloGroup(string sType) {
            var aGroup = GrabGroup(sType);
            if (aGroup == null) {
                return;
            }

            aGroup.isSoloed = false;

            SoloedGroups.Remove(aGroup);
        }

        /// <summary>
        /// This method will return the Sound Group settings for examination purposes.
        /// </summary>
        /// <param name="sType">The name of the Sound Group</param>
        /// <param name="logIfMissing">Whether to log to the Console if Group cannot be found.</param>
        /// <returns>A MasterAudioGroup object</returns>
        public static MasterAudioGroup GrabGroup(string sType, bool logIfMissing = true) {
            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                if (logIfMissing) {
                    Debug.LogError("Could not grab Sound Group '" + sType + "' because it does not exist in this scene.");
                }
                return null;
            }

            var group = AudioSourcesBySoundType[sType];
            var maGroup = group.Group;

            if (maGroup.groupVariations.Count == 0) { // needed for Dynamic SGC's
                RescanChildren(maGroup);
            }

            return maGroup;
        }

        /*! \cond PRIVATE */
        /// <summary>
        /// Used by Inspectors to find the Sound Group so we can select it.
        /// </summary>
        /// <param name="sType">Name of Sound Group</param>
        /// <returns>Transform</returns>
        public static Transform FindGroupTransform(string sType) {
            var grp = Instance.Trans.FindChild(sType);
            if (grp != null) {
                return grp;
            }

            var dgscs = FindObjectsOfType<DynamicSoundGroupCreator>();
            for (var i = 0; i < dgscs.Count(); i++) {
                var d = dgscs[i];
                grp = d.transform.FindChild(sType);

                if (grp != null) {
                    return grp;
                }
            }

            return null;
        }

        /// <summary>
        /// This method will return all Variations of a Sound Group settings for examination purposes.
        /// </summary>
        /// <param name="sType">The name of the Sound Group</param>
        /// <param name="logIfMissing">Whether to log to the Console if Group cannot be found.</param>
        /// <returns>A list of Audio Info objects</returns>
        public static List<AudioInfo> GetAllVariationsOfGroup(string sType, bool logIfMissing = true) {
            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                if (logIfMissing) {
                    Debug.LogError("Could not grab Sound Group '" + sType + "' because it does not exist in this scene.");
                }
                return null;
            }

            var group = AudioSourcesBySoundType[sType];
            return group.Sources;
        }
        /*! \endcond */

        /// <summary>
        /// This method will return the Audio Group Info settings for examination purposes. Use on during play in editor, not during edit.
        /// </summary>
        /// <param name="sType">The name of the Sound Group</param>
        /// <returns>an Audio Group Info object</returns>
        public static AudioGroupInfo GetGroupInfo(string sType) {
            if (!AudioSourcesBySoundType.ContainsKey(sType)) {
                return null;
            }

            var group = AudioSourcesBySoundType[sType];
            return group;
        }

        #endregion

        #region Mixer methods

#if UNITY_5
        public void SetSpatialBlendForMixer() {
            foreach (var key in AudioSourcesBySoundType.Keys) {
                SetGroupSpatialBlend(key);
            }
        }
#endif

        /// <summary>
        /// This method allows you to pause all Audio Sources in the mixer (everything but Playlists).
        /// </summary>
        public static void PauseMixer() {
            foreach (var key in AudioSourcesBySoundType.Keys) {
                PauseSoundGroup(AudioSourcesBySoundType[key].Group.GameObjectName);
            }
        }

        /// <summary>
        /// This method allows you to unpause all Audio Sources in the mixer (everything but Playlists).
        /// </summary>
        public static void UnpauseMixer() {
            foreach (var key in AudioSourcesBySoundType.Keys) {
                UnpauseSoundGroup(AudioSourcesBySoundType[key].Group.GameObjectName);
            }
        }

        /// <summary>
        /// This method allows you to stop all Audio Sources in the mixer (everything but Playlists).
        /// </summary>
        public static void StopMixer() {
            foreach (var key in AudioSourcesBySoundType.Keys) {
                StopAllOfSound(AudioSourcesBySoundType[key].Group.GameObjectName);
            }
        }

        #endregion

        #region Global Controls

        /// <summary>
        /// This method allows you to unsubscribe from all SoundFinished events in the entire MA hierarchy in your Scene.
        /// </summary>
        public static void UnsubscribeFromAllVariations() {
            foreach (var key in AudioSourcesBySoundType.Keys) {
                var varList = AudioSourcesBySoundType[key].Sources;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < varList.Count; i++) {
                    varList[i].Variation.ClearSubscribers();
                }
            }
        }

        /// <summary>
        /// This method allows you to stop all Audio Sources in the mixer and Playlists as well.
        /// </summary>
        public static void StopEverything() {
            StopMixer();
            StopAllPlaylists();
        }

        /// <summary>
        /// This method allows you to pause all Audio Sources in the mixer and Playlists as well.
        /// </summary>
        public static void PauseEverything() {
            PauseMixer();
            PauseAllPlaylists();
        }

        /// <summary>
        /// This method allows you to unpause all Audio Sources in the mixer and Playlists as well.
        /// </summary>
        public static void UnpauseEverything() {
            UnpauseMixer();
            UnpauseAllPlaylists();
        }

        /// <summary>
        /// This method allows you to mute all Audio Sources in the mixer and Playlists as well.
        /// </summary>
        public static void MuteEverything() {
            MixerMuted = true;
            MuteAllPlaylists();
        }

        /// <summary>
        /// This method allows you to unmute all Audio Sources in the mixer and Playlists as well.
        /// </summary>
        public static void UnmuteEverything() {
            MixerMuted = false;
            UnmuteAllPlaylists();
        }

        /// <summary>
        /// This provides a list of of all audio clip names used in all Sound Groups, at edit time.
        /// </summary>
        /// <returns></returns>
        public static List<string> ListOfAudioClipsInGroupsEditTime() {
            var clips = new List<string>();

            for (var i = 0; i < Instance.transform.childCount; i++) {
                var aGrp = Instance.transform.GetChild(i).GetComponent<MasterAudioGroup>();
                for (var c = 0; c < aGrp.transform.childCount; c++) {
                    var aVar = aGrp.transform.GetChild(c).GetComponent<SoundGroupVariation>();

                    var clipName = string.Empty;

                    switch (aVar.audLocation) {
                        case AudioLocation.Clip:
                            var clip = aVar.VarAudio.clip;
                            if (clip != null) {
                                clipName = clip.name;
                            }
                            break;
                        case AudioLocation.ResourceFile:
                            clipName = aVar.resourceFileName;
                            break;
                        case AudioLocation.FileOnInternet:
                            clipName = aVar.internetFileUrl;
                            break;
                    }

                    if (!string.IsNullOrEmpty(clipName) && !clips.Contains(clipName)) {
                        clips.Add(clipName);
                    }
                }
            }

            return clips;
        }

        #endregion

        #region Bus methods

        private static int GetBusIndex(string busName, bool alertMissing) {
            for (var i = 0; i < GroupBuses.Count; i++) {
                if (GroupBuses[i].busName == busName) {
                    return i + HardCodedBusOptions;
                }
            }

            if (alertMissing) {
                LogWarning("Could not find bus '" + busName + "'.");
            }

            return -1;
        }

        private static GroupBus GetBusByIndex(int busIndex) {
            if (busIndex < HardCodedBusOptions) {
                return null;
            }

            return GroupBuses[busIndex - HardCodedBusOptions];
        }

        /// <summary>
        /// This method allows you to change the pitch of all Variations in all Groups in a bus.
        /// </summary>
        /// <param name="busName">The name of the bus.</param>
        /// <param name="pitch">The new pitch to use.</param>
        public static void ChangeBusPitch(string busName, float pitch) {
            var busIndex = GetBusIndex(busName, true);

            if (busIndex < 0) {
                return;
            }

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                ChangeVariationPitch(aGroup.GameObjectName, true, string.Empty, pitch);
            }
        }

        /// <summary>
        /// This method allows you to mute all Groups in a bus.
        /// </summary>
        /// <param name="busName">The name of the bus to mute.</param>
        public static void MuteBus(string busName) {
            var busIndex = GetBusIndex(busName, true);

            if (busIndex < 0) {
                return;
            }

            var bus = GrabBusByName(busName);
            bus.isMuted = true;

            if (bus.isSoloed) {
                UnsoloBus(busName);
            }

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                MuteGroup(aGroup.GameObjectName);
            }
        }

        /// <summary>
        /// This method allows you to unmute all Groups in a bus.
        /// </summary>
        /// <param name="busName">The name of the bus to unmute.</param>
        public static void UnmuteBus(string busName) {
            var busIndex = GetBusIndex(busName, true);

            if (busIndex < 0) {
                return;
            }

            var bus = GrabBusByName(busName);
            bus.isMuted = false;

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                UnmuteGroup(aGroup.GameObjectName);
            }
        }

        /// <summary>
        /// This will mute the bus if unmuted, and vice versa
        /// </summary>
        /// <param name="busName">Name of the bus to toggle mute of</param>
        public static void ToggleMuteBus(string busName) {
            var busIndex = GetBusIndex(busName, true);

            if (busIndex < 0) {
                return;
            }

            var bus = GrabBusByName(busName);
            if (bus.isMuted) {
                UnmuteBus(busName);
            } else {
                MuteBus(busName);
            }
        }

        /// <summary>
        /// This method allows you to pause all Audio Sources in a bus.
        /// </summary>
        /// <param name="busName">The name of the bus to pause.</param>
        public static void PauseBus(string busName) {
            var busIndex = GetBusIndex(busName, true);

            if (busIndex < 0) {
                return;
            }

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                PauseSoundGroup(aGroup.GameObjectName);
            }
        }

        /// <summary>
        /// This method allows you to solo all Groups in a bus.
        /// </summary>
        /// <param name="busName">The name of the bus to solo.</param>
        public static void SoloBus(string busName) {
            var busIndex = GetBusIndex(busName, true);

            if (busIndex < 0) {
                return;
            }

            var bus = GrabBusByName(busName);
            bus.isSoloed = true;

            if (bus.isMuted) {
                UnmuteBus(busName);
            }

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                SoloGroup(aGroup.GameObjectName);
            }
        }

        /// <summary>
        /// This method allows you to unsolo all Groups in a bus.
        /// </summary>
        /// <param name="busName">The name of the bus to unsolo.</param>
        public static void UnsoloBus(string busName) {
            var busIndex = GetBusIndex(busName, true);

            if (busIndex < 0) {
                return;
            }

            var bus = GrabBusByName(busName);
            bus.isSoloed = false;

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                UnsoloGroup(aGroup.GameObjectName);
            }
        }

#if UNITY_5
        public static void RouteBusToUnityMixerGroup(string busName, AudioMixerGroup mixerGroup) {
            if (!Application.isPlaying) {
                return;
            }

            var busIndex = GetBusIndex(busName, true);

            if (busIndex < 0) {
                return;
            }

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                RouteGroupToUnityMixerGroup(aGroup.name, mixerGroup);
            }
        }
#endif

        private static void StopOldestSoundOnBus(GroupBus bus) {
            var busIndex = GetBusIndex(bus.busName, true);

            if (busIndex < 0) {
                return;
            }

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope
            SoundGroupVariation oldestVar = null;
            var oldestVarPlayTime = -1f;

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                // group has same bus, check for time played.
                if (aGroup.ActiveVoices == 0) {
                    continue; // nothing playing, look in next group
                }

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < aInfo.Sources.Count; i++) {
                    var aVar = aInfo.Sources[i].Variation;
                    if (!aVar.PlaySoundParm.IsPlaying) {
                        continue;
                    }

                    if (oldestVar == null) {
                        oldestVar = aVar;
                        oldestVarPlayTime = aVar.LastTimePlayed;
                    } else if (aVar.LastTimePlayed < oldestVarPlayTime) {
                        oldestVar = aVar;
                        oldestVarPlayTime = aVar.LastTimePlayed;
                    }
                }
            }

            if (oldestVar != null) {
                oldestVar.Stop();
            }
        }

        /// <summary>
        /// This method allows you to stop all Audio Sources in a bus.
        /// </summary>
        /// <param name="busName">The name of the bus to stop.</param>
        public static void StopBus(string busName) {
            var busIndex = GetBusIndex(busName, true);

            if (busIndex < 0) {
                return;
            }

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                StopAllOfSound(aGroup.GameObjectName);
            }
        }

        /// <summary>
        /// This method allows you to unpause all paused Audio Sources in a bus.
        /// </summary>
        /// <param name="busName">The name of the bus to unpause.</param>
        public static void UnpauseBus(string busName) {
            var busIndex = GetBusIndex(busName, true);

            if (busIndex < 0) {
                return;
            }

            var sources = AudioSourcesBySoundType.GetEnumerator();

            // ReSharper disable TooWideLocalVariableScope
            MasterAudioGroup aGroup;
            AudioGroupInfo aInfo;
            // ReSharper restore TooWideLocalVariableScope

            while (sources.MoveNext()) {
                aInfo = sources.Current.Value;
                aGroup = aInfo.Group;
                if (aGroup.busIndex != busIndex) {
                    continue;
                }

                UnpauseSoundGroup(aGroup.GameObjectName);
            }
        }

        /// <summary>
        /// This method will create a new bus with the name you specify.
        /// </summary>
        /// <param name="busName">The name of the new bus.</param>
        /// <param name="errorOnExisting">Whether to log an error if the bus already exists (same name).</param>
        public static bool CreateBus(string busName, bool errorOnExisting = true) {
            var match = GroupBuses.FindAll(delegate(GroupBus obj) {
                return obj.busName == busName;
            });

            if (match.Count > 0) {
                if (errorOnExisting) {
                    LogError("You already have a bus named '" + busName + "'. Not creating a second one.");
                }
                return false;
            }

            var newBus = new GroupBus { busName = busName };

            var busVol = PersistentAudioSettings.GetBusVolume(busName);
            GroupBuses.Add(newBus);

            if (busVol.HasValue) {
                SetBusVolumeByName(busName, busVol.Value);
            }

            return true;
        }

        /// <summary>
        /// This method will delete a bus by name.
        /// </summary>
        /// <param name="busName">The name of the bus to delete.</param>
        public static void DeleteBusByName(string busName) {
            var index = GetBusIndex(busName, false);
            if (index > 0) {
                DeleteBusByIndex(index);
            }
        }


        /*! \cond PRIVATE */
        public static void DeleteBusByIndex(int busIndex) {
            GroupBuses.RemoveAt(busIndex - HardCodedBusOptions);

            // ReSharper disable once TooWideLocalVariableScope
            AudioGroupInfo aGroupInfo;

            var sources = AudioSourcesBySoundType.GetEnumerator();

            while (sources.MoveNext()) {
                aGroupInfo = sources.Current.Value;
                var aGroup = aGroupInfo.Group;
                if (aGroup.busIndex == -1) {
                    continue;
                }

                if (aGroup.busIndex == busIndex) {
                    // this bus was just deleted!
                    aGroup.busIndex = -1;

#if UNITY_5
                    RouteGroupToUnityMixerGroup(aGroup.name, null);
                    
                    // re-init Group for "no bus"
                    for (var i = 0; i < aGroupInfo.Sources.Count; i++) {
                        var aVariation = aGroupInfo.Sources[i].Variation;
                        aVariation.SetSpatialBlend();
                    }

                    RecalculateGroupVolumes(aGroupInfo, null);
#endif
                } else if (aGroup.busIndex > busIndex) {
                    aGroup.busIndex--;
                }
            }
        }
        /*! \endcond */

        /// <summary>
        /// This method will return the bus volume of a specified Sound Group, if any. If the Group is not in a bus, this will return 1.
        /// </summary>
        /// <param name="maGroup">The Sound Group object.</param>
        /// <returns>The volume of the bus.</returns>
        public static float GetBusVolume(MasterAudioGroup maGroup) {
            var busVolume = 1f;
            if (maGroup.busIndex >= HardCodedBusOptions) {
                busVolume = GroupBuses[maGroup.busIndex - HardCodedBusOptions].volume;
            }

            return busVolume;
        }

        /// <summary>
        /// This method allows you to fade the volume of a bus over X seconds.
        /// </summary>
        /// <param name="busName">The name of the bus to fade.</param>
        /// <param name="newVolume">The target volume of the bus.</param>
        /// <param name="fadeTime">The amount of time the fade will take.</param>
        /// <param name="completionCallback">(Optional) - a method to execute when the fade has completed.</param>
        // ReSharper disable once RedundantNameQualifier
        public static void FadeBusToVolume(string busName, float newVolume, float fadeTime,
            // ReSharper disable once RedundantNameQualifier
            System.Action completionCallback = null) {
            if (newVolume < 0f || newVolume > 1f) {
                Debug.LogError("Illegal volume passed to FadeBusToVolume: '" + newVolume +
                               "'. Legal volumes are between 0 and 1");
                return;
            }

            if (fadeTime <= InnerLoopCheckInterval) {
                SetBusVolumeByName(busName, newVolume); // time really short, just do it at once.

                if (completionCallback != null) {
                    completionCallback();
                }

                return;
            }

            var bus = GrabBusByName(busName);

            if (bus == null) {
                Debug.Log("Could not find bus '" + busName + "' to fade it.");
                return;
            }

            if (newVolume < 0f || newVolume > 1f) {
                Debug.Log("Cannot fade bus '" + busName +
                          "'. Invalid volume specified. Volume should be between 0 and 1.");
                return;
            }

            // make sure no other bus fades for this bus are happenning.
            var matchingFade = BusFades.Find(delegate(BusFadeInfo obj) {
                return obj.NameOfBus == busName;
            });

            if (matchingFade != null) {
                matchingFade.IsActive = false; // start with a new one, delete old.
            }

            var volStep = (newVolume - bus.volume) / (fadeTime / AudioUtil.FrameTime);

            var busFade = new BusFadeInfo() {
                NameOfBus = busName,
                ActingBus = bus,
                VolumeStep = volStep,
                TargetVolume = newVolume
            };

            if (completionCallback != null) {
                busFade.completionAction = completionCallback;
            }

            BusFades.Add(busFade);
        }

        /// <summary>
        /// This method will set the volume of a bus.
        /// </summary>
        /// <param name="newVolume">The volume to set the bus to.</param>
        /// <param name="busName">The bus name.</param>
        public static void SetBusVolumeByName(string busName, float newVolume) {
            var bus = GrabBusByName(busName);
            if (bus == null) {
                Debug.LogError("bus '" + busName + "' not found!");
                return;
            }

            SetBusVolume(bus, newVolume);
        }

        private static void RecalculateGroupVolumes(AudioGroupInfo aGroup, GroupBus bus) {
            var groupBus = GetBusByIndex(aGroup.Group.busIndex);

            var hasBus = groupBus != null && bus != null && groupBus.busName == bus.busName;
            var busVolume = hasBus ? bus.volume : 1;

            // ReSharper disable TooWideLocalVariableScope
            AudioInfo aInfo;
            AudioSource aSource;
            SoundGroupVariation aVar;
            // ReSharper restore TooWideLocalVariableScope

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < aGroup.Sources.Count; i++) {
                aInfo = aGroup.Sources[i];
                aSource = aInfo.Source;

                if (!aInfo.Variation.IsPlaying) {
                    continue;
                }

                var grpVol = aGroup.Group.groupMasterVolume * busVolume * Instance._masterAudioVolume;
                var newVol = (aInfo.OriginalVolume * aInfo.LastPercentageVolume * grpVol) + aInfo.LastRandomVolume;
                aSource.volume = newVol;

                aVar = aSource.GetComponent<SoundGroupVariation>();
                aVar.SetGroupVolume = grpVol;
            }

        }

        private static void SetBusVolume(GroupBus bus, float newVolume) {
            if (bus != null) {
                bus.volume = newVolume;
            }

            // ReSharper disable TooWideLocalVariableScope
            AudioGroupInfo aGroup;
            // ReSharper restore TooWideLocalVariableScope

            foreach (var key in AudioSourcesBySoundType.Keys) {
                aGroup = AudioSourcesBySoundType[key];
                RecalculateGroupVolumes(aGroup, bus);
            }
        }

        /// <summary>
        /// This method will return the settings of a bus.
        /// </summary>
        /// <param name="busName">The bus name.</param>
        /// <returns>GroupBus object</returns>
        public static GroupBus GrabBusByName(string busName) {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < GroupBuses.Count; i++) {
                var aBus = GroupBuses[i];
                if (aBus.busName == busName) {
                    return aBus;
                }
            }

            return null;
        }

        #endregion

        #region Ducking methods

        /// <summary>
        /// This method will allow you to add a Sound Group to the list of sounds that cause music in the Playlist to duck.
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        /// <param name="riseVolumeStart">Percentage of time length to start unducking.</param>
        /// <param name="duckedVolCut">Amount of decimals to cut the original volume</param>
        /// <param name="unduckTime">Amount of time to return music to original volume.</param>
        public static void AddSoundGroupToDuckList(string sType, float riseVolumeStart, float duckedVolCut, float unduckTime) {
            var ma = Instance;

            if (ma.duckingBySoundType.ContainsKey(sType)) {
                return;
            }

            var newDuck = new DuckGroupInfo() {
                soundType = sType,
                riseVolStart = riseVolumeStart,
                duckedVolumeCut = duckedVolCut,
                unduckTime = unduckTime
            };

            ma.duckingBySoundType.Add(sType, newDuck);
            ma.musicDuckingSounds.Add(newDuck);
        }

        /// <summary>
        /// This method will allow you to remove a Sound Group from the list of sounds that cause music in the Playlist to duck.
        /// </summary>
        /// <param name="sType">The name of the Sound Group.</param>
        public static void RemoveSoundGroupFromDuckList(string sType) {
            var ma = Instance;

            if (!ma.duckingBySoundType.ContainsKey(sType)) {
                return;
            }

            ma.duckingBySoundType.Remove(sType);
        }

        #endregion

        #region Playlist methods

        /// <summary>
        /// This method will find a Playlist by name and return it to you.
        /// </summary>
        public static Playlist GrabPlaylist(string playlistName, bool logErrorIfNotFound = true) {
            if (playlistName == NoGroupName) {
                return null;
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < MusicPlaylists.Count; i++) {
                var aPlaylist = MusicPlaylists[i];
                if (aPlaylist.playlistName == playlistName) {
                    return aPlaylist;
                }
            }

            if (logErrorIfNotFound) {
                Debug.LogError("Could not find Playlist '" + playlistName + "'.");
            }

            return null;
        }

        /// <summary>
        /// This method will change the pitch of all clips in a Playlist, or a single song if you specify the song name.
        /// </summary>
        /// <param name="playlistName">The name of the Playlist.</param>
        /// <param name="pitch">The pitch to change the songs to.</param>
        /// <param name="songName">(Optional) the song name to change the pitch of. If not specified, all songs will be changed.</param>
        public static void ChangePlaylistPitch(string playlistName, float pitch, string songName = null) {
            var playlist = GrabPlaylist(playlistName);

            if (playlist == null) {
                return;
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlist.MusicSettings.Count; i++) {
                var aSong = playlist.MusicSettings[i];

                if (!string.IsNullOrEmpty(songName) && aSong.alias != songName && aSong.songName != songName) {
                    continue;
                }

                aSong.pitch = pitch;
            }
        }

        #region Mute Playlist

        /// <summary>
        /// This method will allow you to mute your Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        public static void MutePlaylist() {
            MutePlaylist(OnlyPlaylistControllerName);
        }

        /// <summary>
        /// This method will allow you to mute a Playlist Controller by name.
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        public static void MutePlaylist(string playlistControllerName) {
            var pcs = PlaylistController.Instances;

            var controllers = new List<PlaylistController>();

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "PausePlaylist")) {
                    return;
                }

                controllers.Add(pcs[0]);
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl != null) {
                    controllers.Add(pl);
                }
            }

            MutePlaylists(controllers);
        }

        /// <summary>
        /// This method will allow you to mute all Playlist Controllers.
        /// </summary>
        public static void MuteAllPlaylists() {
            MutePlaylists(PlaylistController.Instances);
        }

        private static void MutePlaylists(List<PlaylistController> playlists) {
            if (playlists.Count == PlaylistController.Instances.Count) {
                PlaylistsMuted = true;
            }

            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aList;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlists.Count; i++) {
                aList = playlists[i];
                aList.MutePlaylist();
            }
        }

        #endregion

        #region Unmute Playlist

        /// <summary>
        /// This method will allow you to unmute your Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        public static void UnmutePlaylist() {
            UnmutePlaylist(OnlyPlaylistControllerName);
        }

        /// <summary>
        /// This method will allow you to unmute a Playlist Controller by name.
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        public static void UnmutePlaylist(string playlistControllerName) {
            var pcs = PlaylistController.Instances;

            var controllers = new List<PlaylistController>();

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "PausePlaylist")) {
                    return;
                }

                controllers.Add(pcs[0]);
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl != null) {
                    controllers.Add(pl);
                }
            }

            UnmutePlaylists(controllers);
        }

        /// <summary>
        /// This method will allow you to unmute all Playlist Controllers.
        /// </summary>
        public static void UnmuteAllPlaylists() {
            UnmutePlaylists(PlaylistController.Instances);
        }

        private static void UnmutePlaylists(List<PlaylistController> playlists) {
            if (playlists.Count == PlaylistController.Instances.Count) {
                PlaylistsMuted = false;
            }

            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aList;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlists.Count; i++) {
                aList = playlists[i];
                aList.UnmutePlaylist();
            }
        }

        #endregion

        #region Toggle Mute Playlist

        /// <summary>
        /// This method will allow you to toggle mute on your Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        public static void ToggleMutePlaylist() {
            ToggleMutePlaylist(OnlyPlaylistControllerName);
        }

        /// <summary>
        /// This method will allow you to toggle mute on a Playlist Controller by name.
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        public static void ToggleMutePlaylist(string playlistControllerName) {
            var pcs = PlaylistController.Instances;

            var controllers = new List<PlaylistController>();

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "PausePlaylist")) {
                    return;
                }

                controllers.Add(pcs[0]);
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl != null) {
                    controllers.Add(pl);
                }
            }

            ToggleMutePlaylists(controllers);
        }

        /// <summary>
        /// This method will allow you to toggle mute on all Playlist Controllers.
        /// </summary>
        public static void ToggleMuteAllPlaylists() {
            ToggleMutePlaylists(PlaylistController.Instances);
        }

        private static void ToggleMutePlaylists(List<PlaylistController> playlists) {
            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aList;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlists.Count; i++) {
                aList = playlists[i];
                aList.ToggleMutePlaylist();
            }
        }

        #endregion

        #region Pause Playlist

        /// <summary>
        /// This method will allow you to pause your Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        public static void PausePlaylist() {
            PausePlaylist(OnlyPlaylistControllerName);
        }

        /// <summary>
        /// This method will allow you to pause a Playlist Controller by name.
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        public static void PausePlaylist(string playlistControllerName) {
            var pcs = PlaylistController.Instances;

            var controllers = new List<PlaylistController>();

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "PausePlaylist")) {
                    return;
                }

                controllers.Add(pcs[0]);
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl != null) {
                    controllers.Add(pl);
                }
            }

            PausePlaylists(controllers);
        }

        /// <summary>
        /// This method will allow you to pause all Playlist Controllers.
        /// </summary>
        public static void PauseAllPlaylists() {
            PausePlaylists(PlaylistController.Instances);
        }

        private static void PausePlaylists(List<PlaylistController> playlists) {
            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aList;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlists.Count; i++) {
                aList = playlists[i];
                aList.PausePlaylist();
            }
        }

        #endregion

        #region Resume Playlist

        /// <summary>
        /// This method will allow you to resume a paused Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        public static void ResumePlaylist() {
            ResumePlaylist(OnlyPlaylistControllerName);
        }

        /// <summary>
        /// This method will allow you to resume a paused Playlist Controller by name.
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        public static void ResumePlaylist(string playlistControllerName) {
            var pcs = PlaylistController.Instances;

            var controllers = new List<PlaylistController>();

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "ResumePlaylist")) {
                    return;
                }

                controllers.Add(pcs[0]);
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl != null) {
                    controllers.Add(pl);
                }
            }

            UnpausePlaylists(controllers);
        }

        /// <summary>
        /// This method will allow you to resume all paused Playlist Controllers.
        /// </summary>
        public static void UnpauseAllPlaylists() {
            UnpausePlaylists(PlaylistController.Instances);
        }

        private static void UnpausePlaylists(List<PlaylistController> controllers) {
            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aList;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < controllers.Count; i++) {
                aList = controllers[i];
                aList.UnpausePlaylist();
            }
        }

        #endregion

        #region Stop Playlist

        /// <summary>
        /// This method will stop a Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        public static void StopPlaylist() {
            StopPlaylist(OnlyPlaylistControllerName);
        }

        /// <summary>
        /// This method will stop a Playlist Controller by name.
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        public static void StopPlaylist(string playlistControllerName) {
            var pcs = PlaylistController.Instances;

            var controllers = new List<PlaylistController>();

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "StopPlaylist")) {
                    return;
                }

                controllers.Add(pcs[0]);
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl != null) {
                    controllers.Add(pl);
                }
            }

            StopPlaylists(controllers);
        }

        /// <summary>
        /// This method will allow you to stop all Playlist Controllers.
        /// </summary>
        public static void StopAllPlaylists() {
            StopPlaylists(PlaylistController.Instances);
        }

        private static void StopPlaylists(List<PlaylistController> playlists) {
            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aList;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlists.Count; i++) {
                aList = playlists[i];
                aList.StopPlaylist();
            }
        }

        #endregion

        #region Next Playlist Clip

        /// <summary>
        /// This method will advance the Playlist to the next clip in your Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        public static void TriggerNextPlaylistClip() {
            TriggerNextPlaylistClip(OnlyPlaylistControllerName);
        }

        /// <summary>
        /// This method will advance the Playlist to the next clip in the Playlist Controller you name.
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        public static void TriggerNextPlaylistClip(string playlistControllerName) {
            var pcs = PlaylistController.Instances;

            var controllers = new List<PlaylistController>();

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "TriggerNextPlaylistClip")) {
                    return;
                }

                controllers.Add(pcs[0]);
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl != null) {
                    controllers.Add(pl);
                }
            }

            NextPlaylistClips(controllers);
        }

        /// <summary>
        /// This method will allow you to advance Playlists in all Playlist Controllers to the next clip in their Playlist.
        /// </summary>
        public static void TriggerNextClipAllPlaylists() {
            NextPlaylistClips(PlaylistController.Instances);
        }

        private static void NextPlaylistClips(List<PlaylistController> playlists) {
            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aList;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlists.Count; i++) {
                aList = playlists[i];
                aList.PlayNextSong();
            }
        }

        #endregion

        #region Random Playlist Clip

        /// <summary>
        /// This method will play a random clip in the current Playlist for your Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        public static void TriggerRandomPlaylistClip() {
            TriggerRandomPlaylistClip(OnlyPlaylistControllerName);
        }

        /// <summary>
        /// This method will play a random clip in the current Playlist for the Playlist Controller you name.
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        public static void TriggerRandomPlaylistClip(string playlistControllerName) {
            var pcs = PlaylistController.Instances;

            var controllers = new List<PlaylistController>();

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "TriggerRandomPlaylistClip")) {
                    return;
                }

                controllers.Add(pcs[0]);
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl != null) {
                    controllers.Add(pl);
                }
            }

            RandomPlaylistClips(controllers);
        }

        /// <summary>
        /// This method will allow you to play a random clip in all Playlist Controllers using their currenct Playlist
        /// </summary>
        public static void TriggerRandomClipAllPlaylists() {
            RandomPlaylistClips(PlaylistController.Instances);
        }

        private static void RandomPlaylistClips(List<PlaylistController> playlists) {
            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aList;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlists.Count; i++) {
                aList = playlists[i];
                aList.PlayRandomSong();
            }
        }

        #endregion

        #region RestartPlaylist

        /// <summary>
        /// This method will restart the current Playlist in the Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        public static void RestartPlaylist() {
            RestartPlaylist(OnlyPlaylistControllerName);
        }

        /// <summary>
        /// This method will restart a Playlist in the Playlist Controller. 
        /// </summary>
        /// <param name="playlistControllerName">The Playlist Controller.</param>
        public static void RestartPlaylist(string playlistControllerName) {
            var pcs = PlaylistController.Instances;

            PlaylistController controller;

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "RestartPlaylist")) {
                    return;
                }

                controller = pcs[0];
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl == null) {
                    return;
                }

                controller = pl;
            }

            if (controller != null) {
                RestartPlaylists(new List<PlaylistController>() { controller });
            }
        }

        /// <summary>
        /// This method will allow you to restart all Playlists.
        /// </summary>
        public static void RestartAllPlaylists() {
            RestartPlaylists(PlaylistController.Instances);
        }

        private static void RestartPlaylists(List<PlaylistController> playlists) {
            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aList;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlists.Count; i++) {
                aList = playlists[i];
                aList.RestartPlaylist();
            }
        }

        #endregion

        #region StartPlaylist

        /// <summary>
        /// This method is used to start a Playlist whether it's already loaded and playing or not.
        /// </summary>
        /// <param name="playlistName">The name of the Playlist to start</param>
        public static void StartPlaylist(string playlistName) {
            StartPlaylist(OnlyPlaylistControllerName, playlistName);
        }

        /// <summary>
        /// This method is used to start a Playlist whether it's already loaded and playing or not.
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller to use</param>
        /// <param name="playlistName">The name of the Playlist to start</param>
        public static void StartPlaylist(string playlistControllerName, string playlistName) {
            var pcs = PlaylistController.Instances;

            var controllers = new List<PlaylistController>();

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "PausePlaylist")) {
                    return;
                }

                controllers.Add(pcs[0]);
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl != null) {
                    controllers.Add(pl);
                }
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < controllers.Count; i++) {
                controllers[i].StartPlaylist(playlistName);
            }
        }

        #endregion

        #region Stop Looping Current Song

        /// <summary>
        /// This method will stop looping the current song on all Playlist Controllers so the next can play when it's finished (if auto-advance is on).
        /// </summary>
        public static void StopLoopingAllCurrentSongs() {
            StopLoopingCurrentSongs(PlaylistController.Instances);
        }


        /// <summary>
        /// This method will stop looping the current song so the next can play when it's finished (if auto-advance is on). Use this method when only one Playlist Controller exists.
        /// </summary>
        public static void StopLoopingCurrentSong() {
            StopLoopingCurrentSong(OnlyPlaylistControllerName);
        }

        /// <summary>
        /// This method will stop looping the current song so the next can play when it's finished (if auto-advance is on). Use this method when more than one Playlist Controller exists.
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        public static void StopLoopingCurrentSong(string playlistControllerName) {
            var pcs = PlaylistController.Instances;

            PlaylistController controller;

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "StopLoopingCurrentSong")) {
                    return;
                }

                controller = pcs[0];
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl == null) {
                    return;
                }

                controller = pl;
            }

            if (controller != null) {
                StopLoopingCurrentSongs(new List<PlaylistController> { controller });
            }
        }

        private static void StopLoopingCurrentSongs(List<PlaylistController> playlistControllers) {
            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aController;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlistControllers.Count; i++) {
                aController = playlistControllers[i];
                aController.StopLoopingCurrentSong();
            }
        }

        #endregion

        #region Queue Clip

        /// <summary>
        /// This method will play an Audio Clip by name that's in the current Playlist of your Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter. This requires auto-advance to work.
        /// </summary>
        /// <param name="clipName">The name of the clip.</param>
        public static void QueuePlaylistClip(string clipName) {
            QueuePlaylistClip(OnlyPlaylistControllerName, clipName);
        }

        /// <summary>
        /// This method will play an Audio Clip by name that's in the current Playlist of the Playlist Controller you name, as soon as the currently playing song is over. Loop will be turned off on the current song. This requires auto-advance to work.
        /// </summary>
        /// <param name="clipName">The name of the clip.</param>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        public static void QueuePlaylistClip(string playlistControllerName, string clipName) {
            var pcs = PlaylistController.Instances;

            PlaylistController controller;

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "QueuePlaylistClip")) {
                    return;
                }

                controller = pcs[0];
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl == null) {
                    return;
                }

                controller = pl;
            }

            if (controller != null) {
                controller.QueuePlaylistClip(clipName);
            }
        }

        #endregion

        #region Trigger Playlist Clip

        /// <summary>
        /// This method will play an Audio Clip by name that's in the current Playlist of your Playlist Controller. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        /// <param name="clipName">The name of the clip.</param>
        /// <returns>bool - whether the song was played or not.</returns>
        public static bool TriggerPlaylistClip(string clipName) {
            return TriggerPlaylistClip(OnlyPlaylistControllerName, clipName);
        }

        /// <summary>
        /// This method will play an Audio Clip by name that's in the current Playlist of the Playlist Controller you name.
        /// </summary>
        /// <param name="clipName">The name of the clip.</param>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        /// <returns>bool - whether the song was played or not.</returns>
        public static bool TriggerPlaylistClip(string playlistControllerName, string clipName) {
            var pcs = PlaylistController.Instances;

            PlaylistController controller;

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "TriggerPlaylistClip")) {
                    return false;
                }

                controller = pcs[0];
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl == null) {
                    return false;
                }

                controller = pl;
            }

            if (controller == null) {
                return false;
            }

            return controller.TriggerPlaylistClip(clipName);
        }

        #endregion

        #region ChangePlaylistByName

        /// <summary>
        /// This method will change the current Playlist in the Playlist Controller to a Playlist whose name you specify. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        /// <param name="playlistName">The name of the new Playlist.</param>
        /// <param name="playFirstClip"><b>Optional</b> - defaults to True. If you specify false, the first clip in the Playlist will not automatically play.</param>
        public static void ChangePlaylistByName(string playlistName, bool playFirstClip = true) {
            ChangePlaylistByName(OnlyPlaylistControllerName, playlistName, playFirstClip);
        }

        /// <summary>
        /// This method will play an Audio Clip by name that's in the current Playlist of the Playlist Controller you name.
        /// </summary>
        /// <param name="playlistControllerName">The Playlist Controller name</param>
        /// <param name="playlistName">The name of the new Playlist.</param>
        /// <param name="playFirstClip"><b>Optional</b> - defaults to True. If you specify false, the first clip in the Playlist will not automatically play.</param>
        public static void ChangePlaylistByName(string playlistControllerName, string playlistName,
            bool playFirstClip = true) {
            var pcs = PlaylistController.Instances;

            PlaylistController controller;

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "ChangePlaylistByName")) {
                    return;
                }

                controller = pcs[0];
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl == null) {
                    return;
                }

                controller = pl;
            }

            if (controller != null) {
                controller.ChangePlaylist(playlistName, playFirstClip);
            }
        }

        #endregion

        #region Playlist Fade To Volume

        /// <summary>
        /// This method will fade the volume of the Playlist Controller over X seconds. You should not use this if you have more than one Playlist Controller. Use the overloaded method instead, it takes a playlistControllerName parameter.
        /// </summary>
        /// <param name="targetVolume">The target volume of the Playlist.</param>
        /// <param name="fadeTime">The time to fade completely to the target volume.</param>
        public static void FadePlaylistToVolume(float targetVolume, float fadeTime) {
            FadePlaylistToVolume(OnlyPlaylistControllerName, targetVolume, fadeTime);
        }

        /// <summary>
        /// This method will fade the volume of the Playlist Controller whose name you specify over X seconds. 
        /// </summary>
        /// <param name="playlistControllerName">The name of the Playlist Controller.</param>
        /// <param name="targetVolume">The target volume of the Playlist.</param>
        /// <param name="fadeTime">The time to fade completely to the target volume.</param>
        public static void FadePlaylistToVolume(string playlistControllerName, float targetVolume, float fadeTime) {
            var pcs = PlaylistController.Instances;

            var controllers = new List<PlaylistController>();

            if (playlistControllerName == OnlyPlaylistControllerName) {
                if (!IsOkToCallOnlyPlaylistMethod(pcs, "FadePlaylistToVolume")) {
                    return;
                }

                controllers.Add(pcs[0]);
            } else {
                // multiple playlist controllers
                var pl = PlaylistController.InstanceByName(playlistControllerName);
                if (pl != null) {
                    controllers.Add(pl);
                }
            }

            FadePlaylists(controllers, targetVolume, fadeTime);
        }

        /// <summary>
        /// This method will allow you to fade all current Playlists used by Playlist Controllers to a target volume over X seconds.
        /// </summary>
        public static void FadeAllPlaylistsToVolume(float targetVolume, float fadeTime) {
            FadePlaylists(PlaylistController.Instances, targetVolume, fadeTime);
        }

        private static void FadePlaylists(List<PlaylistController> playlists, float targetVolume, float fadeTime) {
            if (targetVolume < 0f || targetVolume > 1f) {
                Debug.LogError("Illegal volume passed to FadePlaylistToVolume: '" + targetVolume +
                               "'. Legal volumes are between 0 and 1");
                return;
            }

            // ReSharper disable once TooWideLocalVariableScope
            PlaylistController aList;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < playlists.Count; i++) {
                aList = playlists[i];
                aList.FadeToVolume(targetVolume, fadeTime);
            }
        }

        #endregion


        /// <summary>
        /// This method will allow you to add a Playlist via code.
        /// </summary>
        /// <param name="playlist">The playlist with all settings included</param>
        /// <param name="errorOnDuplicate">Whether or not to log an error if the Playlist already exists (same name).</param>
        public static void CreatePlaylist(Playlist playlist, bool errorOnDuplicate) {
            var pl = GrabPlaylist(playlist.playlistName, false);

            if (pl != null) {
                if (errorOnDuplicate) {
                    Debug.LogError("You already have a Playlist Controller with the name '" + pl.playlistName +
                                   "'. You must name them all uniquely. Not adding duplicate named Playlist.");
                }

                return;
            }

            MusicPlaylists.Add(playlist);
        }

        /// <summary>
        /// This method will allow you to delete a Playlist via code.
        /// </summary>
        /// <param name="playlistName">The playlist name</param>
        public static void DeletePlaylist(string playlistName) {
            if (SafeInstance == null) {
                return;
            }

            var pl = GrabPlaylist(playlistName);

            if (pl == null) {
                return;
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < PlaylistController.Instances.Count; i++) {
                var pc = PlaylistController.Instances[i];
                if (pc.PlaylistName != playlistName) {
                    continue;
                }
                pc.StopPlaylist();
                break;
            }

            MusicPlaylists.Remove(pl);
        }

        /// <summary>
        /// This method will allow you to add a song to a Playlist by code.
        /// </summary>
        /// <param name="playlistName">The name of the Playlist to add the song to.</param>
        /// <param name="song">The Audio clip of the song.</param>
        /// <param name="loopSong">Optional - whether or not to loop the song.</param>
        /// <param name="songPitch">Optional - the pitch of the song.</param>
        /// <param name="songVolume">Optional - The volume of the song.</param>
        public static void AddSongToPlaylist(string playlistName, AudioClip song, bool loopSong = false,
            float songPitch = 1f, float songVolume = 1f) {
            var pl = GrabPlaylist(playlistName);

            if (pl == null) {
                return;
            }

            var newSong = new MusicSetting() {
                clip = song,
                isExpanded = true,
                isLoop = loopSong,
                pitch = songPitch,
                volume = songVolume
            };

            pl.MusicSettings.Add(newSong);
        }

        /// <summary>
        /// This Property can read and set the Playlist Master Volume. 
        /// </summary>
        public static float PlaylistMasterVolume {
            get { return Instance._masterPlaylistVolume; }
            set {
                Instance._masterPlaylistVolume = value;

                var pcs = PlaylistController.Instances;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < pcs.Count; i++) {
                    pcs[i].UpdateMasterVolume();
                }
            }
        }

        #endregion

        #region InternetFile methods

        /// <summary>
        /// Calling this method will stop all Variations that have an audio file loaded from the Internet and release the memory. Then it will re-download all the internet files (in case they've changes). Good for holiday updates and things like that!
        /// </summary>
        public static void ReDownloadAllInternetFiles() {
            var varsToStop = new List<SoundGroupVariation>();

            foreach (var key in AudioSourcesBySoundType.Keys) {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < AudioSourcesBySoundType[key].Sources.Count; i++) {
                    var aSrc = AudioSourcesBySoundType[key].Sources[i].Source;

                    var aVar = aSrc.GetComponent<SoundGroupVariation>();
                    if (aVar == null) {
                        continue;
                    }

                    if (aVar.audLocation != AudioLocation.FileOnInternet) {
                        continue;
                    }

                    AudioResourceOptimizer.RemoveLoadedInternetClip(aVar.internetFileUrl);
                    aVar.internetFileLoadStatus = InternetFileLoadStatus.Loading;
                    varsToStop.Add(aVar);
                }
            }

            // stop audio and re-download!
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < varsToStop.Count; i++) {
                var aVar = varsToStop[i];
                aVar.Stop();

                AudioResourceOptimizer.AddTargetForClip(aVar.internetFileUrl, aVar.VarAudio);

                aVar.LoadInternetFile();
            }
        }
        #endregion

        #region Custom Events

        /// <summary>
        /// This method is used by MasterAudio to keep track of enabled CustomEventReceivers automatically. This is called when then CustomEventReceiver prefab is enabled.
        /// </summary>
        /// <param name="receiver">The receiver object interface.</param>
        /// <param name="receiverTrans">The receiver object Transform.</param>
        public static void AddCustomEventReceiver(ICustomEventReceiver receiver, Transform receiverTrans) {
            if (AppIsShuttingDown) {
                return;
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            var events = receiver.GetAllEvents();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < events.Count; i++) {
                var anEvent = events[i];
                if (!receiver.SubscribesToEvent(anEvent.customEventName)) {
                    continue;
                }

                if (!ReceiversByEventName.ContainsKey(anEvent.customEventName)) {
                    ReceiversByEventName.Add(anEvent.customEventName, new Dictionary<ICustomEventReceiver, Transform>
                    {
                        {receiver, receiverTrans}
                    });
                } else {
                    var dict = ReceiversByEventName[anEvent.customEventName];
                    if (dict.ContainsKey(receiver)) {
                        continue;
                    }

                    dict.Add(receiver, receiverTrans);
                }
            }
        }

        /// <summary>
        /// This method is used by MasterAudio to keep track of enabled CustomEventReceivers automatically. This is called when then CustomEventReceiver prefab is disabled.
        /// </summary>
        /// <param name="receiver">The receiver object interface.</param>
        public static void RemoveCustomEventReceiver(ICustomEventReceiver receiver) {
            if (AppIsShuttingDown || SafeInstance == null) {
                return;
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Instance.customEvents.Count; i++) {
                var anEvent = Instance.customEvents[i];
                if (!receiver.SubscribesToEvent(anEvent.EventName)) {
                    continue;
                }

                var dict = ReceiversByEventName[anEvent.EventName];
                dict.Remove(receiver);
            }
        }

        /*! \cond PRIVATE */
        public static List<Transform> ReceiversForEvent(string customEventName) {
            var receivers = new List<Transform>();

            if (!ReceiversByEventName.ContainsKey(customEventName)) {
                return receivers;
            }

            var dict = ReceiversByEventName[customEventName];

            foreach (var receiver in dict.Keys) {
                if (receiver.SubscribesToEvent(customEventName)) {
                    receivers.Add(dict[receiver]);
                }
            }

            return receivers;
        }
        /*! \endcond */


        /// <summary>
        /// This method is used to create a Custom Event at runtime.
        /// </summary>
        /// <param name="customEventName">The name of the custom event.</param>
        /// <param name="eventReceiveMode">The receive mode of the event.</param>
        /// <param name="distanceThreshold">The min or max distance to transmit the event to (optional).</param>
        /// <param name="errorOnDuplicate">Whether or not to log an error if the event already exists.</param>
        public static void CreateCustomEvent(string customEventName, CustomEventReceiveMode eventReceiveMode,
            float distanceThreshold, bool errorOnDuplicate = true) {
            if (AppIsShuttingDown) {
                return;
            }

            if (Instance.customEvents.FindAll(delegate(CustomEvent obj) {
                return obj.EventName == customEventName;
            }).Count > 0) {
                if (errorOnDuplicate) {
                    Debug.LogError("You already have a Custom Event named '" + customEventName +
                                   "'. No need to add it again.");
                }
                return;
            }

            var newEvent = new CustomEvent(customEventName) {
                eventReceiveMode = eventReceiveMode,
                distanceThreshold = distanceThreshold
            };

            Instance.customEvents.Add(newEvent);
        }

        /// <summary>
        /// This method is used to delete a temporary Custom Event at runtime.
        /// </summary>
        /// <param name="customEventName">The name of the custom event.</param>
        public static void DeleteCustomEvent(string customEventName) {
            if (AppIsShuttingDown || SafeInstance == null) {
                return;
            }

            Instance.customEvents.RemoveAll(delegate(CustomEvent obj) {
                return obj.EventName == customEventName;
            });
        }

        private static CustomEvent GetCustomEventByName(string customEventName) {
            var matches = Instance.customEvents.FindAll(delegate(CustomEvent obj) {
                return obj.EventName ==
                       customEventName;
            });

            return matches.Count > 0 ? matches[0] : null;
        }

        /// <summary>
        /// Calling this method will fire a Custom Event at the originPoint position. All CustomEventReceivers with the named event specified will do whatever action is assigned to them. If there is a distance criteria applied to receivers, it will be applied.
        /// </summary>
        /// <param name="customEventName">The name of the custom event.</param>
        /// <param name="originPoint">The position of the event.</param>
        /// <param name="logDupe">Whether or not to log an error with duplicate event firing.</param>
        public static void FireCustomEvent(string customEventName, Vector3 originPoint, bool logDupe = true) {
            if (AppIsShuttingDown) {
                return;
            }

            if (NoGroupName == customEventName) {
                return;
            }

            if (!CustomEventExists(customEventName) && !IsWarming) {
                Debug.LogError("Custom Event '" + customEventName + "' was not found in Master Audio.");
                return;
            }

            var customEvent = GetCustomEventByName(customEventName);

            if (customEvent == null) {
                // for warming
                return;
            }

            if (customEvent.frameLastFired >= Time.frameCount) {
                if (logDupe) {
                    Debug.LogWarning("Already fired Custom Event '" + customEventName +
                                     "' this frame or later. Cannot be fired twice in the same frame.");
                }
                return;
            }

            customEvent.frameLastFired = Time.frameCount;

            if (!Instance.disableLogging && Instance.logCustomEvents) {
                Debug.Log("Firing Custom Event: " + customEventName);
            }

            float? sqrDist = null;

            switch (customEvent.eventReceiveMode) {
                case CustomEventReceiveMode.Never:
                    if (Instance.LogSounds) {
                        Debug.LogWarning("Custom Event '" + customEventName +
                                         "' not being transmitted because it is set to 'Never transmit'.");
                    }
                    return; // no transmission.
                case CustomEventReceiveMode.WhenDistanceLessThan:
                case CustomEventReceiveMode.WhenDistanceMoreThan:
                    sqrDist = customEvent.distanceThreshold * customEvent.distanceThreshold;
                    break;
            }

            if (!ReceiversByEventName.ContainsKey(customEventName)) {
                // no receivers
                return;
            }

            var dict = ReceiversByEventName[customEventName];

            foreach (var receiver in dict.Keys) {
                switch (customEvent.eventReceiveMode) {
                    case CustomEventReceiveMode.WhenDistanceLessThan:
                        var dist = (dict[receiver].position - originPoint).sqrMagnitude;
                        if (dist > sqrDist) {
                            continue;
                        }
                        break;
                    case CustomEventReceiveMode.WhenDistanceMoreThan:
                        var dist2 = (dict[receiver].position - originPoint).sqrMagnitude;
                        if (dist2 < sqrDist) {
                            continue;
                        }
                        break;
                }

                receiver.ReceiveEvent(customEventName, originPoint);
            }
        }

        /// <summary>
        /// Calling this method will return whether or not the specified Custom Event exists.
        /// </summary>
        /// <param name="customEventName">The name of the custom event.</param>
        public static bool CustomEventExists(string customEventName) {
            if (AppIsShuttingDown) {
                return true;
            }

            return Instance.customEvents.FindAll(delegate(CustomEvent obj) {
                return obj.EventName == customEventName;
            }).Count > 0;
        }

        #endregion

        #region Logging (only when turned on via Inspector)

        private static bool LoggingEnabledForGroup(MasterAudioGroup grp) {
            if (IsWarming) {
                return false;
            }

            if (Instance.disableLogging) {
                return false;
            }

            if (grp != null && grp.logSound) {
                return true;
            }

            return Instance.LogSounds;
        }

        private static void LogMessage(string message) {
            Debug.Log("T: " + Time.time + " - MasterAudio " + message);
        }

        /// <summary>
        /// This gets or sets whether Logging is enabled in Master Audio
        /// </summary>
        public static bool LogSoundsEnabled {
            get { return Instance.LogSounds; }
            set { Instance.LogSounds = value; }
        }

        /*! \cond PRIVATE */
        public static void LogWarning(string msg) {
            if (Instance.disableLogging) {
                return;
            }

            Debug.LogWarning(msg);
        }

        public static void LogError(string msg) {
            if (Instance.disableLogging) {
                return;
            }

            Debug.LogError(msg);
        }

        public static void LogNoPlaylist(string playlistControllerName, string methodName) {
            LogWarning("There is currently no Playlist assigned to Playlist Controller '" + playlistControllerName +
                       "'. Cannot call '" + methodName + "' method.");
        }
        /*! \endcond */

        private static bool IsOkToCallOnlyPlaylistMethod(List<PlaylistController> pcs, string methodName) {
            if (pcs.Count == 0) {
                LogError(string.Format("You have no Playlist Controllers in the Scene. You cannot '{0}'.", methodName));
                return false;
            } else if (pcs.Count > 1) {
                LogError(
                    string.Format(
                        "You cannot call '{0}' without specifying a Playlist Controller name when you have more than one Playlist Controller.",
                        methodName));
                return false;
            }

            return true;
        }

        #endregion

        #region Occlusion methods
        /*! \cond PRIVATE */
        public static void AddToOcclusionInRangeSources(GameObject src) {
#if UNITY_5
            if (!Application.isEditor || !Instance.occlusionShowCategories) {
                return;
            }

            if (!OcclusionSourcesInRange.Contains(src)) {
                OcclusionSourcesInRange.Add(src);
            }

            if (OcclusionSourcesOutOfRange.Contains(src)) {
                OcclusionSourcesOutOfRange.Remove(src);
            }
#endif
        }

        public static void AddToOcclusionOutOfRangeSources(GameObject src) {
#if UNITY_5
            if (!Application.isEditor || !Instance.occlusionShowCategories) {
                return;
            }

            if (!OcclusionSourcesOutOfRange.Contains(src)) {
                OcclusionSourcesOutOfRange.Add(src);
            }

            if (OcclusionSourcesInRange.Contains(src)) {
                OcclusionSourcesInRange.Remove(src);
            }

            // out of range means no longer blocked
            RemoveFromBlockedOcclusionSources(src);
#endif
        }

        public static void AddToBlockedOcclusionSources(GameObject src) {
#if UNITY_5
            if (!Application.isEditor || !Instance.occlusionShowCategories) {
                return;
            }

            if (!OcclusionSourcesBlocked.Contains(src)) {
                OcclusionSourcesBlocked.Add(src);
            }
#endif
        }

        public static void RemoveFromBlockedOcclusionSources(GameObject src) {
#if UNITY_5
            if (!Application.isEditor || !Instance.occlusionShowCategories) {
                return;
            }

            if (OcclusionSourcesBlocked.Contains(src)) {
                OcclusionSourcesBlocked.Remove(src);
            }
#endif
        }

        public static void StopTrackingOcclusionForSource(GameObject src) {
#if UNITY_5
            if (!Application.isEditor || !Instance.occlusionShowCategories) {
                return;
            }

            if (OcclusionSourcesOutOfRange.Contains(src)) {
                OcclusionSourcesOutOfRange.Remove(src);
            }

            if (OcclusionSourcesInRange.Contains(src)) {
                OcclusionSourcesInRange.Remove(src);
            }

            if (OcclusionSourcesBlocked.Contains(src)) {
                OcclusionSourcesBlocked.Remove(src);
            }
#endif
        }

        /*! \endcond */
        #endregion

        #region Properties

        /// <summary>
        /// This returns a list of all Audio Sources controlled by Master Audio
        /// </summary>
        public static List<AudioSource> MasterAudioSources {
            get {
                return AllAudioSources;
            }
        }

        /*! \cond PRIVATE */
        public static Transform ListenerTrans {
            get {
                // ReSharper disable once InvertIf
                if (_listenerTrans == null) {
                    var listener = FindObjectOfType<AudioListener>();
                    if (listener != null) {
                        _listenerTrans = listener.transform;
                    }
                }

                return _listenerTrans;
            }
            set {
                _listenerTrans = value;
            }
        }

        public static PlaylistController OnlyPlaylistController {
            get {
                var pcs = PlaylistController.Instances;
                if (pcs.Count != 0) {
                    return pcs[0];
                }
                Debug.LogError("There are no Playlist Controller in this Scene.");
                return null;
            }
        }

        public static bool IsWarming {
            get { return Instance._warming; }
        }

#if UNITY_EDITOR

        public MixerWidthMode MixerWidth {
            get {
                return MasterAudioSettings.Instance.MixerWidthSetting;
            }
            set {
                MasterAudioSettings.Instance.MixerWidthSetting = value;
                EditorUtility.SetDirty(MasterAudioSettings.Instance);
            }
        }

        public bool BusesShownInNarrow {
            get {
                return MasterAudioSettings.Instance.BusesShownInNarrow;
            }
            set {
                MasterAudioSettings.Instance.BusesShownInNarrow = value;
                EditorUtility.SetDirty(MasterAudioSettings.Instance);
            }
        }


#endif
        /*! \endcond */


        /// <summary>
        /// This gets or sets whether the entire Mixer is muted or not.
        /// </summary>
        public static bool MixerMuted {
            get { return Instance.mixerMuted; }
            set {
                Instance.mixerMuted = value;

                if (value) {
                    foreach (var key in AudioSourcesBySoundType.Keys) {
                        MuteGroup(AudioSourcesBySoundType[key].Group.GameObjectName);
                    }
                } else {
                    foreach (var key in AudioSourcesBySoundType.Keys) {
                        UnmuteGroup(AudioSourcesBySoundType[key].Group.GameObjectName);
                    }
                }
            }
        }

        /// <summary>
        /// This gets or sets whether the all Playlists are muted or not.
        /// </summary>
        public static bool PlaylistsMuted {
            get { return Instance.playlistsMuted; }
            set {
                Instance.playlistsMuted = value;

                var pcs = PlaylistController.Instances;

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < pcs.Count; i++) {
                    if (value) {
                        pcs[i].MutePlaylist();
                    } else {
                        pcs[i].UnmutePlaylist();
                    }
                }
            }
        }

        /// <summary>
        /// This gets or sets whether music ducking is enabled.
        /// </summary>
        public bool EnableMusicDucking {
            get { return enableMusicDucking; }
            set { enableMusicDucking = value; }
        }

        /// <summary>
        /// This gets the cross-fade time for Playlists
        /// </summary>
        public float MasterCrossFadeTime {
            get { return crossFadeTime; }
        }

        /// <summary>
        /// This property will return all the Playlists set up in the Master Audio game object.
        /// </summary>
        public static List<Playlist> MusicPlaylists {
            get { return Instance.musicPlaylists; }
        }

        /// <summary>
        /// This returns of list of all Buses.
        /// </summary>
        public static List<GroupBus> GroupBuses {
            get { return Instance.groupBuses; }
        }

        /// <summary>
        /// This will get you the list of all Sound Group Names at runtime only.
        /// </summary>
        public static List<string> RuntimeSoundGroupNames {
            get {
                if (!Application.isPlaying) {
                    return new List<string>();
                }
                return new List<string>(AudioSourcesBySoundType.Keys);
            }
        }

        /// <summary>
        /// This will get you the list of all Bus Names at runtime only.
        /// </summary>
        public static List<string> RuntimeBusNames {
            get {
                if (!Application.isPlaying) {
                    return new List<string>();
                }

                var busNames = new List<string>();

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < Instance.groupBuses.Count; i++) {
                    busNames.Add(Instance.groupBuses[i].busName);
                }

                return busNames;
            }
        }

        /// <summary>
        /// This property returns a reference to the Singleton instance of MasterAudio, but does not log anything to the console. This is used by PersistentAudioSettings script only.
        /// </summary>
        public static MasterAudio SafeInstance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                // ReSharper disable once ArrangeStaticMemberQualifier
                _instance = (MasterAudio)GameObject.FindObjectOfType(typeof(MasterAudio));
                return _instance;
            }
        }

        /// <summary>
        /// This property returns a reference to the Singleton instance of 
        /// </summary>
        public static MasterAudio Instance {
            get {
                if (_instance != null) {
                    return _instance;
                }
                // ReSharper disable once ArrangeStaticMemberQualifier
                _instance = (MasterAudio)GameObject.FindObjectOfType(typeof(MasterAudio));

                if (_instance == null && Application.isPlaying) {
                    Debug.LogError("There is no Master Audio prefab in this Scene. Subsequent method calls will fail.");
                }

                return _instance;
            }
            // ReSharper disable once ValueParameterNotUsed
            set {
                _instance = null; // to not cache for Inspectors
            }
        }

        /// <summary>
        /// This returns true if MasterAudio is initialized and ready to use, false otherwise.
        /// </summary>
        public static bool SoundsReady {
            get { return Instance != null && Instance._soundsLoaded; }
        }

        /// <summary>
        /// This property is used to prevent bogus Unity errors while the editor is stopping play. You should never need to read or set 
        /// </summary>
        public static bool AppIsShuttingDown { get; set; }

        /// <summary>
        /// This will return a list of all the Sound Group names.
        /// </summary>
        public List<string> GroupNames {
            get {
                var groupNames = SoundGroupHardCodedNames;

                var others = new List<string>(Trans.childCount);
                for (var i = 0; i < Trans.childCount; i++) {
                    others.Add(Trans.GetChild(i).name);
                }

                others.Sort();
                groupNames.AddRange(others);

                return groupNames;
            }
        }

        /*! \cond PRIVATE */
        /// <summary>
        /// Only used internally, do not use this property
        /// </summary>
        public static List<string> SoundGroupHardCodedNames {
            get {
                return new List<string> { DynamicGroupName, NoGroupName };
            }
        }
        /*! \endcond */

        /// <summary>
        /// This will return a list of all the Bus names, including the selectors for "type in" and "no bus".
        /// </summary>
        public List<string> BusNames {
            get {
                var busNames = new List<string> { DynamicGroupName, NoGroupName };

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < groupBuses.Count; i++) {
                    busNames.Add(groupBuses[i].busName);
                }

                return busNames;
            }
        }

        /// <summary>
        /// This will return a list of all the Playlists, including the selectors for "type in" and "no bus".
        /// </summary>
        public List<string> PlaylistNames {
            get {
                var playlistNames = new List<string> { DynamicGroupName, NoPlaylistName };

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < musicPlaylists.Count; i++) {
                    playlistNames.Add(musicPlaylists[i].playlistName);
                }

                return playlistNames;
            }
        }

        /*! \cond PRIVATE */
        public Transform Trans {
            get {
                if (_trans != null) {
                    return _trans;
                }

                _trans = GetComponent<Transform>();

                return _trans;
            }
        }

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

        /// <summary>
        /// This will return a list of all the Custom Events you have defined, including the selectors for "type in" and "none".
        /// </summary>
        public List<string> CustomEventNames { get
            {
                var customEventNames = CustomEventHardCodedNames;

                var custEvents = Instance.customEvents;

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < custEvents.Count; i++) {
                    customEventNames.Add(custEvents[i].EventName);
                }

                return customEventNames;
            }
        }

        /*! \cond PRIVATE */
        /// <summary>
        /// Only used internally, do not use this property
        /// </summary>
        public static List<string> CustomEventHardCodedNames {
            get {
                return new List<string> { DynamicGroupName, NoGroupName };
            }
        }
        /*! \endcond */

        /// <summary>
        /// This is the overall master volume level which can change the relative volume of all buses and Sound Groups - not Playlist Controller songs though, they have their own master volume.
        /// </summary>
        public static float MasterVolumeLevel {
            get { return Instance._masterAudioVolume; }
            set {
                Instance._masterAudioVolume = value;

                if (!Application.isPlaying) {
                    return;
                }

                // change all currently playing sound volumes!
                var sources = AudioSourcesBySoundType.GetEnumerator();
                // ReSharper disable once TooWideLocalVariableScope
                MasterAudioGroup group;
                while (sources.MoveNext()) {
                    group = sources.Current.Value.Group;
                    SetGroupVolume(group.GameObjectName, group.groupMasterVolume);
                    // set to same volume, but it recalcs based on master volume level.
                }
            }
        }

        /*! \cond PRIVATE */
        private static bool SceneHasMasterAudio {
            get { return Instance != null; }
        }

        public static bool IgnoreTimeScale {
            get { return Instance.ignoreTimeScale; }
        }

        public static YieldInstruction InnerLoopDelay {
            get { return _innerLoopDelay; }
        }
        /*! \endcond */

        /// <summary>
        /// This gets or sets the "Dynamic Language" (needs to be set at runtime based on the user's selection) for use with localized Resource Files.
        /// </summary>
        public static SystemLanguage DynamicLanguage {
            get {
                if (!PlayerPrefs.HasKey(StoredLanguageNameKey)) {
                    PlayerPrefs.SetString(StoredLanguageNameKey, SystemLanguage.Unknown.ToString());
                }

                return
                    (SystemLanguage)Enum.Parse(typeof(SystemLanguage), PlayerPrefs.GetString(StoredLanguageNameKey));
            }
            set {
                PlayerPrefs.SetString(StoredLanguageNameKey, value.ToString());
                AudioResourceOptimizer.ClearSupportLanguageFolder();
            }
        }

        /*! \cond PRIVATE */
#if UNITY_EDITOR
        public static bool UseDbScaleForVolume {
            get {
                return MasterAudioSettings.Instance.UseDbScale;
            }
            set {
                MasterAudioSettings.Instance.UseDbScale = value;
                EditorUtility.SetDirty(MasterAudioSettings.Instance);
            }
        }

        public static bool UseCentsForPitch {
            get {
                return MasterAudioSettings.Instance.UseCentsPitch;
            }
            set {
                MasterAudioSettings.Instance.UseCentsPitch = value;
                EditorUtility.SetDirty(MasterAudioSettings.Instance);
            }
        }

        public static bool HideLogoNav {
            get {
                return MasterAudioSettings.Instance.HideLogoNav;
            }
            set {
                MasterAudioSettings.Instance.HideLogoNav = value;
                EditorUtility.SetDirty(MasterAudioSettings.Instance);
            }
        }
#endif

        public static float ReprioritizeTime {
            get {
                if (_repriTime < 0) {
                    _repriTime = (Instance.rePrioritizeEverySecIndex + 1) * 0.1f;
                }

                return _repriTime;
            }
        }

        public static float ReOccludeCheckTime {
            get {
                if (_repriTime < 0) {
                    _repriTime = (Instance.reOccludeEverySecIndex + 1) * 0.1f;
                }

                return _repriTime;
            }
        }

        public static bool HasAsyncResourceLoaderFeature() {
#if UNITY_4_5_3 || UNITY_4_5_4 || UNITY_4_5_5 || UNITY_4_6 || UNITY_4_7
            return Application.HasProLicense();
#else
#if UNITY_5
            return true;
#else    
            // older versions of Unity
            return false;
#endif
#endif
        }

        public static string ProspectiveMAPath {
            get {
                return _prospectiveMAFolder;
            }
            set {
                _prospectiveMAFolder = value;
            }
        }

#if UNITY_EDITOR
        public static string MasterAudioFolderPath {
            get {
                return MasterAudioSettings.Instance.InstallationFolderPath;
            }
            set {
                MasterAudioSettings.Instance.InstallationFolderPath = value;
                EditorUtility.SetDirty(MasterAudioSettings.Instance);
            }
        }

        public static string GroupTemplateFolder {
            get {
                return MasterAudioFolderPath + "/Sources/Prefabs/GroupTemplates/";
            }
        }

        public static string AudioSourceTemplateFolder {
            get {
                return MasterAudioFolderPath + "/Sources/Prefabs/AudioSourceTemplates/";
            }
        }

        public static List<GameObject> InRangeOcclusionSources {
            get {
                return OcclusionSourcesInRange;
            }
        }

        public static List<GameObject> OutOfRangeOcclusionSources {
            get {
                return OcclusionSourcesOutOfRange;
            }
        }

        public static List<GameObject> BlockedOcclusionSources {
            get {
                return OcclusionSourcesBlocked;
            }
        }
#endif
        /*! \endcond */

        #endregion

        #region Prefab Creation
		/*! \cond PRIVATE */

        /// <summary>
        /// Creates the master audio prefab in the current Scene.
        /// </summary>
        public static GameObject CreateMasterAudio() {
#if UNITY_EDITOR
            var ma = AssetDatabase.LoadAssetAtPath(MasterAudioFolderPath + "/Prefabs/MasterAudio.prefab",
                typeof(GameObject));
#else
				var ma = Resources.Load(MasterAudioDefaultFolder + "/Prefabs/MasterAudio.prefab", typeof(GameObject));
#endif
            if (ma == null) {
                Debug.LogError(
                    "Could not find MasterAudio prefab. Please update the Installation Path in the Master Audio Manager window if you have moved the folder from its default location, then try again.");
                return null;
            }

            // ReSharper disable once ArrangeStaticMemberQualifier
            var go = GameObject.Instantiate(ma) as GameObject;
            // ReSharper disable once PossibleNullReferenceException
            go.name = "MasterAudio";
            return go;
        }

        /// <summary>
        /// Creates a Playlist Controller prefab instance in the current Scene.
        /// </summary>
        public static GameObject CreatePlaylistController() {
#if UNITY_EDITOR
            var pc = AssetDatabase.LoadAssetAtPath(MasterAudioFolderPath + "/Prefabs/PlaylistController.prefab",
                typeof(GameObject));
#else
			var pc = Resources.Load(MasterAudioDefaultFolder + "/Prefabs/PlaylistController.prefab", typeof(GameObject));
#endif
            if (pc == null) {
                Debug.LogError(
                    "Could not find PlaylistController prefab. Please update the Installation Path in the Master Audio Manager window if you have moved the folder from its default location, then try again.");
                return null;
            }

            // ReSharper disable once ArrangeStaticMemberQualifier
            var go = GameObject.Instantiate(pc) as GameObject;
            // ReSharper disable once PossibleNullReferenceException
            go.name = "PlaylistController";
            return go;
        }

        /// <summary>
        /// Creates a Dynamic Sound Group Creator prefab instance in the current Scene.
        /// </summary>
        public static GameObject CreateDynamicSoundGroupCreator() {
#if UNITY_EDITOR
            var pc = AssetDatabase.LoadAssetAtPath(MasterAudioFolderPath + "/Prefabs/DynamicSoundGroupCreator.prefab",
                typeof(GameObject));
#else
				var pc = Resources.Load(MasterAudioDefaultFolder + "/Prefabs/DynamicSoundGroupCreator.prefab", typeof(GameObject));
#endif
            if (pc == null) {
                Debug.LogError(
                    "Could not find DynamicSoundGroupCreator prefab. Please update the Installation Path in the Master Audio Manager window if you have moved the folder from its default location, then try again.");
                return null;
            }
            // ReSharper disable once ArrangeStaticMemberQualifier
            var go = GameObject.Instantiate(pc) as GameObject;
            // ReSharper disable once PossibleNullReferenceException
            go.name = "DynamicSoundGroupCreator";
            return go;
        }

        /// <summary>
        /// Creates a Sound Group Organizer prefab instance in the current Scene.
        /// </summary>
        public static GameObject CreateSoundGroupOrganizer() {
#if UNITY_EDITOR
            var pc = AssetDatabase.LoadAssetAtPath(MasterAudioFolderPath + "/Prefabs/SoundGroupOrganizer.prefab",
                typeof(GameObject));
#else
				var pc = Resources.Load(MasterAudioDefaultFolder + "/Prefabs/SoundGroupOrganizer.prefab", typeof(GameObject));
#endif
            if (pc == null) {
                Debug.LogError(
                    "Could not find SoundGroupOrganizer prefab. Please update the Installation Path in the Master Audio Manager window if you have moved the folder from its default location, then try again.");
                return null;
            }
            // ReSharper disable once ArrangeStaticMemberQualifier
            var go = GameObject.Instantiate(pc) as GameObject;
            // ReSharper disable once PossibleNullReferenceException
            go.name = "SoundGroupOrganizer";
            return go;
        }
		/*! \endcond */

        #endregion
    }
}