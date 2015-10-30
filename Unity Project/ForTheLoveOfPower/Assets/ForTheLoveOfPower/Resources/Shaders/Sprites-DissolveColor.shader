Shader "Beautiful Dissolves/Sprites/Dissolve Color"
{
	Properties
	{
		_Color ("Tint", Color) = (1,1,1,1)
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_TilingX ("X", Float) = 1.0
		_TilingY ("Y", Float) = 1.0
		_SubColor ("Substitute Color", Color) = (1,1,1,1)
		_DissolveMap ("Dissolve Map", 2D) = "white" {}
		_DissolveAmount ("Dissolve Amount", Range(0.0, 1.0)) = 0.5
		_DirectionMap ("Direction Map", 2D) = "white" {}
		[Toggle(_DISSOLVEGLOW_ON)] _DissolveGlow ("Dissolve Glow", Int) = 1
		_GlowColor ("Glow Color", Color) = (1,0.5,0,1)
		_GlowIntensity ("Glow Intensity", Float) = 7
		_OuterEdgeColor ("Outer Edge Color", Color) = (1,0,0,1)
		_InnerEdgeColor ("Inner Edge Color", Color) = (1,1,0,1)
		_OuterEdgeThickness ("Outer Edge Thickness", Range(0.0, 1.0)) = 0.02
		_InnerEdgeThickness ("Inner Edge Thickness", Range(0.0, 1.0)) = 0.02
		[Toggle(_COLORBLENDING_ON)] _ColorBlending ("Color Blending", Int) = 1
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Toggle(_GLOWFOLLOW_ON)] _GlowFollow ("Follow-Through", Int) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma shader_feature _DISSOLVEMAP
			#pragma shader_feature _DIRECTIONMAP
			#pragma shader_feature _DISSOLVEGLOW_ON
			#pragma shader_feature _COLORBLENDING_ON
			#pragma shader_feature _GLOWFOLLOW_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   	: POSITION;
				float4 color    	: COLOR;
				float2 texcoord 	: TEXCOORD0;
				
			};

			struct v2f
			{
				float4 vertex   	: SV_POSITION;
				fixed4 color    	: COLOR;
				half2 texcoord  	: TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _DissolveMap;
			sampler2D _DirectionMap;
			fixed _DissolveAmount;
			fixed _OuterEdgeThickness;
			fixed _InnerEdgeThickness;
			fixed4 _OuterEdgeColor;
			fixed4 _InnerEdgeColor;
			half _GlowIntensity;
			fixed4 _GlowColor;
			fixed4 _SubColor;
			half _TilingX;
			half _TilingY;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 mt = tex2D(_MainTex, IN.texcoord);
				fixed4 color = mt;
				
				#ifdef _DISSOLVEMAP
					half2 tileAdjusted = half2(IN.texcoord.x * _TilingX, IN.texcoord.y * _TilingY);
					fixed4 dm = tex2D(_DissolveMap, tileAdjusted);
					fixed d = dm;
					#ifdef _DIRECTIONMAP
						d *= tex2D(_DirectionMap, tileAdjusted).rgb;
					#endif
					d -= _DissolveAmount;
					fixed totalThickness = _InnerEdgeThickness + _OuterEdgeThickness;
					fixed4 noSubColor = fixed4(0,0,0,0);
					
					#ifdef _COLORBLENDING_ON
						noSubColor = lerp(mt, lerp(_OuterEdgeColor, _InnerEdgeColor, d/(totalThickness + 0.01)), d < totalThickness);
					#else
						noSubColor = lerp(lerp(mt, _InnerEdgeColor, d < totalThickness), _OuterEdgeColor, d < _OuterEdgeThickness);
					#endif
					
					noSubColor = lerp(noSubColor, mt, d < 0);
					noSubColor *= lerp(IN.color, _SubColor, d < 0);
					color = noSubColor;
				
					#ifdef _DISSOLVEGLOW_ON
						fixed4 glow = pow(1 - dm, lerp(3, 1, _DissolveAmount)) * _DissolveAmount * _GlowIntensity * _GlowColor;
						
						#if defined(_SUBMAP) && defined(_GLOWFOLLOW_ON)
							glow *= lerp(totalThickness < d, (1 - _DissolveAmount), d < 0);
						#else
							glow *= (totalThickness < d);
						#endif
						color += glow;
					#endif
				#endif
				
				color.a *= (0 < mt.a);
				color.rgb *= color.a;
				
				return color;
			}
		ENDCG
		}
	}
	CustomEditor "SpriteDissolveShaderGUI"
}
