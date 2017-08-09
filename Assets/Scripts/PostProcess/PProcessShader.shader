// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/BWDiffuse" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_bwBlend ("Black & White blend", Range (0, 1)) = 0
		_angleThreshold("Edge Threshold Angle", Float) = 80
		_depthWeight("Weight of depth difference", Float) = 300
		_kernelRadius("Radius for pixel lookup", Int) = 1
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define PI 3.14159265358979323846264338327
			#include "UnityCG.cginc"
 
			uniform sampler2D _MainTex;
			uniform float _bwBlend;
			uniform float _angleThreshold, _depthWeight;
			uniform int _kernelRadius;

			sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;

			float4 _CameraDepthNormalsTexture_TexelSize;

			void GetMaxDeltas(int kernelRadius, float2 center, out float normalDelta, out float depthDelta)
			{
				float4 px_center = tex2D(_CameraDepthNormalsTexture, center);
				float3 normalValue;
				float depthValue;

				float2 stepSize = _CameraDepthNormalsTexture_TexelSize;
				DecodeDepthNormal(px_center, depthValue, normalValue);

				normalDelta = depthDelta = 0;				
				for(int i = -kernelRadius; i <= kernelRadius; i++)
					for(int j = -kernelRadius; j <= kernelRadius; j++)
					{
						float4 px_current = tex2D(_CameraDepthNormalsTexture, center + float2(i * stepSize.x, j * stepSize.y));
						float3 normal_current;
						float depth_current;

						DecodeDepthNormal(px_current, depth_current, normal_current);

						float angle = abs(acos(dot(normalValue, normal_current) / (length(normalValue) * length(normal_current)))) * 180 / PI;
						normalDelta = max(normalDelta, angle);
						depthDelta = max(depthDelta, abs(depth_current - depthValue));						
					}
			}

			struct v2f {
				float4 pos: SV_POSITION;
				float4 scrPos : TEXCOORD1;
			};

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.scrPos = ComputeScreenPos(o.pos);

				return o;
			}

			float4 frag(v2f i) : COLOR 
			{
				float3 normalValues;
				float3 normalValues_e;
				float3 normalValues_n;
				float depthValue;
				float depthValue_e;
				float depthValue_n;

				float depthDelta, normalDelta;
				GetMaxDeltas(_kernelRadius, i.scrPos.xy, normalDelta, depthDelta);
				float delta = step(_angleThreshold, normalDelta) + depthDelta * _depthWeight;

				float4 edgeColor = float4(delta, delta, delta , 1);
				return edgeColor;
			}

			ENDCG
		}
	}
}