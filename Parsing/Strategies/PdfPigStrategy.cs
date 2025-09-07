using UglyToad.PdfPig;
using System.IO;
using System.Text;

namespace PdfParserTest.Parsing.Strategies
{
    public sealed class PdfPigStrategy : IPdfParseStrategy
    {
        public bool CanHandle() => true;

        public string? TryGetText(Stream pdf)
        {
            pdf.Position = 0;
            using var doc = PdfDocument.Open(pdf);
            var sb = new StringBuilder();
            foreach (var p in doc.GetPages())
            {
                var t = p.Text;
                if (!string.IsNullOrWhiteSpace(t)) sb.AppendLine(t);
            }
            var text = sb.ToString();
            return text.Trim().Length < 40 ? null : text;
        }
    }
}
