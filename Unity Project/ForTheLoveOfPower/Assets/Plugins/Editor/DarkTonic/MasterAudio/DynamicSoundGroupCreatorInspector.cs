using System.Collections.Generic;
using System.IO;
using DarkTonic.MasterAudio;
using UnityEditor;
using UnityEngine;

#if UNITY_5
using UnityEngine.Audio;
#endif

[CustomEditor(typeof(DynamicSoundGroupCreator))]
// ReSharper disable once CheckNamespace
public class DynamicSoundGroupCreatorInspector : Editor {
    private const string ExistingBus = "[EXISTING BUS]";
    private const string ExistingNameName = "[EXISTING BUS NAME]";

    private DynamicSoundGroupCreator _creator;
    private List<DynamicSoundGroup> _groups;
    private bool _isDirty;
    private List<string> _customEventNames = new List<string>();
    private List<string> _audioSourceTemplateNames = new List<string>();
    private GameObject _previewer;

    private List<DynamicSoundGroup> ScanForGroups() {
        var groups = new List<DynamicSoundGroup>();

        for (var i = 0; i < _creator.transform.childCount; i++) {
            var aChild = _creator.transform.GetChild(i);

            var grp = aChild.GetComponent<DynamicSoundGroup>();
            if (grp == null) {
                continue;
            }

            grp.groupVariations = VariationsForGroup(aChild.transform);

            groups.Add(grp);
        }

        if (_creator.groupByBus) {
            groups.Sort(delegate(DynamicSoundGroup g1, DynamicSoundGroup g2) {
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (g1.busIndex == g2.busIndex) {
                    return g1.name.CompareTo(g2.name);
                }

                return g1.busIndex.CompareTo(g2.busIndex);
            });
        } else {
            groups.Sort(delegate(DynamicSoundGroup g1, DynamicSoundGroup g2) {
                return g1.name.CompareTo(g2.name);
            });
        }

        return groups;
    }

    private static List<DynamicGroupVariation> VariationsForGroup(Transform groupTrans) {
        var variations = new List<DynamicGroupVariation>();

        for (var i = 0; i < groupTrans.childCount; i++) {
            var aVar = groupTrans.GetChild(i);

            var variation = aVar.GetComponent<DynamicGroupVariation>();
            variations.Add(variation);
        }

        return variations;
    }

    public override void OnInspectorGUI() {
        EditorGUI.indentLevel = 1;
        _isDirty = false;

        _creator = (DynamicSoundGroupCreator)target;

        var isInProjectView = DTGUIHelper.IsPrefabInProjectView(_creator);

        if (MasterAudioInspectorResources.LogoTexture != null) {
            DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.LogoTexture);
        }

		DTGUIHelper.HelpHeader("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/DynamicSoundGroupCreators.htm", "https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MasterAudio_API/class_dark_tonic_1_1_master_audio_1_1_dynamic_sound_group_creator.html");

        MasterAudio.Instance = null;
        var ma = MasterAudio.SafeInstance;
        var maInScene = ma != null;

        if (maInScene) {
            _customEventNames = ma.CustomEventNames;
        }

        _previewer = _creator.gameObject;
        var allowPreview = !DTGUIHelper.IsPrefabInProjectView(_creator);

        var sliderIndicatorChars = 6;
        var sliderWidth = 40;

        if (MasterAudio.UseDbScaleForVolume) {
            sliderIndicatorChars = 9;
            sliderWidth = 56;
        }

        var busVoiceLimitList = new List<string> { MasterAudio.NoVoiceLimitName };

        for (var i = 1; i <= 32; i++) {
            busVoiceLimitList.Add(i.ToString());
        }

        var busList = new List<string> { MasterAudioGroup.NoBus, MasterAudioInspector.NewBusName, ExistingBus };

        var maxChars = 12;

        foreach (var t in _creator.groupBuses) {
            var bus = t;
            busList.Add(bus.busName);

            if (bus.busName.Length > maxChars) {
                maxChars = bus.busName.Length;
            }
        }
        var busListWidth = 9 * maxChars;

        EditorGUI.indentLevel = 0;  // Space will handle this for the header

