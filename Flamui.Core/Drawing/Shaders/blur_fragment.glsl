#version 450 core

in vec2 vTexCoord;
uniform sampler2D uTexture;
uniform vec2 uViewportSize;
uniform float kernelSize;
uniform vec4 kernel[32];
uniform vec2 direction;

out vec4 out_color;

void main()
{
    vec3 color = kernel[0].x * texture(uTexture,  gl_FragCoord.xy / uViewportSize).rgb;

    for(int i = 1; i < kernelSize; i++){
        float weight = kernel[i].x;
        float offset = kernel[i].y;
        color += weight * texture(uTexture, (gl_FragCoord.xy - (offset * direction)) / uViewportSize).rgb;
        color += weight * texture(uTexture, (gl_FragCoord.xy + (offset * direction)) / uViewportSize).rgb;
    }

    out_color = vec4(color, 1.0);
}