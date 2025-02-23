#version 330 core

in vec2 frag_texCoords;
in float fill_bezier_type;
in vec4 frag_color;
in float texture_type; // 0 == color, 1 == texture, 2 == font
in float texture_id;

uniform int stencil_enabled; //0 = disabled, 1 = enabled
uniform sampler2D uTextures[10]; //maximum of 10 textures


out vec4 out_color;

void main()
{
    if(fill_bezier_type == 0){
        if (texture_type == 0){
            out_color = frag_color;
        }else if(texture_type == 1){
            out_color = texture(uTextures[int(texture_id)], frag_texCoords) * frag_color;
        }else if(texture_type == 2){
            out_color = texture(uTextures[int(texture_id)], frag_texCoords).r * frag_color;
           // out_color = vec4(1, 0, 0, 1);
        }
    }else{
        float x = frag_texCoords.x;
        float y = frag_texCoords.y;

        //anti aliasing: some magic stuff i don't get from this video: https://dl.acm.org/doi/10.1145/1073204.1073303
        float f = x*x-y;
        float dx = dFdx(f);
        float dy = dFdy(f);
        float sd = f/sqrt(dx*dx+dy*dy);

        float opacity = 0;

        if(sd < -1)
            opacity = 0;
        else if (sd > 1)
            opacity = 1;
        else
            opacity = (1 + sd) / 2;

        if(fill_bezier_type > 0){
            opacity = 1 - opacity;
        }

        if(stencil_enabled == 1 && opacity == 0){
            discard;
        }

        out_color = vec4(frag_color.r, frag_color.g, frag_color.b, frag_color.a * opacity);
    }
}