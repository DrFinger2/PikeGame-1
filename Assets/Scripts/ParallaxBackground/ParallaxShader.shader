Shader "Custom/URP_ParallaxLayer"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Tiling("Tiling and Offset", Vector) = (1,1,0,0)
        _UseFade("Use Radial Fade", Float) = 0
        [Toggle] _UseCameraFade("Use Camera Fade", Float) = 1
        _FadeStart("Fade Start Distance", Float) = 0.1
        _FadeRange("Fade Range", Float) = 0.2
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDirVS : TEXCOORD1; // View direction in view space
                float4 screenPos : TEXCOORD2; // Screen position for depth sampling
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float4 _Tiling;
            float _UseFade;
            float _UseCameraFade;
            float _FadeStart;
            float _FadeRange;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // Transform position to clip space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                
                // Calculate view direction in view space
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 cameraPosWS = _WorldSpaceCameraPos;
                float3 viewDirWS = positionWS - cameraPosWS;
                OUT.viewDirVS = TransformWorldToViewDir(viewDirWS);
                
                // Calculate screen position for depth sampling
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Apply tiling and offset
                float2 tiledUV = IN.uv * _Tiling.xy + _Tiling.zw;
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, tiledUV) * _Color;
                
                // Apply radial fade effect if enabled
                if (_UseFade > 0.5)
                {
                    float2 center = float2(0.5, 0.5);
                    float2 distVec = abs(IN.uv - center) * 2;
                    float fade = 1.0 - saturate(max(distVec.x, distVec.y));
                    color.a *= fade;
                }
                
                // Apply camera proximity fade if enabled
                if (_UseCameraFade > 0.5)
                {
                    // Calculate fragment depth in view space
                    float depth = -IN.viewDirVS.z; // Absolute distance to camera
                    
                    // Calculate fade factor (1 = fully visible, 0 = fully faded)
                    float fade = saturate((depth - _FadeStart) / _FadeRange);
                    color.a *= fade;
                }
                
                return color;
            }
            ENDHLSL
        }
    }
}