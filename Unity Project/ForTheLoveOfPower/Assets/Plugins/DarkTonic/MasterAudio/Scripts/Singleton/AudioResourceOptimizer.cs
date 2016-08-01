/*! \cond PRIVATE */
// ReSharper disable once RedundantUsingDirective
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    // ReSharper disable once CheckNamespace
    public static class AudioResourceOptimizer {
        private static readonly Dictionary<string, List<AudioSource>> AudioResourceTargetsByName =
            new Dictionary<string, List<AudioSource>>();

        private static readonly Dictionary<string, AudioClip> AudioClipsByName = new Dictionary<string, AudioClip>();

        private static readonly Dictionary<string, List<AudioClip>> PlaylistClipsByPlaylistName =
            new Dictionary<string, List<AudioClip>>(5);

        private static readonly List<string> InternetFilesStartedLoading = new List<string>();

        private static string _supportedLanguageFolder = string.Empty;

        /// <summary>
        /// Called in MasterAudio Awake
        /// </summary>
        public static void ClearAudioClips() {
            AudioClipsByName.Clear();
            AudioResourceTargetsByName.Clear();
        }

        public static string GetLocalizedDynamicSoundGroupFileName(SystemLanguage localLanguage, bool useLocalization,
            string resourceFileName) {
            if (!useLocalization) {
                return resourceFileName;
            }

            if (MasterAudio.Instance != null) {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                return GetLocalizedFileName(useLocalization, resourceFileName);
            }

            return localLanguage.ToString() + "/" + resourceFileName;
        }

        public static string GetLocalizedFileName(bool useLocalization, string resourceFileName) {
            return useLocalization ? SupportedLanguageFolder() + "/" + resourceFileName : resourceFileName;
        }

        public static void AddTargetForClip(string clipName, AudioSource source) {
            if (!AudioResourceTargetsByName.ContainsKey(clipName)) {
                AudioResourceTargetsByName.Add(clipName, new List<AudioSource> {
                    source
                });
            } else {
                var sources = AudioResourceTargetsByName[clipName];

                // populate the audio clip even if it was loaded previous by another
                AudioClip populatedClip = null;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < sources.Count; i++) {
                    var clip = sources[i].clip;

                    if (clip == null) {
                        continue;
                    }

                    populatedClip = clip;
                    break;
                }

                if (populatedClip != null) {
                    source.clip = populatedClip;
                    var aVar = source.GetComponent<SoundGroupVariation>();

                    if (aVar != null) {
                        aVar.internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Loaded;
                    }
                }

                sources.Add(source);
            }
        }

        private static string SupportedLanguageFolder() {
            if (!string.IsNullOrEmpty(_supportedLanguageFolder)) {
                return _supportedLanguageFolder;
            }

            var curLanguage = Application.systemLanguage;

            if (MasterAudio.Instance != null) {
                switch (MasterAudio.Instance.langMode) {
                    case MasterAudio.LanguageMode.SpecificLanguage:
                        curLanguage = MasterAudio.Instance.testLanguage;
                        break;
                    case MasterAudio.LanguageMode.DynamicallySet:
                        curLanguage = MasterAudio.DynamicLanguage;
                        break;
                }
            }

            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (MasterAudio.Instance.supportedLanguages.Contains(curLanguage)) {
                _supportedLanguageFolder = curLanguage.ToString();
            } else {
                _supportedLanguageFolder = MasterAudio.Instance.defaultLanguage.ToString();
            }

            return _supportedLanguageFolder;
        }

        public static void ClearSupportLanguageFolder() {
            _supportedLanguageFolder = string.Empty;
        }

        public static AudioClip PopulateResourceSongToPlaylistController(string controllerName, string songResourceName,
            string playlistName) {
            var resAudioClip = Resources.Load(songResourceName) as AudioClip;

            if (resAudioClip == null) {
                MasterAudio.LogWarning("Resource file '" + songResourceName + "' could not be located from Playlist '" +
                                       playlistName + "'.");
                return null;
            }

            if (!AudioUtil.AudioClipWillPreload(resAudioClip)) {
                MasterAudio.LogWarning("Audio Clip for Resource file '" + songResourceName + "' from Playlist '" +
                    playlistName + "' has 'Preload Audio Data' turned off, which can cause audio glitches. Resource files should always Preload Audio Data. Please turn it on.");
            }

            FinishRecordingPlaylistClip(controllerName, resAudioClip);

            return resAudioClip;
        }

        private static void FinishRecordingPlaylistClip(string controllerName, AudioClip resAudioClip) {
            List<AudioClip> clips;

            if (!PlaylistClipsByPlaylistName.ContainsKey(controllerName)) {
                clips = new List<AudioClip>(5);
                PlaylistClipsByPlaylistName.Add(controllerName, clips);
            } else {
                clips = PlaylistClipsByPlaylistName[controllerName];
            }

            clips.Add(resAudioClip); // even needs to add duplicates
        }

