Shader "OutputScreen/Parallax"
{
	Properties
	{
		_MainTex        ("Texture",            2D) = "black" {}
		_DeepestDepth   ("Deepest Depth",   Float) = 1.0
		_StepLength     ("Step Length",     Float) = 0.01
		_UScale         ("U Scale",         Float) = 1.0
		_VScale         ("V Scale",         Float) = 1.0
		_ColorIntensity ("Color Intensity", Float) = 1.0
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
				float4 vertex  : POSITION;
				float3 normal  : NORMAL;
				float4 tangent : TANGENT;
				float2 uv      : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex  : SV_POSITION;
				float2 uv      : TEXCOORD0;
				float3 viewDir : TEXCOORD1;
			};

			sampler2D _MainTex;
			float     _DeepestDepth;
			float     _StepLength;
			float     _UScale;
			float     _VScale;
			float     _ColorIntensity;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex  = UnityObjectToClipPos(v.vertex);
				o.uv      = v.uv;
				TANGENT_SPACE_ROTATION;
				o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex));
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 step      = _StepLength * normalize(i.viewDir) * float3(-_UScale, -_VScale, 1.0);
				float2 uvOrigin  = i.uv;
				float2 uv        = uvOrigin;
				float3 position  = float3(0.0, 0.0, 0.0);
				float  depth     = tex2D(_MainTex, uvOrigin).a * _DeepestDepth;
				float  prevDepth = 0.0;
				bool   valid     = step.z > 0;

				while (valid && position.z <= _DeepestDepth && depth > position.z) {
					position += step;
					prevDepth = depth;

					uv = uvOrigin + position.xy;
					depth = tex2Dlod(_MainTex, float4(saturate(uv), 0.0, 0.0)).a * _DeepestDepth;

					valid = uv.x >= 0.0 && uv.x <= 1.0 && uv.y >= 0.0 && uv.y <= 1.0;
				}

				float3 prevPosition = position - step;
				float      delta    =     position.z -     depth;
				float  prevDelta    = prevPosition.z - prevDepth;
				float3 intersection = lerp(position, prevPosition, abs(delta - prevDelta) > 1e-8 ? delta / (delta - prevDelta) : 1.0);

				uv = uvOrigin + intersection.xy;
				return valid && uv.x >= 0.0 && uv.x <= 1.0 && uv.y >= 0.0 && uv.y <= 1.0 ? fixed4(tex2D(_MainTex, uv).rgb * _ColorIntensity, 1.0) : fixed4(0.0, 0.0, 0.0, 1.0);
			}
			ENDCG
		}
	}
}