        if (MasterAudio.SafeInstance == null) {
            var newLang = (SystemLanguage)EditorGUILayout.EnumPopup(new GUIContent("Preview Language", "This setting is only used (and visible) to choose the previewing language when there's no Master Audio prefab in the Scene (language settings are grabbed from there normally). This should only happen when you're using a Master Audio prefab from a previous Scene in persistent mode."), _creator.previewLanguage);
            if (newLang != _creator.previewLanguage) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Preview Language");
                _creator.previewLanguage = newLang;
            }
        }

        EditorGUILayout.Separator();

        var newAllow = (DynamicSoundGroupCreator.CreateItemsWhen)EditorGUILayout.EnumPopup("Items Created When?", _creator.reUseMode);
        if (newAllow != _creator.reUseMode) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Items Created When?");
            _creator.reUseMode = newAllow;
        }

        var newIgnore = EditorGUILayout.Toggle("Error On Duplicate Items", _creator.errorOnDuplicates);
        if (newIgnore != _creator.errorOnDuplicates) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Error On Duplicate Items");
            _creator.errorOnDuplicates = newIgnore;
        }
        if (_creator.errorOnDuplicates) {
            DTGUIHelper.ShowColorWarning("An error will be logged if your Dynamic items already exist in the MA prefab.");
        } else {
            DTGUIHelper.ShowLargeBarAlert("Dynamic items that already exist in the MA prefab will be ignored and not created.");
        }


        var newAwake = EditorGUILayout.Toggle("Auto-create Items", _creator.createOnAwake);
        if (newAwake != _creator.createOnAwake) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Auto-create Items");
            _creator.createOnAwake = newAwake;
        }
        if (_creator.createOnAwake) {
            DTGUIHelper.ShowColorWarning("Items will be created as soon as this object is in the Scene.");
        } else {
            DTGUIHelper.ShowLargeBarAlert("You will need to call this object's CreateItems method manually to create the items.");
        }

        var newRemove = EditorGUILayout.Toggle("Auto-remove Items", _creator.removeGroupsOnSceneChange);
        if (newRemove != _creator.removeGroupsOnSceneChange) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Auto-remove Items");
            _creator.removeGroupsOnSceneChange = newRemove;
        }

        if (_creator.removeGroupsOnSceneChange) {
            DTGUIHelper.ShowColorWarning("Items will be deleted when the Scene changes.");
        } else {
            DTGUIHelper.ShowLargeBarAlert("Items created by this will persist across Scenes if MasterAudio does.");
        }

        // custom event
        DTGUIHelper.StartGroupHeader();
        GUI.color = Color.white;
        var exp = EditorGUILayout.BeginToggleGroup("Fire 'Items Created' Event", _creator.itemsCreatedEventExpanded);
        if (exp != _creator.itemsCreatedEventExpanded) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle expand Fire 'Items Created' Event");
            _creator.itemsCreatedEventExpanded = exp;
        }
        GUI.color = Color.white;
        DTGUIHelper.EndGroupHeader();

        if (_creator.itemsCreatedEventExpanded) {
            EditorGUI.indentLevel = 0;
            DTGUIHelper.ShowColorWarning("When items are created, fire Custom Event below.");

            var existingIndex = _customEventNames.IndexOf(_creator.itemsCreatedCustomEvent);

            int? customEventIndex = null;

            var noEvent = false;
            var noMatch = false;

            if (existingIndex >= 1) {
                customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                if (existingIndex == 1) {
                    noEvent = true;
                }
            } else if (existingIndex == -1 && _creator.itemsCreatedCustomEvent == MasterAudio.NoGroupName) {
                customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
            } else { // non-match
                noMatch = true;
                var newEventName = EditorGUILayout.TextField("Custom Event Name", _creator.itemsCreatedCustomEvent);
                if (newEventName != _creator.itemsCreatedCustomEvent) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Custom Event Name");
                    _creator.itemsCreatedCustomEvent = newEventName;
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
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Custom Event");
                }
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (customEventIndex.Value == -1) {
                    _creator.itemsCreatedCustomEvent = MasterAudio.NoGroupName;
                } else {
                    _creator.itemsCreatedCustomEvent = _customEventNames[customEventIndex.Value];
                }
            }
        }
        EditorGUILayout.EndToggleGroup();

        _groups = ScanForGroups();
        var groupNameList = GroupNameList;

        EditorGUI.indentLevel = 0;

        var state = _creator.showMusicDucking;
        var text = "Dynamic Music Ducking";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTGUIHelper.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTGUIHelper.ActiveHeaderColor;
        }

        DTGUIHelper.AddSpaceForNonU5(2);

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

        GUILayout.Space(2f);



        if (state != _creator.showMusicDucking) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Dynamic Music Ducking");
            _creator.showMusicDucking = state;
        }

        DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/DynamicSoundGroupCreators.htm#Ducking");

        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        if (_creator.showMusicDucking) {
            DTGUIHelper.BeginGroupedControls();
            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Add Duck Group"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "Add Duck Group");

                var defaultBeginUnduck = 0.5f;
                var defaultDuckedVolumeCut = MasterAudio.DefaultDuckVolCut;
                if (maInScene) {
                    defaultBeginUnduck = ma.defaultRiseVolStart;
                    defaultDuckedVolumeCut = ma.defaultDuckedVolumeCut;
                }

                _creator.musicDuckingSounds.Add(new DuckGroupInfo {
                    soundType = MasterAudio.NoGroupName,
                    riseVolStart = defaultBeginUnduck,
                    duckedVolumeCut = defaultDuckedVolumeCut
                });
            }

            EditorGUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
            EditorGUILayout.Separator();

            if (_creator.musicDuckingSounds.Count == 0) {
                DTGUIHelper.ShowLargeBarAlert("You currently have no ducking sounds set up.");
            } else {
                int? duckSoundToRemove = null;

                if (_creator.musicDuckingSounds.Count > 0) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Sound Group", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent("Vol. Cut (dB)", "Amount to duck the music volume."), EditorStyles.boldLabel);
                    GUILayout.Space(9);
                    GUILayout.Label(new GUIContent("Beg. Unduck", "Begin Unducking after this amount of the sound has been played."), EditorStyles.boldLabel);
                    GUILayout.Space(11);
                    GUILayout.Label(new GUIContent("Unduck Time", "Unducking will take X seconds."), EditorStyles.boldLabel);
                    GUILayout.Space(54);
                    EditorGUILayout.EndHorizontal();
                }

                for (var i = 0; i < _creator.musicDuckingSounds.Count; i++) {
                    var duckSound = _creator.musicDuckingSounds[i];
                    var index = groupNameList.IndexOf(duckSound.soundType);
                    if (index == -1) {
                        index = 0;
                    }

                    DTGUIHelper.StartGroupHeader();

                    EditorGUILayout.BeginHorizontal();
                    var newIndex = EditorGUILayout.Popup(index, groupNameList.ToArray(), GUILayout.MaxWidth(200));
                    if (newIndex >= 0) {
                        if (index != newIndex) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Duck Group");
                        }
                        duckSound.soundType = groupNameList[newIndex];
                    }

                    GUILayout.FlexibleSpace();

                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                    GUILayout.TextField(duckSound.duckedVolumeCut.ToString("N1"), 20, EditorStyles.miniLabel);

                    var newDuckMult = GUILayout.HorizontalSlider(duckSound.duckedVolumeCut, DTGUIHelper.MinDb, DTGUIHelper.MaxDb, GUILayout.Width(60));
                    if (newDuckMult != duckSound.duckedVolumeCut) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Ducked Vol Cut");
                        duckSound.duckedVolumeCut = newDuckMult;
                    }
                    GUI.contentColor = Color.white;

                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                    GUILayout.TextField(duckSound.riseVolStart.ToString("N2"), 20, EditorStyles.miniLabel);

                    var newUnduck = GUILayout.HorizontalSlider(duckSound.riseVolStart, 0f, 1f, GUILayout.Width(60));
                    if (newUnduck != duckSound.riseVolStart) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Begin Unduck");
                        duckSound.riseVolStart = newUnduck;
                    }
                    GUI.contentColor = Color.white;

                    GUILayout.Space(4);
                    GUILayout.TextField(duckSound.unduckTime.ToString("N2"), 20, EditorStyles.miniLabel);
                    var newTime = GUILayout.HorizontalSlider(duckSound.unduckTime, 0f, 5f, GUILayout.Width(60));
                    if (newTime != duckSound.unduckTime) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Unduck Time");
                        duckSound.unduckTime = newTime;
                    }

                    GUILayout.Space(10);
                    if (DTGUIHelper.AddDeleteIcon("Duck Sound")) {
                        duckSoundToRemove = i;
                    }

                    EditorGUILayout.EndHorizontal();
                    DTGUIHelper.EndGroupHeader();
                    DTGUIHelper.AddSpaceForNonU5(2);
                }

                if (duckSoundToRemove.HasValue) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "delete Duck Group");
                    _creator.musicDuckingSounds.RemoveAt(duckSoundToRemove.Value);
                }
            }

            DTGUIHelper.EndGroupedControls();
        }

        DTGUIHelper.ResetColors();


        DTGUIHelper.VerticalSpace(3);

        state = _creator.soundGroupsAreExpanded;
        text = "Dynamic Group Mixer";

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

        GUILayout.Space(2f);


        if (state != _creator.soundGroupsAreExpanded) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Dynamic Group Mixer");
            _creator.soundGroupsAreExpanded = state;
        }

        var applyTemplateToAll = false;
        DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/DynamicSoundGroupCreators.htm#Mixer");

        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        if (_creator.soundGroupsAreExpanded) {
            DTGUIHelper.BeginGroupedControls();

            DTGUIHelper.StartGroupHeader();
            EditorGUILayout.LabelField("Group Control");

            EditorGUILayout.EndVertical();

            _audioSourceTemplateNames = new List<string>();

            foreach (var temp in _creator.audioSourceTemplates) {
                if (temp == null) {
                    continue;
                }
                _audioSourceTemplateNames.Add(temp.name);
            }

            var audTemplatesMissing = false;

            if (Directory.Exists(MasterAudio.AudioSourceTemplateFolder)) {
                var audioSrcTemplates = Directory.GetFiles(MasterAudio.AudioSourceTemplateFolder, "*.prefab").Length;
                if (audioSrcTemplates > _creator.audioSourceTemplates.Count) {
                    audTemplatesMissing = true;
                    DTGUIHelper.ShowLargeBarAlert("There's " + (audioSrcTemplates - _creator.audioSourceTemplates.Count) + " Audio Source Template(s) that aren't set up in this MA prefab. Locate them");
                    DTGUIHelper.ShowLargeBarAlert("in Plugins/DarkTonic/MasterAudio/Sources/Prefabs/AudioSourceTemplates and drag them in below.");
                }
            }

            Event aEvent;
            if (audTemplatesMissing) {
                // create groups start
                EditorGUILayout.BeginVertical();
                aEvent = Event.current;

                GUI.color = DTGUIHelper.DragAreaColor;

                var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
                GUI.Box(dragArea, "Drag prefabs here from Project View to create Group Templates!");

                GUI.color = Color.white;

                switch (aEvent.type) {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        if (!dragArea.Contains(aEvent.mousePosition)) {
                            break;
                        }

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (aEvent.type == EventType.DragPerform) {
                            DragAndDrop.AcceptDrag();

                            foreach (var dragged in DragAndDrop.objectReferences) {
                                var temp = dragged as GameObject;
                                if (temp == null) {
                                    continue;
                                }

                                AddAudioSourceTemplate(temp);
                            }
                        }
                        Event.current.Use();
                        break;
                }
                EditorGUILayout.EndVertical();
                // create groups end
            }

            if (_audioSourceTemplateNames.Count == 0) {
                DTGUIHelper.ShowRedError("You have no Audio Source Templates. Drag them in to create them.");
            } else {
                var audSrcTemplateIndex = _audioSourceTemplateNames.IndexOf(_creator.audioSourceTemplateName);
                if (audSrcTemplateIndex < 0) {
                    audSrcTemplateIndex = 0;
                    _creator.audioSourceTemplateName = _audioSourceTemplateNames[0];
                }

                var newIndex = EditorGUILayout.Popup("Audio Source Template", audSrcTemplateIndex, _audioSourceTemplateNames.ToArray());
                if (newIndex != audSrcTemplateIndex) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Audio Source Template");
                    _creator.audioSourceTemplateName = _audioSourceTemplateNames[newIndex];
                }
            }

            var newDragMode = (MasterAudio.DragGroupMode)EditorGUILayout.EnumPopup("Bulk Creation Mode", _creator.curDragGroupMode);
            if (newDragMode != _creator.curDragGroupMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Bulk Creation Mode");
                _creator.curDragGroupMode = newDragMode;
            }

            var bulkMode = DTGUIHelper.GetRestrictedAudioLocation("Variation Create Mode", _creator.bulkVariationMode);
            if (bulkMode != _creator.bulkVariationMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Variation Mode");
                _creator.bulkVariationMode = bulkMode;
            }

            if (_creator.groupBuses.Count > 0) {
                var newGroupByBus = EditorGUILayout.Toggle("Group by Bus", _creator.groupByBus);
                if (newGroupByBus != _creator.groupByBus) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Group by Bus");
                    _creator.groupByBus = newGroupByBus;
                }
            }

            // create groups start
            EditorGUILayout.BeginVertical();
            aEvent = Event.current;

            if (isInProjectView) {
                DTGUIHelper.ShowLargeBarAlert("You are in Project View and cannot create Groups.");
                DTGUIHelper.ShowLargeBarAlert("Pull this prefab into the Scene to create Groups.");
            } else {
                GUI.color = DTGUIHelper.DragAreaColor;

                var dragAreaGroup = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
                GUI.Box(dragAreaGroup, "Drag Audio clips here to create groups!");

                GUI.color = Color.white;

                switch (aEvent.type) {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        if (!dragAreaGroup.Contains(aEvent.mousePosition)) {
                            break;
                        }

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (aEvent.type == EventType.DragPerform) {
                            DragAndDrop.AcceptDrag();

                            Transform groupInfo = null;

                            var clips = new List<AudioClip>();

                            foreach (var dragged in DragAndDrop.objectReferences) {
                                var aClip = dragged as AudioClip;
                                if (aClip == null) {
                                    continue;
                                }

                                clips.Add(aClip);
                            }

                            clips.Sort(delegate(AudioClip x, AudioClip y) {
                                return x.name.CompareTo(y.name);
                            });

                            foreach (var aClip in clips) {
                                if (_creator.curDragGroupMode == MasterAudio.DragGroupMode.OneGroupPerClip) {
                                    CreateGroup(aClip);
                                } else {
                                    if (groupInfo == null) { // one group with variations
                                        groupInfo = CreateGroup(aClip);
                                    } else {
                                        CreateVariation(groupInfo, aClip);
                                    }
                                }

                                _isDirty = true;
                            }
                        }
                        Event.current.Use();
                        break;
                }
            }
            EditorGUILayout.EndVertical();
            // create groups end

            if (_groups.Count == 0) {
                DTGUIHelper.ShowLargeBarAlert("You currently have no Dynamic Sound Groups created.");
            }

            int? indexToDelete = null;
            DTGUIHelper.ResetColors();

            GUI.color = Color.white;
            int? busToCreate = null;
            var isExistingBus = false;

            for (var i = 0; i < _groups.Count; i++) {
                var aGroup = _groups[i];

                var groupDirty = false;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(aGroup.name, GUILayout.MinWidth(150));

                GUILayout.FlexibleSpace();

                // find bus.
                var selectedBusIndex = aGroup.busIndex == -1 ? 0 : aGroup.busIndex;

                GUI.contentColor = Color.white;
                GUI.color = DTGUIHelper.BrightButtonColor;

                var busIndex = EditorGUILayout.Popup("", selectedBusIndex, busList.ToArray(), GUILayout.Width(busListWidth));
                if (busIndex == -1) {
                    busIndex = 0;
                }

                if (aGroup.busIndex != busIndex && busIndex != 1) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref groupDirty, aGroup, "change Group Bus");
                }

                if (busIndex != 1) { // don't change the index, so undo will work.
                    aGroup.busIndex = busIndex;
                }

                GUI.color = Color.white;

                if (selectedBusIndex != busIndex) {
                    if (busIndex == 1 || busIndex == 2) {
                        busToCreate = i;

                        isExistingBus = busIndex == 2;
                    } else if (busIndex >= DynamicSoundGroupCreator.HardCodedBusOptions) {
                        //GroupBus newBus = _creator.groupBuses[busIndex - MasterAudio.HARD_CODED_BUS_OPTIONS];
                        // do nothing unless we add muting and soloing here.
                    }
                }

                GUI.contentColor = DTGUIHelper.BrightTextColor;
                GUILayout.TextField(DTGUIHelper.DisplayVolumeNumber(aGroup.groupMasterVolume, sliderIndicatorChars), sliderIndicatorChars, EditorStyles.miniLabel, GUILayout.Width(sliderWidth));

                var newVol = DTGUIHelper.DisplayVolumeField(aGroup.groupMasterVolume, DTGUIHelper.VolumeFieldType.DynamicMixerGroup, MasterAudio.MixerWidthMode.Normal);
                if (newVol != aGroup.groupMasterVolume) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref groupDirty, aGroup, "change Group Volume");
                    aGroup.groupMasterVolume = newVol;
                }

                GUI.contentColor = Color.white;

                var buttonPressed = DTGUIHelper.AddDynamicGroupButtons(_creator);
                EditorGUILayout.EndHorizontal();

                switch (buttonPressed) {
                    case DTGUIHelper.DTFunctionButtons.Go:
                        Selection.activeGameObject = aGroup.gameObject;
                        break;
                    case DTGUIHelper.DTFunctionButtons.Remove:
                        indexToDelete = i;
                        break;
                    case DTGUIHelper.DTFunctionButtons.Play:
                        PreviewGroup(aGroup);
                        break;
                    case DTGUIHelper.DTFunctionButtons.Stop:
                        StopPreviewingGroup();
                        break;
                }

                if (groupDirty) {
                    EditorUtility.SetDirty(aGroup);
                }
            }

            if (busToCreate.HasValue) {
                CreateBus(busToCreate.Value, isExistingBus);
            }

            if (indexToDelete.HasValue) {
                AudioUndoHelper.DestroyForUndo(_groups[indexToDelete.Value].gameObject);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(10);

            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            if (_groups.Count > 0) {
                if (GUILayout.Button(new GUIContent("Max Group Volumes", "Reset all group volumes to full"),
                    EditorStyles.toolbarButton, GUILayout.Width(120))) {
                    AudioUndoHelper.RecordObjectsForUndo(_groups.ToArray(), "Max Group Volumes");

                    foreach (var aGroup in _groups) {
                        aGroup.groupMasterVolume = 1f;
                    }
                }
            }

            if (_creator.audioSourceTemplates.Count > 0 && !Application.isPlaying && _creator.transform.childCount > 0) {
                if (_groups.Count > 0) {
                    GUILayout.Space(10);
                }

                GUI.contentColor = DTGUIHelper.BrightButtonColor;

                if (GUILayout.Button("Apply Audio Source Template to All", EditorStyles.toolbarButton, GUILayout.Width(190))) {
                    applyTemplateToAll = true;
                }

            }
            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            //buses
            if (_creator.groupBuses.Count > 0) {
                DTGUIHelper.VerticalSpace(3);

                var voiceLimitedBuses = _creator.groupBuses.FindAll(delegate(GroupBus obj) {
                    return obj.voiceLimit >= 0;
                });

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Bus Control", GUILayout.Width(100));
                if (voiceLimitedBuses.Count > 0) {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Stop Oldest", GUILayout.Width(100));
                    GUILayout.Space(234);
                }
                EditorGUILayout.EndHorizontal();

                int? busToDelete = null;

                var showOcclusion = !maInScene || (MasterAudio.Instance.useOcclusion && MasterAudio.Instance.occlusionSelectType == MasterAudio.OcclusionSelectionType.TurnOnPerBusOrGroup);

                for (var i = 0; i < _creator.groupBuses.Count; i++) {
                    DTGUIHelper.StartGroupHeader(1, false);
                    var aBus = _creator.groupBuses[i];

                    var showingMixer = _creator.ShouldShowUnityAudioMixerGroupAssignments && !aBus.isExisting;

                    if (showingMixer) {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                    } else {
                        EditorGUILayout.BeginHorizontal();
                    }

                    var newBusName = EditorGUILayout.TextField("", aBus.busName, GUILayout.MaxWidth(170));
                    if (newBusName != aBus.busName) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Bus Name");
                        aBus.busName = newBusName;
                    }

                    GUILayout.FlexibleSpace();

                    if (!aBus.isExisting) {
                        if (voiceLimitedBuses.Contains(aBus)) {
                            GUI.color = DTGUIHelper.BrightButtonColor;
                            var newMono = GUILayout.Toggle(aBus.stopOldest, new GUIContent("", "Stop Oldest"));
                            if (newMono != aBus.stopOldest) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Stop Oldest");
                                aBus.stopOldest = newMono;
                            }
                        }

                        GUI.color = Color.white;
                        GUILayout.Label("Voices");
                        GUI.color = DTGUIHelper.BrightButtonColor;

                        var oldLimitIndex = busVoiceLimitList.IndexOf(aBus.voiceLimit.ToString());
                        if (oldLimitIndex == -1) {
                            oldLimitIndex = 0;
                        }
                        var busVoiceLimitIndex = EditorGUILayout.Popup("", oldLimitIndex, busVoiceLimitList.ToArray(), GUILayout.MaxWidth(70));
                        if (busVoiceLimitIndex != oldLimitIndex) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Bus Voice Limit");
                            aBus.voiceLimit = busVoiceLimitIndex <= 0 ? -1 : busVoiceLimitIndex;
                        }

                        GUI.color = DTGUIHelper.BrightTextColor;

                        GUILayout.TextField(DTGUIHelper.DisplayVolumeNumber(aBus.volume, sliderIndicatorChars), sliderIndicatorChars, EditorStyles.miniLabel, GUILayout.Width(sliderWidth));

                        GUI.color = Color.white;
                        var newBusVol = DTGUIHelper.DisplayVolumeField(aBus.volume, DTGUIHelper.VolumeFieldType.Bus, MasterAudio.MixerWidthMode.Normal);
                        if (newBusVol != aBus.volume) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Bus Volume");
                            aBus.volume = newBusVol;
                        }

                        GUI.contentColor = Color.white;
                    } else {
                        EditorGUILayout.LabelField("Existing bus. No control.");
                    }

                    if (DTGUIHelper.AddDeleteIcon("Bus")) {
                        busToDelete = i;
                    }

                    EditorGUILayout.EndHorizontal();

