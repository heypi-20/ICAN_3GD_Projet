Shader "Unlit/ComboJaugeBoss"
{
    Properties
    {
        _MainTex          ("Main Texture (Alpha)", 2D)    = "white" {}
        _Progress         ("Reveal Progress", Range(0,1)) = 0
        _EdgeWidth        ("Edge Softness", Range(0,0.1)) = 0.01
        _ColorStart       ("Start Color", Color)           = (1,1,1,1)
        _ColorEnd         ("End Color", Color)             = (1,1,1,1)
        _EmissionColor    ("Emission Color", Color)        = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 1

        [Toggle] _Invert  ("Invert Direction", Float)      = 0

        [Header(Flame Effects)]
        _FlameTex         ("Flame Texture", 2D)            = "white" {}
        _FlameSpeed       ("Flame Scroll Speed", Float)    = 1
        _BorderWidth      ("Effect Border Width", Range(0.001,0.1)) = 0.005

        [Header(Flame Color and Emission)]
        _FlameColor           ("Flame Tint Color", Color)       = (1,1,1,1)
        _FlameEmissionColor   ("Flame Emission Color", Color)   = (1,0,1,1)
        _FlameEmissionStrength("Flame Emission Strength", Range(0,10)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);   SAMPLER(sampler_MainTex);
            TEXTURE2D(_FlameTex);  SAMPLER(sampler_FlameTex);

            float   _Progress, _EdgeWidth;
            float   _Invert;
            float4  _ColorStart, _ColorEnd;
            float4  _EmissionColor; float _EmissionStrength;
            float4  _FlameColor, _FlameEmissionColor; float _FlameEmissionStrength;
            float   _FlameSpeed, _BorderWidth;

            struct Attributes { float4 posOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS);
                OUT.uv     = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Lecture de la texture principale et masque
                half4 src = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float mask = src.a;
                if (mask < 0.01) return half4(0,0,0,0);

                // Calcul de la douceur du bord
                float edge = _EdgeWidth * saturate(_Progress * (1 - _Progress) * 4);
                float localProg = saturate(_Progress);

                // Révélation horizontale : smoothstep sur la coordonnée U
                float rH = smoothstep(localProg - edge, localProg + edge, IN.uv.x);
                float reveal = (_Invert > 0.5) ? 1 - rH : rH;
                float mR = mask * reveal;

                // Couleur principale + émission
                float3 mainC = lerp(_ColorStart.rgb, _ColorEnd.rgb, _Progress);
                float3 outC  = mainC * mR + _EmissionColor.rgb * mR * _EmissionStrength;

                // Bordure pour les flammes
                float shapeV = IN.uv.x;
                float bM = saturate(
                    smoothstep(localProg - _BorderWidth, localProg, shapeV)
                  - smoothstep(localProg, localProg + _BorderWidth, shapeV)
                );

                // Effet de flammes le long de la bordure
                float3 flame = SAMPLE_TEXTURE2D(_FlameTex, sampler_FlameTex, float2(shapeV, _Time.y * _FlameSpeed)).rgb * _FlameColor.rgb;
                outC = lerp(outC, flame, bM)
                     + _FlameEmissionColor.rgb * flame * _FlameEmissionStrength * bM;

                return half4(outC, mR);
            }
            ENDHLSL
        }
    }
}