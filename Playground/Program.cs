using Flamui.Drawing;

var bytes = File.ReadAllBytes(@"C:\Users\bruhw\Downloads\test_tvg_file.tvg");

TinyVG.ParseHeader(bytes);