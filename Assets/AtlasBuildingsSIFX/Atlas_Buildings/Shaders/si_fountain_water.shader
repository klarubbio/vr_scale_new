Shader "sevn_interactive/si_fountain_water" 
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Speed("Speed",Float)=1
	}
	SubShader 
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200
		Cull Off
		CGPROGRAM
		#pragma surface surf Lambert alpha

		sampler2D _MainTex;
		float _Speed;
		
		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			float2 m_offset=float2(0,-frac(_Time.x * _Speed));
			half4 m_tex = tex2D (_MainTex, IN.uv_MainTex + m_offset);
			o.Albedo = m_tex.rgb;
			o.Alpha = m_tex.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
