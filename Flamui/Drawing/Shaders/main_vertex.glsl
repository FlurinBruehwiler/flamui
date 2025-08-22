#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTextureCoord;
layout (location = 2) in float aFillBezierType; //0 = disabled fill, >0 = fill inside, <0 = fill outside
layout (location = 3) in vec4 aColor;
layout (location = 4) in float aTextureType; //0 = dont use texture, >0 = use texture
layout (location = 5) in float aTextureId;

uniform mat4 transform;

out vec2 frag_texCoords;
out float fill_bezier_type;
out vec4 frag_color;
out float texture_type;
out float texture_id;

void main()
{
  gl_Position = transform * vec4(aPosition, 1.0);
  frag_texCoords = aTextureCoord;
  fill_bezier_type = aFillBezierType;
  frag_color = aColor;
  texture_type = aTextureType;
  texture_id = aTextureId;
}