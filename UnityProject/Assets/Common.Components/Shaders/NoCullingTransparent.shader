Shader "NoCullingTransparent" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range (0,1)) = 0.0
	}
	
	SubShader {
		Tags {"Queue"="Transparent"}
	
		// Common to all passes
		Color [_Color]
		Material {
			Diffuse [_Color]
			Ambient [_Color]
		}
		Lighting Off
		
		// set up alpha blending
		Blend SrcAlpha OneMinusSrcAlpha
		
		// Render everything
		Pass {
			AlphaTest Greater [_Cutoff]
			Cull Off
			SetTexture [_MainTex] {
				Combine Primary * Texture
			}
		}
	}
}
