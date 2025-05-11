Shader "Custom/AdditiveScroll_Unlit"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Mask("Mask (Grayscale)", 2D) = "white" {}
        _MaskTiling("Mask Tiling", Vector) = (1,1,0,0)

        _MainTex("Main Tex (optional)", 2D) = "white" {}
        _MainTexTiling("MainTex Tiling", Vector) = (1,1,0,0)
        _MainTexSpeed("MainTex Speed", Vector) = (0,0,0,0)

        _DistortionTex("Distortion Tex", 2D) = "white" {}
        _DistortionAmount("Distortion Amount", Range(0,1)) = 0.33
        _DistortionTiling("Distortion Tiling", Vector) = (1,1,0,0)
        _DistortionSpeed("Distortion Speed", Vector) = (0.5,-0.5,0,0)

        _DissolveTex("Dissolve Tex", 2D) = "white" {}
        _DissolveAmount("Dissolve Amount", Range(0,1)) = 0.5
        _DissolveTiling("Dissolve Tiling", Vector) = (1,1,0,0)
        _DissolveSpeed("Dissolve Speed", Vector) = (0,1,0,0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Propriétés
            fixed4 _Color;
            sampler2D _Mask;
            float4 _MaskTiling;

            sampler2D _MainTex;
            float4 _MainTexTiling;
            float4 _MainTexSpeed;

            sampler2D _DistortionTex;
            float _DistortionAmount;
            float4 _DistortionTiling;
            float4 _DistortionSpeed;

            sampler2D _DissolveTex;
            float _DissolveAmount;
            float4 _DissolveTiling;
            float4 _DissolveSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uvMask       : TEXCOORD0;
                float2 uvMain       : TEXCOORD1;
                float2 uvDistortion : TEXCOORD2;
                float2 uvDissolve   : TEXCOORD3;
                float4 vertex       : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Tiling + scroll pour chaque UV
                float t = _Time.y; // temps en secondes

                o.uvMask = v.uv * _MaskTiling.xy;

                o.uvMain = v.uv * _MainTexTiling.xy
                         + _MainTexSpeed.xy * t;

                o.uvDistortion = v.uv * _DistortionTiling.xy
                               + _DistortionSpeed.xy * t;

                o.uvDissolve = v.uv * _DissolveTiling.xy
                             + _DissolveSpeed.xy * t;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample distortion noise
                fixed4 dist = tex2D(_DistortionTex, i.uvDistortion);
                // Offset UV for mask/main
                float2 maskUV = i.uvMask + (dist.rg * 2 - 1) * _DistortionAmount;

                // Sample mask
                fixed4 mask = tex2D(_Mask, maskUV);

                // Sample optional main texture
                fixed4 mainCol = tex2D(_MainTex, i.uvMain);

                // Multiply mask by color and main texture
                fixed4 col = mask * _Color * mainCol;

                // Dissolve factor
                float dissolveNoise = tex2D(_DissolveTex, i.uvDissolve).r;
                float keep = step(_DissolveAmount, dissolveNoise);

                col.a *= keep;

                return col;
            }
            ENDHLSL
        }
    }
}