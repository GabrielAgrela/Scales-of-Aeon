Shader "Custom/URPTerrainBlend"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _SecondMap ("Overlay Texture", 2D) = "white" {}
        _Blend ("Blend Factor", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            TEXTURE2D(_SecondMap);
            SAMPLER(sampler_BaseMap);
            SAMPLER(sampler_SecondMap);
            float _Blend;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.uv = IN.uv;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = mul(UNITY_MATRIX_M, IN.positionOS).xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 overlayColor = SAMPLE_TEXTURE2D(_SecondMap, sampler_SecondMap, IN.uv);
                
                // Calculate the blend factor based on the world position
                float blendValue = sin(IN.worldPos.x) * sin(IN.worldPos.z);
                blendValue = _Blend * (0.5 + 0.5 * blendValue);

                // Blend the base texture and the overlay texture
                half4 finalColor = lerp(baseColor, overlayColor, blendValue);
                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
