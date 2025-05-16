Shader "Custom/FireFlame"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
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
            float _Speed;
            float _EdgeSoftness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // <- couleur du particle system
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR; // <- passe au fragment
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color; // <- transfert
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv.y += _Time.y * _Speed;

                fixed4 tex = tex2D(_MainTex, uv);

                // Bords arrondis
                float2 centerOffset = i.uv - 0.5;
                float dist = length(centerOffset) * _EdgeSoftness;
                float edgeMask = saturate(1.0 - dist);

                // Applique la couleur du système de particules
                return tex * i.color * edgeMask;
            }
            ENDHLSL
        }
    }
}
