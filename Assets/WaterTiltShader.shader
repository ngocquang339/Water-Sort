Shader "Custom/WaterSortMultiLayer"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _TiltAngle ("Tilt Angle (Radians)", Float) = 0
        _AspectRatio ("Aspect Ratio", Float) = 0.3
        _Curve ("Water Curve", Range(-0.5, 0.5)) = 0.1 

        _Color1 ("Color 1", Color) = (0,0,0,0)
        _Fill1 ("Fill 1", Range(0, 1)) = 0

        _Color2 ("Color 2", Color) = (0,0,0,0)
        _Fill2 ("Fill 2", Range(0, 1)) = 0

        _Color3 ("Color 3", Color) = (0,0,0,0)
        _Fill3 ("Fill 3", Range(0, 1)) = 0

        _Color4 ("Color 4", Color) = (0,0,0,0)
        _Fill4 ("Fill 4", Range(0, 1)) = 0

        _Color5 ("Color 5", Color) = (0,0,0,0)
        _Fill5 ("Fill 5", Range(0, 1)) = 0

        _Color6 ("Color 6", Color) = (0,0,0,0)
        _Fill6 ("Fill 6", Range(0, 1)) = 0

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            float _TiltAngle;
            float _AspectRatio;
            float _Curve;

            fixed4 _Color1, _Color2, _Color3, _Color4, _Color5, _Color6;
            float _Fill1, _Fill2, _Fill3, _Fill4, _Fill5, _Fill6;
            sampler2D _MainTex;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 centeredUV = IN.texcoord - float2(0.5, 0.5);
                float flatY = centeredUV.y + 0.5;

                // 1. TÌM LỚP NƯỚC CAO NHẤT HIỆN TẠI
                float topFill = 0;
                fixed4 topColor = fixed4(0,0,0,0);
                if (_Fill1 > 0.001) { topFill = _Fill1; topColor = _Color1; }
                if (_Fill2 > 0.001) { topFill = _Fill2; topColor = _Color2; }
                if (_Fill3 > 0.001) { topFill = _Fill3; topColor = _Color3; }
                if (_Fill4 > 0.001) { topFill = _Fill4; topColor = _Color4; }
                if (_Fill5 > 0.001) { topFill = _Fill5; topColor = _Color5; }
                if (_Fill6 > 0.001) { topFill = _Fill6; topColor = _Color6; }

                if (topFill <= 0.001) clip(-1);

                // 2. TOÁN HỌC MỚI: TÍNH GÓC VÁT CHUẨN VẬT LÝ BẰNG VECTOR PHÁP TUYẾN
                float s, c;
                sincos(_TiltAngle, s, c);
                
                // Chuẩn hóa vector để không bị bóp méo khi nhân với AspectRatio
                float2 normal = normalize(float2(s * _AspectRatio, c));
                
                // Tính khoảng cách từ tâm đến điểm hiện tại theo hướng nghiêng
                float dist = dot(centeredUV, normal);
                
                // Ngưỡng cắt vát động dựa trên lượng nước
                float threshold = topFill - 0.5;
                float curveOffset = (centeredUV.x * centeredUV.x) * _Curve;

                // CẮT VÁT BỀ MẶT TRÊN CÙNG
                if (dist > threshold - curveOffset) clip(-1);

                // 3. TÔ MÀU CÁC KHỐI BÊN DƯỚI (Phẳng)
                fixed4 outColor = fixed4(0,0,0,0);
                if (flatY <= _Fill1) outColor = _Color1;
                else if (flatY <= _Fill2) outColor = _Color2;
                else if (flatY <= _Fill3) outColor = _Color3;
                else if (flatY <= _Fill4) outColor = _Color4;
                else if (flatY <= _Fill5) outColor = _Color5;
                else if (flatY <= _Fill6) outColor = _Color6;
                else outColor = topColor; // Đỉnh vát nhọn lấy màu của lớp cao nhất

                // 4. ÁP DỤNG MẶT NẠ CHAI GỐC
                fixed4 texC = tex2D(_MainTex, IN.texcoord);
                outColor.a *= texC.a * IN.color.a;
                if (outColor.a == 0) clip(-1);

                outColor.rgb *= outColor.a;
                return outColor;
            }
            ENDCG
        }
    }
}