// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/BWDiffuse" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_bwBlend ("Black & White blend", Range (0, 1)) = 0
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
 
			#include "UnityCG.cginc"
 
			uniform sampler2D _MainTex;
			uniform float _bwBlend;

			sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;

			float4 _CameraDepthNormalsTexture_TexelSize;

			struct v2f {
				float4 pos: SV_POSITION;
				float4 scrPos : TEXCOORD1;
			};

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.scrPos = ComputeScreenPos(o.pos);

				//o.scrPos.y = 1 - o.scrPos.y;
				return o;
			}

			float4 frag(v2f i) : COLOR 
			{
				float3 normalValues;
				float3 normalValues_e;
				float depthValue;
				float depthValue_e;

				float4 px = tex2D(_CameraDepthNormalsTexture, i.scrPos.xy);
				float4 px_e = tex2D(_CameraDepthNormalsTexture, i.scrPos.xy + _CameraDepthNormalsTexture_TexelSize.x);

				DecodeDepthNormal(px, depthValue, normalValues);
				DecodeDepthNormal(px_e, depthValue_e, normalValues_e);


				float depthDelta = abs(depthValue_e - depthValue);
				float normalDelta = length(normalValues_e - normalValues);

				float delta = step(0.01, normalDelta) + step(0.001, depthDelta);

				float4 edgeColor = float4(delta, delta, delta , 1);

				//float4 normalColor = float4(ddx(normalValues) + ddy(normalValues) / (ddx(depthValue) + ddy(depthValue)), 1);

				//return float4(depthDelta, depthDelta, depthDelta, 1);
				return edgeColor;
			}
			ENDCG
		}
	}
}