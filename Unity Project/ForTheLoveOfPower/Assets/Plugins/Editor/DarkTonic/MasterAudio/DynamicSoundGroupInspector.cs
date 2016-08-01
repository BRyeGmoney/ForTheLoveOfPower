using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DynamicSoundGroup))]
// ReSharper disable once CheckNamespace
public class DynamicSoundGroupInspector : Editor {
    private DynamicSoundGroup _group;
    private bool _isValid = true;
    private List<string> _groupNames;
    private GameObject _previewer;
    private DynamicSoundGroupCreator _dgsc;
    private List<string> _customEventNames;

    // ReSharper disable once FunctionComplexityOverflow
    public override void OnInspectorGUI() {
        EditorGUI.indentLevel = 0;
        var isDirty = false;
        _isValid = true;

        _group = (DynamicSoundGroup)target;

        _group = RescanChildren(_group);

        if (_group == null) {
            return;
        }

        var theParent = _group.transform.parent;
        if (theParent != null) {
            _previewer = theParent.gameObject;
        }

        if (_previewer == null) {
            DTGUIHelper.ShowRedError("This prefab must have a GameObject above it. Prefab broken.");
            _isValid = false;
        }

        var previewLang = SystemLanguage.English;

        if (!_isValid) {
            return;
        }

        _dgsc = _previewer.GetComponent<DynamicSoundGroupCreator>();
        if (_dgsc != null) {
            previewLang = _dgsc.previewLanguage;
        }

        var ma = MasterAudio.Instance;
        var maInScene = ma != null;

        if (maInScene) {
            _groupNames = ma.GroupNames;
            _groupNames.Remove(_group.name);
            _customEventNames = ma.CustomEventNames;
        }

        var isInProjectView = DTGUIHelper.IsPrefabInProjectView(_group);

        if (MasterAudioInspectorResources.LogoTexture != null) {
            DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.LogoTexture);
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUI.contentColor = DTGUIHelper.BrightButtonColor;
        if (GUILayout.Button(new GUIContent("Up to Parent", "Select Group in Hierarchy"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
            Selection.activeObject = _group.transform.parent.gameObject;
        }
        GUI.contentColor = Color.white;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        DTGUIHelper.HelpHeader("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/SoundGroups.htm");

        var newVol = DTGUIHelper.DisplayVolumeField(_group.groupMasterVolume, DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true, "Group Master Volume");
        if (newVol != _group.groupMasterVolume) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Group Master Volume");
            _group.groupMasterVolume = newVol;
        }

        EditorGUILayout.BeginHorizontal();
        var newTargetGone = (MasterAudioGroup.TargetDespawnedBehavior)EditorGUILayout.EnumPopup("Caller Despawned Mode", _group.targetDespawnedBehavior);
        DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/SoundGroups.htm#CallerDespawned");
        EditorGUILayout.EndHorizontal();
        if (newTargetGone != _group.targetDespawnedBehavior) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "Change Caller Despawned Mode");
            _group.targetDespawnedBehavior = newTargetGone;
        }

        if (_group.targetDespawnedBehavior == MasterAudioGroup.TargetDespawnedBehavior.FadeOut) {
            EditorGUI.indentLevel = 1;
            var newFade = EditorGUILayout.Slider("Fade Out Time", _group.despawnFadeTime, .1f, 20f);
            if (newFade != _group.despawnFadeTime) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "Change Called Despawned Fade Out Time");
                _group.despawnFadeTime = newFade;
            }
        }

        var groupHasResource = false;
        foreach (var t in _group.groupVariations) {
            if (t.audLocation != MasterAudio.AudioLocation.ResourceFile) {
                continue;
            }

            groupHasResource = true;
            break;
        }

        if (MasterAudio.HasAsyncResourceLoaderFeature() && groupHasResource) {
            if ((maInScene && !ma.resourceClipsAllLoadAsync) || !maInScene) {
                var newAsync = EditorGUILayout.Toggle(new GUIContent("Load Resources Async", "Checking this means Resource files in this Sound Group will be loaded asynchronously."), _group.resourceClipsAllLoadAsync);
                if (newAsync != _group.resourceClipsAllLoadAsync) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "toggle Load Resources Async");
                    _group.resourceClipsAllLoadAsync = newAsync;
                }
            }
        }

        if (!maInScene || ma.prioritizeOnDistance) {
            var hiPri = EditorGUILayout.Toggle("Always Highest Priority", _group.alwaysHighestPriority);
            if (hiPri != _group.alwaysHighestPriority) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "toggle Always Highest Priority");
                _group.alwaysHighestPriority = hiPri;
            }
        }

        if (!maInScene || (MasterAudio.Instance.useOcclusion && MasterAudio.Instance.occlusionSelectType == MasterAudio.OcclusionSelectionType.TurnOnPerBusOrGroup)) {
#if UNITY_5
            if (!maInScene) {
                DTGUIHelper.ShowLargeBarAlert("The Occlusion setting below will only be used if the Master Audio prefab is set to allow Occlusion.");
            }
#endif
            var newOcc = EditorGUILayout.Toggle("Use Occlusion", _group.isUsingOcclusion);
            if (newOcc != _group.isUsingOcclusion) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "toggle Use Occlusion");
                _group.isUsingOcclusion = newOcc;
            }
        }

        var newLog = EditorGUILayout.Toggle("Log Sounds", _group.logSound);
        if (newLog != _group.logSound) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "toggle Log Sounds");
            _group.logSound = newLog;
        }

#if UNITY_5
        DTGUIHelper.ShowLargeBarAlert("The Spatial Blend Rule below will only be used if the Master Audio prefab allows.");

        DTGUIHelper.StartGroupHeader();
        var newSpatialType = (MasterAudio.ItemSpatialBlendType)EditorGUILayout.EnumPopup("Spatial Blend Rule", _group.spatialBlendType);
        if (newSpatialType != _group.spatialBlendType) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Spatial Blend Rule");
            _group.spatialBlendType = newSpatialType;
        }
        EditorGUILayout.EndVertical();

        switch (_group.spatialBlendType) {
            case MasterAudio.ItemSpatialBlendType.ForceToCustom:
                EditorGUI.indentLevel = 0;
                DTGUIHelper.ShowLargeBarAlert(MasterAudioInspector.SpatialBlendSliderText);
                var newBlend = EditorGUILayout.Slider("Spatial Blend", _group.spatialBlend, 0f, 1f);
                if (newBlend != _group.spatialBlend) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Spatial Blend");
                    _group.spatialBlend = newBlend;
                }
                break;
        }
        EditorGUILayout.EndVertical();
