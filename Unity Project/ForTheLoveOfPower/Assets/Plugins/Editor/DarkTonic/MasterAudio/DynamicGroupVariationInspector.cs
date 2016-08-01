using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using DarkTonic.MasterAudio;

[CustomEditor(typeof(DynamicGroupVariation))]
// ReSharper disable once CheckNamespace
public class DynamicGroupVariationInspector : Editor {
    private DynamicGroupVariation _variation;
    private GameObject _previewer;

    // ReSharper disable once FunctionComplexityOverflow
    public override void OnInspectorGUI() {
        EditorGUI.indentLevel = 1;
        var isDirty = false;

        _variation = (DynamicGroupVariation)target;

        if (MasterAudioInspectorResources.LogoTexture != null) {
            DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.LogoTexture);
        }

        EditorGUI.indentLevel = 0;  // Space will handle this for the header
        var previewLang = SystemLanguage.English;

        if (_variation.transform.parent != null && _variation.transform.parent.parent != null) {
            var parentParent = _variation.transform.parent.parent;

            _previewer = parentParent.gameObject;

            var dgsc = _previewer.GetComponent<DynamicSoundGroupCreator>();
            if (dgsc != null) {
                previewLang = dgsc.previewLanguage;
            }
        }

        if (_previewer == null) {
            DTGUIHelper.ShowRedError("This prefab must have a GameObject 2 parents up. Prefab broken.");
            return;
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUI.contentColor = DTGUIHelper.BrightButtonColor;
        if (GUILayout.Button(new GUIContent("Back to Group", "Select Group in Hierarchy"), EditorStyles.toolbarButton, GUILayout.Width(120))) {
            // ReSharper disable once PossibleNullReferenceException
            Selection.activeObject = _variation.transform.parent.gameObject;
        }
        GUILayout.FlexibleSpace();
        GUI.contentColor = Color.white;

        if (!DTGUIHelper.IsPrefabInProjectView(_variation)) {
            var buttonPressed = DTGUIHelper.AddDynamicVariationButtons();

            switch (buttonPressed) {
                case DTGUIHelper.DTFunctionButtons.Play:
                    isDirty = true;
                    var calcVolume = _variation.VarAudio.volume * _variation.ParentGroup.groupMasterVolume;

                    switch (_variation.audLocation) {
                        case MasterAudio.AudioLocation.ResourceFile:
                            StopPreviewer();
                            var fileName = AudioResourceOptimizer.GetLocalizedDynamicSoundGroupFileName(previewLang,
                                _variation.useLocalization, _variation.resourceFileName);

                            var clip = Resources.Load(fileName) as AudioClip;
                            if (clip != null) {
                                GetPreviewer().PlayOneShot(clip, calcVolume);
                            } else {
                                DTGUIHelper.ShowAlert("Could not find Resource file: " + fileName);
                            }
                            break;
                        case MasterAudio.AudioLocation.Clip:
                            _variation.VarAudio.PlayOneShot(_variation.VarAudio.clip, calcVolume);
                            break;
                        case MasterAudio.AudioLocation.FileOnInternet:
                            if (!string.IsNullOrEmpty(_variation.internetFileUrl)) {
                                Application.OpenURL(_variation.internetFileUrl);
                            }
                            break;
                    }
                    break;
                case DTGUIHelper.DTFunctionButtons.Stop:
                    if (_variation.audLocation == MasterAudio.AudioLocation.ResourceFile) {
                        StopPreviewer();
                    } else {
                        _variation.VarAudio.Stop();
                    }
                    break;
            }
        }

        EditorGUILayout.EndHorizontal();

        DTGUIHelper.HelpHeader("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/SoundGroupVariations.htm");

        if (!Application.isPlaying) {
            DTGUIHelper.ShowColorWarning(MasterAudio.PreviewText);
        }

        var oldLocation = _variation.audLocation;
        EditorGUILayout.BeginHorizontal();
        var newLocation = (MasterAudio.AudioLocation)EditorGUILayout.EnumPopup("Audio Origin", _variation.audLocation);
        DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/SoundGroupVariations.htm#AudioOrigin");
        EditorGUILayout.EndHorizontal();

        if (newLocation != oldLocation) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Audio Origin");
            _variation.audLocation = newLocation;
        }

