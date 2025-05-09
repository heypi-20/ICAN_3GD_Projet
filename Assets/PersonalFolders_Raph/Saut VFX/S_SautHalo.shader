Shader "Custom/Halo"
{
    Properties
    {
        _BottomColor ("Couleur Bas (RGBA)", Color)    = (0.1, 0.0, 0.2, 0.1)
        _TopColor    ("Couleur Haut (RGBA)", Color)   = (1.0, 0.4, 1.0, 0.8)
        _MinY        ("Hauteur Mini (world Y)", Float) = 0.05
        _MaxY        ("Hauteur Maxi (world Y)", Float) = 2.00
        _Thickness   ("Épaisseur Contour", Range(0.01,0.5)) = 0.10
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend One One    // Additive

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // PROPRIÉTÉS EXPOSÉES
            fixed4 _BottomColor;
            fixed4 _TopColor;
            float  _MinY;
            float  _MaxY;
            float  _Thickness;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 localPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                // position clip-space
                o.pos = UnityObjectToClipPos(v.vertex);
                // world space pour gradient vertical
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // object space pour calcul radial (le cylindre doit être un mesh de rayon 0.5 à l’import)
                o.localPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1) interpolation hauteur
                float t = saturate((i.worldPos.y - _MinY) / (_MaxY - _MinY));
                fixed4 col = lerp(_BottomColor, _TopColor, t);

                // 2) masque radial pour n’afficher que le contour
                //    note : le cylindre importé doit avoir un rayon d’1 unité ; on normalise donc localPos.xz par 0.5
                float radial = length(i.localPos.xz) / 0.5;
                // smoothstep adouci entre (1–thickness)→1
                float mask = smoothstep(1 - _Thickness, 1.0, radial);

                col.a *= mask;
                // pour que l’additive soit plus propre, on pré‐multiplie la couleur par l’alpha
                col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}