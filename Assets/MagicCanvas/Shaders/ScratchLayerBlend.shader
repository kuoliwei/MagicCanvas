Shader "Custom/ScratchLayerBlend"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BrushTex ("Brush Texture", 2D) = "white" {}                 // [�s�W] ����K�ϡ]��Ρ^
        _BrushPos ("Brush Pos & Size (x, y, width, height)", Vector) = (0.5, 0.5, 0.1, 0.1)
        _AlphaDecayFactor ("Alpha Decay Factor", Range(0, 1)) = 0.1  // [�s�W] ����z���פU���{��
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend One Zero   // [�T�O�g�J RenderTexture�A�����V�X]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BrushTex;           // [�s�W]
            float4 _BrushPos;              // xy = ��m, zw = �ؤo
            float _AlphaDecayFactor;       // [�s�W]

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 col = tex2D(_MainTex, uv);

                float2 halfSize = _BrushPos.zw * 0.5;

                // [�s�W] �p�ⵧ��ϰ������ UV �y��
                float2 brushUV = (uv - (_BrushPos.xy - halfSize)) / _BrushPos.zw;

                // [�s�W] �Y���b����d�򤺴N���L
                if (brushUV.x < 0 || brushUV.x > 1 || brushUV.y < 0 || brushUV.y > 1)
                    return col;

                // [�s�W] �ھڵ���K�� alpha �ӭ��C�z����
                float brushAlpha = tex2D(_BrushTex, brushUV).a;
                col.a = max(0, col.a - brushAlpha * _AlphaDecayFactor);

                // // [�O�d] ���� ��0.01 ����m�e���I
                // float2 delta = abs(uv - _BrushPos.xy);
                // if (delta.x < 0.01 && delta.y < 0.01)
                // {
                //     col.rgb = float3(1, 0, 0);
                // }

                return col;
            }
            ENDCG
        }
    }
}