        switch (_variation.audLocation) {
            case MasterAudio.AudioLocation.Clip:
                var newClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", _variation.VarAudio.clip, typeof(AudioClip), false);

                if (newClip != _variation.VarAudio.clip) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation.VarAudio, "assign Audio Clip");
                    _variation.VarAudio.clip = newClip;
                }
                break;
            case MasterAudio.AudioLocation.FileOnInternet:
                if (oldLocation != _variation.audLocation) {
                    if (_variation.VarAudio.clip != null) {
                        Debug.Log("Audio clip removed to prevent unnecessary memory usage on File On Internet Variation.");
                    }
                    _variation.VarAudio.clip = null;
                }

                if (!Application.isPlaying) {
                    var newUrl = EditorGUILayout.TextField("Internet File URL", _variation.internetFileUrl);
                    if (newUrl != _variation.internetFileUrl) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Internet File URL");
                        _variation.internetFileUrl = newUrl;
                    }
                }

                if (string.IsNullOrEmpty(_variation.internetFileUrl)) {
                    DTGUIHelper.ShowLargeBarAlert("You have not specified a URL for the File On Internet. This Variation will not be available to play without one.");
                }
                break;
            case MasterAudio.AudioLocation.ResourceFile:
                if (oldLocation != _variation.audLocation) {
                    if (_variation.VarAudio.clip != null) {
                        Debug.Log("Audio clip removed to prevent unnecessary memory usage on Resource file Variation.");
                    }
                    _variation.VarAudio.clip = null;
                }

                EditorGUILayout.BeginVertical();
                var anEvent = Event.current;

                GUI.color = DTGUIHelper.DragAreaColor;
                var dragArea = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
                GUI.Box(dragArea, "Drag Resource Audio clip here to use its name!");
                GUI.color = Color.white;

                string newFilename;

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

                                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Resource filename");

                                var useLocalization = false;
                                newFilename = DTGUIHelper.GetResourcePath(aClip, ref useLocalization);
                                if (string.IsNullOrEmpty(newFilename)) {
                                    newFilename = aClip.name;
                                }

                                _variation.resourceFileName = newFilename;
                                _variation.useLocalization = useLocalization;
                                break;
                            }
                        }
                        Event.current.Use();
                        break;
                }
                EditorGUILayout.EndVertical();

                newFilename = EditorGUILayout.TextField("Resource Filename", _variation.resourceFileName);
                if (newFilename != _variation.resourceFileName) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Resource filename");
                    _variation.resourceFileName = newFilename;
                }

                EditorGUI.indentLevel = 1;

                var newLocal = EditorGUILayout.Toggle("Use Localized Folder", _variation.useLocalization);
                if (newLocal != _variation.useLocalization) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "toggle Use Localized Folder");
                    _variation.useLocalization = newLocal;
                }

                break;
        }

        EditorGUI.indentLevel = 0;

        var newVolume = EditorGUILayout.Slider("Volume", _variation.VarAudio.volume, 0f, 1f);
        if (newVolume != _variation.VarAudio.volume) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation.VarAudio, "change Volume");
            _variation.VarAudio.volume = newVolume;
        }

        var newPitch = DTGUIHelper.DisplayPitchField(_variation.VarAudio.pitch);
        if (newPitch != _variation.VarAudio.pitch) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation.VarAudio, "change Pitch");
            _variation.VarAudio.pitch = newPitch;
        }

        if (_variation.ParentGroup.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain) {
            DTGUIHelper.ShowLargeBarAlert("Loop Clip is always OFF for Looped Chain Groups");
        } else {
            var newLoop = EditorGUILayout.Toggle("Loop Clip", _variation.VarAudio.loop);
            if (newLoop != _variation.VarAudio.loop) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation.VarAudio, "toggle Loop");
                _variation.VarAudio.loop = newLoop;
            }
        }

        EditorGUILayout.BeginHorizontal();
        var newWeight = EditorGUILayout.IntSlider("Voices (Weight)", _variation.weight, 0, 100);
        DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/SoundGroupVariations.htm#Voices");
        EditorGUILayout.EndHorizontal();
        if (newWeight != _variation.weight) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Voices (Weight)");
            _variation.weight = newWeight;
        }

        var newFxTailTime = EditorGUILayout.Slider("FX Tail Time", _variation.fxTailTime, 0f, 10f);
        if (newFxTailTime != _variation.fxTailTime) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change FX Tail Time");
            _variation.fxTailTime = newFxTailTime;
        }

        var filterList = new List<string>() {
			MasterAudio.NoGroupName,
			"Low Pass",
			"High Pass",
			"Distortion",
			"Chorus",
			"Echo",
			"Reverb"
		};

        EditorGUILayout.BeginHorizontal();
        var newFilterIndex = EditorGUILayout.Popup("Add Filter Effect", 0, filterList.ToArray());
        DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/FilterFX.htm");
        EditorGUILayout.EndHorizontal();
        switch (newFilterIndex) {
            case 1:
                AddFilterComponent(typeof(AudioLowPassFilter));
                break;
            case 2:
                AddFilterComponent(typeof(AudioHighPassFilter));
                break;
            case 3:
                AddFilterComponent(typeof(AudioDistortionFilter));
                break;
            case 4:
                AddFilterComponent(typeof(AudioChorusFilter));
                break;
            case 5:
                AddFilterComponent(typeof(AudioEchoFilter));
                break;
            case 6:
                AddFilterComponent(typeof(AudioReverbFilter));
                break;
        }

        DTGUIHelper.StartGroupHeader();

        var newUseRndPitch = EditorGUILayout.BeginToggleGroup(" Use Random Pitch", _variation.useRandomPitch);
        if (newUseRndPitch != _variation.useRandomPitch) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "toggle Use Random Pitch");
            _variation.useRandomPitch = newUseRndPitch;
        }
        DTGUIHelper.EndGroupHeader();

        if (_variation.useRandomPitch) {
            var newMode = (SoundGroupVariation.RandomPitchMode)EditorGUILayout.EnumPopup("Pitch Compute Mode", _variation.randomPitchMode);
            if (newMode != _variation.randomPitchMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Pitch Compute Mode");
                _variation.randomPitchMode = newMode;
            }

            var newPitchMin = DTGUIHelper.DisplayPitchField(_variation.randomPitchMin, "Random Pitch Min");
            if (newPitchMin != _variation.randomPitchMin) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Random Pitch Min");
                _variation.randomPitchMin = newPitchMin;
                if (_variation.randomPitchMax <= _variation.randomPitchMin) {
                    _variation.randomPitchMax = _variation.randomPitchMin;
                }
            }

            var newPitchMax = DTGUIHelper.DisplayPitchField(_variation.randomPitchMax, "Random Pitch Max");
            if (newPitchMax != _variation.randomPitchMax) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Random Pitch Max");
                _variation.randomPitchMax = newPitchMax;
                if (_variation.randomPitchMin > _variation.randomPitchMax) {
                    _variation.randomPitchMin = _variation.randomPitchMax;
                }
            }
        }

        EditorGUILayout.EndToggleGroup();
        DTGUIHelper.AddSpaceForNonU5(2);

        DTGUIHelper.StartGroupHeader();

        var newUseRndVol = EditorGUILayout.BeginToggleGroup(" Use Random Volume", _variation.useRandomVolume);
        if (newUseRndVol != _variation.useRandomVolume) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "toggle Use Random Volume");
            _variation.useRandomVolume = newUseRndVol;
        }
        DTGUIHelper.EndGroupHeader();

        if (_variation.useRandomVolume) {
            var newMode = (SoundGroupVariation.RandomVolumeMode)EditorGUILayout.EnumPopup("Volume Compute Mode", _variation.randomVolumeMode);
            if (newMode != _variation.randomVolumeMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Volume Compute Mode");
                _variation.randomVolumeMode = newMode;
            }

            var volMin = 0f;
            if (_variation.randomVolumeMode == SoundGroupVariation.RandomVolumeMode.AddToClipVolume) {
                volMin = -1f;
            }

            var newVolMin = DTGUIHelper.DisplayVolumeField(_variation.randomVolumeMin, DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, volMin, true, "Random Volume Min");
            if (newVolMin != _variation.randomVolumeMin) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Random Volume Min");
                _variation.randomVolumeMin = newVolMin;
                if (_variation.randomVolumeMax <= _variation.randomVolumeMin) {
                    _variation.randomVolumeMax = _variation.randomVolumeMin;
                }
            }

            var newVolMax = DTGUIHelper.DisplayVolumeField(_variation.randomVolumeMax, DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, volMin, true, "Random Volume Max");
            if (newVolMax != _variation.randomVolumeMax) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Random Volume Max");
                _variation.randomVolumeMax = newVolMax;
                if (_variation.randomVolumeMin > _variation.randomVolumeMax) {
                    _variation.randomVolumeMin = _variation.randomVolumeMax;
                }
            }
        }

        EditorGUILayout.EndToggleGroup();
        DTGUIHelper.AddSpaceForNonU5(2);

        DTGUIHelper.StartGroupHeader();

        var newSilence = EditorGUILayout.BeginToggleGroup(" Use Random Delay", _variation.useIntroSilence);
        if (newSilence != _variation.useIntroSilence) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "toggle Use Random Delay");
            _variation.useIntroSilence = newSilence;
        }
        DTGUIHelper.EndGroupHeader();

        if (_variation.useIntroSilence) {
            var newSilenceMin = EditorGUILayout.Slider("Delay Min (sec)", _variation.introSilenceMin, 0f, 100f);
            if (newSilenceMin != _variation.introSilenceMin) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Delay Min (sec)");
                _variation.introSilenceMin = newSilenceMin;
                if (_variation.introSilenceMin > _variation.introSilenceMax) {
                    _variation.introSilenceMax = newSilenceMin;
                }
            }

            var newSilenceMax = EditorGUILayout.Slider("Delay Max (sec)", _variation.introSilenceMax, 0f, 100f);
            if (newSilenceMax != _variation.introSilenceMax) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Delay Max (sec)");
                _variation.introSilenceMax = newSilenceMax;
                if (_variation.introSilenceMax < _variation.introSilenceMin) {
                    _variation.introSilenceMin = newSilenceMax;
                }
            }
        }
        EditorGUILayout.EndToggleGroup();
        DTGUIHelper.AddSpaceForNonU5(2);

        DTGUIHelper.StartGroupHeader();

        var newStart = EditorGUILayout.BeginToggleGroup(" Use Random Start Position", _variation.useRandomStartTime);
        if (newStart != _variation.useRandomStartTime) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "toggle Use Random Start Position");
            _variation.useRandomStartTime = newStart;
        }
        DTGUIHelper.EndGroupHeader();

        if (_variation.useRandomStartTime) {
            var newMin = EditorGUILayout.Slider("Start Min (%)", _variation.randomStartMinPercent, 0f, 100f);
            if (newMin != _variation.randomStartMinPercent) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "toggle Start Min (%)");
                _variation.randomStartMinPercent = newMin;
                if (_variation.randomStartMaxPercent <= _variation.randomStartMinPercent) {
                    _variation.randomStartMaxPercent = _variation.randomStartMinPercent;
                }
            }

            var newMax = EditorGUILayout.Slider("Start Max (%)", _variation.randomStartMaxPercent, 0f, 100f);
            if (newMax != _variation.randomStartMaxPercent) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "toggle Start Max (%)");
                _variation.randomStartMaxPercent = newMax;
                if (_variation.randomStartMinPercent > _variation.randomStartMaxPercent) {
                    _variation.randomStartMinPercent = _variation.randomStartMaxPercent;
                }
            }
        }

        EditorGUILayout.EndToggleGroup();
        DTGUIHelper.AddSpaceForNonU5(2);

        DTGUIHelper.StartGroupHeader();

        var newUseFades = EditorGUILayout.BeginToggleGroup(" Use Custom Fading", _variation.useFades);
        if (newUseFades != _variation.useFades) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "toggle Use Custom Fading");
            _variation.useFades = newUseFades;
        }
        DTGUIHelper.EndGroupHeader();

        if (_variation.useFades) {
            var newFadeIn = EditorGUILayout.Slider("Fade In Time (sec)", _variation.fadeInTime, 0f, 10f);
            if (newFadeIn != _variation.fadeInTime) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Fade In Time");
                _variation.fadeInTime = newFadeIn;
            }

            if (_variation.VarAudio.loop) {
                DTGUIHelper.ShowColorWarning("Looped clips cannot have a custom fade out.");
            } else {
                var newFadeOut = EditorGUILayout.Slider("Fade Out time (sec)", _variation.fadeOutTime, 0f, 10f);
                if (newFadeOut != _variation.fadeOutTime) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref isDirty, _variation, "change Fade Out Time");
                    _variation.fadeOutTime = newFadeOut;
                }
            }
        }

        EditorGUILayout.EndToggleGroup();

        if (GUI.changed || isDirty) {
            EditorUtility.SetDirty(target);
        }

        //DrawDefaultInspector();
    }

    private void StopPreviewer() {
        GetPreviewer().Stop();
    }

    private AudioSource GetPreviewer() {
        var aud = _previewer.GetComponent<AudioSource>();
        if (aud != null) {
            return aud;
        }

        UnityEditorInternal.ComponentUtility.CopyComponent(_variation.GetComponent<AudioSource>());
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(_previewer);

        aud = _previewer.GetComponent<AudioSource>();

        return aud;
    }

    private void AddFilterComponent(Type filterType) {
        _variation.gameObject.AddComponent(filterType);
    }
}
