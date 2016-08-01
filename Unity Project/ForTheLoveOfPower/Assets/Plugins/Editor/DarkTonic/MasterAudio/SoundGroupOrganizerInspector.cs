using System;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor(typeof(SoundGroupOrganizer))]
// ReSharper disable once CheckNamespace
public class SoundGroupOrganizerInspector : Editor {
    private SoundGroupOrganizer _organizer;
    private List<DynamicSoundGroup> _groups;
    private bool _isDirty;
    private GameObject _previewer;

    // ReSharper disable once FunctionComplexityOverflow
    public override void OnInspectorGUI() {
        _isDirty = false;

        if (MasterAudioInspectorResources.LogoTexture != null) {
            DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.LogoTexture);
        }

        _organizer = (SoundGroupOrganizer)target;

        if (Application.isPlaying) {
            DTGUIHelper.ShowRedError("Sound Group Inspector cannot be used at runtime. Press stop to use it.");
            return;
        }

        DTGUIHelper.HelpHeader("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/SoundGroupOrganizer.htm");

        _groups = ScanForGroups();

        var isInProjectView = DTGUIHelper.IsPrefabInProjectView(_organizer);

        _previewer = _organizer.gameObject;

        if (MasterAudio.Instance == null) {
            var newLang = (SystemLanguage)EditorGUILayout.EnumPopup(new GUIContent("Preview Language", "This setting is only used (and visible) to choose the previewing language when there's no Master Audio prefab in the Scene (language settings are grabbed from there normally). This should only happen when you're using a Master Audio prefab from a previous Scene in persistent mode."), _organizer.previewLanguage);
            if (newLang != _organizer.previewLanguage) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Preview Language");
                _organizer.previewLanguage = newLang;
            }
        }

        var ma = MasterAudio.Instance;

        var sources = new List<GameObject>();
        if (ma != null) {
            sources.Add(ma.gameObject);
        }

        var dgscs = FindObjectsOfType(typeof(DynamicSoundGroupCreator));
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var t in dgscs) {
            var dsgc = (DynamicSoundGroupCreator)t;
            sources.Add(dsgc.gameObject);
        }

        var sourceNames = new List<string>();
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var t in sources) {
            sourceNames.Add(t.name);
        }

        var scannedDest = false;

        var newType = (SoundGroupOrganizer.MAItemType)EditorGUILayout.EnumPopup("Item Type", _organizer.itemType);
        if (newType != _organizer.itemType) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Item Type");
            _organizer.itemType = newType;
        }

        var newMode = (SoundGroupOrganizer.TransferMode)EditorGUILayout.EnumPopup("Transfer Mode", _organizer.transMode);
        if (newMode != _organizer.transMode) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Transfer Mode");
            _organizer.transMode = newMode;

            RescanDestinationGroups();
            scannedDest = true;
        }

        if (!scannedDest && _organizer.selectedDestSoundGroups.Count == 0) {
            RescanDestinationGroups();
            // ReSharper disable once RedundantAssignment
            scannedDest = true;
        }

        var shouldRescanGroups = false;
        var hasRescannedGroups = false;
        var shouldRescanEvents = false;
        var hasRescannedEvents = false;

        if (_organizer.itemType == SoundGroupOrganizer.MAItemType.SoundGroups) {
            switch (_organizer.transMode) {
                case SoundGroupOrganizer.TransferMode.Import:
                    if (sources.Count == 0) {
                        DTGUIHelper.ShowRedError("You have no Master Audio or Dynamic Sound Group Creator prefabs in this Scene. Can't import.");
                    } else if (isInProjectView) {
                        DTGUIHelper.ShowRedError("You are in Project View and can't import. Create this prefab with Master Audio Manager.");
                    } else {
                        var srcIndex = sources.IndexOf(_organizer.sourceObject);
                        if (srcIndex < 0) {
                            srcIndex = 0;
                        }

                        DTGUIHelper.StartGroupHeader();
                        var newIndex = EditorGUILayout.Popup("Source Object", srcIndex, sourceNames.ToArray());
                        if (newIndex != srcIndex) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Source Object");
                        }
                        EditorGUILayout.EndVertical();

                        var newSource = sources[newIndex];
						var hasSourceChanged = newSource != _organizer.sourceObject;
						_organizer.sourceObject = newSource;
						
						if (!hasRescannedGroups && (hasSourceChanged || _organizer.selectedSourceSoundGroups.Count == 0)) {
                            if (RescanSourceGroups()) {
                                hasRescannedGroups = true;
                            }
                        }

                        if (!hasRescannedGroups && _organizer.selectedSourceSoundGroups.Count != _organizer.sourceObject.transform.childCount) {
                            if (RescanSourceGroups()) {
                                hasRescannedGroups = true;
                            }
                        }

                        if (_organizer.sourceObject != null) {
                            if (_organizer.selectedSourceSoundGroups.Count > 0) {
                                DTGUIHelper.ShowLargeBarAlert("Check Groups to Import below and click 'Import'");
                            } else {
                                DTGUIHelper.ShowRedError("Source Object has no Groups to import.");
                            }

                            EditorGUI.indentLevel = 0;

                            foreach (var aGroup in _organizer.selectedSourceSoundGroups) {
                                if (!hasRescannedGroups && aGroup.Go == null) {
                                    shouldRescanGroups = true;
                                    continue;
                                }

                                var newSel = EditorGUILayout.Toggle(aGroup.Go.name, aGroup.IsSelected);
                                if (newSel == aGroup.IsSelected) {
                                    continue;
                                }

                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "toggle Sound Group selection");
                                aGroup.IsSelected = newSel;
                            }
                        }

                        if (!hasRescannedGroups && shouldRescanGroups) {
                            if (RescanSourceGroups()) {
                                // ReSharper disable once RedundantAssignment
                                hasRescannedGroups = true;
                            }
                        }

                        if (_organizer.selectedSourceSoundGroups.Count > 0) {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            GUI.contentColor = DTGUIHelper.BrightButtonColor;
                            if (GUILayout.Button(new GUIContent("Import", "Import Selected Groups"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                ImportSelectedGroups();
                            }

                            GUI.contentColor = DTGUIHelper.BrightButtonColor;
                            GUILayout.Space(10);
                            if (GUILayout.Button(new GUIContent("Check All", "Check all Groups above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                CheckUncheckAllSourceGroups(true);
                            }
                            GUILayout.Space(10);
                            if (GUILayout.Button(new GUIContent("Uncheck All", "Uncheck all Groups above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                CheckUncheckAllSourceGroups(false);
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                    }
                    break;
                case SoundGroupOrganizer.TransferMode.Export:
                    if (_groups.Count == 0) {
                        DTGUIHelper.ShowRedError("You have no Groups to export. Import or create some first.");
                    } else if (sources.Count == 0) {
                        DTGUIHelper.ShowRedError("You have no Master Audio or Dynamic Sound Group Creator prefabs in this Scene to export to.");
                    } else {
                        var destIndex = sources.IndexOf(_organizer.destObject);
                        if (destIndex < 0) {
                            destIndex = 0;
                        }

                        DTGUIHelper.StartGroupHeader();

                        var newIndex = EditorGUILayout.Popup("Destination Object", destIndex, sourceNames.ToArray());
                        if (newIndex != destIndex) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Destination Object");
                        }
                        var newDest = sources[newIndex];
                        EditorGUILayout.EndVertical();

                        _organizer.destObject = newDest;
                        DTGUIHelper.ShowLargeBarAlert("Check Groups to export (same as Group Control below) and click 'Export'");

                        if (_organizer.destObject != null) {
                            EditorGUI.indentLevel = 0;

                            foreach (var aGroup in _organizer.selectedDestSoundGroups) {
                                if (!hasRescannedGroups && aGroup.Go == null) {
                                    shouldRescanGroups = true;
                                    continue;
                                }

                                var newSel = EditorGUILayout.Toggle(aGroup.Go.name, aGroup.IsSelected);
                                if (newSel == aGroup.IsSelected) {
                                    continue;
                                }

                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "toggle Sound Group selection");
                                aGroup.IsSelected = newSel;
                            }
                        }

                        if (!hasRescannedGroups && shouldRescanGroups) {
                            RescanDestinationGroups();
                            // ReSharper disable once RedundantAssignment
                            hasRescannedGroups = true;
                        }

                        if (_organizer.selectedDestSoundGroups.Count > 0) {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            GUI.contentColor = DTGUIHelper.BrightButtonColor;
                            if (GUILayout.Button(new GUIContent("Export", "Export Selected Groups"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                ExportSelectedGroups();
                            }

                            GUI.contentColor = DTGUIHelper.BrightButtonColor;
                            GUILayout.Space(10);
                            if (GUILayout.Button(new GUIContent("Check All", "Check all Groups above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                CheckUncheckAllDestGroups(true);
                            }
                            GUILayout.Space(10);
                            if (GUILayout.Button(new GUIContent("Uncheck All", "Uncheck all Groups above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                CheckUncheckAllDestGroups(false);
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                    }

                    break;
            }
        } else {
            // custom events
            switch (_organizer.transMode) {
                case SoundGroupOrganizer.TransferMode.Import:
                    if (sources.Count == 0) {
                        DTGUIHelper.ShowRedError("You have no Master Audio or Dynamic Sound Group Creator prefabs in this Scene. Can't import.");
                    } else if (isInProjectView) {
                        DTGUIHelper.ShowRedError("You are in Project View and can't import. Create this prefab with Master Audio Manager.");
                    } else {
                        var srcMa = _organizer.sourceObject.GetComponent<MasterAudio>();
                        var srcDgsc = _organizer.sourceObject.GetComponent<DynamicSoundGroupCreator>();

                        // ReSharper disable once InconsistentNaming
                        var isSourceMA = srcMa != null;
                        // ReSharper disable once InconsistentNaming
                        var isSourceDGSC = srcDgsc != null;

                        List<CustomEvent> sourceEvents = null;

                        if (isSourceMA) {
                            sourceEvents = srcMa.customEvents;
                        } else if (isSourceDGSC) {
                            sourceEvents = srcDgsc.customEventsToCreate;
                        }

                        var srcIndex = sources.IndexOf(_organizer.sourceObject);
                        if (srcIndex < 0) {
                            srcIndex = 0;
                        }

                        DTGUIHelper.StartGroupHeader();

                        var newIndex = EditorGUILayout.Popup("Source Object", srcIndex, sourceNames.ToArray());
                        if (newIndex != srcIndex) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Source Object");
                        }
                        EditorGUILayout.EndVertical();

                        var newSource = sources[newIndex];
                        if (!hasRescannedEvents && newSource != _organizer.sourceObject || _organizer.selectedSourceCustomEvents.Count == 0) {
                            if (RescanSourceEvents(sourceEvents)) {
                                hasRescannedEvents = true;
                            }
                        }
                        _organizer.sourceObject = newSource;

                        if (!hasRescannedEvents && _organizer.selectedSourceCustomEvents.Count != sourceEvents.Count) {
                            if (RescanSourceEvents(sourceEvents)) {
                                hasRescannedEvents = true;
                            }
                        }

                        if (_organizer.sourceObject != null) {
                            if (_organizer.selectedSourceCustomEvents.Count > 0) {
                                DTGUIHelper.ShowLargeBarAlert("Check Custom Events to Import below and click 'Import'");
                            } else {
                                DTGUIHelper.ShowRedError("Source Object has no Custom Events to import.");
                            }

                            EditorGUI.indentLevel = 0;

                            foreach (var aEvent in _organizer.selectedSourceCustomEvents) {
                                if (!hasRescannedEvents && aEvent.Event == null) {
                                    shouldRescanEvents = true;
                                    continue;
                                }

                                var newSel = EditorGUILayout.Toggle(aEvent.Event.EventName, aEvent.IsSelected);
                                if (newSel == aEvent.IsSelected) {
                                    continue;
                                }

                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "toggle Custom Event selection");
                                aEvent.IsSelected = newSel;
                            }
                        }

                        if (!hasRescannedEvents && shouldRescanEvents) {
                            RescanDestinationEvents();
                            // ReSharper disable once RedundantAssignment
                            hasRescannedEvents = true;
                        }

                        if (_organizer.selectedSourceCustomEvents.Count > 0) {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            GUI.contentColor = DTGUIHelper.BrightButtonColor;
                            if (GUILayout.Button(new GUIContent("Import", "Import Selected Events"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                ImportSelectedEvents();
                                RescanDestinationEvents();
                            }

                            GUI.contentColor = DTGUIHelper.BrightButtonColor;
                            GUILayout.Space(10);
                            if (GUILayout.Button(new GUIContent("Check All", "Check all Events above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                CheckUncheckAllSourceEvents(true);
                            }
                            GUILayout.Space(10);
                            if (GUILayout.Button(new GUIContent("Uncheck All", "Uncheck all Events above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                CheckUncheckAllSourceEvents(false);
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                    }
                    break;
                case SoundGroupOrganizer.TransferMode.Export:
                    if (_organizer.customEvents.Count == 0) {
                        DTGUIHelper.ShowRedError("You have no Custom Events to export. Import or create some first.");
                    } else if (sources.Count == 0) {
                        DTGUIHelper.ShowRedError("You have no Master Audio or Dynamic Sound Group Creator prefabs in this Scene to export to.");
                    } else {
                        var destIndex = sources.IndexOf(_organizer.destObject);
                        if (destIndex < 0) {
                            destIndex = 0;
                        }

                        DTGUIHelper.StartGroupHeader();

                        var newIndex = EditorGUILayout.Popup("Destination Object", destIndex, sourceNames.ToArray());
                        if (newIndex != destIndex) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Destination Object");
                        }
                        EditorGUILayout.EndVertical();

                        var newDest = sources[newIndex];

                        _organizer.destObject = newDest;

                        if (_organizer.destObject != null) {
                            if (_organizer.selectedDestCustomEvents.Count == 0) {
                                DTGUIHelper.ShowRedError("You have no Custom Events to export");
                            } else {
                                DTGUIHelper.ShowLargeBarAlert("Check Custom Events to export (same as Custom Events below) and click 'Export'");
                            }

                            EditorGUI.indentLevel = 0;

                            if (_organizer.selectedDestCustomEvents.Count != _organizer.customEvents.Count) {
                                shouldRescanEvents = true;
                            }
                            if (!hasRescannedEvents && shouldRescanEvents) {
                                RescanDestinationEvents();
                                // ReSharper disable once RedundantAssignment
                                hasRescannedEvents = true;
                            }

                            foreach (var aEvent in _organizer.selectedDestCustomEvents) {
                                var newSel = EditorGUILayout.Toggle(aEvent.Event.EventName, aEvent.IsSelected);
                                if (newSel == aEvent.IsSelected) {
                                    continue;
                                }

                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "toggle Custom Event selection");
                                aEvent.IsSelected = newSel;
                            }
                        }

                        if (_organizer.selectedDestCustomEvents.Count > 0) {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            GUI.contentColor = DTGUIHelper.BrightButtonColor;
                            if (GUILayout.Button(new GUIContent("Export", "Export Selected Custom Events"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                ExportSelectedEvents();
                            }

                            GUI.contentColor = DTGUIHelper.BrightButtonColor;
                            GUILayout.Space(10);
                            if (GUILayout.Button(new GUIContent("Check All", "Check all Custom Events above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                CheckUncheckAllDestEvents(true);
                            }
                            GUILayout.Space(10);
                            if (GUILayout.Button(new GUIContent("Uncheck All", "Uncheck all Custom Events above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                CheckUncheckAllDestEvents(false);
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                    }

                    break;
            }
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        GUI.contentColor = Color.white;
        var sliderIndicatorChars = 6;
        var sliderWidth = 40;

        if (MasterAudio.UseDbScaleForVolume) {
            sliderIndicatorChars = 9;
            sliderWidth = 56;
        }

        EditorGUI.indentLevel = 0;

        GUI.backgroundColor = DTGUIHelper.ActiveHeaderColor;


        if (_organizer.itemType == SoundGroupOrganizer.MAItemType.SoundGroups) {
            // ReSharper disable once ConvertToConstant.Local
            var text = "Group Control";
            GUILayout.BeginHorizontal();
            text = "<b><size=11>" + text + "</size></b>";
            GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f));
            EditorGUILayout.EndHorizontal();

            DTGUIHelper.BeginGroupedControls();
            var newDragMode = (MasterAudio.DragGroupMode)EditorGUILayout.EnumPopup("Bulk Creation Mode", _organizer.curDragGroupMode);
            if (newDragMode != _organizer.curDragGroupMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Bulk Creation Mode");
                _organizer.curDragGroupMode = newDragMode;
            }

            var bulkMode = DTGUIHelper.GetRestrictedAudioLocation("Variation Create Mode", _organizer.bulkVariationMode);
            if (bulkMode != _organizer.bulkVariationMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Variation Mode");
                _organizer.bulkVariationMode = bulkMode;
            }

            if (_groups.Count > 0) {
                var newUseTextGroupFilter = EditorGUILayout.Toggle("Use Text Group Filter", _organizer.useTextGroupFilter);
                if (newUseTextGroupFilter != _organizer.useTextGroupFilter) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "toggle Use Text Group Filter");
                    _organizer.useTextGroupFilter = newUseTextGroupFilter;
                }

                if (_organizer.useTextGroupFilter) {
                    EditorGUI.indentLevel = 1;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.Label("Text Group Filter", GUILayout.Width(140));
                    var newTextFilter = GUILayout.TextField(_organizer.textGroupFilter, GUILayout.Width(180));
                    if (newTextFilter != _organizer.textGroupFilter) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Text Group Filter");
                        _organizer.textGroupFilter = newTextFilter;
                    }
                    GUILayout.Space(10);
                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(70))) {
                        _organizer.textGroupFilter = string.Empty;
                    }
                    GUI.contentColor = Color.white;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Separator();
                }
            }

            EditorGUI.indentLevel = 0;

            // create groups start
            EditorGUILayout.BeginVertical();
            var aEvent = Event.current;

            var groupAdded = false;

            if (isInProjectView) {
                DTGUIHelper.ShowLargeBarAlert("You are in Project View and cannot create or delete Groups.");
                DTGUIHelper.ShowRedError("Create this prefab With Master Audio Manager. Do not drag into Scene!");
            } else {
                //DTGUIHelper.ShowRedError("Make sure this prefab is not in a gameplay Scene. Use a special Sandbox Scene.");
                GUI.color = DTGUIHelper.DragAreaColor;

                var dragAreaGroup = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
                GUI.Box(dragAreaGroup, "Drag Audio clips here to create groups!");

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
                                if (_organizer.curDragGroupMode == MasterAudio.DragGroupMode.OneGroupPerClip) {
                                    CreateGroup(aClip);
                                } else {
                                    if (groupInfo == null) { // one group with variations
                                        groupInfo = CreateGroup(aClip);
                                    } else {
                                        CreateVariation(groupInfo, aClip);
                                    }
                                }
                                groupAdded = true;

                                _isDirty = true;
                            }
                        }
                        Event.current.Use();
                        break;
                }
            }

            EditorGUILayout.EndVertical();
            // create groups end

            if (groupAdded) {
                RescanDestinationGroups();
            }

            var filteredGroups = new List<DynamicSoundGroup>();
            filteredGroups.AddRange(_groups);

            if (_organizer.useTextGroupFilter) {
                if (!string.IsNullOrEmpty(_organizer.textGroupFilter)) {
                    filteredGroups.RemoveAll(delegate(DynamicSoundGroup obj) {
                        return !obj.transform.name.ToLower().Contains(_organizer.textGroupFilter.ToLower());
                    });
                }
            }

            if (_groups.Count == 0) {
                DTGUIHelper.ShowLargeBarAlert("You currently have no Sound Groups created.");
            } else {
                var groupsFiltered = _groups.Count - filteredGroups.Count;
                if (groupsFiltered > 0) {
                    DTGUIHelper.ShowLargeBarAlert(string.Format("{0}/{1} Group(s) filtered out.", groupsFiltered, _groups.Count));
                }
            }

            int? indexToDelete = null;

            GUI.color = Color.white;

            filteredGroups.Sort(delegate(DynamicSoundGroup x, DynamicSoundGroup y) {
                return x.name.CompareTo(y.name);
            });

            DTGUIHelper.ResetColors();

            for (var i = 0; i < filteredGroups.Count; i++) {
                var aGroup = filteredGroups[i];

                var groupDirty = false;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(aGroup.name, GUILayout.Width(150));

                GUILayout.FlexibleSpace();

                GUI.contentColor = Color.white;
                GUI.color = DTGUIHelper.BrightButtonColor;

                GUI.color = Color.white;

                GUI.contentColor = DTGUIHelper.BrightTextColor;
                GUILayout.TextField(DTGUIHelper.DisplayVolumeNumber(aGroup.groupMasterVolume, sliderIndicatorChars), sliderIndicatorChars, EditorStyles.miniLabel, GUILayout.Width(sliderWidth));

                var newVol = DTGUIHelper.DisplayVolumeField(aGroup.groupMasterVolume, DTGUIHelper.VolumeFieldType.DynamicMixerGroup, MasterAudio.MixerWidthMode.Normal);
                if (newVol != aGroup.groupMasterVolume) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref groupDirty, aGroup, "change Group Volume");
                    aGroup.groupMasterVolume = newVol;
                }

                GUI.contentColor = Color.white;

                var buttonPressed = DTGUIHelper.AddDynamicGroupButtons(_organizer);
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
                        StopPreviewer();
                        break;
                }

                if (groupDirty) {
                    EditorUtility.SetDirty(aGroup);
                }
            }

            if (indexToDelete.HasValue) {
                AudioUndoHelper.DestroyForUndo(filteredGroups[indexToDelete.Value].gameObject);
            }

            if (filteredGroups.Count > 0) {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(6);

                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                if (GUILayout.Button(new GUIContent("Max Group Volumes", "Reset all group volumes to full"), EditorStyles.toolbarButton, GUILayout.Width(120))) {
                    AudioUndoHelper.RecordObjectsForUndo(filteredGroups.ToArray(), "Max Group Volumes");

                    foreach (var aGroup in filteredGroups) {
                        aGroup.groupMasterVolume = 1f;
                    }
                }
                GUI.contentColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            DTGUIHelper.EndGroupedControls();
        } else {
            // custom events
            EditorGUI.indentLevel = 0;

            // ReSharper disable once ConvertToConstant.Local
            var text = "Custom Event Control";
            GUILayout.BeginHorizontal();
            text = "<b><size=11>" + text + "</size></b>";
            GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f));
            EditorGUILayout.EndHorizontal();

            DTGUIHelper.BeginGroupedControls();

            var newEvent = EditorGUILayout.TextField("New Event Name", _organizer.newEventName);
            if (newEvent != _organizer.newEventName) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change New Event Name");
                _organizer.newEventName = newEvent;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            if (GUILayout.Button("Create New Event", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                CreateCustomEvent(_organizer.newEventName);
            }
            GUILayout.Space(10);
            GUI.contentColor = DTGUIHelper.BrightButtonColor;

            var hasExpanded = false;
            foreach (var t in _organizer.customEvents) {
                if (t.eventExpanded) {
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

            if (_organizer.customEvents.Count == 0) {
                DTGUIHelper.ShowLargeBarAlert("You currently have no Custom Events.");
            }

            EditorGUILayout.Separator();

            int? customEventToDelete = null;
            int? eventToRename = null;

            DTGUIHelper.ResetColors();

            for (var i = 0; i < _organizer.customEvents.Count; i++) {
                EditorGUI.indentLevel = 1;
                var anEvent = _organizer.customEvents[i];

                DTGUIHelper.StartGroupHeader();

                EditorGUILayout.BeginHorizontal();
                var exp = DTGUIHelper.Foldout(anEvent.eventExpanded, anEvent.EventName);
                if (exp != anEvent.eventExpanded) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "toggle expand Custom Event");
                    anEvent.eventExpanded = exp;
                }

                GUILayout.FlexibleSpace();
                var newName = GUILayout.TextField(anEvent.ProspectiveName, GUILayout.Width(170));
                if (newName != anEvent.ProspectiveName) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Proposed Event Name");
                    anEvent.ProspectiveName = newName;
                }

                var buttonPressed = DTGUIHelper.AddDeleteIcon(true, "Custom Event");

                switch (buttonPressed) {
                    case DTGUIHelper.DTFunctionButtons.Remove:
                        customEventToDelete = i;
                        break;
                    case DTGUIHelper.DTFunctionButtons.Rename:
                        eventToRename = i;
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
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Send To Receivers");
                    anEvent.eventReceiveMode = rcvMode;
                }

                if (rcvMode == MasterAudio.CustomEventReceiveMode.WhenDistanceLessThan || rcvMode == MasterAudio.CustomEventReceiveMode.WhenDistanceMoreThan) {
                    var newDist = EditorGUILayout.Slider("Distance Threshold", anEvent.distanceThreshold, 0f, float.MaxValue);
                    if (newDist != anEvent.distanceThreshold) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "change Distance Threshold");
                        anEvent.distanceThreshold = newDist;
                    }
                }

                EditorGUILayout.EndVertical();
                DTGUIHelper.AddSpaceForNonU5(2);
            }

            if (customEventToDelete.HasValue) {
                _organizer.customEvents.RemoveAt(customEventToDelete.Value);
            }
            if (eventToRename.HasValue) {
                RenameEvent(_organizer.customEvents[eventToRename.Value]);
            }

            DTGUIHelper.EndGroupedControls();
        }

        if (GUI.changed || _isDirty) {
            EditorUtility.SetDirty(target);
        }

        //DrawDefaultInspector();
    }

    private void RescanDestinationGroups() {
        _organizer.selectedDestSoundGroups.Clear();

        for (var i = 0; i < _organizer.transform.childCount; i++) {
            var aGroup = _organizer.transform.GetChild(i);
            _organizer.selectedDestSoundGroups.Add(
                new SoundGroupOrganizer.SoundGroupSelection(aGroup.gameObject, false));
        }
    }

    private void RescanDestinationEvents() {
        _organizer.selectedDestCustomEvents.Clear();

        foreach (var aEvent in _organizer.customEvents) {
            _organizer.selectedDestCustomEvents.Add(
                new SoundGroupOrganizer.CustomEventSelection(aEvent, false));
        }
    }

    private bool RescanSourceGroups() {
        if (_organizer.sourceObject == null) {
            return false;
        }

        _organizer.selectedSourceSoundGroups.Clear();
        for (var i = 0; i < _organizer.sourceObject.transform.childCount; i++) {
            var aGroup = _organizer.sourceObject.transform.GetChild(i);
            _organizer.selectedSourceSoundGroups.Add(
                new SoundGroupOrganizer.SoundGroupSelection(aGroup.gameObject, false));
        }

        _isDirty = true;
        return true;
    }

    private bool RescanSourceEvents(List<CustomEvent> sourceEvents) {
        if (_organizer.sourceObject == null) {
            return false;
        }

        _organizer.selectedSourceCustomEvents.Clear();

        foreach (var anEvent in sourceEvents) {
            _organizer.selectedSourceCustomEvents.Add(
                new SoundGroupOrganizer.CustomEventSelection(anEvent, false));
        }

        _isDirty = true;
        return true;
    }

    private void CheckUncheckAllDestGroups(bool shouldCheck) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "check/uncheck All destination Groups");

        foreach (var t in _organizer.selectedDestSoundGroups) {
            t.IsSelected = shouldCheck;
        }
    }

    private void CheckUncheckAllDestEvents(bool shouldCheck) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "check/uncheck All destination Custom Events");

        foreach (var t in _organizer.selectedDestCustomEvents) {
            t.IsSelected = shouldCheck;
        }
    }

    private void CheckUncheckAllSourceGroups(bool shouldCheck) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "check/uncheck All source Groups");

        foreach (var t in _organizer.selectedSourceSoundGroups) {
            t.IsSelected = shouldCheck;
        }
    }

    private void CheckUncheckAllSourceEvents(bool shouldCheck) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "check/uncheck All source Custom Events");

        foreach (var t in _organizer.selectedSourceCustomEvents) {
            t.IsSelected = shouldCheck;
        }
    }

    private Transform CreateGroup(AudioClip aClip) {
        if (_organizer.dynGroupTemplate == null) {
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

        var spawnedGroup = (GameObject)Instantiate(_organizer.dynGroupTemplate, _organizer.transform.position, Quaternion.identity);
        spawnedGroup.name = groupName;

        AudioUndoHelper.CreateObjectForUndo(spawnedGroup, "create Dynamic Group");
        spawnedGroup.transform.parent = _organizer.transform;

        CreateVariation(spawnedGroup.transform, aClip);

        return spawnedGroup.transform;
    }

    private void CreateVariation(Transform aGroup, AudioClip aClip) {
        if (_organizer.dynVariationTemplate == null) {
            DTGUIHelper.ShowAlert("Your 'Variation Template' field is empty, please assign it in debug mode. Drag the 'DynamicGroupVariation' prefab from MasterAudio/Sources/Prefabs into that field, then switch back to normal mode.");
            return;
        }

        var resourceFileName = string.Empty;
        var useLocalization = false;
        if (_organizer.bulkVariationMode == MasterAudio.AudioLocation.ResourceFile) {
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

        var spawnedVar = (GameObject)Instantiate(_organizer.dynVariationTemplate, _organizer.transform.position, Quaternion.identity);
        spawnedVar.name = clipName;

        spawnedVar.transform.parent = aGroup;

        var dynamicVar = spawnedVar.GetComponent<DynamicGroupVariation>();

        if (_organizer.bulkVariationMode == MasterAudio.AudioLocation.ResourceFile) {
            dynamicVar.audLocation = MasterAudio.AudioLocation.ResourceFile;
            dynamicVar.resourceFileName = resourceFileName;
            dynamicVar.useLocalization = useLocalization;
        } else {
            dynamicVar.VarAudio.clip = aClip;
        }
    }

    private List<DynamicSoundGroup> ScanForGroups() {
        var groups = new List<DynamicSoundGroup>();

        for (var i = 0; i < _organizer.transform.childCount; i++) {
            var aChild = _organizer.transform.GetChild(i);

            var grp = aChild.GetComponent<DynamicSoundGroup>();
            if (grp == null) {
                continue;
            }

            grp.groupVariations = VariationsForGroup(aChild.transform);

            groups.Add(grp);
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

    private void PreviewGroup(DynamicSoundGroup aGroup) {
        var rndIndex = Random.Range(0, aGroup.groupVariations.Count);
        var rndVar = aGroup.groupVariations[rndIndex];

        switch (rndVar.audLocation) {
            case MasterAudio.AudioLocation.ResourceFile:
                StopPreviewer();
                var fileName = AudioResourceOptimizer.GetLocalizedDynamicSoundGroupFileName(_organizer.previewLanguage, rndVar.useLocalization, rndVar.resourceFileName);

                var clip = Resources.Load(fileName) as AudioClip;
                if (clip != null) {
                    GetPreviewer().PlayOneShot(clip, rndVar.VarAudio.volume);
                } else {
                    DTGUIHelper.ShowAlert("Could not find Resource file: " + fileName);
                }
                break;
            case MasterAudio.AudioLocation.Clip:
                GetPreviewer().PlayOneShot(rndVar.VarAudio.clip, rndVar.VarAudio.volume);
                break;
            case MasterAudio.AudioLocation.FileOnInternet:
                if (!string.IsNullOrEmpty(rndVar.internetFileUrl)) {
                    Application.OpenURL(rndVar.internetFileUrl);
                }
                break;
        }
    }

    private void ImportSelectedGroups() {
        if (_organizer.sourceObject == null) {
            return;
        }

        var imported = 0;
        var skipped = 0;

        foreach (var item in _organizer.selectedSourceSoundGroups) {
            if (!item.IsSelected) {
                continue;
            }

            var grp = item.Go;
            var dynGrp = grp.GetComponent<DynamicSoundGroup>();
            var maGrp = grp.GetComponent<MasterAudioGroup>();

            var wasSkipped = false;

            foreach (var t in _groups) {
                if (t.name != grp.name) {
                    continue;
                }

                Debug.LogError("Group '" + grp.name + "' skipped because there's already a Group with that name in your Organizer. If you wish to import the Group, please delete the one in the Organizer first.");
                skipped++;
                wasSkipped = true;
                break;
            }

            if (wasSkipped) {
                continue;
            }

            if (dynGrp != null) {
                ImportDynamicGroup(dynGrp);
                imported++;
            } else if (maGrp != null) {
                ImportMAGroup(maGrp);
                imported++;
            } else {
                Debug.LogError("Invalid Group '" + grp.name + "'. It's set up wrong. Contact DarkTonic for assistance.");
            }
        }

        var summaryText = imported + " Group(s) imported.";
        if (skipped == 0) {
            Debug.Log(summaryText);
        }
    }

    private void ImportSelectedEvents() {
        if (_organizer.sourceObject == null) {
            return;
        }

        var imported = 0;
        var skipped = 0;

        foreach (var item in _organizer.selectedSourceCustomEvents) {
            if (!item.IsSelected) {
                continue;
            }

            var evt = item.Event;

            var wasSkipped = false;

            foreach (var t in _organizer.customEvents) {
                if (t.EventName != evt.EventName) {
                    continue;
                }

                Debug.LogError("Custom Event '" + evt.EventName + "' skipped because there's already a Custom Event with that name in your Organizer. If you wish to import the Custom Event, please delete the one in the Organizer first.");
                skipped++;
                wasSkipped = true;
                break;
            }

            if (wasSkipped) {
                continue;
            }

            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "import Organizer Custom Event(s)");

            _organizer.customEvents.Add(new CustomEvent(item.Event.EventName) {
                distanceThreshold = item.Event.distanceThreshold,
                eventExpanded = item.Event.eventExpanded,
                eventReceiveMode = item.Event.eventReceiveMode,
                ProspectiveName = item.Event.ProspectiveName
            });
            imported++;
        }

        var summaryText = imported + " Custom Event(s) imported.";
        if (skipped == 0) {
            Debug.Log(summaryText);
        }
    }

    private GameObject CreateBlankGroup(string grpName) {
        var spawnedGroup = (GameObject)Instantiate(_organizer.dynGroupTemplate, _organizer.transform.position, Quaternion.identity);
        spawnedGroup.name = grpName;

        AudioUndoHelper.CreateObjectForUndo(spawnedGroup, "import Organizer Group(s)");
        spawnedGroup.transform.parent = _organizer.transform;
        return spawnedGroup;
    }

    private void ImportDynamicGroup(DynamicSoundGroup aGroup) {
        var newGroup = CreateBlankGroup(aGroup.name);

        var groupTrans = newGroup.transform;

        foreach (var t in aGroup.groupVariations) {
            var aVariation = t;

            var newVariation = (GameObject)Instantiate(_organizer.dynVariationTemplate.gameObject, groupTrans.position, Quaternion.identity);
            newVariation.transform.parent = groupTrans;

            var variation = newVariation.GetComponent<DynamicGroupVariation>();

            var clipName = aVariation.name;

            var aVarAudio = aVariation.GetComponent<AudioSource>();

            UnityEditorInternal.ComponentUtility.CopyComponent(aVarAudio);
            // ReSharper disable once ArrangeStaticMemberQualifier
            GameObject.DestroyImmediate(variation.VarAudio);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(variation.gameObject);
            UnityEditorInternal.ComponentUtility.MoveComponentUp(variation.VarAudio);

            switch (aVariation.audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    var clip = aVarAudio.clip;
                    if (clip == null) {
                        continue;
                    }
                    variation.VarAudio.clip = clip;
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    variation.resourceFileName = aVariation.resourceFileName;
                    variation.useLocalization = aVariation.useLocalization;
                    break;
                case MasterAudio.AudioLocation.FileOnInternet:
                    variation.internetFileUrl = aVariation.internetFileUrl;
                    break;
            }

            variation.audLocation = aVariation.audLocation;
            variation.VarAudio.dopplerLevel = aVarAudio.dopplerLevel;
            variation.VarAudio.maxDistance = aVarAudio.maxDistance;
            variation.VarAudio.minDistance = aVarAudio.minDistance;
            variation.VarAudio.bypassEffects = aVarAudio.bypassEffects;
            variation.VarAudio.ignoreListenerVolume = aVarAudio.ignoreListenerVolume;
            variation.VarAudio.mute = aVarAudio.mute;

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            variation.VarAudio.pan = aVarAudio.pan;
#else
            variation.VarAudio.panStereo = aVarAudio.panStereo;
#endif

            variation.VarAudio.rolloffMode = aVarAudio.rolloffMode;
            variation.VarAudio.spread = aVarAudio.spread;

            variation.VarAudio.loop = aVarAudio.loop;
            variation.VarAudio.pitch = aVarAudio.pitch;
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
                Destroy(variation.LowPassFilter);
            }
            if (variation.HighPassFilter != null && !variation.HighPassFilter.enabled) {
                Destroy(variation.HighPassFilter);
            }
            if (variation.DistortionFilter != null && !variation.DistortionFilter.enabled) {
                Destroy(variation.DistortionFilter);
            }
            if (variation.ChorusFilter != null && !variation.ChorusFilter.enabled) {
                Destroy(variation.ChorusFilter);
            }
            if (variation.EchoFilter != null && !variation.EchoFilter.enabled) {
                Destroy(variation.EchoFilter);
            }
            if (variation.ReverbFilter != null && !variation.ReverbFilter.enabled) {
                Destroy(variation.ReverbFilter);
            }
        }
        // added to Hierarchy!

        // populate sounds for playing!
        var groupScript = newGroup.GetComponent<DynamicSoundGroup>();
        // populate other properties.
        groupScript.retriggerPercentage = aGroup.retriggerPercentage;
        groupScript.groupMasterVolume = aGroup.groupMasterVolume;
        groupScript.limitMode = aGroup.limitMode;
        groupScript.limitPerXFrames = aGroup.limitPerXFrames;
        groupScript.minimumTimeBetween = aGroup.minimumTimeBetween;
        groupScript.limitPolyphony = aGroup.limitPolyphony;
        groupScript.voiceLimitCount = aGroup.voiceLimitCount;
        groupScript.curVariationSequence = aGroup.curVariationSequence;
        groupScript.useInactivePeriodPoolRefill = aGroup.useInactivePeriodPoolRefill;
        groupScript.inactivePeriodSeconds = aGroup.inactivePeriodSeconds;
        groupScript.curVariationMode = aGroup.curVariationMode;
        groupScript.useDialogFadeOut = aGroup.useDialogFadeOut;
        groupScript.dialogFadeOutTime = aGroup.dialogFadeOutTime;

        groupScript.chainLoopDelayMin = aGroup.chainLoopDelayMin;
        groupScript.chainLoopDelayMax = aGroup.chainLoopDelayMax;
        groupScript.chainLoopMode = aGroup.chainLoopMode;
        groupScript.chainLoopNumLoops = aGroup.chainLoopNumLoops;

        groupScript.childGroupMode = aGroup.childGroupMode;
        groupScript.childSoundGroups = aGroup.childSoundGroups;

#if UNITY_5
        groupScript.spatialBlendType = aGroup.spatialBlendType;
        groupScript.spatialBlend = aGroup.spatialBlend;
#endif

        groupScript.targetDespawnedBehavior = aGroup.targetDespawnedBehavior;
        groupScript.despawnFadeTime = aGroup.despawnFadeTime;

        groupScript.isUsingOcclusion = aGroup.isUsingOcclusion;

        groupScript.resourceClipsAllLoadAsync = aGroup.resourceClipsAllLoadAsync;
        groupScript.logSound = aGroup.logSound;
        groupScript.alwaysHighestPriority = aGroup.alwaysHighestPriority;

		var dyn = aGroup.GetComponentInParent<DynamicSoundGroupCreator>();
		if (aGroup.busIndex > 0) {
			var srcBus = dyn.groupBuses[aGroup.busIndex - DynamicSoundGroupCreator.HardCodedBusOptions];
			if (srcBus.isExisting) {
				groupScript.isExistingBus = true;
			}
			groupScript.busName = srcBus.busName;
		}

		groupScript.isCopiedFromDGSC = true;
	}

    // ReSharper disable once InconsistentNaming
    private void ImportMAGroup(MasterAudioGroup aGroup) {
        var newGroup = CreateBlankGroup(aGroup.name);

        var groupTrans = newGroup.transform;

        foreach (var t in aGroup.groupVariations) {
            var aVariation = t;

            var newVariation = (GameObject)Instantiate(_organizer.dynVariationTemplate.gameObject, groupTrans.position, Quaternion.identity);
            newVariation.transform.parent = groupTrans;

            var variation = newVariation.GetComponent<DynamicGroupVariation>();

            var clipName = aVariation.name;

            var aVarAudio = aVariation.GetComponent<AudioSource>();

            UnityEditorInternal.ComponentUtility.CopyComponent(aVarAudio);
            // ReSharper disable once ArrangeStaticMemberQualifier
            GameObject.DestroyImmediate(variation.VarAudio);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(variation.gameObject);
            UnityEditorInternal.ComponentUtility.MoveComponentUp(variation.VarAudio);

            switch (aVariation.audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    var clip = aVarAudio.clip;
                    if (clip == null) {
                        continue;
                    }
                    variation.VarAudio.clip = clip;
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    variation.resourceFileName = aVariation.resourceFileName;
                    variation.useLocalization = aVariation.useLocalization;
                    break;
                case MasterAudio.AudioLocation.FileOnInternet:
                    variation.internetFileUrl = aVariation.internetFileUrl;
                    break;
            }

            variation.audLocation = aVariation.audLocation;
            variation.VarAudio.dopplerLevel = aVarAudio.dopplerLevel;
            variation.VarAudio.maxDistance = aVarAudio.maxDistance;
            variation.VarAudio.minDistance = aVarAudio.minDistance;
            variation.VarAudio.bypassEffects = aVarAudio.bypassEffects;
            variation.VarAudio.ignoreListenerVolume = aVarAudio.ignoreListenerVolume;
            variation.VarAudio.mute = aVarAudio.mute;

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            variation.VarAudio.pan = aVarAudio.pan;
#else
            variation.VarAudio.panStereo = aVarAudio.panStereo;
#endif

            variation.VarAudio.rolloffMode = aVarAudio.rolloffMode;
            variation.VarAudio.spread = aVarAudio.spread;

            variation.VarAudio.loop = aVarAudio.loop;
            variation.VarAudio.pitch = aVarAudio.pitch;
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
                Destroy(variation.LowPassFilter);
            }
            if (variation.HighPassFilter != null && !variation.HighPassFilter.enabled) {
                Destroy(variation.HighPassFilter);
            }
            if (variation.DistortionFilter != null && !variation.DistortionFilter.enabled) {
                Destroy(variation.DistortionFilter);
            }
            if (variation.ChorusFilter != null && !variation.ChorusFilter.enabled) {
                Destroy(variation.ChorusFilter);
            }
            if (variation.EchoFilter != null && !variation.EchoFilter.enabled) {
                Destroy(variation.EchoFilter);
            }
            if (variation.ReverbFilter != null && !variation.ReverbFilter.enabled) {
                Destroy(variation.ReverbFilter);
            }
        }
        // added to Hierarchy!

        // populate sounds for playing!
        var groupScript = newGroup.GetComponent<DynamicSoundGroup>();
        // populate other properties.
        groupScript.retriggerPercentage = aGroup.retriggerPercentage;
        groupScript.groupMasterVolume = aGroup.groupMasterVolume;
        groupScript.limitMode = aGroup.limitMode;
        groupScript.limitPerXFrames = aGroup.limitPerXFrames;
        groupScript.minimumTimeBetween = aGroup.minimumTimeBetween;
        groupScript.limitPolyphony = aGroup.limitPolyphony;
        groupScript.voiceLimitCount = aGroup.voiceLimitCount;
        groupScript.curVariationSequence = aGroup.curVariationSequence;
        groupScript.useInactivePeriodPoolRefill = aGroup.useInactivePeriodPoolRefill;
        groupScript.inactivePeriodSeconds = aGroup.inactivePeriodSeconds;
        groupScript.curVariationMode = aGroup.curVariationMode;
        groupScript.useDialogFadeOut = aGroup.useDialogFadeOut;
        groupScript.dialogFadeOutTime = aGroup.dialogFadeOutTime;

        groupScript.chainLoopDelayMin = aGroup.chainLoopDelayMin;
        groupScript.chainLoopDelayMax = aGroup.chainLoopDelayMax;
        groupScript.chainLoopMode = aGroup.chainLoopMode;
        groupScript.chainLoopNumLoops = aGroup.chainLoopNumLoops;

        groupScript.childGroupMode = aGroup.childGroupMode;
        groupScript.childSoundGroups = aGroup.childSoundGroups;

        groupScript.soundPlayedEventActive = aGroup.soundPlayedEventActive;
        groupScript.soundPlayedCustomEvent = aGroup.soundPlayedCustomEvent;

#if UNITY_5
        groupScript.spatialBlendType = aGroup.spatialBlendType;
        groupScript.spatialBlend = aGroup.spatialBlend;
#endif

        groupScript.targetDespawnedBehavior = aGroup.targetDespawnedBehavior;
        groupScript.despawnFadeTime = aGroup.despawnFadeTime;

        groupScript.isUsingOcclusion = aGroup.isUsingOcclusion;

        groupScript.resourceClipsAllLoadAsync = aGroup.resourceClipsAllLoadAsync;
        groupScript.logSound = aGroup.logSound;
        groupScript.alwaysHighestPriority = aGroup.alwaysHighestPriority;

		var dyn = aGroup.GetComponentInParent<MasterAudio>();
		if (aGroup.busIndex > 0) {
			groupScript.busName = dyn.groupBuses[aGroup.busIndex - MasterAudio.HardCodedBusOptions].busName;
		}
    }

    // ReSharper disable once FunctionComplexityOverflow
    private void ExportGroupToDgsc(DynamicSoundGroup aGroup) {
        var newGroup = (GameObject)Instantiate(_organizer.dynGroupTemplate, _organizer.transform.position, Quaternion.identity);
        newGroup.name = aGroup.name;
        newGroup.transform.position = _organizer.destObject.transform.position;

        AudioUndoHelper.CreateObjectForUndo(newGroup, "export Group(s)");
        newGroup.transform.parent = _organizer.destObject.transform;

        var groupTrans = newGroup.transform;

        foreach (var t in aGroup.groupVariations) {
            var aVariation = t;

            var newVariation = (GameObject)Instantiate(_organizer.dynVariationTemplate.gameObject, groupTrans.position, Quaternion.identity);
            newVariation.transform.parent = groupTrans;
            newVariation.transform.position = groupTrans.position;

            var variation = newVariation.GetComponent<DynamicGroupVariation>();

            var clipName = aVariation.name;

            var aVarAudio = aVariation.GetComponent<AudioSource>();

            UnityEditorInternal.ComponentUtility.CopyComponent(aVarAudio);
            // ReSharper disable once ArrangeStaticMemberQualifier
            GameObject.DestroyImmediate(variation.VarAudio);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(variation.gameObject);
            UnityEditorInternal.ComponentUtility.MoveComponentUp(variation.VarAudio);

            switch (aVariation.audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    var clip = aVarAudio.clip;
                    if (clip == null) {
                        continue;
                    }
                    variation.VarAudio.clip = clip;
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    variation.resourceFileName = aVariation.resourceFileName;
                    variation.useLocalization = aVariation.useLocalization;
                    break;
                case MasterAudio.AudioLocation.FileOnInternet:
                    variation.internetFileUrl = aVariation.internetFileUrl;
                    break;
            }

            variation.audLocation = aVariation.audLocation;
            variation.VarAudio.dopplerLevel = aVarAudio.dopplerLevel;
            variation.VarAudio.maxDistance = aVarAudio.maxDistance;
            variation.VarAudio.minDistance = aVarAudio.minDistance;
            variation.VarAudio.bypassEffects = aVarAudio.bypassEffects;
            variation.VarAudio.ignoreListenerVolume = aVarAudio.ignoreListenerVolume;
            variation.VarAudio.mute = aVarAudio.mute;

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            variation.VarAudio.pan = aVarAudio.pan;
#else
            variation.VarAudio.panStereo = aVarAudio.panStereo;
#endif

            variation.VarAudio.rolloffMode = aVarAudio.rolloffMode;
            variation.VarAudio.spread = aVarAudio.spread;

            variation.VarAudio.loop = aVarAudio.loop;
            variation.VarAudio.pitch = aVarAudio.pitch;
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
                Destroy(variation.LowPassFilter);
            }
            if (variation.HighPassFilter != null && !variation.HighPassFilter.enabled) {
                Destroy(variation.HighPassFilter);
            }
            if (variation.DistortionFilter != null && !variation.DistortionFilter.enabled) {
                Destroy(variation.DistortionFilter);
            }
            if (variation.ChorusFilter != null && !variation.ChorusFilter.enabled) {
                Destroy(variation.ChorusFilter);
            }
            if (variation.EchoFilter != null && !variation.EchoFilter.enabled) {
                Destroy(variation.EchoFilter);
            }
            if (variation.ReverbFilter != null && !variation.ReverbFilter.enabled) {
                Destroy(variation.ReverbFilter);
            }
        }
        // added to Hierarchy!

        // populate sounds for playing!
        var groupScript = newGroup.GetComponent<DynamicSoundGroup>();
        // populate other properties.
        groupScript.retriggerPercentage = aGroup.retriggerPercentage;
        groupScript.groupMasterVolume = aGroup.groupMasterVolume;
        groupScript.limitMode = aGroup.limitMode;
        groupScript.limitPerXFrames = aGroup.limitPerXFrames;
        groupScript.minimumTimeBetween = aGroup.minimumTimeBetween;
        groupScript.limitPolyphony = aGroup.limitPolyphony;
        groupScript.voiceLimitCount = aGroup.voiceLimitCount;
        groupScript.curVariationSequence = aGroup.curVariationSequence;
        groupScript.useInactivePeriodPoolRefill = aGroup.useInactivePeriodPoolRefill;
        groupScript.inactivePeriodSeconds = aGroup.inactivePeriodSeconds;
        groupScript.curVariationMode = aGroup.curVariationMode;
        groupScript.useDialogFadeOut = aGroup.useDialogFadeOut;
        groupScript.dialogFadeOutTime = aGroup.dialogFadeOutTime;

        groupScript.chainLoopDelayMin = aGroup.chainLoopDelayMin;
        groupScript.chainLoopDelayMax = aGroup.chainLoopDelayMax;
        groupScript.chainLoopMode = aGroup.chainLoopMode;
        groupScript.chainLoopNumLoops = aGroup.chainLoopNumLoops;

        groupScript.childGroupMode = aGroup.childGroupMode;
        groupScript.childSoundGroups = aGroup.childSoundGroups;

#if UNITY_5
        groupScript.spatialBlendType = aGroup.spatialBlendType;
        groupScript.spatialBlend = aGroup.spatialBlend;
#endif

        groupScript.targetDespawnedBehavior = aGroup.targetDespawnedBehavior;
        groupScript.despawnFadeTime = aGroup.despawnFadeTime;

        groupScript.isUsingOcclusion = aGroup.isUsingOcclusion;

        groupScript.resourceClipsAllLoadAsync = aGroup.resourceClipsAllLoadAsync;
        groupScript.logSound = aGroup.logSound;
        groupScript.alwaysHighestPriority = aGroup.alwaysHighestPriority;

		var dyn = groupScript.GetComponentInParent<DynamicSoundGroupCreator>();
		if (!string.IsNullOrEmpty(aGroup.busName)) {
			var busIndex = -1;

			var targetBus = dyn.groupBuses.Find(delegate(GroupBus obj) {
				return obj.busName == aGroup.busName;
			});

			if (targetBus != null) {
				busIndex = dyn.groupBuses.IndexOf(targetBus) + DynamicSoundGroupCreator.HardCodedBusOptions;
			}

			if (busIndex < 0) { // didn't find bus.
				if (aGroup.isCopiedFromDGSC) {
					// create bus on DGSC
					dyn.groupBuses.Add(new GroupBus() {
						busName = aGroup.busName,
						isExisting = aGroup.isExistingBus
					});
				} else {
					// create bus on DGSC
					dyn.groupBuses.Add(new GroupBus() {
						busName = aGroup.busName,
						isExisting = true
					});
				}

				targetBus = dyn.groupBuses.Find(delegate(GroupBus obj) {
					return obj.busName == aGroup.busName;
				});
				
				if (targetBus != null) {
					busIndex = dyn.groupBuses.IndexOf(targetBus) + DynamicSoundGroupCreator.HardCodedBusOptions;
				}
			}

			groupScript.busIndex = busIndex;
			groupScript.busName = aGroup.busName;
		}
    }

    private void ExportGroupToMA(DynamicSoundGroup aGroup) {
        var newGroup = (GameObject)Instantiate(_organizer.maGroupTemplate, _organizer.transform.position, Quaternion.identity);
        newGroup.name = aGroup.name;
        newGroup.transform.position = _organizer.destObject.transform.position;

        AudioUndoHelper.CreateObjectForUndo(newGroup, "export Group(s)");
        newGroup.transform.parent = _organizer.destObject.transform;

        var groupTrans = newGroup.transform;

        foreach (var aVariation in aGroup.groupVariations) {
            var newVariation = (GameObject)Instantiate(_organizer.maVariationTemplate.gameObject, groupTrans.position, Quaternion.identity);
            newVariation.transform.parent = groupTrans;
            newVariation.transform.position = groupTrans.position;

            var variation = newVariation.GetComponent<SoundGroupVariation>();

            var clipName = aVariation.name;

            var aVarAudio = aVariation.GetComponent<AudioSource>();

            UnityEditorInternal.ComponentUtility.CopyComponent(aVarAudio);
            // ReSharper disable once ArrangeStaticMemberQualifier
            GameObject.DestroyImmediate(variation.VarAudio);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(variation.gameObject);
            UnityEditorInternal.ComponentUtility.MoveComponentUp(variation.VarAudio);

            switch (aVariation.audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    var clip = aVarAudio.clip;
                    if (clip == null) {
                        continue;
                    }
                    variation.VarAudio.clip = clip;
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    variation.resourceFileName = aVariation.resourceFileName;
                    variation.useLocalization = aVariation.useLocalization;
                    break;
                case MasterAudio.AudioLocation.FileOnInternet:
                    variation.internetFileUrl = aVariation.internetFileUrl;
                    break;
            }

            variation.audLocation = aVariation.audLocation;
            variation.VarAudio.dopplerLevel = aVarAudio.dopplerLevel;
            variation.VarAudio.maxDistance = aVarAudio.maxDistance;
            variation.VarAudio.minDistance = aVarAudio.minDistance;
            variation.VarAudio.bypassEffects = aVarAudio.bypassEffects;
            variation.VarAudio.ignoreListenerVolume = aVarAudio.ignoreListenerVolume;
            variation.VarAudio.mute = aVarAudio.mute;

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            variation.VarAudio.pan = aVarAudio.pan;
#else
            variation.VarAudio.panStereo = aVarAudio.panStereo;
#endif

            variation.VarAudio.rolloffMode = aVarAudio.rolloffMode;
            variation.VarAudio.spread = aVarAudio.spread;

            variation.VarAudio.loop = aVarAudio.loop;
            variation.VarAudio.pitch = aVarAudio.pitch;
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
                Destroy(variation.LowPassFilter);
            }
            if (variation.HighPassFilter != null && !variation.HighPassFilter.enabled) {
                Destroy(variation.HighPassFilter);
            }
            if (variation.DistortionFilter != null && !variation.DistortionFilter.enabled) {
                Destroy(variation.DistortionFilter);
            }
            if (variation.ChorusFilter != null && !variation.ChorusFilter.enabled) {
                Destroy(variation.ChorusFilter);
            }
            if (variation.EchoFilter != null && !variation.EchoFilter.enabled) {
                Destroy(variation.EchoFilter);
            }
            if (variation.ReverbFilter != null && !variation.ReverbFilter.enabled) {
                Destroy(variation.ReverbFilter);
            }
        }
        // added to Hierarchy!

        // populate sounds for playing!
        var groupScript = newGroup.GetComponent<MasterAudioGroup>();
        // populate other properties.
        groupScript.retriggerPercentage = aGroup.retriggerPercentage;
        groupScript.groupMasterVolume = aGroup.groupMasterVolume;
        groupScript.limitMode = aGroup.limitMode;
        groupScript.limitPerXFrames = aGroup.limitPerXFrames;
        groupScript.minimumTimeBetween = aGroup.minimumTimeBetween;
        groupScript.limitPolyphony = aGroup.limitPolyphony;
        groupScript.voiceLimitCount = aGroup.voiceLimitCount;
        groupScript.curVariationSequence = aGroup.curVariationSequence;
        groupScript.useInactivePeriodPoolRefill = aGroup.useInactivePeriodPoolRefill;
        groupScript.inactivePeriodSeconds = aGroup.inactivePeriodSeconds;
        groupScript.curVariationMode = aGroup.curVariationMode;
        groupScript.useDialogFadeOut = aGroup.useDialogFadeOut;
        groupScript.dialogFadeOutTime = aGroup.dialogFadeOutTime;

        groupScript.chainLoopDelayMin = aGroup.chainLoopDelayMin;
        groupScript.chainLoopDelayMax = aGroup.chainLoopDelayMax;
        groupScript.chainLoopMode = aGroup.chainLoopMode;
        groupScript.chainLoopNumLoops = aGroup.chainLoopNumLoops;

        groupScript.childGroupMode = aGroup.childGroupMode;
        groupScript.childSoundGroups = aGroup.childSoundGroups;

        groupScript.soundPlayedEventActive = aGroup.soundPlayedEventActive;
        groupScript.soundPlayedCustomEvent = aGroup.soundPlayedCustomEvent;

