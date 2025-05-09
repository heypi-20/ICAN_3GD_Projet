Shader "Custom/UI/EnergyBarFlow" {
    Properties {
        _MainTex ("UI Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _NoiseColor ("Noise Color", Color) = (1,1,1,1)
        _EdgeColor ("Edge Transition Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _NoiseScale ("Noise Scale", Float) = 1.0
        _NoiseSpeed ("Noise Speed", Float) = 1.0
        _FlowDirection ("Noise Flow Direction (XY)", Vector) = (1,0,0,0)
        [Enum(Horizontal,0,Vertical,1)] _FillMode ("Fill Mode", Float) = 0
        _FlipFill ("Reverse Fill", Float) = 0
        _Progress ("Fill Progress", Range(0,1)) = 1.0
        _EdgeWidth ("Edge Fade Width", Range(0,0.5)) = 0.1
    }
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            float4 _TintColor;
            float4 _NoiseColor;
            float4 _EdgeColor;
            float4 _EmissionColor;
            float _NoiseScale;
            float _NoiseSpeed;
            float2 _FlowDirection;
            float _FillMode;
            float _FlipFill;
            float _Progress;
            float _EdgeWidth;

            v2f vert(appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                // Base texture color
                float4 baseCol = tex2D(_MainTex, i.uv) * _TintColor;

                // Select axis: 0=Horizontal (x), 1=Vertical (y)
                float proj = (_FillMode < 0.5) ? i.uv.x : i.uv.y;
                if (_FlipFill > 0.5) {
                    proj = 1 - proj;
                }

                // Noise for boundary modulation
                float2 noiseUV = i.uv * _NoiseScale + _Time.y * _NoiseSpeed * _FlowDirection;
                float noiseSample = tex2D(_NoiseTex, noiseUV).r;

                // Compute dynamic edges
                float noiseOffset = (noiseSample - 0.5) * _EdgeWidth;
                float startEdge = _Progress - _EdgeWidth + noiseOffset;
                float endEdge = _Progress + noiseOffset;
                float fillMask = smoothstep(startEdge, endEdge, proj);

                // Inside fill coloring
                float4 filledCol = lerp(baseCol, _NoiseColor, noiseSample);

                // Edge transition coloring
                float edgeFactor = smoothstep(startEdge, _Progress, proj);
                float4 edgeCol = lerp(baseCol, _EdgeColor, 1 - edgeFactor);

                // Composite layers
                float4 col = baseCol;
                col = lerp(col, edgeCol, edgeFactor);
                col = lerp(col, filledCol, fillMask);
                col.rgb += _EmissionColor.rgb * fillMask;
                col.a = baseCol.a * fillMask;
                return col;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
