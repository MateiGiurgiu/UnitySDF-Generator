Shader "Hidden/SDFVisualiser_ImageEffect2"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			#define MAX_STEP 128
			#define EPSILON 0.001

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			uniform float4x4 _FrustumCornersMatrix;
			uniform float4x4 _CameraInvViewMatrix;

			uniform sampler3D _SDF;
			uniform float4x4 _SDFMappingMatrix;
			uniform float _SdfScale;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 ray : TEXCOORD1;
			};

			struct Ray
			{
				float3 Origin;
				float3 Direction;
			};

			v2f vert (appdata v)
			{
				v2f o;
				int index = v.vertex.z;
				o.vertex = UnityObjectToClipPos(float4(v.vertex.xy, 0, 1));
				o.uv = v.uv;
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
				#endif
				
				o.ray = _FrustumCornersMatrix[index].xyz;
				o.ray = mul(_CameraInvViewMatrix, o.ray);
				return o;
			}

			bool RayToBox(float boxSize, Ray ray, out float tHit, float tMinLimit, float tMaxLimit)
			{
				float3 invRayDir = rcp(ray.Direction);
				float3 t0 = (-boxSize - ray.Origin) * invRayDir;
				float3 t1 = (+boxSize - ray.Origin) * invRayDir;

				float3 tMinVec = min(t0, t1);
				float3 tMaxVec = max(t0, t1);

				float tMin = max(tMinLimit, max(tMinVec[0], max(tMinVec[1], tMinVec[2])));
				float tMax = min(tMaxLimit, min(tMaxVec[0], min(tMaxVec[1], tMaxVec[2])));

				tHit = tMin;
				return (tMin < tMax);
			}

			float SampleSDF(float3 wsPosition)
			{
				wsPosition = mul(_SDFMappingMatrix, float4(wsPosition, 1)).xyz;
				wsPosition = clamp(wsPosition, 0, 1);
				return tex3D(_SDF, wsPosition).r * _SdfScale;
			}
			
			int Raymarch(Ray ray) 
			{
				float t = 0; 
				for (int i = 0; i < MAX_STEP; i++)
				{
					float3 p = ray.Origin + ray.Direction * t; 
					float d = SampleSDF(p);

					if (d < EPSILON) 
					{
						return i;
					}

					t += d;
				}

				return 0;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//fixed4 col = tex2D(_MainTex, i.uv);
				Ray ray;
				ray.Origin = _WorldSpaceCameraPos.xyz;
				ray.Direction = normalize(i.ray);
			
				float t = 0;
				int steps = 0;
				fixed3 col = fixed3(0, 0, 0);
				if (RayToBox(_SdfScale / 2, ray, t, 0.001, 100))
				{
					ray.Origin += ray.Direction * t;
					steps = Raymarch(ray);

					if (steps == 0)
					{
						col = fixed3(0, 0.4, 1);
					}
					else
					{
						float c = ((float)steps / MAX_STEP);
						col = lerp(fixed3(0, 1, 0), fixed3(1, 0, 0), c);
					}
				}

				return fixed4(col, 1);
			}
			ENDCG
		}
	}
}
