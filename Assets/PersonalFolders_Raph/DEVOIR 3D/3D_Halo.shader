Shader "URP/Unlit/RotatingHaloMasked"
{
    Properties
    {
        _MaskTex ("Shape Mask (Alpha)", 2D) = "white" {}
        _HaloColor ("Halo Color", Color) = (1,1,1,1)
        _HaloAlpha ("Halo Transparency", Range(0.0, 1.0)) = 0.5
        _HaloWidth ("Halo Width", Range(0.01, 1.0)) = 0.1
        _HaloSharpness ("Halo Sharpness", Range(1, 50)) = 10
        _Speed ("Rotation Speed", Float) = 1.0
        _AngleOffset ("Angle Offset", Float) = 0
        [Toggle] _InvertDirection ("Invert Rotation (Clockwise)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MaskTex;
            float4 _MaskTex_ST;

            float4 _HaloColor;
            float _HaloAlpha;
            float _HaloWidth;
            float _HaloSharpness;
            float _Speed;
            float _AngleOffset;
            float _InvertDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MaskTex);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Masque alpha
                float alpha = tex2D(_MaskTex, uv).a;
                if (alpha < 0.01)
                    discard;

                // Coordonnées centrées pour calcul d’angle
                float2 centeredUV = uv * 2.0 - 1.0;
                float angle = atan2(centeredUV.y, centeredUV.x);
                angle = (angle + 3.14159265) / (2.0 * 3.14159265); // Normalize [0..1]

                // Sens du temps
                float direction = (_InvertDirection > 0.5) ? -1.0 : 1.0;
                float rotation = frac(angle + (direction * _Time.y * _Speed) + _AngleOffset);

                // Forme du halo
                float halo = exp(-pow((rotation - 0.5) / _HaloWidth, 2.0) * _HaloSharpness);

                float intensity = halo * alpha;

                float4 finalColor = _HaloColor * intensity;
                finalColor.a = intensity * _HaloAlpha;

                return finalColor;
            }
            ENDHLSL
        }
    }
}