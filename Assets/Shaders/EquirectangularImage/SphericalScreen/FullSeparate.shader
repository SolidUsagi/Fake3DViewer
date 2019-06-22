﻿Shader "EquirectangularImage/SphericalScreen/FullSeparate"
{
	Properties
	{
		_MainTex     ("Texture",         2D) = "black" {}
		_DepthMap    ("Depth Map",       2D) = "black" {}
		_DepthTop    ("Depth Top",    Float) = 0.0
		_DepthBottom ("Depth Bottom", Float) = 1.0
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv     : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _DepthMap;
			float     _DepthTop;
			float     _DepthBottom;

			v2f vert (appdata v)
			{
				v.uv.x = 1.0 - v.uv.x;

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv     = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.a = lerp(_DepthTop, _DepthBottom, tex2D(_DepthMap, i.uv).r);
				return col;
			}
			ENDCG
		}
	}
}
