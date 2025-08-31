
#version 450 core
#extension GL_ARB_bindless_texture : enable


in vec4 vColor;
in vec2 vRectCenterPx;
in vec2 vRectHalfSizePx;
in float vCornerRadiusPx;
in float vBorderThicknessPx;
flat in uvec2 vTextureHandle;
in vec2 vTextureCoordinate;
in float vShadowBlur;

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
        border_sdf = abs(sdBoxRound(sdf_sample_pos, vRectHalfSizePx, vCornerRadiusPx)  + 2 * vBorderThicknessPx) - vBorderThicknessPx;
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

    float shadow_sdf = 1;
    if(vShadowBlur > 0)
    {
        shadow_sdf = sdBoxRound(sdf_sample_pos, vRectHalfSizePx, vCornerRadiusPx);
        shadow_sdf += 2 * vShadowBlur; //make the shape smaller by shadow blur, because we have expanded the mesh by shadowBlur, in the renderer
        shadow_sdf = 1 - smoothstep(-vShadowBlur, +vShadowBlur, shadow_sdf);
    }

    out_color = vColor;

    if(vTextureHandle.x != 0 || vTextureHandle.y != 0)
    {
        out_color.a *= texture(sampler2D(vTextureHandle), vTextureCoordinate).r;
    }

    out_color.a *= shadow_sdf;
    out_color.a *= corner_sdf;
    out_color.a *= border_sdf;
}