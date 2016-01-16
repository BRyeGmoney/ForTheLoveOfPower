//#define FXPRO_EFFECT
#define BLOOMPRO_EFFECT
//#define DOFPRO_EFFECT

#if FXPRO_EFFECT
	#define BLOOMPRO_EFFECT
	#define DOFPRO_EFFECT
#endif

using System.IO;

using UnityEngine;
using UnityEditor;
using BloomProNS;

#if FXPRO_EFFECT
[CustomEditor( typeof( FxPro ) )]
public class FxProEditor : Editor
#elif BLOOMPRO_EFFECT
[CustomEditor( typeof( BloomPro ) )]
public class BloomProEditor : Editor
#elif DOFPRO_EFFECT
[CustomEditor( typeof( DOFPro ) )]
public class DOFProEditor : Editor
#endif
{
	private Texture2D logo;

	private SerializedObject serializedObj;

    private SerializedProperty quality;

	private SerializedProperty lensDirtTexture, lensDirtIntensity, chromaticAberration, chromaticAberrationOffset;

    //Bloom properties
	#if BLOOMPRO_EFFECT
    private SerializedProperty bloomParameters, bloomEnabled, bloomThreshold, bloomIntensity, bloomSoftness;
	#endif

    //DOF properties
    #if DOFPRO_EFFECT
    private SerializedProperty dofParameters, dofEnabled, blurCOCTexture, visualizeCOC, useUnityDepthBuffer, autoFocus, autoFocusLayerMask, autoFocusSpeed,
                                focalLengthMultiplier, depthCompression, dofTarget, dofBlurSize;
    #endif

    private EffectsQuality prevQuality;

	void OnEnable()
	{
		serializedObj = new SerializedObject(target);

        quality = serializedObj.FindProperty( "Quality" );

		lensDirtTexture = serializedObj.FindProperty( "LensDirtTexture" );
		lensDirtIntensity = serializedObj.FindProperty( "LensDirtIntensity" );
		chromaticAberration = serializedObj.FindProperty( "ChromaticAberration" );
		chromaticAberrationOffset = serializedObj.FindProperty( "ChromaticAberrationOffset" );


		#if BLOOMPRO_EFFECT
        //Bloom
        bloomParameters = serializedObj.FindProperty( "BloomParams" );

        bloomEnabled = serializedObj.FindProperty( "BloomEnabled" );

        bloomThreshold          = bloomParameters.FindPropertyRelative( "BloomThreshold" );
        bloomIntensity          = bloomParameters.FindPropertyRelative( "BloomIntensity" );
		bloomSoftness           = bloomParameters.FindPropertyRelative( "BloomSoftness" );
		#endif

	    //DOF
		#if DOFPRO_EFFECT
        dofParameters           = serializedObj.FindProperty( "DOFParams" );

        dofEnabled              = serializedObj.FindProperty( "DOFEnabled" );
        blurCOCTexture          = serializedObj.FindProperty( "BlurCOCTexture" );
        visualizeCOC            = serializedObj.FindProperty( "VisualizeCOC" );

        dofBlurSize             = dofParameters.FindPropertyRelative( "DOFBlurSize" );
        useUnityDepthBuffer     = dofParameters.FindPropertyRelative( "UseUnityDepthBuffer" );
        autoFocus               = dofParameters.FindPropertyRelative( "AutoFocus" );
        autoFocusLayerMask      = dofParameters.FindPropertyRelative( "AutoFocusLayerMask" );
        autoFocusSpeed          = dofParameters.FindPropertyRelative( "AutoFocusSpeed" );
        focalLengthMultiplier   = dofParameters.FindPropertyRelative( "FocalLengthMultiplier" );
        depthCompression        = dofParameters.FindPropertyRelative( "DepthCompression" );
        dofTarget               = dofParameters.FindPropertyRelative( "Target" );
		#endif

        //Load dynamic resources
		string pluginPath = FilePathToAssetPath( GetPluginPath() );
		
		var lensTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(pluginPath + "/Lens/lens_01.png");
		
		if (null == lensDirtTexture.objectReferenceValue) {
			lensDirtTexture.objectReferenceValue = lensTexture;
			serializedObj.ApplyModifiedProperties();
		}

	    string logoPath = Path.Combine(pluginPath, "Editor");
        logoPath = Path.Combine(logoPath, "banner.png");

        logo = AssetDatabase.LoadAssetAtPath<Texture2D>(logoPath);

	    if (null == logo)
            Debug.LogError("null == logo");

	    prevQuality = null == quality ? EffectsQuality.Normal : (EffectsQuality)quality.enumValueIndex;
	}
	
	private string AssetPathToFilePath(string assetPath) {
		return Application.dataPath + "/" + assetPath.Remove( assetPath.IndexOf("Assets/"), "Assets/".Length);
	}
	
	private string FilePathToAssetPath(string filePath) {
	    int indexOfAssets = filePath.LastIndexOf("Assets");

        return filePath.Substring(indexOfAssets);
	}
	
	private string GetPluginPath() {
		MonoScript ms = MonoScript.FromScriptableObject( this );
		string scriptPath = AssetDatabase.GetAssetPath( ms );

	    var directoryInfo = Directory.GetParent( scriptPath ).Parent;
	    return directoryInfo != null ? directoryInfo.FullName : null;
	}

    private void CheckIfQualityChanged()
    {
        if ( (EffectsQuality)quality.enumValueIndex == prevQuality)
            return;


		#if DOFPRO_EFFECT
        //Set default quality values
        if ((EffectsQuality)quality.enumValueIndex == EffectsQuality.Fastest) {
            chromaticAberration.boolValue = false;
            blurCOCTexture.boolValue = false;
            useUnityDepthBuffer.boolValue = false;
        }

        //Fastest => any other
        if (prevQuality == EffectsQuality.Fastest) {
            chromaticAberration.boolValue = true;
            blurCOCTexture.boolValue = true;
        }
		#endif

        prevQuality = (EffectsQuality)quality.enumValueIndex;
    }

	public override void OnInspectorGUI()
	{	
		serializedObj.Update();
		
		EditorGUILayout.Space();

		float bannerWidth = Screen.width - 35;
		EditorGUILayout.LabelField( new GUIContent( logo ), GUILayout.Width( bannerWidth ), GUILayout.Height( bannerWidth / 6 ) );
		//EditorGUILayout.LabelField( new GUIContent( logo ), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) );
		
		GUI.backgroundColor = new Color(.74f, .74f, 1f, 1f);
		GUI.contentColor = new Color(.8f, .8f, 1f, 1f);
		EditorGUILayout.Space();
		
		EditorGUILayout.PropertyField(quality, new GUIContent("Quality", "Set to lower value if you experience performance issues."));
		
        EditorGUILayout.Space();
        //EditorGUILayout.Space();

        //
        //Bloom
		#if BLOOMPRO_EFFECT
        EditorGUILayout.PropertyField( bloomEnabled, new GUIContent( "Bloom", "Makes bright pixels bloom." ) );

        if (bloomEnabled.boolValue)
	    {
            //EditorGUILayout.Space();

            EditorGUILayout.BeginVertical( "box" );

            EditorGUILayout.Space();

            //bloomEnabled.boolValue = EditorGUILayout.BeginToggleGroup( new GUIContent( "Bloom", "Makes bright pixels bloom." ), bloomEnabled.boolValue );
            EditorGUILayout.PropertyField( bloomThreshold, new GUIContent( "Bloom Threshold", "Higher value = less blooming pixels. Set close to zero for a dreamy look." ) );
            EditorGUILayout.PropertyField( bloomIntensity, new GUIContent( "Bloom Intensity", "Higher value = brighter bloom." ) );
            EditorGUILayout.PropertyField( bloomSoftness, new GUIContent( "Bloom Softness", "Lower value = harder bloom edge. Higher value = softer bloom." ) );

	        EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
	    }

        EditorGUILayout.Space();
        //EditorGUILayout.Space();
        EditorGUILayout.Space();
		#endif

	    //
        //Depth of Field
		#if DOFPRO_EFFECT
        EditorGUILayout.PropertyField( dofEnabled, new GUIContent( "Depth of Field", "Blurs out-of-focus areas." ) );

        if (dofEnabled.boolValue)
	    {
            //EditorGUILayout.Space();

            EditorGUILayout.BeginVertical( "box" );

            EditorGUILayout.Space();

	        //dofEnabled.boolValue = EditorGUILayout.BeginToggleGroup( new GUIContent("Depth of Field", "Blurs out-of-focus areas."), dofEnabled.boolValue);

            EditorGUILayout.PropertyField( blurCOCTexture, new GUIContent( "Blur COC", "Makes DOF look correct at edges of objects. Has performance impact." ) );

            EditorGUILayout.PropertyField( visualizeCOC, new GUIContent( "Visualize COC", "Circle of Confusion (bluriness) visualization. Use for testing purposes only." ) );

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField( autoFocus, new GUIContent( "Autofocus", "Makes camera focus automatically on objects in the center of the screen." +
	                                                                             "Requires a collider attached to focusable objects.") );

            if (autoFocus.boolValue) EditorGUILayout.PropertyField( autoFocusLayerMask, new GUIContent( "\tLayers", "Autofocus Speed" ) );

            if (autoFocus.boolValue) EditorGUILayout.PropertyField( autoFocusSpeed, new GUIContent( "\tSpeed", "Autofocus Speed" ) );

            if (!autoFocus.boolValue) EditorGUILayout.PropertyField( dofTarget, new GUIContent( "Focus On", "Drop here a scene object that you'd like the camera to focus on." ) );

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField( useUnityDepthBuffer, new GUIContent( "Use Unity Depth Buffer", "Has a big performance impact. It's recommended to to disable this option on mobile devices, " +
	                                                                                                    "and to make all shaders output depth to alpha channel (refer to manual for details)"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField( focalLengthMultiplier, new GUIContent( "Focal Length Multiplier", "Higher values result in shallower depth-of-field." ) );
            //DOFParams.focalDistMultiplier = EditorGUILayout.FloatField("Focal Dist Multiplier", DOFParams.focalDistMultiplier);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField( depthCompression, new GUIContent( "Depth Compression", "Compresses depth-map by moving camera's far clipping plane closer to improve COC-map quality." +
                                                                                                         "In most cases it's recommended to use the default value.") );

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField( dofBlurSize, new GUIContent( "DOF Strength", "Higher values will make out-of-focus areas appear blurrier." ) );

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
	    }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        //EditorGUILayout.Space();
		#endif

        //General effects
        {
            EditorGUILayout.BeginVertical( "box" );

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField( lensDirtIntensity, new GUIContent( "Lens Dirt Intensity", "Lens Dirt Intensity" ) );

            if (lensDirtIntensity.floatValue > .0001f) EditorGUILayout.PropertyField( lensDirtTexture, new GUIContent( "Texture", "Lens Dirt Texture" ) );

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField( chromaticAberration, new GUIContent( "Chromatic Aberration", "Simulates real camera lens' chromatic aberration." ) );

            if (chromaticAberration.boolValue) EditorGUILayout.PropertyField( chromaticAberrationOffset, new GUIContent( "Offset", "Larger value gives larger chromatic aberration effect." ) );

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        CheckIfQualityChanged();

	    serializedObj.ApplyModifiedProperties();
	}
}