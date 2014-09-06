Shader "2DVLS/Light"
{
	Properties
	{
		_MainTex ("Base (RGB) Trans. (Alpha)", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Back
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend One One
		 
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile DUMMY PIXELSNAP_ON
				#include "UnityCG.cginc"
				
				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color    : COLOR;
					half2 texcoord  : TEXCOORD0;
				};
				
				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = float4(IN.color.rgb, 0);
					
					return OUT;
				}

				sampler2D _MainTex;

				fixed4 frag(v2f IN) : COLOR
				{
					return tex2D(_MainTex, IN.texcoord) * IN.color;
				}
			ENDCG
		}
	}
}