// Normal Mapping for a Triplanar Shader - Ben Golus 2017
// Unity Surface Shader example shader

// Implements correct triplanar normals in a Surface Shader with out computing or passing additional information from the
// vertex shader.

Shader "MyTriplanarShaderSplatter" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _TexAmtX ("Amount Textures X", Range(1, 64)) = 1
        _TexAmtY ("Amount Textures Y", Range(1, 64)) = 1
        _MainTex2 ("Do not use", 2D) = "white" {}
        _MainTex3 ("Do not use", 2D) = "white" {}
        _MainTex4 ("Do not use", 2D) = "white" {}
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _OcclusionMap("Occlusion", 2D) = "white" {}
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #include "UnityStandardUtils.cginc"

        // flip UVs horizontally to correct for back side projection
        #define TRIPLANAR_CORRECT_PROJECTED_U

        // offset UVs to prevent obvious mirroring
        // #define TRIPLANAR_UV_OFFSET

        // Reoriented Normal Mapping
        // http://blog.selfshadow.com/publications/blending-in-detail/
        // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
        half3 blend_rnm(half3 n1, half3 n2)
        {
            n1.z += 1;
            n2.xy = -n2.xy;

            return n1 * dot(n1, n2) / n1.z - n2;
        }

        sampler2D _MainTex;
        // float4 _MainTex_ST;

        sampler2D _BumpMap;
        sampler2D _OcclusionMap;

        half _Glossiness;
        half _Metallic;

        fixed _TexAmtX;
        fixed _TexAmtY;
        
        half _OcclusionStrength;

        struct Input {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_MainTex : TEXCOORD0;
            float2 uv2_MainTex2 : TEXCOORD1;
            float2 uv3_MainTex3 : TEXCOORD2;
            float2 uv4_MainTex4 : TEXCOORD3;
            INTERNAL_DATA
        };

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
            float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
            float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // work around bug where IN.worldNormal is always (0,0,0)!
            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

            // calculate triplanar blend
            half3 triblend = saturate(pow(IN.worldNormal, 4));
            triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

            // calculate triplanar uvs
            // applying texture scale and offset values ala TRANSFORM_TEX macro
            // float2 uvX = IN.worldPos.zy * _MainTex_ST.xy + _MainTex_ST.zy;
            // float2 uvY = IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zy;
            // float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy + _MainTex_ST.zy;

            float2 uvX1 = abs(IN.worldPos.zy % 1);
            float2 uvY1 = abs(IN.worldPos.xz % 1);
            float2 uvZ1 = abs(IN.worldPos.xy % 1);

            
            float2 TexRatio;
            TexRatio[0] = 1/floor(_TexAmtX);
            TexRatio[1] = 1/floor(_TexAmtY);
            

            // float2

            

            // offset UVs to prevent obvious mirroring
        #if defined(TRIPLANAR_UV_OFFSET)
            uvY1 += 0.33;
            uvZ1 += 0.67;
        #endif

            // minor optimization of sign(). prevents return value of 0
            half3 axisSign = IN.worldNormal < 0 ? -1 : 1;
            
            // flip UVs horizontally to correct for back side projection
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            uvX1.x *= axisSign.x;
            uvY1.x *= axisSign.y;
            uvZ1.x *= -axisSign.z;
        #endif

            float2 uvX2,uvX3,uvX4,uvY2,uvY3,uvY4,uvZ2,uvZ3,uvZ4;

            uvX2 = uvX1;
            uvY2 = uvY1;
            uvZ2 = uvZ1;

            uvX2[0] = uvX2[0]*TexRatio[0]+IN.uv2_MainTex2[0];
            uvY2[0] = uvY2[0]*TexRatio[0]+IN.uv2_MainTex2[0];
            uvZ2[0] = uvZ2[0]*TexRatio[0]+IN.uv2_MainTex2[0];

            uvX2[1] = uvX2[1]*TexRatio[1]+IN.uv2_MainTex2[1];
            uvY2[1] = uvY2[1]*TexRatio[1]+IN.uv2_MainTex2[1];
            uvZ2[1] = uvZ2[1]*TexRatio[1]+IN.uv2_MainTex2[1];

            uvX3 = uvX1;
            uvY3 = uvY1;
            uvZ3 = uvZ1;

            uvX3[0] = uvX3[0]*TexRatio[0]+IN.uv3_MainTex3[0];
            uvY3[0] = uvY3[0]*TexRatio[0]+IN.uv3_MainTex3[0];
            uvZ3[0] = uvZ3[0]*TexRatio[0]+IN.uv3_MainTex3[0];

            uvX3[1] = uvX3[1]*TexRatio[1]+IN.uv3_MainTex3[1];
            uvY3[1] = uvY3[1]*TexRatio[1]+IN.uv3_MainTex3[1];
            uvZ3[1] = uvZ3[1]*TexRatio[1]+IN.uv3_MainTex3[1];

            uvX4 = uvX1;
            uvY4 = uvY1;
            uvZ4 = uvZ1;

            uvX4[0] = uvX4[0]*TexRatio[0]+IN.uv4_MainTex4[0];
            uvY4[0] = uvY4[0]*TexRatio[0]+IN.uv4_MainTex4[0];
            uvZ4[0] = uvZ4[0]*TexRatio[0]+IN.uv4_MainTex4[0];

            uvX4[1] = uvX4[1]*TexRatio[1]+IN.uv4_MainTex4[1];
            uvY4[1] = uvY4[1]*TexRatio[1]+IN.uv4_MainTex4[1];
            uvZ4[1] = uvZ4[1]*TexRatio[1]+IN.uv4_MainTex4[1];

            uvX1[0] = uvX1[0]*TexRatio[0]+IN.uv_MainTex[0];
            uvY1[0] = uvY1[0]*TexRatio[0]+IN.uv_MainTex[0];
            uvZ1[0] = uvZ1[0]*TexRatio[0]+IN.uv_MainTex[0];

            uvX1[1] = uvX1[1]*TexRatio[1]+IN.uv_MainTex[1];
            uvY1[1] = uvY1[1]*TexRatio[1]+IN.uv_MainTex[1];
            uvZ1[1] = uvZ1[1]*TexRatio[1]+IN.uv_MainTex[1];

            // albedo textures
            fixed4 colX1 = tex2D(_MainTex, uvX1);
            fixed4 colY1 = tex2D(_MainTex, uvY1);
            fixed4 colZ1 = tex2D(_MainTex, uvZ1);
            fixed4 col1 = colX1 * triblend.x + colY1 * triblend.y + colZ1 * triblend.z;

            fixed4 colX2 = tex2D(_MainTex, uvX2);
            fixed4 colY2 = tex2D(_MainTex, uvY2);
            fixed4 colZ2 = tex2D(_MainTex, uvZ2);
            fixed4 col2 = colX2 * triblend.x + colY2 * triblend.y + colZ2 * triblend.z;

            fixed4 colX3 = tex2D(_MainTex, uvX3);
            fixed4 colY3 = tex2D(_MainTex, uvY3);
            fixed4 colZ3 = tex2D(_MainTex, uvZ3);
            fixed4 col3 = colX3 * triblend.x + colY3 * triblend.y + colZ3 * triblend.z;

            fixed4 colX4 = tex2D(_MainTex, uvX4);
            fixed4 colY4 = tex2D(_MainTex, uvY4);
            fixed4 colZ4 = tex2D(_MainTex, uvZ4);
            fixed4 col4 = colX4 * triblend.x + colY4 * triblend.y + colZ4 * triblend.z;

            fixed4 col = col1*0.25 + col2*0.25 + col3*0.25 + col4*0.25;



            // occlusion textures
            half occX = tex2D(_OcclusionMap, uvX1).g;
            half occY = tex2D(_OcclusionMap, uvY1).g;
            half occZ = tex2D(_OcclusionMap, uvZ1).g;
            half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _OcclusionStrength);
            

            // tangent space normal maps
            half3 tnormalX = UnpackNormal(tex2D(_BumpMap, uvX1));
            half3 tnormalY = UnpackNormal(tex2D(_BumpMap, uvY1));
            half3 tnormalZ = UnpackNormal(tex2D(_BumpMap, uvZ1));

            // flip normal maps' x axis to account for flipped UVs
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            tnormalX.x *= axisSign.x;
            tnormalY.x *= axisSign.y;
            tnormalZ.x *= -axisSign.z;
        #endif

            half3 absVertNormal = abs(IN.worldNormal);

            // swizzle world normals to match tangent space and apply reoriented normal mapping blend
            tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
            tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
            tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

            // apply world space sign to tangent space Z
            tnormalX.z *= axisSign.x;
            tnormalY.z *= axisSign.y;
            tnormalZ.z *= axisSign.z;

            // sizzle tangent normals to match world normal and blend together
            half3 worldNormal = normalize(
                tnormalX.zyx * triblend.x +
                tnormalY.xzy * triblend.y +
                tnormalZ.xyz * triblend.z
                );

            // set surface ouput properties
            o.Albedo = col.rgb;
            // o.Albedo = float4(uvX2[0],0,0,1);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Occlusion = occ;

            // convert world space normals into tangent normals
            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
        }
        ENDCG
    }
    FallBack "Diffuse"
}