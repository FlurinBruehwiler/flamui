#version 450 core

layout(location = 0) in vec4 inColor;
layout(location = 1) in vec2 inRectCenter;
layout(location = 2) in vec2 inRectHalfSize;
layout(location = 3) in float inCornerRadius;
layout(location = 4) in float inBorderThickness;

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

    vec2 localPos = corners[gl_VertexID] * inRectHalfSize; // scale to half-size
    vec2 worldPos = inRectCenter + localPos;               // translate to center

    gl_Position = transform * vec4(worldPos, 0.0, 1.0);

    // pass per-instance data to fragment shader
    vColor = inColor;
    vRectCenter = transform * inRectCenter;
    vRectHalfSize = transform * inRectHalfSize;
    vCornerRadius = inCornerRadius;
    vBorderThickness = inBorderThickness;
}