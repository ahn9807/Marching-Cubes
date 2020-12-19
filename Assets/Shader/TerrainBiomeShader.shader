Shader "Custom/TerrainBiomeShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTexArray ("Terrain Texture Array", 2DArray) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        UNITY_DECLARE_TEX2DARRAY(_MainTexArray);

        struct Input
        {
            float4 color : COLOR;
            float3 worldPos;
            float3 normalPos;
            float3 terrain;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input data) {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.terrain = v.texcoord2.xyz;
        }

        float4 GetTerrainColor (Input IN, float scale) {
            float3 scaledWordPos = IN.worldPos / 50;
			float4 c_xy = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, float3(scaledWordPos.x, scaledWordPos.y, IN.terrain[0]));
		    float4 c_xz = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, float3(scaledWordPos.x, scaledWordPos.z, IN.terrain[0]));
		    float4 c_yz = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, float3(scaledWordPos.y, scaledWordPos.z, IN.terrain[0]));
            return (c_xy + c_xz + c_yz) / 3;
		} 

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = GetTerrainColor(IN, IN.terrain[1]);
			o.Albedo = c.rgb * _Color;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
