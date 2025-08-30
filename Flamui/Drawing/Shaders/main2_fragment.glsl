
//render doc tutorial https://www.youtube.com/watch?v=lFexgk_2FTc&t=439s
#version 450 core


in vec4 vColor;
in vec2 vRectCenterPx;
in vec2 vRectHalfSizePx;
in float vCornerRadiusPx;
in float vBorderThicknessPx;

layout(location = 0)
out vec4 out_color;

layout(origin_upper_left) in vec4 gl_FragCoord;

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
    vec2 sdf_sample_pos = gl_FragCoord.xy - vRectCenterPx;

    float border_sdf = 1;
    if(vBorderThicknessPx > 0)
    {
        border_sdf = abs(sdBoxRound(sdf_sample_pos, vRectHalfSizePx, vCornerRadiusPx)  + 3 * vBorderThicknessPx) - vBorderThicknessPx;
        border_sdf = smoothstep(0.0, 1.0, -border_sdf);
    }
    if(border_sdf < 0.001f)
    {
        discard;
    }

    float corner_sdf = 1;
    if(vCornerRadiusPx > 0)
    {
        corner_sdf = -sdBoxRound(sdf_sample_pos, vRectHalfSizePx, vCornerRadiusPx);
        corner_sdf = smoothstep(0.0, 1.0, corner_sdf);
    }

    out_color = vColor;
    out_color.a *= corner_sdf;
    out_color.a *= border_sdf;
}