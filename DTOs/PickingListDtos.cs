using System;
using System.Collections.Generic;

namespace PickingListApp.DTOs;

public class PickingListDto
{
    public int Id { get; set; }
    public string SalesOrderNumber { get; set; } = default!;
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
    public string? ReceivingHours { get; set; }
    public string? CallBeforePhone { get; set; }
    public decimal? TotalWeightLbs { get; set; }
    public ICollection<PickingListItemDto> Items { get; set; } = new List<PickingListItemDto>();
}

public class PartyDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
}

public class PickingListItemDto
{
    public int Id { get; set; }
    public int LineNo { get; set; }
    public int Quantity { get; set; }
    public string QuantityUnit { get; set; } = "PCS";
    public int? QuantityStaged { get; set; }
    public string ItemCode { get; set; } = default!;
    public decimal? WidthIn { get; set; }
    public decimal? LengthIn { get; set; }
    public decimal? WeightLbs { get; set; }
    public string? Description { get; set; }
    public string? ProcessType { get; set; }
    public ICollection<ItemTagDetailDto> TagDetails { get; set; } = new List<ItemTagDetailDto>();
}

public class ItemTagDetailDto
{
    public int Id { get; set; }
    public string? TagNo { get; set; }
    public string? HeatNo { get; set; }
    public string? MillRef { get; set; }
    public int? Qty { get; set; }
    public decimal? ThicknessIn { get; set; }
    public string? Size { get; set; }
    public string? Location { get; set; }
}
