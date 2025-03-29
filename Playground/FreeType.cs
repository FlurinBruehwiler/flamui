using System.Runtime.InteropServices;

//we copy what we need from here https://raw.githubusercontent.com/space-wizards/SharpFont/refs/heads/wizards/Source/SharpFont/FT.Internal.cs,
namespace Playground;

public static unsafe class FT
{
	private const string FreetypeDll = "freetype.dll";
	private const CallingConvention CallConvention = CallingConvention.Cdecl;

	[DllImport(FreetypeDll, CallingConvention = CallConvention)]
	internal static extern Error FT_Init_FreeType(out IntPtr alibrary);

	[DllImport(FreetypeDll, CallingConvention = CallConvention, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
	internal static extern Error FT_New_Face(IntPtr library, char* filepathname, int face_index, FT_Face* aface);

	[DllImport(FreetypeDll, CallingConvention = CallConvention)]
	internal static extern Error FT_New_Memory_Face(IntPtr library, byte* file_base, int file_size, int face_index, FT_Face* aface);

	[DllImport(FreetypeDll, CallingConvention = CallConvention)]
	internal static extern uint FT_Get_Char_Index(FT_Face* ftFace, uint charcode);

	[DllImport(FreetypeDll, CallingConvention = CallConvention)]
	internal static extern Error FT_Load_Glyph(FT_Face* ftFace, uint glyph_index, int load_flags);

	[DllImport(FreetypeDll, CallingConvention = CallConvention)]
	internal static extern Error FT_Render_Glyph(FT_GlyphSlot* slot, RenderMode render_mode);

	[DllImport(FreetypeDll, CallingConvention = CallConvention)]
	internal static extern Error FT_Set_Pixel_Sizes(FT_Face* face, uint pixel_width, uint pixel_height);
}