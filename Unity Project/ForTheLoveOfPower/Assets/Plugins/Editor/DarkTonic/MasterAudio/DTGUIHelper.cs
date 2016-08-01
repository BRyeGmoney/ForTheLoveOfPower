using System;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
// ReSharper disable once InconsistentNaming
public static class DTGUIHelper {
    public const float MinDb = -90;
    public const float MaxDb = 0f;

    public static readonly string UpArrow = '\u25B2'.ToString();
    public static readonly string DownArrow = '\u25BC'.ToString();

    // ReSharper disable InconsistentNaming
    // COLORS FOR DARK SCHEME
    private static readonly Color DarkSkin_OuterGroupBoxColor = new Color(.7f, 1f, 1f);
    private static readonly Color DarkSkin_SecondaryHeaderColor = new Color(.8f, .8f, .8f);
    private static readonly Color DarkSkin_GroupBoxColor = new Color(.6f, .6f, .6f);
    private static readonly Color DarkSkin_SecondaryGroupBoxColor = new Color(.5f, .8f, 1f);
    private static readonly Color DarkSkin_BrightButtonColor = Color.cyan;
    private static readonly Color DarkSkin_BrightTextColor = Color.yellow;
    private static readonly Color DarkSkin_DragAreaColor = Color.yellow;
    private static readonly Color DarkSkin_InactiveHeaderColor = new Color(.6f, .6f, .6f);
    private static readonly Color DarkSkin_ActiveHeaderColor = new Color(.3f, .8f, 1f);
    private static readonly Color DarkSkin_HelpIconColor = new Color(.2f, 1f, .2f);

    // COLORS FOR LIGHT SCHEME
    private static readonly Color LightSkin_OuterGroupBoxColor = Color.white;
    private static readonly Color LightSkin_SecondaryHeaderColor = Color.white;
    private static readonly Color LightSkin_GroupBoxColor = new Color(.7f, .7f, .8f);
    private static readonly Color LightSkin_SecondaryGroupBoxColor = new Color(.6f, 1f, 1f);
    private static readonly Color LightSkin_BrightButtonColor = new Color(0f, 1f, 1f);
    private static readonly Color LightSkin_BrightTextColor = Color.yellow;
    private static readonly Color LightSkin_DragAreaColor = new Color(1f, 1f, .3f);
    private static readonly Color LightSkin_InactiveHeaderColor = new Color(.6f, .6f, .6f);
    private static readonly Color LightSkin_ActiveHeaderColor = new Color(.3f, .8f, 1f);
    private static readonly Color LightSkin_HelpIconColor = Color.green;
    // ReSharper restore InconsistentNaming

    private static List<string> _layers;
    private static string[] _layerNames;

    private const string AlertTitle = "Master Audio Alert";
    private const string AlertOkText = "Ok";
    private const string FoldOutTooltip = "Click to expand or collapse";
    private const string DbText = " (dB)";
    private const float LedFrameTime = .07f;

    private const int WideModeWidth = 290;
    private const int NormalModeWidth = 60;
    private const int NormalBusWidth = 84;

    public enum JukeboxButtons {
        None,
        NextSong,
        Pause,
        Play,
        RandomSong,
        Stop
    }

    public enum DTFunctionButtons {
        None,
        Add,
        Remove,
        Mute,
        Solo,
        Go,
        ShiftUp,
        ShiftDown,
        Play,
        Stop,
        Rename,
        Clone,
        Find,
        Pause
    }

    public static LayerMask LayerMaskField(string label, LayerMask selected) {
        if (_layers == null) {
            _layers = new List<string>();
            _layerNames = new string[4];
        } else {
            _layers.Clear();
        }

        var emptyLayers = 0;
        for (var i = 0; i < 32; i++) {
            var layerName = LayerMask.LayerToName(i);

            if (layerName != "") {

                for (; emptyLayers > 0; emptyLayers--) {
                    _layers.Add("Layer " + (i - emptyLayers));
                }

                _layers.Add(layerName);
            } else {
                emptyLayers++;
            }
        }

        if (_layerNames.Length != _layers.Count) {
            _layerNames = new string[_layers.Count];
        }
        for (var i = 0; i < _layerNames.Length; i++) {
            _layerNames[i] = _layers[i];
        }

        selected.value = EditorGUILayout.MaskField(label, selected.value, _layerNames);

        return selected;
    }

    public static void ResetColors() {
        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
    }

    private static bool IsDarkSkin {
        get {
            return EditorPrefs.GetInt("UserSkin") == 1;
        }
    }

    public static Color InactiveMixerGroupColor {
        get {
            return new Color(.5f, .5f, .5f);
        }
    }

    public static Color InactiveHeaderColor {
        get {
            return IsDarkSkin ? DarkSkin_InactiveHeaderColor : LightSkin_InactiveHeaderColor;
        }
    }

    public static Color ActiveHeaderColor {
        get {
            return IsDarkSkin ? DarkSkin_ActiveHeaderColor : LightSkin_ActiveHeaderColor;
        }
    }

    public static Color DragAreaColor {
        get {
            return IsDarkSkin ? DarkSkin_DragAreaColor : LightSkin_DragAreaColor;
        }
    }

    public static Color BrightButtonColor {
        get {
            return IsDarkSkin ? DarkSkin_BrightButtonColor : LightSkin_BrightButtonColor;
        }
    }

    public static Color BrightTextColor {
        get {
            return IsDarkSkin ? DarkSkin_BrightTextColor : LightSkin_BrightTextColor;
        }
    }

