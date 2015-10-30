Shader "Beautiful Dissolves/Mobile/Dissolve" {
	Properties {
		_MainTex ("Albedo", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_DissolveMap ("Dissolve Map", 2D) = "white" {}
		_DissolveAmount ("Dissolve Amount", Range(0.0, 1.0)) = 0.5
		_DirectionMap ("Direction Map", 2D) = "white" {}
		_SubTex ("Substitute Texture", 2D) = "white" {}
		[Toggle(_DISSOLVEGLOW_ON)] _DissolveGlow ("Dissolve Glow", Int) = 1
		_GlowColor ("Glow Color", Color) = (1,0.5,0,1)
		_GlowIntensity ("Glow Intensity", Float) = 7
		[Toggle(_EDGEGLOW_ON)] _EdgeGlow ("Edge Glow", Int) = 1
		[Toggle(_COLORBLENDING_ON)] _ColorBlending ("Color Blending", Int) = 1
		_OuterEdgeColor ("Outer Edge Color", Color) = (1,0,0,1)
		_InnerEdgeColor ("Inner Edge Color", Color) = (1,1,0,1)
		_OuterEdgeThickness ("Outer Edge Thickness", Range(0.0, 1.0)) = 0.02
		_InnerEdgeThickness ("Inner Edge Thickness", Range(0.0, 1.0)) = 0.02
		[Toggle(_GLOWFOLLOW_ON)] _GlowFollow ("Follow-Through", Int) = 0
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha	
		LOD 150

		CGPROGRAM
		#pragma surface surf Lambert keepalpha noforwardadd
		#pragma shader_feature _EDGEGLOW_ON
		#pragma shader_feature _COLORBLENDING_ON
		#pragma shader_feature _DISSOLVEGLOW_ON
		#pragma shader_feature _NORMALMAP
		#pragma shader_feature _DISSOLVEMAP
		#pragma shader_feature _DIRECTIONMAP
		#pragma shader_feature _SUBMAP
		#pragma shader_feature _GLOWFOLLOW_ON

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
		};
		
		sampler2D _DissolveMap;
		sampler2D _DirectionMap;
		sampler2D _SubTex;
		fixed _DissolveAmount;
		fixed _OuterEdgeThickness;
		fixed _InnerEdgeThickness;
		fixed3 _OuterEdgeColor;
		fixed3 _InnerEdgeColor;
		half _GlowIntensity;
		fixed3 _GlowColor;

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 mt = tex2D(_MainTex, IN.uv_MainTex);
			fixed3 color;
			fixed alpha = mt.a;

			#ifdef _DISSOLVEMAP
				fixed3 dm = tex2D(_DissolveMap, IN.uv_MainTex).rgb;
				fixed d = dm;
				#ifdef _DIRECTIONMAP
					d *= tex2D(_DirectionMap, IN.uv_MainTex).rgb;
				#endif
				d -= _DissolveAmount;
				
				fixed totalThickness = _InnerEdgeThickness + _OuterEdgeThickness;
				fixed3 noSubColor = fixed3(0,0,0);
				#ifdef _COLORBLENDING_ON
					noSubColor = lerp(mt.rgb, lerp(_OuterEdgeColor, _InnerEdgeColor, d/(totalThickness + 0.01)), d < totalThickness);
				#else
					noSubColor = lerp(lerp(mt.rgb, _InnerEdgeColor, d < totalThickness), _OuterEdgeColor, d < _OuterEdgeThickness);
				#endif
				
				#ifdef _SUBMAP
					color = lerp(noSubColor, tex2D(_SubTex, IN.uv_MainTex).rgb, d < 0);
				#else
					color = noSubColor * (0 < d);
					alpha *= (0 < d);
				#endif

				#if defined(_DISSOLVEGLOW_ON) || defined(_EDGEGLOW_ON)
					half3 glow = half3(0,0,0);
					#ifdef _DISSOLVEGLOW_ON
						glow = pow((1 - dm), 3) * 2 * _DissolveAmount * _GlowIntensity * _GlowColor.rgb;
						
						#if defined(_SUBMAP) && defined(_GLOWFOLLOW_ON)
							glow *= lerp(totalThickness < d, (1 - _DissolveAmount), d < 0);
						#else
							glow *= totalThickness < d;
						#endif
					#endif

					#ifdef _EDGEGLOW_ON
						glow += noSubColor * (0 < d) * (d < totalThickness);
					#endif
					o.Emission = glow;
				#endif
			#else
				color = mt.rgb;
			#endif
			
			o.Albedo = color;
			o.Alpha = alpha;
			
			#ifdef _NORMALMAP
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			#endif
		}
		ENDCG
	}

	Fallback "Mobile/Transparent/VertexLit"
	CustomEditor "MobileDissolveShaderGUI"
}
