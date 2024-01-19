// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Adaptive Input/Key Input Shader"
{

	Properties{
		 _Color("Tint", Color) = (0, 0, 0, 1)
		 _MainTex("Texture", 2D) = "white" {}
		 _HighlightedTex("Texture", 2D) = "white" {}
		 _HighlightKeyA("HighlightKeyA", Float) = 0
		 _KeyA("KeyA", Vector) = (0,0,0,0)
		 _HighlightKeyB("HighlightKeyB", Float) = 0
		 _KeyB("KeyB", Vector) = (0,0,0,0)
	}

		SubShader{
			Tags{ "RenderType" = "Transparent" "Queue" = "Transparent"}

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite off

			Pass{
				CGPROGRAM

				#include "UnityCG.cginc"

				#pragma vertex vert
				#pragma fragment frag

				sampler2D _MainTex;
				sampler2D _HighlightedTex;
				float4 _MainTex_ST;
				float4 _HighlightedTex_ST; 
				float _HighlightKeyA;
				float4 _KeyA;
				float _HighlightKeyB;
				float4 _KeyB;

				fixed4 _Color;

				struct appdata {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f {
					float4 position : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				v2f vert(appdata v) {
					v2f o;
					o.position = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_TARGET{
					fixed4 col;
					
					fixed2 posA = _KeyA.xy;
					fixed2 scaleA = _KeyA.zw; 
					fixed2 highlightA = i.uv; 
					highlightA -= posA; 
					highlightA = abs(highlightA);
					highlightA = scaleA - highlightA; 
					highlightA = step(0, highlightA);

					fixed2 posB = _KeyB.xy;
					fixed2 scaleB = _KeyB.zw;
					fixed2 highlightB = i.uv;
					highlightB -= posB;
					highlightB = abs(highlightB);
					highlightB = scaleB - highlightB;
					highlightB = step(0, highlightB);

					fixed val = min(_HighlightKeyA * highlightA.x * highlightA.y + _HighlightKeyB * highlightB.x * highlightB.y, 1);
					col = val * tex2D(_HighlightedTex, i.uv) + (1 - val) * tex2D(_MainTex, i.uv);

					return col;
				}

				ENDCG
			}
	}
}