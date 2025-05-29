Shader "Universal Render Pipeline/Unlit/BossEggShader"
{
    Properties
    {
        [Header(Always Visible Texture)]
        _AlwaysTex          ("Always Visible Texture",     2D)            = "white" {}
        _AlwaysTexBlend     ("Always Tex Blend",           Range(0,1))    = 1
        _AlwaysTexColor     ("Always Tex Tint Color",      Color)         = (1,1,1,1)
        _AlwaysTex_ST ("AlwaysTex Tiling/Offset", Vector) = (1,1,0,0)

        [Header(General)]
        _MainTex             ("Main Texture (Alpha)",      2D)    = "white" {}
        _Progress            ("Reveal Progress",           Range(0,1))     = 0
        _EdgeWidth           ("Edge Softness",             Range(0,0.1))   = 0.01
        _RevealAlpha         ("Reveal Opacity",            Range(0,1))     = 1

        [Space(15)]
        [Header(Main Gradient and Emission)]
        _ColorStart          ("Start Color",               Color)          = (1,1,1,1)
        _ColorEnd            ("End Color",                 Color)          = (1,1,1,1)
        [Space(10)]
        _EmissionColor       ("Emission Color",            Color)          = (1,1,1,1)
        _EmissionStrength    ("Emission Strength",         Range(0,10))    = 1

        [Space(30)]
        [Header(Mode and Invert)]
        // Angle=0, Radial=1, Random=2, Vertical=3, Horizontal=4
        [Enum(Angle,0, Radial,1, Random,2, Vertical,3, Horizontal,4)]
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
        [Header(Border Color and Emission)]
        _BorderColor         ("Border Color",              Color)          = (1,0.5,0,1)
        [Space(10)]
        [Toggle]
        _UseBorderGradient   ("Use Border Gradient Tex",   Float)          = 0
        _BorderGradientTex   ("Border Gradient Texture",   2D)             = "white" {}
        [Space(10)]
        _BorderEmissionColor ("Border Emission Color",     Color)          = (1,0.5,0,1)
        _BorderEmissionStrength("Border Emission Strength",Range(0,10))     = 1

        [Space(30)]
        [Header(Halo or Lens Dirt)]
        [Toggle]
        _UseHaloNoise        ("Use Halo Noise Texture",    Float)          = 0
        _HaloTex             ("Halo Noise Texture",        2D)             = "white" {}
        _HaloNoiseScale      ("Halo Noise Scale (Tiling)", Range(0.1,10))  = 1
        _HaloStrength        ("Halo Strength",             Range(0,10))    = 1
        [Space(5)]
        _HaloColor           ("Halo Tint Color",           Color)          = (1,1,1,1)
        _HaloEmissionColor   ("Halo Emission Color",       Color)          = (1,1,1,1)
        _HaloEmissionStrength("Halo Emission Strength",    Range(0,10))    = 1
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

            // Textures & samplers
            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);           SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_BorderNoiseTex);     SAMPLER(sampler_BorderNoiseTex);
            TEXTURE2D(_FlameTex);           SAMPLER(sampler_FlameTex);
            TEXTURE2D(_BorderGradientTex);  SAMPLER(sampler_BorderGradientTex);
            TEXTURE2D(_HaloTex);            SAMPLER(sampler_HaloTex);
            TEXTURE2D(_AlwaysTex);        SAMPLER(sampler_AlwaysTex);

            // Parameters
            float   _Progress, _EdgeWidth, _RevealAlpha, _BorderWidth;
            float   _NoiseScale, _BorderNoiseScale, _BorderNoiseStrength, _BorderFade;
            float   _FlameSpeed;
            int     _RevealMode;
            float   _Invert;
            float4  _ColorStart, _ColorEnd;
            float4  _EmissionColor;     float _EmissionStrength;
            float4  _FlameColor, _FlameEmissionColor;   float _FlameEmissionStrength;
            float4  _BorderColor, _BorderEmissionColor; float _BorderEmissionStrength;
            float   _UseBorderGradient;

            float   _UseHaloNoise, _HaloNoiseScale, _HaloStrength;
            float4  _HaloColor, _HaloEmissionColor;     float _HaloEmissionStrength;

            struct Attributes { float4 posOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            float _AlwaysTexBlend;
            float4 _AlwaysTexColor;
            float4 _AlwaysTex_ST;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS);
                OUT.uv    = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 1) Alpha mask
                half4 src = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float mask = src.a;
                if (mask < 0.01) return half4(0,0,0,0);

                // 2) Dynamic edge softness
                float edge = _EdgeWidth * saturate(_Progress * (1 - _Progress) * 4);

                // 3) Border distortion envelope
                float rawBn = SAMPLE_TEXTURE2D(_BorderNoiseTex, sampler_BorderNoiseTex, IN.uv * _BorderNoiseScale).r - 0.5;
                float env   = smoothstep(0, _BorderFade, _Progress) * smoothstep(0, _BorderFade, 1 - _Progress);
                float localProg = saturate(_Progress + rawBn * _BorderNoiseStrength * env);

                // 4) Shape factors
                float2 dUV     = IN.uv - 0.5;
                float angle01  = frac((atan2(dUV.y, dUV.x) + PI) / (2.0 * PI));
                float dist01   = saturate(length(dUV) / 0.5);
                float noiseV   = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uv * _NoiseScale).r;

                // 5) Compute reveals
                float rA = smoothstep(localProg - edge, localProg + edge, angle01);
                float rR = smoothstep(localProg - edge, localProg + edge, dist01);
                float rN = step(noiseV, _Progress);
                float rV = smoothstep(localProg - edge, localProg + edge, IN.uv.y);
                float rH = smoothstep(localProg - edge, localProg + edge, IN.uv.x);

                // 6) Mode select
                float reveal =
                    (_RevealMode==1)? rR :
                    (_RevealMode==2)? rN :
                    (_RevealMode==3)? rV :
                    (_RevealMode==4)? rH :
                                      rA;

                // 7) Invert
                if (_Invert>0.5) reveal = 1 - reveal;

                // —— 分离两个蒙版 —— 
                float mRColor = mask * reveal * _RevealAlpha;
                float mREmit  = mask * reveal;

                // 8) Base Color
                float3 mainCol = lerp(_ColorStart.rgb, _ColorEnd.rgb, _Progress);
                float3 colRGB  = mainCol * mRColor;

                // 9) Emission (for Bloom) unaffected by _RevealAlpha
                colRGB += _EmissionColor.rgb * mREmit * _EmissionStrength;

                // 10) Boundary ring mask
                float shapeVal =
                    (_RevealMode==1)? dist01 :
                    (_RevealMode==2)? noiseV :
                    (_RevealMode==3)? IN.uv.y :
                    (_RevealMode==4)? IN.uv.x :
                                      angle01;
                float lo = smoothstep(localProg - _BorderWidth, localProg, shapeVal);
                float hi = smoothstep(localProg,   localProg + _BorderWidth, shapeVal);
                float bM = saturate(lo - hi) * saturate(_Progress * (1 - _Progress) * 4);

                // 11) Border color & emission
                float3 bCol = (_UseBorderGradient>0.5)
                    ? SAMPLE_TEXTURE2D(_BorderGradientTex, sampler_BorderGradientTex, float2(reveal,0.5)).rgb
                    : _BorderColor.rgb * mRColor;
                colRGB = lerp(colRGB, bCol, bM)
                       + _BorderEmissionColor.rgb * bM * _BorderEmissionStrength;

                // 12) Flame overlay & emission
                float2 fuv    = float2(shapeVal, _Time.y * _FlameSpeed);
                float3 flame  = SAMPLE_TEXTURE2D(_FlameTex, sampler_FlameTex, fuv).rgb * _FlameColor.rgb;
                colRGB = lerp(colRGB, flame, bM)
                       + _FlameEmissionColor.rgb * flame * _FlameEmissionStrength * bM;

                // 13) Halo / Lens Dirt
                if (mRColor > 0.001)
                {
                    float3 haloSample = _UseHaloNoise > 0.5
                        ? SAMPLE_TEXTURE2D(_HaloTex, sampler_HaloTex, IN.uv * _HaloNoiseScale).rgb
                        : float3(1,1,1);
                    float hVal = mRColor * _HaloStrength;
                    colRGB += haloSample * _HaloColor.rgb * hVal;
                    colRGB += haloSample * _HaloEmissionColor.rgb * hVal * _HaloEmissionStrength;
                }

                float2 alwaysUV = IN.uv * _AlwaysTex_ST.xy + _AlwaysTex_ST.zw;
                float3 alwaysTex = SAMPLE_TEXTURE2D(_AlwaysTex, sampler_AlwaysTex, alwaysUV).rgb * _AlwaysTexColor.rgb;
                colRGB = lerp(colRGB, alwaysTex, _AlwaysTexBlend);
                return half4(colRGB, mRColor);
            }
            ENDHLSL
        }
    }
}
