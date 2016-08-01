using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_4_6 || UNITY_4_7 || UNITY_5
using UnityEngine.UI;
using UnityEngine.EventSystems;
#endif

#if UNITY_5
using UnityEngine.Audio;
#endif

[CustomEditor(typeof(EventSounds))]
[CanEditMultipleObjects]
// ReSharper disable once CheckNamespace
public class AudioEventInspector : Editor {
    private List<string> _groupNames;
    private List<string> _busNames;
    private List<string> _playlistNames;
    private List<string> _playlistControllerNames;
    private List<string> _customEventNames;
    private bool _maInScene;
    private MasterAudio _ma;
    private EventSounds _sounds;
    // ReSharper disable once ConvertToConstant.Local
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    // ReSharper disable once RedundantDefaultMemberInitializer
    private bool _hasMechanim = false;
    private readonly List<bool> _changedList = new List<bool>();
    private bool _isDirty;

    protected virtual void PopulateItemNames(List<string> groups, List<string> buses, List<string> playlists, List<string> events) {
        if (groups == null) {
            groups = new List<string>();
        }
        if (buses == null) {
            buses = new List<string>();
        }
        if (playlists == null) {
            playlists = new List<string>();
        }
        if (events == null) {
            events = new List<string>();
        }

        var creators = FindObjectsOfType(typeof(DynamicSoundGroupCreator)) as DynamicSoundGroupCreator[];
        // ReSharper disable once PossibleNullReferenceException
        foreach (var dsgc in creators) {
            var trans = dsgc.transform;
            for (var i = 0; i < trans.childCount; ++i) {
                var group = trans.GetChild(i).GetComponent<DynamicSoundGroup>();
                if (group != null) {
                    groups.Add(group.name);
                }
            }

            foreach (var bus in dsgc.groupBuses) {
                buses.Add(bus.busName);
            }

            foreach (var playlist in dsgc.musicPlaylists) {
                playlists.Add(playlist.playlistName);
            }

            foreach (var custom in dsgc.customEventsToCreate) {
                events.Add(custom.EventName);
            }
        }

        groups.Sort();
        if (groups.Count > 1) { // "type in" back to index 0 (sort puts it at #1)
            groups.Insert(0, groups[1]);
        }

        buses.Sort();
        if (buses.Count > 1) { // "type in" back to index 0 (sort puts it at #1)
            buses.Insert(0, buses[1]);
        }
        
        playlists.Sort();
        if (playlists.Count > 1) { // "type in" back to index 0 (sort puts it at #1)
            playlists.Insert(0, playlists[1]);
        }

        events.Sort();
        if (events.Count > 1) { // "type in" back to index 0 (sort puts it at #1)
            events.Insert(0, events[1]);
        }
    }

    public override void OnInspectorGUI() {
        MasterAudio.Instance = null;

        _ma = MasterAudio.Instance;
        _maInScene = _ma != null;

        if (_maInScene) {
            DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.LogoTexture);
        }

        _isDirty = false;

        DTGUIHelper.HelpHeader("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/EventSounds.htm");

        _sounds = (EventSounds)target;

#if UNITY_4_6 || UNITY_4_7 || UNITY_5
        var showNewUIEvents = _sounds.unityUIMode == EventSounds.UnityUIVersion.uGUI;
        var hasSlider = _sounds.GetComponent<Slider>() != null;
        var hasButton = _sounds.GetComponent<Button>() != null;
        var hasToggle = _sounds.GetComponent<Toggle>() != null;
        var hasRect = _sounds.GetComponent<RectTransform>() != null;
        var canClick = hasRect;
#else
        const bool showNewUIEvents = false;
        const bool hasSlider = false;
        const bool hasButton = false;
        const bool canClick = false;
        const bool hasToggle = false;
#endif

        if (_maInScene) {
            // ReSharper disable once PossibleNullReferenceException
            _groupNames = _ma.GroupNames;
            _busNames = _ma.BusNames;
            _playlistNames = _ma.PlaylistNames;
            _customEventNames = _ma.CustomEventNames;
        }
        PopulateItemNames(_groupNames, _busNames, _playlistNames, _customEventNames);

        _playlistControllerNames = new List<string> { MasterAudio.DynamicGroupName, MasterAudio.NoGroupName };

        var anim = _sounds.GetComponent<Animator>();
        _hasMechanim = anim != null;

        _changedList.Clear();
        var pcs = FindObjectsOfType(typeof(PlaylistController));
        foreach (var t in pcs) {
            _playlistControllerNames.Add(t.name);
        }

        // populate unused Events for dropdown
        var unusedEventTypes = new List<string>();
        if (!_sounds.useStartSound) {
            unusedEventTypes.Add("Start");
        }
        if (!_sounds.useEnableSound) {
            unusedEventTypes.Add("Enable");
        }
        if (!_sounds.useDisableSound) {
            unusedEventTypes.Add("Disable");
        }
        if (!_sounds.useVisibleSound) {
            unusedEventTypes.Add("Visible");
        }
        if (!_sounds.useInvisibleSound) {
            unusedEventTypes.Add("Invisible");
        }

        if (!_sounds.useCollision2dSound) {
            unusedEventTypes.Add("2D Collision Enter");
        }
        if (!_sounds.useCollisionExit2dSound) {
            unusedEventTypes.Add("2D Collision Exit");
        }
        if (!_sounds.useTriggerEnter2dSound) {
            unusedEventTypes.Add("2D Trigger Enter");
        }
        if (!_sounds.useTriggerExit2dSound) {
            unusedEventTypes.Add("2D Trigger Exit");
        }

        if (!_sounds.useCollisionSound) {
            unusedEventTypes.Add("Collision Enter");
        }
        if (!_sounds.useCollisionExitSound) {
            unusedEventTypes.Add("Collision Exit");
        }
        if (!_sounds.useTriggerEnterSound) {
            unusedEventTypes.Add("Trigger Enter");
        }
        if (!_sounds.useTriggerExitSound) {
            unusedEventTypes.Add("Trigger Exit");
        }
        if (!_sounds.useParticleCollisionSound) {
            unusedEventTypes.Add("Particle Collision");
        }
        if (_sounds.unityUIMode == EventSounds.UnityUIVersion.Legacy) {
            if (!_sounds.useMouseEnterSound) {
                unusedEventTypes.Add("Mouse Enter (Legacy)");
            }
            if (!_sounds.useMouseExitSound) {
                unusedEventTypes.Add("Mouse Exit (Legacy)");
            }
            if (!_sounds.useMouseClickSound) {
                unusedEventTypes.Add("Mouse Down (Legacy)");
            }
            if (!_sounds.useMouseDragSound) {
                unusedEventTypes.Add("Mouse Drag (Legacy)");
            }
            if (!_sounds.useMouseUpSound) {
                unusedEventTypes.Add("Mouse Up (Legacy)");
            }
        }

        // ReSharper disable HeuristicUnreachableCode
#pragma warning disable 162
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (showNewUIEvents) {
            if (hasSlider) {
                if (!_sounds.useUnitySliderChangedSound) {
                    unusedEventTypes.Add("Slider Changed (uGUI)");
                }
            }
            if (hasButton) {
                if (!_sounds.useUnityButtonClickedSound) {
                    unusedEventTypes.Add("Button Click (uGUI)");
                }
            }

            if (hasToggle) {
                if (!_sounds.useUnityToggleSound) {
                    unusedEventTypes.Add("Toggle (uGUI)");
                }
            }

            if (canClick) {
                if (!_sounds.useUnityPointerEnterSound) {
                    unusedEventTypes.Add("Pointer Enter (uGUI)");
                }
                if (!_sounds.useUnityPointerExitSound) {
                    unusedEventTypes.Add("Pointer Exit (uGUI)");
                }
                if (!_sounds.useUnityPointerDownSound) {
                    unusedEventTypes.Add("Pointer Down (uGUI)");
                }
                if (!_sounds.useUnityPointerUpSound) {
                    unusedEventTypes.Add("Pointer Up (uGUI)");
                }
                if (!_sounds.useUnityDragSound) {
                    unusedEventTypes.Add("Drag (uGUI)");
                }
                if (!_sounds.useUnityDropSound) {
                    unusedEventTypes.Add("Drop (uGUI)");
                }
                if (!_sounds.useUnityScrollSound) {
                    unusedEventTypes.Add("Scroll (uGUI)");
                }
                if (!_sounds.useUnityUpdateSelectedSound) {
                    unusedEventTypes.Add("Update Selected (uGUI)");
                }
                if (!_sounds.useUnitySelectSound) {
                    unusedEventTypes.Add("Select (uGUI)");
                }
                if (!_sounds.useUnityDeselectSound) {
                    unusedEventTypes.Add("Deselect (uGUI)");
                }
                if (!_sounds.useUnityMoveSound) {
                    unusedEventTypes.Add("Move (uGUI)");
                }
                if (!_sounds.useUnityInitializePotentialDragSound) {
                    unusedEventTypes.Add("Initialize Potential Drag (uGUI)");
                }
                if (!_sounds.useUnityBeginDragSound) {
                    unusedEventTypes.Add("Begin Drag (uGUI)");
                }
                if (!_sounds.useUnityEndDragSound) {
                    unusedEventTypes.Add("End Drag (uGUI)");
                }
                if (!_sounds.useUnitySubmitSound) {
                    unusedEventTypes.Add("Submit (uGUI)");
                }
                if (!_sounds.useUnityCancelSound) {
                    unusedEventTypes.Add("Cancel (uGUI)");
                }
                if (!_sounds.useUnityToggleSound) {
                    unusedEventTypes.Add("Toggle (uGUI)");
                }
            }
        }
#pragma warning restore 162
        // ReSharper restore HeuristicUnreachableCode

        if (!_sounds.useNguiOnClickSound && _sounds.showNGUI) {
            unusedEventTypes.Add("NGUI Mouse Click");
        }
        if (!_sounds.useNguiMouseDownSound && _sounds.showNGUI) {
            unusedEventTypes.Add("NGUI Mouse Down");
        }
        if (!_sounds.useNguiMouseUpSound && _sounds.showNGUI) {
            unusedEventTypes.Add("NGUI Mouse Up");
        }
        if (!_sounds.useNguiMouseEnterSound && _sounds.showNGUI) {
            unusedEventTypes.Add("NGUI Mouse Enter");
        }
        if (!_sounds.useNguiMouseExitSound && _sounds.showNGUI) {
            unusedEventTypes.Add("NGUI Mouse Exit");
        }
        if (!_sounds.useSpawnedSound && _sounds.showPoolManager) {
            unusedEventTypes.Add("Spawned");
        }
        if (!_sounds.useDespawnedSound && _sounds.showPoolManager) {
            unusedEventTypes.Add("Despawned");
        }


        if (_hasMechanim) {
            unusedEventTypes.Add("Mechanim State Entered");
        }

        unusedEventTypes.Add("Custom Event");

