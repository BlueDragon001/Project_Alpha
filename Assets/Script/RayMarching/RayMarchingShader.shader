Shader "Custom/RayMarchingShader"
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
        Tags{"RenderType" = "Transparent" "Queue" = "Transparent"}
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            // Properties
            int _MaxSteps;
            float _MaxDistance;
            float _SurfaceThreshold;
            float _StepSize;
            fixed4 _Color;
            float _Smoothness;
            float _Metallic;
            float4 _LightDirection;
            float _AmbientStrength;
            float _DiffuseStrength;

            // Field data from MetaballManager (global shader properties)
            float4 _FieldPositions[32];
            float _FieldStrengths[32];
            float _FieldRadii[32];
            float _FieldTypes[32];
            float _FieldNegatives[32];
            int _FieldCount;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 rayDir : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                // Calculate world position and ray direction
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.rayDir = normalize(o.worldPos - _WorldSpaceCameraPos.xyz);

                return o;
            }

            float CalculateFieldStrength(float distance, float strength, float radius, int fieldType, float isNegative)
            {
                float normalizedDistance = distance / radius;
                float falloff = 0.0;
                if (fieldType == 0) //Polymomial
                {
                    falloff = max(0.0, 1.0 - normalizedDistance * normalizedDistance * normalizedDistance);

                }else if (fieldType == 1) //Gaussian
                {
                    falloff = exp(- normalizedDistance * normalizedDistance * 5.0);
                }
                else if (fieldType == 2) //Inverse Square
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
                    if(distance > radius) continue; // Skip if outside radius
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

            struct RayHit{
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

            fixed4 CalculateLighting(RayHit rayHit, float3 viewDir)
            {
                float3 worldPos = rayHit.position;
                float3 normal = CalculateSurfaceNormal(worldPos);
                float3 lightDir = normalize(_LightDirection.xyz);
                float lightIntensity = max(0.0, dot(normal, lightDir)); // Basic Lambertian diffuse lighting
                float3 halfVector = normalize(lightDir - viewDir);
                float specular = pow(max(0.0, dot(normal, halfVector)), 32.0 * _Smoothness); // Blinn Phong specular
                // Combine lighting components
                float3 ambient = _Color.rgb * _AmbientStrength;
                float3 diffuse = _Color.rgb * lightIntensity * _DiffuseStrength;
                float3 specularColor = float3(1, 1, 1) * specular * _Metallic;

                float3 finalColor = ambient + diffuse + specularColor;

                return fixed4(finalColor, _Color.a);


            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 rayOrigin = _WorldSpaceCameraPos;
                float rayDirection = normalize(i.rayDir);
                RayHit marchResult = MarchRay(rayOrigin, rayDirection);
                if (marchResult.hit)
                {
                    // Calculate lighting and color at hit point
                    fixed4 finalColor = CalculateLighting(marchResult, normalize(rayOrigin - marchResult.position));
                    return finalColor;
                }
                else
                {
                    // Return transparent color if no hit
                    return fixed4(0, 0, 0, 0);
                }
            }
            ENDCG

        }
    }

    Fallback "Diffuse"
}