#if UNITY_5
                    if (showingMixer) {
                        var newChan = (AudioMixerGroup)  EditorGUILayout.ObjectField(aBus.mixerChannel, typeof(AudioMixerGroup), false);
                        if (newChan != aBus.mixerChannel) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Bus Mixer Group");
                            aBus.mixerChannel = newChan;
                        }
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.BeginHorizontal();
                    var new2D = GUILayout.Toggle(aBus.forceTo2D, "Force to 2D");
                    if (new2D != aBus.forceTo2D) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Force to 2D");
                        aBus.forceTo2D = new2D;
                    }


                    if (!aBus.forceTo2D) {
                        var newOcc = GUILayout.Toggle(aBus.isUsingOcclusion, "Use Occlusion");
                        if (newOcc != aBus.isUsingOcclusion) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Use Occlusion");
                            aBus.isUsingOcclusion = newOcc;
                        }
                    }
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.EndHorizontal();

#endif

                    EditorGUILayout.EndVertical();
                    DTGUIHelper.AddSpaceForNonU5(2);
                }

                if (showOcclusion && _creator.groupBuses.Count > 0 && !maInScene) {
                    DTGUIHelper.ShowLargeBarAlert("The Occlusion setting on Buses will only be used if the Master Audio prefab is set to allow Occlusion.");
                }

                if (busToDelete.HasValue) {
                    DeleteBus(busToDelete.Value);
                }

