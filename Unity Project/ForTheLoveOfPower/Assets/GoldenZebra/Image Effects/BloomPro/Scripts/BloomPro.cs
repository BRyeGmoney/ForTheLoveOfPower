//#define FXPRO_EFFECT
#define BLOOMPRO_EFFECT
//#define DOFPRO_EFFECT

#if FXPRO_EFFECT
#define BLOOMPRO_EFFECT
#define DOFPRO_EFFECT
#endif

using System.Security.Cryptography;
using UnityEngine;

#if FXPRO_EFFECT
using FxProNS;
#elif BLOOMPRO_EFFECT
using BloomProNS;
#elif DOFPRO_EFFECT
using DOFProNS;
#endif

[ExecuteInEditMode]
[RequireComponent( typeof( Camera ) )]
#if FXPRO_EFFECT
[AddComponentMenu( "Image Effects/FxPro™" )]
public class FxPro : MonoBehaviour
#elif BLOOMPRO_EFFECT
	[AddComponentMenu( "Image Effects/BloomPro™" )]
	public class BloomPro : MonoBehaviour
#elif DOFPRO_EFFECT
	[AddComponentMenu( "Image Effects/DOF Pro™" )]
	public class DOFPro : MonoBehaviour	
#endif
{
    public EffectsQuality Quality = EffectsQuality.Normal;

    private static Material _mat;

    public static Material Mat
    {
        get
        {
            if ( null == _mat )
                _mat = new Material( Shader.Find( "Hidden/FxPro" ) )
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

            return _mat;
        }
    }

    private static Material _tapMat;

    private static Material TapMat
    {
        get
        {
            if ( null == _tapMat )
                _tapMat = new Material( Shader.Find( "Hidden/FxProTap" ) )
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

            return _tapMat;
        }
    }

    //Bloom
#if BLOOMPRO_EFFECT
    public bool BloomEnabled = true;
    public BloomHelperParams BloomParams = new BloomHelperParams();
#endif

    public Texture2D LensDirtTexture = null;

    [Range( 0f, 2f )]
    public float LensDirtIntensity = 1f;

    public bool ChromaticAberration = true;

    [Range( 1f, 2.5f )]
    public float ChromaticAberrationOffset = 1f;

    //Depth of Field
#if DOFPRO_EFFECT
    public bool DOFEnabled = true;
    public bool BlurCOCTexture = true;
    public DOFHelperParams DOFParams = new DOFHelperParams();

    public bool VisualizeCOC = false;
#endif

    public void Start()
    {
        if ( !SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures )
        {
            Debug.LogError( "Image effects are not supported on this platform." );
            enabled = false;
            return;
        }
    }

    public void Init( bool searchForNonDepthmapAlphaObjects )
    {
        Mat.SetFloat( "_DirtIntensity", Mathf.Exp( LensDirtIntensity ) - 1f );

        if ( null == LensDirtTexture || LensDirtIntensity <= 0f )
        {
            Mat.DisableKeyword( "LENS_DIRT_ON" );
            Mat.EnableKeyword( "LENS_DIRT_OFF" );
        } else
        {
            Mat.SetTexture( "_LensDirtTex", LensDirtTexture );
            Mat.EnableKeyword( "LENS_DIRT_ON" );
            Mat.DisableKeyword( "LENS_DIRT_OFF" );
        }

        if ( ChromaticAberration )
        {
            Mat.EnableKeyword( "CHROMATIC_ABERRATION_ON" );
            Mat.DisableKeyword( "CHROMATIC_ABERRATION_OFF" );
        } else
        {
            Mat.EnableKeyword( "CHROMATIC_ABERRATION_OFF" );
            Mat.DisableKeyword( "CHROMATIC_ABERRATION_ON" );
        }

        //
        //Depth of Field
#if DOFPRO_EFFECT
        if ( DOFEnabled )
        {

            if ( null == DOFParams.EffectCamera )
            {
                DOFParams.EffectCamera = GetComponent<Camera>();
            }

            //Validating DOF parameters
            DOFParams.DepthCompression = Mathf.Clamp( DOFParams.DepthCompression, 2f, 8f );

            DOFHelper.Instance.SetParams( DOFParams );
            DOFHelper.Instance.Init( searchForNonDepthmapAlphaObjects );

            Mat.DisableKeyword( "DOF_DISABLED" );
            Mat.EnableKeyword( "DOF_ENABLED" );

            DOFHelper.Instance.SetBlurRadius( 5 );
        } else
        {
            Mat.EnableKeyword( "DOF_DISABLED" );
            Mat.DisableKeyword( "DOF_ENABLED" );
        }
#endif

        //
        //Bloom
#if BLOOMPRO_EFFECT
        if ( BloomEnabled )
        {
            BloomParams.Quality = Quality;
            BloomHelper.Instance.SetParams( BloomParams );
            BloomHelper.Instance.Init();

            Mat.DisableKeyword( "BLOOM_DISABLED" );
            Mat.EnableKeyword( "BLOOM_ENABLED" );
        } else
        {
            Mat.EnableKeyword( "BLOOM_DISABLED" );
            Mat.DisableKeyword( "BLOOM_ENABLED" );
        }
#endif

        Mat.SetFloat( "_ChromaticAberrationOffset", ChromaticAberrationOffset );
    }

    public void OnEnable()
    {
        Init( true );
    }

    public void OnDisable()
    {
        if ( null != Mat )
            DestroyImmediate( Mat );

        RenderTextureManager.Instance.Dispose();

#if DOFPRO_EFFECT
        DOFHelper.Instance.Dispose();
#endif

#if BLOOMPRO_EFFECT
        BloomHelper.Instance.Dispose();
#endif
    }

    //
    //Settings:
    //
    //High:     10 blur, 5 samples
    //Normal:   5 blur, 5 samples
    //Fast:     5 blur, 3 samples
    //Fastest:  5 blur, 3 samples, 2 pre-samples



    public void OnValidate()
    {
        Init( false );
    }

    public static RenderTexture DownsampleTex( RenderTexture input, float downsampleBy )
    {
        RenderTexture tempRenderTex = RenderTextureManager.Instance.RequestRenderTexture( Mathf.RoundToInt( (float)input.width / downsampleBy ), Mathf.RoundToInt( (float)input.height / downsampleBy ), input.depth, input.format );
        tempRenderTex.filterMode = FilterMode.Bilinear;

        //Downsample pass
        //		Graphics.Blit(input, tempRenderTex, _mat, 1);

        const float off = 1f;
        Graphics.BlitMultiTap( input, tempRenderTex, TapMat,
            new Vector2( -off, -off ),
            new Vector2( -off, off ),
            new Vector2( off, off ),
            new Vector2( off, -off )
        );

        return tempRenderTex;
    }

    private RenderTexture ApplyChromaticAberration( RenderTexture input )
    {
        if ( !ChromaticAberration )
            return null;

        RenderTexture tempRenderTex = RenderTextureManager.Instance.RequestRenderTexture( input.width, input.height, input.depth, input.format );
        tempRenderTex.filterMode = FilterMode.Bilinear;

        //Chromatic aberration pass
        Graphics.Blit( input, tempRenderTex, Mat, 2 );

        Mat.SetTexture( "_ChromAberrTex", tempRenderTex );	//Chromatic abberation texture

        return tempRenderTex;
    }

    void RenderEffects( RenderTexture source, RenderTexture destination )
    {
        source.filterMode = FilterMode.Bilinear;

        //Optimization - render all at 1/4 resolution
        RenderTexture curRenderTex = DownsampleTex( source, 2f );

        if ( Quality == EffectsQuality.Fastest )
            RenderTextureManager.Instance.SafeAssign( ref curRenderTex, DownsampleTex( curRenderTex, 2f ) );

        //
        //Depth of Field
        //
        //Optimization: being rendered at 1/4 resolution
        //
#if DOFPRO_EFFECT
        RenderTexture cocRenderTex = null, dofRenderTex = null;
        if ( DOFEnabled )
        {
            if ( null == DOFParams.EffectCamera )
            {
                Debug.LogError( "null == DOFParams.camera" );
                return;
            }

            cocRenderTex = RenderTextureManager.Instance.RequestRenderTexture( curRenderTex.width, curRenderTex.height, curRenderTex.depth, curRenderTex.format );

            DOFHelper.Instance.RenderCOCTexture( curRenderTex, cocRenderTex, BlurCOCTexture ? 1.5f : 0f );

            if ( VisualizeCOC )
            {
                Graphics.Blit( cocRenderTex, destination, DOFHelper.Mat, 3 );
                RenderTextureManager.Instance.ReleaseRenderTexture( cocRenderTex );
                RenderTextureManager.Instance.ReleaseRenderTexture( curRenderTex );
                return;
            }

            dofRenderTex = RenderTextureManager.Instance.RequestRenderTexture( curRenderTex.width, curRenderTex.height, curRenderTex.depth, curRenderTex.format );

            DOFHelper.Instance.RenderDOFBlur( curRenderTex, dofRenderTex, cocRenderTex );
            Mat.SetTexture( "_DOFTex", dofRenderTex );

            //Make bloom DOF-based?
            //RenderTextureManager.Instance.SafeAssign(ref curRenderTex, dofRenderTex);
        }
#endif

        RenderTexture chromaticAberrationTex = null;

        //Chromatic Aberration; no DOF - on main texture, DOF - on DOF texture;
#if DOFPRO_EFFECT
        chromaticAberrationTex = ApplyChromaticAberration( !DOFEnabled ? curRenderTex : dofRenderTex );
#else
		chromaticAberrationTex = ApplyChromaticAberration( curRenderTex );
#endif

        //Render bloom
#if BLOOMPRO_EFFECT
        if ( BloomEnabled )
        {
            RenderTexture bloomTexture = RenderTextureManager.Instance.RequestRenderTexture( curRenderTex.width, curRenderTex.height, curRenderTex.depth, curRenderTex.format );
            BloomHelper.Instance.RenderBloomTexture( curRenderTex, bloomTexture );

            Mat.SetTexture( "_BloomTex", bloomTexture );
        }
#endif

        //Final composite pass
        Graphics.Blit( source, destination, Mat, 0 );
        //Graphics.Blit( bloomTexture, destination );

#if DOFPRO_EFFECT
        RenderTextureManager.Instance.ReleaseRenderTexture( cocRenderTex );
        RenderTextureManager.Instance.ReleaseRenderTexture( dofRenderTex );
#endif

        RenderTextureManager.Instance.ReleaseRenderTexture( curRenderTex );
        RenderTextureManager.Instance.ReleaseRenderTexture( chromaticAberrationTex );
    }

    public void OnRenderImage( RenderTexture source, RenderTexture destination )
    {
        RenderEffects( source, destination );
        RenderTextureManager.Instance.ReleaseAllRenderTextures();
    }
}
//Bloom:
//full screen chromab: 6.7ms; without - 4.9ms; 1/4 screen chromab: 5.2ms
//
//5 samples: 4.3 ms; 3 samples :3.6ms
//2 pre-samples, 3 total: 3.1 ms +27%
//
//HQ: 4.8ms
//NQ: 4.3ms
//FQ: 3.9ms
//FSQ: 3.4ms