Shader "Custom/PainterlyScrollingNoiseColor_NetSmooth"
{
    Properties
    {
        _Speed ("Scroll Speed", Float) = 0.2
        _Scale ("Noise Scale", Float) = 4.0
        _Octaves ("Detail Level", Int) = 5
        _Intensity ("Intensity", Float) = 1.0
        _Color ("Color", Color) = (1,1,1,1)
        _Contrast ("Contrast", Range(0.1, 3)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _Speed;
            float _Scale;
            int _Octaves;
            float _Intensity;
            float4 _Color;
            float _Contrast;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // quintic smoothstep interpolation (plus net que cubic smoothstep)
            float2 quinticInterp(float2 f)
            {
                return f * f * f * (f * (f * 6 - 15) + 10);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                float2 u = quinticInterp(f);

                float lerpX1 = lerp(a, b, u.x);
                float lerpX2 = lerp(c, d, u.x);
                return lerp(lerpX1, lerpX2, u.y);
            }

            float fbm(float2 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < octaves; i++)
                {
                    value += noise(p * frequency) * amplitude;
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }

                return value;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv.x += _Time.y * _Speed;

                float n = fbm(uv * _Scale, _Octaves);

                // Appliquer contraste en jouant sur la puissance
                n = pow(n, _Contrast);

                return _Color * n * _Intensity;
            }

            ENDCG
        }
    }
}

