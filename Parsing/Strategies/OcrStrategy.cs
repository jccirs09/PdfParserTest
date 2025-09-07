using Microsoft.Extensions.Configuration;
using NReco.PdfRenderer;
using Tesseract;
using System.IO;
using System.Text;
using System.Drawing;

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
            var psm = int.TryParse(_cfg["Parsing:OcrPsm"], out var p) ? p : 6;
            var tessdata = _cfg["Parsing:TessdataPath"]!;
            var lang = "eng";

            using var engine = new TesseractEngine(tessdata, lang, EngineMode.Default);
            engine.SetVariable("tessedit_pageseg_mode", psm.ToString());

            var pdfToImage = new PdfToImageConverter();
            var pdfInfo = new PdfInfo();
            pdf.Position = 0;
            var info = pdfInfo.GetPdfInfo(pdf);
            int pageCount = info.Pages;

            var sb = new StringBuilder();
            for (int i = 1; i <= pageCount; i++)
            {
                pdf.Position = 0;
                using var img = pdfToImage.GenerateImage(pdf, i);
                using var ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                using var pix = Pix.LoadFromMemory(ms.ToArray());
                using var page = engine.Process(pix);
                sb.AppendLine(page.GetText());
            }

            var text = sb.ToString();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
    }
}