        var newDisable = EditorGUILayout.Toggle("Disable Sounds", _sounds.disableSounds);
        if (newDisable != _sounds.disableSounds) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Disable Sounds");
            _sounds.disableSounds = newDisable;
        }

        if (!_sounds.disableSounds) {
            var newGiz = EditorGUILayout.Toggle("Show 3D Gizmo", _sounds.showGizmo);
            if (newGiz != _sounds.showGizmo) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Show 3D Gizmo");
                _sounds.showGizmo = newGiz;
            }

            var newSpawnMode = (MasterAudio.SoundSpawnLocationMode)EditorGUILayout.EnumPopup("Sound Spawn Mode", _sounds.soundSpawnMode);
            if (newSpawnMode != _sounds.soundSpawnMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Spawn Mode");
                _sounds.soundSpawnMode = newSpawnMode;
            }

            var newUI = (EventSounds.UnityUIVersion)EditorGUILayout.EnumPopup("Unity UI Version", _sounds.unityUIMode);
            if (newUI != _sounds.unityUIMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Unity UI Version");
                _sounds.unityUIMode = newUI;
            }

            var newNGUI = EditorGUILayout.Toggle("NGUI Events", _sounds.showNGUI);
            if (newNGUI != _sounds.showNGUI) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle NGUI Events");
                _sounds.showNGUI = newNGUI;
            }

            var newPM = EditorGUILayout.Toggle("Pooling Events", _sounds.showPoolManager);
            if (newPM != _sounds.showPoolManager) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Pooling Events");
                _sounds.showPoolManager = newPM;
            }

            var newLogMissing = EditorGUILayout.Toggle("Log Missing Events", _sounds.logMissingEvents);
            if (newLogMissing != _sounds.logMissingEvents) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Log Missing Events");
                _sounds.logMissingEvents = newLogMissing;
            }

            EditorGUILayout.BeginHorizontal();
            var newEventIndex = EditorGUILayout.Popup("Event To Activate", -1, unusedEventTypes.ToArray());
            DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/EventSounds.htm#SupportedEvents");

            EditorGUILayout.EndHorizontal();
            if (newEventIndex > -1) {
                var selectedEvent = unusedEventTypes[newEventIndex];
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Active Event");

                switch (selectedEvent) {
                    case "Start":
                        _sounds.useStartSound = true;
                        AddEventIfZero(_sounds.startSound);
                        break;
                    case "Enable":
                        _sounds.useEnableSound = true;
                        AddEventIfZero(_sounds.enableSound);
                        break;
                    case "Disable":
                        _sounds.useDisableSound = true;
                        AddEventIfZero(_sounds.disableSound);
                        break;
                    case "Visible":
                        _sounds.useVisibleSound = true;
                        AddEventIfZero(_sounds.visibleSound);
                        break;
                    case "Invisible":
                        _sounds.useInvisibleSound = true;
                        AddEventIfZero(_sounds.invisibleSound);
                        break;
                    case "2D Collision Enter":
                        _sounds.useCollision2dSound = true;
                        AddEventIfZero(_sounds.collision2dSound);
                        break;
                    case "2D Collision Exit":
                        _sounds.useCollisionExit2dSound = true;
                        AddEventIfZero(_sounds.collisionExit2dSound);
                        break;
                    case "2D Trigger Enter":
                        _sounds.useTriggerEnter2dSound = true;
                        AddEventIfZero(_sounds.triggerEnter2dSound);
                        break;
                    case "2D Trigger Exit":
                        _sounds.useTriggerExit2dSound = true;
                        AddEventIfZero(_sounds.triggerExit2dSound);
                        break;
                    case "Collision Enter":
                        _sounds.useCollisionSound = true;
                        AddEventIfZero(_sounds.collisionSound);
                        break;
                    case "Collision Exit":
                        _sounds.useCollisionExitSound = true;
                        AddEventIfZero(_sounds.collisionExitSound);
                        break;
                    case "Trigger Enter":
                        _sounds.useTriggerEnterSound = true;
                        AddEventIfZero(_sounds.triggerSound);
                        break;
                    case "Trigger Exit":
                        _sounds.useTriggerExitSound = true;
                        AddEventIfZero(_sounds.triggerExitSound);
                        break;
                    case "Particle Collision":
                        _sounds.useParticleCollisionSound = true;
                        AddEventIfZero(_sounds.particleCollisionSound);
                        break;
                    case "Mouse Enter (Legacy)":
                        _sounds.useMouseEnterSound = true;
                        AddEventIfZero(_sounds.mouseEnterSound);
                        break;
                    case "Mouse Exit (Legacy)":
                        _sounds.useMouseExitSound = true;
                        AddEventIfZero(_sounds.mouseExitSound);
                        break;
                    case "Mouse Down (Legacy)":
                        _sounds.useMouseClickSound = true;
                        AddEventIfZero(_sounds.mouseClickSound);
                        break;
                    case "Mouse Drag (Legacy)":
                        _sounds.useMouseDragSound = true;
                        AddEventIfZero(_sounds.mouseDragSound);
                        break;
                    case "Mouse Up (Legacy)":
                        _sounds.useMouseUpSound = true;
                        AddEventIfZero(_sounds.mouseUpSound);
                        break;
                    case "Slider Changed (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnitySliderChanged);
                        break;
                    case "Button Click (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityButtonClicked);
                        break;
                    case "Pointer Down (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityPointerDown);
                        break;
                    case "Pointer Up (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityPointerUp);
                        break;
                    case "Drag (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityDrag);
                        break;
                    case "Drop (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityDrop);
                        break;
                    case "Pointer Enter (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityPointerEnter);
                        break;
                    case "Pointer Exit (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityPointerExit);
                        break;
                    case "Scroll (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityScroll);
                        break;
                    case "Update Selected (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityUpdateSelected);
                        break;
                    case "Select (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnitySelect);
                        break;
                    case "Deselect (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityDeselect);
                        break;
                    case "Move (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityMove);
                        break;
                    case "Initialize Potential Drag (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityInitializePotentialDrag);
                        break;
                    case "Begin Drag (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityBeginDrag);
                        break;
                    case "End Drag (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityEndDrag);
                        break;
                    case "Submit (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnitySubmit);
                        break;
                    case "Cancel (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityCancel);
                        break;
                    case "Toggle (uGUI)":
                        ActivateEvent(EventSounds.EventType.UnityToggle);
                        break;
                    case "NGUI Mouse Click":
                        _sounds.useNguiOnClickSound = true;
                        AddEventIfZero(_sounds.nguiOnClickSound);
                        break;
                    case "NGUI Mouse Down":
                        _sounds.useNguiMouseDownSound = true;
                        AddEventIfZero(_sounds.nguiMouseDownSound);
                        break;
                    case "NGUI Mouse Up":
                        _sounds.useNguiMouseUpSound = true;
                        AddEventIfZero(_sounds.nguiMouseUpSound);
                        break;
                    case "NGUI Mouse Enter":
                        _sounds.useNguiMouseEnterSound = true;
                        AddEventIfZero(_sounds.nguiMouseEnterSound);
                        break;
                    case "NGUI Mouse Exit":
                        _sounds.useNguiMouseExitSound = true;
                        AddEventIfZero(_sounds.nguiMouseExitSound);
                        break;
                    case "Spawned":
                        _sounds.useSpawnedSound = true;
                        AddEventIfZero(_sounds.spawnedSound);
                        break;
                    case "Despawned":
                        _sounds.useDespawnedSound = true;
                        AddEventIfZero(_sounds.despawnedSound);
                        break;
                    case "Mechanim State Entered":
                        CreateMechanimStateEntered(false);
                        break;
                    case "Custom Event":
                        CreateCustomEvent(false);
                        break;
                    default:
                        Debug.LogError("Add code for event type: " + selectedEvent);
                        break;
                }
            }
        }

        if (_sounds.userDefinedSounds.Count > 0) {
            EditorGUILayout.BeginHorizontal();
            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            if (GUILayout.Button("Alpha Sort Custom Event Triggers", EditorStyles.toolbarButton, GUILayout.Width(200))) {
                SortCustomEventTriggers();
            }
            EditorGUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
        }

        EditorGUILayout.Separator();
        var suffix = string.Empty;
        if (_sounds.disableSounds) {
            suffix = " (DISABLED)";
        } else if (unusedEventTypes.Count > 0) {
            suffix = " (" + unusedEventTypes.Count + " hidden)";
        }
        GUILayout.Label("Sound Triggers" + suffix, EditorStyles.boldLabel);

        // trigger sounds
        if (_sounds.useStartSound) {
            RenderEventWithHeader("Start" + DisabledText, "toggle Start Sound", _sounds.startSound, EventSounds.EventType.OnStart);
        }

        if (_sounds.useEnableSound) {
            RenderEventWithHeader("Enable" + DisabledText, "toggle Enable Sound", _sounds.enableSound, EventSounds.EventType.OnEnable);
        }

        if (_sounds.useDisableSound) {
            RenderEventWithHeader("Disable" + DisabledText, "toggle Disable Sound", _sounds.disableSound, EventSounds.EventType.OnDisable);
        }

        if (_sounds.useVisibleSound) {
            RenderEventWithHeader("Visible" + DisabledText, "toggle Visible Sound", _sounds.visibleSound, EventSounds.EventType.OnVisible);
        }

        if (_sounds.useInvisibleSound) {
            RenderEventWithHeader("Invisible" + DisabledText, "toggle Invisible Sound", _sounds.invisibleSound, EventSounds.EventType.OnInvisible);
        }

        if (_sounds.useCollision2dSound) {
            RenderEventWithHeader("2D Collision Enter" + DisabledText, "toggle 2D Collision Enter Sound", _sounds.collision2dSound, EventSounds.EventType.OnCollision2D);
        }

        if (_sounds.useCollisionExit2dSound) {
            RenderEventWithHeader("2D Collision Exit" + DisabledText, "toggle 2D Collision Exit Sound", _sounds.collisionExit2dSound, EventSounds.EventType.OnCollisionExit2D);
        }

        if (_sounds.useTriggerEnter2dSound) {
            RenderEventWithHeader("2D Trigger Enter" + DisabledText, "toggle 2D Trigger Enter Sound", _sounds.triggerEnter2dSound, EventSounds.EventType.OnTriggerEnter2D);
        }

        if (_sounds.useTriggerExit2dSound) {
            RenderEventWithHeader("2D Trigger Exit" + DisabledText, "toggle 2D Trigger Exit Sound", _sounds.triggerExit2dSound, EventSounds.EventType.OnTriggerExit2D);
        }

        if (_sounds.useCollisionSound) {
            RenderEventWithHeader("Collision Enter" + DisabledText, "toggle Collision Enter Sound", _sounds.collisionSound, EventSounds.EventType.OnCollision);
        }

        if (_sounds.useCollisionExitSound) {
            RenderEventWithHeader("Collision Exit" + DisabledText, "toggle Collision Exit Sound", _sounds.collisionExitSound, EventSounds.EventType.OnCollisionExit);
        }

        if (_sounds.useTriggerEnterSound) {
            RenderEventWithHeader("Trigger Enter" + DisabledText, "toggle Trigger Enter Sound", _sounds.triggerSound, EventSounds.EventType.OnTriggerEnter);
        }

        if (_sounds.useTriggerExitSound) {
            RenderEventWithHeader("Trigger Exit" + DisabledText, "toggle Trigger Exit Sound", _sounds.triggerExitSound, EventSounds.EventType.OnTriggerExit);
        }

        if (_sounds.useParticleCollisionSound) {
            RenderEventWithHeader("Particle Collision" + DisabledText, "toggle Particle Collision Sound", _sounds.particleCollisionSound, EventSounds.EventType.OnParticleCollision);
        }

        if (_sounds.useMouseEnterSound) {
            RenderEventWithHeader("Mouse Enter (Legacy)" + DisabledText, "toggle Mouse Enter (Legacy) Sound", _sounds.mouseEnterSound, EventSounds.EventType.OnMouseEnter);
        }

        if (_sounds.useMouseExitSound) {
            RenderEventWithHeader("Mouse Exit (Legacy)" + DisabledText, "toggle Mouse Exit (Legacy) Sound", _sounds.mouseExitSound, EventSounds.EventType.OnMouseExit);
        }

        if (_sounds.useMouseClickSound) {
            RenderEventWithHeader("Mouse Down (Legacy)" + DisabledText, "toggle Mouse Down (Legacy) Sound", _sounds.mouseClickSound, EventSounds.EventType.OnMouseClick);
        }

        if (_sounds.useMouseDragSound) {
            RenderEventWithHeader("Mouse Drag (Legacy)" + DisabledText, "toggle Mouse Drag (Legacy) Sound", _sounds.mouseDragSound, EventSounds.EventType.OnMouseDrag);
        }

        if (_sounds.useMouseUpSound) {
            RenderEventWithHeader("Mouse Up (Legacy)" + DisabledText, "toggle Mouse Up (Legacy) Sound", _sounds.mouseUpSound, EventSounds.EventType.OnMouseUp);
        }

