// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SpriteShadow" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
	}
		SubShader{
			Tags
			{
				"Queue" = "Geometry"
				"RenderType" = "TransparentCutout"
			}
			LOD 200

			Cull Off

			CGPROGRAM
			// Lambert lighting model, and enable shadows on all light types
			#pragma surface surf Lambert addshadow fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;
			fixed4 _Color;
			fixed _Cutoff;
			half4 _In0;
			half4 _Out0;
			half4 _In1;
			half4 _Out1;
			half4 _In2;
			half4 _Out2;
			half4 _In3;
			half4 _Out3;
			struct Input
			{
				float2 uv_MainTex;
			};
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			half4 frag(v2f i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);

				if (all(col.rgb == _In0.rgb))
					return _Out0;

				if (all(col.rgb == _In1.rgb))
					return _Out1;

				if (all(col.rgb == _In2.rgb))
					return _Out2;

				if (all(col.rgb == _In3.rgb))
					return _Out3;

				return col;
			}
			void surf(Input IN, inout SurfaceOutput o) {
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Alpha = c.a;
				clip(o.Alpha - _Cutoff);
			}
			ENDCG
		}
			FallBack "Diffuse"
}