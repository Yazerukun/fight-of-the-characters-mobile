/* === 07. RENDER PIPELINE, UI HUD, MODALS & TOASTS === */
Shader "FOC/URP/AnimeCelShading"
{
    Properties
    {
        [Header(Base Textures)]
        _BaseMap ("Base Texture (RGB)", 2D) = "white" {}
        _BaseColor ("Base Tint", Color) = (1, 1, 1, 1)

        [Header(Cel Shading Steps)]
        _ShadowThreshold ("Shadow Threshold", Range(0.0, 1.0)) = 0.45
        _ShadowSmoothness ("Shadow Smoothness", Range(0.001, 0.2)) = 0.04
        _ShadowColor ("Shadow Tint", Color) = (0.55, 0.58, 0.68, 1)

        [Header(Anime Hair and Specular)]
        _SpecularColor ("Specular Tint", Color) = (1, 1, 1, 1)
        _SpecularThreshold ("Specular Threshold", Range(0.5, 1.0)) = 0.85
        _SpecularSmoothness ("Specular Smoothness", Range(0.001, 0.1)) = 0.02

        [Header(Reiatsu Aura and Rim Lighting)]
        _RimColor ("Reiatsu Rim Color", Color) = (0.0, 0.82, 1.0, 1)
        _RimPower ("Rim Exponent", Range(0.5, 8.0)) = 3.2
        _RimIntensity ("Rim Intensity", Range(0.0, 5.0)) = 1.8
        _AuraPulseSpeed ("Aura Pulse Speed", Range(0.0, 10.0)) = 2.5

        [Header(Inverted Hull Outline)]
        _OutlineColor ("Outline Color", Color) = (0.05, 0.05, 0.08, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 5.0)) = 1.2
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry"
        }
        LOD 200

        // =========================================================================
        // PASS 1: FORWARD CEL SHADING
        // =========================================================================
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float3 viewDirWS    : TEXCOORD2;
                float3 positionWS   : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _ShadowThreshold;
                half _ShadowSmoothness;
                half4 _ShadowColor;
                half4 _SpecularColor;
                half _SpecularThreshold;
                half _SpecularSmoothness;
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;
                half _AuraPulseSpeed;
                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normalize(normInputs.normalWS);
                output.viewDirWS = normalize(GetCameraPositionWS() - posInputs.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                Light mainLight = GetMainLight();

                half3 N = normalize(input.normalWS);
                half3 L = normalize(mainLight.direction);
                half3 V = normalize(input.viewDirWS);
                half3 H = normalize(L + V);

                // 1. Two-Band Stepped Cel Diffuse
                half NdotL = dot(N, L);
                half halfLambert = NdotL * 0.5 + 0.5;
                half celFactor = smoothstep(_ShadowThreshold - _ShadowSmoothness, _ShadowThreshold + _ShadowSmoothness, halfLambert);
                half3 diffuseLighting = lerp(_ShadowColor.rgb, half3(1, 1, 1), celFactor) * mainLight.color;

                // 2. Stylized Anime Specular Band
                half NdotH = saturate(dot(N, H));
                half specFactor = smoothstep(_SpecularThreshold - _SpecularSmoothness, _SpecularThreshold + _SpecularSmoothness, NdotH);
                half3 specularLighting = _SpecularColor.rgb * specFactor * celFactor;

                // 3. Bleach Brave Souls Style Reiatsu / Rim Aura with Pulse
                half NdotV = 1.0 - saturate(dot(N, V));
                half rimBase = pow(NdotV, _RimPower);
                half pulse = 1.0 + 0.25 * sin(_Time.y * _AuraPulseSpeed);
                half3 rimLighting = _RimColor.rgb * rimBase * _RimIntensity * pulse;

                // Composite Final Stylized Color
                half3 finalRGB = (albedo.rgb * diffuseLighting) + specularLighting + rimLighting;
                return half4(finalRGB, albedo.a);
            }
            ENDHLSL
        }

        // =========================================================================
        // PASS 2: INVERTED HULL OUTLINE (Anime Ink Line)
        // =========================================================================
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex VertOutline
            #pragma fragment FragOutline

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _ShadowThreshold;
                half _ShadowSmoothness;
                half4 _ShadowColor;
                half4 _SpecularColor;
                half _SpecularThreshold;
                half _SpecularSmoothness;
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;
                half _AuraPulseSpeed;
                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END

            Varyings VertOutline(Attributes input)
            {
                Varyings output;
                float3 offsetPos = input.positionOS.xyz + (input.normalOS * (_OutlineWidth * 0.003));
                output.positionCS = TransformObjectToHClip(offsetPos);
                return output;
            }

            half4 FragOutline(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
