#version 330 core

in vec2 vTexCoord;
uniform sampler2D uTexture;
uniform vec2 uViewportSize;

out vec4 out_color;

void main()
{
    const float kernelSize = 3.0;
    vec3 boxBlurColor = vec3(0.0);

    float boxBlurDivisor = pow(2.0 * kernelSize + 1.0, 2.0);

    for(float i = -kernelSize; i <= kernelSize; i++)
    {
        for(float j = -kernelSize; j <= kernelSize; j++)
        {
            vec2 uv = (gl_FragCoord.xy + vec2(i, j)) / uViewportSize;
            boxBlurColor = texture(uTexture, uv).rgb + boxBlurColor;
        }
    }

    boxBlurColor = boxBlurColor / boxBlurDivisor;

    out_color = vec4(boxBlurColor, 1.0);
}