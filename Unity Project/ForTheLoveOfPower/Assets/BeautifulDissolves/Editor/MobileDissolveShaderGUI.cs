using System;
using UnityEngine;

namespace UnityEditor
{
	internal class MobileDissolveShaderGUI : ShaderGUI
	{
		private static class Styles
		{
			public static string emptyTootip = "";
			public static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
			public static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map");
			public static GUIContent dissolveMapText = new GUIContent("Dissolve Map", "Dissolve Map (RGB)");
			public static GUIContent directionMapText = new GUIContent("Direction Map", "Direction of Dissolve (RGB)");
			public static GUIContent dissolveAmountText = new GUIContent("Dissolve Amount", "Dissolve Amount");
			public static GUIContent substituteText = new GUIContent("Substitute Texture", "Substitute Texture (RGB)");
			public static GUIContent outerEdgeText = new GUIContent("Outer Edge", "");
			public static GUIContent innerEdgeText = new GUIContent("Inner Edge", "");
			public static GUIContent edgeThicknessText = new GUIContent("Thickness", "");
			public static GUIContent blendColorsText = new GUIContent("Edge Color Blending", "");
			public static GUIContent edgeGlowText = new GUIContent("Edge Glow", "");
			public static GUIContent burnInText = new GUIContent("Dissolve Glow", "");
			public static GUIContent burnColorText = new GUIContent("Glow Color", "");
			public static GUIContent burnIntensity = new GUIContent("Glow Intensity", "");
			public static GUIContent shininessText = new GUIContent("Shininess", "");
			public static GUIContent glowFollowText = new GUIContent("Follow-Through", "");

			public static string whiteSpaceString = " ";
			public static string primaryMapsText = "Main Maps";
			public static string dissolveSettings = "Dissolve Settings";
		}

		MaterialProperty albedoMap = null;
		MaterialProperty bumpMap = null;
		MaterialProperty dissolveMap = null;
		MaterialProperty directionMap = null;
		MaterialProperty dissolveAmount = null;
		MaterialProperty substituteMap = null;
		MaterialProperty outerEdgeColor = null;
		MaterialProperty innerEdgeColor = null;
		MaterialProperty outerEdgeThickness = null;
		MaterialProperty innerEdgeThicnkess = null;
		MaterialProperty colorBlend = null;
		MaterialProperty edgeGlow = null;
		MaterialProperty dissolveGlow = null;
		MaterialProperty glowColor = null;
		MaterialProperty glowIntensity = null;
		MaterialProperty shininess = null;
		MaterialProperty glowFollow = null;

		MaterialEditor m_MaterialEditor;

		bool m_FirstTimeApply = true;

		public void FindProperties (MaterialProperty[] props)
		{
			albedoMap = FindProperty ("_MainTex", props);
			bumpMap = FindProperty ("_BumpMap", props);
			dissolveMap = FindProperty ("_DissolveMap", props);
			directionMap = FindProperty ("_DirectionMap", props);
			dissolveAmount = FindProperty ("_DissolveAmount", props);
			substituteMap = FindProperty ("_SubTex", props);
			outerEdgeColor = FindProperty ("_OuterEdgeColor", props);
			innerEdgeColor = FindProperty ("_InnerEdgeColor", props);
			outerEdgeThickness = FindProperty ("_OuterEdgeThickness", props);
			innerEdgeThicnkess = FindProperty ("_InnerEdgeThickness", props);
			colorBlend = FindProperty ("_ColorBlending", props);
			edgeGlow = FindProperty ("_EdgeGlow", props);
			dissolveGlow = FindProperty ("_DissolveGlow", props);
			glowColor = FindProperty ("_GlowColor", props);
			glowIntensity = FindProperty ("_GlowIntensity", props);
			shininess = FindProperty("_Shininess", props, false);
			glowFollow = FindProperty("_GlowFollow", props);
		}

