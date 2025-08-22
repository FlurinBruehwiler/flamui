#version 330 core

in vec2 vTexCoord;
uniform sampler2D uTexture;

out vec4 out_color;

void main()
{
    out_color = texture(uTexture, vTexCoord);
}