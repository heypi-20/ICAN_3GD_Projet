Shader "Custom/PortalSoftMaskNoise"
{
    Properties
    {
        _Frequency    ("Noise Frequency",    Float) = 1.4
        _Distortion   ("Noise Distortion",   Float) = 0.01
        _Speed        ("Animation Speed",    Float) = 1.0
        _Color        ("Portal Tint Color", Color) = (1,0.7,0.2,1)
        _MaskRadius   ("Portal Radius",      Float) = 1.0
        _EdgeFeather  ("Edge Fade Width",    Float) = 0.2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Frequency;
            float _Distortion;
            float _Speed;
            float4 _Color;
            float _MaskRadius;
            float _EdgeFeather;
            // _ScreenParams, _Time from UnityCG.cginc

            struct app2vert { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct vert2frag { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            vert2frag vert(app2vert v)
            {
                vert2frag o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            // --- 3D Simplex & FBM ---
            float4 permute_3d(float4 x) { return fmod(((x*34.0)+1.0)*x, 289.0); }
            float4 taylorInvSqrt3d(float4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
            float simplexNoise3d(float3 v)
            {
                const float2 C = float2(1.0/6.0, 1.0/3.0);
                const float4 D = float4(0, 0.5, 1, 2);
                float3 i  = floor(v + dot(v, C.yyy));
                float3 x0 = v - i + dot(i, C.xxx);
                float3 g = step(x0.yzx, x0.xyz);
                float3 l = 1 - g;
                float3 i1 = min(g.xyz, l.zxy);
                float3 i2 = max(g.xyz, l.zxy);
                float3 x1 = x0 - i1 + C.xxx;
                float3 x2 = x0 - i2 + 2.0*C.xxx;
                float3 x3 = x0 - 1.0 + 3.0*C.xxx;
                i = fmod(i, 289.0);
                float4 p = permute_3d(permute_3d(permute_3d(i.z + float4(0,i1.z,i2.z,1))
                    + i.y + float4(0,i1.y,i2.y,1)) + i.x + float4(0,i1.x,i2.x,1));
                float3 ns = (1.0/7.0) * D.wyz - D.xzx;
                float4 j = p - 49.0 * floor(p * ns.z * ns.z);
                float4 x_ = floor(j * ns.z);
                float4 y_ = floor(j - 7.0 * x_);
                float4 x = x_*ns.x + ns.yyyy;
                float4 y = y_*ns.x + ns.yyyy;
                float4 h = 1.0 - abs(x) - abs(y);
                float4 b0 = float4(x.xy, y.xy);
                float4 b1 = float4(x.zw, y.zw);
                float4 s0 = floor(b0)*2 + 1;
                float4 s1 = floor(b1)*2 + 1;
                float4 sh = -step(h, 0.0);
                float4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;
                float4 a1 = b1.xzyw + s1.xzyw*sh.zzww;
                float3 p0 = float3(a0.xy, h.x);
                float3 p1 = float3(a0.zw, h.y);
                float3 p2 = float3(a1.xy, h.z);
                float3 p3 = float3(a1.zw, h.w);
                float4 norm = taylorInvSqrt3d(float4(dot(p0,p0), dot(p1,p1), dot(p2,p2), dot(p3,p3)));
                p0 *= norm.x; p1 *= norm.y; p2 *= norm.z; p3 *= norm.w;
                float4 m = max(0.6 - float4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
                m = m*m;
                return 42.0 * dot(m*m, float4(dot(p0,x0), dot(p1,x1), dot(p2,x2), dot(p3,x3)));
            }
            float fbm3d(float3 x, int it)
            {
                float v=0, a=0.5;
                float3 shift=100;
                [unroll] for(int i=0;i<32;i++) if(i<it){ v+=a*simplexNoise3d(x); x = x*2+shift; a*=0.5; }
                return v;
            }
            float3 rotateZ(float3 v, float ang)
            {
                float c=cos(ang), s=sin(ang);
                return float3(v.x*c - v.y*s, v.x*s + v.y*c, v.z);
            }

            fixed4 frag(vert2frag i) : SV_Target
            {
                // UV 居中 [-1,1]
                float2 uv = i.uv * 2 - 1;
                uv.x *= _ScreenParams.y / _ScreenParams.x;

                // 圆形边缘渐隐 alpha
                float d = length(uv);
                float alpha = smoothstep(_MaskRadius, _MaskRadius - _EdgeFeather, d);
                if(alpha <= 0) discard;

                // 噪声逻辑
                float3 col = float3(uv, 0);
                col.z += 0.5;
                col = normalize(col);
                col -= 0.2 * float3(0,0,_Time.y * _Speed);
                col = rotateZ(col, -log2(length(uv)));
                col.x = fbm3d(col*_Frequency + 0, 5) + _Distortion;
                col.y = fbm3d(col*_Frequency + 1, 5) + _Distortion;
                col.z = fbm3d(col*_Frequency + 2, 5) + _Distortion;

                // 发光色
                float3 finalCol = _Color.rgb * (length(col) * 0.4);
                return float4(finalCol, alpha);
            }
            ENDCG
        }
    }
}
