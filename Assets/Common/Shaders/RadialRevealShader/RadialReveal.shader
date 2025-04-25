Shader "Universal Render Pipeline/Unlit/RadialReveal_NoHalo"
{
    Properties
    {
        [Header(General)]
        _MainTex             ("Main Texture (Alpha)",      2D)    = "white" {}
        _Progress            ("Reveal Progress",           Range(0,1))     = 0
        _EdgeWidth           ("Edge Softness",             Range(0,0.1))   = 0.01

        [Space(15)]
        [Header(Main Gradient and Emission)]
        _ColorStart          ("Start Color",               Color)          = (1,1,1,1)
        _ColorEnd            ("End Color",                 Color)          = (1,1,1,1)
        [Space(10)]
        _EmissionColor       ("Emission Color",            Color)          = (1,1,1,1)
        _EmissionStrength    ("Emission Strength",         Range(0,10))    = 1

        [Space(30)]
        [Header(Mode and Invert)]
        [Enum(Angle,0, Radial,1, Random,2)]
        _RevealMode          ("Mode",                      Int)            = 0
        [Toggle]
        _Invert              ("Invert Direction",          Float)          = 0

        [Space(30)]
        [Header(Noise Reveal)]
        _NoiseTex            ("Noise Texture",             2D)             = "white" {}
        _NoiseScale          ("Noise Scale (Tiling)",      Range(0.1,10))  = 1

        [Space(30)]
        [Header(Border Distortion)]
        _BorderNoiseTex      ("Border Noise Texture",      2D)             = "white" {}
        _BorderNoiseScale    ("Border Noise Scale",        Range(0.1,10))  = 1
        _BorderNoiseStrength ("Border Noise Strength",     Range(0,0.5))   = 0.1
        [Space(5)]
        _BorderFade          ("Border Fade Zone",          Range(0,0.5))   = 0.02

        [Space(30)]
        [Header(Flame Effects)]
        _FlameTex            ("Flame Texture",             2D)             = "white" {}
        _FlameSpeed          ("Flame Scroll Speed",        Float)          = 1
        _BorderWidth         ("Effect Border Width",       Range(0.001,0.1))= 0.005

        [Space(10)]
        [Header(Flame Color and Emission)]
        _FlameColor          ("Flame Tint Color",          Color)          = (1,1,1,1)
        _FlameEmissionColor  ("Flame Emission Color",      Color)          = (1,1,1,1)
        _FlameEmissionStrength("Flame Emission Strength",  Range(0,10))    = 1

        [Space(30)]
        [Header(Border Gradient and Emission)]
        _BorderColor         ("Border Color",              Color)          = (1,0.5,0,1)
        [Space(10)]
        [Toggle]
        _UseBorderGradient   ("Use Border Gradient Tex",   Float)          = 0
        _BorderGradientTex   ("Border Gradient Texture",   2D)             = "white" {}
        [Space(10)]
        _BorderEmissionColor ("Border Emission Color",     Color)          = (1,0.5,0,1)
        _BorderEmissionStrength("Border Emission Strength",Range(0,10))     = 1
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

            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);           SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_BorderNoiseTex);     SAMPLER(sampler_BorderNoiseTex);
            TEXTURE2D(_FlameTex);           SAMPLER(sampler_FlameTex);
            TEXTURE2D(_BorderGradientTex);  SAMPLER(sampler_BorderGradientTex);

            float   _Progress, _EdgeWidth, _BorderWidth;
            float   _NoiseScale, _BorderNoiseScale, _BorderNoiseStrength, _BorderFade;
            float   _FlameSpeed;
            int     _RevealMode;
            float   _Invert;
            float4  _ColorStart, _ColorEnd;
            float4  _EmissionColor; float _EmissionStrength;
            float4  _FlameColor, _FlameEmissionColor; float _FlameEmissionStrength;
            float4  _BorderColor, _BorderEmissionColor; float _BorderEmissionStrength;
            float   _UseBorderGradient;

            struct Attributes { float4 posOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS);
                OUT.uv    = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 1) 采样 alpha
                half4 src = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float mask = src.a;
                if (mask < 0.01) return half4(0,0,0,0);
                

                // 3) 动态边缘宽度：Progress 越接近 0 或 1，Edge 越趋近 0
                float edge = _EdgeWidth * saturate(_Progress * (1 - _Progress) * 4);

                // 4) 边界扰动包络
                float rawBn = SAMPLE_TEXTURE2D(_BorderNoiseTex, sampler_BorderNoiseTex, IN.uv * _BorderNoiseScale).r - 0.5;
                float env   = smoothstep(0,_BorderFade,_Progress) * smoothstep(0,_BorderFade,1-_Progress);
                float localProg = saturate(_Progress + rawBn * _BorderNoiseStrength * env);

                // 5) 形状因子
                float2 cUV = IN.uv - 0.5;
                float angle01 = (atan2(cUV.y,cUV.x)+PI)/(2*PI);
                float dist01  = saturate(length(cUV)/0.5);
                float noiseV  = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uv*_NoiseScale).r;

                // 6) 选择 Reveal
                float rA = smoothstep(localProg-edge, localProg+edge, angle01);
                float rR = smoothstep(localProg-edge, localProg+edge, dist01);
                float rN = smoothstep(localProg-edge, localProg+edge, noiseV);
                float reveal = (_RevealMode==1)?rR:(_RevealMode==2)?rN:rA;

                // 7) 反向
                if(_Invert>0.5) reveal=1-reveal;
                float mR = mask * reveal;

                // 8) 主色 & Emission
                float3 mainC = lerp(_ColorStart.rgb, _ColorEnd.rgb, _Progress);
                float3 outC  = mainC * mR + _EmissionColor.rgb * mR*_EmissionStrength;

                // 9) 边界环带
                float shapeV = (_RevealMode==1)?dist01:(_RevealMode==2)?noiseV:angle01;
                float lo = smoothstep(localProg - _BorderWidth, localProg, shapeV);
                float hi = smoothstep(localProg, localProg + _BorderWidth, shapeV);
                float bM = saturate(lo - hi) * saturate(_Progress*(1-_Progress)*4);

                // 10) 边界色 & 火焰
                float3 bC = _BorderColor.rgb * mR;
                outC = lerp(outC, bC, bM);
                float3 flame = SAMPLE_TEXTURE2D(_FlameTex, sampler_FlameTex, float2(shapeV,_Time.y*_FlameSpeed)).rgb * _FlameColor.rgb;
                outC = lerp(outC, flame, bM) + _FlameEmissionColor.rgb*flame*_FlameEmissionStrength*bM;

                return half4(outC, mR);
            }
            ENDHLSL
        }
    }
}
