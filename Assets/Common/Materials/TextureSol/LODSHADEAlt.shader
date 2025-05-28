Shader "Custom/LODSHADEAlt"
{
    Properties
    {
        [Header(Main Settings)]
        _BaseColor("Base Color", Color) = (0.4, 0.2, 0.1, 1)
        _HighlightColor("Highlight Color", Color) = (1.0, 0.3, 0.0, 1)
        _Contrast("Noise Contrast", Float) = 2.0

        [Header(Textures)]
        _NoiseTex("Rock Texture", 2D) = "white" {}
        _CrackTex("Crack Texture", 2D) = "black" {}

        [Header(Cracks)]
        _CrackColor("Crack Color", Color) = (1.0, 0.2, 0.1, 1)
        _CrackIntensity("Crack Intensity", Float) = 1.0

        [Header(Distance Fade)]
        _FadeStart("Fade Start Distance", Float) = 20.0
        _FadeEnd("Fade End Distance", Float) = 50.0
        _FadeColor("Fade Color", Color) = (0.1, 0.05, 0.03, 1)
        _FadeStrength("Fade Strength (0-1)", Range(0, 1)) = 0.6
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

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            sampler2D _CrackTex;
            float4 _CrackTex_ST;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _HighlightColor;
                float _Contrast;

                float4 _CrackColor;
                float _CrackIntensity;

                float _FadeStart;
                float _FadeEnd;
                float4 _FadeColor;
                float _FadeStrength;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldPos = worldPos;
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Rock texture
                float2 noiseUV = TRANSFORM_TEX(IN.uv, _NoiseTex);
                float noiseValue = tex2D(_NoiseTex, noiseUV).r;
                noiseValue = pow(noiseValue, _Contrast);
                float3 base = lerp(_BaseColor.rgb, _HighlightColor.rgb, noiseValue);

                // Generate random rotation angle based on world position (XZ plane)
                float2 tileID = floor(IN.worldPos.xz); // one rotation per world unit
                float rnd = frac(sin(dot(tileID, float2(12.9898, 78.233))) * 43758.5453);
                float angle = rnd * 6.2831853; // 0 to 2*PI
                float2x2 rotMatrix = float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));

                // Center UVs before rotation
                float2 crackUV = IN.uv - 0.5;
                crackUV = mul(rotMatrix, crackUV);
                crackUV += 0.5;
                crackUV = TRANSFORM_TEX(crackUV, _CrackTex);

                // Crack texture
                float crackValue = tex2D(_CrackTex, crackUV).r;
                crackValue = pow(crackValue, 3.0);
                float3 cracks = _CrackColor.rgb * crackValue * _CrackIntensity;

                float3 finalColor = base + cracks;

                // Distance-based fade
                float dist = distance(_WorldSpaceCameraPos.xyz, IN.worldPos);
                float rawFade = smoothstep(_FadeStart, _FadeEnd, dist);
                float fadeFactor = 1.0 - rawFade * _FadeStrength;
                finalColor = lerp(_FadeColor.rgb, finalColor, fadeFactor);

                return half4(saturate(finalColor), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}