		public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
		{
			FindProperties (props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
			m_MaterialEditor = materialEditor;
			Material material = materialEditor.target as Material;

			ShaderPropertiesGUI (material);

			// Make sure that needed keywords are set up if we're switching some existing
			// material to a standard shader.
			if (m_FirstTimeApply)
			{
				SetMaterialKeywords (material);
				m_FirstTimeApply = false;
			}
		}

		public void ShaderPropertiesGUI (Material material)
		{
			// Use default labelWidth
			EditorGUIUtility.labelWidth = 0f;

			// Detect any changes to the material
			EditorGUI.BeginChangeCheck();
			{
				// Primary properties
				GUILayout.Label (Styles.primaryMapsText, EditorStyles.boldLabel);
				DoAlbedoArea();
				m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap);
				m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);

				EditorGUILayout.Space();

				// Dissolve settings
				GUILayout.Label(Styles.dissolveSettings, EditorStyles.boldLabel);
				DoDissolveArea(material);

			}
			if (EditorGUI.EndChangeCheck())
			{
				SetMaterialKeywords(material);
			}
		}

		public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader)
		{
			base.AssignNewShaderToMaterial(material, oldShader, newShader);

			if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
				return;

			SetMaterialKeywords(material);
		}

		void DoDissolveArea(Material material)
		{
			m_MaterialEditor.TexturePropertySingleLine(Styles.dissolveMapText, dissolveMap);

			if (material.GetTexture("_DissolveMap") != null) {
				m_MaterialEditor.ShaderProperty(dissolveAmount, Styles.dissolveAmountText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
			}

			m_MaterialEditor.TexturePropertySingleLine(Styles.directionMapText, directionMap);
			m_MaterialEditor.TexturePropertySingleLine(Styles.substituteText, substituteMap);

			EditorGUILayout.Space();
			m_MaterialEditor.ShaderProperty(dissolveGlow, Styles.burnInText.text);

			if (material.IsKeywordEnabled("_DISSOLVEGLOW_ON")) {
				m_MaterialEditor.ShaderProperty(glowFollow, Styles.glowFollowText.text);
				m_MaterialEditor.ColorProperty(glowColor, Styles.burnColorText.text);
				m_MaterialEditor.FloatProperty(glowIntensity, Styles.burnIntensity.text);
			}

			EditorGUILayout.Space();
			m_MaterialEditor.ShaderProperty(edgeGlow, Styles.edgeGlowText.text);
			m_MaterialEditor.ShaderProperty(colorBlend, Styles.blendColorsText.text);
			m_MaterialEditor.ColorProperty(outerEdgeColor, Styles.outerEdgeText.text);
			m_MaterialEditor.ShaderProperty(outerEdgeThickness, Styles.edgeThicknessText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
			m_MaterialEditor.ColorProperty(innerEdgeColor, Styles.innerEdgeText.text);
			m_MaterialEditor.ShaderProperty(innerEdgeThicnkess, Styles.edgeThicknessText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);

			EditorGUILayout.Space();
		}

		void DoAlbedoArea()
		{
			m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap);

			if (shininess != null) {
				m_MaterialEditor.ShaderProperty(shininess, Styles.shininessText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
			}
		}

		static void SetMaterialKeywords(Material material)
		{
			// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
			// (MaterialProperty value might come from renderer material property block)
			SetKeyword (material, "_NORMALMAP", material.GetTexture ("_BumpMap"));// || material.GetTexture ("_DetailNormalMap"));
			SetKeyword (material, "_SUBMAP", material.GetTexture ("_SubTex"));
			SetKeyword (material, "_DISSOLVEMAP", material.GetTexture ("_DissolveMap"));
			SetKeyword (material, "_DIRECTIONMAP", material.GetTexture ("_DirectionMap"));
		}

		static void SetKeyword(Material m, string keyword, bool state)
		{
			if (state)
				m.EnableKeyword (keyword);
			else
				m.DisableKeyword (keyword);
		}
	}

} // namespace UnityEditor
