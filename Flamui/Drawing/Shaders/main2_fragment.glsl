#version 450 core


in vec4 vColor;
in vec2 vRectCenter;
in vec2 vRectHalfSize;
in float vCornerRadius;
in float vBorderThickness;

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
    out_color = vec4(1.0, 0.0, 0.0, 1.0);
/*
    float sdf_result = sdBoxRound((gl_FragCoord.xy / uViewportSize) - vRectCenter, vRectHalfSize, vCornerRadius);

    if(sdf_result < 0)
    {
        discard;
    }

    out_color = vColor;
    */
}