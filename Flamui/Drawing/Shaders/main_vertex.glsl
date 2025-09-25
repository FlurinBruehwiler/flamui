#version 450 core

layout(location = 0) in vec4 inColor;
layout(location = 1) in vec2 inTopLeft;
layout(location = 2) in vec2 inBottomRight;
layout(location = 3) in float inCornerRadius;
layout(location = 4) in float inBorderThickness;
layout(location = 5) in int inTextureSlot;
layout(location = 6) in vec4 inTextureCoordinate;
layout(location = 7) in float inShadowBlur;

out vec4 vColor;
out vec2 vRectCenterPx;
out vec2 vRectHalfSizePx;
out float vCornerRadiusPx;
out float vBorderThicknessPx;
flat out int vTextureSlot;
out vec2 vTextureCoordinate;
out float vShadowBlur;

uniform mat4 transform;


void main()
{
    vec2 topLeft = (transform * vec4(inTopLeft, 1.0, 1.0)).xy;
    vec2 bottomRight = (transform * vec4(inBottomRight, 1.0, 1.0)).xy;

    if(gl_VertexID == 0)
    {
        gl_Position = vec4(bottomRight, 0.0, 1.0);
        vTextureCoordinate = inTextureCoordinate.xy + inTextureCoordinate.zw;
    }else if(gl_VertexID == 1)
    {
        gl_Position = vec4(topLeft.x, bottomRight.y, 0.0, 1.0);
        vTextureCoordinate = inTextureCoordinate.xy + vec2(0, inTextureCoordinate.w);

    }
    else if(gl_VertexID == 2)
    {
        vTextureCoordinate = inTextureCoordinate.xy + vec2(inTextureCoordinate.z, 0);
        gl_Position = vec4(bottomRight.x, topLeft.y, 0.0, 1.0);
    }
    else if(gl_VertexID == 3)
    {
        vTextureCoordinate = inTextureCoordinate.xy;
        gl_Position = vec4(topLeft, 0.0, 1.0);
    }

    //vTextureCoordinate = inTextureCoordinate.xy ;

    // pass per-instance data to fragment shader
    vColor = inColor;
    vTextureSlot = inTextureSlot;
    vShadowBlur = inShadowBlur;

    vRectHalfSizePx = abs(inBottomRight - inTopLeft) / 2;

    vec2 bottomLeftPx = vec2(inTopLeft.x, inBottomRight.y);
    vRectCenterPx = inTopLeft + vRectHalfSizePx;
    vCornerRadiusPx = inCornerRadius;
    vBorderThicknessPx = inBorderThickness;

}