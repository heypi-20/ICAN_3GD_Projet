Shader "Custom/SolSaMère"
{
    Properties
    {
        [Header(Base_Layer)]
        [Space(5)]
        _Tiling("Tiling", Vector) = (5, 5, 0, 0)
        _BaseColor("Base Color", Color) = (0.6, 0.1, 0.05, 1)
        _HighlightColor("Highlight Color", Color) = (0.9, 0.2, 0.1, 1)
        _NoiseScale("Noise Scale", Float) = 3.0
        _Contrast("Noise Contrast", Float) = 1.0
        _TimeSpeed("Noise Movement Speed", Float) = 0.1

        [Header(Detail_Layer Medium)]
        [Space(5)]
        _MidNoiseScale("Mid Noise Scale", Float) = 10.0
        _MidIntensity("Mid Intensity", Float) = 0.4
        _MidColor("Mid Color", Color) = (0.8, 0.4, 0.2, 1)

        [Header(Detail_Layer Rough)]
        [Space(5)]
        _DetailNoiseScale("Detail Noise Scale", Float) = 20.0
        _DetailIntensity("Detail Intensity", Float) = 0.3
        _DetailColor("Detail Color", Color) = (1.0, 0.9, 0.7, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) +
                       (c - a) * u.y * (1.0 - u.x) +
                       (d - b) * u.x * u.y;
            }

            float fbm(float2 p, int octaves)
            {
                float total = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < octaves; i++)
                {
                    total += noise(p) * amplitude;
                    p *= 2.0;
                    amplitude *= 0.5;
                }
                return total;
            }

            float fbm_rough(float2 p)
            {
                float total = 0.0;
                float amplitude = 1.0;
                float frequency = 1.0;
                for (int i = 0; i < 3; i++)
                {
                    float2 pi = floor(p * frequency);
                    float n = hash(pi);
                    n = n * n;
                    total += n * amplitude;
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                return saturate(total);
            }

            // Approximate normal from procedural height
            float3 computeNormal(float2 uv, float scale)
            {
                float e = 0.01;
                float hL = fbm(uv - float2(e, 0) * scale, 5);
                float hR = fbm(uv + float2(e, 0) * scale, 5);
                float hD = fbm(uv - float2(0, e) * scale, 5);
                float hU = fbm(uv + float2(0, e) * scale, 5);
                float3 n = normalize(float3(hL - hR, hD - hU, 2.0));
                return n;
            }

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _HighlightColor;
                float4 _Tiling;
                float _NoiseScale;
                float _Contrast;
                float _TimeSpeed;

                float _DetailNoiseScale;
                float _DetailIntensity;
                float4 _DetailColor;

                float _MidNoiseScale;
                float _MidIntensity;
                float4 _MidColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = IN.uv * _Tiling.xy;
                OUT.worldPos = worldPos;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 timeOffset = _Time.y * _TimeSpeed;
                float2 uvMain = IN.uv * _NoiseScale + timeOffset;

                float baseNoise = pow(fbm(uvMain, 5), _Contrast);
                float3 colorBase = lerp(_BaseColor.rgb, _HighlightColor.rgb, baseNoise);

                float2 uvMid = IN.uv * _MidNoiseScale + timeOffset;
                float midNoise = pow(fbm(uvMid, 4), _Contrast);
                float3 colorMid = _MidColor.rgb * midNoise;

                float2 uvDetail = IN.uv * _DetailNoiseScale + timeOffset;
                float detailNoise = fbm_rough(uvDetail);
                float3 colorDetail = _DetailColor.rgb * detailNoise;

                float3 finalColor = colorBase;
                finalColor += colorMid * _MidIntensity;
                finalColor += colorDetail * _DetailIntensity;

                // Approximate lighting with procedural normal
                float3 normal = computeNormal(uvMain, _NoiseScale);
                float3 lightDir = normalize(float3(0.5, 0.8, 1.0)); // faux soleil
                float NdotL = saturate(dot(normal, lightDir));
                finalColor *= 0.5 + 0.5 * NdotL;

                return half4(saturate(finalColor), 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