#if UNITY_4_6 || UNITY_4_7 || UNITY_5
        if (showNewUIEvents) {
            if (hasSlider) {
                if (_sounds.useUnitySliderChangedSound) {
                    RenderEventWithHeader("Slider Changed (uGUI)" + DisabledText, "toggle Slider Changed (uGUI) Sound", _sounds.unitySliderChangedSound, EventSounds.EventType.UnitySliderChanged);
                }
            }

            if (hasButton) {
                if (_sounds.useUnityButtonClickedSound) {
                    RenderEventWithHeader("Button Click (uGUI)" + DisabledText, "toggle Button Click (uGUI) Sound", _sounds.unityButtonClickedSound, EventSounds.EventType.UnityButtonClicked);
                }
            }

            if (hasToggle) {
                if (_sounds.useUnityToggleSound) {
                    RenderEventWithHeader("Toggle (uGUI)" + DisabledText, "toggle Toggle (uGUI) Sound", _sounds.unityToggleSound, EventSounds.EventType.UnityToggle);
                }
            }

            if (canClick) {
                if (_sounds.useUnityPointerEnterSound) {
                    RenderEventWithHeader("Pointer Enter (uGUI)" + DisabledText, "toggle Pointer Enter (uGUI) Sound", _sounds.unityPointerEnterSound, EventSounds.EventType.UnityPointerEnter);
                }

                if (_sounds.useUnityPointerExitSound) {
                    RenderEventWithHeader("Pointer Exit (uGUI)" + DisabledText, "toggle Pointer Exit (uGUI) Sound", _sounds.unityPointerExitSound, EventSounds.EventType.UnityPointerExit);
                }

                if (_sounds.useUnityPointerDownSound) {
                    RenderEventWithHeader("Pointer Down (uGUI)" + DisabledText, "toggle Pointer Down (uGUI) Sound", _sounds.unityPointerDownSound, EventSounds.EventType.UnityPointerDown);
                }

                if (_sounds.useUnityPointerUpSound) {
                    RenderEventWithHeader("Pointer Up (uGUI)" + DisabledText, "toggle Pointer Up (uGUI) Sound", _sounds.unityPointerUpSound, EventSounds.EventType.UnityPointerUp);
                }

                if (_sounds.useUnityDragSound) {
                    RenderEventWithHeader("Drag (uGUI)" + DisabledText, "toggle Drag (uGUI) Sound", _sounds.unityDragSound, EventSounds.EventType.UnityDrag);
                }

                if (_sounds.useUnityDropSound) {
                    RenderEventWithHeader("Drop (uGUI)" + DisabledText, "toggle Drop (uGUI) Sound", _sounds.unityDropSound, EventSounds.EventType.UnityDrop);
                }

                if (_sounds.useUnityScrollSound) {
                    RenderEventWithHeader("Scroll (uGUI)" + DisabledText, "toggle Scroll (uGUI) Sound", _sounds.unityScrollSound, EventSounds.EventType.UnityScroll);
                }

                if (_sounds.useUnityUpdateSelectedSound) {
                    RenderEventWithHeader("Update Selected (uGUI)" + DisabledText, "toggle Update Selected (uGUI) Sound", _sounds.unityUpdateSelectedSound, EventSounds.EventType.UnityUpdateSelected);
                }

                if (_sounds.useUnitySelectSound) {
                    RenderEventWithHeader("Select (uGUI)" + DisabledText, "toggle Select (uGUI) Sound", _sounds.unitySelectSound, EventSounds.EventType.UnitySelect);
                }

                if (_sounds.useUnityDeselectSound) {
                    RenderEventWithHeader("Deselect (uGUI)" + DisabledText, "toggle Deselect (uGUI) Sound", _sounds.unityDeselectSound, EventSounds.EventType.UnityDeselect);
                }

                if (_sounds.useUnityMoveSound) {
                    RenderEventWithHeader("Move (uGUI)" + DisabledText, "toggle Move (uGUI) Sound", _sounds.unityMoveSound, EventSounds.EventType.UnityMove);
                }

                if (_sounds.useUnityInitializePotentialDragSound) {
                    RenderEventWithHeader("Initialize Potential Drag (uGUI)" + DisabledText, "toggle Initialize Potential Drag (uGUI) Sound", _sounds.unityInitializePotentialDragSound, EventSounds.EventType.UnityInitializePotentialDrag);
                }

                if (_sounds.useUnityBeginDragSound) {
                    RenderEventWithHeader("Begin Drag (uGUI)" + DisabledText, "toggle Begin Drag (uGUI) Sound", _sounds.unityBeginDragSound, EventSounds.EventType.UnityBeginDrag);
                }

                if (_sounds.useUnityEndDragSound) {
                    RenderEventWithHeader("End Drag (uGUI)" + DisabledText, "toggle End Drag (uGUI) Sound", _sounds.unityEndDragSound, EventSounds.EventType.UnityEndDrag);
                }

                if (_sounds.useUnitySubmitSound) {
                    RenderEventWithHeader("Submit (uGUI)" + DisabledText, "toggle Submit (uGUI) Sound", _sounds.unitySubmitSound, EventSounds.EventType.UnitySubmit);
                }

                if (_sounds.useUnityCancelSound) {
                    RenderEventWithHeader("Cancel (uGUI)" + DisabledText, "toggle Cancel (uGUI) Sound", _sounds.unityCancelSound, EventSounds.EventType.UnityCancel);
                }
            }
        }
#endif

        if (_sounds.showNGUI) {
            if (_sounds.useNguiOnClickSound) {
                RenderEventWithHeader("NGUI Mouse Click" + DisabledText, "toggle NGUI Mouse Click Sound", _sounds.nguiOnClickSound, EventSounds.EventType.NGUIOnClick);
            }

            if (_sounds.useNguiMouseDownSound) {
                RenderEventWithHeader("NGUI Mouse Down" + DisabledText, "toggle NGUI Mouse Down Sound", _sounds.nguiMouseDownSound, EventSounds.EventType.NGUIMouseDown);
            }

            if (_sounds.useNguiMouseUpSound) {
                RenderEventWithHeader("NGUI Mouse Up" + DisabledText, "toggle NGUI Mouse Up Sound", _sounds.nguiMouseUpSound, EventSounds.EventType.NGUIMouseUp);
            }

            if (_sounds.useNguiMouseEnterSound) {
                RenderEventWithHeader("NGUI Mouse Enter" + DisabledText, "toggle NGUI Mouse Enter Sound", _sounds.nguiMouseEnterSound, EventSounds.EventType.NGUIMouseEnter);
            }

            if (_sounds.useNguiMouseExitSound) {
                RenderEventWithHeader("NGUI Mouse Exit" + DisabledText, "toggle NGUI Mouse Exit Sound", _sounds.nguiMouseExitSound, EventSounds.EventType.NGUIMouseExit);
            }
        }

        if (_sounds.showPoolManager) {
            if (_sounds.useSpawnedSound) {
                RenderEventWithHeader("Spawned (Pooling)" + DisabledText, "toggle Spawned (Pooling) Sound", _sounds.spawnedSound, EventSounds.EventType.OnSpawned);
            }

            if (_sounds.useDespawnedSound) {
                RenderEventWithHeader("Despawned (Pooling)" + DisabledText, "toggle Despawned (Pooling) Sound", _sounds.despawnedSound, EventSounds.EventType.OnDespawned);
            }
        }

        if (_sounds.mechanimStateChangedSounds.Count > 0) {
            EditorGUI.indentLevel = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _sounds.mechanimStateChangedSounds.Count; i++) {
                var mechEvt = _sounds.mechanimStateChangedSounds[i];

                var mechName = "Mechanim State Entered";
                if (!string.IsNullOrEmpty(mechEvt.mechanimStateName)) {
                    mechName += ": " + mechEvt.mechanimStateName;
                }
                mechName += DisabledText;

                RenderEventWithHeader(mechName, "toggle " + mechName, mechEvt, EventSounds.EventType.MechanimStateChanged, i);
            }
        }

        if (_sounds.userDefinedSounds.Count > 0) {
            EditorGUI.indentLevel = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _sounds.userDefinedSounds.Count; i++) {
                var customEventGrp = _sounds.userDefinedSounds[i];

                var custName = "Custom Event";
                if (!string.IsNullOrEmpty(customEventGrp.customEventName)) {
                    custName += ": " + customEventGrp.customEventName;
                }
                custName += DisabledText;

                RenderEventWithHeader(custName, "toggle " + custName, customEventGrp, EventSounds.EventType.UserDefinedEvent, i);
            }
        }

        if (GUI.changed || _isDirty || _changedList.Contains(true)) {
            EditorUtility.SetDirty(target);
        }

        //DrawDefaultInspector();
    }

#if UNITY_4_6 || UNITY_4_7 || UNITY_5
    private bool HasEventTrigger {
        get {
            return _sounds.GetComponent<EventTrigger>() != null;
        }
    }

    // ReSharper disable once UnusedMember.Local
    private EventTrigger GetOrCreateEventTrigger {
        get {
            if (HasEventTrigger) {
                return _sounds.GetComponent<EventTrigger>();
            }

            var trig = _sounds.gameObject.AddComponent<EventTrigger>();

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7
            if (trig.triggers == null) {
                trig.triggers = new List<EventTrigger.Entry>();
            }
#else 
                if (trig.delegates == null) {
                    trig.delegates = new List<EventTrigger.Entry>();
                }
#endif

            return trig;
        }
    }
