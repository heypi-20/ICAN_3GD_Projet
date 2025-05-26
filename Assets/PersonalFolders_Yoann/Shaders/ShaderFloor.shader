Shader "Custom/ProceduralRedSandLit"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.6, 0.1, 0.05, 1)
        _HighlightColor("Highlight Color", Color) = (0.9, 0.2, 0.1, 1)
        _Tiling("Tiling", Vector) = (5, 5, 0, 0)
        _NoiseScale("Noise Scale", Float) = 3.0

        _MidNoiseScale("Mid Noise Scale", Float) = 10.0
        _MidIntensity("Mid Intensity", Float) = 0.4
        _MidColor("Mid Color", Color) = (0.8, 0.4, 0.2, 1)

        _DetailNoiseScale("Detail Noise Scale", Float) = 20.0
        _DetailIntensity("Detail Intensity", Float) = 0.3
        _DetailColor("Detail Color", Color) = (1.0, 0.9, 0.7, 1)

        _Metallic("Metallic", Range(0,1)) = 0.0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor, _HighlightColor;
                float4 _MidColor, _DetailColor;
                float4 _Tiling;
                float _NoiseScale;
                float _MidNoiseScale, _MidIntensity;
                float _DetailNoiseScale, _DetailIntensity;
                float _Metallic, _Smoothness;
            CBUFFER_END

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float fbm(float2 p, int octaves)
            {
                float total = 0.0, amplitude = 0.5;
                for (int i = 0; i < octaves; ++i)
                {
                    total += noise(p) * amplitude;
                    p *= 2.0;
                    amplitude *= 0.5;
                }
                return total;
            }

            float fbm_rough(float2 p)
            {
                float total = 0.0, amplitude = 1.0, frequency = 1.0;
                for (int i = 0; i < 3; ++i)
                {
                    float2 pi = floor(p * frequency);
                    float n = hash(pi);
                    total += (n * n) * amplitude;
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                return saturate(total);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv * _Tiling.xy;
                OUT.fogCoord = ComputeFogFactor(OUT.positionHCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float baseNoise = fbm(uv * _NoiseScale, 5);
                float3 baseColor = lerp(_BaseColor.rgb, _HighlightColor.rgb, baseNoise);

                float midNoise = fbm(uv * _MidNoiseScale, 4);
                float3 midColor = _MidColor.rgb * midNoise * _MidIntensity;

                float detailNoise = fbm_rough(uv * _DetailNoiseScale);
                float3 detailColor = _DetailColor.rgb * detailNoise * _DetailIntensity;

                float3 finalColor = saturate(baseColor + midColor + detailColor);

                // Lighting
                Light mainLight = GetMainLight();
                float NdotL = max(0, dot(normalize(IN.normalWS), mainLight.direction));
                float3 litColor = finalColor * mainLight.color * NdotL;

                // Apply fog
                litColor = MixFog(litColor, IN.fogCoord);

                return half4(litColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}