#version 450 core

layout(location = 0) in vec4 inColor;
layout(location = 1) in vec4 inBorderColor;
layout(location = 2) in vec2 inTopLeft;
layout(location = 3) in vec2 inBottomRight;
layout(location = 4) in float inCornerRadius;
layout(location = 5) in float inBorderThickness;

out vec4 vColor;
out vec2 vRectCenter;
out vec2 vRectHalfSize;
out float vCornerRadius;
out float vBorderThickness;

uniform mat4 transform;

void main()
{

    vec2 corners[4] = vec2[4](
        vec2(-1.0, -1.0),
        vec2( 1.0, -1.0),
        vec2(-1.0,  1.0),
        vec2( 1.0,  1.0)
    );

    if(gl_VertexID == 0)
    {
        gl_Position = vec4(inBottomRight, 0.0, 1.0);
    }else if(gl_VertexID == 1)
    {
        gl_Position = vec4(inTopLeft.x, inBottomRight.y, 0.0, 1.0);
    }
    else if(gl_VertexID == 2)
    {
        gl_Position = vec4(inTopLeft, 0.0, 1.0);
    }else if(gl_VertexID == 3)
    {
        gl_Position = vec4(inBottomRight.x, inTopLeft.y, 0.0, 1.0);
    }



    // pass per-instance data to fragment shader
    vColor = inColor;
    vRectCenter = (transform * vec4(inTopLeft, 0.0, 1.0)).xy;
    vRectHalfSize = (transform * vec4(inTopLeft, 0.0, 1.0)).xy;
    vCornerRadius = inCornerRadius;
    vBorderThickness = inBorderThickness;

}