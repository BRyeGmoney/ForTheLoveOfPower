using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SoundGroupAttribute))]
// ReSharper disable once CheckNamespace
public class SoundGroupPropertyDrawer : PropertyDrawer {
    // ReSharper disable once InconsistentNaming
    public int index;
    // ReSharper disable once InconsistentNaming
    public bool typeIn;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!typeIn) {
            return base.GetPropertyHeight(property, label);
        }
        return base.GetPropertyHeight(property, label) + 16;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var ma = MasterAudio.SafeInstance;
        // ReSharper disable once RedundantAssignment
        var groupName = "[Type In]";

        var groupNames = new List<string>();

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (ma != null) {
            groupNames.AddRange(ma.GroupNames);
        } else {
            groupNames.AddRange(MasterAudio.SoundGroupHardCodedNames);
        }

        var creators = Object.FindObjectsOfType(typeof(DynamicSoundGroupCreator)) as DynamicSoundGroupCreator[];
        // ReSharper disable once PossibleNullReferenceException
        foreach (var dsgc in creators) {
            var trans = dsgc.transform;
            for (var i = 0; i < trans.childCount; ++i) {
                var group = trans.GetChild(i).GetComponent<DynamicSoundGroup>();
                if (group != null) {
                    groupNames.Add(group.name);
                }
            }
        }

        groupNames.Sort();
        if (groupNames.Count > 1) { // "type in" back to index 0 (sort puts it at #1)
            groupNames.Insert(0, groupNames[1]);
        }

        if (groupNames.Count == 0) {
            index = -1;
            typeIn = false;
            property.stringValue = EditorGUI.TextField(position, label.text, property.stringValue);
            return;
        }

        index = groupNames.IndexOf(property.stringValue);

        if (typeIn || index == -1) {
            index = 0;
            typeIn = true;
            position.height -= 16;
        }

        index = EditorGUI.Popup(position, label.text, index, groupNames.ToArray());
        groupName = groupNames[index];

        switch (groupName) {
            case "[Type In]":
                typeIn = true;
                position.yMin += 16;
                position.height += 16;
                EditorGUI.BeginChangeCheck();
                property.stringValue = EditorGUI.TextField(position, label.text, property.stringValue);
                EditorGUI.EndChangeCheck();
                break;
            default:
                typeIn = false;
                property.stringValue = groupName;
                break;
        }
    }
}
