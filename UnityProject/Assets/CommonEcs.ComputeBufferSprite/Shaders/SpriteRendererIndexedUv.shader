 Shader "Instanced/SpriteRendererIndexedUv" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    
    SubShader {
        Tags{
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
        }
        Cull Back
        Lighting Off
        ZWrite On
        AlphaTest Greater 0
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
            #pragma exclude_renderers gles

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed _Cutoff;

            // xyz is the position, w is the rotation
            StructuredBuffer<float4> translationAndRotationBuffer;
            StructuredBuffer<float4> rotationBuffer;
            StructuredBuffer<float> scaleBuffer;

            StructuredBuffer<float2> sizeBuffer; // Size of each sprite
            StructuredBuffer<float2> pivotBuffer; // Pivot of each sprite
            
			StructuredBuffer<float4> colorsBuffer;

            // Note here that uvBuffer is only the available UV coordinates
            // An int value from uvIndexBuffer would then index the uvBuffer
            StructuredBuffer<float4> uvBuffer;
            StructuredBuffer<int> uvIndexBuffer;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv: TEXCOORD0;
				fixed4 color : COLOR0;
            };

            float4x4 rotationZMatrix(float zRotRadians){
                float c = cos(zRotRadians);
                float s = sin(zRotRadians);
                float4x4 ZMatrix  = 
                    float4x4( 
                       c,  s, 0,  0,
                       -s, c, 0,  0,
                       0,  0, 1,  0,
                       0,  0, 0,  1);
                return ZMatrix;
            }

            float4x4 quaternionToMatrix(float4 quat)
            {
                float4x4 m = float4x4(float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0));

                float x = quat.x, y = quat.y, z = quat.z, w = quat.w;
                float x2 = x + x, y2 = y + y, z2 = z + z;
                float xx = x * x2, xy = x * y2, xz = x * z2;
                float yy = y * y2, yz = y * z2, zz = z * z2;
                float wx = w * x2, wy = w * y2, wz = w * z2;

                m[0][0] = 1.0 - (yy + zz);
                m[0][1] = xy - wz;
                m[0][2] = xz + wy;

                m[1][0] = xy + wz;
                m[1][1] = 1.0 - (xx + zz);
                m[1][2] = yz - wx;

                m[2][0] = xz - wy;
                m[2][1] = yz + wx;
                m[2][2] = 1.0 - (xx + yy);

                m[3][3] = 1.0;

                return m;
            }

            v2f vert(appdata_full v, uint instanceID : SV_InstanceID) {
                // pivot
                float2 pivot = pivotBuffer[instanceID];
                v.vertex = v.vertex - float4(pivot, 0, 0);
                
                // size
                float2 size = sizeBuffer[instanceID];
                v.vertex.x = v.vertex.x * size.x;
                v.vertex.y = v.vertex.y * size.y;
                
                // rotate the vertex (rotate at center)
                float4 quaternion = rotationBuffer[instanceID];
                v.vertex = mul(v.vertex, quaternionToMatrix(quaternion));
                
                // scale it
                float scale = scaleBuffer[instanceID];
                float4 translationAndRot = translationAndRotationBuffer[instanceID];
                float3 worldPosition = translationAndRot.xyz + (v.vertex.xyz * scale);
                
                v2f o;
                o.pos = UnityObjectToClipPos(float4(worldPosition, 1.0f));
                
                // XY here is the dimension (width, height). 
                // ZW is the offset in the texture (the actual UV coordinates)
                int uvIndex = uvIndexBuffer[instanceID];
                float4 uv = uvBuffer[uvIndex];
                o.uv =  v.texcoord * uv.xy + uv.zw;
                
				o.color = colorsBuffer[instanceID];
                return o;
            }

            fixed4 frag(v2f i, out float depth : SV_Depth) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                clip(col.a - _Cutoff);
                col.rgb *= col.a;

				return col;
            }

            ENDCG
        }
    }
}