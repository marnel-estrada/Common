 Shader "Instanced/ComputeBufferSpriteTRS" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    
    SubShader {
        Tags{
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }
        
        ZWrite Off 
		Lighting Off 
		Cull Back
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

            // Transform here contains xy for position and zw for scale
            StructuredBuffer<float4> transformBuffer;
            StructuredBuffer<float> rotationBuffer;
            
            // xy is the size, zw is the anchor
            StructuredBuffer<float4> sizePivotBuffer;
             
            StructuredBuffer<float4> uvBuffer;
			StructuredBuffer<float4> colorsBuffer;
			
            struct v2f{
                float4 pos : SV_POSITION;
                float2 uv: TEXCOORD0;
				fixed4 color : COLOR0;
            };
            
            float4x4 scaleMatrix(float2 s) {
                return float4x4(
                    s.x, 0,   0, 0,
                    0,   s.y, 0, 0,
                    0,   0,   1, 0,
                    0,   0,   0, 1
                );
            }

            float4x4 rotationMatrix(float radians){
                float c = cos(radians);
                float s = sin(radians);
                return float4x4( 
                    c,  s, 0, 0,
                    -s, c, 0, 0,
                    0,  0, 1, 0,
                    0,  0, 0, 1);
            }

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID){
                float4 transform = transformBuffer[instanceID];
                float4 uv = uvBuffer[instanceID];
                
                // transform the vertex
                float2 size = sizePivotBuffer[instanceID].xy;
                float4x4 scaleAndRotate = mul(scaleMatrix(size * transform.zw), rotationMatrix(rotationBuffer[instanceID]));
                
                // Apply anchor
                float2 anchor = sizePivotBuffer[instanceID].zw;
                v.vertex = mul(v.vertex - float4(anchor, 0, 0), scaleAndRotate);
                
                // scale it using the sprite size and the transform scale which is transform.zw
                float3 worldPosition = float3(transform.x, transform.y, -transform.y/10) + v.vertex.xyz;
                
                v2f o;
                o.pos = UnityObjectToClipPos(float4(worldPosition, 1.0f));
                
                // XY here is the dimension (width, height). 
                // ZW is the offset in the texture (the actual UV coordinates)
                o.uv =  v.texcoord * uv.xy + uv.zw;
                
				o.color = colorsBuffer[instanceID];
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				clip(col.a - 1.0 / 255.0);
                col.rgb *= col.a;

				return col;
            }

            ENDCG
        }
    }
}