
Shader "Custom/ShadertoyConvertedEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _SkyboxTex ("Skybox Texture", 2D) = "white" {}
        _TimeScale ("Time Scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _SkyboxTex;
            float4 _MainTex_ST;
            float _TimeScale;

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

            float3 cmap1(float x) {
                return pow(0.5 + 0.5 * cos(3.14159 * x + float3(1, 2, 3)), float3(2.5, 2.5, 2.5));
            }

            float3 cmap2(float x) {
                float3 col = float3(0.35, 1, 1) * (cos(3.14159 * x * float3(1,1,1) + 0.75 * float3(2,1,3)) * 0.5 + 0.5);
                return col * col * col;
            }

            float3 cmap3(float x) {
                float3 yellow = float3(1, 0.9, 0);
                float3 purple = float3(0.75, 0, 1);
                float3 col = lerp(purple, yellow, cos(x / 1.25) * 0.5 + 0.5);
                return col * col * col;
            }

            float3 cmap(float x, float time) {
                float t = fmod(time, 30.0);
                return
                    (smoothstep(-1.0, 0.0, t) - smoothstep(9.0, 10.0, t)) * cmap1(x) +
                    (smoothstep(9.0, 10.0, t) - smoothstep(19.0, 20.0, t)) * cmap2(x) +
                    (smoothstep(19.0, 20.0, t) - smoothstep(29.0, 30.0, t)) * cmap3(x) +
                    (smoothstep(29.0, 30.0, t) - smoothstep(39.0, 40.0, t)) * cmap1(x);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv * 2.0 - 1.0;
                float time = _Time.y * _TimeScale;

                float focal = 2.0;
                float3 ro = float3(0, 0, 6.0 + cos(time * 0.25) * 0.75);
                float ct = cos(time * 0.5), st = sin(time * 0.5);
                float2 rotX = float2(ct, st);
                ro.xz = float2(ro.x * ct - ro.z * st, ro.x * st + ro.z * ct);

                float3 rd = normalize(float3(uv, -focal));
                rd.xz = float2(rd.x * ct - rd.z * st, rd.x * st + rd.z * ct);

                float3 color = pow(tex2D(_SkyboxTex, rd.xy * 0.5 + 0.5).rgb, float3(2.2, 2.2, 2.2));

                float t = dot(-ro, rd);
                float3 p = t * rd + ro;
                float y2 = dot(p, p);
                float x2 = 4.0 - y2;

                if (y2 <= 4.0)
                {
                    float a = t - sqrt(x2);
                    float b = t + sqrt(x2);
                    color *= exp(-(b - a));

                    float2 noiseUV = frac(i.uv + _Time.y * 0.01);
                    float tAdd = tex2D(_NoiseTex, noiseUV).a * 0.01;
                    t = a + tAdd;

                    for (int j = 0; j < 99 && t < b; j++)
                    {
                        float3 p = t * rd + ro;
                        float T = (t + time) / 5.0;
                        float cosT = cos(T), sinT = sin(T);
                        p.xy = float2(p.x * cosT - p.y * sinT, p.x * sinT + p.y * cosT);

                        for (float f = 0.0; f < 9.0; f++)
                        {
                            float a = exp(f) / exp2(f);
                            p += cos(p.yzx * a + time) / a;
                        }

                        float d = 1.0 / 100.0 + abs((ro - p - float3(0, 1, 0)).y - 1.0) / 10.0;
                        color += cmap(t, time) * 0.001 / d;
                        t += d * 0.25;
                    }

                    float3 N = normalize(a * rd + ro);
                    float cosTheta = dot(-rd, N);
                    float fresnel = 0.04 + (1.0 - 0.04) * pow(1.0 - cosTheta, 5.0);

                    color *= (1.0 - fresnel);
                    color += fresnel * pow(tex2D(_SkyboxTex, reflect(rd, N).xy * 0.5 + 0.5).rgb, float3(2.2, 2.2, 2.2));
                }

                color = 1.0 - exp(-color);
                color *= 1.0 - dot(uv * 0.55, uv * 0.55) * 0.15;
                color = pow(color, float3(1.0 / 2.2, 1.0 / 2.2, 1.0 / 2.2));

                return float4(saturate(color), 1.0);
            }
            ENDCG
        }
    }
}
