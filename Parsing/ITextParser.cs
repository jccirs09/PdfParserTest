using PdfParserTest.Models;

namespace PdfParserTest.Parsing
{
    public interface ITextParser
    {
        PickingListDto ParseFromPlainText(string text);
    }
}