#endif

    private void RenderEventWithHeader(string text, string undoText, AudioEventGroup grp, EventSounds.EventType eType, int? itemIndex = null) {
        EditorGUI.indentLevel = 0;

        var state = grp.isExpanded;

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

        switch (eType) {
            case EventSounds.EventType.MechanimStateChanged:
                if (DTGUIHelper.AddDeleteIcon(false, "Mechanim State Entered Trigger") == DTGUIHelper.DTFunctionButtons.Remove) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                        "delete Mechanim State Entered Sound");
                    // ReSharper disable once PossibleInvalidOperationException
                    _sounds.mechanimStateChangedSounds.RemoveAt(itemIndex.Value);
                    grp.mechanimEventActive = false;
                }
                break;
            case EventSounds.EventType.UserDefinedEvent:
                if (DTGUIHelper.AddDeleteIcon(false, "Custom Event Trigger") == DTGUIHelper.DTFunctionButtons.Remove) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                        "delete Custom Event Sound");
                    // ReSharper disable once PossibleInvalidOperationException
                    _sounds.userDefinedSounds.RemoveAt(itemIndex.Value);
                    grp.customSoundActive = false;
                }
                break;
            default:
                if (DTGUIHelper.AddDeleteIcon(false, "Trigger") == DTGUIHelper.DTFunctionButtons.Remove) {
                    switch (eType) {
                        case EventSounds.EventType.NGUIMouseDown:
                            _sounds.useNguiMouseDownSound = false;
                            break;
                        case EventSounds.EventType.NGUIMouseEnter:
                            _sounds.useNguiMouseEnterSound = false;
                            break;
                        case EventSounds.EventType.NGUIMouseExit:
                            _sounds.useNguiMouseExitSound = false;
                            break;
                        case EventSounds.EventType.NGUIMouseUp:
                            _sounds.useNguiMouseUpSound = false;
                            break;
                        case EventSounds.EventType.NGUIOnClick:
                            _sounds.useNguiOnClickSound = false;
                            break;
                        case EventSounds.EventType.OnCollision:
                            _sounds.useCollisionSound = false;
                            break;
                        case EventSounds.EventType.OnCollision2D:
                            _sounds.useCollision2dSound = false;
                            break;
                        case EventSounds.EventType.OnCollisionExit:
                            _sounds.useCollisionExitSound = false;
                            break;
                        case EventSounds.EventType.OnCollisionExit2D:
                            _sounds.useCollisionExit2dSound = false;
                            break;
                        case EventSounds.EventType.OnDespawned:
                            _sounds.useDespawnedSound = false;
                            break;
                        case EventSounds.EventType.OnDisable:
                            _sounds.useDisableSound = false;
                            break;
                        case EventSounds.EventType.OnEnable:
                            _sounds.useEnableSound = false;
                            break;
                        case EventSounds.EventType.OnInvisible:
                            _sounds.useInvisibleSound = false;
                            break;
                        case EventSounds.EventType.OnMouseClick:
                            _sounds.useMouseClickSound = false;
                            break;
                        case EventSounds.EventType.OnMouseDrag:
                            _sounds.useMouseDragSound = false;
                            break;
                        case EventSounds.EventType.OnMouseEnter:
                            _sounds.useMouseEnterSound = false;
                            break;
                        case EventSounds.EventType.OnMouseExit:
                            _sounds.useMouseExitSound = false;
                            break;
                        case EventSounds.EventType.OnMouseUp:
                            _sounds.useMouseUpSound = false;
                            break;
                        case EventSounds.EventType.OnParticleCollision:
                            _sounds.useParticleCollisionSound = false;
                            break;
                        case EventSounds.EventType.OnSpawned:
                            _sounds.useSpawnedSound = false;
                            break;
                        case EventSounds.EventType.OnStart:
                            _sounds.useStartSound = false;
                            break;
                        case EventSounds.EventType.OnTriggerEnter:
                            _sounds.useTriggerEnterSound = false;
                            break;
                        case EventSounds.EventType.OnTriggerEnter2D:
                            _sounds.useTriggerEnter2dSound = false;
                            break;
                        case EventSounds.EventType.OnTriggerExit:
                            _sounds.useTriggerExitSound = false;
                            break;
                        case EventSounds.EventType.OnTriggerExit2D:
                            _sounds.useTriggerExit2dSound = false;
                            break;
                        case EventSounds.EventType.OnVisible:
                            _sounds.useVisibleSound = false;
                            break;
                        case EventSounds.EventType.UnityBeginDrag:
                            _sounds.useUnityBeginDragSound = false;
                            break;
                        case EventSounds.EventType.UnityButtonClicked:
                            _sounds.useUnityButtonClickedSound = false;
                            break;
                        case EventSounds.EventType.UnityCancel:
                            _sounds.useUnityCancelSound = false;
                            break;
                        case EventSounds.EventType.UnityDeselect:
                            _sounds.useUnityDeselectSound = false;
                            break;
                        case EventSounds.EventType.UnityDrag:
                            _sounds.useUnityDragSound = false;
                            break;
                        case EventSounds.EventType.UnityDrop:
                            _sounds.useUnityDropSound = false;
                            break;
                        case EventSounds.EventType.UnityEndDrag:
                            _sounds.useUnityEndDragSound = false;
                            break;
                        case EventSounds.EventType.UnityInitializePotentialDrag:
                            _sounds.useUnityInitializePotentialDragSound = false;
                            break;
                        case EventSounds.EventType.UnityMove:
                            _sounds.useUnityMoveSound = false;
                            break;
                        case EventSounds.EventType.UnityPointerDown:
                            _sounds.useUnityPointerDownSound = false;
                            break;
                        case EventSounds.EventType.UnityPointerEnter:
                            _sounds.useUnityPointerEnterSound = false;
                            break;
                        case EventSounds.EventType.UnityPointerExit:
                            _sounds.useUnityPointerExitSound = false;
                            break;
                        case EventSounds.EventType.UnityPointerUp:
                            _sounds.useUnityPointerUpSound = false;
                            break;
                        case EventSounds.EventType.UnityScroll:
                            _sounds.useUnityScrollSound = false;
                            break;
                        case EventSounds.EventType.UnitySelect:
                            _sounds.useUnitySelectSound = false;
                            break;
                        case EventSounds.EventType.UnitySliderChanged:
                            _sounds.useUnitySliderChangedSound = false;
                            break;
                        case EventSounds.EventType.UnitySubmit:
                            _sounds.useUnitySubmitSound = false;
                            break;
                        case EventSounds.EventType.UnityUpdateSelected:
                            _sounds.useUnityUpdateSelectedSound = false;
                            break;
                        case EventSounds.EventType.UnityToggle:
                            _sounds.useUnityToggleSound = false;
                            break;
                        default:
                            Debug.LogError("Add code to delete: " + eType);
                            break;
                    }
                }

                break;
        }

        GUILayout.Space(4f);
        DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/EventSounds.htm#EventSettings");

        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!state) {
            GUILayout.Space(3f);
        }

        if (state != grp.isExpanded) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, undoText);
            grp.isExpanded = state;
        }

        if (grp.isExpanded && !_sounds.disableSounds) {
            _changedList.Add(RenderAudioEvent(grp, eType));
        }

        DTGUIHelper.VerticalSpace(2);
    }

    private void ActivateEvent(EventSounds.EventType eType) {
        switch (eType) {
            case EventSounds.EventType.UnitySliderChanged:
                _sounds.useUnitySliderChangedSound = true;
                AddEventIfZero(_sounds.unitySliderChangedSound);
                break;
            case EventSounds.EventType.UnityButtonClicked:
                _sounds.useUnityButtonClickedSound = true;
                AddEventIfZero(_sounds.unityButtonClickedSound);
                break;
            case EventSounds.EventType.UnityPointerDown:
                _sounds.useUnityPointerDownSound = true;
                AddEventIfZero(_sounds.unityPointerDownSound);
                break;
            case EventSounds.EventType.UnityPointerUp:
                _sounds.useUnityPointerUpSound = true;
                AddEventIfZero(_sounds.unityPointerUpSound);
                break;
            case EventSounds.EventType.UnityDrag:
                _sounds.useUnityDragSound = true;
                AddEventIfZero(_sounds.unityDragSound);
                break;
            case EventSounds.EventType.UnityDrop:
                _sounds.useUnityDropSound = true;
                AddEventIfZero(_sounds.unityDropSound);
                break;
            case EventSounds.EventType.UnityPointerEnter:
                _sounds.useUnityPointerEnterSound = true;
                AddEventIfZero(_sounds.unityPointerEnterSound);
                break;
            case EventSounds.EventType.UnityPointerExit:
                _sounds.useUnityPointerExitSound = true;
                AddEventIfZero(_sounds.unityPointerExitSound);
                break;
            case EventSounds.EventType.UnityScroll:
                _sounds.useUnityScrollSound = true;
                AddEventIfZero(_sounds.unityScrollSound);
                break;
            case EventSounds.EventType.UnityUpdateSelected:
                _sounds.useUnityUpdateSelectedSound = true;
                AddEventIfZero(_sounds.unityUpdateSelectedSound);
                break;
            case EventSounds.EventType.UnitySelect:
                _sounds.useUnitySelectSound = true;
                AddEventIfZero(_sounds.unitySelectSound);
                break;
            case EventSounds.EventType.UnityDeselect:
                _sounds.useUnityDeselectSound = true;
                AddEventIfZero(_sounds.unityDeselectSound);
                break;
            case EventSounds.EventType.UnityMove:
                _sounds.useUnityMoveSound = true;
                AddEventIfZero(_sounds.unityMoveSound);
                break;
            case EventSounds.EventType.UnityInitializePotentialDrag:
                _sounds.useUnityInitializePotentialDragSound = true;
                AddEventIfZero(_sounds.unityInitializePotentialDragSound);
                break;
            case EventSounds.EventType.UnityBeginDrag:
                _sounds.useUnityBeginDragSound = true;
                AddEventIfZero(_sounds.unityBeginDragSound);
                break;
            case EventSounds.EventType.UnityEndDrag:
                _sounds.useUnityEndDragSound = true;
                AddEventIfZero(_sounds.unityEndDragSound);
                break;
            case EventSounds.EventType.UnitySubmit:
                _sounds.useUnitySubmitSound = true;
                AddEventIfZero(_sounds.unitySubmitSound);
                break;
            case EventSounds.EventType.UnityCancel:
                _sounds.useUnityCancelSound = true;
                AddEventIfZero(_sounds.unityCancelSound);
                break;
            case EventSounds.EventType.UnityToggle:
                _sounds.useUnityToggleSound = true;
                AddEventIfZero(_sounds.unityToggleSound);
                break;
            default:
                Debug.LogError("Add code to activate: " + eType);
                break;
        }
    }

    // ReSharper disable once UnusedMember.Local
    private void DeactivateEvent(EventSounds.EventType eType) {
        switch (eType) {
            case EventSounds.EventType.UnitySliderChanged:
                _sounds.useUnitySliderChangedSound = false;
                break;
            case EventSounds.EventType.UnityButtonClicked:
                _sounds.useUnityButtonClickedSound = false;
                break;
            case EventSounds.EventType.UnityPointerDown:
                _sounds.useUnityPointerDownSound = false;
                break;
            case EventSounds.EventType.UnityPointerUp:
                _sounds.useUnityPointerUpSound = false;
                break;
            case EventSounds.EventType.UnityDrag:
                _sounds.useUnityDragSound = false;
                break;
            case EventSounds.EventType.UnityDrop:
                _sounds.useUnityDropSound = false;
                break;
            case EventSounds.EventType.UnityPointerEnter:
                _sounds.useUnityPointerEnterSound = false;
                break;
            case EventSounds.EventType.UnityPointerExit:
                _sounds.useUnityPointerExitSound = false;
                break;
            case EventSounds.EventType.UnityScroll:
                _sounds.useUnityScrollSound = false;
                break;
            case EventSounds.EventType.UnityUpdateSelected:
                _sounds.useUnityUpdateSelectedSound = false;
                break;
            case EventSounds.EventType.UnitySelect:
                _sounds.useUnitySelectSound = false;
                break;
            case EventSounds.EventType.UnityDeselect:
                _sounds.useUnityDeselectSound = false;
                break;
            case EventSounds.EventType.UnityMove:
                _sounds.useUnityMoveSound = false;
                break;
            case EventSounds.EventType.UnityInitializePotentialDrag:
                _sounds.useUnityInitializePotentialDragSound = false;
                break;
            case EventSounds.EventType.UnityBeginDrag:
                _sounds.useUnityBeginDragSound = false;
                break;
            case EventSounds.EventType.UnityEndDrag:
                _sounds.useUnityEndDragSound = false;
                break;
            case EventSounds.EventType.UnitySubmit:
                _sounds.useUnitySubmitSound = false;
                break;
            case EventSounds.EventType.UnityCancel:
                _sounds.useUnityCancelSound = false;
                break;
            default:
                Debug.LogError("Add code to remove: " + eType);
                break;
        }
    }

    // ReSharper disable once FunctionComplexityOverflow
    private bool RenderAudioEvent(AudioEventGroup eventGrp, EventSounds.EventType eType) {
        DTGUIHelper.BeginGroupedControls();

        int? indexToRemove = null;
        int? indexToInsert = null;
        int? indexToShiftUp = null;
        int? indexToShiftDown = null;
        var hideActions = _sounds.disableSounds;

        var isSliderChangedEvent = (eType == EventSounds.EventType.UnitySliderChanged);

        if (_sounds.useMouseDragSound && eType == EventSounds.EventType.OnMouseUp) {
            var newStopDragSound = (EventSounds.PreviousSoundStopMode)EditorGUILayout.EnumPopup("Mouse Drag Sound End", eventGrp.mouseDragStopMode);
            if (newStopDragSound != eventGrp.mouseDragStopMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Mouse Drag Sound End");
                eventGrp.mouseDragStopMode = newStopDragSound;
            }

            if (eventGrp.mouseDragStopMode == EventSounds.PreviousSoundStopMode.FadeOut) {
                EditorGUI.indentLevel = 1;
                var newFade = EditorGUILayout.Slider("Mouse Drag Fade Time", eventGrp.mouseDragFadeOutTime, 0f, 1f);
                if (newFade != eventGrp.mouseDragFadeOutTime) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Mouse Drag Fade Time");
                    eventGrp.mouseDragFadeOutTime = newFade;
                }
            }
        }

        EditorGUI.indentLevel = 0;
        var showLayerTagFilter = EventSounds.LayerTagFilterEvents.Contains(eType.ToString());

        if (showLayerTagFilter) {
            DTGUIHelper.StartGroupHeader();
            var newUseLayers = EditorGUILayout.BeginToggleGroup(" Layer filter", eventGrp.useLayerFilter);
            if (newUseLayers != eventGrp.useLayerFilter) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Layer filter");
                eventGrp.useLayerFilter = newUseLayers;
            }

            DTGUIHelper.EndGroupHeader();

            if (eventGrp.useLayerFilter) {
                for (var i = 0; i < eventGrp.matchingLayers.Count; i++) {
                    var newLayer = EditorGUILayout.LayerField("Layer Match " + (i + 1), eventGrp.matchingLayers[i]);
                    if (newLayer == eventGrp.matchingLayers[i]) {
                        continue;
                    }
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Layer filter");
                    eventGrp.matchingLayers[i] = newLayer;
                }
                EditorGUILayout.BeginHorizontal();

                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                if (GUILayout.Button(new GUIContent("Add", "Click to add a layer match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "add Layer filter");
                    eventGrp.matchingLayers.Add(0);
                }
                if (eventGrp.matchingLayers.Count > 1) {
                    GUILayout.Space(10);
                    if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last layer match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "remove Layer filter");
                        eventGrp.matchingLayers.RemoveAt(eventGrp.matchingLayers.Count - 1);
                    }
                }
                GUI.contentColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndToggleGroup();
            DTGUIHelper.AddSpaceForNonU5(2);

            DTGUIHelper.StartGroupHeader();

            var newTagFilter = EditorGUILayout.BeginToggleGroup(" Tag filter", eventGrp.useTagFilter);
            if (newTagFilter != eventGrp.useTagFilter) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Tag filter");
                eventGrp.useTagFilter = newTagFilter;
            }

            DTGUIHelper.EndGroupHeader();

            if (eventGrp.useTagFilter) {
                for (var i = 0; i < eventGrp.matchingTags.Count; i++) {
                    var newTag = EditorGUILayout.TagField("Tag Match " + (i + 1), eventGrp.matchingTags[i]);
                    if (newTag == eventGrp.matchingTags[i]) {
                        continue;
                    }
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Tag filter");
                    eventGrp.matchingTags[i] = newTag;
                }
                EditorGUILayout.BeginHorizontal();
                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                if (GUILayout.Button(new GUIContent("Add", "Click to add a tag match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Add Tag filter");
                    eventGrp.matchingTags.Add("Untagged");
                }
                if (eventGrp.matchingTags.Count > 1) {
                    GUILayout.Space(10);
                    if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last tag match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "remove Tag filter");
                        eventGrp.matchingTags.RemoveAt(eventGrp.matchingLayers.Count - 1);
                    }
                }
                GUI.contentColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndToggleGroup();
        }

        if (eType == EventSounds.EventType.MechanimStateChanged) {
            if (!eventGrp.mechanimEventActive) {
                hideActions = true;
            }

            if (eventGrp.mechanimEventActive && !hideActions) {
                var newName = EditorGUILayout.TextField("State Name", eventGrp.mechanimStateName);
                if (newName != eventGrp.mechanimStateName) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change State Name");
                    eventGrp.mechanimStateName = newName;
                }

                if (!_hasMechanim) {
                    DTGUIHelper.ShowRedError("This Game Object does not have an Animator component.");
                } else {
                    if (string.IsNullOrEmpty(eventGrp.mechanimStateName)) {
                        DTGUIHelper.ShowRedError("No State Name specified. This event will do nothing.");
                    }
                }
            }
        }

        if (eType == EventSounds.EventType.UserDefinedEvent) {
            if (!eventGrp.customSoundActive) {
                DTGUIHelper.EndGroupedControls();
                return true;
            }

            if (!hideActions) {
                if (_maInScene) {
                    var existingIndex = _customEventNames.IndexOf(eventGrp.customEventName);

                    int? customEventIndex = null;

                    EditorGUI.indentLevel = 0;

                    var noEvent = false;
                    var noMatch = false;

                    if (existingIndex >= 1) {
                        customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                        if (existingIndex == 1) {
                            noEvent = true;
                        }
                    } else if (existingIndex == -1 && eventGrp.customEventName == MasterAudio.NoGroupName) {
                        customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                    } else { // non-match
                        noMatch = true;
                        var newEventName = EditorGUILayout.TextField("Custom Event Name", eventGrp.customEventName);
                        if (newEventName != eventGrp.customEventName) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Custom Event Name");
                            eventGrp.customEventName = newEventName;
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
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Custom Event");
                        }
                        switch (customEventIndex.Value) {
                            case -1:
                                eventGrp.customEventName = MasterAudio.NoGroupName;
                                break;
                            default:
                                eventGrp.customEventName = _customEventNames[customEventIndex.Value];
                                break;
                        }
                    }
                } else {
                    var newCustomEvent = EditorGUILayout.TextField("Custom Event Name", eventGrp.customEventName);
                    if (newCustomEvent != eventGrp.customEventName) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Custom Event Name");
                        eventGrp.customEventName = newCustomEvent;
                    }
                }
            }
        }

        if (eventGrp.SoundEvents.Count == 0) {
            eventGrp.SoundEvents.Add(new AudioEvent());
        }

        if (!hideActions) {
            if (showLayerTagFilter) {
                DTGUIHelper.AddSpaceForNonU5(2);
            }
            DTGUIHelper.StartGroupHeader();
            EditorGUILayout.BeginHorizontal();

            var newRetrigger = (EventSounds.RetriggerLimMode)EditorGUILayout.EnumPopup("Retrigger Limit Mode", eventGrp.retriggerLimitMode);
            if (newRetrigger != eventGrp.retriggerLimitMode) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Retrigger Limit Mode");
                eventGrp.retriggerLimitMode = newRetrigger;
            }

            DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/EventSounds.htm#Retrigger");

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            switch (eventGrp.retriggerLimitMode) {
                case EventSounds.RetriggerLimMode.FrameBased:
                    var newFrm = EditorGUILayout.IntSlider("Min Frames Between", eventGrp.limitPerXFrm, 0, 10000);
                    if (newFrm != eventGrp.limitPerXFrm) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Min Frames Between");
                        eventGrp.limitPerXFrm = newFrm;
                    }
                    break;
                case EventSounds.RetriggerLimMode.TimeBased:
                    var newSec = EditorGUILayout.Slider("Min Seconds Between", eventGrp.limitPerXSec, 0f, 10000f);
                    if (newSec != eventGrp.limitPerXSec) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Min Seconds Between");
                        eventGrp.limitPerXSec = newSec;
                    }
                    break;
            }
            EditorGUILayout.EndVertical();

            AudioEvent prevEvent = null;

            for (var j = 0; j < eventGrp.SoundEvents.Count; j++) {
                DTGUIHelper.AddSpaceForNonU5(2);
                var showVolumeSlider = true;
                var aEvent = eventGrp.SoundEvents[j];

                EditorGUI.indentLevel = 1;

                DTGUIHelper.StartGroupHeader();

                EditorGUILayout.BeginHorizontal();

                var newExpanded = DTGUIHelper.Foldout(aEvent.isExpanded, "Action #" + (j + 1));
                if (newExpanded != aEvent.isExpanded) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle expand Action");
                    aEvent.isExpanded = newExpanded;
                }

                GUILayout.FlexibleSpace();

                var newActionName = GUILayout.TextField(aEvent.actionName, GUILayout.Width(150));
                if (newActionName != aEvent.actionName) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Rename action");
                    aEvent.actionName = newActionName;
                }

                var buttonPressed = DTGUIHelper.AddFoldOutListItemButtonItems(j, eventGrp.SoundEvents.Count, "Action", true, false, true);

                GUILayout.Space(4);
                DTGUIHelper.AddHelpIcon("https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MA_OnlineDocs/EventSounds.htm#Actions");

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;

                if (prevEvent != null && prevEvent.IsFadeCommand) {
                    DTGUIHelper.ShowLargeBarAlert("This action will start immediately after the fade action above is *started*, not finished");
                }

                if (aEvent.isExpanded) {
                    EditorGUI.indentLevel = 0;

                    var newSoundType = (MasterAudio.EventSoundFunctionType)EditorGUILayout.EnumPopup("Action Type", aEvent.currentSoundFunctionType);
                    if (newSoundType != aEvent.currentSoundFunctionType) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Action Type");
                        aEvent.currentSoundFunctionType = newSoundType;
                    }

                    switch (aEvent.currentSoundFunctionType) {
                        case MasterAudio.EventSoundFunctionType.PlaySound:
                            if (_maInScene) {
                                var existingIndex = _groupNames.IndexOf(aEvent.soundType);

                                int? groupIndex = null;

                                EditorGUI.indentLevel = 1;

                                var noGroup = false;
                                var noMatch = false;

                                if (existingIndex >= 1) {
                                    EditorGUILayout.BeginHorizontal();
                                    groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, _groupNames.ToArray());
                                    if (existingIndex == 1) {
                                        noGroup = true;
                                    }

                                    if (groupIndex > MasterAudio.HardCodedBusOptions - 1) {
                                        var button = DTGUIHelper.AddSettingsButton("Sound Group");
                                        switch (button) {
                                            case DTGUIHelper.DTFunctionButtons.Go:
                                                var grp = _groupNames[existingIndex];
                                                var trs = MasterAudio.FindGroupTransform(grp);
                                                if (trs != null) {
                                                    Selection.activeObject = trs;
                                                }
                                                break;
                                        }
                                    }

                                    EditorGUILayout.EndHorizontal();
                                } else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName) {
                                    groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, _groupNames.ToArray());
                                } else { // non-match
                                    noMatch = true;
                                    var newSound = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                    if (newSound != aEvent.soundType) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Group");
                                        aEvent.soundType = newSound;
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

                                if (groupIndex.HasValue) {
                                    if (existingIndex != groupIndex.Value) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Group");
                                    }
                                    switch (groupIndex.Value) {
                                        case -1:
                                            aEvent.soundType = MasterAudio.NoGroupName;
                                            break;
                                        default:
                                            aEvent.soundType = _groupNames[groupIndex.Value];
                                            break;
                                    }
                                }
                            } else {
                                var newSType = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                if (newSType != aEvent.soundType) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Group");
                                    aEvent.soundType = newSType;
                                }
                            }

                            var newVarType = (EventSounds.VariationType)EditorGUILayout.EnumPopup("Variation Mode", aEvent.variationType);
                            if (newVarType != aEvent.variationType) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Variation Mode");
                                aEvent.variationType = newVarType;
                            }

                            if (aEvent.variationType == EventSounds.VariationType.PlaySpecific) {
                                var newVarName = EditorGUILayout.TextField("Variation Name", aEvent.variationName);
                                if (newVarName != aEvent.variationName) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Variation Name");
                                    aEvent.variationName = newVarName;
                                }

                                if (string.IsNullOrEmpty(aEvent.variationName)) {
                                    DTGUIHelper.ShowRedError("Variation Name is empty. No sound will play.");
                                }
                            }

                            if (isSliderChangedEvent) {
                                var newSlider =
                                    (AudioEvent.TargetVolumeMode)
                                        EditorGUILayout.EnumPopup("Volume Mode", aEvent.targetVolMode);
                                if (newSlider != aEvent.targetVolMode) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                        "change Volume Mode");
                                    aEvent.targetVolMode = newSlider;
                                }

                                if (aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue) {
                                    showVolumeSlider = false;
                                }
                            }

                            if (showVolumeSlider) {
                                var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                    DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true);
                                if (newVol != aEvent.volume) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Volume");
                                    aEvent.volume = newVol;
                                }
                            }

                            var newFixedPitch = EditorGUILayout.Toggle("Override pitch?", aEvent.useFixedPitch);
                            if (newFixedPitch != aEvent.useFixedPitch) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Override pitch");
                                aEvent.useFixedPitch = newFixedPitch;
                            }
                            if (aEvent.useFixedPitch) {
                                var newPitch = DTGUIHelper.DisplayPitchField(aEvent.pitch);
                                if (newPitch != aEvent.pitch) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Pitch");
                                    aEvent.pitch = newPitch;
                                }
                            }

                            var newDelay = EditorGUILayout.Slider("Delay Sound (sec)", aEvent.delaySound, 0f, 10f);
                            if (newDelay != aEvent.delaySound) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Delay Sound");
                                aEvent.delaySound = newDelay;
                            }
                            break;
                        case MasterAudio.EventSoundFunctionType.PlaylistControl:
                            EditorGUI.indentLevel = 1;
                            var newPlaylistCmd = (MasterAudio.PlaylistCommand)EditorGUILayout.EnumPopup("Playlist Command", aEvent.currentPlaylistCommand);
                            if (newPlaylistCmd != aEvent.currentPlaylistCommand) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Playlist Command");
                                aEvent.currentPlaylistCommand = newPlaylistCmd;
                            }

                            if (aEvent.currentPlaylistCommand != MasterAudio.PlaylistCommand.None) {
                                // show Playlist Controller dropdown
                                if (EventSounds.PlaylistCommandsWithAll.Contains(aEvent.currentPlaylistCommand)) {
                                    var newAllControllers = EditorGUILayout.Toggle("All Playlist Controllers?", aEvent.allPlaylistControllersForGroupCmd);
                                    if (newAllControllers != aEvent.allPlaylistControllersForGroupCmd) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle All Playlist Controllers");
                                        aEvent.allPlaylistControllersForGroupCmd = newAllControllers;
                                    }
                                }

                                if (!aEvent.allPlaylistControllersForGroupCmd) {
                                    if (_playlistControllerNames.Count > 0) {
                                        var existingIndex = _playlistControllerNames.IndexOf(aEvent.playlistControllerName);

                                        int? playlistControllerIndex = null;

                                        var noPC = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1) {
                                            playlistControllerIndex = EditorGUILayout.Popup("Playlist Controller", existingIndex, _playlistControllerNames.ToArray());
                                            if (existingIndex == 1) {
                                                noPC = true;
                                            }
                                        } else if (existingIndex == -1 && aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                            playlistControllerIndex = EditorGUILayout.Popup("Playlist Controller", existingIndex, _playlistControllerNames.ToArray());
                                        } else { // non-match
                                            noMatch = true;

                                            var newPlaylistController = EditorGUILayout.TextField("Playlist Controller", aEvent.playlistControllerName);
                                            if (newPlaylistController != aEvent.playlistControllerName) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Playlist Controller");
                                                aEvent.playlistControllerName = newPlaylistController;
                                            }
                                            var newIndex = EditorGUILayout.Popup("All Playlist Controllers", -1, _playlistControllerNames.ToArray());
                                            if (newIndex >= 0) {
                                                playlistControllerIndex = newIndex;
                                            }
                                        }

                                        if (noPC) {
                                            DTGUIHelper.ShowRedError("No Playlist Controller specified. Action will do nothing.");
                                        } else if (noMatch) {
                                            DTGUIHelper.ShowRedError("Playlist Controller found no match. Type in or choose one.");
                                        }

                                        if (playlistControllerIndex.HasValue) {
                                            if (existingIndex != playlistControllerIndex.Value) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Playlist Controller");
                                            }
                                            switch (playlistControllerIndex.Value) {
                                                case -1:
                                                    aEvent.playlistControllerName = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    aEvent.playlistControllerName = _playlistControllerNames[playlistControllerIndex.Value];
                                                    break;
                                            }
                                        }
                                    } else {
                                        var newPlaylistControllerName = EditorGUILayout.TextField("Playlist Controller", aEvent.playlistControllerName);
                                        if (newPlaylistControllerName != aEvent.playlistControllerName) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Playlist Controller");
                                            aEvent.playlistControllerName = newPlaylistControllerName;
                                        }
                                    }
                                }
                            }

                            switch (aEvent.currentPlaylistCommand) {
                                case MasterAudio.PlaylistCommand.None:
                                    DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                                    break;
                                case MasterAudio.PlaylistCommand.StopLoopingCurrentSong:
                                    break;
                                case MasterAudio.PlaylistCommand.ChangePlaylist:
                                case MasterAudio.PlaylistCommand.Start:
                                    // show playlist name dropdown
                                    if (_maInScene) {
                                        var existingIndex = _playlistNames.IndexOf(aEvent.playlistName);

                                        int? playlistIndex = null;

                                        var noPl = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1) {
                                            playlistIndex = EditorGUILayout.Popup("Playlist Name", existingIndex, _playlistNames.ToArray());
                                            if (existingIndex == 1) {
                                                noPl = true;
                                            }
                                        } else if (existingIndex == -1 && aEvent.playlistName == MasterAudio.NoGroupName) {
                                            playlistIndex = EditorGUILayout.Popup("Playlist Name", existingIndex, _playlistNames.ToArray());
                                        } else { // non-match
                                            noMatch = true;

                                            var newPlaylist = EditorGUILayout.TextField("Playlist Name", aEvent.playlistName);
                                            if (newPlaylist != aEvent.playlistName) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Playlist Name");
                                                aEvent.playlistName = newPlaylist;
                                            }
                                            var newIndex = EditorGUILayout.Popup("All Playlists", -1, _playlistNames.ToArray());
                                            if (newIndex >= 0) {
                                                playlistIndex = newIndex;
                                            }
                                        }

                                        if (noPl) {
                                            DTGUIHelper.ShowRedError("No Playlist Name specified. Action will do nothing.");
                                        } else if (noMatch) {
                                            DTGUIHelper.ShowRedError("Playlist Name found no match. Type in or choose one.");
                                        }

                                        if (playlistIndex.HasValue) {
                                            if (existingIndex != playlistIndex.Value) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Playlist Name");
                                            }
                                            switch (playlistIndex.Value) {
                                                case -1:
                                                    aEvent.playlistName = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    aEvent.playlistName = _playlistNames[playlistIndex.Value];
                                                    break;
                                            }
                                        }
                                    } else {
                                        var newPlaylistName = EditorGUILayout.TextField("Playlist Name", aEvent.playlistName);
                                        if (newPlaylistName != aEvent.playlistName) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Playlist Name");
                                            aEvent.playlistName = newPlaylistName;
                                        }
                                    }

                                    if (aEvent.currentPlaylistCommand == MasterAudio.PlaylistCommand.ChangePlaylist) {
                                        var newStartPlaylist = EditorGUILayout.Toggle("Start Playlist?", aEvent.startPlaylist);
                                        if (newStartPlaylist != aEvent.startPlaylist) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Start Playlist");
                                            aEvent.startPlaylist = newStartPlaylist;
                                        }
                                    }
                                    break;
                                case MasterAudio.PlaylistCommand.FadeToVolume:
                                    if (isSliderChangedEvent) {
                                        var newSlider =
                                            (AudioEvent.TargetVolumeMode)
                                                EditorGUILayout.EnumPopup("Volume Mode", aEvent.targetVolMode);
                                        if (newSlider != aEvent.targetVolMode) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Volume Mode");
                                            aEvent.targetVolMode = newSlider;
                                        }

                                        if (aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue) {
                                            showVolumeSlider = false;
                                        }
                                    }

                                    if (showVolumeSlider) {
                                        var newFadeVol = DTGUIHelper.DisplayVolumeField(aEvent.fadeVolume, DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true, "Target Volume");
                                        if (newFadeVol != aEvent.fadeVolume) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Target Volume");
                                            aEvent.fadeVolume = newFadeVol;
                                        }
                                    }

                                    var newFadeTime = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                    if (newFadeTime != aEvent.fadeTime) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Fade Time");
                                        aEvent.fadeTime = newFadeTime;
                                    }
                                    break;
                                case MasterAudio.PlaylistCommand.PlayClip:
                                    var newClip = EditorGUILayout.TextField("Clip Name", aEvent.clipName);
                                    if (newClip != aEvent.clipName) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Clip Name");
                                        aEvent.clipName = newClip;
                                    }
                                    if (string.IsNullOrEmpty(aEvent.clipName)) {
                                        DTGUIHelper.ShowRedError("Clip name is empty. Action will do nothing.");
                                    }
                                    break;
                            }
                            break;
                        case MasterAudio.EventSoundFunctionType.GroupControl:
                            EditorGUI.indentLevel = 1;

                            var newGroupCmd = (MasterAudio.SoundGroupCommand)EditorGUILayout.EnumPopup("Group Command", aEvent.currentSoundGroupCommand);
                            if (newGroupCmd != aEvent.currentSoundGroupCommand) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Group Command");
                                aEvent.currentSoundGroupCommand = newGroupCmd;
                            }

                            if (!MasterAudio.GroupCommandsWithNoGroupSelector.Contains(aEvent.currentSoundGroupCommand)) {
                                if (!MasterAudio.GroupCommandsWithNoAllGroupSelector.Contains(aEvent.currentSoundGroupCommand)) {
                                    var newAllTypes = EditorGUILayout.Toggle("Do For Every Group?", aEvent.allSoundTypesForGroupCmd);
                                    if (newAllTypes != aEvent.allSoundTypesForGroupCmd) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Do For Every Group?");
                                        aEvent.allSoundTypesForGroupCmd = newAllTypes;
                                    }
                                }

                                if (!aEvent.allSoundTypesForGroupCmd) {
                                    if (_maInScene) {
                                        var existingIndex = _groupNames.IndexOf(aEvent.soundType);

                                        int? groupIndex = null;

                                        var noGroup = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1) {
                                            groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, _groupNames.ToArray());
                                            if (existingIndex == 1) {
                                                noGroup = true;
                                            }
                                        } else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName) {
                                            groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, _groupNames.ToArray());
                                        } else { // non-match
                                            var newSType = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                            if (newSType != aEvent.soundType) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Group");
                                                aEvent.soundType = newSType;
                                            }

                                            var newIndex = EditorGUILayout.Popup("All Sound Groups", -1, _groupNames.ToArray());
                                            if (newIndex >= 0) {
                                                groupIndex = newIndex;
                                            }

                                            noMatch = true;
                                        }

                                        if (noMatch) {
                                            DTGUIHelper.ShowRedError("Sound Group found no match. Type in or choose one.");
                                        } else if (noGroup) {
                                            DTGUIHelper.ShowRedError("No Sound Group specified. Action will do nothing.");
                                        }

                                        if (groupIndex.HasValue) {
                                            if (existingIndex != groupIndex.Value) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Group");
                                            }
                                            switch (groupIndex.Value) {
                                                case -1:
                                                    aEvent.soundType = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    aEvent.soundType = _groupNames[groupIndex.Value];
                                                    break;
                                            }
                                        }
                                    } else {
                                        var newSoundT = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                        if (newSoundT != aEvent.soundType) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Group");
                                            aEvent.soundType = newSoundT;
                                        }
                                    }
                                }
                            }

                            switch (aEvent.currentSoundGroupCommand) {
                                case MasterAudio.SoundGroupCommand.None:
                                    DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                                    break;
                                case MasterAudio.SoundGroupCommand.FadeToVolume:
                                    if (isSliderChangedEvent) {
                                        var newSlider =
                                            (AudioEvent.TargetVolumeMode)
                                                EditorGUILayout.EnumPopup("Volume Mode", aEvent.targetVolMode);
                                        if (newSlider != aEvent.targetVolMode) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Volume Mode");
                                            aEvent.targetVolMode = newSlider;
                                        }

                                        if (aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue) {
                                            showVolumeSlider = false;
                                        }
                                    }

                                    if (showVolumeSlider) {
                                        var newFadeVol = DTGUIHelper.DisplayVolumeField(aEvent.fadeVolume,
                                            DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true, "Target Volume");
                                        if (newFadeVol != aEvent.fadeVolume) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Target Volume");
                                            aEvent.fadeVolume = newFadeVol;
                                        }
                                    }

                                    var newFadeTime = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                    if (newFadeTime != aEvent.fadeTime) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Fade Time");
                                        aEvent.fadeTime = newFadeTime;
                                    }
                                    break;
                                case MasterAudio.SoundGroupCommand.FadeOutAllOfSound:
                                    var newFadeT = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                    if (newFadeT != aEvent.fadeTime) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Fade Time");
                                        aEvent.fadeTime = newFadeT;
                                    }
                                    break;
                                case MasterAudio.SoundGroupCommand.FadeOutSoundGroupOfTransform:
                                    var newFade = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                    if (newFade != aEvent.fadeTime) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Fade Time");
                                        aEvent.fadeTime = newFade;
                                    }
                                    break;
                                case MasterAudio.SoundGroupCommand.RouteToBus:
                                    if (_maInScene) {
                                        var existingIndex = _busNames.IndexOf(aEvent.busName);

                                        int? busIndex = null;

                                        var noBus = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1) {
                                            busIndex = EditorGUILayout.Popup("Bus Name", existingIndex,
                                                _busNames.ToArray());
                                            if (existingIndex == 1) {
                                                noBus = true;
                                            }
                                        } else if (existingIndex == -1 && aEvent.busName == MasterAudio.NoGroupName) {
                                            busIndex = EditorGUILayout.Popup("Bus Name", existingIndex,
                                                _busNames.ToArray());
                                        } else {
                                            // non-match
                                            var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                            if (newBusName != aEvent.busName) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                    "change Bus Name");
                                                aEvent.busName = newBusName;
                                            }

                                            var newIndex = EditorGUILayout.Popup("All Buses", -1,
                                                _busNames.ToArray());
                                            if (newIndex >= 0) {
                                                busIndex = newIndex;
                                            }
                                            noMatch = true;
                                        }

                                        if (noBus) {
                                            DTGUIHelper.ShowRedError(
                                                "No Bus Name specified. Action will do nothing.");
                                        } else if (noMatch) {
                                            DTGUIHelper.ShowRedError(
                                                "Bus Name found no match. Type in or choose one.");
                                        }

                                        if (busIndex.HasValue) {
                                            if (existingIndex != busIndex.Value) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                    "change Bus");
                                            }
                                            switch (busIndex.Value) {
                                                case -1:
                                                    aEvent.busName = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    aEvent.busName = _busNames[busIndex.Value];
                                                    break;
                                            }
                                        }
                                    } else {
                                        var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                        if (newBusName != aEvent.busName) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Bus Name");
                                            aEvent.busName = newBusName;
                                        }
                                    }
                                    break;
                                case MasterAudio.SoundGroupCommand.Mute:
                                    break;
                                case MasterAudio.SoundGroupCommand.Pause:
                                    break;
                                case MasterAudio.SoundGroupCommand.Solo:
                                    break;
                                case MasterAudio.SoundGroupCommand.Unmute:
                                    break;
                                case MasterAudio.SoundGroupCommand.Unpause:
                                    break;
                                case MasterAudio.SoundGroupCommand.Unsolo:
                                    break;
                            }

                            break;
                        case MasterAudio.EventSoundFunctionType.BusControl:
                            EditorGUI.indentLevel = 1;
                            var newBusCmd = (MasterAudio.BusCommand)EditorGUILayout.EnumPopup("Bus Command", aEvent.currentBusCommand);
                            if (newBusCmd != aEvent.currentBusCommand) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Command");
                                aEvent.currentBusCommand = newBusCmd;
                            }

                            if (aEvent.currentBusCommand != MasterAudio.BusCommand.None) {
                                var newAllTypes = EditorGUILayout.Toggle("Do For Every Bus?", aEvent.allSoundTypesForBusCmd);
                                if (newAllTypes != aEvent.allSoundTypesForBusCmd) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Do For Every Bus?");
                                    aEvent.allSoundTypesForBusCmd = newAllTypes;
                                }

                                if (!aEvent.allSoundTypesForBusCmd) {
                                    if (_maInScene) {
                                        var existingIndex = _busNames.IndexOf(aEvent.busName);

                                        int? busIndex = null;

                                        var noBus = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1) {
                                            busIndex = EditorGUILayout.Popup("Bus Name", existingIndex, _busNames.ToArray());
                                            if (existingIndex == 1) {
                                                noBus = true;
                                            }
                                        } else if (existingIndex == -1 && aEvent.busName == MasterAudio.NoGroupName) {
                                            busIndex = EditorGUILayout.Popup("Bus Name", existingIndex, _busNames.ToArray());
                                        } else { // non-match
                                            var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                            if (newBusName != aEvent.busName) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Name");
                                                aEvent.busName = newBusName;
                                            }

                                            var newIndex = EditorGUILayout.Popup("All Buses", -1, _busNames.ToArray());
                                            if (newIndex >= 0) {
                                                busIndex = newIndex;
                                            }

                                            noMatch = true;
                                        }

                                        if (noMatch) {
                                            DTGUIHelper.ShowRedError("Bus Name found no match. Type in or choose one.");
                                        } else if (noBus) {
                                            DTGUIHelper.ShowRedError("No Bus Name specified. Action will do nothing.");
                                        } 

                                        if (busIndex.HasValue) {
                                            if (existingIndex != busIndex.Value) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus");
                                            }
                                            switch (busIndex.Value) {
                                                case -1:
                                                    aEvent.busName = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    aEvent.busName = _busNames[busIndex.Value];
                                                    break;
                                            }
                                        }
                                    } else {
                                        var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                        if (newBusName != aEvent.busName) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Name");
                                            aEvent.busName = newBusName;
                                        }
                                    }
                                }
                            }

                            switch (aEvent.currentBusCommand) {
                                case MasterAudio.BusCommand.None:
                                    DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                                    break;
                                case MasterAudio.BusCommand.ChangeBusPitch:
                                    var newPitch = DTGUIHelper.DisplayPitchField(aEvent.pitch);
                                    if (newPitch != aEvent.pitch) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Pitch");
                                        aEvent.pitch = newPitch;
                                    }
                                    break;
                                case MasterAudio.BusCommand.FadeToVolume:
                                    if (isSliderChangedEvent) {
                                        var newSlider =
                                            (AudioEvent.TargetVolumeMode)
                                                EditorGUILayout.EnumPopup("Volume Mode", aEvent.targetVolMode);
                                        if (newSlider != aEvent.targetVolMode) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Volume Mode");
                                            aEvent.targetVolMode = newSlider;
                                        }

                                        if (aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue) {
                                            showVolumeSlider = false;
                                        }
                                    }

                                    if (showVolumeSlider) {
                                        var newFadeVol = DTGUIHelper.DisplayVolumeField(aEvent.fadeVolume,
                                            DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true, "Target Volume");
                                        if (newFadeVol != aEvent.fadeVolume) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Target Volume");
                                            aEvent.fadeVolume = newFadeVol;
                                        }
                                    }

                                    var newFadeTime = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                    if (newFadeTime != aEvent.fadeTime) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Fade Time");
                                        aEvent.fadeTime = newFadeTime;
                                    }
                                    break;
                                case MasterAudio.BusCommand.Pause:
                                    break;
                                case MasterAudio.BusCommand.Unpause:
                                    break;
                            }

                            break;
                        case MasterAudio.EventSoundFunctionType.CustomEventControl:
                            if (eType == EventSounds.EventType.UserDefinedEvent) {
                                DTGUIHelper.ShowRedError("Custom Event Receivers cannot fire events. Select another Action Type.");
                                break;
                            }

                            EditorGUI.indentLevel = 1;
                            var newEventCmd = (MasterAudio.CustomEventCommand)EditorGUILayout.EnumPopup("Custom Event Cmd", aEvent.currentCustomEventCommand);
                            if (newEventCmd != aEvent.currentCustomEventCommand) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Custom Event Command");
                                aEvent.currentCustomEventCommand = newEventCmd;
                            }

                            switch (aEvent.currentCustomEventCommand) {
                                case MasterAudio.CustomEventCommand.None:
                                    DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                                    break;
                                case MasterAudio.CustomEventCommand.FireEvent:
                                    if (_maInScene) {
                                        var existingIndex = _customEventNames.IndexOf(aEvent.theCustomEventName);

                                        int? customEventIndex = null;

                                        EditorGUI.indentLevel = 1;

                                        var noEvent = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1) {
                                            customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                                            if (existingIndex == 1) {
                                                noEvent = true;
                                            }
                                        } else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName) {
                                            customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _customEventNames.ToArray());
                                        } else { // non-match
                                            noMatch = true;
                                            var newEventName = EditorGUILayout.TextField("Custom Event Name", aEvent.theCustomEventName);
                                            if (newEventName != aEvent.theCustomEventName) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Custom Event Name");
                                                aEvent.theCustomEventName = newEventName;
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
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Custom Event");
                                            }
                                            switch (customEventIndex.Value) {
                                                case -1:
                                                    aEvent.theCustomEventName = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    aEvent.theCustomEventName = _customEventNames[customEventIndex.Value];
                                                    break;
                                            }
                                        }
                                    } else {
                                        var newCustomEvent = EditorGUILayout.TextField("Custom Event Name", aEvent.theCustomEventName);
                                        if (newCustomEvent != aEvent.theCustomEventName) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Custom Event Name");
                                            aEvent.theCustomEventName = newCustomEvent;
                                        }
                                    }

                                    break;
                            }

                            break;
                        case MasterAudio.EventSoundFunctionType.GlobalControl:
                            EditorGUI.indentLevel = 1;
                            var newCmd = (MasterAudio.GlobalCommand)EditorGUILayout.EnumPopup("Global Cmd", aEvent.currentGlobalCommand);
                            if (newCmd != aEvent.currentGlobalCommand) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Global Command");
                                aEvent.currentGlobalCommand = newCmd;
                            }

                            if (aEvent.currentGlobalCommand == MasterAudio.GlobalCommand.None) {
                                DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                            }

                            switch (aEvent.currentGlobalCommand) {
                                case MasterAudio.GlobalCommand.SetMasterMixerVolume:
                                    if (isSliderChangedEvent) {
                                        var newSlider =
                                            (AudioEvent.TargetVolumeMode)
                                                EditorGUILayout.EnumPopup("Volume Mode", aEvent.targetVolMode);
                                        if (newSlider != aEvent.targetVolMode) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Volume Mode");
                                            aEvent.targetVolMode = newSlider;
                                        }

                                        if (aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue) {
                                            showVolumeSlider = false;
                                        }
                                    }

                                    if (showVolumeSlider) {
                                        var newFadeVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                            DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true, "Master Mixer Volume");
                                        if (newFadeVol != aEvent.volume) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Master Mixer Volume");
                                            aEvent.volume = newFadeVol;
                                        }
                                    }

                                    break;
                                case MasterAudio.GlobalCommand.SetMasterPlaylistVolume:
                                    if (isSliderChangedEvent) {
                                        var newSlider =
                                            (AudioEvent.TargetVolumeMode)
                                                EditorGUILayout.EnumPopup("Volume Mode", aEvent.targetVolMode);
                                        if (newSlider != aEvent.targetVolMode) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Volume Mode");
                                            aEvent.targetVolMode = newSlider;
                                        }

                                        if (aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue) {
                                            showVolumeSlider = false;
                                        }
                                    }

                                    if (showVolumeSlider) {
                                        var newFadeVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                            DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true, "Master Playlist Volume");
                                        if (newFadeVol != aEvent.volume) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Master Playlist Volume");
                                            aEvent.volume = newFadeVol;
                                        }
                                    }

                                    break;
                            }
                            break;
