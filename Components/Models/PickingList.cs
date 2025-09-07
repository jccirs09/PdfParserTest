using System.Collections.Generic;

namespace PdfParserTest.Components.Models
{
    public class PickingList
    {
        public string? OrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
    }

    public class Product
    {
        public string? Sku { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
    }
}
