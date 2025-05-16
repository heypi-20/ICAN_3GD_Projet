Shader "Custom/FireFlame"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 0.5, 0, 1)
        _Speed ("Scroll Speed", Float) = 1
        _EdgeSoftness ("Edge Softness", Float) = 2.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Speed;
            float _EdgeSoftness;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Scroll the UV for flame motion
                float2 uv = i.uv;
                uv.y += _Time.y * _Speed;

                // Texture lookup
                fixed4 tex = tex2D(_MainTex, uv);

                // Distance from UV center (0.5, 0.5)
                float2 centerOffset = i.uv - 0.5;
                float dist = length(centerOffset) * _EdgeSoftness;

                // Smooth circular fade
                float edgeMask = saturate(1.0 - dist);

                // Final color
                return tex * _Color * edgeMask;
            }
            ENDHLSL
        }
    }
}
