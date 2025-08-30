
using System.Numerics;

Console.WriteLine(smoothstep(0, 5, 400));
Console.WriteLine(smoothstep(0, 5, 100));
Console.WriteLine(smoothstep(0, 5, -100));
Console.WriteLine(smoothstep(0, 5, 3));

float smoothstep(float edge0, float edge1, float x)
{
    float t = x;  /* Or genDType t; */
    t = (float)Math.Clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
    return t * t * (3.0f - 2.0f * t);
}

