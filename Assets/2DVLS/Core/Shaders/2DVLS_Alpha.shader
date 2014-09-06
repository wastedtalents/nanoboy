Shader "2DVLS/Alpha" 
{
	Properties 
	{
		_MainTex ("", any) = "" {}
		_Alpha ("", any) = "" {}
	}

	SubShader 
	{
		Tags {"Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
	
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
	
		Pass 
		{  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
			
				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
				};

				sampler2D _MainTex;
				sampler2D _Alpha;
				float4 _MainTex_ST;
			
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					return o;
				}
			
				fixed4 frag (v2f i) : COLOR
				{
					half4 col = tex2D(_MainTex, i.texcoord);
					half4 alp = tex2D(_Alpha, i.texcoord);

					return lerp(col, half4(col.r, col.g, col.b, 1), alp.a);
				}
			ENDCG
		}
	}

}