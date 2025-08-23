#version 330 core

in vec2 vTexCoord;
uniform sampler2D uTexture;
uniform vec2 uViewportSize;
uniform float kernelSize;
uniform vec4 kernel[32];
uniform vec2 direction;

out vec4 out_color;

void main()
{
//    if(kernel[5].y == 9){
//        out_color = vec4(1.0, 0.0, 0.0, 1.0);
//    }else{
        //
        vec3 color = kernel[0].x * texture(uTexture,  gl_FragCoord.xy / uViewportSize).rgb;

        for(int i = 1; i < kernelSize; i++){
            float weight = kernel[i].x;
            float offset = kernel[i].y;
            color += weight * texture(uTexture, (gl_FragCoord.xy - (offset * direction)) / uViewportSize).rgb;
            color += weight * texture(uTexture, (gl_FragCoord.xy + (offset * direction)) / uViewportSize).rgb;
        }

        out_color = vec4(color, 1.0);
//    }
//    vec3 boxBlurColor = vec3(0.0);
//
//    float boxBlurDivisor = pow(2.0 * kernelSize + 1.0, 2.0);
//
//    for(float i = -kernelSize; i <= kernelSize; i++)
//    {
//        for(float j = -kernelSize; j <= kernelSize; j++)
//        {
//            vec2 uv = (gl_FragCoord.xy + vec2(i, j)) / uViewportSize;
//            boxBlurColor = texture(uTexture, uv).rgb + boxBlurColor;
//        }
//    }
//
//    boxBlurColor = boxBlurColor / boxBlurDivisor;
//
//    out_color = vec4(boxBlurColor, 1.0);
}