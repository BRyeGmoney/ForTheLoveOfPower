using UnityEngine;
using UnityEditor;

namespace BeautifulDissolves {
	[CustomEditor(typeof(DissolveGlowUpdate)), CanEditMultipleObjects]
	public class DissolveGlowUpdateEditor : Editor {

		private GameObject m_GameObject;
		private Renderer m_Renderer;
		private static string m_CreateLightText = "Create Light Source";
		private static string m_UpdateLightText = "Update Light Source";

		private SerializedObject m_SerializedObject;
		private SerializedProperty script;
		private SerializedProperty startMode;
		private SerializedProperty updateRate;
		private SerializedProperty glowSource;
		private SerializedProperty glowCutoff;
		private SerializedProperty glowLightSource;
		private SerializedProperty frameDelay;
		private SerializedProperty updateTimestep;

		void OnEnable()
		{
			m_GameObject = ((DissolveGlowUpdate)target).gameObject;
			m_Renderer = m_GameObject.GetComponentInChildren<Renderer>();

			m_SerializedObject = new SerializedObject(target);
			script = m_SerializedObject.FindProperty("m_Script");
			startMode = m_SerializedObject.FindProperty("m_StartMode");
			updateRate = m_SerializedObject.FindProperty("m_UpdateRate");
			glowSource = m_SerializedObject.FindProperty("m_GlowSource");
			glowCutoff = m_SerializedObject.FindProperty("m_GlowCutoff");
			glowLightSource = m_SerializedObject.FindProperty("m_GlowLightSource");
			frameDelay = m_SerializedObject.FindProperty("m_FrameDelay");
			updateTimestep = m_SerializedObject.FindProperty("m_UpdateTimestep");
		}

		public override void OnInspectorGUI()
		{
			m_SerializedObject.Update();

			EditorGUILayout.PropertyField(script);
			EditorGUILayout.PropertyField(startMode);
			EditorGUILayout.PropertyField(updateRate);

			if (updateRate.enumValueIndex == 1) {
				EditorGUILayout.PropertyField(frameDelay);
			}

			if (updateRate.enumValueIndex == 2) {
				EditorGUILayout.PropertyField(updateTimestep);
			}

			EditorGUILayout.PropertyField(glowSource);

			if (glowSource.enumValueIndex == 0) {
				if (m_Renderer.GetType() == typeof(SpriteRenderer)) {
					EditorGUILayout.HelpBox("Emissive glow does not work on sprites. Please use the 'Light' glow source mode instead.", MessageType.Warning, false);
				} else if (!m_GameObject.isStatic) {
					EditorGUILayout.HelpBox("Emissive glow only works on static objects. Please use the 'Light' glow source mode if you require glow on dynamic objects.", MessageType.Warning, false);
				}
			} else {
				EditorGUILayout.PropertyField(glowLightSource);
				
				if (GUILayout.Button(glowLightSource.objectReferenceValue == null ? m_CreateLightText : m_UpdateLightText)) {
					((DissolveGlowUpdate)target).CreateLightSource();
				}
			}

			EditorGUILayout.PropertyField(glowCutoff);

			m_SerializedObject.ApplyModifiedProperties();
		}
	}
}
