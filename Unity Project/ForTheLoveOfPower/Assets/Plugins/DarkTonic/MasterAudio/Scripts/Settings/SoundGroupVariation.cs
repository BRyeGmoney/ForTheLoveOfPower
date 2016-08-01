using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    /// <summary>
    /// This class contains the actual Audio Source, Unity Filter FX components and other convenience methods having to do with playing sound effects.
    /// </summary>
    [AudioScriptOrder(-40)]
    [RequireComponent(typeof(SoundGroupVariationUpdater))]
    // ReSharper disable once CheckNamespace
    public class SoundGroupVariation : MonoBehaviour {
        /*! \cond PRIVATE */
        // ReSharper disable InconsistentNaming
        public int weight = 1;

        public bool useLocalization = false;

        public bool useRandomPitch = false;
        public RandomPitchMode randomPitchMode = RandomPitchMode.AddToClipPitch;
        public float randomPitchMin = 0f;
        public float randomPitchMax = 0f;

        public bool useRandomVolume = false;
        public RandomVolumeMode randomVolumeMode = RandomVolumeMode.AddToClipVolume;
        public float randomVolumeMin = 0f;
        public float randomVolumeMax = 0f;

        public MasterAudio.AudioLocation audLocation = MasterAudio.AudioLocation.Clip;
        public string resourceFileName;
        public string internetFileUrl;
        public MasterAudio.InternetFileLoadStatus internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Loading;
        public float fxTailTime = 0f;
        public float original_pitch;
        public float original_volume;
        public bool isExpanded = true;
        public bool isChecked = true;

        public bool useFades = false;
        public float fadeInTime = 0f;
        public float fadeOutTime = 0f;

        public bool useRandomStartTime = false;
        public float randomStartMinPercent = 0f;
        public float randomStartMaxPercent = 0f;

        public bool useIntroSilence = false;
        public float introSilenceMin = 0f;
        public float introSilenceMax = 0f;
        // ReSharper restore InconsistentNaming

        // ReSharper disable InconsistentNaming
        public float fadeMaxVolume;
        public FadeMode curFadeMode = FadeMode.None;
        public DetectEndMode curDetectEndMode = DetectEndMode.None;
        public int frames = 0;
        // ReSharper restore InconsistentNaming

        private AudioSource _audioSource;

        private readonly PlaySoundParams _playSndParam = new PlaySoundParams(string.Empty, 1f, 1f, 1f, null, false, 0f,
            false, false);

        private AudioDistortionFilter _distFilter;
        private AudioEchoFilter _echoFilter;
        private AudioHighPassFilter _hpFilter;
        private AudioLowPassFilter _lpFilter;
        private AudioReverbFilter _reverbFilter;
        private AudioChorusFilter _chorusFilter;
        private bool _isWaitingForDelay;
        private float _maxVol = 1f;
        private int _instanceId = -1;
        private bool? _audioLoops;
        private SoundGroupVariationUpdater _varUpdater;
        private int _previousSoundFinishedFrame = -1;
        private string _soundGroupName;

        public delegate void SoundFinishedEventHandler();

        /// <summary>
        /// Subscribe to this event to be notified when the sound stops playing.
        /// </summary>
        public event SoundFinishedEventHandler SoundFinished;

        private Transform _trans;
        private GameObject _go;
        private AudioSource _aud;
        private Transform _objectToFollow;
        private Transform _objectToTriggerFrom;
        private MasterAudioGroup _parentGroupScript;
        private bool _attachToSource;
        private string _resFileName = string.Empty;

        public class PlaySoundParams {
            public string SoundType;
            public float VolumePercentage;
            public float? Pitch;
            public Transform SourceTrans;
            public bool AttachToSource;
            public float DelaySoundTime;
            public bool IsChainLoop;
            public bool IsSingleSubscribedPlay;
            public float GroupCalcVolume;
            public bool IsPlaying;

            public PlaySoundParams(string soundType, float volPercent, float groupCalcVolume, float? pitch,
                Transform sourceTrans, bool attach, float delaySoundTime, bool isChainLoop, bool isSingleSubscribedPlay) {
                SoundType = soundType;
                VolumePercentage = volPercent;
                GroupCalcVolume = groupCalcVolume;
                Pitch = pitch;
                SourceTrans = sourceTrans;
                AttachToSource = attach;
                DelaySoundTime = delaySoundTime;
                IsChainLoop = isChainLoop;
                IsSingleSubscribedPlay = isSingleSubscribedPlay;
                IsPlaying = false;
            }
        }

        public enum FadeMode {
            None,
            FadeInOut,
            FadeOutEarly,
            GradualFade
        }

        public enum RandomPitchMode {
            AddToClipPitch,
            IgnoreClipPitch
        }

        public enum RandomVolumeMode {
            AddToClipVolume,
            IgnoreClipVolume
        }

        public enum DetectEndMode {
            None,
            DetectEnd
        }
        /*! \endcond */

        // ReSharper disable once UnusedMember.Local
        private void Awake() {
            original_pitch = VarAudio.pitch;
            original_volume = VarAudio.volume;
            _audioLoops = VarAudio.loop;
            var c = VarAudio.clip; // pre-warm the clip access
            var g = GameObj; // pre-warm the game object clip access

            if (c != null || g != null) { } // to disable the warning for not using it.

            if (VarAudio.playOnAwake) {
                Debug.LogWarning("The 'Play on Awake' checkbox in the Audio Source for Sound Group '" + ParentGroup.name +
                                 "', Variation '" + name +
                                 "' is checked. This is not used in Master Audio and can lead to buggy behavior. Make sure to uncheck it before hitting Play next time. To play ambient sounds, use an EventSounds component and activate the Start event to play a Sound Group of your choice.");
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void Start() {
            // this code needs to wait for cloning (for weight).
            var theParent = ParentGroup;
            if (theParent == null) {
                Debug.LogError("Sound Variation '" + name + "' has no parent!");
                return;
            }

            switch (audLocation) {
                case MasterAudio.AudioLocation.FileOnInternet:
                    if (internetFileLoadStatus == MasterAudio.InternetFileLoadStatus.Loading) {
                        LoadInternetFile();
                    }
                    break;
            }

#if UNITY_5
            SetMixerGroup();
            SetSpatialBlend();
#endif

            SetPriority();

            SetOcclusion();
        }

#if UNITY_5
        public void SetMixerGroup() {
            var aBus = ParentGroup.BusForGroup;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (aBus != null) {
                VarAudio.outputAudioMixerGroup = aBus.mixerChannel;
            } else {
                VarAudio.outputAudioMixerGroup = null;
            }
        }

        public void SetSpatialBlend() {
            var blend = ParentGroup.SpatialBlendForGroup;
            if (blend != MasterAudioGroup.UseCurveSpatialBlend) {
                VarAudio.spatialBlend = blend;
            }

            var aBus = ParentGroup.BusForGroup;
            if (aBus != null && MasterAudio.Instance.mixerSpatialBlendType != MasterAudio.AllMixerSpatialBlendType.ForceAllTo2D && aBus.forceTo2D) {
                VarAudio.spatialBlend = 0;
            }
        }
#endif

        /*! \cond PRIVATE */
        public void LoadInternetFile() {
            StartCoroutine(AudioResourceOptimizer.PopulateSourceWithInternetFile(internetFileUrl, this, InternetFileLoaded, InternetFileFailedToLoad));
        }

        private void SetOcclusion() {
            VariationUpdater.UpdateCachedObjects();
            var doesGroupUseOcclusion = UsesOcclusion;

            if (!doesGroupUseOcclusion) {
                return;
            }

            // set occlusion default
            if (LowPassFilter == null) {
                _lpFilter = gameObject.AddComponent<AudioLowPassFilter>();
            }

            // ReSharper disable once PossibleNullReferenceException
            LowPassFilter.cutoffFrequency = AudioUtil.MinCutoffFreq;
        }
        /*! \endcond */

        private void SetPriority() {
            if (!MasterAudio.Instance.prioritizeOnDistance) {
                return;
            }
            if (ParentGroup.alwaysHighestPriority) {
                AudioPrioritizer.Set2DSoundPriority(VarAudio);
            } else {
                AudioPrioritizer.SetSoundGroupInitialPriority(VarAudio);
            }
        }

        /// <summary>
        /// Do not call this! It's called by Master Audio after it is  done initializing.
        /// </summary>
        public void DisableUpdater() {
            if (VariationUpdater == null) {
                return;
            }

            VariationUpdater.enabled = false;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDestroy() {
            StopSoundEarly();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable() {
            StopSoundEarly();
        }

        private void StopSoundEarly() {
            if (MasterAudio.AppIsShuttingDown) {
                return;
            }

            Stop(); // maybe unload clip from Resources
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDrawGizmos() {
            if (MasterAudio.Instance.showGizmos && IsPlaying) {
                Gizmos.DrawIcon(transform.position, MasterAudio.GizmoFileName, true);
            }
        }

        /*! \cond PRIVATE */
        public void Play(float? pitch, float maxVolume, string gameObjectName, float volPercent, float targetVol,
            float? targetPitch, Transform sourceTrans, bool attach, float delayTime, bool isChaining,
            bool isSingleSubscribedPlay) {

            if (!MasterAudio.IsWarming && audLocation == MasterAudio.AudioLocation.FileOnInternet) {
                switch (internetFileLoadStatus) {
                    case MasterAudio.InternetFileLoadStatus.Loading:
                        if (MasterAudio.Instance.LogSounds) {
                            MasterAudio.LogWarning("Cannot play Variation '" + name +
                                                   "' because its Internet file has not been downloaded yet.");
                        }
                        return;
                    case MasterAudio.InternetFileLoadStatus.Failed:
                        if (MasterAudio.Instance.LogSounds) {
                            MasterAudio.LogWarning("Cannot play Variation '" + name +
                                                   "' because its Internet file failed downloading.");
                        }
                        return;
                }
            }

            SoundFinished = null; // clear it out so subscribers don't have to clean up
            _isWaitingForDelay = false;

            _playSndParam.SoundType = gameObjectName;
            _playSndParam.VolumePercentage = volPercent;
            _playSndParam.GroupCalcVolume = targetVol;
            _playSndParam.Pitch = targetPitch;
            _playSndParam.SourceTrans = sourceTrans;
            _playSndParam.AttachToSource = attach;
            _playSndParam.DelaySoundTime = delayTime;
            _playSndParam.IsChainLoop = isChaining ||
                                        ParentGroup.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain;
            _playSndParam.IsSingleSubscribedPlay = isSingleSubscribedPlay;
            _playSndParam.IsPlaying = true;

            SetPriority(); // reset it back to normal priority in case you're playing 2D this time.

            if (MasterAudio.HasAsyncResourceLoaderFeature() && ShouldLoadAsync) {
                StopAllCoroutines(); // The only Coroutine right now requires pro version and Unity 4.5.3
            }

            // compute pitch
            if (pitch.HasValue) {
                VarAudio.pitch = pitch.Value;
            } else if (useRandomPitch) {
                var randPitch = Random.Range(randomPitchMin, randomPitchMax);

                switch (randomPitchMode) {
                    case RandomPitchMode.AddToClipPitch:
                        randPitch += OriginalPitch;
                        break;
                }

                VarAudio.pitch = randPitch;
            } else {
                // non random pitch
                VarAudio.pitch = OriginalPitch;
            }

#if UNITY_5
            // in case it was changed at runtime.
            SetSpatialBlend();
#endif

            // set fade mode
            curFadeMode = FadeMode.None;
            curDetectEndMode = DetectEndMode.DetectEnd;
            _maxVol = maxVolume;

            switch (audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    FinishSetupToPlay();
                    return;
                case MasterAudio.AudioLocation.ResourceFile:
                    if (MasterAudio.HasAsyncResourceLoaderFeature() && ShouldLoadAsync) {
                        StartCoroutine(AudioResourceOptimizer.PopulateSourcesWithResourceClipAsync(ResFileName, this,
                            FinishSetupToPlay, ResourceFailedToLoad));
                    } else {
                        if (!AudioResourceOptimizer.PopulateSourcesWithResourceClip(ResFileName, this)) {
                            return; // audio file not found!
                        }

                        FinishSetupToPlay();
                    }
                    return;
                case MasterAudio.AudioLocation.FileOnInternet:
                    FinishSetupToPlay();
                    return;
            }
        }
        /*! \endcond */

        private void InternetFileFailedToLoad() {
            internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Failed;
        }

        private void InternetFileLoaded() {
            if (MasterAudio.Instance.LogSounds) {
                MasterAudio.LogWarning("Internet file: '" + internetFileUrl + "' loaded successfully.");
            }

            internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Loaded;
        }

        private void ResourceFailedToLoad() {
            Stop(); // to stop other behavior and disable the Updater script.
        }

        private void FinishSetupToPlay() {
            if (!VarAudio.isPlaying && VarAudio.time > 0f) {
                // paused. Do nothing except Play
            } else if (useFades && (fadeInTime > 0f || fadeOutTime > 0f)) {
                fadeMaxVolume = _maxVol;

                if (fadeInTime > 0f) {
                    VarAudio.volume = 0f;
                }

                if (VariationUpdater != null) {
                    VariationUpdater.enabled = true;
                    VariationUpdater.FadeInOut();
                }
            }

            VarAudio.loop = AudioLoops;
            // restore original loop setting in case it got lost by loop setting code below for a previous play.

            if (_playSndParam.IsPlaying && (_playSndParam.IsChainLoop || _playSndParam.IsSingleSubscribedPlay)) {
                VarAudio.loop = false;
            }

            if (!_playSndParam.IsPlaying) {
                return; // has already been "stop" 'd.
            }

            ParentGroup.AddActiveAudioSourceId(InstanceId);

            if (VariationUpdater != null) {
                VariationUpdater.enabled = true;
                VariationUpdater.WaitForSoundFinish(_playSndParam.DelaySoundTime);
            }

            _attachToSource = false;

            var useClipAgePriority = MasterAudio.Instance.prioritizeOnDistance &&
                                     (MasterAudio.Instance.useClipAgePriority || ParentGroup.useClipAgePriority);

            if (!_playSndParam.AttachToSource && !useClipAgePriority) {
                return;
            }
            _attachToSource = _playSndParam.AttachToSource;

            if (VariationUpdater != null) {
                VariationUpdater.FollowObject(_attachToSource, ObjectToFollow, useClipAgePriority);
            }
        }

        /// <summary>
        /// This method allows you to jump to a specific time in an already playing or just triggered Audio Clip.
        /// </summary>
        /// <param name="timeToJumpTo">The time in seconds to jump to.</param>
        public void JumpToTime(float timeToJumpTo) {
            if (!_playSndParam.IsPlaying) {
                return;
            }

            VarAudio.time = timeToJumpTo;
        }

        /// <summary>
        /// This method allows you to adjust the volume of an already playing clip, accounting for bus volume, mixer volume and group volume.
        /// </summary>
        /// <param name="volumePercentage"></param>
        public void AdjustVolume(float volumePercentage) {
            if (!VarAudio.isPlaying || !_playSndParam.IsPlaying) {
                return;
            }

            var newVol = _playSndParam.GroupCalcVolume * volumePercentage;
            VarAudio.volume = newVol;

            _playSndParam.VolumePercentage = volumePercentage;
        }

        /// <summary>
        /// This method allows you to pause the audio being played by this Variation. This is automatically called by MasterAudio.PauseSoundGroup and MasterAudio.PauseBus.
        /// </summary>
        public void Pause() {
            if (audLocation == MasterAudio.AudioLocation.ResourceFile && !MasterAudio.Instance.resourceClipsPauseDoNotUnload) {
                Stop();
                return;
            }

            VarAudio.Pause();
            curFadeMode = FadeMode.None;
            if (VariationUpdater != null) {
                VariationUpdater.StopWaitingForFinish(); // necessary so that the clip can be unpaused.
            }

            MaybeUnloadClip();
        }

        private void MaybeUnloadClip() {
            if (audLocation == MasterAudio.AudioLocation.ResourceFile) {
                AudioResourceOptimizer.UnloadClipIfUnused(_resFileName);
            }

            AudioUtil.UnloadNonPreloadedAudioData(VarAudio.clip);
        }

        /// <summary>
        /// This method allows you to stop the audio being played by this Variation. 
        /// </summary>
        public void Stop(bool stopEndDetection = false) {
            var waitStopped = false;

            if (stopEndDetection || _isWaitingForDelay) {
                if (VariationUpdater != null) {
                    VariationUpdater.StopWaitingForFinish(); // turn off the chain loop endless repeat
                    waitStopped = true;
                }
            }

            _objectToFollow = null;
            _objectToTriggerFrom = null;
            ParentGroup.RemoveActiveAudioSourceId(InstanceId);
            MasterAudio.StopTrackingOcclusionForSource(GameObj);

            VarAudio.Stop();

            VarAudio.time = 0f;
            if (VariationUpdater != null) {
                VariationUpdater.StopFollowing();
                VariationUpdater.StopFading();
            }

            if (!waitStopped) {
                if (VariationUpdater != null) {
                    VariationUpdater.StopWaitingForFinish();
                }
            }

            _playSndParam.IsPlaying = false;

            if (SoundFinished != null) {
                var willAbort = _previousSoundFinishedFrame == Time.frameCount;
                _previousSoundFinishedFrame = Time.frameCount;

                if (!willAbort) {
                    SoundFinished(); // parameters aren't used
                }
                SoundFinished = null; // clear it out so subscribers don't have to clean up
            }

            Trans.localPosition = Vector3.zero;

            MaybeUnloadClip();
        }

        /// <summary>
        /// This method allows you to fade the sound from this Variation to a specified volume over X seconds.
        /// </summary>
        /// <param name="newVolume">The target volume to fade to.</param>
        /// <param name="fadeTime">The time it will take to fully fade to the target volume.</param>
        public void FadeToVolume(float newVolume, float fadeTime) {
            if (newVolume < 0f || newVolume > 1f) {
                Debug.LogError("Illegal volume passed to FadeToVolume: '" + newVolume +
                               "'. Legal volumes are between 0 and 1");
                return;
            }

            if (fadeTime <= MasterAudio.InnerLoopCheckInterval) {
                VarAudio.volume = newVolume; // time really short, just do it at once.
                if (VarAudio.volume <= 0f) {
                    Stop();
                }
                return;
            }

            if (VariationUpdater != null) {
                VariationUpdater.FadeOverTimeToVolume(newVolume, fadeTime);
            }
        }

        /// <summary>
        /// This method will fully fade out the sound from this Variation to zero using its existing fadeOutTime.
        /// </summary>
        public void FadeOutNow() {
            if (MasterAudio.AppIsShuttingDown) {
                return;
            }

            if (IsPlaying && useFades && VariationUpdater != null) {
                VariationUpdater.FadeOutEarly(fadeOutTime);
            }
        }

        /// <summary>
        /// This method will fully fade out the sound from this Variation to zero using over X seconds.
        /// </summary>
        /// <param name="fadeTime">The time it will take to fully fade to the target volume.</param>
        public void FadeOutNow(float fadeTime) {
            if (MasterAudio.AppIsShuttingDown) {
                return;
            }

            if (IsPlaying && VariationUpdater != null) {
                VariationUpdater.FadeOutEarly(fadeTime);
            }
        }

        /*! \cond PRIVATE */
        public bool WasTriggeredFromTransform(Transform trans) {
            if (ObjectToFollow == trans || ObjectToTriggerFrom == trans) {
                return true;
            }

            return false;
        }
        /*! \endcond */

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity Distortion Filter FX component.
        /// </summary>
        public AudioDistortionFilter DistortionFilter {
            get {
                if (_distFilter != null) {
                    return _distFilter;
                }
                _distFilter = GetComponent<AudioDistortionFilter>();

                return _distFilter;
            }
        }

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity Reverb Filter FX component.
        /// </summary>
        public AudioReverbFilter ReverbFilter {
            get {
                if (_reverbFilter != null) {
                    return _reverbFilter;
                }
                _reverbFilter = GetComponent<AudioReverbFilter>();

                return _reverbFilter;
            }
        }

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity Chorus Filter FX component.
        /// </summary>
        public AudioChorusFilter ChorusFilter {
            get {
                if (_chorusFilter != null) {
                    return _chorusFilter;
                }
                _chorusFilter = GetComponent<AudioChorusFilter>();

                return _chorusFilter;
            }
        }

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity Echo Filter FX component.
        /// </summary>
        public AudioEchoFilter EchoFilter {
            get {
                if (_echoFilter != null) {
                    return _echoFilter;
                }
                _echoFilter = GetComponent<AudioEchoFilter>();

                return _echoFilter;
            }
        }

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity Low Pass Filter FX component.
        /// </summary>
        public AudioLowPassFilter LowPassFilter {
            get {
                if (_lpFilter != null) {
                    return _lpFilter;
                }

                _lpFilter = GetComponent<AudioLowPassFilter>();

                return _lpFilter;
            }
        }

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity High Pass Filter FX component.
        /// </summary>
        public AudioHighPassFilter HighPassFilter {
            get {
                if (_hpFilter != null) {
                    return _hpFilter;
                }
                _hpFilter = GetComponent<AudioHighPassFilter>();

                return _hpFilter;
            }
        }

        /*! \cond PRIVATE */
        public Transform ObjectToFollow {
            get { return _objectToFollow; }
            set { _objectToFollow = value; }
        }

        public Transform ObjectToTriggerFrom {
            get { return _objectToTriggerFrom; }
            set { _objectToTriggerFrom = value; }
        }
        /*! \endcond */

        /// <summary>
        /// This property will return whether there are any Unity FX Filters enabled on this Variation.
        /// </summary>
        public bool HasActiveFXFilter {
            get {
                if (HighPassFilter != null && HighPassFilter.enabled) {
                    return true;
                }
                if (LowPassFilter != null && LowPassFilter.enabled) {
                    return true;
                }
                if (ReverbFilter != null && ReverbFilter.enabled) {
                    return true;
                }
                if (DistortionFilter != null && DistortionFilter.enabled) {
                    return true;
                }
                if (EchoFilter != null && EchoFilter.enabled) {
                    return true;
                }
                if (ChorusFilter != null && ChorusFilter.enabled) {
                    return true;
                }

                return false;
            }
        }

        /*! \cond PRIVATE */
        public MasterAudioGroup ParentGroup {
            get {
                if (Trans.parent == null) {
                    return null; // project view
                }

                if (_parentGroupScript == null) {
                    _parentGroupScript = Trans.parent.GetComponent<MasterAudioGroup>();
                }

                if (_parentGroupScript == null) {
                    Debug.LogError("The Group that Sound Variation '" + name +
                                   "' is in does not have a MasterAudioGroup script in it!");
                }

                return _parentGroupScript;
            }
        }
        /*! \endcond */

        /// <summary>
        /// This property will return the original pitch of the Variation.
        /// </summary>
        public float OriginalPitch {
            get {
                if (original_pitch == 0f) {
                    // lazy lookup for race conditions.
                    original_pitch = VarAudio.pitch;
                }

                return original_pitch;
            }
        }

        /// <summary>
        /// This property will return the original volume of the Variation.
        /// </summary>
        public float OriginalVolume {
            get {
                if (original_volume == 0f) {
                    // lazy lookup for race conditions.
                    original_volume = VarAudio.volume;
                }

                return original_volume;
            }
        }

        /// <summary>
        /// This returns the name of the Sound Group the Variation belongs to.
        /// </summary>
        public string SoundGroupName {
            get {
                if (_soundGroupName != null) {
                    return _soundGroupName;
                }

                _soundGroupName = ParentGroup.GameObjectName;
                return _soundGroupName;
            }
        }

        /*! \cond PRIVATE */
        public bool IsAvailableToPlay {
            get {
                if (weight == 0) {
                    return false;
                }

                if (!_playSndParam.IsPlaying && VarAudio.time == 0f) {
                    return true; // paused aren't available
                }

                return AudioUtil.GetAudioPlayedPercentage(VarAudio) >= ParentGroup.retriggerPercentage;
            }
        }
        /*! \endcond */

        /// <summary>
        /// This property will return the time of the last play of this Variation.
        /// </summary>
        public float LastTimePlayed { get; set; }

        /*! \cond PRIVATE */
        public int InstanceId {
            get {
                if (_instanceId < 0) {
                    _instanceId = GetInstanceID();
                }

                return _instanceId;
            }
        }

        public Transform Trans {
            get {
                if (_trans != null) {
                    return _trans;
                }
                _trans = transform;

                return _trans;
            }
        }

        public GameObject GameObj {
            get {
                if (_go != null) {
                    return _go;
                }
                _go = gameObject;

                return _go;
            }
        }

        public AudioSource VarAudio {
            get {
                if (_audioSource != null) {
                    return _audioSource;
                }

                _audioSource = GetComponent<AudioSource>();

                return _audioSource;
            }
        }

        public bool AudioLoops {
            get {
                if (!_audioLoops.HasValue) {
                    _audioLoops = VarAudio.loop;
                }

                return _audioLoops.Value;
            }
        }

        public string ResFileName {
            get {
                if (string.IsNullOrEmpty(_resFileName)) {
                    _resFileName = AudioResourceOptimizer.GetLocalizedFileName(useLocalization, resourceFileName);
                }

                return _resFileName;
            }
        }

        public SoundGroupVariationUpdater VariationUpdater {
            get {
                if (_varUpdater != null) {
                    return _varUpdater;
                }

                _varUpdater = GetComponent<SoundGroupVariationUpdater>();

                return _varUpdater;
            }
        }

        public bool IsWaitingForDelay {
            get { return _isWaitingForDelay; }
            set { _isWaitingForDelay = value; }
        }

        public PlaySoundParams PlaySoundParm {
            get { return _playSndParam; }
        }

        public bool IsPlaying {
            get { return _playSndParam.IsPlaying; }
        }

        public float SetGroupVolume {
            get { return _playSndParam.GroupCalcVolume; }
            set { _playSndParam.GroupCalcVolume = value; }
        }

        private bool Is2D {
            get {
#if UNITY_5
                return VarAudio.spatialBlend <= 0;
#else
				return false;
#endif
            }
        }

        private bool ShouldLoadAsync {
            get {
                if (MasterAudio.Instance.resourceClipsAllLoadAsync) {
                    return true;
                }

                return ParentGroup.resourceClipsAllLoadAsync;
            }
        }

        public bool UsesOcclusion {
            get { 
                if (!VariationUpdater.MAThisFrame.useOcclusion) {
                    return false;
                }

                if (Is2D) {
                    return false;
                }

                switch (VariationUpdater.MAThisFrame.occlusionSelectType) {
                    case MasterAudio.OcclusionSelectionType.AllGroups:
                        return true;
                    case MasterAudio.OcclusionSelectionType.TurnOnPerBusOrGroup:
                        var theBus = ParentGroup.BusForGroup;
                        if (theBus != null && theBus.isUsingOcclusion) {
                            return true;
                        }
                        
                        return ParentGroup.isUsingOcclusion;
                }

                // unreachable, but wanted by compiler
                return false;
            }
        }

        public void ClearSubscribers() {
            SoundFinished = null;
        }
        /*! \endcond */
    }
}