    private static Color GroupBoxColor {
        get {
            return IsDarkSkin ? DarkSkin_GroupBoxColor : LightSkin_GroupBoxColor;
        }
    }

    private static Color SecondaryHeaderColor {
        get {
            return IsDarkSkin ? DarkSkin_SecondaryHeaderColor : LightSkin_SecondaryHeaderColor;
        }
    }

    private static Color HelpIconColor {
        get {
            return IsDarkSkin ? DarkSkin_HelpIconColor : LightSkin_HelpIconColor;
        }
    }

    private static Color OuterGroupBoxColor {
        get {
            return IsDarkSkin ? DarkSkin_OuterGroupBoxColor : LightSkin_OuterGroupBoxColor;
        }
    }

    private static Color SecondaryGroupBoxColor {
        get {
            return IsDarkSkin ? DarkSkin_SecondaryGroupBoxColor : LightSkin_SecondaryGroupBoxColor;
        }
    }

    public static GUIStyle CornerGUIStyle {
        get {
#if UNITY_5
            return EditorStyles.helpBox;
#else
            return EditorStyles.textArea;
#endif
        }

    }

    public static void AddSpaceForNonU5(int height) {
#if UNITY_5
        //
#else
        GUILayout.Space(height);
#endif
    }
    public static void ShowHeaderTexture(Texture tex) {
        if (MasterAudio.HideLogoNav) {
            return;
        }

        var rect = GUILayoutUtility.GetRect(0f, 0f);
        rect.width = tex.width;
        rect.height = tex.height;
        GUILayout.Space(rect.height);
        GUI.DrawTexture(rect, tex);

        var e = Event.current;
        if (e.type != EventType.MouseUp) {
            return;
        }
        if (!rect.Contains(e.mousePosition)) {
            return;
        }
        var ma = MasterAudio.Instance;
        if (ma != null) {
            Selection.activeObject = ma.gameObject;
        }
    }

    public static void HelpHeader(string helpUrl, string apiUrl = "https://dl.dropboxusercontent.com/u/40293802/DarkTonic/MasterAudio_API/annotated.html") {
        EditorGUILayout.BeginHorizontal(CornerGUIStyle);
        AddHelpIcon(helpUrl, true);
        GUILayout.Label("Click button for online help!");
        AddAPIIcon(apiUrl);
        EditorGUILayout.EndHorizontal();
    }

