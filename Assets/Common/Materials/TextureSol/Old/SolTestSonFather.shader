Shader "Custom/SolTestSonFather"
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
        _MacroTex("Macro Texture", 2D) = "white" {}

        [Header(Cracks)]
        _CrackColor("Crack Color", Color) = (1.0, 0.2, 0.1, 1)
        _CrackIntensity("Crack Intensity", Float) = 1.0

        [Header(Blending)]
        _MacroTiling("Macro Tiling", Float) = 0.2
        _BlendStrength("Blend Strength", Range(0,1)) = 0.5
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

            sampler2D _MacroTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _HighlightColor;
                float _Contrast;

                float4 _CrackColor;
                float _CrackIntensity;

                float _MacroTiling;
                float _BlendStrength;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float2 noiseUV = TRANSFORM_TEX(uv, _NoiseTex);
                float2 crackUV = TRANSFORM_TEX(uv, _CrackTex);

                // Macro texture blending
                float2 macroUV = IN.worldPos.xz * _MacroTiling;
                float macroValue = tex2D(_MacroTex, macroUV).r;

                // Rock texture with contrast
                float noiseValue = tex2D(_NoiseTex, noiseUV).r;
                noiseValue = pow(noiseValue, _Contrast);
                float blend = lerp(noiseValue, macroValue, _BlendStrength);
                float3 base = lerp(_BaseColor.rgb, _HighlightColor.rgb, blend);

                // Cracks
                float crackValue = tex2D(_CrackTex, crackUV).r;
                crackValue = pow(crackValue, 3.0);
                float3 cracks = _CrackColor.rgb * crackValue * _CrackIntensity;

                float3 finalColor = base + cracks;
                return half4(saturate(finalColor), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
