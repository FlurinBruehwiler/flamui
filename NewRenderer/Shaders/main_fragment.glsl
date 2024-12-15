#version 330 core

in vec2 frag_texCoords;
in float fill_bezier_type;
in vec4 frag_color;

out vec4 out_color;

uniform int stencil_enabled; //0 = disabled, 1 = enabled

void main()
{
    if(fill_bezier_type == 0){
        out_color = frag_color;
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