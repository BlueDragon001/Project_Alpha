Shader "Universal Render Pipeline/RayMarchingShader"
{
    Properties
    {
        [Header(Ray Marching Settings)]
        _MaxSteps ("Max Steps", Range(16, 128)) = 64
        _MaxDistance ("Max Distance", Range(10, 200)) = 100
        _SurfaceThreshold ("Surface Threshold", Range(0.1, 2.0)) = 1.0
        _StepSize ("Step Size", Range(0.01, 0.5)) = 0.1

        [Header(Visual Properties)]
        _Color ("Metaball Color", Color) = (0.5, 0.8, 1.0, 1.0)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.8
        _Metallic ("Metallic", Range(0, 1)) = 0.1

        [Header(Lighting)]
        _LightDirection ("Light Direction", Vector) = (1, 1, 1, 0)
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.2
        _DiffuseStrength ("Diffuse Strength", Range(0, 2)) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"
        }
        LOD 100
        Blend Off
        ZWrite On
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            // URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Properties
            CBUFFER_START(UnityPerMaterial)
            int _MaxSteps;
            float _MaxDistance;
            float _SurfaceThreshold;
            float _StepSize;
            half4 _Color;
            float _Smoothness;
            float _Metallic;
            float4 _LightDirection;
            float _AmbientStrength;
            float _DiffuseStrength;
            CBUFFER_END

            // Field data from MetaballManager (global shader properties)
            float4 _FieldPositions[32];
            float _FieldStrengths[32];
            float _FieldRadii[32];
            float _FieldTypes[32];
            float _FieldNegatives[32];
            int _FieldCount;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 rayDir : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;

                // Calculate ray direction for ray marching
                float3 viewVector = mul(unity_CameraInvProjection, float4(input.uv * 2 - 1, 0, - 1)).xyz;
                output.rayDir = mul(unity_CameraToWorld, float4(viewVector, 0)).xyz;

                return output;
            }

            float CalculateFieldStrength(float distance, float strength, float radius, int fieldType, float isNegative)
            {
                float normalizedDistance = distance / radius;
                float falloff = 0.0;

                if (fieldType == 0) // Polynomial
                {
                    falloff = max(0.0, 1.0 - normalizedDistance * normalizedDistance * normalizedDistance);
                }
                else if (fieldType == 1) // Gaussian
                {
                    falloff = exp(- normalizedDistance * normalizedDistance * 5.0);
                }
                else if (fieldType == 2) // Inverse Square
                {
                    distance = max(distance, 0.01); // Prevent division by zero
                    falloff = 1.0 / (distance * distance + 0.1);
                    falloff *= radius * radius; // Scale by radius squared
                }

                float result = strength * falloff;
                if (isNegative > 0.5)
                {
                    result = - result; // Negate if negative field
                }
                return result;
            }

            float SampleMetaballField(float3 worldPos)
            {
                float totalStrength = 0.0;
                for (int i = 0; i < _FieldCount; i ++)
                {
                    float3 fieldPos = _FieldPositions[i].xyz;
                    float strength = _FieldStrengths[i];
                    float radius = _FieldRadii[i];
                    int fieldType = int(_FieldTypes[i]);
                    float isNegative = _FieldNegatives[i];

                    float distance = length(worldPos - fieldPos);
                    if (distance > radius) continue; // Skip if outside radius

                    float fieldStrength = CalculateFieldStrength(distance, strength, radius, fieldType, isNegative);
                    totalStrength += fieldStrength;
                }
                return totalStrength;
            }

            float3 CalculateSurfaceNormal(float3 worldPos)
            {
                float epsilon = 0.01;
                // Sample field at slightly offset positions
                float fieldCenter = SampleMetaballField(worldPos);
                float fieldX = SampleMetaballField(worldPos + float3(epsilon, 0, 0));
                float fieldY = SampleMetaballField(worldPos + float3(0, epsilon, 0));
                float fieldZ = SampleMetaballField(worldPos + float3(0, 0, epsilon));

                float3 gradient = float3(
                fieldX - fieldCenter,
                fieldY - fieldCenter,
                fieldZ - fieldCenter
                );

                return normalize(gradient);
            }

            struct RayHit
            {
                bool hit;
                float3 position;
                float distance;
            };

            RayHit MarchRay(float3 rayOrigin, float3 rayDirection)
            {
                RayHit result;
                result.hit = false;
                result.position = float3(0, 0, 0);
                result.distance = 0.0;

                float3 currentPos = rayOrigin;
                float totalDistance = 0.0;

                for (int step = 0; step < _MaxSteps; step ++)
                {
                    float fieldValue = SampleMetaballField(currentPos);
                    if (fieldValue > _SurfaceThreshold)
                    {
                        result.hit = true;
                        result.position = currentPos;
                        result.distance = totalDistance;
                        return result; // Hit detected
                    }
                    // Move along the ray
                    currentPos += rayDirection * _StepSize;
                    totalDistance += _StepSize;
                    if (totalDistance > _MaxDistance) break; // Stop if max distance exceeded
                }

                result.distance = totalDistance;
                return result;
            }

            half4 CalculateLighting(RayHit rayHit, float3 viewDir)
            {
                float3 worldPos = rayHit.position;
                float3 normal = CalculateSurfaceNormal(worldPos);

                // Get main light data
                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                float3 lightColor = mainLight.color;

                // Basic Lambertian diffuse lighting
                float lightIntensity = max(0.0, dot(normal, lightDir));

                // Blinn - Phong specular
                float3 halfVector = normalize(lightDir - viewDir);
                float specular = pow(max(0.0, dot(normal, halfVector)), 32.0 * _Smoothness);

                // Combine lighting components
                float3 ambient = _Color.rgb * _AmbientStrength;
                float3 diffuse = _Color.rgb * lightIntensity * _DiffuseStrength * lightColor;
                float3 specularColor = lightColor * specular * _Metallic;

                float3 finalColor = ambient + diffuse + specularColor;

                return half4(finalColor, _Color.a);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 rayOrigin = GetCameraPositionWS();
                float3 rayDirection = normalize(input.positionWS- rayOrigin);

                // Debug : Output ray direction as color
                half3 dirColor = 0.5 + 0.5 * normalize(input.rayDir);
                return half4(dirColor, 1);

                // Debug : Output field value at ray origin
                float field = SampleMetaballField(rayOrigin);
                return half4(field, field, field, 1);

                RayHit marchResult = MarchRay(rayOrigin, rayDirection);

                if (marchResult.hit)
                {
                    // Calculate lighting and color at hit point
                    half4 finalColor = CalculateLighting(marchResult, normalize(rayOrigin - marchResult.position));
                    return finalColor;
                }
                else
                {
                    // Return transparent color if no hit
                    return half4(1, 0, 1, 1);
                }
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Unlit"
}