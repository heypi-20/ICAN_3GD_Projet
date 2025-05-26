// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Custom/RotatingRing"
{
    Properties
    {
        _RingColor    ("环颜色", Color)      = (1,1,1,1)
        _Speed        ("旋转速度", Float)    = 1
        _Threshold    ("环宽度阈值", Range(0.8,0.999)) = 0.98
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            fixed4 _RingColor;
            float  _Speed;
            float  _Threshold;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 归一化到单位球面
                float3 p = normalize(i.worldPos);
                // 计算当前像素绕 Y 轴的经度角（-PI 到 +PI）
                float ang = atan2(p.z, p.x);
                // 环的相位随时间旋转
                float phase = ang - _Time.y * _Speed;
                // 用 cos(phase) 在 +1 处高亮，其他地方透明
                float c = cos(phase);
                // 阈值控制环的粗细：c 从 _Threshold~1 这一小段映射到 0~1
                float mask = smoothstep(_Threshold, 1, c);
                // 输出环色 + alpha
                return fixed4(_RingColor.rgb, mask * _RingColor.a);
            }
            ENDCG
        }
    }
}