#if UNITY_5
                        case MasterAudio.EventSoundFunctionType.UnityMixerControl:
                            var newMix = (MasterAudio.UnityMixerCommand)EditorGUILayout.EnumPopup("Unity Mixer Cmd", aEvent.currentMixerCommand);
                            if (newMix != aEvent.currentMixerCommand) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Unity Mixer Cmd");
                                aEvent.currentMixerCommand = newMix;
                            }

                            EditorGUI.indentLevel = 1;

                            switch (aEvent.currentMixerCommand) {
                                case MasterAudio.UnityMixerCommand.TransitionToSnapshot:
                                    var newTime = EditorGUILayout.Slider("Transition Time", aEvent.snapshotTransitionTime, 0, 100);
                                    if (newTime != aEvent.snapshotTransitionTime) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Transition Time");
                                        aEvent.snapshotTransitionTime = newTime;
                                    }

                                    var newSnap = (AudioMixerSnapshot)EditorGUILayout.ObjectField("Snapshot", aEvent.snapshotToTransitionTo, typeof(AudioMixerSnapshot), false);
                                    if (newSnap != aEvent.snapshotToTransitionTo) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Snapshot");
                                        aEvent.snapshotToTransitionTo = newSnap;
                                    }

                                    if (aEvent.snapshotToTransitionTo == null) {
                                        DTGUIHelper.ShowRedError("No snapshot selected. No transition will be made.");
                                    }

                                    break;
                                case MasterAudio.UnityMixerCommand.TransitionToSnapshotBlend:
                                    newTime = EditorGUILayout.Slider("Transition Time", aEvent.snapshotTransitionTime, 0, 100);
                                    if (newTime != aEvent.snapshotTransitionTime) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Transition Time");
                                        aEvent.snapshotTransitionTime = newTime;
                                    }

                                    if (aEvent.snapshotsToBlend.Count == 0) {
                                        DTGUIHelper.ShowRedError("You have no snapshots to blend. This action will do nothing.");
                                    } else {
                                        EditorGUILayout.Separator();
                                    }

                                    for (var i = 0; i < aEvent.snapshotsToBlend.Count; i++) {
                                        var aSnap = aEvent.snapshotsToBlend[i];
                                        newSnap = (AudioMixerSnapshot)EditorGUILayout.ObjectField("Snapshot #" + (i + 1), aSnap.snapshot, typeof(AudioMixerSnapshot), false);
                                        if (newSnap != aSnap.snapshot) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Snapshot");
                                            aSnap.snapshot = newSnap;
                                        }

                                        if (aSnap.snapshot == null) {
                                            DTGUIHelper.ShowRedError("No snapshot selected. This item will not be used for blending.");
                                            continue;
                                        }

                                        var newWeight = EditorGUILayout.Slider("Weight", aSnap.weight, 0f, 1f);
                                        if (newWeight != aSnap.weight) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Weight");
                                            aSnap.weight = newWeight;
                                        }
                                        EditorGUILayout.Separator();
                                    }

                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(16);
                                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                                    if (GUILayout.Button(new GUIContent("Add Snapshot", "Click to add a Snapshot"), EditorStyles.toolbarButton, GUILayout.Width(85))) {
                                        aEvent.snapshotsToBlend.Add(new AudioEvent.MA_SnapshotInfo(null, 1f));
                                    }

                                    if (aEvent.snapshotsToBlend.Count > 0) {
                                        GUILayout.Space(6);
                                        GUI.contentColor = Color.red;
                                        if (DTGUIHelper.AddDeleteIcon("Snapshot", true)) {
                                            aEvent.snapshotsToBlend.RemoveAt(aEvent.snapshotsToBlend.Count - 1);
                                        }
                                    }

                                    EditorGUILayout.EndHorizontal();
                                    GUI.contentColor = Color.white;

                                    break;
                            }

                            break;
