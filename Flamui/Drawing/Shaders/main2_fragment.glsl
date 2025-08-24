#version 450 core


in vec4 color;
in vec2 rect_center;
in vec2 rect_half_size;
in float corner_radius_px;
in float border_thickness_px;

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
  return sdBox(p, b) - r;
}

void main()
{
    float sdf_result = sbBoxRounded((gl_FragCoord.xy / uViewportSize) - rect_center, rect_half_size, corner_radius);

    if(sdf_result < 0)
    {
        discard;
    }

    return color;
}