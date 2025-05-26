Shader "Custom/TestLaser"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TimeScale ("Time Scale", Float) = 4.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One        // Additive blending (pas d'alpha classique)
        ZWrite Off           // Pas d’écriture dans le Z-buffer
        Lighting Off         // Pas d’éclairage
        Cull Off             // Affiche des deux côtés (utile sur plans)
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _TimeScale;
            float4 _MainTex_ST;

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

            float iTime;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float Strand(float2 uv, float hoffset, float hscale, float vscale, float timescale, float2 resolution)
            {
                float glow = 0.06 * resolution.y;
                float twopi = 6.28318530718;
                float wave = sin(fmod(uv.x * hscale / 100.0 / resolution.x * 1000.0 + iTime * timescale + hoffset, twopi));
                float curve = 1.0 - abs(uv.y - (wave * resolution.y * 0.25 * vscale + resolution.y / 2.0));
                float i = saturate(curve);
                i += saturate((glow + curve) / glow) * 0.4;
                return i;
            }

            float3 Muzzle(float2 uv, float2 resolution)
            {
                float theta = atan2(resolution.y / 2.0 - uv.y, resolution.x - uv.x + 0.13 * resolution.x);
                float len = resolution.y * (10.0 + sin(theta * 20.0 + floor(iTime * 20.0) * -35.0)) / 11.0;
                float dist = length(float2(abs(resolution.x - uv.x), abs(resolution.y / 2.0 - ((uv.y - resolution.y / 2.0) * 4.0 + resolution.y / 2.0))));
                float d = max(-0.6, 1.0 - dist / len);
                return float3(
                    d * (1.0 + sin(theta * 10.0 + floor(iTime * 20.0) * 10.77) * 0.5),
                    d * (1.0 - cos(theta * 8.0 - floor(iTime * 20.0) * 8.77) * 0.5),
                    d * (1.0 - sin(theta * 6.0 - floor(iTime * 20.0) * 134.77) * 0.5)
                );
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 resolution = _ScreenParams.xy;
                iTime = _Time.y;

                float3 c = float3(0, 0, 0);

                float3 gold = float3(1.0, 0.84, 0.3);
                float3 white = float3(1.0, 1.0, 1.0);

                c += Strand(i.uv * resolution, 0.7934 + 1.0 + sin(iTime) * 30.0, 1.0,  0.16, 10.0 * _TimeScale, resolution) * gold;
                c += Strand(i.uv * resolution, 0.645 + 1.0 + sin(iTime) * 30.0, 1.5,  0.2,  10.3 * _TimeScale, resolution) * gold;
                c += Strand(i.uv * resolution, 0.735 + 1.0 + sin(iTime) * 30.0, 1.3,  0.19, 8.0  * _TimeScale, resolution) * gold;
                c += Strand(i.uv * resolution, 0.9245 + 1.0 + sin(iTime) * 30.0, 1.6,  0.14, 12.0 * _TimeScale, resolution) * white;
                c += Strand(i.uv * resolution, 0.7234 + 1.0 + sin(iTime) * 30.0, 1.9,  0.23, 14.0 * _TimeScale, resolution) * white;
                c += Strand(i.uv * resolution, 0.84525 + 1.0 + sin(iTime) * 30.0, 1.2,  0.18, 9.0 * _TimeScale, resolution) * white;

                c += saturate(Muzzle(i.uv * resolution, resolution));

                // Seulement le laser : si la somme des couleurs est très faible, alpha = 0
                float brightness = dot(c, float3(0.2126, 0.7152, 0.0722));
                float alpha = saturate((brightness - 0.02) * 10.0); // seuil dynamique
                return float4(c, alpha);
            }
            ENDCG
        }
    }
}
