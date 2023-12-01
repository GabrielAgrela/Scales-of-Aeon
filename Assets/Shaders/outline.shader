// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "OutlineShader" 
 {
    Properties {
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Float) = 0.1
    }
    SubShader {
        Tags { "Queue" = "Geometry+1" }
        // Render the outline
        Pass {
            ZWrite On
            ColorMask 0
            Stencil {
                Ref 1
                Comp always
                Pass replace
            }
            CGPROGRAM
            #pragma vertex vert
            #include "UnityCG.cginc"

            float _OutlineWidth;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                // Inflate the vertex positions
                float3 inflated = v.normal * _OutlineWidth;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                worldPos.xyz += inflated;
                o.pos = UnityObjectToClipPos(worldPos);
                return o;
            }

            ENDCG
        }
        // Render the object normally but only where the stencil is not equal to 1
        Pass {
            Stencil {
                Ref 1
                Comp notequal
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag() : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}