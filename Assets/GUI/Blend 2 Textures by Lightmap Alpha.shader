// http://wiki.unity3d.com/index.php/Blend_2_Textures_by_Lightmap_Alpha
Shader "Blend 2 Textures by Lightmap Alpha" {

Properties {
    _MainTex ("Texture 1 (RGB)", 2D) = ""
    _Tex2 ("Texture 2 (RGB)", 2D) = ""
    _Lightmap ("Lightmap (A=Blend)", 2D) = ""
}

// iPhone 3GS and later
//SubShader {Pass {
//    GLSLPROGRAM
//    varying mediump vec2 mainTexUV, tex2UV, lightmapUV;
//            
//    #ifdef VERTEX      
//    uniform mediump vec4 _MainTex_ST, _Tex2_ST;
//    void main() {
//        gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
//        mainTexUV = gl_MultiTexCoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
//        tex2UV = gl_MultiTexCoord0.xy * _Tex2_ST.xy + _Tex2_ST.zw;
//        lightmapUV = gl_MultiTexCoord1.xy;    
//    }
//    #endif
//    
//    #ifdef FRAGMENT
//    uniform lowp sampler2D _MainTex, _Tex2, _Lightmap;
//    void main() {
//        lowp vec4 lightmap_mix = texture2D(_Lightmap, lightmapUV);
//        gl_FragColor = lightmap_mix *
//            mix(texture2D(_Tex2, tex2UV), texture2D(_MainTex, mainTexUV), lightmap_mix.a);
//    }
//    #endif  
//    ENDGLSL
//}}

// pre-3GS devices, including the September 2009 8GB iPod touch
SubShader {
    BindChannels {
        Bind "vertex", vertex
        Bind "texcoord", texcoord0  // 1st UV - tiling textures      
        Bind "texcoord1", texcoord1 // 2nd UV - lightmap and blend map
    }
    Pass {      
        SetTexture[_Tex2]
        SetTexture[_Lightmap] {Combine previous * texture}
    }
    Pass {
        Blend SrcAlpha OneMinusSrcAlpha 
        SetTexture[_MainTex]
        // Apply the lightmap to Texture 1 
        // and blend in the result, using the alpha map
        SetTexture[_Lightmap] {Combine previous * texture, texture}
    }
}
 
}