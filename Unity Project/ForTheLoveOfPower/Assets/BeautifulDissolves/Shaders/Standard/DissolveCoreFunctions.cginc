#ifndef DISSOLVE_CORE_FUNCTIONS_INCLUDED
#define DISSOLVE_CORE_FUNCTIONS_INCLUDED

half		_GlowIntensity;
half4		_GlowColor;
sampler2D	_DissolveMap;
sampler2D	_DirectionMap;
half		_DissolveAmount;

sampler2D	_SubTex;

half3		_OuterEdgeColor;
half3		_InnerEdgeColor;
half		_OuterEdgeThickness;
half		_InnerEdgeThickness;

//-------------------------------------------------------------------------------------
// Core dissolve functions

half GetDissolveLevel(float2 uv)
{
	half d = tex2D(_DissolveMap, uv).rgb;
	#ifdef _DIRECTIONMAP
		d *= tex2D(_DirectionMap, uv).rgb;
	#endif
	d -= _DissolveAmount;
	
	return d;
}

half3 Dissolve(half3 diffColor, float2 uv, half oneMinusReflectivity)
{
#ifdef _DISSOLVEMAP
	half d = GetDissolveLevel(uv);
	half totalThickness = _InnerEdgeThickness + _OuterEdgeThickness;
	half3 noSubColor = half3(0,0,0);
	half3 color = half3(0,0,0);

	#ifdef _COLORBLENDING_ON
		noSubColor = lerp(diffColor, lerp(_OuterEdgeColor, _InnerEdgeColor, d/(totalThickness + 0.01)), d < totalThickness);
	#else
		noSubColor = lerp(lerp(diffColor, _InnerEdgeColor, d < totalThickness), _OuterEdgeColor, d < _OuterEdgeThickness);
	#endif

	#ifdef _SUBMAP
		color = lerp(noSubColor, tex2D(_SubTex, uv).rgb * oneMinusReflectivity, d < 0);
	#else
		clip(d);
		color = noSubColor;
	#endif
	return color;
#else
	return diffColor;
#endif
}

half3 DissolveEmission(half3 emis, float2 uv, half3 diffColor)
{
#ifdef _DISSOLVEMAP
	half3 dm = tex2D(_DissolveMap, uv).rgb;
	half d = dm;
	#ifdef _DIRECTIONMAP
		d *= tex2D(_DirectionMap, uv).rgb;
	#endif
	d -= _DissolveAmount;
	half totalThickness = _InnerEdgeThickness + _OuterEdgeThickness;
	#ifdef _DISSOLVEGLOW_ON
		half3 glow = pow(1 - dm, lerp(3, 1, _DissolveAmount)) * _DissolveAmount * _GlowIntensity * _GlowColor.rgb;
		#if defined(_SUBMAP) && defined(_GLOWFOLLOW_ON)
			glow *= lerp(totalThickness < d, (1 - _DissolveAmount), d < 0);
		#else
			#ifndef _SUBMAP
				emis *= (0 < d);
			#endif
			glow *= (totalThickness < d);
		#endif
		emis += glow;
	#endif

	#ifdef _EDGEGLOW_ON
		emis += diffColor * (0 < d) * (d < totalThickness);
	#endif
#endif
	return emis;
}
			
#endif // DISSOLVE_CORE_FUNCTIONS_INCLUDED
