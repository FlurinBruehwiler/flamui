namespace Playground
{
    public static class Program
    {
        public static unsafe void Main()
        {
            var error = FT.FT_Init_FreeType(out var library);
            if (error != Error.Ok)
            {
                Console.WriteLine($"{nameof(FT.FT_Init_FreeType)}: {error}");
                return;
            }

            FT_Face face = default;

            var bytes = File.ReadAllBytes(@"C:\Windows\Fonts\segoeui.ttf");
            fixed (byte* b = bytes)
            {
                error = FT.FT_New_Memory_Face(library, b, bytes.Length, 0, &face);
                if (error != Error.Ok)
                {
                    Console.WriteLine($"{nameof(FT.FT_New_Memory_Face)}: {error}");
                    return;
                }
            }

            var glyphIndex = FT.FT_Get_Char_Index(&face, 'A');

            error = FT.FT_Set_Pixel_Sizes(&face, 10, 10);
            if (error != Error.Ok)
            {
                Console.WriteLine($"{nameof(FT.FT_Set_Pixel_Sizes)}: {error}");
                return;
            }

            error = FT.FT_Load_Glyph(&face, glyphIndex, 0);
            if (error != Error.Ok)
            {
                Console.WriteLine($"{nameof(FT.FT_Load_Glyph)}: {error}");
                return;
            };

            error = FT.FT_Render_Glyph(face.glyph, RenderMode.Normal);
            if (error != Error.Ok)
            {
                Console.WriteLine($"{nameof(FT.FT_Render_Glyph)}: {error}");
                return;
            };

            if (face.glyph->format != GlyphFormat.Bitmap)
            {
                Console.WriteLine("Not a bitmap????");
                return;
            }

            using (var fileStream = File.OpenWrite(@"C:\Programming\Github\ImageTest\"))
            {
                var bitmap = face.glyph->bitmap;
                // var imageWriter = new ImageWriter();
                // imageWriter.WriteBmp((void*)bitmap.buffer, bitmap.width, bitmap.rows, ColorComponents.Grey, fileStream);
            }

        }
    }
}

