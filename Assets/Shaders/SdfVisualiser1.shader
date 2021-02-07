Shader "SDF/SdfVisualiser1"
{
    Properties
    {
        _SDF ("SDF", 3D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="AlphaTest" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #define MAX_STEP 128
            #define EPSILON 0.001
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float3 worldPos : TEXCOORD2;
            };

            sampler3D _SDF;
            float4 _MainTex_ST;

            float SampleSDF(float3 wsPos)
            {
                // convert the world space position to a [0, 1] range
                float3 coords = mul(unity_WorldToObject, float4(wsPos, 1)).xyz;
                coords += float3(0.5, 0.5, 0.5);
                return tex3D(_SDF, coords).r;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            int Raymarch(float3 rayOrigin, float3 rayDirection)
            {
                float t = 0;
                for (int i = 0; i < MAX_STEP; i++)
                {
                    float3 p = rayOrigin + rayDirection * t;
                    float d = SampleSDF(p);

                    if (d < EPSILON)
                    {
                        return i;
                    }

                    t += d;
                }

                return 0;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 direction = normalize(i.worldPos - _WorldSpaceCameraPos.xyz);
                int steps = Raymarch(i.worldPos, direction);

                if (steps == 0)
                {
                    discard;
                }

                float c = 1-  ((float)steps / MAX_STEP);
                fixed4 col = fixed4(c, c, c, 1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