#if UNITY_5
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(6);

                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                var buttonText = "Show Unity Mixer Groups";
                if (_creator.showUnityMixerGroupAssignment) {
                    buttonText = "Hide Unity Mixer Groups";
                }
                if (GUILayout.Button(new GUIContent(buttonText), EditorStyles.toolbarButton, GUILayout.Width(140))) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, buttonText);
                    _creator.showUnityMixerGroupAssignment = !_creator.showUnityMixerGroupAssignment;
                }

                GUI.contentColor = Color.white;
                EditorGUILayout.EndHorizontal();
#endif
            }

            DTGUIHelper.EndGroupedControls();
        }

        if (applyTemplateToAll) {
            AudioUndoHelper.RecordObjectsForUndo(_groups.ToArray(), "Apply Audio Source Template to All");

            foreach (var myGroup in _groups) {
                for (var v = 0; v < myGroup.transform.childCount; v++) {
                    var aVar = myGroup.transform.GetChild(v);
                    var oldAudio = aVar.GetComponent<AudioSource>();
                    CopyFromAudioSourceTemplate(_creator, oldAudio, true);
                }
            }
        }

        DTGUIHelper.VerticalSpace(3);
        DTGUIHelper.ResetColors();

        // Music playlist Start		
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel = 0;  // Space will handle this for the header

        state = _creator.playListExpanded;
        text = "Dynamic Playlist Settings";

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

        GUILayout.Space(2f);


        if (state != _creator.playListExpanded) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Dynamic Playlist Settings");
            _creator.playListExpanded = state;
        }

        DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/DynamicSoundGroupCreators.htm#Playlists");

        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        EditorGUILayout.EndHorizontal();

        if (_creator.playListExpanded) {
            DTGUIHelper.BeginGroupedControls();
            EditorGUI.indentLevel = 0;  // Space will handle this for the header

            if (_creator.musicPlaylists.Count == 0) {
                DTGUIHelper.ShowLargeBarAlert("You currently have no Playlists set up.");
            }

            EditorGUI.indentLevel = 1;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            var oldPlayExpanded = DTGUIHelper.Foldout(_creator.playlistEditorExp, string.Format("Playlists ({0})", _creator.musicPlaylists.Count));
            if (oldPlayExpanded != _creator.playlistEditorExp) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Playlists");
                _creator.playlistEditorExp = oldPlayExpanded;
            }

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));

            const string buttonText = "Click to add new Playlist at the end";

            // Add button - Process presses later
            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            var addPressed = GUILayout.Button(new GUIContent("Add", buttonText),
                EditorStyles.toolbarButton);

            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            var content = new GUIContent("Collapse", "Click to collapse all");
            var masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton);

            content = new GUIContent("Expand", "Click to expand all");
            var masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton);
            if (masterExpand) {
                ExpandCollapseAllPlaylists(true);
            }
            if (masterCollapse) {
                ExpandCollapseAllPlaylists(false);
            }
            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            if (_creator.playlistEditorExp) {
                int? playlistToRemove = null;
                int? playlistToInsertAt = null;
                int? playlistToMoveUp = null;
                int? playlistToMoveDown = null;

                for (var i = 0; i < _creator.musicPlaylists.Count; i++) {
                    DTGUIHelper.StartGroupHeader();

                    var aList = _creator.musicPlaylists[i];

                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.BeginHorizontal();
                    aList.isExpanded = DTGUIHelper.Foldout(aList.isExpanded, "Playlist: " + aList.playlistName);

                    var playlistButtonPressed = DTGUIHelper.AddFoldOutListItemButtonItems(i, _creator.musicPlaylists.Count, "playlist", false, false, true);

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                    if (aList.isExpanded) {
                        EditorGUI.indentLevel = 0;
                        var newPlaylist = EditorGUILayout.TextField("Name", aList.playlistName);
                        if (newPlaylist != aList.playlistName) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Name");
                            aList.playlistName = newPlaylist;
                        }

                        var crossfadeMode = (MasterAudio.Playlist.CrossfadeTimeMode)EditorGUILayout.EnumPopup("Crossfade Mode", aList.crossfadeMode);
                        if (crossfadeMode != aList.crossfadeMode) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Crossfade Mode");
                            aList.crossfadeMode = crossfadeMode;
                        }
                        if (aList.crossfadeMode == MasterAudio.Playlist.CrossfadeTimeMode.Override) {
                            var newCf = EditorGUILayout.Slider("Crossfade time (sec)", aList.crossFadeTime, 0f, MasterAudio.MaxCrossFadeTimeSeconds);
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            if (newCf != aList.crossFadeTime) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Crossfade time (sec)");
                                aList.crossFadeTime = newCf;
                            }
                        }

                        var newFadeIn = EditorGUILayout.Toggle("Fade In First Song", aList.fadeInFirstSong);
                        if (newFadeIn != aList.fadeInFirstSong) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Fade In First Song");
                            aList.fadeInFirstSong = newFadeIn;
                        }

                        var newFadeOut = EditorGUILayout.Toggle("Fade Out Last Song", aList.fadeOutLastSong);
                        if (newFadeOut != aList.fadeOutLastSong) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Fade Out Last Song");
                            aList.fadeOutLastSong = newFadeOut;
                        }

                        var newTransType = (MasterAudio.SongFadeInPosition)EditorGUILayout.EnumPopup("Song Transition Type", aList.songTransitionType);
                        if (newTransType != aList.songTransitionType) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Song Transition Type");
                            aList.songTransitionType = newTransType;
                        }
                        if (aList.songTransitionType == MasterAudio.SongFadeInPosition.SynchronizeClips) {
                            DTGUIHelper.ShowColorWarning("All clips must be of exactly the same length in this mode.");
                        }

                        EditorGUI.indentLevel = 0;
                        var newBulkMode = DTGUIHelper.GetRestrictedAudioLocation("Clip Create Mode", aList.bulkLocationMode);
                        if (newBulkMode != aList.bulkLocationMode) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Bulk Clip Mode");
                            aList.bulkLocationMode = newBulkMode;
                        }

                        var playlistHasResource = false;
                        foreach (var t in aList.MusicSettings) {
                            if (t.audLocation != MasterAudio.AudioLocation.ResourceFile) {
                                continue;
                            }
                            playlistHasResource = true;
                            break;
                        }

                        if (MasterAudio.HasAsyncResourceLoaderFeature() && playlistHasResource) {
                            if (!maInScene || !ma.resourceClipsAllLoadAsync) {
                                var newAsync = EditorGUILayout.Toggle(new GUIContent("Load Resources Async", "Checking this means Resource files in this Playlist will be loaded asynchronously."), aList.resourceClipsAllLoadAsync);
                                if (newAsync != aList.resourceClipsAllLoadAsync) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Load Resources Async");
                                    aList.resourceClipsAllLoadAsync = newAsync;
                                }
                            }
                        }
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        GUI.contentColor = DTGUIHelper.BrightButtonColor;
                        if (GUILayout.Button(new GUIContent("Eq. Song Volumes"), EditorStyles.toolbarButton, GUILayout.Width(110))) {
                            EqualizePlaylistVolumes(aList.MusicSettings);
                        }

                        var hasExpanded = false;
                        foreach (var t in aList.MusicSettings) {
                            if (!t.isExpanded) {
                                continue;
                            }
                            hasExpanded = true;
                            break;
                        }

                        var theButtonText = hasExpanded ? "Collapse All" : "Expand All";

                        GUILayout.Space(10);
                        GUI.contentColor = DTGUIHelper.BrightButtonColor;
                        if (GUILayout.Button(new GUIContent(theButtonText), EditorStyles.toolbarButton, GUILayout.Width(100))) {
                            ExpandCollapseSongs(aList, !hasExpanded);
                        }
                        GUILayout.Space(10);
                        if (GUILayout.Button(new GUIContent("Sort Alpha"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
                            SortSongsAlpha(aList);
                        }

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.contentColor = Color.white;
                        EditorGUILayout.Separator();

                        EditorGUILayout.BeginVertical();
                        var anEvent = Event.current;

                        GUI.color = DTGUIHelper.DragAreaColor;

                        var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
                        GUI.Box(dragArea, "Drag Audio clips here to add to playlist!");

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

                                        AddSongToPlaylist(aList, aClip);
                                    }
                                }
                                Event.current.Use();
                                break;
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUI.indentLevel = 2;

                        int? addIndex = null;
                        int? removeIndex = null;
                        int? moveUpIndex = null;
                        int? moveDownIndex = null;
                        int? indexToClone = null;

                        if (aList.MusicSettings.Count == 0) {
                            EditorGUI.indentLevel = 0;
                            DTGUIHelper.ShowLargeBarAlert("You currently have no songs in this Playlist.");
                        }

                        EditorGUI.indentLevel = 0;

                        for (var j = 0; j < aList.MusicSettings.Count; j++) {
                            DTGUIHelper.StartGroupHeader(1);
                            var aSong = aList.MusicSettings[j];
                            var clipName = "Empty";
                            switch (aSong.audLocation) {
                                case MasterAudio.AudioLocation.Clip:
                                    if (aSong.clip != null) {
                                        clipName = aSong.clip.name;
                                    }
                                    break;
                                case MasterAudio.AudioLocation.ResourceFile:
                                    if (!string.IsNullOrEmpty(aSong.resourceFileName)) {
                                        clipName = aSong.resourceFileName;
                                    }
                                    break;
                            }
                            EditorGUILayout.BeginHorizontal();
                            EditorGUI.indentLevel = 1;

                            aSong.songName = aSong.alias;
                            if (!string.IsNullOrEmpty(clipName) && string.IsNullOrEmpty(aSong.songName)) {
                                switch (aSong.audLocation) {
                                    case MasterAudio.AudioLocation.Clip:
                                        aSong.songName = clipName;
                                        break;
                                    case MasterAudio.AudioLocation.ResourceFile:
                                        aSong.songName = clipName;
                                        break;
                                }
                            }

                            var newSongExpanded = DTGUIHelper.Foldout(aSong.isExpanded, aSong.songName);
                            if (newSongExpanded != aSong.isExpanded) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Song expand");
                                aSong.isExpanded = newSongExpanded;
                            }

                            var songButtonPressed = DTGUIHelper.AddFoldOutListItemButtonItems(j, aList.MusicSettings.Count, "clip", false, true, true, allowPreview);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();

                            if (aSong.isExpanded) {
                                EditorGUI.indentLevel = 0;

                                var newName = EditorGUILayout.TextField(new GUIContent("Song Alias (optional)", "When you 'Play song by name', Song Aliases will be searched first before audio file name."), aSong.alias);
                                if (newName != aSong.alias) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Song Id");
                                    aSong.alias = newName;
                                }

                                var oldLocation = aSong.audLocation;
                                var newClipSource = DTGUIHelper.GetRestrictedAudioLocation("Audio Origin", aSong.audLocation);
                                if (newClipSource != aSong.audLocation) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Audio Origin");
                                    aSong.audLocation = newClipSource;
                                }

                                switch (aSong.audLocation) {
                                    case MasterAudio.AudioLocation.Clip:
                                        var newClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", aSong.clip, typeof(AudioClip), true);
                                        if (newClip != aSong.clip) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Clip");
                                            aSong.clip = newClip;
                                            var cName = newClip == null ? "Empty" : newClip.name;
                                            aSong.songName = cName;
                                        }
                                        break;
                                    case MasterAudio.AudioLocation.ResourceFile:
                                        if (oldLocation != aSong.audLocation) {
                                            if (aSong.clip != null) {
                                                Debug.Log("Audio clip removed to prevent unnecessary memory usage on Resource file Playlist clip.");
                                            }
                                            aSong.clip = null;
                                            aSong.songName = string.Empty;
                                        }

                                        EditorGUILayout.BeginVertical();
                                        anEvent = Event.current;

                                        GUI.color = DTGUIHelper.DragAreaColor;
                                        dragArea = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
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

                                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Resource Filename");

                                                        var unused = false;
                                                        var resourceFileName = DTGUIHelper.GetResourcePath(aClip, ref unused, true);
                                                        if (string.IsNullOrEmpty(resourceFileName)) {
                                                            resourceFileName = aClip.name;
                                                        }

                                                        aSong.resourceFileName = resourceFileName;
                                                        aSong.songName = aClip.name;
                                                    }
                                                }
                                                Event.current.Use();
                                                break;
                                        }
                                        EditorGUILayout.EndVertical();

                                        var newFilename = EditorGUILayout.TextField("Resource Filename", aSong.resourceFileName);
                                        if (newFilename != aSong.resourceFileName) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Resource Filename");
                                            aSong.resourceFileName = newFilename;
                                        }

                                        break;
                                }

                                var newVol = DTGUIHelper.DisplayVolumeField(aSong.volume, DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true);
                                if (newVol != aSong.volume) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Volume");
                                    aSong.volume = newVol;
                                }

                                var newPitch = DTGUIHelper.DisplayPitchField(aSong.pitch);
                                if (newPitch != aSong.pitch) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Pitch");
                                    aSong.pitch = newPitch;
                                }

                                if (aList.songTransitionType == MasterAudio.SongFadeInPosition.SynchronizeClips) {
                                    DTGUIHelper.ShowLargeBarAlert("All songs must loop in Synchronized Playlists when crossfade time is not zero. Auto-advance is disabled.");
                                } else {
                                    var newLoop = EditorGUILayout.Toggle("Loop Clip", aSong.isLoop);
                                    if (newLoop != aSong.isLoop) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Loop Clip");
                                        aSong.isLoop = newLoop;
                                    }
                                }

                                if (aList.songTransitionType == MasterAudio.SongFadeInPosition.NewClipFromBeginning) {
                                    var newStart = EditorGUILayout.FloatField("Start Time (seconds)", aSong.customStartTime, GUILayout.Width(300));
                                    if (newStart < 0) {
                                        newStart = 0f;
                                    }
                                    if (newStart != aSong.customStartTime) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Start Time (seconds)");
                                        aSong.customStartTime = newStart;
                                    }
                                }


                                EditorGUI.indentLevel = 0;
                                exp = EditorGUILayout.BeginToggleGroup("Song Started Event", aSong.songStartedEventExpanded);
                                if (exp != aSong.songStartedEventExpanded) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle expand Song Started Event");
                                    aSong.songStartedEventExpanded = exp;
                                }
                                GUI.color = Color.white;

                                if (aSong.songStartedEventExpanded) {
                                    EditorGUI.indentLevel = 1;
                                    DTGUIHelper.ShowColorWarning("When song starts, fire Custom Event below.");

                                    if (maInScene) {
                                        var existingIndex = _customEventNames.IndexOf(aSong.songStartedCustomEvent);

                                        int? customEventIndex = null;

                                        var noEvent = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1) {
                                            customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                                            if (existingIndex == 1) {
                                                noEvent = true;
                                            }
                                        } else if (existingIndex == -1 && aSong.songStartedCustomEvent == MasterAudio.NoGroupName) {
                                            customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                                        } else { // non-match
                                            noMatch = true;
                                            var newEventName = EditorGUILayout.TextField("Custom Event Name", aSong.songStartedCustomEvent);
                                            if (newEventName != aSong.songStartedCustomEvent) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Custom Event Name");
                                                aSong.songStartedCustomEvent = newEventName;
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
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Custom Event");
                                            }
                                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                            if (customEventIndex.Value == -1) {
                                                aSong.songStartedCustomEvent = MasterAudio.NoGroupName;
                                            } else {
                                                aSong.songStartedCustomEvent = _customEventNames[customEventIndex.Value];
                                            }
                                        }
                                    } else {
                                        var newCustomEvent = EditorGUILayout.TextField("Custom Event Name", aSong.songStartedCustomEvent);
                                        if (newCustomEvent != aSong.songStartedCustomEvent) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "Custom Event Name");
                                            aSong.songStartedCustomEvent = newCustomEvent;
                                        }
                                    }
                                }
                                EditorGUILayout.EndToggleGroup();

                                EditorGUI.indentLevel = 0;
                                exp = EditorGUILayout.BeginToggleGroup("Song Changed Event", aSong.songChangedEventExpanded);
                                if (exp != aSong.songChangedEventExpanded) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle expand Song Changed Event");
                                    aSong.songChangedEventExpanded = exp;
                                }
                                GUI.color = Color.white;

                                if (aSong.songChangedEventExpanded) {
                                    EditorGUI.indentLevel = 1;
                                    DTGUIHelper.ShowColorWarning("When song changes to another, fire Custom Event below.");
                                    DTGUIHelper.ShowLargeBarAlert("If you are using gapless transitions, Song Changed Event cannot be used.");

                                    if (maInScene) {
                                        var existingIndex = _customEventNames.IndexOf(aSong.songChangedCustomEvent);

                                        int? customEventIndex = null;


                                        var noEvent = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1) {
                                            customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                                            if (existingIndex == 1) {
                                                noEvent = true;
                                            }
                                        } else if (existingIndex == -1 && aSong.songChangedCustomEvent == MasterAudio.NoGroupName) {
                                            customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                                        } else { // non-match
                                            noMatch = true;
                                            var newEventName = EditorGUILayout.TextField("Custom Event Name", aSong.songChangedCustomEvent);
                                            if (newEventName != aSong.songChangedCustomEvent) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Custom Event Name");
                                                aSong.songChangedCustomEvent = newEventName;
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
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Custom Event");
                                            }
                                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                            if (customEventIndex.Value == -1) {
                                                aSong.songChangedCustomEvent = MasterAudio.NoGroupName;
                                            } else {
                                                aSong.songChangedCustomEvent = _customEventNames[customEventIndex.Value];
                                            }
                                        }
                                    } else {
                                        var newCustomEvent = EditorGUILayout.TextField("Custom Event Name", aSong.songChangedCustomEvent);
                                        if (newCustomEvent != aSong.songChangedCustomEvent) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "Custom Event Name");
                                            aSong.songChangedCustomEvent = newCustomEvent;
                                        }
                                    }
                                }
                                EditorGUILayout.EndToggleGroup();
                            }

                            switch (songButtonPressed) {
                                case DTGUIHelper.DTFunctionButtons.Add:
                                    addIndex = j;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.Remove:
                                    removeIndex = j;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.Clone:
                                    indexToClone = j;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.ShiftUp:
                                    moveUpIndex = j;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.ShiftDown:
                                    moveDownIndex = j;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.Play:
                                    StopPreviewer();
                                    switch (aSong.audLocation) {
                                        case MasterAudio.AudioLocation.Clip:
                                            GetPreviewer().PlayOneShot(aSong.clip, aSong.volume);
                                            break;
                                        case MasterAudio.AudioLocation.ResourceFile:
                                            GetPreviewer().PlayOneShot(Resources.Load(aSong.resourceFileName) as AudioClip, aSong.volume);
                                            break;
                                    }
                                    break;
                                case DTGUIHelper.DTFunctionButtons.Stop:
                                    GetPreviewer().clip = null;
                                    StopPreviewer();
                                    break;
                            }

                            EditorGUILayout.EndVertical();
                            DTGUIHelper.AddSpaceForNonU5(2);
                        }

                        if (addIndex.HasValue) {
                            var mus = new MusicSetting();
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "add song");
                            aList.MusicSettings.Insert(addIndex.Value + 1, mus);
                        } else if (removeIndex.HasValue) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "delete song");
                            aList.MusicSettings.RemoveAt(removeIndex.Value);
                        } else if (moveUpIndex.HasValue) {
                            var item = aList.MusicSettings[moveUpIndex.Value];

                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "shift up song");

                            aList.MusicSettings.Insert(moveUpIndex.Value - 1, item);
                            aList.MusicSettings.RemoveAt(moveUpIndex.Value + 1);
                        } else if (moveDownIndex.HasValue) {
                            var index = moveDownIndex.Value + 1;
                            var item = aList.MusicSettings[index];

                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "shift down song");

                            aList.MusicSettings.Insert(index - 1, item);
                            aList.MusicSettings.RemoveAt(index + 1);
                        } else if (indexToClone.HasValue) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "clone song");
                            aList.MusicSettings.Insert(indexToClone.Value, MusicSetting.Clone(aList.MusicSettings[indexToClone.Value]));
                        }
                    }

                    switch (playlistButtonPressed) {
                        case DTGUIHelper.DTFunctionButtons.Remove:
                            playlistToRemove = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Add:
                            playlistToInsertAt = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.ShiftUp:
                            playlistToMoveUp = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.ShiftDown:
                            playlistToMoveDown = i;
                            break;
                    }

                    EditorGUILayout.EndVertical();
                    DTGUIHelper.AddSpaceForNonU5(4);
                }


                if (playlistToRemove.HasValue) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "delete Playlist");

                    _creator.musicPlaylists.RemoveAt(playlistToRemove.Value);
                }
                if (playlistToInsertAt.HasValue) {
                    var pl = new MasterAudio.Playlist();
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "add Playlist");
                    _creator.musicPlaylists.Insert(playlistToInsertAt.Value + 1, pl);
                }
                if (playlistToMoveUp.HasValue) {
                    var item = _creator.musicPlaylists[playlistToMoveUp.Value];
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "shift up Playlist");
                    _creator.musicPlaylists.Insert(playlistToMoveUp.Value - 1, item);
                    _creator.musicPlaylists.RemoveAt(playlistToMoveUp.Value + 1);
                }
                if (playlistToMoveDown.HasValue) {
                    var index = playlistToMoveDown.Value + 1;
                    var item = _creator.musicPlaylists[index];

                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "shift down Playlist");

                    _creator.musicPlaylists.Insert(index - 1, item);
                    _creator.musicPlaylists.RemoveAt(index + 1);
                }
            }

            if (addPressed) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "add Playlist");
                _creator.musicPlaylists.Add(new MasterAudio.Playlist());
            }

            DTGUIHelper.EndGroupedControls();
        }
        // Music playlist End

        EditorGUI.indentLevel = 0;
        // Show Custom Events

        DTGUIHelper.VerticalSpace(3);
        DTGUIHelper.ResetColors();

        state = _creator.showCustomEvents;
        text = "Dynamic Custom Events";

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

        GUILayout.Space(2f);

        if (_creator.showCustomEvents != state) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Dynamic Custom Events");
            _creator.showCustomEvents = state;
        }

        DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/DynamicSoundGroupCreators.htm#CustomEvents");
        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        if (_creator.showCustomEvents) {
            DTGUIHelper.BeginGroupedControls();
            var newEvent = EditorGUILayout.TextField("New Event Name", _creator.newEventName);
            if (newEvent != _creator.newEventName) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change New Event Name");
                _creator.newEventName = newEvent;
            }

            GUI.contentColor = DTGUIHelper.BrightButtonColor;

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("Create New Event", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                CreateCustomEvent(_creator.newEventName);
            }

            GUILayout.Space(10);
            GUI.contentColor = DTGUIHelper.BrightButtonColor;

            var hasExpanded = false;
            foreach (var t in _creator.customEventsToCreate) {
                if (!t.eventExpanded) {
                    continue;
                }
                hasExpanded = true;
                break;
            }

            var buttonText = hasExpanded ? "Collapse All" : "Expand All";

            if (GUILayout.Button(buttonText, EditorStyles.toolbarButton, GUILayout.Width(100))) {
                ExpandCollapseCustomEvents(!hasExpanded);
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Sort Alpha", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                SortCustomEvents();
            }

            GUI.contentColor = Color.white;

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = 0;
            if (_creator.customEventsToCreate.Count == 0) {
                DTGUIHelper.ShowLargeBarAlert("You currently have no custom events defined here.");
            }

            int? indexToDelete = null;
            int? indexToRename = null;

            for (var i = 0; i < _creator.customEventsToCreate.Count; i++) {
                DTGUIHelper.AddSpaceForNonU5(2);
                DTGUIHelper.StartGroupHeader();
                EditorGUI.indentLevel = 1;
                var anEvent = _creator.customEventsToCreate[i];

                EditorGUILayout.BeginHorizontal();
                exp = DTGUIHelper.Foldout(anEvent.eventExpanded, anEvent.EventName);
                if (exp != anEvent.eventExpanded) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle expand Custom Event");
                    anEvent.eventExpanded = exp;
                }

                GUILayout.FlexibleSpace();
                var newName = GUILayout.TextField(anEvent.ProspectiveName, GUILayout.Width(170));
                if (newName != anEvent.ProspectiveName) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Proposed Event Name");
                    anEvent.ProspectiveName = newName;
                }

                var buttonPressed = DTGUIHelper.AddDeleteIcon(true, "Custom Event");

                switch (buttonPressed) {
                    case DTGUIHelper.DTFunctionButtons.Remove:
                        indexToDelete = i;
                        break;
                    case DTGUIHelper.DTFunctionButtons.Rename:
                        indexToRename = i;
                        break;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                if (!anEvent.eventExpanded) {
                    EditorGUILayout.EndVertical();
                    continue;
                }

                EditorGUI.indentLevel = 0;
                var rcvMode = (MasterAudio.CustomEventReceiveMode)EditorGUILayout.EnumPopup("Send To Receivers", anEvent.eventReceiveMode);
                if (rcvMode != anEvent.eventReceiveMode) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Send To Receivers");
                    anEvent.eventReceiveMode = rcvMode;
                }

                if (rcvMode == MasterAudio.CustomEventReceiveMode.WhenDistanceLessThan || rcvMode == MasterAudio.CustomEventReceiveMode.WhenDistanceMoreThan) {
                    var newDist = EditorGUILayout.Slider("Distance Threshold", anEvent.distanceThreshold, 0f, float.MaxValue);
                    if (newDist != anEvent.distanceThreshold) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "change Distance Threshold");
                        anEvent.distanceThreshold = newDist;
                    }
                }

                EditorGUILayout.EndVertical();
            }

            if (indexToDelete.HasValue) {
                _creator.customEventsToCreate.RemoveAt(indexToDelete.Value);
            }
            if (indexToRename.HasValue) {
                RenameEvent(_creator.customEventsToCreate[indexToRename.Value]);
            }

            DTGUIHelper.EndGroupedControls();
        }

        // End Show Custom Events

        if (GUI.changed || _isDirty) {
            EditorUtility.SetDirty(target);
        }

        //DrawDefaultInspector();
    }

    private Transform CreateGroup(AudioClip aClip) {
        if (_creator.groupTemplate == null) {
            DTGUIHelper.ShowAlert("Your 'Group Template' field is empty, please assign it in debug mode. Drag the 'DynamicSoundGroup' prefab from MasterAudio/Sources/Prefabs into that field, then switch back to normal mode.");
            return null;
        }

        var groupName = UtilStrings.TrimSpace(aClip.name);

        var matchingGroup = _groups.Find(delegate(DynamicSoundGroup obj) {
            return obj.transform.name == groupName;
        });

        if (matchingGroup != null) {
            DTGUIHelper.ShowAlert("You already have a Group named '" + groupName + "'. \n\nPlease rename this Group when finished to be unique.");
        }

        var spawnedGroup = (GameObject)Instantiate(_creator.groupTemplate, _creator.transform.position, Quaternion.identity);
        spawnedGroup.name = groupName;

        AudioUndoHelper.CreateObjectForUndo(spawnedGroup, "create Dynamic Group");
        spawnedGroup.transform.parent = _creator.transform;

        CreateVariation(spawnedGroup.transform, aClip);

        return spawnedGroup.transform;
    }

    private void CreateVariation(Transform aGroup, AudioClip aClip) {
        if (_creator.variationTemplate == null) {
            DTGUIHelper.ShowAlert("Your 'Variation Template' field is empty, please assign it in debug mode. Drag the 'DynamicGroupVariation' prefab from MasterAudio/Sources/Prefabs into that field, then switch back to normal mode.");
            return;
        }

        var resourceFileName = string.Empty;
        var useLocalization = false;
        if (_creator.bulkVariationMode == MasterAudio.AudioLocation.ResourceFile) {
            resourceFileName = DTGUIHelper.GetResourcePath(aClip, ref useLocalization);
            if (string.IsNullOrEmpty(resourceFileName)) {
                resourceFileName = aClip.name;
            }
        }

        var clipName = UtilStrings.TrimSpace(aClip.name);

        var myGroup = aGroup.GetComponent<DynamicSoundGroup>();

        var matches = myGroup.groupVariations.FindAll(delegate(DynamicGroupVariation obj) {
            return obj.name == clipName;
        });

        if (matches.Count > 0) {
            DTGUIHelper.ShowAlert("You already have a variation for this Group named '" + clipName + "'. \n\nPlease rename these variations when finished to be unique, or you may not be able to play them by name if you have a need to.");
        }

        var spawnedVar = (GameObject)Instantiate(_creator.variationTemplate, _creator.transform.position, Quaternion.identity);
        spawnedVar.name = clipName;

        spawnedVar.transform.parent = aGroup;

        var dynamicVar = spawnedVar.GetComponent<DynamicGroupVariation>();

        if (_creator.bulkVariationMode == MasterAudio.AudioLocation.ResourceFile) {
            dynamicVar.audLocation = MasterAudio.AudioLocation.ResourceFile;
            dynamicVar.resourceFileName = resourceFileName;
            dynamicVar.useLocalization = useLocalization;
        } else {
            dynamicVar.VarAudio.clip = aClip;
        }

        CopyFromAudioSourceTemplate(_creator, dynamicVar.VarAudio, false);
    }

    private void CreateCustomEvent(string newEventName) {
        var match = _creator.customEventsToCreate.FindAll(delegate(CustomEvent custEvent) {
            return custEvent.EventName == newEventName;
        });

        if (match.Count > 0) {
            DTGUIHelper.ShowAlert("You already have a custom event named '" + newEventName + "' configured here. Please choose a different name.");
            return;
        }

        var newEvent = new CustomEvent(newEventName);

        _creator.customEventsToCreate.Add(newEvent);
    }

    private void RenameEvent(CustomEvent cEvent) {
        var match = _creator.customEventsToCreate.FindAll(delegate(CustomEvent obj) {
            return obj.EventName == cEvent.ProspectiveName;
        });

        if (match.Count > 0) {
            DTGUIHelper.ShowAlert("You already have a custom event named '" + cEvent.ProspectiveName + "' configured here. Please choose a different name.");
            return;
        }

        cEvent.EventName = cEvent.ProspectiveName;
    }

    private void PreviewGroup(DynamicSoundGroup aGroup) {
        var rndIndex = Random.Range(0, aGroup.groupVariations.Count);
        var rndVar = aGroup.groupVariations[rndIndex];

        switch (rndVar.audLocation) {
            case MasterAudio.AudioLocation.ResourceFile:
                StopPreviewer();
                var fileName = AudioResourceOptimizer.GetLocalizedDynamicSoundGroupFileName(_creator.previewLanguage, rndVar.useLocalization, rndVar.resourceFileName);

                var clip = Resources.Load(fileName) as AudioClip;
                if (clip != null) {
                    GetPreviewer().PlayOneShot(clip, rndVar.VarAudio.volume * aGroup.groupMasterVolume);
                } else {
                    DTGUIHelper.ShowAlert("Could not find Resource file: " + fileName);
                }
                break;
            case MasterAudio.AudioLocation.Clip:
                GetPreviewer().PlayOneShot(rndVar.VarAudio.clip, rndVar.VarAudio.volume * aGroup.groupMasterVolume);
                break;
            case MasterAudio.AudioLocation.FileOnInternet:
                if (!string.IsNullOrEmpty(rndVar.internetFileUrl)) {
                    Application.OpenURL(rndVar.internetFileUrl);
                }
                break;
        }
    }

    private void StopPreviewingGroup() {
        GetPreviewer().Stop();
    }

    private List<string> GroupNameList {
        get {
            var groupNames = new List<string> { MasterAudio.NoGroupName };

            foreach (var t in _groups) {
                groupNames.Add(t.name);
            }

            return groupNames;
        }
    }

    private void DeleteBus(int busIndex) {
        var groupsWithBus = new List<DynamicSoundGroup>();
        var groupsWithHigherBus = new List<DynamicSoundGroup>();

        foreach (var aGroup in _groups) {
            if (aGroup.busIndex == -1) {
                continue;
            }
            if (aGroup.busIndex == busIndex + DynamicSoundGroupCreator.HardCodedBusOptions) {
                groupsWithBus.Add(aGroup);
            } else if (aGroup.busIndex > busIndex + DynamicSoundGroupCreator.HardCodedBusOptions) {
                groupsWithHigherBus.Add(aGroup);
            }
        }

        var allObjects = new List<Object> { _creator };
        foreach (var g in groupsWithBus) {
            allObjects.Add(g);
        }

        foreach (var g in groupsWithHigherBus) {
            allObjects.Add(g);
        }

        AudioUndoHelper.RecordObjectsForUndo(allObjects.ToArray(), "delete Bus");

        // change all
        _creator.groupBuses.RemoveAt(busIndex);

        foreach (var group in groupsWithBus) {
            group.busIndex = -1;
        }

        foreach (var group in groupsWithHigherBus) {
            group.busIndex--;
        }
    }

    private void CreateBus(int groupIndex, bool isExisting) {
        var sourceGroup = _groups[groupIndex];

        var affectedObjects = new Object[] {
			_creator,
			sourceGroup
		};

        AudioUndoHelper.RecordObjectsForUndo(affectedObjects, "create Bus");

        var newBusName = isExisting ? ExistingNameName : MasterAudioInspector.RenameMeBusName;

        var newBus = new GroupBus {
            busName = newBusName,
            isExisting = isExisting
        };


        _creator.groupBuses.Add(newBus);

        sourceGroup.busIndex = DynamicSoundGroupCreator.HardCodedBusOptions + _creator.groupBuses.Count - 1;
    }

    private void ExpandCollapseAllPlaylists(bool expand) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "toggle Expand / Collapse Playlists");

        foreach (var aList in _creator.musicPlaylists) {
            aList.isExpanded = expand;

            foreach (var aSong in aList.MusicSettings) {
                aSong.isExpanded = expand;
            }
        }
    }

    private void EqualizePlaylistVolumes(List<MusicSetting> playlistClips) {
        var clips = new Dictionary<MusicSetting, float>();

        if (playlistClips.Count < 2) {
            DTGUIHelper.ShowAlert("You must have at least 2 clips in a Playlist to use this function.");
            return;
        }

        var lowestVolume = 1f;

        foreach (var setting in playlistClips) {
            var ac = setting.clip;

            if (setting.audLocation == MasterAudio.AudioLocation.Clip && ac == null) {
                continue;
            }

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (setting.audLocation == MasterAudio.AudioLocation.FileOnInternet) {
                continue;
            }

            if (setting.audLocation == MasterAudio.AudioLocation.ResourceFile) {
                ac = Resources.Load(setting.resourceFileName) as AudioClip;
                if (ac == null) {
                    Debug.LogError("Song '" + setting.resourceFileName + "' could not be loaded and is being skipped.");
                    continue;
                }
            }

            var average = 0f;
            var buffer = new float[ac.samples];

            Debug.Log("Measuring amplitude of '" + ac.name + "'.");

            try {
                ac.GetData(buffer, 0);
            }
            catch {
                Debug.Log("Could not read data from compressed sample. Skipping '" + setting.clip.name + "'.");
                continue;
            }

            for (var c = 0; c < ac.samples; c++) {
                average += Mathf.Pow(buffer[c], 2);
            }

            average = Mathf.Sqrt(1f / ac.samples * average);

            if (average == 0) {
                Debug.LogError("Song '" + setting.songName + "' is being excluded because it's compressed or streaming.");
                continue;
            }

            if (average < lowestVolume) {
                lowestVolume = average;
            }

            clips.Add(setting, average);
        }

        if (clips.Count < 2) {
            DTGUIHelper.ShowAlert("You must have at least 2 clips in a Playlist to use this function.");
            return;
        }

        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "Equalize Song Volumes");

        foreach (var kv in clips) {
            var adjustedVol = lowestVolume / kv.Value;
            //set your volume for each song in your playlist.
            kv.Key.volume = adjustedVol;
        }
    }

    private void AddSongToPlaylist(MasterAudio.Playlist pList, AudioClip aClip) {
        MusicSetting lastClip = null;
        if (pList.MusicSettings.Count > 0) {
            lastClip = pList.MusicSettings[pList.MusicSettings.Count - 1];
        }

        MusicSetting mus;

        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "add Song");

        if (lastClip != null && lastClip.clip == null) {
            mus = lastClip;
            mus.clip = aClip;
        } else {
            mus = new MusicSetting {
                volume = 1f,
                pitch = 1f,
                isExpanded = true,
                audLocation = pList.bulkLocationMode
            };

            switch (pList.bulkLocationMode) {
                case MasterAudio.AudioLocation.Clip:
                    mus.clip = aClip;
                    if (aClip != null) {
                        mus.songName = aClip.name;
                    }
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    var unused = false;
                    var resourceFileName = DTGUIHelper.GetResourcePath(aClip, ref unused);
                    if (string.IsNullOrEmpty(resourceFileName)) {
                        resourceFileName = aClip.name;
                    }

                    mus.resourceFileName = resourceFileName;
                    mus.songName = aClip.name;
                    break;
            }

            pList.MusicSettings.Add(mus);
        }
    }

    private void SortCustomEvents() {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "Sort Custom Events Alpha");

        _creator.customEventsToCreate.Sort(delegate(CustomEvent x, CustomEvent y) {
            return x.EventName.CompareTo(y.EventName);
        });
    }

    private void ExpandCollapseCustomEvents(bool shouldExpand) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "Expand / Collapse All Custom Events");

        foreach (var t in _creator.customEventsToCreate) {
            t.eventExpanded = shouldExpand;
        }
    }

    private void SortSongsAlpha(MasterAudio.Playlist aList) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "Sort Playlist Songs Alpha");

        aList.MusicSettings.Sort(delegate(MusicSetting x, MusicSetting y) {
            return x.songName.CompareTo(y.songName);
        });
    }

    private void ExpandCollapseSongs(MasterAudio.Playlist aList, bool shouldExpand) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "Expand / Collapse Playlist Songs");

        foreach (var t in aList.MusicSettings) {
            t.isExpanded = shouldExpand;
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

        UnityEditorInternal.ComponentUtility.CopyComponent(_creator.variationTemplate.GetComponent<AudioSource>());
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(_previewer);

        aud = _previewer.GetComponent<AudioSource>();

        return aud;
    }

    private void AddAudioSourceTemplate(GameObject temp) {
        if (_audioSourceTemplateNames.Contains(temp.name)) {
            Debug.LogError("There is already an Audio Source Template named '" + temp.name + "'. The names of Templates must be unique.");
            return;
        }

        if (temp.GetComponent<AudioSource>() == null) {
            Debug.LogError("This is not an Audio Source Template. It must have an Audio Source component in the top-level.");
            return;
        }

        var hasAudioClips = false;
        for (var i = 0; i < temp.transform.childCount; i++) {
            var aChild = temp.transform.GetChild(i);
            var aud = aChild.transform.GetComponent<AudioSource>();
            if (aud == null) {
                continue;
            }

            if (aud.clip != null) {
                hasAudioClips = true;
            }
        }

        if (hasAudioClips) {
            Debug.LogError("Audio Source Templates cannot include any Audio Clips. Please remove them and try again.");
            return;
        }

        var hasFilters = false;
        for (var i = 0; i < temp.transform.childCount; i++) {
            var aChild = temp.transform.GetChild(i);

            if (aChild.GetComponent<AudioDistortionFilter>() != null
                || aChild.GetComponent<AudioHighPassFilter>() != null
                || aChild.GetComponent<AudioLowPassFilter>() != null
                || aChild.GetComponent<AudioReverbFilter>() != null
                || aChild.GetComponent<AudioEchoFilter>() != null
                || aChild.GetComponent<AudioChorusFilter>() != null) {

                hasFilters = true;
            }
        }

        if (hasFilters) {
            Debug.LogError("Audio Source Templates cannot include any Filter FX in their Variations. Please remove them and try again.");
            return;
        }

        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _creator, "Add Audio Source Template");
        _creator.audioSourceTemplates.Add(temp.gameObject);
        _creator.audioSourceTemplates.Sort(delegate(GameObject x, GameObject y) {
            return x.name.CompareTo(y.name);
        });

        Debug.Log("Added Audio Source Template '" + temp.name + "'");
    }

    // ReSharper disable once UnusedMember.Local
    private static GameObject SelectedAudioSourceTemplate(DynamicSoundGroupCreator creator) {
        if (creator.audioSourceTemplates.Count == 0) {
            return null;
        }

        var selTemplate = creator.audioSourceTemplates.Find(delegate(GameObject obj) {
            return obj.name == creator.audioSourceTemplateName;
        });

        return selTemplate;
    }

    public static void CopyFromAudioSourceTemplate(DynamicSoundGroupCreator creator, AudioSource oldAudSrc, bool showError) {
        var selSource = SelectedAudioSourceTemplate(creator);
        if (selSource == null) {
            if (showError) {
                Debug.LogError("No Audio Source Template selected.");
            }
            return;
        }

        var templateAudio = selSource.GetComponent<AudioSource>();

        var oldPitch = oldAudSrc.pitch;
        var oldLoop = oldAudSrc.loop;
        var oldClip = oldAudSrc.clip;
        var oldVol = oldAudSrc.volume;

        UnityEditorInternal.ComponentUtility.CopyComponent(templateAudio);
        UnityEditorInternal.ComponentUtility.PasteComponentValues(oldAudSrc);

        oldAudSrc.pitch = oldPitch;
        oldAudSrc.loop = oldLoop;
        oldAudSrc.clip = oldClip;
        oldAudSrc.volume = oldVol;
    }
}
