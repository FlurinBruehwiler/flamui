
//render doc tutorial https://www.youtube.com/watch?v=lFexgk_2FTc&t=439s
#version 450 core


in vec4 vColor;
in vec2 vRectCenterPx;
in vec2 vRectHalfSizePx;
in float vCornerRadiusPx;
in float vBorderThicknessPx;

layout(location = 0)
out vec4 out_color;

uniform vec2 uViewportSize;

float sdBox( in vec2 p, in vec2 b )
{
    vec2 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
}

// p = sample point
// b = half size
// r = radius
float sdBoxRound( in vec2 p, in vec2 b, in float r )
{
  return sdBox(p, vec2(b.x - r, b.y - r)) - r;
}

void main()
{
    vec2 p = gl_FragCoord.xy - vRectCenterPx;

    float sdf_result = -sdBoxRound(p, vRectHalfSizePx, vCornerRadiusPx);

    float alpha = smoothstep(0.0, 1.0, sdf_result);

/*
    if(alpha <= 0){
        discard;
    }
*/

    out_color = vec4(vColor.rgb, alpha);
}