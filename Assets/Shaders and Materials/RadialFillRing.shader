Shader "Custom/RadialFillRing"
{
    Properties
    {
        _MainTex ("Ring Texture", 2D) = "white" {}
        _FillAmount ("Fill Amount", Range(0,1)) = 0.0
        _Color ("Fill Color", Color) = (1,1,1,1)
        _BackgroundColor ("Background Color", Color) = (0.5, 0.5, 0.5, 0.15)
        // 1.5708 is pi/2 (the top of the ring)
        _FillStartAngle ("Fill Start Angle", Float) = 1.5708
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _BackgroundColor;
                float _FillAmount;
                float _FillStartAngle;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                float2 centered = IN.uv - 0.5;
                float angle = atan2(centered.y, centered.x) - _FillStartAngle;
                float normalizedAngle = frac(angle / (2.0 * PI));
                float inFill = smoothstep(normalizedAngle - 0.01, normalizedAngle + 0.01, _FillAmount);
                
                half4 fillColor = texColor * _Color;
                half4 bgColor = texColor * _BackgroundColor;
                return lerp(bgColor, fillColor, inFill);
            }
            ENDHLSL
        }
    }
}
