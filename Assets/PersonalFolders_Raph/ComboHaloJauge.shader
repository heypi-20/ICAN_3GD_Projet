Shader "URP/Unlit/WaveInsideMask_AutoAnim"
{
    Properties
    {
        _MaskTex ("Shape Mask (Alpha)", 2D) = "white" {}
        _WaveTex ("Wave Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Float) = 1
        _WaveScale ("Wave Scale", Float) = 1
        _WaveColor ("Wave Color", Color) = (1,1,1,1)
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
            sampler2D _WaveTex;

            float4 _MaskTex_ST;
            float4 _WaveTex_ST;

            float _WaveSpeed;
            float _WaveScale;
            float4 _WaveColor;

            struct Attributes { float4 posOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Masque alpha
                float alpha = tex2D(_MaskTex, uv).a;
                if (alpha < 0.01)
                    discard;

                // Animation permanente dans l’UV
                float2 waveUV = uv * _WaveScale + float2(_Time.y * _WaveSpeed, 0.0);
                float wave = tex2D(_WaveTex, waveUV).r;

                // Combine avec la couleur des vagues et masque alpha
                float4 col = _WaveColor * wave;
                col.a = wave * alpha;

                return col;
            }
            ENDHLSL
        }
    }
}