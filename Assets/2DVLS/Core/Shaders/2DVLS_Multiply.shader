Shader "2DVLS/Multiply" {
Properties {
	_MainTex ("", any) = "" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend DstColor SrcColor
	Cull Off 
	Lighting Off 
	ZWrite Off
	//Fog { Mode On }
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			sampler2D _CameraDepthTexture;
			float _InvFade;
			
			fixed4 frag (v2f i) : COLOR
			{				
				half4 prev = i.color * tex2D(_MainTex, i.texcoord);
				half4 nCol = lerp(half4(1,1,1,1), prev, prev.a);

				return half4(nCol.r, nCol.g, nCol.b, 1);
			}
			ENDCG 
		}
	}
}
}
