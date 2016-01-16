Shader "Hidden/BloomPro" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DsTex1 ("DsTexture 1 (RGB)", 2D) = "black" {}
		_DsTex2 ("DsTexture 2 (RGB)", 2D) = "black" {}
		_DsTex3 ("DsTexture 3 (RGB)", 2D) = "black" {}
		_DsTex4 ("DsTexture 4 (RGB)", 2D) = "black" {}
		_DsTex5 ("DsTexture 5 (RGB)", 2D) = "black" {}
		_BloomThreshold ("Bloom Threshold", Float) = .8
		_BloomIntensity ("Bloom Intensity", Float) = 5
		_BloomTexFactors1 ("Bloom Tex Factors 1", Vector) = (.166, .166, .166, .166)
		_BloomTexFactors2 ("Bloom Tex Factors 2", Vector) = (.166, .166, .0, .0)
	}
	
	
	CGINCLUDE
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma glsl
		#pragma fragmentoption ARB_precision_hint_fastest
		
		sampler2D _MainTex;
		half4 _MainTex_TexelSize;

		inline fixed3 Screen(fixed3 _a, fixed3 _b) {
			return 1 - (1 - _a) * (1 - _b);
		}

		inline fixed4 Screen(fixed4 _a, fixed4 _b) {
			return 1 - (1 - _a) * (1 - _b);
		}

		v2f_img vert_img_aa(appdata_img v)
		{
			v2f_img o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord;

			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv.y =  1 - o.uv.y;
			#endif
			return o;
		}
	ENDCG

	SubShader 
	{
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		Blend Off
		
		Pass 	//[Pass 0]	bloom amount => alpha
		{
			name "bloom_to_alpha"
			CGPROGRAM			
			#pragma vertex vert_img_aa
			#pragma fragment fragBloomAmountToAlpha

			//sampler2D _MainTex;
			//half4 _MainTex_TexelSize;

			half _BloomThreshold;
			
			fixed4 fragBloomAmountToAlpha ( v2f_img i ) : COLOR
			{
				fixed4 mainTex = tex2D(_MainTex, i.uv);
				fixed finalLum = Luminance( mainTex.rgb );
				fixed lumFromChannels = max( max(mainTex.r, mainTex.g), mainTex.b) * .5;
				
				finalLum = max( finalLum, lumFromChannels );
				finalLum = ( finalLum - _BloomThreshold) / (1 - _BloomThreshold);
				
//				finalLum = sqrt( finalLum );
				
//				finalLum *= ( 1.0 + (finalLum/(_BloomThreshold * _BloomThreshold) ) );
//				finalLum -= .5;
//				finalLum /= (1.0 + finalLum);
				
				mainTex.a = finalLum;

				return mainTex;
			}			
			ENDCG		 
		}
		
		Pass {	//[Pass 1] Bloom composite
			name "bloom_composite"
			CGPROGRAM
			#pragma vertex vert_img_aa
			#pragma fragment frag
 	
			#pragma multi_compile BLOOM_SAMPLES_3 BLOOM_SAMPLES_5
 			
			sampler2D _DsTex1;
			sampler2D _DsTex2;
			sampler2D _DsTex3;
			sampler2D _DsTex4;
			sampler2D _DsTex5;
			
			half4 _BloomTexFactors1;
			half4 _BloomTexFactors2;
			
			half _BloomIntensity;
			
			fixed4 frag ( v2f_img i ) : COLOR
			{	
				
				//fixed4 mainTex = tex2D(_MainTex, i.uv);
								
				fixed4 ds1 = tex2D(_DsTex1, i.uv);
				fixed4 ds2 = tex2D(_DsTex2, i.uv);
				fixed4 ds3 = tex2D(_DsTex3, i.uv);

				//#undef BLOOM_SAMPLES_5

				#ifdef BLOOM_SAMPLES_5
				fixed4 ds4 = tex2D(_DsTex4, i.uv);
				fixed4 ds5 = tex2D(_DsTex5, i.uv);
				#endif

				ds1.a *= _BloomTexFactors1.x; ds2.a *= _BloomTexFactors1.y; ds3.a *= _BloomTexFactors1.z;

				#ifdef BLOOM_SAMPLES_5
				ds4.a *= _BloomTexFactors1.w; ds5.a *= _BloomTexFactors2.x;
				#endif
				
				ds1.rgb *= ds1.a;	ds2.rgb *= ds2.a;	ds3.rgb *= ds3.a;

				#ifdef BLOOM_SAMPLES_5
				ds4.rgb *= ds4.a;	ds5.rgb *= ds5.a;
				#endif

				fixed4	bloomFinal = Screen( ds1, ds2 );
						bloomFinal = Screen( bloomFinal, ds3 );

				#ifdef BLOOM_SAMPLES_5
						bloomFinal = Screen( bloomFinal, ds4 );
						bloomFinal = Screen( bloomFinal, ds5 );
				#endif
	
				bloomFinal.rgb = bloomFinal.rgb * _BloomIntensity;
	
				return bloomFinal;

				/*fixed3 resColor = Screen(mainTex.rgb, bloomFinal.rgb);
				
				return fixed4( resColor, mainTex.a);*/
			} 
			ENDCG
		}

		

		Pass {	//[Pass 2]
			name "blur"
			
			CGPROGRAM
			#pragma vertex vert_img_aa
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			//sampler2D _MainTex;
			//half4 _MainTex_TexelSize;
			
//			#define BLUR_RADIUS_10
//			#define BLUR_RADIUS_5
//			#define BLUR_RADIUS_2
//			#define BOX_KERNEL
			
			#define GAUSSIAN_KERNEL
			
			//Blur depth channel as well
			#define BLUR_ALPHA_CHANNEL
			
			#include "SeparableBlur.cginc"

			fixed4 frag (v2f_img i) : COLOR  {
				return BlurTex(_MainTex, i, 1.0);
			}
		ENDCG
		}
	}
	
	fallback off
}