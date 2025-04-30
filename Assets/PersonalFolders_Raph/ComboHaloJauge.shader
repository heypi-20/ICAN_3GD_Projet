Shader "Unlit/ComboTextGlow"
{
    Properties
    {
        _MainTex       ("Font Texture", 2D) = "white" {}
        _GlowColor     ("Glow Color", Color) = (1,1,1,1)
        _GlowStrength  ("Glow Strength", Float) = 1.5
        _PulseSpeed    ("Pulse Speed", Float) = 2
        _PulseAmount   ("Pulse Intensity", Range(0, 2)) = 0.5
        _Softness      ("Edge Softness", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _GlowColor;
            float _GlowStrength;
            float _PulseSpeed;
            float _PulseAmount;
            float _Softness;

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).a;
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                // Soft glow based on edge falloff
                float glow = smoothstep(0.0, _Softness, alpha);
                float finalGlow = glow * _GlowStrength * pulse;

                return half4(_GlowColor.rgb * finalGlow, finalGlow);
            }
            ENDHLSL
        }
    }
}