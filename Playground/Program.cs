// See https://aka.ms/new-console-template for more information

using Flamui;

var bytes = File.ReadAllBytes(@"C:\Users\bruhw\Downloads\test_tga_file.tga");

TGALoader.TgaToBitmap(bytes.AsSpan());