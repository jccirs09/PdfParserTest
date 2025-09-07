using UglyToad.PdfPig;
using PdfParserTest.Components.Models;
using System.Text.RegularExpressions;
using System.Globalization;

namespace PdfParserTest.Components.Services
{
    public class PdfParsingService
    {
        public PickingList ParsePickingList(string filePath)
        {
            using (PdfDocument document = PdfDocument.Open(filePath))
            {
                string fullText = "";
                foreach (var page in document.GetPages())
                {
                    fullText += page.Text;
                }

                var pickingList = new PickingList();
                var lines = fullText.Split('\n');

                // Find Order Number
                var orderNumberLine = lines.FirstOrDefault(l => l.Contains("No."));
                if (orderNumberLine != null)
                {
                    var match = Regex.Match(orderNumberLine, @"No\.(\d+)");
                    if (match.Success)
                    {
                        pickingList.OrderNumber = match.Groups[1].Value;
                    }
                }

                // Find Customer Name
                var buyerLineIndex = Array.FindIndex(lines, l => l.Contains("BUYER"));
                if (buyerLineIndex != -1 && buyerLineIndex + 1 < lines.Length)
                {
                    pickingList.CustomerName = lines[buyerLineIndex + 1].Trim();
                }

                // Find Products
                var productLinesStartIndex = Array.FindIndex(lines, l => l.Contains("LINE    QUANTITY"));
                if (productLinesStartIndex != -1)
                {
                    for (int i = productLinesStartIndex + 1; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        if (line.Contains("----------")) break;

                        var match = Regex.Match(line, @"(\d+)\s+([\d,]+)\s+LBS\s+.*\s+([A-Z0-9/]+)\s+");
                        if (match.Success)
                        {
                            var product = new Product
                            {
                                Sku = match.Groups[3].Value,
                                Description = line.Substring(match.Index + match.Length).Trim(),
                                Quantity = int.Parse(match.Groups[2].Value, NumberStyles.AllowThousands)
                            };
                            pickingList.Products.Add(product);
                        }
                    }
                }

                return pickingList;
            }
        }
    }
}
