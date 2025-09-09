using Silk.NET.OpenGL;

namespace Flamui.Drawing;

public class RenderTexture
{
    public uint FramebufferName;
    public uint textureId;
    public int width;
    public int height;
    public uint DepthStencilRbo;

    public void UpdateSize(GL gl, int pWidth, int pHeight)
    {
        if (pWidth == 0 || pHeight == 0)
            return;

        if (width != pWidth || height != pHeight)
        {
            width = pWidth;
            height = pHeight;

            gl.DeleteTexture(textureId);

            textureId = CreateTexture(gl, pWidth, pHeight);

            // Reattach color texture
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferName);
            gl.FramebufferTexture(GLEnum.Framebuffer, GLEnum.ColorAttachment0, textureId, 0);

            // Resize depth-stencil renderbuffer
            gl.BindRenderbuffer(GLEnum.Renderbuffer, DepthStencilRbo);
            gl.RenderbufferStorage(GLEnum.Renderbuffer, InternalFormat.Depth24Stencil8, (uint)width, (uint)height);

            // Make sure framebuffer is complete
            if (gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
                throw new Exception("Framebuffer incomplete after resize");
        }
    }

    private static unsafe uint CreateTexture(GL gl, int width, int height)
    {
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
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest); //not sure if this cast to int is correct
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest); //not sure if this cast to int is correct

        return renderedTexture;
    }

    public static unsafe RenderTexture Create(GL gl, int width, int height)
    {
        //gen frame buffer
        gl.GenFramebuffers(1, out uint FramebufferName);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferName);

        var renderedTexture = CreateTexture(gl, width, height);

        // Set "renderedTexture" as our colour attachement #0
        gl.FramebufferTexture(GLEnum.Framebuffer, GLEnum.ColorAttachment0, renderedTexture, 0);

        gl.GenRenderbuffers(1, out uint depthStencilRbo);
        gl.BindRenderbuffer(GLEnum.Renderbuffer, depthStencilRbo);

        // Use DEPTH24_STENCIL8 for a combined depth+stencil buffer
        gl.RenderbufferStorage(
            GLEnum.Renderbuffer,
            InternalFormat.Depth24Stencil8,
            (uint)width,
            (uint)height
        );

        // Attach it to the framebuffer
        gl.FramebufferRenderbuffer(
            GLEnum.Framebuffer,
            GLEnum.DepthStencilAttachment,
            GLEnum.Renderbuffer,
            depthStencilRbo
        );

        // Set the list of draw buffers.
        gl.DrawBuffers(1, [GLEnum.ColorAttachment0]);

        if (gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
            throw new Exception("womp womp");

        return new RenderTexture
        {
            FramebufferName = FramebufferName,
            textureId = renderedTexture,
            DepthStencilRbo = depthStencilRbo,
            height = height,
            width = width
        };
    }
}