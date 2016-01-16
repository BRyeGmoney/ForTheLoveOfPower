Shader "Hidden/FxPro" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ChromAberrTex ("Chromatic Aberration (RGB)", 2D) = "black" {}
		_LensDirtTex ("Lens Dirt Texture", 2D) = "black" {}
		_DirtIntensity ("Lens Dirt Intensity", Float) = .1
		_ChromaticAberrationOffset("Chromatic Aberration Offset", Float) = 1

		_BloomTex("Bloom (RGBA)", 2D) = "black" {}
		_DOFTex("DOF (RGB), COC(A)", 2D) = "black" {}
//		_DOFStrength("DOF Strength", Float) = .5
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

		struct v2f_img_aa {
			float4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			half2 uv2 : TEXCOORD1;	//Flipped uv on DirectX platforms to work correctly with AA
		};

		v2f_img_aa vert_img_aa(appdata_img v)
		{
			v2f_img_aa o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord;
			o.uv2 = v.texcoord;

			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv2.y = 1 - o.uv2.y;
			#endif

			return o;
		}
	ENDCG

	SubShader 
	{
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		Blend Off
		
		
		Pass {	//[Pass 0] Bloom/DOF final composite
			name "bloom_dof_composite"
			CGPROGRAM
			#pragma vertex vert_img_aa
			#pragma fragment frag
 	
 			#pragma multi_compile LENS_DIRT_ON LENS_DIRT_OFF
			#pragma multi_compile CHROMATIC_ABERRATION_ON CHROMATIC_ABERRATION_OFF
			#pragma multi_compile DOF_ENABLED DOF_DISABLED
			#pragma multi_compile BLOOM_ENABLED BLOOM_DISABLED

 			#ifdef CHROMATIC_ABERRATION_ON
 			sampler2D _ChromAberrTex;
 			half4 _ChromAberrTex_TexelSize;
			#endif

			#if defined(DOF_ENABLED) | defined(DOF_SIMPLIFIED)
			sampler2D _DOFTex;
			#endif
			
			sampler2D _BloomTex;

			#ifdef LENS_DIRT_ON
			sampler2D _LensDirtTex;
			half _DirtIntensity;
			#endif
			
			fixed4 frag ( v2f_img_aa i ) : COLOR
			{	
				
				fixed4 mainTex = tex2D(_MainTex, i.uv);

				#ifdef DOF_ENABLED
					fixed4 srcTex = tex2D(_DOFTex, i.uv2);
					srcTex.rgb = lerp(mainTex.rgb, srcTex.rgb, srcTex.a);
				#else
					fixed4 srcTex = mainTex;
				#endif


				#ifdef BLOOM_ENABLED
					fixed4 bloomTex = tex2D(_BloomTex, i.uv2);
					fixed3 resColor = Screen(srcTex.rgb, bloomTex.rgb);
				#else
					fixed4 bloomTex = fixed4(0, 0, 0, 0);
					fixed3 resColor = srcTex.rgb;
				#endif
				
				#ifdef LENS_DIRT_ON
				fixed3 lensDirtTex = tex2D(_LensDirtTex, i.uv2).rgb;
				resColor = Screen(resColor, saturate(lensDirtTex * max(bloomTex.rgb, srcTex.rgb) * _DirtIntensity));
				#endif
				
				#ifdef CHROMATIC_ABERRATION_ON
				fixed3 chromaticAberration = tex2D(_ChromAberrTex, i.uv2).rgb;
				
				chromaticAberration = saturate(chromaticAberration - srcTex.rgb);//Make sure not to make the overall image brighter - just add the abberation

				resColor = Screen(resColor, chromaticAberration);
				#endif
				
				return fixed4( resColor, mainTex.a );
			} 
			ENDCG
		}
		
		Pass 	//[Pass 1] Downsample
		{ 	
			CGPROGRAM			
			#pragma vertex vert
			#pragma fragment frag
			
			struct v2f {
				float4 pos : SV_POSITION;
				float4 uv[4] : TEXCOORD0;
			};
						
			v2f vert (appdata_img v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				float4 uv;
				uv.xy = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				uv.zw = 0;

				float offX = _MainTex_TexelSize.x;
				float offY = _MainTex_TexelSize.y;
				
				// Direct3D9 needs some texel offset!
				#ifdef UNITY_HALF_TEXEL_OFFSET
				uv.x += offX * 2.0f;
				uv.y += offY * 2.0f;
				#endif
				o.uv[0] = uv + float4(-offX,-offY,0,1);
				o.uv[1] = uv + float4( offX,-offY,0,1);
				o.uv[2] = uv + float4( offX, offY,0,1);
				o.uv[3] = uv + float4(-offX, offY,0,1);

				return o;
			}
			
			fixed4 frag( v2f i ) : SV_Target
			{
				fixed4 c;
				c  = tex2D( _MainTex, i.uv[0].xy );
				c += tex2D( _MainTex, i.uv[1].xy );
				c += tex2D( _MainTex, i.uv[2].xy );
				c += tex2D( _MainTex, i.uv[3].xy );
				c *= .25f;

				return c;
			}	
			ENDCG		 
		}
		
		Pass {	//[Pass 2]
			name "chromatic_aberration"
			CGPROGRAM
				#pragma vertex vert_img_aa
				#pragma fragment frag

				half _ChromaticAberrationOffset;
	
				inline fixed3 ChromaticAberration(sampler2D _tex, half2 _uv, half2 _texelSize, half _size) {
					fixed3 texOrig = tex2D(_tex, _uv).rgb;

					fixed3 texR = tex2D(_tex, _uv + half2(_texelSize.x, 0) * _size).rgb;
					fixed3 texG = tex2D(_tex, _uv + half2(-_texelSize.x, -_texelSize.y) * _size).rgb;
					fixed3 texB = tex2D(_tex, _uv + half2(-_texelSize.x, _texelSize.y) * _size).rgb;

					return fixed3(texR.r, texG.g, texB.b);
				}

				fixed4 frag (v2f_img_aa i) : COLOR  {
					fixed3 chromaticAberration = ChromaticAberration(_MainTex, i.uv, _MainTex_TexelSize.xy, _ChromaticAberrationOffset);
					return fixed4(chromaticAberration, 1);
				}
			ENDCG
		}
	}
	
	fallback off
}