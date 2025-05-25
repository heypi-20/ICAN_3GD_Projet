Shader "Custom/PortalSoftMaskNoise_TwoColors_Soft"
{
    Properties
    {
        _Frequency      ("Noise Frequency",   Float) = 1.4
        _Distortion     ("Noise Distortion",  Float) = 0.01
        _Speed          ("Animation Speed",   Float) = 1.0
        _MaskRadius     ("Portal Radius",     Float) = 1.0
        _EdgeFeather    ("Edge Fade Width",   Float) = 0.2

        _ColorCenter    ("Center Tint Color", Color) = (1,0.2,0.2,1)
        _ColorEdge      ("Edge Tint Color",   Color) = (0,0,0,1)
        _ColorFeather   ("Color Gradient Softness", Float) = 0.3
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
            float4 _ColorCenter;
            float4 _ColorEdge;
            float _MaskRadius;
            float _EdgeFeather;
            float _ColorFeather;

            // Simplex & FBM (同原版，不变)
            float4 permute_3d(float4 x) { return fmod(((x*34.0)+1.0)*x, 289.0); }
            float4 taylorInvSqrt3d(float4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
            float simplexNoise3d(float3 v)
            {
                // ... 原始实现略去，完全保留
                const float2 C = float2(1.0/6.0, 1.0/3.0);
                const float4 D = float4(0,0.5,1,2);
                float3 i = floor(v + dot(v, C.yyy));
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
                float3 ns = (1.0/7.0)*D.wyz - D.xzx;
                float4 j = p - 49.0 * floor(p * ns.z * ns.z);
                float4 x_ = floor(j * ns.z);
                float4 y_ = floor(j - 7.0*x_);
                float4 x = x_*ns.x + ns.yyyy;
                float4 y = y_*ns.x + ns.yyyy;
                float4 h = 1.0 - abs(x) - abs(y);
                float4 b0 = float4(x.xy, y.xy);
                float4 b1 = float4(x.zw, y.zw);
                float4 s0 = floor(b0)*2 + 1;
                float4 s1 = floor(b1)*2 + 1;
                float4 sh = -step(h,0);
                float4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;
                float4 a1 = b1.xzyw + s1.xzyw*sh.zzww;
                float3 p0 = float3(a0.xy, h.x);
                float3 p1 = float3(a0.zw, h.y);
                float3 p2 = float3(a1.xy, h.z);
                float3 p3 = float3(a1.zw, h.w);
                float4 norm = taylorInvSqrt3d(float4(dot(p0,p0), dot(p1,p1), dot(p2,p2), dot(p3,p3)));
                p0*=norm.x; p1*=norm.y; p2*=norm.z; p3*=norm.w;
                float4 m = max(0.6 - float4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
                m = m*m;
                return 42.0 * dot(m*m, float4(dot(p0,x0), dot(p1,x1), dot(p2,x2), dot(p3,x3)));
            }
            float fbm3d(float3 x, int it)
            {
                float v=0, a=0.5;
                float3 shift=100;
                [unroll] for(int i=0;i<it;i++) { v += a * simplexNoise3d(x); x = x*2 + shift; a *= 0.5; }
                return v;
            }
            float3 rotateZ(float3 v, float ang)
            {
                float c=cos(ang), s=sin(ang);
                return float3(v.x*c - v.y*s, v.x*s + v.y*c, v.z);
            }

            struct app2vert { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct vert2frag { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            vert2frag vert(app2vert v)
            {
                vert2frag o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(vert2frag i):SV_Target
            {
                // UV 居中并修正宽高比
                float2 uv = i.uv * 2 - 1;
                uv.x *= _ScreenParams.y / _ScreenParams.x;

                // 圆形软蒙版
                float d = length(uv);
                float alpha = smoothstep(_MaskRadius, _MaskRadius - _EdgeFeather, d);
                if(alpha <= 0) discard;

                // 噪声扭曲流动
                float3 ncol = float3(uv,0);
                ncol.z += 0.5;
                ncol = normalize(ncol);
                ncol -= 0.2 * float3(0,0,_Time.y * _Speed);
                ncol = rotateZ(ncol, -log2(max(length(uv),0.0001)));
                ncol.x = fbm3d(ncol * _Frequency + 0, 5) + _Distortion;
                ncol.y = fbm3d(ncol * _Frequency + 1, 5) + _Distortion;
                ncol.z = fbm3d(ncol * _Frequency + 2, 5) + _Distortion;
                float intensity = length(ncol) * 0.4;

                // 双色渐变 with softness
                float lower = _MaskRadius * (1.0 - _ColorFeather);
                float upper = _MaskRadius;
                float t = smoothstep(lower, upper, d);
                float3 tint = lerp(_ColorCenter.rgb, _ColorEdge.rgb, t);

                // 将颜色乘以 alpha 以消除边缘噪声
                float3 finalCol = tint * intensity * alpha;

                return float4(finalCol, alpha);
            }
            ENDCG
        }
    }
}