#if UNITY_4_5_3 || UNITY_4_5_4 || UNITY_4_5_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5
        public static IEnumerator PopulateResourceSongToPlaylistControllerAsync(string songResourceName,
            string playlistName, PlaylistController controller, PlaylistController.AudioPlayType playType) {
            var asyncRes = Resources.LoadAsync(songResourceName, typeof(AudioClip));

            while (!asyncRes.isDone) {
                yield return MasterAudio.EndOfFrameDelay;
            }

            var resAudioClip = asyncRes.asset as AudioClip;

            if (resAudioClip == null) {
                MasterAudio.LogWarning("Resource file '" + songResourceName + "' could not be located from Playlist '" +
                                       playlistName + "'.");
                yield break;
            }

            if (!AudioUtil.AudioClipWillPreload(resAudioClip)) {
                MasterAudio.LogWarning("Audio Clip for Resource file '" + songResourceName + "' from Playlist '" +
                    playlistName + "' has 'Preload Audio Data' turned off, which can cause audio glitches. Resource files should always Preload Audio Data. Please turn it on.");
            }

            FinishRecordingPlaylistClip(controller.ControllerName, resAudioClip);

            controller.FinishLoadingNewSong(resAudioClip, playType);
        }

        /// <summary>
        /// Populates the sources with audio hosted on internet, non-thread blocking.
        /// </summary>
        /// <param name="fileUrl">URL of internet audio file.</param>
        /// <param name="variation">Variation.</param>
        /// <param name="successAction">Method to execute if successful.</param>
        /// <param name="failureAction">Method to execute if not successful.</param>
        // ReSharper disable RedundantNameQualifier
        public static IEnumerator PopulateSourceWithInternetFile(string fileUrl, SoundGroupVariation variation, System.Action successAction, System.Action failureAction) {
            // ReSharper restore RedundantNameQualifier
            if (AudioClipsByName.ContainsKey(fileUrl)) {
                if (successAction != null) {
                    successAction();
                }

                yield break;
            }

            if (InternetFilesStartedLoading.Contains(fileUrl)) { // don't download the same file multiple times.
                yield break;
            }

            InternetFilesStartedLoading.Add(fileUrl);

            AudioClip internetClip;

            using (var fileRequest = new WWW(fileUrl)) {
                yield return fileRequest;

                if (fileRequest.error != null) {
                    if (string.IsNullOrEmpty(fileUrl)) {
                        MasterAudio.LogWarning("Internet file is EMPTY for a Variation of Sound Group '" + variation.ParentGroup.name + "' could not be loaded.");
                    } else {
                        MasterAudio.LogWarning("Internet file '" + fileUrl + "' in a Variation of Sound Group '" + variation.ParentGroup.name + "' could not be loaded. This can happen if the URL is incorrect or you are not online.");
                    }
                    if (failureAction != null) {
                        failureAction();
                    }
                    yield break;
                }

                internetClip = fileRequest.audioClip;
            }

            if (!AudioResourceTargetsByName.ContainsKey(fileUrl)) {
                MasterAudio.LogError("No Audio Sources found to add Internet File '" + fileUrl + "' to.");

                if (failureAction != null) {
                    failureAction();
                }
                yield break;
            }

            var sources = AudioResourceTargetsByName[fileUrl];

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < sources.Count; i++) {
                sources[i].clip = internetClip;
                var aVar = sources[i].GetComponent<SoundGroupVariation>();

                if (aVar == null) {
                    continue;
                }

                aVar.internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Loaded;
            }

            if (!AudioClipsByName.ContainsKey(fileUrl)) {
                AudioClipsByName.Add(fileUrl, internetClip);
            }

            if (successAction != null) {
                successAction();
            }
        }

        public static void RemoveLoadedInternetClip(string fileUrl) {
            if (!InternetFilesStartedLoading.Contains(fileUrl)) {
                return;
            }

            InternetFilesStartedLoading.Remove(fileUrl);

            if (AudioResourceTargetsByName.ContainsKey(fileUrl)) {
                // get rid of clip references
                var sources = AudioResourceTargetsByName[fileUrl];
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < sources.Count; i++) {
                    // ReSharper disable once AccessToStaticMemberViaDerivedType
                    GameObject.Destroy(sources[i].clip); // this is the line that is necessary to free the memory again.
                    sources[i].clip = null;
                }
                
                AudioResourceTargetsByName.Remove(fileUrl);
            }

            if (AudioClipsByName.ContainsKey(fileUrl)) {
                AudioClipsByName.Remove(fileUrl);
            }

        }

        /// <summary>
        /// Populates the sources with resource clip, non-thread blocking.
        /// </summary>
        /// <param name="clipName">Clip name.</param>
        /// <param name="variation">Variation.</param>
        /// <param name="successAction">Method to execute if successful.</param>
        /// <param name="failureAction">Method to execute if not successful.</param>
        public static IEnumerator PopulateSourcesWithResourceClipAsync(string clipName, SoundGroupVariation variation,
            // ReSharper disable RedundantNameQualifier
            System.Action successAction, System.Action failureAction) {

            // ReSharper restore RedundantNameQualifier
            if (AudioClipsByName.ContainsKey(clipName)) {
                if (successAction != null) {
                    successAction();
                }

                yield break;
            }

            var asyncRes = Resources.LoadAsync(clipName, typeof(AudioClip));

            while (!asyncRes.isDone) {
                yield return MasterAudio.EndOfFrameDelay;
            }

            var resAudioClip = asyncRes.asset as AudioClip;

            if (resAudioClip == null) {
                MasterAudio.LogError("Resource file '" + clipName + "' could not be located.");

                if (failureAction != null) {
                    failureAction();
                }
                yield break;
            }

            if (!AudioResourceTargetsByName.ContainsKey(clipName)) {
                MasterAudio.LogError("No Audio Sources found to add Resource file '" + clipName + "'.");

                if (failureAction != null) {
                    failureAction();
                }
                yield break;
            }

            if (!AudioUtil.AudioClipWillPreload(resAudioClip)) {
                MasterAudio.LogWarning("Audio Clip for Resource file '" + clipName + "' of Sound Group '" + variation.ParentGroup.name + "' has 'Preload Audio Data' turned off, which can cause audio glitches. Resource files should always Preload Audio Data. Please turn it on.");
            }

            var sources = AudioResourceTargetsByName[clipName];

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < sources.Count; i++) {
                sources[i].clip = resAudioClip;
            }

            if (!AudioClipsByName.ContainsKey(clipName)) {
                AudioClipsByName.Add(clipName, resAudioClip);
            }

            if (successAction != null) {
                successAction();
            }
        }
