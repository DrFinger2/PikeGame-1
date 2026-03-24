Shader "Custom/URP_DepthPrepass"
{
    SubShader
    {
        // Render in the Opaque/Geometry queue so it draws FIRST
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "DepthMask"
            ColorMask 0 // Write NO color to the screen
            ZWrite On   // DO write to the depth buffer

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(0,0,0,0);
            }
            ENDHLSL
        }
    }
}