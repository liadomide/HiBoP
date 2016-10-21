﻿

Shader "Custom/MeshShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_AoTex("AO (RGB)", 2D) = "white" {}
		_ColorTex("Color map (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		//_Amount("Extrusion Amount", Range(-1,1)) = 0.5
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM

			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;
			sampler2D _AoTex;
			sampler2D _ColorTex;

			struct Input
			{
				float4 pos : SV_POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float2 uv2_AoTex :   TEXCOORD1;
				float2 uv3_ColorTex :   TEXCOORD2;
				//float3 worldPos;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			//float _Amount;
			//void vert(inout appdata_full v) {
			//	v.vertex.xyz += v.normal * _Amount;
			//}


			void surf(Input IN, inout SurfaceOutputStandard o)
			{				
				//clip(frac((IN.worldPos.y + IN.worldPos.z*0.1) * 5) - 0.5);

				fixed4 ao = tex2D(_AoTex, IN.uv2_AoTex.xy);
				float color = ao.r;

				// boost alpha (because of low tri mesh density compated to cuts textures)
				color *= 3;				
				if (color > 1)
					color = 1;
	
				fixed4 col = (1 - color) * tex2D(_MainTex, IN.uv_MainTex.xy) + (color)* tex2D(_ColorTex, IN.uv3_ColorTex.xy);
							
				col *= _Color;
				o.Albedo = col.rgb;
				o.Alpha = col.a;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
			}

		ENDCG
	}

		FallBack "Diffuse"
}
