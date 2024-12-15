using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace NewRenderer;

public struct Mesh
{
    public float[] Floats;
    public uint[] Indices;
}

public class Renderer
{
    const string vertexCode =
        """
        #version 330 core

        layout (location = 0) in vec3 aPosition;
        layout (location = 1) in vec2 aTextureCoord;
        layout (location = 2) in float aFillBezierType; //0 = disabled fill, >0 = fill inside, <0 = fill outside 
        layout (location = 3) in vec4 aColor;

        uniform mat4 transform;

        out vec2 frag_texCoords;
        out float fill_bezier_type;
        out vec4 frag_color;

        void main()
        {
          gl_Position = transform * vec4(aPosition, 1.0);
          frag_texCoords = aTextureCoord;
          fill_bezier_type = aFillBezierType;
          frag_color = aColor;
        }
        """;

    const string fragmentCode =
        """
        #version 330 core

        in vec2 frag_texCoords;
        in float fill_bezier_type;
        in vec4 frag_color;

        out vec4 out_color;

        void main()
        {
            if(fill_bezier_type == 0){
                out_color = frag_color;
            }else{
                float x = frag_texCoords.x;
                float y = frag_texCoords.y;
                
                //anti aliasing: some magic stuff i don't get from this video: https://dl.acm.org/doi/10.1145/1073204.1073303
                float f = x*x-y;
                float dx = dFdx(f);
                float dy = dFdy(f);
                float sd = f/sqrt(dx*dx+dy*dy);
                
                float opacity = 0;
                
                if(sd < -1)
                    opacity = 0;
                else if (sd > 1)
                    opacity = 1;
                else
                    opacity = (1 + sd) / 2;
                
                if(fill_bezier_type > 0){
                    opacity = 1 - opacity;
                }
        
                out_color = vec4(frag_color.r, frag_color.g, frag_color.b, frag_color.a * opacity);    
            }
        }
        """;

    public GL Gl;
    private uint _program;
    private int _transformLoc;
    private uint _vao;
    public IWindow Window;

    public void Initialize(IWindow window)
    {
        Window = window;

        Gl = Window.CreateOpenGL();

        Gl.ClearColor(Color.FromArgb(43, 45, 48));

        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Gl.Enable(EnableCap.Blend);

        _vao = Gl.GenVertexArray();
        Gl.BindVertexArray(_vao);


        //vertex shader compile stuff
        uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
        Gl.ShaderSource(vertexShader, vertexCode);

        Gl.CompileShader(vertexShader);

        Gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vertexShader));

        //the same for the fragment shader
        uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
        Gl.ShaderSource(fragmentShader, fragmentCode);

        Gl.CompileShader(fragmentShader);

        Gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fragmentShader));

        _program = Gl.CreateProgram();

        Gl.AttachShader(_program, vertexShader);
        Gl.AttachShader(_program, fragmentShader);

        Gl.LinkProgram(_program);

        Gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + Gl.GetProgramInfoLog(_program));

        _transformLoc = Gl.GetUniformLocation(_program, "transform");

        Gl.DetachShader(_program, vertexShader);
        Gl.DetachShader(_program, fragmentShader);
        Gl.DeleteShader(vertexShader);
        Gl.DeleteShader(fragmentShader);

        //pass attributes to vertex shader

        // Console.WriteLine(_gl.GetError());


        Gl.BindVertexArray(0);
    }

    public unsafe void DrawMesh(Mesh mesh)
    {
        Gl.BindVertexArray(_vao);
        Gl.UseProgram(_program);

        //create / bind vbo
        var vbo = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        Gl.BufferData(BufferTargetARB.ArrayBuffer, new ReadOnlySpan<float>(mesh.Floats), BufferUsageARB.StaticDraw);

        //create / bind ebo
        var ebo = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        Gl.BufferData(BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(mesh.Indices), BufferUsageARB.StaticDraw);

        const int stride = 3 + 2 + 1 + 4;; //10 because of 3 vertices + 2 UVs + 1 filltype + 4 color

        const uint positionLoc = 0; //aPosition in shader
        Gl.EnableVertexAttribArray(positionLoc);
        Gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)0);

        const uint texCoordLoc = 1;
        Gl.EnableVertexAttribArray(texCoordLoc);
        Gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(3 * sizeof(float)));

        const uint bezierTypeLoc = 2;
        Gl.EnableVertexAttribArray(bezierTypeLoc);
        Gl.VertexAttribPointer(bezierTypeLoc, 1, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(5 * sizeof(float)));

        const uint colorLoc = 3;
        Gl.EnableVertexAttribArray(colorLoc);
        Gl.VertexAttribPointer(colorLoc, 4, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(6 * sizeof(float)));

        var matrix = GetWorldToScreenMatrix();


        Gl.ProgramUniformMatrix4(_program, _transformLoc, false, new ReadOnlySpan<float>(GetAsFloatArray(matrix)));

        Gl.BindVertexArray(_vao);
        Gl.UseProgram(_program);

        Gl.DrawElements(PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt,  (void*) 0);

        Gl.DeleteBuffer(ebo);
        Gl.DeleteBuffer(vbo);
    }

    private Matrix4X4<float> GetWorldToScreenMatrix()
    {
        return Matrix4X4.CreateScale(1f / Window.Size.X, 1f / Window.Size.Y, 1) *
            Matrix4X4.CreateScale(2f, 2f, 1) *
            Matrix4X4.CreateTranslation(-1f, -1f, 0) *
            Matrix4X4.CreateScale(1f, -1f, 1f);
    }

    private static float[] GetAsFloatArray(Matrix4X4<float> matrix)
    {
        return
        [
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        ];
    }
}