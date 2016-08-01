#if UNITY_5
    // clip manager doesn't exist because they've closed off the API of the Audio Importer
#else
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using DarkTonic.MasterAudio;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public class MasterAudioClipManager : EditorWindow {
    private const string NoClipsSelected = "There are no clips selected.";
    private const string CacheFilePath = "Assets/Plugins/DarkTonic/MasterAudio/audioImportSettings.xml";
    private const string AllFoldersKey = "[All]";
    private const int MaxPageSize = 200;

    private readonly AudioInfoData _clipList = new AudioInfoData();

    private int _bulkBitrate = 156000;
    private bool _bulk3D = true;
    private bool _bulkForceMono;
    private AudioImporterFormat _bulkFormat = AudioImporterFormat.Native;
    private AudioImporterLoadType _bulkLoadType = AudioImporterLoadType.CompressedInMemory;
    private int _pageNumber;

    private List<AudioInformation> _filterClips;
    private List<AudioInformation> _filteredOut;
    private Vector2 _scrollPos;
    private Vector2 _outsideScrollPos;
    private readonly List<string> _folderPaths = new List<string>();
    private string _selectedFolderPath = AllFoldersKey;

    [MenuItem("Window/Master Audio/Master Audio Clip Manager")]
    // ReSharper disable once UnusedMember.Local
    static void Init() {
        GetWindow(typeof(MasterAudioClipManager));
    }

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    void OnGUI() {
        _outsideScrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), _outsideScrollPos, new Rect(0, 0, 900, 666));

        if (MasterAudioInspectorResources.LogoTexture != null) {
            DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.LogoTexture);
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUI.contentColor = DTGUIHelper.BrightButtonColor;
        if (GUILayout.Button(new GUIContent("Scan Project"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
            BuildCache();
            return;
        }

        GUILayout.Space(10);
        if (GUILayout.Button(new GUIContent("Revert Selected"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
            RevertSelected();
            return;
        }

        GUILayout.Space(10);
        if (GUILayout.Button(new GUIContent("Apply Selected"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
            ApplySelected();
            return;
        }

        GUILayout.Space(10);
        RevertColor();

        GUILayout.Label("Full Path Filter");
        var oldFilter = _clipList.SearchFilter;
        var newFilter = GUILayout.TextField(_clipList.SearchFilter, EditorStyles.toolbarTextField, GUILayout.Width(200));
        if (newFilter != oldFilter) {
            _clipList.SearchFilter = newFilter;
            RebuildFilteredList();
        }

        var myPosition = GUILayoutUtility.GetRect(10, 10, ToolbarSeachCancelButton);
        myPosition.x -= 5;
        if (GUI.Button(myPosition, "", ToolbarSeachCancelButton)) {
            _clipList.SearchFilter = string.Empty;
            RebuildFilteredList();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (!File.Exists(CacheFilePath)) {
            DTGUIHelper.ShowLargeBarAlert("Click 'Scan Project' to generate list of Audio Clips.");
            GUI.EndScrollView();
            return;
        }

        if (_clipList.AudioInfor.Count == 0 || _clipList.NeedsRefresh) {
            if (!LoadAndTranslateFile()) {
                GUI.EndScrollView();
                return;
            }
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Folder");
        var selectedIndex = _folderPaths.IndexOf(_selectedFolderPath);
        var newIndex = EditorGUILayout.Popup(selectedIndex, _folderPaths.ToArray(), GUILayout.Width(800));
        if (newIndex != selectedIndex) {
            _selectedFolderPath = _folderPaths[newIndex];
            RebuildFilteredList();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        var totalClips = _clipList.AudioInfor.Count;
        var dynamicText = string.Format("{0}/{1} clips selected.", SelectedClips.Count, FilteredClips.Count);
        dynamicText += " Total clips: " + totalClips;

        double clipCount = totalClips;
        if (_filteredOut != null) {
            clipCount = _filteredOut.Count;
        }

        var pageCount = (int)Math.Ceiling(clipCount / MaxPageSize);

        var pageNames = new string[pageCount];
        var pageNums = new int[pageCount];
        for (var i = 0; i < pageCount; i++) {
            pageNames[i] = "Page " + (i + 1);
            pageNums[i] = i;
        }


        EditorGUILayout.LabelField(dynamicText);

        var oldPage = _pageNumber;

        EditorGUILayout.BeginHorizontal();
        _pageNumber = EditorGUILayout.IntPopup("", _pageNumber, pageNames, pageNums, GUILayout.Width(100));
        if (oldPage != _pageNumber) {
            RebuildFilteredList(true);
        }
        GUILayout.Label("of " + pageCount);

        EditorGUILayout.EndHorizontal();

        // display
        DisplayClips();

        ShowBulkOperations();

        GUI.EndScrollView();
    }

    private void RebuildFilteredList(bool keepPageNumber = false) {
        if (!keepPageNumber) {
            _pageNumber = 0;
        }

        _filterClips = null;
        _filteredOut = null;
    }

    private void ShowBulkOperations() {
        GUILayout.BeginArea(new Rect(0, 616, 895, 200));
        GUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
        GUI.contentColor = DTGUIHelper.BrightButtonColor;
        GUILayout.Label("Bulk Settings: Click Copy buttons to copy setting to all selected."); //  the setting above it to all selected.
        GUILayout.Space(26);

        GUI.contentColor = DTGUIHelper.BrightButtonColor;
        if (GUILayout.Button(new GUIContent("Copy", "Copy Compression bitrate above to all selected"), EditorStyles.toolbarButton, GUILayout.Width(45))) {
            CopyBitrateToSelected();
        }
        GUILayout.Space(6);
        if (GUILayout.Button(new GUIContent("Copy", "Copy 3D setting above to all selected"), EditorStyles.toolbarButton, GUILayout.Width(45))) {
            Copy3DToSelected();
        }

        GUILayout.Space(8);
        if (GUILayout.Button(new GUIContent("Copy", "Copy Force Mono setting above to all selected"), EditorStyles.toolbarButton, GUILayout.Width(45))) {
            CopyForceMonoToSelected();
        }

        GUILayout.Space(26);
        if (GUILayout.Button(new GUIContent("Copy", "Copy Audio Format setting above to all selected"), EditorStyles.toolbarButton, GUILayout.Width(45))) {
            CopyFormatToSelected();
        }

        GUILayout.Space(101);
        if (GUILayout.Button(new GUIContent("Copy", "Copy Load Type setting above to all selected"), EditorStyles.toolbarButton, GUILayout.Width(45))) {
            CopyLoadTypeToSelected();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUI.contentColor = Color.white;

        GUILayout.BeginHorizontal(DTGUIHelper.CornerGUIStyle);
        GUILayout.Space(246);

        _bulkBitrate = EditorGUILayout.IntSlider("", _bulkBitrate / 1000, 32, 256, GUILayout.Width(202)) * 1000;
        GUILayout.Space(13);
        _bulk3D = GUILayout.Toggle(_bulk3D, "");
        GUILayout.Space(36);
        _bulkForceMono = GUILayout.Toggle(_bulkForceMono, "");
        GUILayout.Space(35);

        _bulkFormat = (AudioImporterFormat)EditorGUILayout.EnumPopup(_bulkFormat, GUILayout.Width(136));

        GUILayout.Space(6);

        _bulkLoadType = (AudioImporterLoadType)EditorGUILayout.EnumPopup(_bulkLoadType, GUILayout.Width(140));

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private bool LoadAndTranslateFile() {
        XmlDocument xFiles;
        try {
            xFiles = new XmlDocument();
            xFiles.Load(CacheFilePath);
        }
        catch {
            DTGUIHelper.ShowRedError("Cache file is malformed. Click 'Scan Project' to regenerate it.");
            return false;
        }

        if (_clipList.AudioInfor.Count == 0) {
            _clipList.AudioInfor.Clear();
        }

        // translate
        var success = TranslateFromXml(xFiles);
        if (!success) {
            return false;
        }

        return true;
    }

    private void ApplySelected() {
        if (SelectedClips.Count == 0) {
            DTGUIHelper.ShowAlert(NoClipsSelected);
            return;
        }

        foreach (var aClip in SelectedClips) {
            ApplyClipChanges(aClip, false);
            aClip.HasChanged = true;
        }

        _clipList.NeedsRefresh = true;

        WriteFile(_clipList);
    }

    private void RevertSelected() {
        if (SelectedClips.Count == 0) {
            DTGUIHelper.ShowAlert(NoClipsSelected);
            return;
        }

        foreach (var aClip in SelectedClips) {
            RevertChanges(aClip);
        }
    }

    private List<AudioInformation> SelectedClips {
        get {
            var selected = new List<AudioInformation>();

            foreach (var t in FilteredClips) {
                if (!t.IsSelected) {
                    continue;
                }

                selected.Add(t);
            }

            return selected;
        }
    }

    private List<AudioInformation> FilteredClips {
        get {
            if (_filterClips != null) {
                return _filterClips;
            }

            _filterClips = new List<AudioInformation>();

            if (!string.IsNullOrEmpty(_clipList.SearchFilter)) {
                if (_filteredOut == null) {
                    _filteredOut = new List<AudioInformation>();
                    _filteredOut.AddRange(_clipList.AudioInfor);
                }

                _filteredOut.RemoveAll(delegate(AudioInformation obj) {
                    return !obj.FullPath.ToLower().Contains(_clipList.SearchFilter.ToLower());
                });
            }

            if (_selectedFolderPath != AllFoldersKey) {
                if (_filteredOut == null) {
                    _filteredOut = new List<AudioInformation>();
                    _filteredOut.AddRange(_clipList.AudioInfor);
                }

                _filteredOut.RemoveAll(delegate(AudioInformation obj) {
                    // ReSharper disable once StringLastIndexOfIsCultureSpecific.1
                    var index = obj.FullPath.ToLower().LastIndexOf(_selectedFolderPath.ToLower());
                    if (index <= -1) {
                        return
                            !obj.FullPath.ToLower()
                                .Contains(_selectedFolderPath.ToLower());
                    }
                    var endPart = obj.FullPath.Substring(index + _selectedFolderPath.Length + 1);
                    if (endPart.Contains("/")) {
                        return true; // don't show sub-folders
                    }
                    return !obj.FullPath.ToLower().Contains(_selectedFolderPath.ToLower());
                });
            }

            var arrayToAddFrom = _clipList.AudioInfor;
            if (_filteredOut != null) {
                arrayToAddFrom = _filteredOut;
            }

            var firstResultNum = MaxPageSize * _pageNumber;
            var lastResultNum = firstResultNum + MaxPageSize - 1;
            if (lastResultNum > arrayToAddFrom.Count) {
                lastResultNum = arrayToAddFrom.Count;
            }

            if (arrayToAddFrom.Count > 0) {
                var isAsc = _clipList.SortDir == ClipSortDirection.Ascending;

                arrayToAddFrom.Sort(delegate(AudioInformation x, AudioInformation y) {
                    if (_clipList.SortColumn == ClipSortColumn.Name) {
                        if (isAsc) {
                            return x.Name.CompareTo(y.Name);
                        }
                        return y.Name.CompareTo(x.Name);
                    }
                    if (_clipList.SortColumn == ClipSortColumn.Bitrate) {
                        if (isAsc) {
                            return x.OrigCompressionBitrate.CompareTo(y.OrigCompressionBitrate);
                        }
                        return y.OrigCompressionBitrate.CompareTo(x.OrigCompressionBitrate);
                    }
                    if (_clipList.SortColumn == ClipSortColumn.Is3D) {
                        if (isAsc) {
                            return x.OrigIs3D.CompareTo(y.OrigIs3D);
                        }
                        return y.OrigIs3D.CompareTo(x.OrigIs3D);
                    }
                    if (_clipList.SortColumn == ClipSortColumn.ForceMono) {
                        if (isAsc) {
                            return x.OrigForceMono.CompareTo(y.OrigForceMono);
                        }
                        return y.OrigForceMono.CompareTo(x.OrigForceMono);
                    }
                    if (_clipList.SortColumn == ClipSortColumn.AudioFormat) {
                        if (isAsc) {
                            return x.OrigFormat.CompareTo(y.OrigFormat);
                        }
                        return y.OrigFormat.CompareTo(x.OrigFormat);
                    }
                    // ReSharper disable once InvertIf
                    if (_clipList.SortColumn == ClipSortColumn.LoadType) {
                        // ReSharper disable once ConvertIfStatementToReturnStatement
                        if (isAsc) {
                            return x.OrigLoadType.CompareTo(y.OrigLoadType);
                        }
                        return y.OrigLoadType.CompareTo(x.OrigLoadType);
                    }

                    return x.Name.CompareTo(y.Name);
                });
            }

            // de-select filtered out clips 
            foreach (var aClip in _clipList.AudioInfor) {
                if (!arrayToAddFrom.Contains(aClip)) {
                    aClip.IsSelected = false;
                }
            }

            for (var i = firstResultNum; i < lastResultNum; i++) {
                _filterClips.Add(arrayToAddFrom[i]);
            }

            return _filterClips;
        }
    }

    private void ChangeSortColumn(ClipSortColumn col) {
        var oldCol = _clipList.SortColumn;
        _clipList.SortColumn = col;
        if (oldCol != _clipList.SortColumn) {
            _clipList.SortDir = ClipSortDirection.Ascending;
        } else {
            _clipList.SortDir = _clipList.SortDir == ClipSortDirection.Ascending ? ClipSortDirection.Descending : ClipSortDirection.Ascending;
        }

        RebuildFilteredList();
    }

    private string ColumnPrefix(ClipSortColumn col) {
        if (col != _clipList.SortColumn) {
            return " ";
        }

        return _clipList.SortDir == ClipSortDirection.Ascending ? DTGUIHelper.UpArrow : DTGUIHelper.DownArrow;
    }

    private void DisplayClips() {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUI.contentColor = DTGUIHelper.BrightButtonColor;
        if (GUILayout.Button("All", EditorStyles.toolbarButton, GUILayout.Width(36))) {
            foreach (var t in FilteredClips) {
                t.IsSelected = true;
            }
        }

        if (GUILayout.Button("None", EditorStyles.toolbarButton, GUILayout.Width(36))) {
            foreach (var t in _clipList.AudioInfor) {
                t.IsSelected = false;
            }
        }

        GUI.contentColor = DTGUIHelper.BrightButtonColor;
        GUILayout.Space(6);
        var columnPrefix = ColumnPrefix(ClipSortColumn.Name);
        if (GUILayout.Button(new GUIContent(columnPrefix + "Clip Name", "Click to sort by Clip Name"), EditorStyles.toolbarButton, GUILayout.Width(156))) {
            ChangeSortColumn(ClipSortColumn.Name);
        }

        columnPrefix = ColumnPrefix(ClipSortColumn.Bitrate);
        if (GUILayout.Button(new GUIContent(columnPrefix + "Compression (kbps)", "Click to sort by Compression Bitrate"), EditorStyles.toolbarButton, GUILayout.Width(214))) {
            ChangeSortColumn(ClipSortColumn.Bitrate);
        }

        columnPrefix = ColumnPrefix(ClipSortColumn.Is3D);
        if (GUILayout.Button(new GUIContent(columnPrefix + "3D", "Click to sort by 3D"), EditorStyles.toolbarButton, GUILayout.Width(36))) {
            ChangeSortColumn(ClipSortColumn.Is3D);
        }

        columnPrefix = ColumnPrefix(ClipSortColumn.ForceMono);
        if (GUILayout.Button(new GUIContent(columnPrefix + "Force Mono", "Click to sort by Force Mono"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
            ChangeSortColumn(ClipSortColumn.ForceMono);
        }

        columnPrefix = ColumnPrefix(ClipSortColumn.AudioFormat);
        if (GUILayout.Button(new GUIContent(columnPrefix + "Audio Format", "Click to sort by Audio Format"), EditorStyles.toolbarButton, GUILayout.Width(144))) {
            ChangeSortColumn(ClipSortColumn.AudioFormat);
        }

        columnPrefix = ColumnPrefix(ClipSortColumn.LoadType);
        if (GUILayout.Button(new GUIContent(columnPrefix + "Load Type", "Click to sort by Load Type"), EditorStyles.toolbarButton, GUILayout.Width(182))) {
            ChangeSortColumn(ClipSortColumn.LoadType);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (FilteredClips.Count == 0) {
            DTGUIHelper.ShowLargeBarAlert("You have filtered all clips out.");
            return;
        }

        _scrollPos = GUI.BeginScrollView(new Rect(0, 123, 896, 485), _scrollPos, new Rect(0, 124, 880, 24 * FilteredClips.Count + 4));

        foreach (var aClip in FilteredClips) {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (aClip.IsSelected) {
                GUI.backgroundColor = DTGUIHelper.BrightButtonColor;
            } else {
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.BeginHorizontal(EditorStyles.miniButtonMid); // miniButtonMid, numberField, textField
            EditorGUILayout.BeginHorizontal();

            var wasSelected = aClip.IsSelected;
            aClip.IsSelected = GUILayout.Toggle(aClip.IsSelected, "");

            if (aClip.IsSelected) {
                if (!wasSelected) {
                    SelectClip(aClip);
                }
            }

            var bitrateChanged = !aClip.OrigCompressionBitrate.Equals(aClip.CompressionBitrate);
            var is3DChanged = !aClip.OrigIs3D.Equals(aClip.Is3D);
            var isMonoChanged = !aClip.OrigForceMono.Equals(aClip.ForceMono);
            var isFormatChanged = !aClip.OrigFormat.Equals(aClip.Format);
            var isLoadTypeChanged = !aClip.OrigLoadType.Equals(aClip.LoadType);

            var hasChanged = bitrateChanged || is3DChanged || isMonoChanged || isFormatChanged || isLoadTypeChanged;

            if (!hasChanged) {
                ShowDisabledColors();
            } else {
                GUI.contentColor = DTGUIHelper.BrightButtonColor;
            }
            if (GUILayout.Button(new GUIContent("Revert"), EditorStyles.toolbarButton, GUILayout.Width(45))) {
                if (!hasChanged) {
                    DTGUIHelper.ShowAlert("This clip's properties have not changed.");
                } else {
                    RevertChanges(aClip);
                }
            }

            RevertColor();

            GUILayout.Space(10);
            GUILayout.Label(new GUIContent(aClip.Name, aClip.FullPath), GUILayout.Width(150));

            GUILayout.Space(10);
            MaybeShowChangedColors(bitrateChanged);
            var oldBitrate = aClip.CompressionBitrate;
            var bitRate = (int)(aClip.CompressionBitrate * .001f);
            aClip.CompressionBitrate = EditorGUILayout.IntSlider("", bitRate, 32, 256, GUILayout.Width(202)) * 1000;
            if (oldBitrate != aClip.CompressionBitrate) {
                aClip.IsSelected = true;
                SelectClip(aClip);
            }
            RevertColor();

            GUILayout.Space(12);
            MaybeShowChangedColors(is3DChanged);
            var old3D = aClip.Is3D;
            aClip.Is3D = GUILayout.Toggle(aClip.Is3D, "");
            if (old3D != aClip.Is3D) {
                aClip.IsSelected = true;
                SelectClip(aClip);
            }
            RevertColor();

            GUILayout.Space(36);
            MaybeShowChangedColors(isMonoChanged);
            var oldMono = aClip.ForceMono;
            aClip.ForceMono = GUILayout.Toggle(aClip.ForceMono, "", GUILayout.Width(40));
            if (oldMono != aClip.ForceMono) {
                aClip.IsSelected = true;
                SelectClip(aClip);
            }
            RevertColor();

            GUILayout.Space(10);
            MaybeShowChangedColors(isFormatChanged);
            var oldFmt = aClip.Format;

            aClip.Format = (AudioImporterFormat)EditorGUILayout.EnumPopup(aClip.Format, GUILayout.Width(136));

            if (oldFmt != aClip.Format) {
                aClip.IsSelected = true;
                SelectClip(aClip);
            }
            RevertColor();

            GUILayout.Space(6);
            MaybeShowChangedColors(isLoadTypeChanged);
            var oldLoad = aClip.LoadType;

            aClip.LoadType = (AudioImporterLoadType)EditorGUILayout.EnumPopup(aClip.LoadType, GUILayout.Width(140));

            if (oldLoad != aClip.LoadType) {
                aClip.IsSelected = true;
                SelectClip(aClip);
            }
            RevertColor();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            RevertColor();
        }

        GUI.EndScrollView();
    }

    private static void ShowDisabledColors() {
        GUI.color = Color.gray;
        GUI.contentColor = Color.white;
    }

    private static void MaybeShowChangedColors(bool areChanged) {
        if (!areChanged) {
            return;
        }

        GUI.backgroundColor = DTGUIHelper.BrightButtonColor;
        GUI.color = DTGUIHelper.BrightButtonColor;
    }

    private static void RevertColor() {
        GUI.backgroundColor = Color.white;
        GUI.color = Color.white;
        GUI.contentColor = Color.white;
    }

    private static void RevertChanges(AudioInformation info) {
        info.CompressionBitrate = info.OrigCompressionBitrate;
        info.ForceMono = info.OrigForceMono;
        info.Format = info.OrigFormat;
        info.LoadType = info.OrigLoadType;
        info.Is3D = info.OrigIs3D;
    }

    private void ApplyClipChanges(AudioInformation info, bool writeChanges) {
        Selection.objects = new Object[] { }; // unselect to get "Apply" to work automatically.

        // ReSharper disable once AccessToStaticMemberViaDerivedType
        var importer = (AudioImporter)AudioImporter.GetAtPath(info.FullPath);

        importer.compressionBitrate = info.CompressionBitrate;

        importer.forceToMono = info.ForceMono;
        importer.format = info.Format;
        importer.loadType = info.LoadType;
        importer.threeD = info.Is3D;

        AssetDatabase.ImportAsset(info.FullPath, ImportAssetOptions.ForceUpdate);
        info.HasChanged = true;

        if (writeChanges) {
            WriteFile(_clipList);
        }
    }

    private bool TranslateFromXml(XmlDocument xDoc) {
        _folderPaths.Clear();
        _folderPaths.Add("[All]");

        var files = xDoc.SelectNodes("/Files//File");

        // ReSharper disable once PossibleNullReferenceException
        if (files.Count == 0) {
            DTGUIHelper.ShowLargeBarAlert("You have no audio files in this project. Add some, then click 'Scan Project'.");
            return false;
        }

        try {
            // ReSharper disable once PossibleNullReferenceException
            _clipList.SearchFilter = xDoc.DocumentElement.Attributes["searchFilter"].Value;
            _clipList.SortColumn = (ClipSortColumn)Enum.Parse(typeof(ClipSortColumn), xDoc.DocumentElement.Attributes["sortColumn"].Value);
            _clipList.SortDir = (ClipSortDirection)Enum.Parse(typeof(ClipSortDirection), xDoc.DocumentElement.Attributes["sortDir"].Value);

            var currentPaths = new List<string>();

            for (var i = 0; i < files.Count; i++) {
                var aNode = files[i];
                // ReSharper disable once PossibleNullReferenceException
                var path = aNode.Attributes["path"].Value.Trim();
                var clipName = aNode.Attributes["name"].Value.Trim();
                var is3D = bool.Parse(aNode.Attributes["is3d"].Value);
                var compressionBitrate = int.Parse(aNode.Attributes["bitRate"].Value);
                var forceMono = bool.Parse(aNode.Attributes["forceMono"].Value);

                var format = (AudioImporterFormat)Enum.Parse(typeof(AudioImporterFormat), aNode.Attributes["format"].Value);
                var loadType = (AudioImporterLoadType)Enum.Parse(typeof(AudioImporterLoadType), aNode.Attributes["loadType"].Value);

                currentPaths.Add(path);

                var folderPath = Path.GetDirectoryName(path);
                if (!_folderPaths.Contains(folderPath)) {
                    _folderPaths.Add(folderPath);
                }

                var matchingClip = _clipList.AudioInfor.Find(delegate(AudioInformation obj) {
                    return obj.FullPath == path;
                });

                if (matchingClip == null) {
                    var aud = new AudioInformation(path, clipName, is3D, compressionBitrate, forceMono, format, loadType);
                    _clipList.AudioInfor.Add(aud);
                } else {
                    matchingClip.OrigIs3D = is3D;
                    matchingClip.OrigFormat = format;
                    matchingClip.OrigLoadType = loadType;
                    matchingClip.OrigForceMono = forceMono;
                    matchingClip.OrigCompressionBitrate = compressionBitrate;
                }

                _clipList.NeedsRefresh = false;
            }

            // delete clips no longer in the XML
            _clipList.AudioInfor.RemoveAll(delegate(AudioInformation obj) {
                return !currentPaths.Contains(obj.FullPath);
            });
        }
        catch {
            DTGUIHelper.ShowRedError("Could not translate XML from cache file. Please click 'Scan Project'.");
            return false;
        }

        return true;
    }

    private void BuildCache() {
        var filePaths = AssetDatabase.GetAllAssetPaths();

        var audioInfo = new AudioInfoData();
        _filterClips = null;
        _pageNumber = 0;

        var updatedTime = DateTime.Now.Ticks;

        foreach (var aPath in filePaths) {
            if (!aPath.EndsWith(".wav", StringComparison.InvariantCultureIgnoreCase)
                && !aPath.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase)
                && !aPath.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase)
                && !aPath.EndsWith(".aiff", StringComparison.InvariantCultureIgnoreCase)) {

                continue;
            }

            // ReSharper disable once AccessToStaticMemberViaDerivedType
            var importer = (AudioImporter)AudioImporter.GetAtPath(aPath);

            var bitrate = importer.compressionBitrate;

            if (bitrate < 0) {
                bitrate = 156000;
            }

            // ReSharper disable once UseObjectOrCollectionInitializer
            var newClip = new AudioInformation(aPath, Path.GetFileNameWithoutExtension(aPath), importer.threeD, bitrate, importer.forceToMono, importer.format, importer.loadType);

            newClip.LastUpdated = updatedTime;

            audioInfo.AudioInfor.Add(newClip);
        }

        audioInfo.AudioInfor.RemoveAll(delegate(AudioInformation obj) {
            return obj.LastUpdated < updatedTime;
        });

        // write file
        if (!WriteFile(audioInfo)) {
            return;
        }

        LoadAndTranslateFile();
    }

    private bool WriteFile(AudioInfoData audInfo) {
        StreamWriter writer = null;

        try {
            var sb = new StringBuilder(string.Empty);

            var safeFilter = audInfo.SearchFilter.Replace("'", "").Replace("\"", "");
            sb.Append(string.Format("<Files searchFilter='{0}' sortColumn='{1}' sortDir='{2}'>", safeFilter, audInfo.SortColumn, audInfo.SortDir));
            foreach (var aud in audInfo.AudioInfor) {
                var is3D = aud.HasChanged ? aud.Is3D : aud.OrigIs3D;
                var bitrate = aud.HasChanged ? aud.CompressionBitrate : aud.OrigCompressionBitrate;
                var mono = aud.HasChanged ? aud.ForceMono : aud.OrigForceMono;
                var fmt = aud.HasChanged ? aud.Format : aud.OrigFormat;
                var loadType = aud.HasChanged ? aud.LoadType : aud.OrigLoadType;

                sb.Append(string.Format("<File path='{0}' name='{1}' is3d='{2}' bitRate='{3}' forceMono='{4}' format='{5}' loadType='{6}' />",
                    UtilStrings.ReplaceUnsafeChars(aud.FullPath),
                    UtilStrings.ReplaceUnsafeChars(aud.Name),
                    is3D,
                    bitrate,
                    mono,
                    fmt,
                    loadType));
            }
            sb.Append("</Files>");

            writer = new StreamWriter(CacheFilePath);
            writer.WriteLine(sb.ToString());

            _clipList.AudioInfor.RemoveAll(delegate(AudioInformation obj) {
                return obj.HasChanged;
            });
        }
        catch (Exception ex) {
            Debug.LogError("Error occurred constructing or writing audioImportSettings.xml file: " + ex);
            return false;
        }
        finally {
            if (writer != null) {
                writer.Close();
            }
        }

        return true;
    }

    private static void SelectClip(AudioInformation info) {
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(info.FullPath);
    }

    public enum ClipSortColumn {
        Name,
        Is3D,
        Bitrate,
        ForceMono,
        AudioFormat,
        LoadType
    }

    public enum ClipSortDirection {
        Ascending,
        Descending
    }

    public class AudioInfoData {
        public List<AudioInformation> AudioInfor = new List<AudioInformation>();
        public string SearchFilter = string.Empty;
        public ClipSortColumn SortColumn = ClipSortColumn.Name;
        public ClipSortDirection SortDir = ClipSortDirection.Ascending;

        public bool NeedsRefresh;
    }

    public class AudioInformation {
        public bool OrigIs3D;

        public int OrigCompressionBitrate;
        public int CompressionBitrate;
        public AudioImporterFormat OrigFormat;
        public AudioImporterLoadType OrigLoadType;
        public AudioImporterFormat Format;
        public AudioImporterLoadType LoadType;

        public bool OrigForceMono;

        public string FullPath;
        public string Name;
        public bool Is3D;
        public bool ForceMono;

        public bool IsSelected;
        public bool HasChanged;
        public long LastUpdated;

        public AudioInformation(string fullPath, string name, bool is3D, int compressionBitrate, bool forceMono, AudioImporterFormat format, AudioImporterLoadType loadType) {
            OrigIs3D = is3D;
            OrigCompressionBitrate = compressionBitrate;
            OrigForceMono = forceMono;
            OrigFormat = format;
            OrigLoadType = loadType;

            FullPath = fullPath;
            Name = name;
            Is3D = is3D;
            CompressionBitrate = compressionBitrate;
            ForceMono = forceMono;
            Format = format;
            LoadType = loadType;
            IsSelected = false;
            HasChanged = false;
            LastUpdated = DateTime.MinValue.Ticks;
        }
    }

    private static GUIStyle ToolbarSeachCancelButton { get { return GetStyle("ToolbarSeachCancelButton"); } }

    private static GUIStyle GetStyle(string styleName) {
        var guiStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        if (guiStyle != null) {
            return guiStyle;
        }

        Debug.LogError("Missing built-in guistyle " + styleName);
        guiStyle = GUI.skin.button;
        return guiStyle;
    }

    private void CopyBitrateToSelected() {
        foreach (var aClip in SelectedClips) {
            aClip.CompressionBitrate = _bulkBitrate;
        }
    }

    private void Copy3DToSelected() {
        foreach (var aClip in SelectedClips) {
            aClip.Is3D = _bulk3D;
        }
    }

    private void CopyForceMonoToSelected() {
        foreach (var aClip in SelectedClips) {
            aClip.ForceMono = _bulkForceMono;
        }
    }

    private void CopyFormatToSelected() {
        foreach (var aClip in SelectedClips) {
            aClip.Format = _bulkFormat;
        }
    }

    private void CopyLoadTypeToSelected() {
        foreach (var aClip in SelectedClips) {
            aClip.LoadType = _bulkLoadType;
        }
    }
}
#endif