#if UNITY_5
        groupScript.spatialBlendType = aGroup.spatialBlendType;
        groupScript.spatialBlend = aGroup.spatialBlend;
#endif

        groupScript.targetDespawnedBehavior = aGroup.targetDespawnedBehavior;
        groupScript.despawnFadeTime = aGroup.despawnFadeTime;

        groupScript.isUsingOcclusion = aGroup.isUsingOcclusion;

        groupScript.resourceClipsAllLoadAsync = aGroup.resourceClipsAllLoadAsync;
        groupScript.logSound = aGroup.logSound;
        groupScript.alwaysHighestPriority = aGroup.alwaysHighestPriority;

		var dyn = groupScript.GetComponentInParent<MasterAudio>();
		if (!string.IsNullOrEmpty(aGroup.busName)) {
			var busIndex = -1;

			var targetBus = dyn.groupBuses.Find(delegate(GroupBus obj) {
				return obj.busName == aGroup.busName;
			});

			if (targetBus != null) {
				busIndex = dyn.groupBuses.IndexOf(targetBus) + MasterAudio.HardCodedBusOptions;
			}

			if (busIndex < 0) { // didn't find bus.
				// create bus on DGSC
				dyn.groupBuses.Add(new GroupBus() {
					busName = aGroup.busName
				});

				targetBus = dyn.groupBuses.Find(delegate(GroupBus obj) {
					return obj.busName == aGroup.busName;
				});
				
				if (targetBus != null) {
					busIndex = dyn.groupBuses.IndexOf(targetBus) + MasterAudio.HardCodedBusOptions;
				}
			}
			
			groupScript.busIndex = busIndex;
		}
	}

    private void ExportSelectedGroups() {
        if (_organizer.destObject == null) {
            return;
        }

        var exported = 0;
        var skipped = 0;

        // ReSharper disable once InconsistentNaming
        var isDestMA = _organizer.destObject.GetComponent<MasterAudio>() != null;
        // ReSharper disable once InconsistentNaming
        var isDestDGSC = _organizer.destObject.GetComponent<DynamicSoundGroupCreator>() != null;

        if (!isDestMA && !isDestDGSC) {
            Debug.LogError("Invalid Destination Object '" + _organizer.destObject.name + "'. It's set up wrong. Aborting Export. Contact DarkTonic for assistance.");
            return;
        }

        foreach (var item in _organizer.selectedDestSoundGroups) {
            if (!item.IsSelected) {
                continue;
            }

            var wasSkipped = false;
            var grp = item.Go.GetComponent<DynamicSoundGroup>();

            if (isDestDGSC) {
                for (var g = 0; g < _organizer.destObject.transform.childCount; g++) {
                    var aGroup = _organizer.destObject.transform.GetChild(g);
                    if (aGroup.name != grp.name) {
                        continue;
                    }

                    Debug.LogError("Group '" + grp.name + "' skipped because there's already a Group with that name in the destination Dynamic Sound Group Creator object. If you wish to export the Group, please delete the one in the DSGC object first.");
                    skipped++;
                    wasSkipped = true;
                }

                if (wasSkipped) {
                    continue;
                }

                ExportGroupToDgsc(grp);
                exported++;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            } else if (isDestMA) {
                for (var g = 0; g < _organizer.destObject.transform.childCount; g++) {
                    var aGroup = _organizer.destObject.transform.GetChild(g);
                    if (aGroup.name != grp.name) {
                        continue;
                    }

                    Debug.LogError("Group '" + grp.name + "' skipped because there's already a Group with that name in the destination Master Audio object. If you wish to export the Group, please delete the one in the MA object first.");
                    skipped++;
                    wasSkipped = true;
                }

                if (wasSkipped) {
                    continue;
                }

                ExportGroupToMA(grp);
                exported++;
            }
        }

        var summaryText = exported + " Group(s) exported.";
        if (skipped == 0) {
            Debug.Log(summaryText);
        }
    }

    private void ExportSelectedEvents() {
        if (_organizer.destObject == null) {
            return;
        }

        var exported = 0;
        var skipped = 0;

        var ma = _organizer.destObject.GetComponent<MasterAudio>();
        var dgsc = _organizer.destObject.GetComponent<DynamicSoundGroupCreator>();

        var isDestMa = ma != null;
        var isDestDgsc = dgsc != null;

        if (!isDestMa && !isDestDgsc) {
            Debug.LogError("Invalid Destination Object '" + _organizer.destObject.name + "'. It's set up wrong. Aborting Export. Contact DarkTonic for assistance.");
            return;
        }

        foreach (var item in _organizer.selectedDestCustomEvents) {
            if (!item.IsSelected) {
                continue;
            }

            var wasSkipped = false;
            var evt = item.Event;

            if (isDestDgsc) {
                foreach (var aEvt in dgsc.customEventsToCreate) {
                    if (aEvt.EventName != evt.EventName) {
                        continue;
                    }

                    Debug.LogError("Group '" + evt.EventName + "' skipped because there's already a Custom Event with that name in the destination Dynamic Sound Group Creator object. If you wish to export the Custom Event, please delete the one in the DSGC object first.");
                    skipped++;
                    wasSkipped = true;
                }

                if (wasSkipped) {
                    continue;
                }

                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, dgsc, "export Custom Event(s)");

                dgsc.customEventsToCreate.Add(new CustomEvent(evt.EventName) {
                    distanceThreshold = evt.distanceThreshold,
                    eventExpanded = evt.eventExpanded,
                    eventReceiveMode = evt.eventReceiveMode,
                    ProspectiveName = evt.EventName
                });

                exported++;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            } else if (isDestMa) {
                foreach (var aEvt in ma.customEvents) {
                    if (aEvt.EventName != evt.EventName) {
                        continue;
                    }

                    Debug.LogError("Custom Event '" + evt.EventName + "' skipped because there's already a Custom Event with that name in the destination Master Audio object. If you wish to export the Custom Event, please delete the one in the MA object first.");
                    skipped++;
                    wasSkipped = true;
                }

                if (wasSkipped) {
                    continue;
                }

                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, ma, "export Custom Event(s)");

                ma.customEvents.Add(new CustomEvent(evt.EventName) {
                    distanceThreshold = evt.distanceThreshold,
                    eventExpanded = evt.eventExpanded,
                    eventReceiveMode = evt.eventReceiveMode,
                    ProspectiveName = evt.EventName
                });

                exported++;
            }
        }

        var summaryText = exported + " Custom Event(s) exported.";
        if (skipped == 0) {
            Debug.Log(summaryText);
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

        UnityEditorInternal.ComponentUtility.CopyComponent(_organizer.maVariationTemplate.GetComponent<AudioSource>());
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(_previewer);

        aud = _previewer.GetComponent<AudioSource>();

        return aud;
    }

    private void ExpandCollapseCustomEvents(bool shouldExpand) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "Expand / Collapse All Custom Events");

        foreach (var t in _organizer.customEvents) {
            t.eventExpanded = shouldExpand;
        }
    }

    private void SortCustomEvents() {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _organizer, "Sort Custom Events Alpha");

        _organizer.customEvents.Sort(delegate(CustomEvent x, CustomEvent y) {
            return String.Compare(x.EventName, y.EventName, StringComparison.Ordinal);
        });
    }

    private void CreateCustomEvent(string newEventName) {
        if (_organizer.customEvents.FindAll(delegate(CustomEvent obj) {
            return obj.EventName == newEventName;
        }).Count > 0) {
            DTGUIHelper.ShowAlert("You already have a custom event named '" + newEventName + "'. Please choose a different name.");
            return;
        }

        _organizer.customEvents.Add(new CustomEvent(newEventName));
    }

    private void RenameEvent(CustomEvent cEvent) {
        var match = _organizer.customEvents.FindAll(delegate(CustomEvent obj) {
            return obj.EventName == cEvent.ProspectiveName;
        });

        if (match.Count > 0) {
            DTGUIHelper.ShowAlert("You already have a Custom Event named '" + cEvent.ProspectiveName + "'. Please choose a different name.");
            return;
        }

        cEvent.EventName = cEvent.ProspectiveName;
    }
}