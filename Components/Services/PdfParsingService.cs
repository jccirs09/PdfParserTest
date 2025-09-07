using UglyToad.PdfPig;

namespace PdfParserTest.Components.Services
{
    public class PdfParsingService
    {
        public string GetRawText(string filePath)
        {
            using (PdfDocument document = PdfDocument.Open(filePath))
            {
                string fullText = "";
                foreach (var page in document.GetPages())
                {
                    fullText += page.Text;
                }
                return fullText;
            }
        }
    }
}
