Shader "Custom/TacheShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0,0,0,1)
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 1
        // _UVSpeed supprimé car plus utilisé
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _EmissionColor;
            float _EmissionStrength;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
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
                fixed4 texCol = tex2D(_MainTex, i.uv);
                float brightness = dot(texCol.rgb, float3(0.299, 0.587, 0.114));
                float alpha = 1.0 - brightness;

                fixed3 baseColor = _Color.rgb;
                fixed3 emissive = _EmissionColor.rgb * _EmissionStrength * (1.0 - brightness);

                fixed3 finalColor = baseColor + emissive * 0.5;

                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
}
