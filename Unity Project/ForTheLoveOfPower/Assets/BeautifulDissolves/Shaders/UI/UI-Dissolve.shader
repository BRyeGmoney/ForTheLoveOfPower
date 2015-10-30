Shader "Beautiful Dissolves/UI/Dissolve"
{
	Properties
	{
		_Color ("Tint", Color) = (1,1,1,1)
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_TilingX ("X", Float) = 1.0
		_TilingY ("Y", Float) = 1.0
		_SubTex ("Substitute Texture", 2D) = "black" {}
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
		[Toggle(_GLOWFOLLOW_ON)] _GlowFollow ("Follow-Through", Int) = 0
		
		[HideInInSpector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInSpector] _Stencil ("Stencil ID", Float) = 0
		[HideInInSpector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInSpector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInSpector] _StencilReadMask ("Stencil Read Mask", Float) = 255

		[HideInInSpector] _ColorMask ("Color Mask", Float) = 15
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
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _DISSOLVEMAP
			#pragma shader_feature _DIRECTIONMAP
			#pragma shader_feature _SUBMAP
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
#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
#endif
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _SubTex;
			sampler2D _DissolveMap;
			sampler2D _DirectionMap;
			fixed _DissolveAmount;
			fixed _OuterEdgeThickness;
			fixed _InnerEdgeThickness;
			fixed4 _OuterEdgeColor;
			fixed4 _InnerEdgeColor;
			half _GlowIntensity;
			fixed4 _GlowColor;
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

					#ifdef _SUBMAP
						color = lerp(noSubColor, tex2D(_SubTex, IN.texcoord), d < 0);
					#else
						noSubColor.a = lerp(mt.a, 0, d < 0);
						color = noSubColor;
					#endif
					
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
				color *= IN.color;
				
				clip (color.a - 0.01);
				
				return color;
			}
		ENDCG
		}
	}
	
	CustomEditor "SpriteDissolveShaderGUI"
}
