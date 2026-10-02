 Shader "Instanced/SpriteRendererIndexedUv" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.1
    }
    
    SubShader {
        Tags{
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
        }
        Cull Off
        Lighting Off
        ZWrite On
        ZTest LEqual
        Fog{ Mode Off }
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

            // Section size of the packed buffers
            // Note that we pack buffers of the same type to reduce the amount of StructuredBuffer
            // It was done this way since on web builds, some environments may only support 8 StructuredBuffers
            int _Capacity;

            // [0, cap) translationAndScale (xyz position, w scale), [cap, cap * 2) rotation, [cap * 2, cap * 3) color
            StructuredBuffer<float4> float4Buffer;

            // [0, cap) size, [cap, cap * 2) pivot
            StructuredBuffer<float2> float2Buffer;

            // [0, cap) sortedIndices, [cap, cap * 2) active (1 active, 0 inactive), [cap * 2, cap * 3) layerOrder
            StructuredBuffer<int> intBuffer;
            
            // Note here that uvBuffer is only the available UV coordinates
            // An int value from uvIndexBuffer would then index the uvBuffer
            StructuredBuffer<float4> uvBuffer;
            StructuredBuffer<int> uvIndexBuffer;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv: TEXCOORD0;
				fixed4 color : COLOR0;
            };

            float4x4 quaternionToMatrix(float4 quat)
            {
                float4x4 m = float4x4(float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0));

                float x = quat.x, y = quat.y, z = quat.z, w = quat.w;
                float x2 = x + x,  y2 = y + y,  z2 = z + z;
                float xx = x * x2, xy = x * y2, xz = x * z2;
                float yy = y * y2, yz = y * z2, zz = z * z2;
                float wx = w * x2, wy = w * y2, wz = w * z2;

                m[0][0] = 1.0 - (yy + zz);
                m[0][1] = xy + wz;
                m[0][2] = xz + wy;

                m[1][0] = xy - wz;
                m[1][1] = 1.0 - (xx + zz);
                m[1][2] = yz - wx;

                m[2][0] = xz - wy;
                m[2][1] = yz + wx;
                m[2][2] = 1.0 - (xx + yy);

                m[3][3] = 1.0;

                return m;
            }

            v2f vert(appdata_full v, uint instanceID : SV_InstanceID) {
                uint sortedIndex = intBuffer[instanceID];
                
                // pivot
                float2 pivot = float2Buffer[_Capacity + sortedIndex];
                v.vertex = v.vertex - float4(pivot, 0, 0);
                
                // size
                float2 size = float2Buffer[sortedIndex];
                v.vertex.x = v.vertex.x * size.x;
                v.vertex.y = v.vertex.y * size.y;
                
                // rotate the vertex (rotate at center)
                float4 quaternion = float4Buffer[_Capacity + sortedIndex];
                v.vertex = mul(v.vertex, quaternionToMatrix(quaternion));
                
                // scale it
                float4 translationAndScale = float4Buffer[sortedIndex];
                float scale = translationAndScale.w;
                float3 worldPosition = translationAndScale.xyz + (v.vertex.xyz * scale);

                // layer order
                // We multiply by negative value here because higher order means to be rendered later
                int layerOrder = intBuffer[_Capacity * 2 + sortedIndex];
                worldPosition.z = worldPosition.z + (layerOrder * -0.001);
                
                v2f o;
                o.pos = UnityObjectToClipPos(float4(worldPosition, 1.0f));
                o.pos = UnityPixelSnap(o.pos);
                
                // XY here is the dimension (width, height). 
                // ZW is the offset in the texture (the actual UV coordinates)
                int uvIndex = uvIndexBuffer[sortedIndex];
                float4 uv = uvBuffer[uvIndex];
                o.uv =  v.texcoord * uv.xy + uv.zw;
                
				o.color = float4Buffer[_Capacity * 2 + sortedIndex];
                o.color.a = o.color.a * intBuffer[_Capacity + sortedIndex];
                return o;
            }

            fixed4 frag(v2f i, out float depth : SV_Depth) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                clip(col.a - _Cutoff);
                //col.a = i.color.a;

				return col;
            }

            ENDCG
        }
    }
}