#version 460 core

uniform sampler2D uTexture;

out vec4 out_color;

void main()
{
    vec4 color = texture(uTexture, gl_FragCoord.xy);

    out_color = color;
}