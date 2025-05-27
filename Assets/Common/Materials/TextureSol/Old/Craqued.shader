Shader "Custom/Craqued"
{
    Properties
    {
        [Header(Main Settings)]
        _Tiling("Tiling", Vector) = (5, 5, 0, 0)
        _BaseColor("Base Color", Color) = (0.4, 0.2, 0.1, 1)
        _HighlightColor("Highlight Color", Color) = (1.0, 0.3, 0.0, 1)
        _Contrast("Noise Contrast", Float) = 2.0

        [Header(Textures)]
        _NoiseTex("Rock Texture", 2D) = "white" {}
        _CrackTex("Crack Texture", 2D) = "black" {}

        [Header(Cracks)]
        _CrackColor("Crack Color", Color) = (1.0, 0.2, 0.1, 1)
        _CrackIntensity("Crack Intensity", Float) = 1.0
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
            };

            sampler2D _NoiseTex;
            sampler2D _CrackTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _HighlightColor;
                float4 _Tiling;
                float _Contrast;

                float4 _CrackColor;
                float _CrackIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv * _Tiling.xy;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Texture rocheuse
                float noiseValue = tex2D(_NoiseTex, uv).r;
                noiseValue = pow(noiseValue, _Contrast);
                float3 base = lerp(_BaseColor.rgb, _HighlightColor.rgb, noiseValue);

                // Texture de craquelures
                float crackValue = tex2D(_CrackTex, uv).r;
                crackValue = pow(crackValue, 3.0); // accentuation
                float3 cracks = _CrackColor.rgb * crackValue * _CrackIntensity;

                float3 finalColor = base + cracks;
                return half4(saturate(finalColor), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