#endif
                        case MasterAudio.EventSoundFunctionType.PersistentSettingsControl:
                            EditorGUI.indentLevel = 1;

                            var newPersistentCmd = (MasterAudio.PersistentSettingsCommand)EditorGUILayout.EnumPopup("Persistent Settings Command", aEvent.currentPersistentSettingsCommand);
                            if (newPersistentCmd != aEvent.currentPersistentSettingsCommand) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Persistent Settings Command");
                                aEvent.currentPersistentSettingsCommand = newPersistentCmd;
                            }

                            switch (aEvent.currentPersistentSettingsCommand) {
                                case MasterAudio.PersistentSettingsCommand.None:
                                    DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                                    break;
                                case MasterAudio.PersistentSettingsCommand.SetBusVolume:

                                    var newAllTypes = EditorGUILayout.Toggle("Do For Every Bus?", aEvent.allSoundTypesForBusCmd);
                                    if (newAllTypes != aEvent.allSoundTypesForBusCmd) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Do For Every Bus?");
                                        aEvent.allSoundTypesForBusCmd = newAllTypes;
                                    }

                                    if (!aEvent.allSoundTypesForBusCmd) {
                                        if (_maInScene) {
                                            var existingIndex = _busNames.IndexOf(aEvent.busName);

                                            int? busIndex = null;

                                            var noBus = false;
                                            var noMatch = false;

                                            if (existingIndex >= 1) {
                                                busIndex = EditorGUILayout.Popup("Bus Name", existingIndex, _busNames.ToArray());
                                                if (existingIndex == 1) {
                                                    noBus = true;
                                                }
                                            } else if (existingIndex == -1 && aEvent.busName == MasterAudio.NoGroupName) {
                                                busIndex = EditorGUILayout.Popup("Bus Name", existingIndex, _busNames.ToArray());
                                            } else { // non-match
                                                var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                                if (newBusName != aEvent.busName) {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Name");
                                                    aEvent.busName = newBusName;
                                                }

                                                var newIndex = EditorGUILayout.Popup("All Buses", -1, _busNames.ToArray());
                                                if (newIndex >= 0) {
                                                    busIndex = newIndex;
                                                }
                                                noMatch = true;
                                            }

                                            if (noBus) {
                                                DTGUIHelper.ShowRedError("No Bus Name specified. Action will do nothing.");
                                            } else if (noMatch) {
                                                DTGUIHelper.ShowRedError("Bus Name found no match. Type in or choose one.");
                                            }

                                            if (busIndex.HasValue) {
                                                if (existingIndex != busIndex.Value) {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus");
                                                }
                                                switch (busIndex.Value) {
                                                    case -1:
                                                        aEvent.busName = MasterAudio.NoGroupName;
                                                        break;
                                                    default:
                                                        aEvent.busName = _busNames[busIndex.Value];
                                                        break;
                                                }
                                            }
                                        } else {
                                            var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                            if (newBusName != aEvent.busName) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Name");
                                                aEvent.busName = newBusName;
                                            }
                                        }
                                    }

                                    if (isSliderChangedEvent) {
                                        var newSlider =
                                            (AudioEvent.TargetVolumeMode)
                                                EditorGUILayout.EnumPopup("Volume Mode", aEvent.targetVolMode);
                                        if (newSlider != aEvent.targetVolMode) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Volume Mode");
                                            aEvent.targetVolMode = newSlider;
                                        }

                                        if (aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue) {
                                            showVolumeSlider = false;
                                        }
                                    }

                                    if (showVolumeSlider) {
                                        var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                            DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true);
                                        if (newVol != aEvent.volume) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Volume");
                                            aEvent.volume = newVol;
                                        }
                                    }

                                    break;
                                case MasterAudio.PersistentSettingsCommand.SetGroupVolume:
                                    var newAllGrps = EditorGUILayout.Toggle("Do For Every Group?", aEvent.allSoundTypesForGroupCmd);
                                    if (newAllGrps != aEvent.allSoundTypesForGroupCmd) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Do For Every Group?");
                                        aEvent.allSoundTypesForGroupCmd = newAllGrps;
                                    }

                                    if (!aEvent.allSoundTypesForGroupCmd) {
                                        if (_maInScene) {
                                            var existingIndex = _groupNames.IndexOf(aEvent.soundType);

                                            int? groupIndex = null;

                                            var noGroup = false;
                                            var noMatch = false;

                                            if (existingIndex >= 1) {
                                                groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, _groupNames.ToArray());
                                                if (existingIndex == 1) {
                                                    noGroup = true;
                                                }
                                            } else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName) {
                                                groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, _groupNames.ToArray());
                                            } else { // non-match
                                                noMatch = true;

                                                var newSType = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                                if (newSType != aEvent.soundType) {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Group");
                                                    aEvent.soundType = newSType;
                                                }
                                                var newIndex = EditorGUILayout.Popup("All Sound Groups", -1, _groupNames.ToArray());
                                                if (newIndex >= 0) {
                                                    groupIndex = newIndex;
                                                }
                                            }

                                            if (noMatch) {
                                                DTGUIHelper.ShowRedError("Sound Group found no match. Type in or choose one.");
                                            } else if (noGroup) {
                                                DTGUIHelper.ShowRedError("No Sound Group specified. Action will do nothing.");
                                            }

                                            if (groupIndex.HasValue) {
                                                if (existingIndex != groupIndex.Value) {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Group");
                                                }
                                                switch (groupIndex.Value) {
                                                    case -1:
                                                        aEvent.soundType = MasterAudio.NoGroupName;
                                                        break;
                                                    default:
                                                        aEvent.soundType = _groupNames[groupIndex.Value];
                                                        break;
                                                }
                                            }
                                        } else {
                                            var newSoundT = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                            if (newSoundT != aEvent.soundType) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Group");
                                                aEvent.soundType = newSoundT;
                                            }
                                        }
                                    }

                                    if (isSliderChangedEvent) {
                                        var newSlider =
                                            (AudioEvent.TargetVolumeMode)
                                                EditorGUILayout.EnumPopup("Volume Mode", aEvent.targetVolMode);
                                        if (newSlider != aEvent.targetVolMode) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Volume Mode");
                                            aEvent.targetVolMode = newSlider;
                                        }

                                        if (aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue) {
                                            showVolumeSlider = false;
                                        }
                                    }

                                    if (showVolumeSlider) {
                                        var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                            DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true);
                                        if (newVol != aEvent.volume) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Volume");
                                            aEvent.volume = newVol;
                                        }
                                    }

                                    break;
                                case MasterAudio.PersistentSettingsCommand.SetMixerVolume:
                                    if (isSliderChangedEvent) {
                                        var newSlider =
                                            (AudioEvent.TargetVolumeMode)
                                                EditorGUILayout.EnumPopup("Volume Mode", aEvent.targetVolMode);
                                        if (newSlider != aEvent.targetVolMode) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Volume Mode");
                                            aEvent.targetVolMode = newSlider;
                                        }

                                        if (aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue) {
                                            showVolumeSlider = false;
                                        }
                                    }

                                    if (showVolumeSlider) {
                                        var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                            DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true, "Mixer Volume");
                                        if (newVol != aEvent.volume) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Mixer Volume");
                                            aEvent.volume = newVol;
                                        }
                                    }

                                    break;
                                case MasterAudio.PersistentSettingsCommand.SetMusicVolume:
                                    if (isSliderChangedEvent) {
                                        var newSlider =
                                            (AudioEvent.TargetVolumeMode)
                                                EditorGUILayout.EnumPopup("Volume Mode", aEvent.targetVolMode);
                                        if (newSlider != aEvent.targetVolMode) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                                "change Volume Mode");
                                            aEvent.targetVolMode = newSlider;
                                        }

                                        if (aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue) {
                                            showVolumeSlider = false;
                                        }
                                    }

                                    if (showVolumeSlider) {
                                        var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                            DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true, "Music Volume");
                                        if (newVol != aEvent.volume) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Music Volume");
                                            aEvent.volume = newVol;
                                        }
                                    }

                                    break;
                            }

                            break;
                    }

                    EditorGUI.indentLevel = 0;
                }

                switch (buttonPressed) {
                    case DTGUIHelper.DTFunctionButtons.Add:
                        indexToInsert = j + 1;
                        break;
                    case DTGUIHelper.DTFunctionButtons.Remove:
                        indexToRemove = j;
                        break;
                    case DTGUIHelper.DTFunctionButtons.ShiftUp:
                        indexToShiftUp = j;
                        break;
                    case DTGUIHelper.DTFunctionButtons.ShiftDown:
                        indexToShiftDown = j;
                        break;
                }

                prevEvent = aEvent;

                EditorGUILayout.EndVertical();
            }
        }

        AudioEvent item = null;

        if (indexToInsert.HasValue) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Add action");
            eventGrp.SoundEvents.Insert(indexToInsert.Value, new AudioEvent());
        } else if (indexToRemove.HasValue) {
            if (eventGrp.SoundEvents.Count <= 1) {
                DTGUIHelper.ShowAlert("You cannot delete the last Action. Delete this event trigger if you don't need it.");
            } else {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Delete action");
                eventGrp.SoundEvents.RemoveAt(indexToRemove.Value);
            }
        } else if (indexToShiftUp.HasValue) {
            item = eventGrp.SoundEvents[indexToShiftUp.Value];

            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Shift up event action");

            eventGrp.SoundEvents.Insert(indexToShiftUp.Value - 1, item);
            eventGrp.SoundEvents.RemoveAt(indexToShiftUp.Value + 1);
        } else if (indexToShiftDown.HasValue) {
            var index = indexToShiftDown.Value + 1;
            item = eventGrp.SoundEvents[index];

            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Shift down event action");

            eventGrp.SoundEvents.Insert(index - 1, item);
            eventGrp.SoundEvents.RemoveAt(index + 1);
        }

        DTGUIHelper.EndGroupedControls();

        return _isDirty;
    }

    private void CreateMechanimStateEntered(bool recordUndo) {
        var newEvent = new AudioEvent();

        if (recordUndo) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "add Mechanim State Entered Sound");
        }

        var newGrp = new AudioEventGroup { isMechanimStateCheckEvent = true, mechanimEventActive = true };

        newGrp.SoundEvents.Add(newEvent);
        _sounds.mechanimStateChangedSounds.Add(newGrp);
    }

    private void CreateCustomEvent(bool recordUndo) {
        var newEvent = new AudioEvent();

        if (recordUndo) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "add Custom Event Sound");
        }

        var newGrp = new AudioEventGroup { isCustomEvent = true, customSoundActive = true };

        newGrp.SoundEvents.Add(newEvent);
        _sounds.userDefinedSounds.Add(newGrp);
    }

    private static void AddEventIfZero(AudioEventGroup grp) {
        if (grp.SoundEvents.Count == 0) {
            grp.SoundEvents.Add(new AudioEvent());
        }
    }

    private string DisabledText {
        get {
            var disabledText = "";
            if (_sounds.disableSounds) {
                disabledText = " (DISABLED) ";
            }

            return disabledText;
        }
    }

    private void SortCustomEventTriggers() {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Alpha Sort Custom Event Triggers");

        _sounds.userDefinedSounds.Sort(delegate(AudioEventGroup x, AudioEventGroup y) {
            return x.customEventName.CompareTo(y.customEventName);
        });
    }
}
