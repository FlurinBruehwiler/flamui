#version 450 core

in vec2 frag_texCoords;
in float fill_bezier_type;
in vec4 frag_color;
in float texture_type; // 0 == color, 1 == texture, 2 == font
in float texture_id;

uniform int stencil_enabled; //0 = disabled, 1 = enabled
uniform sampler2D uTextures[10]; //maximum of 10 textures
uniform vec2 uViewportSize;

layout(location = 0)
out vec4 out_color;

void main()
{
    float opacity = 1;

    if(fill_bezier_type != 0){
        float x = frag_texCoords.x;
        float y = frag_texCoords.y;

        //anti aliasing: some magic stuff i don't get from this video: https://dl.acm.org/doi/10.1145/1073204.1073303
        float f = x*x-y;
        float dx = dFdx(f);
        float dy = dFdy(f);
        float sd = f/sqrt(dx*dx+dy*dy);

        opacity = 0;

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

        opacity *= frag_color.a;
    }

    if (texture_type == 0){
        out_color = frag_color;
    }else if(texture_type == 1){
        out_color = texture(uTextures[int(texture_id)], frag_texCoords);
    }else if(texture_type == 2){
        float alpha = texture(uTextures[int(texture_id)], frag_texCoords).r;
        out_color = vec4(frag_color.rgb * alpha, alpha);
        // out_color = vec4(1, 0, 0, 1);
    }
    else if(texture_type == 3){
        out_color = texture(uTextures[int(texture_id)], gl_FragCoord.xy / uViewportSize);
        out_color = vec4((frag_color.rgb * frag_color.a) + (out_color.rgb * (1.0 - frag_color.a)), 1.0);
    }

    out_color = out_color * opacity;// vec4(out_color.r * opacity, out_color.g * opacity, out_color.b * opacity, out_color.a * opacity);
}