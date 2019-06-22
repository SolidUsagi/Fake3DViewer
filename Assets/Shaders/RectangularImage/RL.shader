Shader "RectangularImage/RL"
{
	Properties
	{
		_MainTex     ("Texture",         2D) = "black" {}
		_DepthTop    ("Depth Top",    Float) = 0.0
		_DepthBottom ("Depth Bottom", Float) = 1.0
		_U           ("U",            Float) = 0.5
		_V           ("V",            Float) = 0.5
		_UScale      ("U Scale",      Float) = 1.0
		_VScale      ("V Scale",      Float) = 1.0
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag

			#include "UnityCustomRenderTexture.cginc"

			sampler2D _MainTex;
			float     _DepthTop;
			float     _DepthBottom;
			float     _U;
			float     _V;
			float     _UScale;
			float     _VScale;

			half4 frag (v2f_customrendertexture i) : SV_Target
			{
				float2 uv = (i.globalTexcoord - float2(0.5, 0.5)) * float2(_UScale, _VScale) + float2(_U, _V);
				half4 col = tex2D(_MainTex, uv * float2(0.5, 1.0) + float2(0.5, 0.0));
				col.a = lerp(_DepthTop, _DepthBottom, tex2D(_MainTex, uv * float2(0.5, 1.0)).r);
				return uv.x >= 0.0 && uv.x <= 1.0 && uv.y >= 0.0 && uv.y <= 1.0 ? col : half4(0.0, 0.0, 0.0, 1.0);
			}
			ENDCG
		}
	}
}