#endif

        EditorGUI.indentLevel = 0;

        DTGUIHelper.StartGroupHeader();

        var newVarSequence = (MasterAudioGroup.VariationSequence)EditorGUILayout.EnumPopup("Variation Sequence", _group.curVariationSequence);
        if (newVarSequence != _group.curVariationSequence) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Variation Sequence");
            _group.curVariationSequence = newVarSequence;
        }

        EditorGUILayout.EndVertical();

        switch (_group.curVariationSequence) {
            case MasterAudioGroup.VariationSequence.TopToBottom:
                var newUseInactive = EditorGUILayout.BeginToggleGroup(" Refill Variation Pool After Inactive Time", _group.useInactivePeriodPoolRefill);
                if (newUseInactive != _group.useInactivePeriodPoolRefill) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "toggle Inactive Refill");
                    _group.useInactivePeriodPoolRefill = newUseInactive;
                }

                EditorGUI.indentLevel = 1;
                var newInactivePeriod = EditorGUILayout.Slider(" Inactive Time (sec)", _group.inactivePeriodSeconds, .2f, 30f);
                if (newInactivePeriod != _group.inactivePeriodSeconds) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Inactive Time");
                    _group.inactivePeriodSeconds = newInactivePeriod;
                }

                EditorGUILayout.EndToggleGroup();
                break;
            case MasterAudioGroup.VariationSequence.Randomized:
                if (_group.groupVariations.Count >= MasterAudioGroup.MinNoRepeatVariations) {
                    var newRepeat = EditorGUILayout.Toggle("No-Repeat On Refill", _group.useNoRepeatRefill);
                    if (newRepeat != _group.useNoRepeatRefill) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change No-Repeat On Refill");
                        _group.useNoRepeatRefill = newRepeat;
                    }
                } else {
                    DTGUIHelper.ShowLargeBarAlert("No-Repeat is disabled unless you have at least 3 Variations.");
                }
                break;
        }
        EditorGUILayout.EndVertical();

        EditorGUI.indentLevel = 0;
        DTGUIHelper.AddSpaceForNonU5(2);
        DTGUIHelper.StartGroupHeader();
        EditorGUILayout.BeginHorizontal();
        var newVarMode = (MasterAudioGroup.VariationMode)EditorGUILayout.EnumPopup("Variation Mode", _group.curVariationMode);
        DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/SoundGroups.htm#VarMode");
        EditorGUILayout.EndHorizontal();
        if (newVarMode != _group.curVariationMode) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Variation Mode");
            _group.curVariationMode = newVarMode;
        }
        EditorGUILayout.EndVertical();

        EditorGUI.indentLevel = 0;
        switch (_group.curVariationMode) {
            case MasterAudioGroup.VariationMode.LoopedChain:
                DTGUIHelper.ShowColorWarning("In this mode, only one Variation can be played at a time.");

                var newLoopMode = (MasterAudioGroup.ChainedLoopLoopMode)EditorGUILayout.EnumPopup("Loop Mode", _group.chainLoopMode);
                if (newLoopMode != _group.chainLoopMode) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Loop Mode");
                    _group.chainLoopMode = newLoopMode;
                }

                if (_group.chainLoopMode == MasterAudioGroup.ChainedLoopLoopMode.NumberOfLoops) {
                    var newLoopCount = EditorGUILayout.IntSlider("Number of Loops", _group.chainLoopNumLoops, 1, 500);
                    if (newLoopCount != _group.chainLoopNumLoops) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Number of Loops");
                        _group.chainLoopNumLoops = newLoopCount;
                    }
                }

                var newDelayMin = EditorGUILayout.Slider("Clip Change Delay Min", _group.chainLoopDelayMin, 0f, 20f);
                if (newDelayMin != _group.chainLoopDelayMin) {
                    if (_group.chainLoopDelayMax < newDelayMin) {
                        _group.chainLoopDelayMax = newDelayMin;
                    }
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Chained Clip Delay Min");
                    _group.chainLoopDelayMin = newDelayMin;
                }

                var newDelayMax = EditorGUILayout.Slider("Clip Change Delay Max", _group.chainLoopDelayMax, 0f, 20f);
                if (newDelayMax != _group.chainLoopDelayMax) {
                    if (newDelayMax < _group.chainLoopDelayMin) {
                        newDelayMax = _group.chainLoopDelayMin;
                    }
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Chained Clip Delay Max");
                    _group.chainLoopDelayMax = newDelayMax;
                }
                break;
            case MasterAudioGroup.VariationMode.Normal:
                EditorGUILayout.BeginHorizontal();
                var newRetrigger = EditorGUILayout.IntSlider("Retrigger Percentage", _group.retriggerPercentage, 0, 100);
                DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/SoundGroups.htm#Retrigger");
                EditorGUILayout.EndHorizontal();
                if (newRetrigger != _group.retriggerPercentage) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Retrigger Percentage");
                    _group.retriggerPercentage = newRetrigger;
                }

                var newLimitPoly = EditorGUILayout.Toggle("Limit Polyphony", _group.limitPolyphony);
                if (newLimitPoly != _group.limitPolyphony) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "toggle Limit Polyphony");
                    _group.limitPolyphony = newLimitPoly;
                }
                if (_group.limitPolyphony) {
                    var maxVoices = 0;
                    foreach (var variation in _group.groupVariations) {
                        maxVoices += variation.weight;
                    }

                    var newVoiceLimit = EditorGUILayout.IntSlider("Polyphony Voice Limit", _group.voiceLimitCount, 1, maxVoices);
                    if (newVoiceLimit != _group.voiceLimitCount) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Polyphony Voice Limit");
                        _group.voiceLimitCount = newVoiceLimit;
                    }
                }

                var newLimitMode = (MasterAudioGroup.LimitMode)EditorGUILayout.EnumPopup("Replay Limit Mode", _group.limitMode);
                if (newLimitMode != _group.limitMode) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Replay Limit Mode");
                    _group.limitMode = newLimitMode;
                }

                switch (_group.limitMode) {
                    case MasterAudioGroup.LimitMode.FrameBased:
                        var newFrameLimit = EditorGUILayout.IntSlider("Min Frames Between", _group.limitPerXFrames, 1, 120);
                        if (newFrameLimit != _group.limitPerXFrames) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Min Frames Between");
                            _group.limitPerXFrames = newFrameLimit;
                        }
                        break;
                    case MasterAudioGroup.LimitMode.TimeBased:
                        var newMinTime = EditorGUILayout.Slider("Min Seconds Between", _group.minimumTimeBetween, 0.1f, 10f);
                        if (newMinTime != _group.minimumTimeBetween) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Min Seconds Between");
                            _group.minimumTimeBetween = newMinTime;
                        }
                        break;
                }
                break;
            case MasterAudioGroup.VariationMode.Dialog:
                DTGUIHelper.ShowColorWarning("In this mode, only one Variation can be played at a time.");

                var newUseDialog = EditorGUILayout.Toggle("Dialog Custom Fade?", _group.useDialogFadeOut);
                if (newUseDialog != _group.useDialogFadeOut) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "toggle Dialog Custom Fade?");
                    _group.useDialogFadeOut = newUseDialog;
                }

                if (_group.useDialogFadeOut) {
                    var newFadeTime = EditorGUILayout.Slider("Custom Fade Out Time", _group.dialogFadeOutTime, 0.1f, 20f);
                    if (newFadeTime != _group.dialogFadeOutTime) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Custom Fade Out Time");
                        _group.dialogFadeOutTime = newFadeTime;
                    }
                }
                break;
        }
        EditorGUILayout.EndVertical();

        DTGUIHelper.AddSpaceForNonU5(2);
        DTGUIHelper.StartGroupHeader();
        EditorGUI.indentLevel = 0;

        var canCopy = false;

        var newChildMode = (MasterAudioGroup.ChildGroupMode)EditorGUILayout.EnumPopup(new GUIContent("Linked Group Mode", "Groups you set up in this section will also get played automatically when this Group plays."), _group.childGroupMode);
        if (newChildMode != _group.childGroupMode) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Linked Group Mode");
            _group.childGroupMode = newChildMode;
        }
        EditorGUILayout.EndVertical();

        if (_group.childGroupMode != MasterAudioGroup.ChildGroupMode.None) {
            if (_group.childSoundGroups.Count == 0) {
                DTGUIHelper.ShowLargeBarAlert("You have no other Groups set up to trigger.");
                EditorGUILayout.Separator();
            }

            EditorGUI.indentLevel = 1;
            for (var i = 0; i < _group.childSoundGroups.Count; i++) {
                var aGroup = _group.childSoundGroups[i];
                if (maInScene) {
                    var existingIndex = _groupNames.IndexOf(aGroup);

                    int? groupIndex = null;

                    EditorGUI.indentLevel = 0;

                    var noGroup = false;
                    var noMatch = false;

                    if (existingIndex >= 1) {
                        EditorGUILayout.BeginHorizontal();
                        groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, _groupNames.ToArray());
                        if (existingIndex == 1) {
                            noGroup = true;
                        }

                        if (groupIndex > MasterAudio.HardCodedBusOptions - 1) {
                            var buttonPressed = DTGUIHelper.AddSettingsButton("Linked Sound Group");
                            if (buttonPressed == DTGUIHelper.DTFunctionButtons.Go) {
                                var grp = _groupNames[existingIndex];
                                var trs = MasterAudio.FindGroupTransform(grp);
                                if (trs != null) {
                                    Selection.activeObject = trs;
                                }
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    } else if (existingIndex == -1 && aGroup == MasterAudio.NoGroupName) {
                        groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, _groupNames.ToArray());
                    } else { // non-match
                        noMatch = true;
                        var newSound = EditorGUILayout.TextField("Sound Group", aGroup);
                        if (newSound != aGroup) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Sound Group");
                            _group.childSoundGroups[i] = newSound;
                        }

                        var newIndex = EditorGUILayout.Popup("All Sound Groups", -1, _groupNames.ToArray());
                        if (newIndex >= 0) {
                            groupIndex = newIndex;
                        }
                    }

                    if (noGroup) {
                        DTGUIHelper.ShowRedError("No Sound Group specified. Action will do nothing.");
                    } else if (noMatch) {
                        DTGUIHelper.ShowRedError("Sound Group found no match. Type in or choose one.");
                    }

                    if (!groupIndex.HasValue) {
                        continue;
                    }
                    if (existingIndex != groupIndex.Value) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Sound Group");
                    }
                    if (groupIndex.Value == -1) {
                        _group.childSoundGroups[i] = MasterAudio.NoGroupName;
                    } else {
                        _group.childSoundGroups[i] = _groupNames[groupIndex.Value];
                    }
                } else {
                    var newSType = EditorGUILayout.TextField("Sound Group", aGroup);
                    if (newSType == aGroup) {
                        continue;
                    }
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Sound Group");
                    _group.childSoundGroups[i] = newSType;
                }
            }

            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Add Trigger Group"), EditorStyles.toolbarButton, GUILayout.Width(120))) {
                _group.childSoundGroups.Add(string.Empty);
            }
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Delete Trigger Group", "Delete the last Trigger Group"), EditorStyles.toolbarButton, GUILayout.Width(120))) {
                _group.childSoundGroups.RemoveAt(_group.childSoundGroups.Count - 1);
            }
            EditorGUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }
        EditorGUILayout.EndVertical();

        EditorGUI.indentLevel = 0;

        DTGUIHelper.StartGroupHeader();
        var newUse = EditorGUILayout.BeginToggleGroup(" Group Played Event", _group.soundPlayedEventActive);
        if (newUse != _group.soundPlayedEventActive) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "toggle expand Group Played Event");
            _group.soundPlayedEventActive = newUse;
        }
        DTGUIHelper.EndGroupHeader();

        GUI.color = Color.white;

        if (_group.soundPlayedEventActive) {
            DTGUIHelper.ShowColorWarning("When this Group plays, fire Custom Event below.");

            if (maInScene) {
                var existingIndex = _customEventNames.IndexOf(_group.soundPlayedCustomEvent);

                int? customEventIndex = null;

                EditorGUI.indentLevel = 0;

                var noEvent = false;
                var noMatch = false;

                if (existingIndex >= 1) {
                    customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                    if (existingIndex == 1) {
                        noEvent = true;
                    }
                } else if (existingIndex == -1 && _group.soundPlayedCustomEvent == MasterAudio.NoGroupName) {
                    customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                } else { // non-match
                    noMatch = true;
                    var newEventName = EditorGUILayout.TextField("Custom Event Name", _group.soundPlayedCustomEvent);
                    if (newEventName != _group.soundPlayedCustomEvent) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Custom Event Name");
                        _group.soundPlayedCustomEvent = newEventName;
                    }

                    var newIndex = EditorGUILayout.Popup("All Custom Events", -1, _customEventNames.ToArray());
                    if (newIndex >= 0) {
                        customEventIndex = newIndex;
                    }
                }

                if (noEvent) {
                    DTGUIHelper.ShowRedError("No Custom Event specified. This section will do nothing.");
                } else if (noMatch) {
                    DTGUIHelper.ShowRedError("Custom Event found no match. Type in or choose one.");
                }

                if (customEventIndex.HasValue) {
                    if (existingIndex != customEventIndex.Value) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Custom Event");
                    }
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (customEventIndex.Value == -1) {
                        _group.soundPlayedCustomEvent = MasterAudio.NoGroupName;
                    } else {
                        _group.soundPlayedCustomEvent = _customEventNames[customEventIndex.Value];
                    }
                }
            } else {
                var newCustomEvent = EditorGUILayout.TextField("Custom Event Name", _group.soundPlayedCustomEvent);
                if (newCustomEvent != _group.soundPlayedCustomEvent) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "Custom Event Name");
                    _group.soundPlayedCustomEvent = newCustomEvent;
                }
            }
        }
        EditorGUILayout.EndToggleGroup();

        if (!Application.isPlaying) {
            DTGUIHelper.AddSpaceForNonU5(2);
            DTGUIHelper.StartGroupHeader();
            EditorGUI.indentLevel = 1;
            EditorGUILayout.BeginHorizontal();
            var newBulk = DTGUIHelper.Foldout(_group.copySettingsExpanded, "Copy Settings");
            DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/SoundGroups.htm#CopySettings");
            EditorGUILayout.EndHorizontal();
            if (newBulk != _group.copySettingsExpanded) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "toggle Copy Settings");
                _group.copySettingsExpanded = newBulk;
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel = 0;
            GUI.color = Color.white;

            if (_group.copySettingsExpanded) {
                if (_group.groupVariations.Count == 0) {
                    DTGUIHelper.ShowLargeBarAlert("You currently have no Variations in this Group.");
                } else if (_group.groupVariations.Count == 1) {
                    DTGUIHelper.ShowLargeBarAlert("You only have a single Variation in this Group. Nothing to copy to.");
                } else {
                    canCopy = true;

                    var varNames = new List<string>(_group.groupVariations.Count);
                    foreach (var t in _group.groupVariations) {
                        varNames.Add(t.name);
                    }

                    if (_group.selectedVariationIndex >= varNames.Count) {
                        _group.selectedVariationIndex = 0;
                    }

                    var newVar = EditorGUILayout.Popup("Source Variation", _group.selectedVariationIndex,
                        varNames.ToArray());
                    if (newVar != _group.selectedVariationIndex) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Source Variation");
                        _group.selectedVariationIndex = newVar;
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Target Variations");

                    GUILayout.Space(44);
                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                    if (GUILayout.Button("Check All", EditorStyles.toolbarButton, GUILayout.Width(80))) {
                        CheckAll();
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button("Uncheck All", EditorStyles.toolbarButton, GUILayout.Width(80))) {
                        UncheckAll();
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Separator();

                    DTGUIHelper.ShowColorWarning(
                        "Click buttons below to copy from Source to checked Variations.");

                    var hasSelected = GetNonMatchingVariations().Count > 0;
                    if (!hasSelected) {
                        DTGUIHelper.ShowRedError(
                            "You have no Variations checked. Please use the checkboxes.");
                        EditorGUILayout.Separator();
                    }

                    var sourceVar = _group.groupVariations[_group.selectedVariationIndex];
                    const int btnWidth = 96;

                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    if (GUILayout.Button("Volume", EditorStyles.toolbarButton, GUILayout.Width(btnWidth))) {
                        CopyVolumes(sourceVar);
                        isDirty = true;
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button("Pitch", EditorStyles.toolbarButton, GUILayout.Width(btnWidth))) {
                        CopyPitches(sourceVar);
                        isDirty = true;
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button("Loop", EditorStyles.toolbarButton, GUILayout.Width(btnWidth))) {
                        CopyLoops(sourceVar);
                        isDirty = true;
                    }
                    EditorGUILayout.EndHorizontal();
                    DTGUIHelper.VerticalSpace(2);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    if (GUILayout.Button("FX Tail Time", EditorStyles.toolbarButton, GUILayout.Width(btnWidth))) {
                        CopyFxTail(sourceVar);
                        isDirty = true;
                    }

                    GUILayout.Space(10);
                    if (GUILayout.Button("Rand. Pitch", EditorStyles.toolbarButton, GUILayout.Width(btnWidth))) {
                        CopyRandomPitch(sourceVar);
                        isDirty = true;
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button("Rand. Volume", EditorStyles.toolbarButton, GUILayout.Width(btnWidth))) {
                        CopyRandomVolume(sourceVar);
                        isDirty = true;
                    }
                    EditorGUILayout.EndHorizontal();
                    DTGUIHelper.VerticalSpace(2);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    if (GUILayout.Button("Rand. Delay", EditorStyles.toolbarButton, GUILayout.Width(btnWidth))) {
                        CopyRandomDelay(sourceVar);
                        isDirty = true;
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button("Custom Fade", EditorStyles.toolbarButton, GUILayout.Width(btnWidth))) {
                        CopyCustomFade(sourceVar);
                        isDirty = true;
                    }

                    EditorGUILayout.EndHorizontal();

                    GUI.contentColor = Color.white;
                }
                EditorGUILayout.Separator();
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUI.indentLevel = 0;

        int? deadChildIndex = null;

        if (!Application.isPlaying) {
            DTGUIHelper.AddSpaceForNonU5(2);
            DTGUIHelper.StartGroupHeader();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Actions", EditorStyles.wordWrappedLabel, GUILayout.Width(50f));
            GUILayout.Space(30);
            GUI.contentColor = DTGUIHelper.BrightButtonColor;

            var buttonText = "Collapse All";
            var allCollapsed = true;

            foreach (var t in _group.groupVariations) {
                if (!t.isExpanded) {
                    continue;
                }

                allCollapsed = false;
                break;
            }

            if (allCollapsed) {
                buttonText = "Expand All";
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(buttonText), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                isDirty = true;
                ExpandCollapseAll(allCollapsed);
            }
            GUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Eq. Voices", "Reset Voices to one"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                isDirty = true;
                EqualizeWeights(_group);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Eq. Volumes"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                EqualizeVariationVolumes(_group.groupVariations);
            }

            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();

            DTGUIHelper.VerticalSpace(1);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Localization", EditorStyles.wordWrappedLabel, GUILayout.Width(80f));
            GUILayout.FlexibleSpace();
            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            if (GUILayout.Button(new GUIContent("All Use Loc.", "Check the 'Use Localized Folder' checkbox for all Variations."), EditorStyles.toolbarButton, GUILayout.Width(125))) {
                isDirty = true;
                BulkUseLocalization(_group.groupVariations, true);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("None Use Loc.", "Uncheck the 'Use Localized Folder' checkbox for all Variations."), EditorStyles.toolbarButton, GUILayout.Width(125))) {
                isDirty = true;
                BulkUseLocalization(_group.groupVariations, false);
            }

            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();

            var newBulkMode = DTGUIHelper.GetRestrictedAudioLocation("Variation Create Mode", _group.bulkVariationMode);
            if (newBulkMode != _group.bulkVariationMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _group, "change Bulk Variation Mode");
                _group.bulkVariationMode = newBulkMode;
            }
            if (_group.bulkVariationMode == MasterAudio.AudioLocation.ResourceFile) {
                DTGUIHelper.ShowColorWarning("Resource mode: make sure to drag from Resource folders only.");
            }

            DTGUIHelper.EndGroupHeader();
        }

        DTGUIHelper.VerticalSpace(2);

        if (!Application.isPlaying) {
            // new variation settings
            EditorGUILayout.BeginVertical();
            var anEvent = Event.current;

            if (isInProjectView) {
                DTGUIHelper.ShowLargeBarAlert("You are in Project View and cannot create Variations.");
                DTGUIHelper.ShowLargeBarAlert("Pull this prefab into the Scene to create Variations.");
            } else {
                GUI.color = DTGUIHelper.DragAreaColor;

                var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
                GUI.Box(dragArea, "Drag Audio clips here to create Variations!");

                GUI.color = Color.white;

                switch (anEvent.type) {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        if (!dragArea.Contains(anEvent.mousePosition)) {
                            break;
                        }

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (anEvent.type == EventType.DragPerform) {
                            DragAndDrop.AcceptDrag();

                            foreach (var dragged in DragAndDrop.objectReferences) {
                                var aClip = dragged as AudioClip;
                                if (aClip == null) {
                                    continue;
                                }

                                CreateVariation(_group, aClip);
                            }
                        }
                        Event.current.Use();
                        break;
                }
            }
            EditorGUILayout.EndVertical();
            // end new variation settings
        }

        if (_group.groupVariations.Count == 0) {
            DTGUIHelper.ShowRedError("You currently have no Variations.");
        } else {
            for (var i = 0; i < _group.groupVariations.Count; i++) {
                var variation = _group.groupVariations[i];

                var isNotSource = _group.selectedVariationIndex != i;

                var state = variation.isExpanded;
                var text = variation.name;

                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (!state) {
                    GUI.backgroundColor = DTGUIHelper.InactiveHeaderColor;
                } else {
                    GUI.backgroundColor = DTGUIHelper.ActiveHeaderColor;
                }

                GUILayout.BeginHorizontal();

                text = "<b><size=11>" + text + "</size></b>";

                if (state) {
                    text = "\u25BC " + text;
                } else {
                    text = "\u25BA " + text;
                }
                if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
                    state = !state;
                }

                GUI.backgroundColor = Color.white;
                if (!state) {
                    GUILayout.Space(3f);
                }

                if (state != variation.isExpanded) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, variation, "toggle Expand Variation");
                    variation.isExpanded = state;
                }

                EditorGUI.indentLevel = 0;

                if (canCopy) {
                    if (isNotSource) {
                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                        var newChecked = EditorGUILayout.Toggle(variation.isChecked, GUILayout.Width(16), GUILayout.Height(16));
                        if (newChecked != variation.isChecked) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, variation, "toggle check Variation");
                            variation.isChecked = newChecked;
                        }
                        EditorGUILayout.EndHorizontal();
                    } else {
                        GUI.contentColor = DTGUIHelper.BrightTextColor;
                        GUILayout.Label("SOURCE", GUILayout.Width(54));
                        GUI.contentColor = Color.white;
                    }
                }

                if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.GearTexture, "Click to goto Variation"), EditorStyles.toolbarButton, GUILayout.Height(16), GUILayout.Width(40))) {
                    Selection.activeObject = variation;
                }

                if (!Application.isPlaying && !DTGUIHelper.IsPrefabInProjectView(_group)) {
                    if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.CopyTexture, "Click to clone Variation"), EditorStyles.toolbarButton, GUILayout.Height(16), GUILayout.Width(40))) {
                        CloneVariation(i);
                    }
                }

                var varIsDirty = false;

                var buttonPressed = DTGUIHelper.AddDynamicGroupButtons(_group);

                if (!Application.isPlaying && !DTGUIHelper.IsPrefabInProjectView(_group)) {
                    if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.DeleteTexture, "Click to delete this Variation"), EditorStyles.toolbarButton, GUILayout.Height(16), GUILayout.Width(40))) {
                        deadChildIndex = i;
                        isDirty = true;
                    }
                }

                GUILayout.Space(4);
                DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/SoundGroups.htm#Variations");

                switch (buttonPressed) {
                    case DTGUIHelper.DTFunctionButtons.Play:
                        var calcVolume = variation.VarAudio.volume * _group.groupMasterVolume;

                        switch (variation.audLocation) {
                            case MasterAudio.AudioLocation.ResourceFile:
                                StopPreviewer();
                                var fileName = AudioResourceOptimizer.GetLocalizedDynamicSoundGroupFileName(previewLang, variation.useLocalization, variation.resourceFileName);
                                var clip = Resources.Load(fileName) as AudioClip;
                                if (clip != null) {
                                    GetPreviewer().PlayOneShot(clip, calcVolume);
                                } else {
                                    DTGUIHelper.ShowAlert("Could not find Resource file: " + fileName);
                                }
                                break;
                            case MasterAudio.AudioLocation.Clip:
                                variation.VarAudio.PlayOneShot(variation.VarAudio.clip, calcVolume);
                                break;
                            case MasterAudio.AudioLocation.FileOnInternet:
                                if (!string.IsNullOrEmpty(variation.internetFileUrl)) {
                                    Application.OpenURL(variation.internetFileUrl);
                                }

                                break;
                        }
                        isDirty = true;
                        break;
                    case DTGUIHelper.DTFunctionButtons.Stop:
                        if (variation.audLocation == MasterAudio.AudioLocation.ResourceFile) {
                            StopPreviewer();
                        } else {
                            variation.VarAudio.Stop();
                        }
                        isDirty = true;
                        break;
                }

                EditorGUILayout.EndHorizontal();

                GUI.backgroundColor = Color.white;

                if (!variation.isExpanded) {
                    DTGUIHelper.VerticalSpace(3);
                    continue;
                }

                DTGUIHelper.BeginGroupedControls();

                if (!Application.isPlaying) {
                    DTGUIHelper.ShowColorWarning(MasterAudio.PreviewText);
                }
                if (variation.VarAudio == null) {
                    DTGUIHelper.ShowRedError(string.Format("The Variation: '{0}' has no Audio Source.", variation.name));
                    break;
                }

                var oldLocation = variation.audLocation;
                var newLocation = (MasterAudio.AudioLocation)EditorGUILayout.EnumPopup("Audio Origin", variation.audLocation);
                if (newLocation != variation.audLocation) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Audio Origin");
                    variation.audLocation = newLocation;
                }

                switch (variation.audLocation) {
                    case MasterAudio.AudioLocation.Clip:
                        var newClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", variation.VarAudio.clip, typeof(AudioClip), false);
                        if (newClip != variation.VarAudio.clip) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation.VarAudio, "change Audio Clip");
                            variation.VarAudio.clip = newClip;
                        }
                        break;
                    case MasterAudio.AudioLocation.FileOnInternet:
                        if (oldLocation != variation.audLocation) {
                            if (variation.VarAudio.clip != null) {
                                Debug.Log("Audio clip removed to prevent unnecessary memory usage on File On Internet Variation.");
                            }
                            variation.VarAudio.clip = null;
                        }

                        if (!Application.isPlaying) {
                            var newUrl = EditorGUILayout.TextField("Internet File URL", variation.internetFileUrl);
                            if (newUrl != variation.internetFileUrl) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Internet File URL");
                                variation.internetFileUrl = newUrl;
                            }
                        }

                        if (string.IsNullOrEmpty(variation.internetFileUrl)) {
                            DTGUIHelper.ShowLargeBarAlert("You have not specified a URL for the File On Internet. This Variation will not be available to play without one.");
                        }
                        break;
                    case MasterAudio.AudioLocation.ResourceFile:
                        if (oldLocation != variation.audLocation) {
                            if (variation.VarAudio.clip != null) {
                                Debug.Log("Audio clip removed to prevent unnecessary memory usage on Resource file Variation.");
                            }
                            variation.VarAudio.clip = null;
                            varIsDirty = true;
                        }

                        EditorGUILayout.BeginVertical();
                        var anEvent = Event.current;

                        GUI.color = DTGUIHelper.DragAreaColor;
                        var dragArea = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
                        GUI.Box(dragArea, "Drag Resource Audio clip here to use its name!");
                        GUI.color = Color.white;

                        switch (anEvent.type) {
                            case EventType.DragUpdated:
                            case EventType.DragPerform:
                                if (!dragArea.Contains(anEvent.mousePosition)) {
                                    break;
                                }

                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                if (anEvent.type == EventType.DragPerform) {
                                    DragAndDrop.AcceptDrag();

                                    foreach (var dragged in DragAndDrop.objectReferences) {
                                        var aClip = dragged as AudioClip;
                                        if (aClip == null) {
                                            continue;
                                        }

                                        var useLocalization = false;
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Resource Filename");

                                        var newsFilename = DTGUIHelper.GetResourcePath(aClip, ref useLocalization);

                                        variation.resourceFileName = newsFilename;
                                        variation.useLocalization = useLocalization;
                                    }
                                }
                                Event.current.Use();
                                break;
                        }
                        EditorGUILayout.EndVertical();

                        var newFilename = EditorGUILayout.TextField("Resource Filename", variation.resourceFileName);
                        if (newFilename != variation.resourceFileName) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Resource Filename");
                            variation.resourceFileName = newFilename;
                        }

                        EditorGUI.indentLevel = 1;

                        var newLocal = EditorGUILayout.Toggle("Use Localized Folder", variation.useLocalization);
                        if (newLocal != variation.useLocalization) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, variation, "toggle Use Localized Folder");
                            variation.useLocalization = newLocal;
                        }

                        break;
                }

                EditorGUI.indentLevel = 0;

                var newVolume = DTGUIHelper.DisplayVolumeField(variation.VarAudio.volume, DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true);
                if (newVolume != variation.VarAudio.volume) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation.VarAudio, "change Volume");
                    variation.VarAudio.volume = newVolume;
                }

                var newPitch = DTGUIHelper.DisplayPitchField(variation.VarAudio.pitch);
                if (newPitch != variation.VarAudio.pitch) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation.VarAudio, "change Pitch");
                    variation.VarAudio.pitch = newPitch;
                }

                if (_group.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain) {
                    DTGUIHelper.ShowLargeBarAlert("Loop Clip is always OFF for Looped Chain Groups");
                } else {
                    var newLoop = EditorGUILayout.Toggle("Loop Clip", variation.VarAudio.loop);
                    if (newLoop != variation.VarAudio.loop) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation.VarAudio, "toggle Loop Clip");
                        variation.VarAudio.loop = newLoop;
                    }
                }

                var newWeight = EditorGUILayout.IntSlider("Voices (Weight)", variation.weight, 0, 100);
                if (newWeight != variation.weight) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Voices (Weight)");
                    variation.weight = newWeight;
                }

                var newFxTailTime = EditorGUILayout.Slider("FX Tail Time", variation.fxTailTime, 0f, 10f);
                if (newFxTailTime != variation.fxTailTime) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change FX Tail Time");
                    variation.fxTailTime = newFxTailTime;
                }

                DTGUIHelper.StartGroupHeader();

                var newUseRndPitch = EditorGUILayout.BeginToggleGroup(" Use Random Pitch", variation.useRandomPitch);
                if (newUseRndPitch != variation.useRandomPitch) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "toggle Use Random Pitch");
                    variation.useRandomPitch = newUseRndPitch;
                }
                DTGUIHelper.EndGroupHeader();

                if (variation.useRandomPitch) {
                    var newMode = (SoundGroupVariation.RandomPitchMode)EditorGUILayout.EnumPopup("Pitch Compute Mode", variation.randomPitchMode);
                    if (newMode != variation.randomPitchMode) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Pitch Compute Mode");
                        variation.randomPitchMode = newMode;
                    }

                    var newPitchMin = DTGUIHelper.DisplayPitchField(variation.randomPitchMin, "Random Pitch Min");
                    if (newPitchMin != variation.randomPitchMin) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Random Pitch Min");
                        variation.randomPitchMin = newPitchMin;
                        if (variation.randomPitchMax <= variation.randomPitchMin) {
                            variation.randomPitchMax = variation.randomPitchMin;
                        }
                    }

                    var newPitchMax = DTGUIHelper.DisplayPitchField(variation.randomPitchMax, "Random Pitch Max");
                    if (newPitchMax != variation.randomPitchMax) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Random Pitch Max");
                        variation.randomPitchMax = newPitchMax;
                        if (variation.randomPitchMin > variation.randomPitchMax) {
                            variation.randomPitchMin = variation.randomPitchMax;
                        }
                    }
                }

                EditorGUILayout.EndToggleGroup();
                DTGUIHelper.AddSpaceForNonU5(2);

                DTGUIHelper.StartGroupHeader();

                var newUseRndVol = EditorGUILayout.BeginToggleGroup(" Use Random Volume", variation.useRandomVolume);
                if (newUseRndVol != variation.useRandomVolume) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "toggle Use Random Volume");
                    variation.useRandomVolume = newUseRndVol;
                }
                DTGUIHelper.EndGroupHeader();

                if (variation.useRandomVolume) {
                    var newMode = (SoundGroupVariation.RandomVolumeMode)EditorGUILayout.EnumPopup("Volume Compute Mode", variation.randomVolumeMode);
                    if (newMode != variation.randomVolumeMode) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Volume Compute Mode");
                        variation.randomVolumeMode = newMode;
                    }

                    var volMin = 0f;
                    if (variation.randomVolumeMode == SoundGroupVariation.RandomVolumeMode.AddToClipVolume) {
                        volMin = -1f;
                    }

                    var newVolMin = DTGUIHelper.DisplayVolumeField(variation.randomVolumeMin, DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, volMin, true, "Random Volume Min");
                    if (newVolMin != variation.randomVolumeMin) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Random Volume Min");
                        variation.randomVolumeMin = newVolMin;
                        if (variation.randomVolumeMax <= variation.randomVolumeMin) {
                            variation.randomVolumeMax = variation.randomVolumeMin;
                        }
                    }

                    var newVolMax = DTGUIHelper.DisplayVolumeField(variation.randomVolumeMax, DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, volMin, true, "Random Volume Max");
                    if (newVolMax != variation.randomVolumeMax) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Random Volume Max");
                        variation.randomVolumeMax = newVolMax;
                        if (variation.randomVolumeMin > variation.randomVolumeMax) {
                            variation.randomVolumeMin = variation.randomVolumeMax;
                        }
                    }
                }

                EditorGUILayout.EndToggleGroup();
                DTGUIHelper.AddSpaceForNonU5(2);

                DTGUIHelper.StartGroupHeader();
                var newSilence = EditorGUILayout.BeginToggleGroup(" Use Random Delay", variation.useIntroSilence);
                if (newSilence != variation.useIntroSilence) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "toggle Use Random Delay");
                    variation.useIntroSilence = newSilence;
                }
                DTGUIHelper.EndGroupHeader();

                if (variation.useIntroSilence) {
                    var newSilenceMin = EditorGUILayout.Slider("Delay Min (sec)", variation.introSilenceMin, 0f, 100f);
                    if (newSilenceMin != variation.introSilenceMin) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Delay Min (sec)");
                        variation.introSilenceMin = newSilenceMin;
                        if (variation.introSilenceMin > variation.introSilenceMax) {
                            variation.introSilenceMax = newSilenceMin;
                        }
                    }

                    var newSilenceMax = EditorGUILayout.Slider("Delay Max (sec)", variation.introSilenceMax, 0f, 100f);
                    if (newSilenceMax != variation.introSilenceMax) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Delay Max (sec)");
                        variation.introSilenceMax = newSilenceMax;
                        if (variation.introSilenceMax < variation.introSilenceMin) {
                            variation.introSilenceMin = newSilenceMax;
                        }
                    }
                }

                EditorGUILayout.EndToggleGroup();
                DTGUIHelper.AddSpaceForNonU5(2);

                DTGUIHelper.StartGroupHeader();
                var newStart = EditorGUILayout.BeginToggleGroup(" Use Random Start Position", variation.useRandomStartTime);
                if (newStart != variation.useRandomStartTime) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "toggle Use Random Start Position");
                    variation.useRandomStartTime = newStart;
                }
                DTGUIHelper.EndGroupHeader();

                if (variation.useRandomStartTime) {
                    var newMin = EditorGUILayout.Slider("Start Min (%)", variation.randomStartMinPercent, 0f, 100f);
                    if (newMin != variation.randomStartMinPercent) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "toggle Start Min (%)");
                        variation.randomStartMinPercent = newMin;
                        if (variation.randomStartMaxPercent <= variation.randomStartMinPercent) {
                            variation.randomStartMaxPercent = variation.randomStartMinPercent;
                        }
                    }

                    var newMax = EditorGUILayout.Slider("Start Max (%)", variation.randomStartMaxPercent, 0f, 100f);
                    if (newMax != variation.randomStartMaxPercent) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "toggle Start Max (%)");
                        variation.randomStartMaxPercent = newMax;
                        if (variation.randomStartMinPercent > variation.randomStartMaxPercent) {
                            variation.randomStartMinPercent = variation.randomStartMaxPercent;
                        }
                    }
                }

                EditorGUILayout.EndToggleGroup();
                DTGUIHelper.AddSpaceForNonU5(2);

                DTGUIHelper.StartGroupHeader();
                var newFades = EditorGUILayout.BeginToggleGroup(" Use Custom Fading", variation.useFades);
                if (newFades != variation.useFades) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "toggle Use Custom Fading");
                    variation.useFades = newFades;
                }
                DTGUIHelper.EndGroupHeader();

                if (variation.useFades) {
                    var newFadeIn = EditorGUILayout.Slider("Fade In Time (sec)", variation.fadeInTime, 0f, 10f);
                    if (newFadeIn != variation.fadeInTime) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Fade In Time");
                        variation.fadeInTime = newFadeIn;
                    }

                    var newFadeOut = EditorGUILayout.Slider("Fade Out time (sec)", variation.fadeOutTime, 0f, 10f);
                    if (newFadeOut != variation.fadeOutTime) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref varIsDirty, variation, "change Fade Out Time");
                        variation.fadeOutTime = newFadeOut;
                    }
                }
                EditorGUILayout.EndToggleGroup();
                DTGUIHelper.EndGroupedControls();

                DTGUIHelper.VerticalSpace(3);

                if (!varIsDirty) {
                    continue;
                }
                EditorUtility.SetDirty(variation.VarAudio);
                EditorUtility.SetDirty(variation);
            }
        }

        if (deadChildIndex.HasValue) {
            var deadVar = _group.groupVariations[deadChildIndex.Value];

            if (deadVar != null) {
                // delete variation from Hierarchy
                AudioUndoHelper.DestroyForUndo(deadVar.gameObject);
            }

            // delete group.
            _group.groupVariations.RemoveAt(deadChildIndex.Value);
        }


        if (GUI.changed || isDirty) {
            EditorUtility.SetDirty(target);
        }

        //DrawDefaultInspector();
    }

    private static DynamicSoundGroup RescanChildren(DynamicSoundGroup group) {
        var newChildren = new List<DynamicGroupVariation>();

        var childNames = new List<string>();

        for (var i = 0; i < group.transform.childCount; i++) {
            var child = group.transform.GetChild(i);

            if (!Application.isPlaying) {
                if (childNames.Contains(child.name)) {
                    DTGUIHelper.ShowRedError("You have more than one Variation named: " + child.name + ".");
                    DTGUIHelper.ShowRedError("Please ensure each Variation of this Group has a unique name.");
                }
            }

            childNames.Add(child.name);

            var variation = child.GetComponent<DynamicGroupVariation>();

            newChildren.Add(variation);
        }

        group.groupVariations = newChildren;
        return group;
    }

    public void EqualizeWeights(DynamicSoundGroup grp) {
        var variations = new DynamicGroupVariation[grp.groupVariations.Count];

        for (var i = 0; i < grp.groupVariations.Count; i++) {
            var variation = grp.groupVariations[i];
            variations[i] = variation;
        }

        AudioUndoHelper.RecordObjectsForUndo(variations, "Equalize Voices");

        foreach (var vari in variations) {
            vari.weight = 1;
        }
    }

    private static void EqualizeVariationVolumes(List<DynamicGroupVariation> variations) {
        var clips = new Dictionary<DynamicGroupVariation, float>();

        if (variations.Count < 2) {
            DTGUIHelper.ShowAlert("You must have at least 2 Variations to use this function.");
            return;
        }

        var lowestVolume = 1f;

        foreach (var setting in variations) {
            AudioClip ac = null;

            switch (setting.audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    if (setting.VarAudio.clip == null) {
                        continue;
                    }
                    ac = setting.VarAudio.clip;
                    break;
                case MasterAudio.AudioLocation.FileOnInternet:
                    continue;
                case MasterAudio.AudioLocation.ResourceFile:
                    if (string.IsNullOrEmpty(setting.resourceFileName)) {
                        continue;
                    }

                    ac = Resources.Load(setting.resourceFileName) as AudioClip;

                    if (ac == null) {
                        continue; // bad resource path
                    }
                    break;
            }

            if (!AudioUtil.IsClipReadyToPlay(ac)) {
                Debug.Log("Clip is not ready to play (streaming?). Skipping '" + setting.name + "'.");
                continue;
            }

            var average = 0f;
            // ReSharper disable once PossibleNullReferenceException
            var buffer = new float[ac.samples];

            Debug.Log("Measuring amplitude of '" + ac.name + "'.");

            ac.GetData(buffer, 0);

            for (var c = 0; c < ac.samples; c++) {
                average += Mathf.Pow(buffer[c], 2);
            }

            average = Mathf.Sqrt(1f / ac.samples * average);

            if (average < lowestVolume) {
                lowestVolume = average;
            }

            if (average == 0f) {
                // don't factor in.
                continue;
            }
            clips.Add(setting, average);
        }

        if (clips.Count < 2) {
            DTGUIHelper.ShowAlert("You must have at least 2 Variations with non-compressed, non-streaming clips to use this function.");
            return;
        }

        foreach (var kv in clips) {
            if (kv.Value == 0) {
                // skip
                continue;
            }
            var adjustedVol = lowestVolume / kv.Value;
            //set your volume for each Variation in your Sound Group.
            kv.Key.VarAudio.volume = adjustedVol;
        }
    }

    public void CreateVariation(DynamicSoundGroup group, AudioClip clip) {
        var resourceFileName = string.Empty;
        var useLocalization = false;

        if (group.bulkVariationMode == MasterAudio.AudioLocation.ResourceFile) {
            resourceFileName = DTGUIHelper.GetResourcePath(clip, ref useLocalization);
            if (string.IsNullOrEmpty(resourceFileName)) {
                resourceFileName = clip.name;
            }
        }

        var clipName = clip.name;

        if (group.transform.FindChild(clipName) != null) {
            DTGUIHelper.ShowAlert("You already have a Variation for this Group named '" + clipName + "'. \n\nPlease rename these Variations when finished to be unique, or you may not be able to play them by name if you have a need to.");
        }

        var newVar = (GameObject)Instantiate(_group.variationTemplate, _group.transform.position, Quaternion.identity);
        AudioUndoHelper.CreateObjectForUndo(newVar, "create Variation");

        newVar.transform.name = clipName;
        newVar.transform.parent = group.transform;
        var variation = newVar.GetComponent<DynamicGroupVariation>();

        if (group.bulkVariationMode == MasterAudio.AudioLocation.ResourceFile) {
            variation.audLocation = MasterAudio.AudioLocation.ResourceFile;
            variation.resourceFileName = resourceFileName;
            variation.useLocalization = useLocalization;
        } else {
            variation.VarAudio.clip = clip;
        }

        DynamicSoundGroupCreatorInspector.CopyFromAudioSourceTemplate(_dgsc, variation.VarAudio, false);
    }

    private static void BulkUseLocalization(List<DynamicGroupVariation> variations, bool shouldUse) {
        foreach (var setting in variations) {
            if (setting.audLocation != MasterAudio.AudioLocation.ResourceFile) {
                continue;
            }

            setting.useLocalization = shouldUse;
        }
    }

    private List<DynamicGroupVariation> GetNonMatchingVariations() {
        var changedVars = new List<DynamicGroupVariation>();

        for (var i = 0; i < _group.groupVariations.Count; i++) {
            if (i == _group.selectedVariationIndex) {
                continue;
            }

            var vari = _group.groupVariations[i];
            if (!vari.isChecked) {
                continue;
            }

            changedVars.Add(vari);
        }

        return changedVars;
    }

    private void CopyVolumes(DynamicGroupVariation variation) {
        var changed = 0;

        var changedVars = GetNonMatchingVariations();

        if (changedVars.Count > 0) {
            AudioUndoHelper.RecordObjectsForUndo(changedVars.ToArray(), "change Variation Volumes");
        }

        foreach (var aVar in changedVars) {
            aVar.VarAudio.volume = variation.VarAudio.volume;
            changed++;
        }

        Debug.LogWarning(changed + " Variation Volume(s) changed.");
    }

    private void CopyPitches(DynamicGroupVariation variation) {
        var changed = 0;

        var changedVars = GetNonMatchingVariations();

        if (changedVars.Count > 0) {
            AudioUndoHelper.RecordObjectsForUndo(changedVars.ToArray(), "change Variation Pitches");
        }

        foreach (var aVar in changedVars) {
            aVar.VarAudio.pitch = variation.VarAudio.pitch;
            changed++;
        }

        Debug.LogWarning(changed + " Variation Pitch(es) changed.");
    }

    private void CopyLoops(DynamicGroupVariation variation) {
        var changed = 0;

        var changedVars = GetNonMatchingVariations();

        if (changedVars.Count > 0) {
            AudioUndoHelper.RecordObjectsForUndo(changedVars.ToArray(), "change Variation Loops");
        }

        foreach (var aVar in changedVars) {
            aVar.VarAudio.loop = variation.VarAudio.loop;
            changed++;
        }

        Debug.LogWarning(changed + " Variation Loop(s) changed.");
    }

    private void CopyFxTail(DynamicGroupVariation variation) {
        var changed = 0;

        var changedVars = GetNonMatchingVariations();

        if (changedVars.Count > 0) {
            AudioUndoHelper.RecordObjectsForUndo(changedVars.ToArray(), "change Variation Fx Tail");
        }

        foreach (var aVar in changedVars) {
            aVar.fxTailTime = variation.fxTailTime;
            changed++;
        }

        Debug.LogWarning(changed + " Fx Tail(s) changed.");
    }

    private void CopyRandomPitch(DynamicGroupVariation variation) {
        var changed = 0;

        var changedVars = GetNonMatchingVariations();

        if (changedVars.Count > 0) {
            AudioUndoHelper.RecordObjectsForUndo(changedVars.ToArray(), "change Variation Random Pitch");
        }

        foreach (var aVar in changedVars) {
            aVar.useRandomPitch = variation.useRandomPitch;
            aVar.randomPitchMode = variation.randomPitchMode;
            aVar.randomPitchMin = variation.randomPitchMin;
            aVar.randomPitchMax = variation.randomPitchMax;
            changed++;
        }

        Debug.LogWarning(changed + " Random Pitch(es) changed.");
    }

    private void CopyRandomVolume(DynamicGroupVariation variation) {
        var changed = 0;

        var changedVars = GetNonMatchingVariations();

        if (changedVars.Count > 0) {
            AudioUndoHelper.RecordObjectsForUndo(changedVars.ToArray(), "change Variation Random Volume");
        }

        foreach (var aVar in changedVars) {
            aVar.useRandomVolume = variation.useRandomVolume;
            aVar.randomVolumeMode = variation.randomVolumeMode;
            aVar.randomVolumeMin = variation.randomVolumeMin;
            aVar.randomVolumeMax = variation.randomVolumeMax;
            changed++;
        }

        Debug.LogWarning(changed + " Random Volume(s) changed.");
    }

    private void CopyRandomDelay(DynamicGroupVariation variation) {
        var changed = 0;

        var changedVars = GetNonMatchingVariations();

        if (changedVars.Count > 0) {
            AudioUndoHelper.RecordObjectsForUndo(changedVars.ToArray(), "change Variation Random Delay");
        }

        foreach (var aVar in changedVars) {
            aVar.useIntroSilence = variation.useIntroSilence;
            aVar.introSilenceMin = variation.introSilenceMin;
            aVar.introSilenceMax = variation.introSilenceMax;
            changed++;
        }

        Debug.LogWarning(changed + " Random Delay(s) changed.");
    }

    private void CopyCustomFade(DynamicGroupVariation variation) {
        var changed = 0;

        var changedVars = GetNonMatchingVariations();

        if (changedVars.Count > 0) {
            AudioUndoHelper.RecordObjectsForUndo(changedVars.ToArray(), "change Variation Custom Fade");
        }

        foreach (var aVar in changedVars) {
            aVar.useFades = variation.useFades;
            aVar.fadeInTime = variation.fadeInTime;
            aVar.fadeOutTime = variation.fadeOutTime;
            changed++;
        }

        Debug.LogWarning(changed + " Random Custom Fade(s) changed.");
    }

    private void ExpandCollapseAll(bool expand) {
        var vars = new List<DynamicGroupVariation>();

        foreach (var t in _group.groupVariations) {
            vars.Add(t);
        }

        AudioUndoHelper.RecordObjectsForUndo(vars.ToArray(), "toggle Expand / Collapse Variations");
        foreach (var t in vars) {
            t.isExpanded = expand;
        }
    }

    private void CloneVariation(int index) {
        var gameObj = _group.groupVariations[index].gameObject;

        var dupe = DTGUIHelper.DuplicateGameObject(gameObj, _group.name, _group.groupVariations.Count + 1);

        if (dupe == null) {
            return;
        }

        dupe.transform.parent = _group.transform;
    }

    private void CheckAll() {
        var vars = new List<DynamicGroupVariation>();

        for (var i = 0; i < _group.groupVariations.Count; i++) {
            var vari = _group.groupVariations[i];
            if (i == _group.selectedVariationIndex) {
                continue;
            }
            vars.Add(vari);
        }

        AudioUndoHelper.RecordObjectsForUndo(vars.ToArray(), "check Variations");

        foreach (var t in vars) {
            t.isChecked = true;
        }
    }

    private void UncheckAll() {
        var vars = new List<DynamicGroupVariation>();

        for (var i = 0; i < _group.groupVariations.Count; i++) {
            var vari = _group.groupVariations[i];
            if (i == _group.selectedVariationIndex) {
                continue;
            }
            vars.Add(vari);
        }

        AudioUndoHelper.RecordObjectsForUndo(vars.ToArray(), "check Variations");

        foreach (var t in vars) {
            t.isChecked = false;
        }
    }

    private void StopPreviewer() {
        GetPreviewer().Stop();
    }

    private AudioSource GetPreviewer() {
        var aud = _previewer.GetComponent<AudioSource>();
        if (aud != null) {
            return aud;
        }

        var dsgc = _previewer.GetComponent<DynamicSoundGroupCreator>();
        UnityEditorInternal.ComponentUtility.CopyComponent(dsgc.variationTemplate.GetComponent<AudioSource>());
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(_previewer);

        aud = _previewer.GetComponent<AudioSource>();

        return aud;
    }
}

