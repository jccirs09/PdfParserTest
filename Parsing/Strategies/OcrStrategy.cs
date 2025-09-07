#if WINDOWS
using Microsoft.Extensions.Configuration;
using PdfiumViewer;
using Tesseract;
using ImageMagick;
using System.IO;
using System.Text;
using System;

namespace PdfParserTest.Parsing.Strategies
{
    public sealed class OcrStrategy : IPdfParseStrategy
    {
        private readonly IConfiguration _cfg;
        public OcrStrategy(IConfiguration cfg) => _cfg = cfg;

        public bool CanHandle()
        {
            var td = _cfg["Parsing:TessdataPath"];
            return !string.IsNullOrWhiteSpace(td) && Directory.Exists(td);
        }

        public string? TryGetText(Stream pdf)
        {
            pdf.Position = 0;
            var dpi = int.TryParse(_cfg["Parsing:OcrDpi"], out var d) ? d : 350;
            var psm = int.TryParse(_cfg["Parsing:OcrPsm"], out var p) ? p : 6;
            var tessdata = _cfg["Parsing:TessdataPath"]!;
            var lang = "eng";

            using var engine = new TesseractEngine(tessdata, lang, EngineMode.Default);
            engine.SetVariable("tessedit_pageseg_mode", psm.ToString());

            using var doc = PdfDocument.Load(pdf);
            var sb = new StringBuilder();

            for (int i = 0; i < doc.PageCount; i++)
            {
                using var img = doc.Render(i, dpi, dpi, PdfRenderFlags.ForPrinting);
                using var mg = new MagickImage(img) { ColorSpace = ColorSpace.Gray };
                mg.Deskew(new Percentage(40));
                mg.ContrastStretch(new Percentage(0.5), new Percentage(0.5));
                mg.OtsuThreshold();

                using var ms = new MemoryStream(); mg.Write(ms, MagickFormat.Png); ms.Position = 0;
                using var pix = Pix.LoadFromMemory(ms.ToArray());
                using var page = engine.Process(pix);
                sb.AppendLine(page.GetText());
            }
            var text = sb.ToString();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
    }
}
#endif