#else
	public static IEnumerator PopulateResourceSongToPlaylistControllerAsync(string songResourceName, string playlistName, PlaylistController controller, PlaylistController.AudioPlayType playType) {
		MasterAudio.LogError("If this method got called, please report it to Dark Tonic immediately. It should not happen.");
		yield break;
	}

	public static IEnumerator PopulateSourcesWithResourceClipAsync(string clipName, SoundGroupVariation variation, Action successAction, Action failureAction) {
		MasterAudio.LogError("If this method got called, please report it to Dark Tonic immediately. It should not happen.");
		yield break;
	}
#endif

        public static void UnloadPlaylistSongIfUnused(string controllerName, AudioClip clipToRemove) {
            if (clipToRemove == null) {
                return; // no need
            }

            if (!PlaylistClipsByPlaylistName.ContainsKey(controllerName)) {
                return; // no resource clips have been played yet.
            }

            var clips = PlaylistClipsByPlaylistName[controllerName];
            if (!clips.Contains(clipToRemove)) {
                return; // this resource clip hasn't been played yet.
            }

            clips.Remove(clipToRemove);

            var hasDuplicateClip = clips.Contains(clipToRemove);

            if (!hasDuplicateClip) {
                Resources.UnloadAsset(clipToRemove);
            }
        }

        /// <summary>
        /// Populates the sources with resource clip.
        /// </summary>
        /// <returns><c>true</c>, if sources with resource clip was populated, <c>false</c> otherwise.</returns>
        /// <param name="clipName">Clip name.</param>
        /// <param name="variation">Variation.</param>
        public static bool PopulateSourcesWithResourceClip(string clipName, SoundGroupVariation variation) {
            if (AudioClipsByName.ContainsKey(clipName)) {
                //MasterAudio.Log("clip already exists: " + clipName);
                return true; // work is done already!
            }

            var resAudioClip = Resources.Load(clipName) as AudioClip;

            if (resAudioClip == null) {
                MasterAudio.LogError("Resource file '" + clipName + "' could not be located.");
                return false;
            }

            if (!AudioResourceTargetsByName.ContainsKey(clipName)) {
                MasterAudio.LogError("No Audio Sources found to add Resource file '" + clipName + "'.");
                return false;
            }
            
            var sources = AudioResourceTargetsByName[clipName];

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < sources.Count; i++) {
                sources[i].clip = resAudioClip;
            }

            if (!AudioUtil.AudioClipWillPreload(resAudioClip)) {
                MasterAudio.LogWarning("Audio Clip for Resource file '" + clipName + "' of Sound Group '" + variation.ParentGroup.name + "' has 'Preload Audio Data' turned off, which can cause audio glitches. Resource files should always Preload Audio Data. Please turn it on.");
            }

            AudioClipsByName.Add(clipName, resAudioClip);
            return true;
        }

        public static void DeleteAudioSourceFromList(string clipName, AudioSource source) {
            if (!AudioResourceTargetsByName.ContainsKey(clipName)) {
                MasterAudio.LogError("No Audio Sources found for Resource file '" + clipName + "'.");
                return;
            }

            var sources = AudioResourceTargetsByName[clipName];
            sources.Remove(source);

            if (sources.Count == 0) {
                AudioResourceTargetsByName.Remove(clipName);
            }
        }

        public static void UnloadClipIfUnused(string clipName) {
            if (!AudioClipsByName.ContainsKey(clipName)) {
                // already removed.
                return;
            }

            var sources = new List<AudioSource>();

            if (AudioResourceTargetsByName.ContainsKey(clipName)) {
                sources = AudioResourceTargetsByName[clipName];

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < sources.Count; i++) {
                    var aSource = sources[i];
                    var aVar = aSource.GetComponent<SoundGroupVariation>();

                    if (aVar.IsPlaying) {
                        return; // still something playing
                    }
                }
            }

            var clipToRemove = AudioClipsByName[clipName];

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < sources.Count; i++) {
                sources[i].clip = null;
            }

            AudioClipsByName.Remove(clipName);
            Resources.UnloadAsset(clipToRemove);
        }
    }
}
/*! \endcond */