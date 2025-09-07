using System;
using System.Collections.Generic;

namespace PdfParserTest.Models
{
    public class PickingListDto
    {
        public string SalesOrderNumber { get; set; } = "";
        public DateTime? PrintDateTime { get; set; }
        public string? PickingGroup { get; set; }
        public string? Buyer { get; set; }
        public DateTime? ShipDate { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? JobName { get; set; }
        public string? SalesRep { get; set; }
        public string? ShipVia { get; set; }
        public PartyDto SoldTo { get; set; } = new();
        public PartyDto ShipTo { get; set; } = new();
        public string? FobPoint { get; set; }
        public string? Route { get; set; }
        public string? Terms { get; set; }
        public decimal? TotalWeightLbs { get; set; }
        public List<PickingListItemDto> Items { get; set; } = new();
    }
    public class PartyDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
    }
    public class PickingListItemDto
    {
        public int LineNo { get; set; }
        public int Quantity { get; set; }                 // PCS or LBS depending on QuantityUnit
        public string QuantityUnit { get; set; } = "PCS"; // "PCS" | "LBS"
        public int? QuantityStaged { get; set; }
        public string ItemCode { get; set; } = "";
        public decimal? WidthIn { get; set; }
        public decimal? LengthIn { get; set; }
        public decimal? WeightLbs { get; set; }
        public string? Description { get; set; }
        public string? ProcessType { get; set; }          // "CTL" | "SHEET STOCK" | "SLITTER"
        public List<ItemTagDetailDto> TagDetails { get; set; } = new();
    }
    public class ItemTagDetailDto
    {
        public string? TagNo { get; set; }
        public string? HeatNo { get; set; }
        public string? MillRef { get; set; }
        public int? Qty { get; set; }              // numeric part only
        public string? QtyUnit { get; set; }       // "PCS" | "LBS" | null
        public decimal? ThicknessIn { get; set; }
        public string? Size { get; set; }
        public string? Location { get; set; }
    }
}
