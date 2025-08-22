using Silk.NET.OpenGL;

namespace Flamui.Drawing;

public class RenderTexture
{
    public uint FramebufferName;
    public uint renderedTexture;
    public int width;
    public int height;

    public void UpdateSize(GL gl, int width, int height)
    {
        if (this.width != width || this.height != height)
        {
            gl.BindTexture(GLEnum.Texture2D, renderedTexture);
            gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgb, (uint)width, (uint)height, 0, GLEnum.Rgb, GLEnum.UnsignedByte, 0);
        }
    }

    public static unsafe RenderTexture Create(GL gl, int width, int height)
    {
        //gen frame buffer
        gl.GenFramebuffers(1, out uint FramebufferName);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferName);

        //gen texture
        gl.GenTextures(1, out uint renderedTexture);

        // "Bind" the newly created texture : all future texture functions will modify this texture
        gl.BindTexture(GLEnum.Texture2D, renderedTexture);

        var err = gl.GetError();
        if (err != GLEnum.NoError)
        {
            Console.WriteLine($"{err}");
            throw new Exception("anita");
        }

        // Give an empty image to OpenGL ( the last "0" )
        gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgb, (uint)width, (uint)height, 0, GLEnum.Rgb, GLEnum.UnsignedByte, null);
        // gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, (uint)width, (uint)height, 0, PixelFormat.Red, PixelType.UnsignedByte, null);

        // Poor filtering. Needed !
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest); //not sure if this cast to int is correct
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest); //not sure if this cast to int is correct

        //todo insert depth testing here if needed

        // Set "renderedTexture" as our colour attachement #0
        gl.FramebufferTexture(GLEnum.Framebuffer, GLEnum.ColorAttachment0, renderedTexture, 0);

        // Set the list of draw buffers.
        gl.DrawBuffers(1, [GLEnum.ColorAttachment0]);

        if (gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
            throw new Exception("womp womp");

        return new RenderTexture
        {
            FramebufferName = FramebufferName,
            renderedTexture = renderedTexture,
            height = height,
            width = width
        };
    }
}