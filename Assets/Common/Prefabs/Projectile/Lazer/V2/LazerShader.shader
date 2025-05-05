Shader "Custom/LazerShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Tint ("Tint Color", Color) = (1,1,1,1)
        _UVScroll ("UV Scroll (X,Y)", Vector) = (1, 0, 0, 0)
        _UVRotation ("UV Rotation (Degrees)", Float) = 0

        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Float) = 1

        _MaskTex ("Mask Texture", 2D) = "white" {}
        _MaskStrength ("Mask Influence", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            float4 _Tint;
            float4 _UVScroll;
            float _UVRotation;

            float4 _EmissionColor;
            float _EmissionStrength;

            float _MaskStrength;

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
                float2 uv = v.uv;

                // Rotation
                float angle = radians(_UVRotation);
                float cosA = cos(angle);
                float sinA = sin(angle);

                uv -= 0.5;
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosA - uv.y * sinA;
                rotatedUV.y = uv.x * sinA + uv.y * cosA;
                uv = rotatedUV + 0.5;

                // Scroll
                uv += _UVScroll.xy * _Time.y;

                o.uv = TRANSFORM_TEX(uv, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainColor = tex2D(_MainTex, i.uv);
                fixed4 maskColor = tex2D(_MaskTex, i.uv);

                // Combine mask (multiplier effect)
                fixed4 maskedColor = lerp(mainColor, mainColor * maskColor, _MaskStrength);

                fixed4 baseColor = maskedColor * _Tint;

                fixed4 emission = _EmissionColor * _EmissionStrength;

                return baseColor + emission;
            }
            ENDCG
        }
    }
}
