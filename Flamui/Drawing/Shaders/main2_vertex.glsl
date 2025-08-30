#version 450 core

layout(location = 0) in vec4 inColor;
layout(location = 1) in vec4 inBorderColor;
layout(location = 2) in vec2 inTopLeft;
layout(location = 3) in vec2 inBottomRight;
layout(location = 4) in float inCornerRadius;
layout(location = 5) in float inBorderThickness;

out vec4 vColor;
out vec2 vRectCenterPx;
out vec2 vRectHalfSizePx;
out float vCornerRadiusPx;
out float vBorderThicknessPx;

uniform mat4 transform;

void main()
{
    vec2 topLeft = (transform * vec4(inTopLeft, 1.0, 1.0)).xy;
    vec2 bottomRight = (transform * vec4(inBottomRight, 1.0, 1.0)).xy;

    if(gl_VertexID == 0)
    {
        gl_Position = vec4(bottomRight, 0.0, 1.0);
    }else if(gl_VertexID == 1)
    {
        gl_Position = vec4(topLeft.x, bottomRight.y, 0.0, 1.0);
    }
    else if(gl_VertexID == 2)
    {
        gl_Position = vec4(bottomRight.x, topLeft.y, 0.0, 1.0);
    }
    else if(gl_VertexID == 3)
    {
        gl_Position = vec4(topLeft, 0.0, 1.0);
    }

    // pass per-instance data to fragment shader
    vColor = inColor;
    vRectHalfSizePx = abs(inBottomRight - inTopLeft) / 2;

    vec2 bottomLeftPx = vec2(inTopLeft.x, inBottomRight.y);
    vRectCenterPx = inTopLeft + vRectHalfSizePx;
    vCornerRadiusPx = inCornerRadius;
    vBorderThicknessPx = inBorderThickness;

}