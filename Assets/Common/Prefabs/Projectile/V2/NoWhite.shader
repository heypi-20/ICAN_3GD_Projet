Shader "Custom/NoWhite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0,0,0,1)
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 1
        _UVSpeed ("UV Rotation Speed (deg/sec)", Float) = 30
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
            float _UVSpeed;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float2 RotateUV(float2 uv, float angle)
            {
                float2 center = float2(0.5, 0.5);
                uv -= center;
                float rad = radians(angle);
                float cosA = cos(rad);
                float sinA = sin(rad);
                float2 rotated = float2(
                    uv.x * cosA - uv.y * sinA,
                    uv.x * sinA + uv.y * cosA
                );
                return rotated + center;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float angle = _Time.y * _UVSpeed;
                float2 uvRotated = RotateUV(i.uv, angle);

                fixed4 texCol = tex2D(_MainTex, uvRotated);
                float brightness = dot(texCol.rgb, float3(0.299, 0.587, 0.114));
                float alpha = 1.0 - brightness;

                fixed3 baseColor = _Color.rgb;
                fixed3 emissive = _EmissionColor.rgb * _EmissionStrength * (1.0 - brightness);

                // Mélange équilibré entre base et émissive
                fixed3 finalColor = baseColor + emissive * 0.5;

                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
}
