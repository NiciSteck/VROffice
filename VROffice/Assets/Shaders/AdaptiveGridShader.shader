Shader "Adaptive Input/Adaptive Grid Shader"
{
    Properties
    {
       _MainColor("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)
       _SecondaryColor("Secondary Color", Color) = (1.0, 1.0, 1.0, 1.0)
       _BackgroundColor("Background Color", Color) = (0.0, 0.0, 0.0, 0.0)
       _ColorInterp("Color Interpolation Value", Range(0., 1.)) = 0.

       [Header(Grid)]
       _Scale("Scale", Range(1,100)) = 1.0
       _GraduationScale("Graduation Scale", Float) = 1.0
       _Thickness("Lines Thickness", Range(0.0001, 0.01)) = 0.005
       _SecondaryFadeInSpeed("Secondary Fade In Speed", Range(0.1, 4)) = 0.5
    }
        SubShader
       {
          Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
          LOD 100

          ZWrite On // We need to write in depth to avoid tearing issues
          Blend SrcAlpha OneMinusSrcAlpha

          Pass
          {
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
             #include "UnityCG.cginc"

             struct appdata
             {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
             };

             struct v2f
             {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
             };

             sampler2D _GridTexture;
             float4 _GridTexture_ST;

             sampler2D _MaskTexture;
             float4 _MaskTexture_ST;

             float _Scale;
             float _GraduationScale;
             float _Thickness;
             float _SecondaryFadeInSpeed;

             fixed4 _MainColor;
             fixed4 _SecondaryColor;
             fixed4 _BackgroundColor;
             float _ColorInterp;

             v2f vert(appdata v)
             {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Remap UVs from [0:1] to [-0.5:0.5] to make scaling effect start from the center 
                o.uv = v.uv; // -0.5f;
                // Scale the whole thing if necessary

                // UVs for mask texture
                o.uv1 = TRANSFORM_TEX(v.uv1, _MaskTexture);
                return o;
             }

             // Remap value from a range to another
             float remap(float value, float from1, float to1, float from2, float to2) {
                return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
             }

             fixed4 frag(v2f i) : SV_Target
             {
                fixed4 col = _BackgroundColor;
                col.w = 0; 
                fixed4 gridCol = (1 - _ColorInterp) * _MainColor + _ColorInterp * _SecondaryColor;
                
                float scale = _GraduationScale * _Scale;
                if (scale >= 1) {
                    scale = log2(scale);
                    float thicknessScale = 1 / max(1, scale);

                    // Secondary grid lines
                    float2 secondary = { 0,0 };

                    float divisions = 2 * max(pow(2, floor(scale)), 1);
                    float width = 1.;
                    if (divisions > 0)
                        width /= divisions;
                    for (float division = 0; division < 1; division += width) {
                        secondary.x = max(secondary.x, 1 - step(thicknessScale * _Thickness, abs(i.uv.x - division)));
                        secondary.y = max(secondary.y, 1 - step(thicknessScale * _Thickness, abs(i.uv.y - division)));
                    }
                    if (secondary.x == 1 || secondary.y == 1) {
                        float fade = frac(scale);
                        col = gridCol;
                        col.w = fade * col.w;
                    }

                    // Main grid lines 
                    float2 main = { 0,0 };
                    main.x = max(main.x, 1 - step(thicknessScale * _Thickness, abs(i.uv.x - 1)));
                    main.y = max(main.y, 1 - step(thicknessScale * _Thickness, abs(i.uv.y - 1)));
                    main.x = max(main.x, 1 - step(thicknessScale * _Thickness, abs(i.uv.x)));
                    main.y = max(main.y, 1 - step(thicknessScale * _Thickness, abs(i.uv.y)));

                    divisions = max(pow(2, floor(scale)), 1);
                    width = 1.;
                    if (divisions > 0) {
                        width /= divisions;
                    }
                    for (float division = 0; division < 1; division += width) {
                        main.x = max(main.x, 1 - step(thicknessScale * _Thickness, abs(i.uv.x - division)));
                        main.y = max(main.y, 1 - step(thicknessScale * _Thickness, abs(i.uv.y - division)));
                    }

                    if (main.x == 1 || main.y == 1) {
                        col = gridCol;
                    }
                }
                else {
                    float2 main = { 0,0 };
                    main.x = max(main.x, 1 - step(_Thickness, abs(i.uv.x - 1)));
                    main.y = max(main.y, 1 - step(_Thickness, abs(i.uv.y - 1)));
                    main.x = max(main.x, 1 - step(_Thickness, abs(i.uv.x)));
                    main.y = max(main.y, 1 - step(_Thickness, abs(i.uv.y)));
                    if (main.x == 1 || main.y == 1) {
                        col = gridCol;
                        col.w = _Scale;
                    }
                }

                 return col;
             }
            ENDCG
        }
    }
}