    public static void StartGroupHeader(int level = 0, bool showBoth = true) {
        switch (level) {
            case 0:
                GUI.backgroundColor = GroupBoxColor;
                break;
            case 1:
                GUI.backgroundColor = SecondaryGroupBoxColor;
                break;
        }

        EditorGUILayout.BeginVertical(CornerGUIStyle);

        if (!showBoth) {
            return;
        }

        switch (level) {
            case 0:
                GUI.backgroundColor = SecondaryHeaderColor;
                break;
        }

        EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb);
    }

    public static void EndGroupHeader() {
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }

    public static void VerticalSpace(int pixels) {
        EditorGUILayout.BeginVertical();
        GUILayout.Space(pixels);
        EditorGUILayout.EndVertical();
    }

    public static void AddHelpIcon(string helpUrl, bool useMiddleButton = false) {
        var oldColor = GUI.color;
        var oldBG = GUI.backgroundColor;
        GUI.color = HelpIconColor;
        GUI.backgroundColor = Color.white;
        var buttonStyle = EditorStyles.miniButtonRight;
        if (useMiddleButton) {
            buttonStyle = EditorStyles.miniButton;
        }

        if (GUILayout.Button(new GUIContent("?", "Online Help"), buttonStyle, GUILayout.MaxWidth(16), GUILayout.Height(15))) {
            Application.OpenURL(helpUrl);
        }
        GUILayout.Space(3);
        GUI.color = oldColor;
        GUI.backgroundColor = oldBG;
    }

    public static void AddAPIIcon(string apiUrl) {
        var oldColor = GUI.color;
        var oldBG = GUI.backgroundColor;
        GUI.color = HelpIconColor;
        GUI.backgroundColor = Color.white;
        var buttonStyle = EditorStyles.miniButton;

        if (GUILayout.Button(new GUIContent("API", "Online Coding API Guide"), buttonStyle, GUILayout.MaxWidth(32), GUILayout.Height(15))) {
            Application.OpenURL(apiUrl);
        }
        GUILayout.Space(3);
        GUI.color = oldColor;
        GUI.backgroundColor = oldBG;
    }

    public static DTFunctionButtons AddDeleteIcon(bool showRenameButton, string eventName) {
        GUI.backgroundColor = Color.white;
        GUI.color = Color.white;
        GUI.contentColor = BrightButtonColor;

        var shouldRename = false;
        if (showRenameButton) {
            shouldRename = GUILayout.Button(new GUIContent("Rename", "Click to rename " + eventName), EditorStyles.toolbarButton, GUILayout.MaxWidth(50));
        }

        var deleteIcon = MasterAudioInspectorResources.DeleteTexture;
        GUI.contentColor = Color.red;
        var shouldDelete = GUILayout.Button(new GUIContent(deleteIcon, "Click to delete " + eventName), EditorStyles.toolbarButton, GUILayout.MaxWidth(32), GUILayout.Height(15));
        GUI.contentColor = Color.white;

        if (shouldDelete) {
            return DTFunctionButtons.Remove;
        }
        if (shouldRename) {
            return DTFunctionButtons.Rename;
        }

        return DTFunctionButtons.None;
    }

    public static bool AddDeleteIcon(string itemName, bool showLastText = false) {
        if (showLastText) {
            itemName = "last " + itemName;
        }

        var deleteIcon = MasterAudioInspectorResources.DeleteTexture;
        return GUILayout.Button(new GUIContent(deleteIcon, "Click to delete " + itemName), EditorStyles.toolbarButton, GUILayout.MaxWidth(30));
    }

    public static JukeboxButtons AddJukeboxIcons() {
        var buttonPressed = JukeboxButtons.None;

        var stopIcon = MasterAudioInspectorResources.StopTexture;
        var stopContent = stopIcon == null ? new GUIContent("Stop", "Stop Playlist") : new GUIContent(stopIcon, "Stop Playlist");
        var buttonWidth = stopIcon == null ? 50 : 30;
        if (GUILayout.Button(stopContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(buttonWidth))) {
            buttonPressed = JukeboxButtons.Stop;
        }

        var pauseIcon = MasterAudioInspectorResources.PauseTexture;
        var pauseContent = pauseIcon == null ? new GUIContent("Pause", "Pause Playlist") : new GUIContent(pauseIcon, "Pause Playlist");
        buttonWidth = pauseIcon == null ? 50 : 30;
        if (GUILayout.Button(pauseContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(buttonWidth))) {
            buttonPressed = JukeboxButtons.Pause;
        }

        var playIcon = MasterAudioInspectorResources.PlaySongTexture;
        var playContent = playIcon == null ? new GUIContent("Play", "Play Playlist") : new GUIContent(playIcon, "Play Playlist");
        buttonWidth = playIcon == null ? 50 : 30;
        if (GUILayout.Button(playContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(buttonWidth))) {
            buttonPressed = JukeboxButtons.Play;
        }

        var nextTrackIcon = MasterAudioInspectorResources.NextTrackTexture;
        var nextContent = nextTrackIcon == null ? new GUIContent("Next", "Next Track in Playlist") : new GUIContent(nextTrackIcon, "Next Track in Playlist");
        buttonWidth = nextTrackIcon == null ? 50 : 30;
        if (GUILayout.Button(nextContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(buttonWidth))) {
            buttonPressed = JukeboxButtons.NextSong;
        }

        GUILayout.Space(10);

        var randomIcon = MasterAudioInspectorResources.RandomTrackTexture;
        var randomContent = randomIcon == null ? new GUIContent("Random", "Random Track in Playlist") : new GUIContent(randomIcon, "Random Track in Playlist");
        buttonWidth = randomIcon == null ? 50 : 30;
        if (GUILayout.Button(randomContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(buttonWidth))) {
            buttonPressed = JukeboxButtons.RandomSong;
        }

        if (!Application.isPlaying) {
            buttonPressed = JukeboxButtons.None;
        }

        return buttonPressed;
    }

    public static DTFunctionButtons AddDynamicGroupButtons(Object obj) {
        GUIContent deleteIcon = null;
        GUIContent settingsIcon;

        var isProjectView = IsPrefabInProjectView(obj);

        if (!isProjectView) {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (MasterAudioInspectorResources.DeleteTexture != null) {
                deleteIcon = new GUIContent(MasterAudioInspectorResources.DeleteTexture, "Click to delete Group");
            } else {
                deleteIcon = new GUIContent("Delete", "Click to delete Group");
            }
        }

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (MasterAudioInspectorResources.GearTexture != null) {
            settingsIcon = new GUIContent(MasterAudioInspectorResources.GearTexture, "Click to edit Group");
        } else {
            settingsIcon = new GUIContent("Edit", "Click to edit Group");
        }

        if (GUILayout.Button(settingsIcon, EditorStyles.toolbarButton)) {
            return DTFunctionButtons.Go;
        }

        if (!Application.isPlaying && !isProjectView) {
            GUIContent previewIcon;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (MasterAudioInspectorResources.PreviewTexture != null) {
                previewIcon = new GUIContent(MasterAudioInspectorResources.PreviewTexture, "Click to preview Group");
            } else {
                previewIcon = new GUIContent("Preview", "Click to preview Group");
            }

            GUIContent stopPreviewIcon;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (MasterAudioInspectorResources.StopTexture != null) {
                stopPreviewIcon = new GUIContent(MasterAudioInspectorResources.StopTexture, "Click to stop previewing Group");
            } else {
                stopPreviewIcon = new GUIContent("End Preview", "Click to stop previewing Group");
            }

            if (GUILayout.Button(previewIcon, EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Play;
            }

            if (GUILayout.Button(stopPreviewIcon, EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Stop;
            }
        } else {
            GUILayout.Space(76);
        }

        if (Application.isPlaying || deleteIcon == null) {
            return DTFunctionButtons.None;
        }
        if (GUILayout.Button(deleteIcon, EditorStyles.toolbarButton)) {
            return DTFunctionButtons.Remove;
        }

        return DTFunctionButtons.None;
    }

    public static DTFunctionButtons AddMixerBusButtons(GroupBus gb) {
        var deleteIcon = MasterAudioInspectorResources.DeleteTexture;
        var stopIcon = MasterAudioInspectorResources.StopTexture;

        var muteContent = new GUIContent(MasterAudioInspectorResources.MuteOffTexture, "Click to mute bus");

        if (gb.isMuted) {
            muteContent.image = MasterAudioInspectorResources.MuteOnTexture;
        }

        var soloContent = new GUIContent(MasterAudioInspectorResources.SoloOffTexture, "Click to solo bus");

        if (gb.isSoloed) {
            soloContent.image = MasterAudioInspectorResources.SoloOnTexture;
        }

        var soloPressed = GUILayout.Button(soloContent, EditorStyles.toolbarButton);
        var mutePressed = GUILayout.Button(muteContent, EditorStyles.toolbarButton);

        if (ShowFindUsages("Bus '" + gb.busName + "'")) {
            return DTFunctionButtons.Find;
        }

        var stopPressed = false;

        if (Application.isPlaying) {
            stopPressed = GUILayout.Button(new GUIContent(stopIcon, "Click to stop bus"), EditorStyles.toolbarButton);
        }

        var removePressed = GUILayout.Button(new GUIContent(deleteIcon, "Click to delete bus"), EditorStyles.toolbarButton);

        // Return the pressed button if any
        if (removePressed) {
            return DTFunctionButtons.Remove;
        }
        if (soloPressed) {
            return DTFunctionButtons.Solo;
        }
        if (mutePressed) {
            return DTFunctionButtons.Mute;
        }
        if (stopPressed) {
            return DTFunctionButtons.Stop;
        }

        return DTFunctionButtons.None;
    }

    public static DTFunctionButtons AddDynamicVariationButtons() {
        if (Application.isPlaying) {
            return DTFunctionButtons.None;
        }

        if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.PreviewTexture, "Click to preview Variation"), EditorStyles.toolbarButton, GUILayout.Width(40))) {
            return DTFunctionButtons.Play;
        }

        if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.StopTexture, "Click to stop audio preview"), EditorStyles.toolbarButton, GUILayout.Width(40))) {
            return DTFunctionButtons.Stop;
        }

        return DTFunctionButtons.None;
    }

    public static DTFunctionButtons AddDynamicGroupButtons(DynamicSoundGroup grp) {
        if (Application.isPlaying) {
            return DTFunctionButtons.None;
        }

        // ReSharper disable once InvertIf
        if (!IsPrefabInProjectView(grp)) {
            if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.PreviewTexture, "Click to preview Variation"), EditorStyles.toolbarButton, GUILayout.Height(16), GUILayout.Width(40))) {
                return DTFunctionButtons.Play;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (
                GUILayout.Button(
                    new GUIContent(MasterAudioInspectorResources.StopTexture, "Click to stop audio preview"),
                    EditorStyles.toolbarButton, GUILayout.Height(16), GUILayout.Width(40))) {
                return DTFunctionButtons.Stop;
            }
        }

        return DTFunctionButtons.None;
    }

    public static DTFunctionButtons AddVariationButtons() {
        if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.PreviewTexture, "Click to preview Variation"), EditorStyles.toolbarButton, GUILayout.Height(16), GUILayout.Width(40))) {
            return DTFunctionButtons.Play;
        }

        if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.StopTexture, "Click to stop audio preview"), EditorStyles.toolbarButton, GUILayout.Height(16), GUILayout.Width(40))) {
            return DTFunctionButtons.Stop;
        }

        return DTFunctionButtons.None;
    }

    public static string DisplayVolumeNumber(float vol, int totalChars) {
        if (MasterAudio.UseDbScaleForVolume) {
            var v = AudioUtil.GetDbFromFloatVolume(vol).ToString("N1") + " dB";
            while (v.Length < totalChars) {
                v = " " + v;
            }
            return v;
        } else {
            return "V " + vol.ToString("N2");
        }
    }

    public enum VolumeFieldType {
        None,
        MixerGroup,
        Bus,
        PlaylistController,
        DynamicMixerGroup,
        GlobalVolume
    }

    public static float DisplayPitchField(float pitch, string fieldName = "Pitch") {
        if (!MasterAudio.UseCentsForPitch) {
            return EditorGUILayout.Slider(fieldName, pitch, -3f, 3f);
        }

        float pitchSemiTones;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (pitch == 1) {
            pitchSemiTones = 0;
        } else {
            pitchSemiTones = AudioUtil.GetSemitonesFromPitch(pitch);
        }

        var newSemi = EditorGUILayout.Slider("Pitch Chg. Semitones", pitchSemiTones, -24f, 19f);
        var newPitch = AudioUtil.GetPitchFromSemitones(newSemi);

        return newPitch;
    }

    public static MasterAudio.AudioLocation GetRestrictedAudioLocation(string label, MasterAudio.AudioLocation currentLoc) {
        var options = new List<GUIContent>();

        var iEnum = Enum.GetNames(typeof(MasterAudio.AudioLocation)).GetEnumerator();

        var selIndex = 0;
        var i = 0;
        while (iEnum.MoveNext()) {
            // ReSharper disable once PossibleNullReferenceException
            if (iEnum.Current.ToString() == currentLoc.ToString()) {
                selIndex = i;
            }

            i++;

            if (iEnum.Current.ToString() == MasterAudio.AudioLocation.FileOnInternet.ToString()) {
                continue;
            }

            options.Add(new GUIContent(iEnum.Current.ToString(), iEnum.Current.ToString()));
        }

        var newIndex = EditorGUILayout.Popup(new GUIContent(label, label), selIndex, options.ToArray());
        var selEnum = (MasterAudio.AudioLocation)Enum.Parse(typeof(MasterAudio.AudioLocation), options[newIndex].text);

        return selEnum;
    }

    public static float DisplayVolumeField(float vol, VolumeFieldType fieldType, MasterAudio.MixerWidthMode widthMode, float volumeMin = 0f, bool showFieldName = false, string fieldName = "Volume") {
        var wideMode = widthMode == MasterAudio.MixerWidthMode.Wide;
        var narrowMode = widthMode == MasterAudio.MixerWidthMode.Narrow;

        var forceToNonDb = MasterAudio.UseDbScaleForVolume && vol < 0;

        if (!MasterAudio.UseDbScaleForVolume || forceToNonDb) {
            switch (fieldType) {
                case VolumeFieldType.MixerGroup:
                    return GUILayout.HorizontalSlider(vol, 0f, 1f, GUILayout.Width(wideMode ? WideModeWidth : NormalModeWidth));
                case VolumeFieldType.DynamicMixerGroup:
                    return GUILayout.HorizontalSlider(vol, 0f, 1f, GUILayout.Width(100));
                case VolumeFieldType.Bus:
                    var width = wideMode ? WideModeWidth : NormalBusWidth;
                    if (narrowMode) {
                        width = NormalModeWidth;
                    }

                    return GUILayout.HorizontalSlider(vol, 0f, 1f, GUILayout.Width(width));
                case VolumeFieldType.PlaylistController:
                    var wid = 74;
                    if (narrowMode) {
                        wid = NormalModeWidth;
                    }
                    return GUILayout.HorizontalSlider(vol, 0f, 1f, GUILayout.Width(wid));
                case VolumeFieldType.None:
                    if (showFieldName) {
                        return EditorGUILayout.Slider(fieldName, vol, volumeMin, 1f);
                    }

                    return EditorGUILayout.Slider(vol, volumeMin, 1f, narrowMode ? GUILayout.Width(115) : GUILayout.Width(252));
                case VolumeFieldType.GlobalVolume:
                    if (showFieldName) {
                        return EditorGUILayout.Slider(fieldName, vol, volumeMin, 1f);
                    }

                    var newVol = GUILayout.HorizontalSlider(vol, volumeMin, 1f, narrowMode ? GUILayout.Width(61) : GUILayout.Width(198));
                    var newVol2 = EditorGUILayout.FloatField(vol, GUILayout.Width(50));
                    if (newVol > 1) {
                        newVol = 1;
                    }
                    if (newVol < 0) {
                        newVol = 0;
                    }
                    if (newVol != vol) {
                        return newVol;
                    }
                    return newVol2;
            }
        }

        var dbLevel = (float)Math.Round(AudioUtil.GetDbFromFloatVolume(vol), 1);

        var newDbLevel = 0f;
        switch (fieldType) {
            case VolumeFieldType.MixerGroup:
                newDbLevel = GUILayout.HorizontalSlider(dbLevel, MinDb, MaxDb, GUILayout.Width(wideMode ? WideModeWidth : NormalModeWidth));
                break;
            case VolumeFieldType.DynamicMixerGroup:
                newDbLevel = GUILayout.HorizontalSlider(dbLevel, MinDb, MaxDb, GUILayout.Width(100));
                break;
            case VolumeFieldType.Bus:
                var sliderWidth = wideMode ? WideModeWidth : NormalBusWidth;
                if (narrowMode) {
                    sliderWidth = NormalModeWidth;
                }

                newDbLevel = GUILayout.HorizontalSlider(dbLevel, MinDb, MaxDb, GUILayout.Width(sliderWidth));
                break;
            case VolumeFieldType.PlaylistController:
                var sliderWid = 74;
                if (narrowMode) {
                    sliderWid = NormalModeWidth;
                }
                newDbLevel = GUILayout.HorizontalSlider(dbLevel, MinDb, MaxDb, GUILayout.Width(sliderWid));
                break;
            case VolumeFieldType.None:
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (showFieldName) {
                    newDbLevel = EditorGUILayout.Slider(fieldName + DbText, dbLevel, MinDb, MaxDb);
                } else {
                    newDbLevel = EditorGUILayout.Slider(dbLevel, MinDb, MaxDb, narrowMode ? GUILayout.Width(115) : GUILayout.Width(252));
                }
                break;
            case VolumeFieldType.GlobalVolume:
                if (showFieldName) {
                    return EditorGUILayout.Slider(fieldName + DbText, dbLevel, MinDb, MaxDb);
                }

                newDbLevel = GUILayout.HorizontalSlider(dbLevel, MinDb, MaxDb, narrowMode ? GUILayout.Width(61) : GUILayout.Width(198));
                newDbLevel = EditorGUILayout.FloatField(newDbLevel, GUILayout.Width(50));
                if (newDbLevel > MaxDb) {
                    newDbLevel = MaxDb;
                }
                if (newDbLevel < MinDb) {
                    newDbLevel = MinDb;
                }
                break;
        }

        return AudioUtil.GetFloatVolumeFromDb(newDbLevel);
    }

    public static string LabelVolumeField(string fieldName) {
        if (MasterAudio.UseDbScaleForVolume) {
            return fieldName + DbText;
        }

        return fieldName;
    }

    public static void BeginGroupedControls() {
        GUI.backgroundColor = OuterGroupBoxColor;
        GUILayout.BeginHorizontal();
        EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }

    public static void EndGroupedControls() {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(3f);
        GUILayout.EndHorizontal();

        GUILayout.Space(3f);
    }

    public static DTFunctionButtons AddMasterMixerButtons(string itemName, MasterAudio sounds) {
        var muteContent = new GUIContent(MasterAudioInspectorResources.MuteOffTexture, "Click to mute " + itemName);

        if (sounds.mixerMuted) {
            muteContent.image = MasterAudioInspectorResources.MuteOnTexture;
        }

        if (GUILayout.Button(muteContent, EditorStyles.toolbarButton)) {
            return DTFunctionButtons.Mute;
        }

        if (Application.isPlaying) {
            var stopContent = new GUIContent(MasterAudioInspectorResources.StopTexture, "Click to stop " + itemName);
            if (GUILayout.Button(stopContent, EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Stop;
            }
            var pauseContent = new GUIContent(MasterAudioInspectorResources.PauseTexture, "Click to pause " + itemName);
            if (GUILayout.Button(pauseContent, EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Pause;
            }
            var playContent = new GUIContent(MasterAudioInspectorResources.PlaySongTexture, "Click to unpause " + itemName);
            if (GUILayout.Button(playContent, EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Play;
            }
        }

        return DTFunctionButtons.None;
    }

    public static DTFunctionButtons AddMasterPlaylistButtons(string itemName, MasterAudio sounds) {
        var muteContent = new GUIContent(MasterAudioInspectorResources.MuteOffTexture, "Click to mute " + itemName);

        if (sounds.playlistsMuted) {
            muteContent.image = MasterAudioInspectorResources.MuteOnTexture;
        }

        if (GUILayout.Button(muteContent, EditorStyles.toolbarButton)) {
            return DTFunctionButtons.Mute;
        }

        if (Application.isPlaying) {
            var stopContent = new GUIContent(MasterAudioInspectorResources.StopTexture, "Click to stop " + itemName);
            if (GUILayout.Button(stopContent, EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Stop;
            }
            var pauseContent = new GUIContent(MasterAudioInspectorResources.PauseTexture, "Click to pause " + itemName);
            if (GUILayout.Button(pauseContent, EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Pause;
            }
            var playContent = new GUIContent(MasterAudioInspectorResources.PlaySongTexture, "Click to unpause " + itemName);
            if (GUILayout.Button(playContent, EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Play;
            }
        }

        return DTFunctionButtons.None;
    }

    public static void AddLedSignalLight(MasterAudio sounds, string groupName) {
        var content = new GUIContent(MasterAudioInspectorResources.LedTextures[MasterAudioInspectorResources.LedTextures.Length - 1]);

        if (Application.isPlaying) {
            var groupInfo = MasterAudio.GetGroupInfo(groupName);
            if (groupInfo != null && !groupInfo.PlayedForWarming && groupInfo.LastTimePlayed > 0f && groupInfo.LastTimePlayed <= Time.realtimeSinceStartup) {
                var timeDiff = Time.realtimeSinceStartup - groupInfo.LastTimePlayed;

                var timeSlot = (int)(timeDiff / LedFrameTime);

                if (timeSlot >= 4 && timeSlot < 5) {
                    content = new GUIContent(MasterAudioInspectorResources.LedTextures[4]);
                } else if (timeSlot >= 3 && timeSlot < 4) {
                    content = new GUIContent(MasterAudioInspectorResources.LedTextures[3]);
                } else if (timeSlot >= 2 && timeSlot < 3) {
                    content = new GUIContent(MasterAudioInspectorResources.LedTextures[2]);
                } else if (timeSlot >= 1 && timeSlot < 2) {
                    content = new GUIContent(MasterAudioInspectorResources.LedTextures[1]);
                } else if (timeSlot >= 0 && timeSlot < 1f) {
                    content = new GUIContent(MasterAudioInspectorResources.LedTextures[0]);
                }
            }
        }

        GUILayout.Label(content, EditorStyles.toolbarButton, GUILayout.Width(26));
    }

    public static DTFunctionButtons AddGroupButtons(MasterAudioGroup aGroup, string itemName) {
        var muteContent = new GUIContent(MasterAudioInspectorResources.MuteOffTexture, "Click to mute " + itemName);

        if (aGroup.isMuted) {
            muteContent.image = MasterAudioInspectorResources.MuteOnTexture;
        }

        var soloContent = new GUIContent(MasterAudioInspectorResources.SoloOffTexture, "Click to solo " + itemName);

        if (aGroup.isSoloed) {
            soloContent.image = MasterAudioInspectorResources.SoloOnTexture;
        }

        if (GUILayout.Button(soloContent, EditorStyles.toolbarButton, GUILayout.Width(32))) {
            return DTFunctionButtons.Solo;
        }
        if (GUILayout.Button(muteContent, EditorStyles.toolbarButton, GUILayout.Width(32))) {
            return DTFunctionButtons.Mute;
        }

        return DTFunctionButtons.None;
    }

    public static DTFunctionButtons AddSettingsButton(string itemName) {
        var settingsIcon = MasterAudioInspectorResources.GearTexture;

        if (GUILayout.Button(new GUIContent(settingsIcon, "Click to edit " + itemName), EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(20))) {
            return DTFunctionButtons.Go;
        }

        return DTFunctionButtons.None;
    }

    public static bool ShowFindUsages(string itemName) {
        if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.FindTexture, string.Format("Click to show all usages of {0} in Scene", itemName)),
            EditorStyles.toolbarButton, GUILayout.Width(24))) {

            return true;
        }

        return false;
    }

    public static void ShowFilteredRelationsGraph(string groupFilter = null, string busFilter = null) {
        if (string.IsNullOrEmpty(groupFilter)) {
            groupFilter = MasterAudioEventBackend.AllBusesOrGroupsWord;
        }
        if (string.IsNullOrEmpty(busFilter)) {
            busFilter = MasterAudioEventBackend.AllBusesOrGroupsWord;
        }

        MasterAudioEventBackend.GroupFilter = groupFilter;
        MasterAudioEventBackend.BusFilter = busFilter;
        RelationsInspectorLink.ResetTargets(new object[] { "currentScene" }, "MasterAudioEventBackend");
    }

    public static DTFunctionButtons AddMixerButtons(MasterAudioGroup aGroup, string itemName, bool showSettingsIcon = true) {
        var deleteIcon = MasterAudioInspectorResources.DeleteTexture;
        var settingsIcon = MasterAudioInspectorResources.GearTexture;

        var muteContent = new GUIContent(MasterAudioInspectorResources.MuteOffTexture, "Click to mute " + itemName);

        if (aGroup.isMuted) {
            muteContent.image = MasterAudioInspectorResources.MuteOnTexture;
        }

        var soloContent = new GUIContent(MasterAudioInspectorResources.SoloOffTexture, "Click to solo " + itemName);

        if (aGroup.isSoloed) {
            soloContent.image = MasterAudioInspectorResources.SoloOnTexture;
        }

        if (GUILayout.Button(soloContent, EditorStyles.toolbarButton)) {
            return DTFunctionButtons.Solo;
        }
        if (GUILayout.Button(muteContent, EditorStyles.toolbarButton)) {
            return DTFunctionButtons.Mute;
        }

        if (showSettingsIcon) {
            if (GUILayout.Button(new GUIContent(settingsIcon, "Click to edit " + itemName), EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Go;
            }
        }

        if (!IsPrefabInProjectView(aGroup)) {
            if (GUILayout.Button(
                    new GUIContent(MasterAudioInspectorResources.PreviewTexture, "Click to preview " + itemName),
                    EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Play;
            }
            if (GUILayout.Button(
                new GUIContent(MasterAudioInspectorResources.StopTexture, "Click to stop all of Sound"),
                EditorStyles.toolbarButton)) {
                return DTFunctionButtons.Stop;
            }
            if (ShowFindUsages("Sound Group '" + aGroup.name + "'")) {
                return DTFunctionButtons.Find;
            }
        }

        if (IsPrefabInProjectView(aGroup)) {
            return DTFunctionButtons.None;
        }

        if (GUILayout.Button(new GUIContent(deleteIcon, "Click to delete " + itemName), EditorStyles.toolbarButton)) {
            return DTFunctionButtons.Remove;
        }

        return DTFunctionButtons.None;
    }

    public static DTFunctionButtons AddPlaylistControllerGOButtons(PlaylistController controller, string itemName) {
        var muteContent = new GUIContent(MasterAudioInspectorResources.MuteOffTexture, "Click to mute " + itemName);
        if (controller.isMuted) {
            muteContent.image = MasterAudioInspectorResources.MuteOnTexture;
        }

        var mutePressed = GUILayout.Button(muteContent, EditorStyles.toolbarButton, GUILayout.Width(32));

        // Return the pressed button if any
        if (mutePressed) {
            return DTFunctionButtons.Mute;
        }

        return DTFunctionButtons.None;
    }

    public static DTFunctionButtons AddPlaylistControllerSetupButtons(PlaylistController controller, string itemName, bool jukeboxMode, bool narrowMode = false) {
        var deleteIcon = MasterAudioInspectorResources.DeleteTexture;
        var settingsIcon = MasterAudioInspectorResources.GearTexture;

        var muteContent = new GUIContent(MasterAudioInspectorResources.MuteOffTexture, "Click to mute " + itemName);
        if (controller.isMuted) {
            muteContent.image = MasterAudioInspectorResources.MuteOnTexture;
        }

        var mutePressed = GUILayout.Button(muteContent, EditorStyles.toolbarButton);

        if (!jukeboxMode) {
            // Remove Button - Process presses later
            var goPressed = false;
            if (!narrowMode) {
                goPressed = GUILayout.Button(new GUIContent(settingsIcon, "Click to edit " + itemName),
                    EditorStyles.toolbarButton);
            }
            var removePressed = false;

            if (Application.isPlaying) {
                //GUILayout.Space(26);
            } else {
                removePressed = GUILayout.Button(new GUIContent(deleteIcon, "Click to delete " + itemName), EditorStyles.toolbarButton);
            }

            if (removePressed) {
                return DTFunctionButtons.Remove;
            }
            if (goPressed) {
                return DTFunctionButtons.Go;
            }
        }

        // Return the pressed button if any
        if (mutePressed) {
            return DTFunctionButtons.Mute;
        }

        return DTFunctionButtons.None;
    }

    public static DTFunctionButtons AddFoldOutListItemButtonItems(int position, int totalPositions, string itemName, bool showAfterText, bool showCopyButton = false, bool showMoveButtons = false, bool showAudioPreview = false) {
        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));

        // A little space between button groups
        GUILayout.Space(24);

        var upPressed = false;
        var downPressed = false;
        var previewPressed = false;
        var stopPressed = false;
        var copyPressed = false;

        if (showAudioPreview) {
            if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.PreviewTexture, "Click to preview clip"),
                    EditorStyles.toolbarButton)) {
                previewPressed = true;
            }
            if (GUILayout.Button(new GUIContent(MasterAudioInspectorResources.StopTexture, "Click to stop previewing clip"),
                    EditorStyles.toolbarButton)) {
                stopPressed = true;
            }
        }

        if (showCopyButton) {
            copyPressed = GUILayout.Button(new GUIContent(MasterAudioInspectorResources.CopyTexture, "Click to clone Song"),
                    EditorStyles.toolbarButton, GUILayout.Height(16), GUILayout.Width(40));
        }

        if (showMoveButtons) {
            if (position > 0) {
                // the up arrow.
                upPressed = GUILayout.Button(new GUIContent(UpArrow, "Click to shift " + itemName + " up"),
                    EditorStyles.toolbarButton);
            } else {
                GUILayout.Space(19);
            }

            if (position < totalPositions - 1) {
                // The down arrow will move things towards the end of the List
                downPressed = GUILayout.Button(new GUIContent(DownArrow, "Click to shift " + itemName + " down"),
                    EditorStyles.toolbarButton);
            } else {
                GUILayout.Space(19);
            }
        }

        var buttonText = "Click to add new " + itemName;
        if (showAfterText) {
            buttonText += " after this one";
        }

        // Add button - Process presses later
        GUI.contentColor = BrightButtonColor;
        var addPressed = GUILayout.Button(new GUIContent("Add", buttonText),
            EditorStyles.toolbarButton);
        GUI.contentColor = Color.white;

        // Remove Button - Process presses later
        var removePressed = GUILayout.Button(new GUIContent(MasterAudioInspectorResources.DeleteTexture, "Click to remove " + itemName),
                                                  EditorStyles.toolbarButton);

        EditorGUILayout.EndHorizontal();

        // Return the pressed button if any
        if (removePressed) {
            return DTFunctionButtons.Remove;
        }
        if (addPressed) {
            return DTFunctionButtons.Add;
        }
        if (upPressed) {
            return DTFunctionButtons.ShiftUp;
        }
        if (downPressed) {
            return DTFunctionButtons.ShiftDown;
        }
        if (previewPressed) {
            return DTFunctionButtons.Play;
        }
        if (stopPressed) {
            return DTFunctionButtons.Stop;
        }
        if (copyPressed) {
            return DTFunctionButtons.Clone;
        }

        return DTFunctionButtons.None;
    }

    public static bool Foldout(bool expanded, string label) {
        var content = new GUIContent(label, FoldOutTooltip);
        expanded = EditorGUILayout.Foldout(expanded, content);

        return expanded;
    }

    public static void ShowColorWarning(string warningText) {
        EditorGUILayout.HelpBox(warningText, MessageType.Info);
    }

    public static void ShowRedError(string errorText) {
        EditorGUILayout.HelpBox(errorText, MessageType.Error);
    }

    public static void ShowLargeBarAlert(string errorText) {
        EditorGUILayout.HelpBox(errorText, MessageType.Warning);
    }

    public static void ShowAlert(string text) {
        if (Application.isPlaying) {
            Debug.LogWarning(text);
        } else {
            EditorUtility.DisplayDialog(AlertTitle, text,
                    AlertOkText);
        }
    }

    public static string GetResourcePath(AudioClip audioClip, ref bool isLocalizedFolder, bool ignoreLanguageFolders = false) {
        var fullPath = AssetDatabase.GetAssetPath(audioClip);
        // ReSharper disable once StringIndexOfIsCultureSpecific.1
        var index = fullPath.ToLower().IndexOf("/resources/");
        if (index <= -1) {
            ShowAlert("You have dragged an Audio Clip that is not in a Resource folder while in Resource file mode. Creation succeeded, but this Group / Variation will probably not function.");
            return null;
        }

        var shortPath = fullPath.Substring(index + 11);

        // ReSharper disable once StringIndexOfIsCultureSpecific.1
        var nextSlash = shortPath.IndexOf("/");
        if (nextSlash > -1 && !ignoreLanguageFolders) {
            var firstFolder = shortPath.Substring(0, nextSlash);
            try {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Enum.Parse(typeof(SystemLanguage), firstFolder);
                shortPath = shortPath.Substring(nextSlash + 1);
                isLocalizedFolder = true;
            }
            catch {
                // do nothing, it's not a language name folder
            }
        }

        // ReSharper disable once StringLastIndexOfIsCultureSpecific.1
        var dotIndex = shortPath.LastIndexOf(".");
        if (dotIndex >= 0) {
            shortPath = shortPath.Substring(0, dotIndex);
        }
        return shortPath;
    }

    private static PrefabType GetPrefabType(Object gObject) {
        return PrefabUtility.GetPrefabType(gObject);
    }

    public static bool IsPrefabInProjectView(Object gObject) {
        return GetPrefabType(gObject) == PrefabType.Prefab;
    }

    public static GameObject DuplicateGameObject(GameObject gameObj, string baseName, int? optionalCountSuffix) {
        var prefabRoot = PrefabUtility.GetPrefabParent(gameObj);

        GameObject dupe;

        if (prefabRoot != null) {
            dupe = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
        } else {
            // ReSharper disable RedundantCast
            // ReSharper disable once AccessToStaticMemberViaDerivedType
            dupe = (GameObject)GameObject.Instantiate(gameObj);
            // ReSharper restore RedundantCast
        }

        if (dupe == null) {
            return null;
        }
        var newName = baseName;
        if (optionalCountSuffix.HasValue) {
            newName += optionalCountSuffix.Value;
        }
        dupe.name = newName;

        return dupe;
    }
}
