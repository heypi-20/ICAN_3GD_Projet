Shader "Unlit/LavaGeyser"
{
    Properties
    {
        _MainTex ("Lava Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 0.5, 0, 1)
        _ScrollSpeed ("Scroll Speed", Vector) = (0, 1, 0, 0)
        _Intensity ("Emission Intensity", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Lighting Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // Particle color from system
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _ScrollSpeed;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                float2 scrolledUV = v.uv + (_ScrollSpeed.xy * _Time.y);
                o.uv = TRANSFORM_TEX(scrolledUV, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv);
                half3 emissive = texColor.rgb * i.color.rgb * _Intensity;
                return half4(emissive, texColor.a * i.color.a);
            }
            ENDHLSL
        }
    }
}
