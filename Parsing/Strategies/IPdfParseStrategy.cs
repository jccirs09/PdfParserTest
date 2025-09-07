using System.IO;

namespace PdfParserTest.Parsing.Strategies
{
    public interface IPdfParseStrategy
    {
        bool CanHandle();
        string? TryGetText(Stream pdf);
    }
}
