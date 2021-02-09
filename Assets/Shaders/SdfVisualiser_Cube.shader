Shader "SDF/SdfVisualiser_Cube"
{
    Properties
    {
        _SDF("SDF", 3D) = "black" {}
    }
        SubShader
    {
        Tags { "RenderType" = "AlphaTest" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #define MAX_STEP 100
            #define EPSILON 0.002
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                UNITY_FOG_COORDS(0)
                float3 worldPos : TEXCOORD1;
                float3 localPos : TEXCOORD2;
            };

            sampler3D _SDF;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.localPos = v.vertex.xyz;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            float SampleSDF(float3 localPos)
            {
                // add an offset since Unity's default cube is centered at origin
                localPos += float3(0.5, 0.5, 0.5);
                localPos = clamp(localPos, 0, 1);
                return tex3D(_SDF, localPos).r;
            }

            int SphereMarch(float3 rayOrigin, float3 rayDirection)
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

            int SphereMarch2(float3 rayOrigin, float3 rayDirection)
            {
                float t = 0;
                int steps = 0;
                [unroll(MAX_STEP)]
                while (t < 1)
                {
                    float3 p = rayOrigin + rayDirection * t;
                    float d = SampleSDF(p);

                    if (d < EPSILON)
                    {
                        return steps;
                    }

                    t += d;
                    steps++;
                }

                return 0;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 direction = normalize(i.worldPos - _WorldSpaceCameraPos.xyz);
                int steps = SphereMarch(i.localPos, direction);

                if (steps == 0)
                {
                    discard;
                }

                float c = 1 - ((float)steps / MAX_STEP);
                fixed4 col = fixed4(c, c, c, 1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
