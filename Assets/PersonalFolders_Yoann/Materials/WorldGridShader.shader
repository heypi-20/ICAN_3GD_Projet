// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "WorldGridShader"
{
	Properties
	{
		_GridTexture("GridTexture", 2D) = "white" {}
		_Color0("Color 0", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color0;
		uniform sampler2D _GridTexture;
		uniform float4 _GridTexture_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 Color118 = _Color0;
			float2 uv_GridTexture = i.uv_texcoord * _GridTexture_ST.xy + _GridTexture_ST.zw;
			o.Albedo = ( Color118 * tex2D( _GridTexture, uv_GridTexture ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.CommentaryNode;119;-3552,192;Inherit;False;1476;277;Property;6;72;68;43;73;117;118;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;116;-2032,-464;Inherit;False;1546;965;Apply Texture Based on Face;17;51;59;65;58;64;69;88;54;55;53;70;113;60;66;71;114;89;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;115;-3552.855,-462.2167;Inherit;False;1435.478;635;Check Face Orientation;9;105;104;106;107;111;109;108;110;112;;1,1,1,1;0;0
Node;AmplifyShaderEditor.AbsOpNode;105;-3294.857,-262.7696;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;104;-3502.855,-262.7696;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.BreakToComponentsNode;106;-3150.857,-262.7696;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.Compare;107;-2791.378,-412.2166;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;111;-2567.378,-412.2166;Inherit;False;X Greater Y;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;109;-2791.378,-252.2166;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;108;-2791.378,-12.21676;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;110;-2551.378,-252.2166;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;112;-2359.378,-252.2166;Inherit;False;Z Greater Max XY;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;51;-1984,-176;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1248,-176;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FractNode;65;-1056,-176;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-1248,-416;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FractNode;64;-1056,-416;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;69;-1472,-304;Inherit;False;68;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;88;-848,-192;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;54;-1728,-176;Inherit;True;True;True;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;55;-1712,272;Inherit;True;True;True;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;53;-1728,-416;Inherit;True;True;True;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;70;-1472,-64;Inherit;False;68;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;-1040,-48;Inherit;False;111;X Greater Y;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1232,272;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FractNode;66;-1040,272;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;71;-1456,384;Inherit;False;68;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-816,224;Inherit;False;112;Z Greater Max XY;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;89;-672,-16;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-3024,240;Inherit;False;Property;_TextureScale;Texture Scale;0;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;68;-2832,240;Inherit;False;TextureScale;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;43;-3504,240;Inherit;True;Property;_GridTexture;GridTexture;1;0;Create;True;0;0;0;False;0;False;9470468fda72f4773a90a0306decbf0f;9470468fda72f4773a90a0306decbf0f;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;73;-3280,240;Inherit;False;GridTexture;-1;True;1;0;SAMPLER2D;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;-2320,240;Inherit;False;Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;117;-2576,240;Inherit;False;Property;_Color0;Color 0;2;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;26;-192,-352;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;9470468fda72f4773a90a0306decbf0f;9470468fda72f4773a90a0306decbf0f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;74;-400,-352;Inherit;False;73;GridTexture;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;121;-400,-464;Inherit;False;118;Color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;128,-464;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;320,-464;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;WorldGridShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;105;0;104;0
WireConnection;106;0;105;0
WireConnection;107;0;106;0
WireConnection;107;1;106;1
WireConnection;111;0;107;0
WireConnection;109;0;106;0
WireConnection;109;1;106;1
WireConnection;108;0;106;1
WireConnection;108;1;106;2
WireConnection;110;0;106;2
WireConnection;110;1;109;0
WireConnection;112;0;110;0
WireConnection;59;0;54;0
WireConnection;59;1;70;0
WireConnection;65;0;59;0
WireConnection;58;0;53;0
WireConnection;58;1;69;0
WireConnection;64;0;58;0
WireConnection;88;0;64;0
WireConnection;88;1;65;0
WireConnection;88;2;113;0
WireConnection;54;0;51;0
WireConnection;55;0;51;0
WireConnection;53;0;51;0
WireConnection;60;0;55;0
WireConnection;60;1;71;0
WireConnection;66;0;60;0
WireConnection;89;0;88;0
WireConnection;89;1;66;0
WireConnection;89;2;114;0
WireConnection;68;0;72;0
WireConnection;73;0;43;0
WireConnection;118;0;117;0
WireConnection;26;0;74;0
WireConnection;120;0;121;0
WireConnection;120;1;26;0
WireConnection;0;0;120;0
ASEEND*/
//CHKSM=A25AAFFB41B15B979379150292CB780F471AAA77