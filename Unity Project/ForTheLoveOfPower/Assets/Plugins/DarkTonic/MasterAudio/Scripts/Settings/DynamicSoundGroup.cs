/*! \cond PRIVATE */
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    // ReSharper disable once CheckNamespace
    public class DynamicSoundGroup : MonoBehaviour {
        // ReSharper disable InconsistentNaming
        public GameObject variationTemplate;

        public bool alwaysHighestPriority = false;
        public float groupMasterVolume = 1f;
        public int retriggerPercentage = 50;
        public MasterAudioGroup.VariationSequence curVariationSequence = MasterAudioGroup.VariationSequence.Randomized;
        public bool useNoRepeatRefill = true;

        public bool useInactivePeriodPoolRefill = false;
        public float inactivePeriodSeconds = 5f;
        public MasterAudioGroup.VariationMode curVariationMode = MasterAudioGroup.VariationMode.Normal;
        public MasterAudio.AudioLocation bulkVariationMode = MasterAudio.AudioLocation.Clip;

        public float chainLoopDelayMin;
        public float chainLoopDelayMax;
        public MasterAudioGroup.ChainedLoopLoopMode chainLoopMode = MasterAudioGroup.ChainedLoopLoopMode.Endless;
        public int chainLoopNumLoops = 0;
        public bool useDialogFadeOut = false;
        public float dialogFadeOutTime = .5f;

        public bool resourceClipsAllLoadAsync = true;
        public bool logSound = false;

        public bool soundPlayedEventActive = false;
        public string soundPlayedCustomEvent = string.Empty;

        public int busIndex = -1;

#if UNITY_5
        public MasterAudio.ItemSpatialBlendType spatialBlendType = MasterAudio.ItemSpatialBlendType.ForceTo3D;
		public float spatialBlend = 1f;
#endif

        public string busName = string.Empty; // only used to remember the bus name during group creation.
		public bool isExistingBus; // marked from DGSC's only
		public bool isCopiedFromDGSC; // so we know if it's existing Buses or new.

        public MasterAudioGroup.LimitMode limitMode = MasterAudioGroup.LimitMode.None;
        public int limitPerXFrames = 1;
        public float minimumTimeBetween = 0.1f;
        public bool limitPolyphony = false;
        public int voiceLimitCount = 1;

        public MasterAudioGroup.TargetDespawnedBehavior targetDespawnedBehavior =
            MasterAudioGroup.TargetDespawnedBehavior.FadeOut;

        public float despawnFadeTime = .3f;

        public bool isUsingOcclusion;

        public bool copySettingsExpanded = false;
        public int selectedVariationIndex = 0;

        public MasterAudioGroup.ChildGroupMode childGroupMode = MasterAudioGroup.ChildGroupMode.None;
        public List<string> childSoundGroups = new List<string>();

        public List<DynamicGroupVariation> groupVariations = new List<DynamicGroupVariation>();
        // filled and used by Inspector only

        // ReSharper restore InconsistentNaming
    }
}
/*! \endcond */