global using FT_Long = System.IntPtr;

using System.Runtime.InteropServices;


namespace Playground
{
	[StructLayout(LayoutKind.Sequential)]
	public struct BBox
	{
		private FT_Long xMin, yMin;
		private FT_Long xMax, yMax;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct GenericRec
	{
		internal IntPtr data;
		internal IntPtr finalizer;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct FT_Face
	{
		internal FT_Long num_faces;
		internal FT_Long face_index;

		internal FT_Long face_flags;
		internal FT_Long style_flags;

		internal FT_Long num_glyphs;

		internal IntPtr family_name;
		internal IntPtr style_name;

		internal int num_fixed_sizes;
		internal IntPtr available_sizes;

		internal int num_charmaps;
		internal IntPtr charmaps;

		internal GenericRec generic;

		internal BBox bbox;

		internal ushort units_per_EM;
		internal short ascender;
		internal short descender;
		internal short height;

		internal short max_advance_width;
		internal short max_advance_height;

		internal short underline_position;
		internal short underline_thickness;

		internal FT_GlyphSlot* glyph;
		internal IntPtr size;
		internal IntPtr charmap;

		private IntPtr driver;
		private IntPtr memory;
		private IntPtr stream;

		private IntPtr sizes_list;
		private GenericRec autohint;
		private IntPtr extensions;

		private IntPtr @internal;
	}

	internal struct FT_Library
	{

	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct GlyphMetricsRec
	{
		internal FT_Long width;
		internal FT_Long height;

		internal FT_Long horiBearingX;
		internal FT_Long horiBearingY;
		internal FT_Long horiAdvance;

		internal FT_Long vertBearingX;
		internal FT_Long vertBearingY;
		internal FT_Long vertAdvance;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FTVector26Dot6
	{

		private IntPtr x;
		private IntPtr y;
	}

	[CLSCompliant(false)]
	public enum GlyphFormat : uint
	{
		/// <summary>
		/// The value 0 is reserved.
		/// </summary>
		None = 0,

		/// <summary>
		/// The glyph image is a composite of several other images. This format is only used with
		/// <see cref="LoadFlags.NoRecurse"/>, and is used to report compound glyphs (like accented characters).
		/// </summary>
		Composite = ('c' << 24 | 'o' << 16 | 'm' << 8 | 'p'),

		/// <summary>
		/// The glyph image is a bitmap, and can be described as an <see cref="FTBitmap"/>. You generally need to
		/// access the ‘bitmap’ field of the <see cref="GlyphSlot"/> structure to read it.
		/// </summary>
		Bitmap = ('b' << 24 | 'i' << 16 | 't' << 8 | 's'),

		/// <summary>
		/// The glyph image is a vectorial outline made of line segments and Bézier arcs; it can be described as an
		/// <see cref="Outline"/>; you generally want to access the ‘outline’ field of the <see cref="GlyphSlot"/>
		/// structure to read it.
		/// </summary>
		Outline = ('o' << 24 | 'u' << 16 | 't' << 8 | 'l'),

		/// <summary>
		/// The glyph image is a vectorial path with no inside and outside contours. Some Type 1 fonts, like those in
		/// the Hershey family, contain glyphs in this format. These are described as <see cref="Outline"/>, but
		/// FreeType isn't currently capable of rendering them correctly.
		/// </summary>
		Plotter = ('p' << 24 | 'l' << 16 | 'o' << 8 | 't')
	}
	[StructLayout(LayoutKind.Sequential)]
	internal struct BitmapRec
	{
		internal int rows;
		internal int width;
		internal int pitch;
		internal IntPtr buffer;
		internal short num_grays;
		internal PixelMode pixel_mode;
		internal byte palette_mode;
		internal IntPtr palette;
	}
	/// </summary>
	public enum PixelMode : byte
	{
		/// <summary>
		/// Value 0 is reserved.
		/// </summary>
		None = 0,

		/// <summary>
		/// A monochrome bitmap, using 1 bit per pixel. Note that pixels are stored in most-significant order (MSB),
		/// which means that the left-most pixel in a byte has value 128.
		/// </summary>
		Mono,

		/// <summary>
		/// An 8-bit bitmap, generally used to represent anti-aliased glyph images. Each pixel is stored in one byte.
		/// Note that the number of ‘gray’ levels is stored in the ‘num_grays’ field of the <see cref="FTBitmap"/>
		/// structure (it generally is 256).
		/// </summary>
		Gray,

		/// <summary>
		/// A 2-bit per pixel bitmap, used to represent embedded anti-aliased bitmaps in font files according to the
		/// OpenType specification. We haven't found a single font using this format, however.
		/// </summary>
		Gray2,

		/// <summary>
		/// A 4-bit per pixel bitmap, representing embedded anti-aliased bitmaps in font files according to the
		/// OpenType specification. We haven't found a single font using this format, however.
		/// </summary>
		Gray4,

		/// <summary>
		/// An 8-bit bitmap, representing RGB or BGR decimated glyph images used for display on LCD displays; the
		/// bitmap is three times wider than the original glyph image. See also <see cref="RenderMode.Lcd"/>.
		/// </summary>
		Lcd,

		/// <summary>
		/// An 8-bit bitmap, representing RGB or BGR decimated glyph images used for display on rotated LCD displays;
		/// the bitmap is three times taller than the original glyph image. See also
		/// <see cref="RenderMode.VerticalLcd"/>.
		/// </summary>
		VerticalLcd,

		/// <summary>
		/// An image with four 8-bit channels per pixel, representing a color image (such as emoticons) with alpha
		/// channel. For each pixel, the format is BGRA, which means, the blue channel comes first in memory. The color
		/// channels are pre-multiplied and in the sRGB colorspace. For example, full red at half-translucent opacity
		/// will be represented as ‘00,00,80,80’, not ‘00,00,FF,80’.
		/// </summary>
		/// <seealso cref="LoadFlags.Color"/>
		Bgra
	}
	[StructLayout(LayoutKind.Sequential)]
	internal struct OutlineRec
	{
		internal short n_contours;
		internal short n_points;

		internal IntPtr points;
		internal IntPtr tags;
		internal IntPtr contours;

		internal OutlineFlags flags;
	}
	[Flags]
	public enum OutlineFlags
	{
		/// <summary>
		/// Value 0 is reserved.
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// If set, this flag indicates that the outline's field arrays (i.e., ‘points’, ‘flags’, and ‘contours’) are
		/// ‘owned’ by the outline object, and should thus be freed when it is destroyed.
		/// </summary>
		Owner = 0x0001,

		/// <summary>
		/// By default, outlines are filled using the non-zero winding rule. If set to 1, the outline will be filled
		/// using the even-odd fill rule (only works with the smooth rasterizer).
		/// </summary>
		EvenOddFill = 0x0002,

		/// <summary>
		/// By default, outside contours of an outline are oriented in clock-wise direction, as defined in the TrueType
		/// specification. This flag is set if the outline uses the opposite direction (typically for Type 1 fonts).
		/// This flag is ignored by the scan converter.
		/// </summary>
		ReverseFill = 0x0004,

		/// <summary>
		/// By default, the scan converter will try to detect drop-outs in an outline and correct the glyph bitmap to
		/// ensure consistent shape continuity. If set, this flag hints the scan-line converter to ignore such cases.
		/// See below for more information.
		/// </summary>
		IgnoreDropouts = 0x0008,

		/// <summary>
		/// Select smart dropout control. If unset, use simple dropout control. Ignored if
		/// <see cref="OutlineFlags.IgnoreDropouts"/> is set. See below for more information.
		/// </summary>
		SmartDropouts = 0x0010,

		/// <summary>
		/// If set, turn pixels on for ‘stubs’, otherwise exclude them. Ignored if
		/// <see cref="OutlineFlags.IgnoreDropouts"/> is set. See below for more information.
		/// </summary>
		IncludeStubs =		0x0020,

		/// <summary>
		/// This flag indicates that the scan-line converter should try to convert this outline to bitmaps with the
		/// highest possible quality. It is typically set for small character sizes. Note that this is only a hint that
		/// might be completely ignored by a given scan-converter.
		/// </summary>
		HighPrecision =		0x0100,

		/// <summary>
		/// This flag is set to force a given scan-converter to only use a single pass over the outline to render a
		/// bitmap glyph image. Normally, it is set for very large character sizes. It is only a hint that might be
		/// completely ignored by a given scan-converter.
		/// </summary>
		SinglePass =		0x0200
	}

	internal unsafe struct FT_GlyphSlot
	{
		public FT_Library*        library;
		public FT_Face*           face;
		public FT_GlyphSlot*      next;
		public uint               glyph_index; /* new in 2.10; was reserved previously */
		public GenericRec         generic;

		public GlyphMetricsRec   metrics;
		public uint          linearHoriAdvance;
		public uint          linearVertAdvance;
		public FTVector26Dot6          advance;

		internal GlyphFormat format;

		internal BitmapRec bitmap;
		internal int bitmap_left;
		internal int bitmap_top;

		internal OutlineRec outline;

		internal uint num_subglyphs;
		internal IntPtr subglyphs;

		internal IntPtr control_data;
		internal FT_Long control_len;

		internal FT_Long lsb_delta;
		internal FT_Long rsb_delta;

		internal IntPtr other;

		private IntPtr @internal;
	}

	public enum RenderMode
	{
		/// <summary>
		/// This is the default render mode; it corresponds to 8-bit anti-aliased bitmaps.
		/// </summary>
		Normal = 0,

		/// <summary>
		/// This is equivalent to <see cref="RenderMode.Normal"/>. It is only defined as a separate value because
		/// render modes are also used indirectly to define hinting algorithm selectors.
		/// </summary>
		/// <see cref="LoadTarget"/>
		Light,

		/// <summary>
		/// This mode corresponds to 1-bit bitmaps (with 2 levels of opacity).
		/// </summary>
		Mono,

		/// <summary>
		/// This mode corresponds to horizontal RGB and BGR sub-pixel displays like LCD screens. It produces 8-bit
		/// bitmaps that are 3 times the width of the original glyph outline in pixels, and which use the
		/// <see cref="PixelMode.Lcd"/> mode.
		/// </summary>
		Lcd,

		/// <summary>
		/// This mode corresponds to vertical RGB and BGR sub-pixel displays (like PDA screens, rotated LCD displays,
		/// etc.). It produces 8-bit bitmaps that are 3 times the height of the original glyph outline in pixels and
		/// use the <see cref="PixelMode.VerticalLcd"/> mode.
		/// </summary>
		VerticalLcd,
	}

	/// <summary>
	/// FreeType error codes.
	/// </summary>
	public enum Error
	{
		/// <summary>No error.</summary>
		Ok = 0x00,

		/// <summary>Cannot open resource.</summary>
		CannotOpenResource = 0x01,

		/// <summary>Unknown file format.</summary>
		UnknownFileFormat = 0x02,

		/// <summary>Broken file.</summary>
		InvalidFileFormat = 0x03,

		/// <summary>Invalid FreeType version.</summary>
		InvalidVersion = 0x04,

		/// <summary>Module version is too low.</summary>
		LowerModuleVersion = 0x05,

		/// <summary>Invalid argument.</summary>
		InvalidArgument = 0x06,

		/// <summary>Unimplemented feature.</summary>
		UnimplementedFeature = 0x07,

		/// <summary>Broken table.</summary>
		InvalidTable = 0x08,

		/// <summary>Broken offset within table.</summary>
		InvalidOffset = 0x09,

		/// <summary>Array allocation size too large.</summary>
		ArrayTooLarge = 0x0A,

		/// <summary>Invalid glyph index.</summary>
		InvalidGlyphIndex = 0x10,

		/// <summary>Invalid character code.</summary>
		InvalidCharacterCode = 0x11,

		/// <summary>Unsupported glyph image format.</summary>
		InvalidGlyphFormat = 0x12,

		/// <summary>Cannot render this glyph format.</summary>
		CannotRenderGlyph = 0x13,

		/// <summary>Invalid outline.</summary>
		InvalidOutline = 0x14,

		/// <summary>Invalid composite glyph.</summary>
		InvalidComposite = 0x15,

		/// <summary>Too many hints.</summary>
		TooManyHints = 0x16,

		/// <summary>Invalid pixel size.</summary>
		InvalidPixelSize = 0x17,

		/// <summary>Invalid object handle.</summary>
		InvalidHandle = 0x20,

		/// <summary>Invalid library handle.</summary>
		InvalidLibraryHandle = 0x21,

		/// <summary>Invalid module handle.</summary>
		InvalidDriverHandle = 0x22,

		/// <summary>Invalid face handle.</summary>
		InvalidFaceHandle = 0x23,

		/// <summary>Invalid size handle.</summary>
		InvalidSizeHandle = 0x24,

		/// <summary>Invalid glyph slot handle.</summary>
		InvalidSlotHandle = 0x25,

		/// <summary>Invalid charmap handle.</summary>
		InvalidCharMapHandle = 0x26,

		/// <summary>Invalid cache manager handle.</summary>
		InvalidCacheHandle = 0x27,

		/// <summary>Invalid stream handle.</summary>
		InvalidStreamHandle = 0x28,

		/// <summary>Too many modules.</summary>
		TooManyDrivers = 0x30,

		/// <summary>Too many extensions.</summary>
		TooManyExtensions = 0x31,

		/// <summary>Out of memory.</summary>
		OutOfMemory = 0x40,

		/// <summary>Unlisted object.</summary>
		UnlistedObject = 0x41,

		/// <summary>Cannot open stream.</summary>
		CannotOpenStream = 0x51,

		/// <summary>Invalid stream seek.</summary>
		InvalidStreamSeek = 0x52,

		/// <summary>Invalid stream skip.</summary>
		InvalidStreamSkip = 0x53,

		/// <summary>Invalid stream read.</summary>
		InvalidStreamRead = 0x54,

		/// <summary>Invalid stream operation.</summary>
		InvalidStreamOperation = 0x55,

		/// <summary>Invalid frame operation.</summary>
		InvalidFrameOperation = 0x56,

		/// <summary>Nested frame access.</summary>
		NestedFrameAccess = 0x57,

		/// <summary>Invalid frame read.</summary>
		InvalidFrameRead = 0x58,

		/// <summary>Raster uninitialized.</summary>
		RasterUninitialized = 0x60,

		/// <summary>Raster corrupted.</summary>
		RasterCorrupted = 0x61,

		/// <summary>Raster overflow.</summary>
		RasterOverflow = 0x62,

		/// <summary>Negative height while rastering.</summary>
		RasterNegativeHeight = 0x63,

		/// <summary>Too many registered caches.</summary>
		TooManyCaches = 0x70,

		/// <summary>Invalid opcode.</summary>
		InvalidOpCode = 0x80,

		/// <summary>Too few arguments.</summary>
		TooFewArguments = 0x81,

		/// <summary>Stack overflow.</summary>
		StackOverflow = 0x82,

		/// <summary>Code overflow.</summary>
		CodeOverflow = 0x83,

		/// <summary>Bad argument.</summary>
		BadArgument = 0x84,

		/// <summary>Division by zero.</summary>
		DivideByZero = 0x85,

		/// <summary>Invalid reference.</summary>
		InvalidReference = 0x86,

		/// <summary>Found debug opcode.</summary>
		DebugOpCode = 0x87,

		/// <summary>Found ENDF opcode in execution stream.</summary>
		EndfInExecStream = 0x88,

		/// <summary>Nested DEFS.</summary>
		NestedDefs = 0x89,

		/// <summary>Invalid code range.</summary>
		InvalidCodeRange = 0x8A,

		/// <summary>Execution context too long.</summary>
		ExecutionTooLong = 0x8B,

		/// <summary>Too many function definitions.</summary>
		TooManyFunctionDefs = 0x8C,

		/// <summary>Too many instruction definitions.</summary>
		TooManyInstructionDefs = 0x8D,

		/// <summary>SFNT font table missing.</summary>
		TableMissing = 0x8E,

		/// <summary>Horizontal header (hhea) table missing.</summary>
		HorizHeaderMissing = 0x8F,

		/// <summary>Locations (loca) table missing.</summary>
		LocationsMissing = 0x90,

		/// <summary>Name table missing.</summary>
		NameTableMissing = 0x91,

		/// <summary>Character map (cmap) table missing.</summary>
		CMapTableMissing = 0x92,

		/// <summary>Horizontal metrics (hmtx) table missing.</summary>
		HmtxTableMissing = 0x93,

		/// <summary>PostScript (post) table missing.</summary>
		PostTableMissing = 0x94,

		/// <summary>Invalid horizontal metrics.</summary>
		InvalidHorizMetrics = 0x95,

		/// <summary>Invalid character map (cmap) format.</summary>
		InvalidCharMapFormat = 0x96,

		/// <summary>Invalid ppem value.</summary>
		InvalidPPem = 0x97,

		/// <summary>Invalid vertical metrics.</summary>
		InvalidVertMetrics = 0x98,

		/// <summary>Could not find context.</summary>
		CouldNotFindContext = 0x99,

		/// <summary>Invalid PostScript (post) table format.</summary>
		InvalidPostTableFormat = 0x9A,

		/// <summary>Invalid PostScript (post) table.</summary>
		InvalidPostTable = 0x9B,

		/// <summary>Opcode syntax error.</summary>
		SyntaxError = 0xA0,

		/// <summary>Argument stack underflow.</summary>
		StackUnderflow = 0xA1,

		/// <summary>Ignore this error.</summary>
		Ignore = 0xA2,

		/// <summary>No Unicode glyph name found.</summary>
		NoUnicodeGlyphName = 0xA3,

		/// <summary>`STARTFONT' field missing.</summary>
		MissingStartfontField = 0xB0,

		/// <summary>`FONT' field missing.</summary>
		MissingFontField = 0xB1,

		/// <summary>`SIZE' field missing.</summary>
		MissingSizeField = 0xB2,

		/// <summary>`FONTBOUNDINGBOX' field missing.</summary>
		MissingFontboudingboxField = 0xB3,

		/// <summary>`CHARS' field missing.</summary>
		MissingCharsField = 0xB4,

		/// <summary>`STARTCHAR' field missing.</summary>
		MissingStartcharField = 0xB5,

		/// <summary>`ENCODING' field missing.</summary>
		MissingEncodingField = 0xB6,

		/// <summary>`BBX' field missing.</summary>
		MissingBbxField = 0xB7,

		/// <summary>`BBX' too big.</summary>
		BbxTooBig = 0xB8,

		/// <summary>Font header corrupted or missing fields.</summary>
		CorruptedFontHeader = 0xB9,

		/// <summary>Font glyphs corrupted or missing fields.</summary>
		CorruptedFontGlyphs = 0xBA
	}
}