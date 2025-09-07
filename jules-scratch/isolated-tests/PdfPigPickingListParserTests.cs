using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PickingListApp.DTOs;
using PickingListApp.Services;
using Xunit;

namespace PickingListApp.Tests;

public class PdfPigPickingListParserTests
{
    private readonly string _samplePdfPath = "/app/Samples/15355234.pdf";
    private readonly string _sampleWithTagsPdfPath = "/app/Samples/sales order sample 1.pdf";

    private async Task<PickingListDto> ParseFile(string path)
    {
        var parser = new PdfPigPickingListParser();
        await using var stream = File.OpenRead(path);
        return await parser.Parse(stream);
    }

    // --- Tests for 15355234.pdf ---
    [Fact] public async Task Parse_Sample1_ParsesSalesOrderNumber_Correctly() => Assert.Equal("39053467", (await ParseFile(_samplePdfPath)).SalesOrderNumber);
    [Fact] public async Task Parse_Sample1_ParsesPrintDateTime_Correctly() => Assert.Equal(new DateTime(2025, 8, 28, 15, 35, 0), (await ParseFile(_samplePdfPath)).PrintDateTime);
    [Fact] public async Task Parse_Sample1_ParsesBuyer_Correctly() => Assert.Equal("KIM OWENS-MCLACHLAN", (await ParseFile(_samplePdfPath)).Buyer);
    [Fact] public async Task Parse_Sample1_ParsesShipDate_Correctly() => Assert.Equal(new DateTime(2025, 9, 3), (await ParseFile(_samplePdfPath)).ShipDate);
    [Fact] public async Task Parse_Sample1_ParsesPurchaseOrderNumber_Correctly() => Assert.Equal("242907", (await ParseFile(_samplePdfPath)).PurchaseOrderNumber);
    [Fact] public async Task Parse_Sample1_ParsesOrderDate_Correctly() => Assert.Equal(new DateTime(2025, 8, 28), (await ParseFile(_samplePdfPath)).OrderDate);
    [Fact] public async Task Parse_Sample1_ParsesJobName_Correctly() => Assert.Equal("DYLAN WILLIAMS", (await ParseFile(_samplePdfPath)).JobName);
    [Fact] public async Task Parse_Sample1_ParsesSalesRep_Correctly() => Assert.Equal("", (await ParseFile(_samplePdfPath)).SalesRep);
    [Fact] public async Task Parse_Sample1_ParsesShipVia_Correctly() => Assert.Equal("TRUCK", (await ParseFile(_samplePdfPath)).ShipVia);
    [Fact] public async Task Parse_Sample1_ParsesSoldToName_Correctly() => Assert.Equal("PRECISION METALS LTD.", (await ParseFile(_samplePdfPath)).SoldTo.Name);
    [Fact] public async Task Parse_Sample1_ParsesSoldToEmail_Correctly() => Assert.Equal("PAYABLES@PRECISIONMETALS.CA", (await ParseFile(_samplePdfPath)).SoldTo.Email);
    [Fact] public async Task Parse_Sample1_ParsesSoldToAddress_Correctly() => Assert.Equal("", (await ParseFile(_samplePdfPath)).SoldTo.AddressLine);
    [Fact] public async Task Parse_Sample1_ParsesShipToAddress_Correctly() => Assert.Equal("8075-132ND STREET", (await ParseFile(_samplePdfPath)).ShipTo.AddressLine);
    [Fact] public async Task Parse_Sample1_ParsesSharedCity_Correctly() => Assert.Equal("SURREY", (await ParseFile(_samplePdfPath)).ShipTo.City);
    [Fact] public async Task Parse_Sample1_ParsesSharedProvince_Correctly() => Assert.Equal("BC", (await ParseFile(_samplePdfPath)).ShipTo.Province);
    [Fact] public async Task Parse_Sample1_ParsesSharedPostalCode_Correctly() => Assert.Equal("V3W4N5", (await ParseFile(_samplePdfPath)).ShipTo.PostalCode);
    [Fact] public async Task Parse_Sample1_ParsesCorrectNumberOfLineItems() => Assert.Single((await ParseFile(_samplePdfPath)).Items);
    [Fact]
    public async Task Parse_Sample1_ParsesFirstLineItemData_Correctly()
    {
        var firstItem = (await ParseFile(_samplePdfPath)).Items.First();
        Assert.NotNull(firstItem);
        Assert.Equal(1, firstItem.LineNo);
        Assert.Equal(1990, firstItem.Quantity);
        Assert.Equal("LBS", firstItem.QuantityUnit);
        Assert.Equal("PP2448IO/C", firstItem.ItemCode);
        Assert.Equal(15.875m, firstItem.WidthIn);
        Assert.Null(firstItem.LengthIn);
        Assert.Equal(1990, firstItem.WeightLbs);
        Assert.Contains("24 GA(.0245) X48\" DB IRON ORE/CHARCOAL", firstItem.Description);
    }

    // --- Tests for sales order sample 1.pdf ---
    [Fact]
    public async Task Parse_Sample2_ParsesProcessType_Correctly()
    {
        var firstItem = (await ParseFile(_sampleWithTagsPdfPath)).Items.First();
        Assert.Equal("CTL", firstItem.ProcessType);
        Assert.DoesNotContain("CTL -", firstItem.Description);
    }

    [Fact]
    public async Task Parse_Sample2_FindsOneTagDetail()
    {
        var firstItem = (await ParseFile(_sampleWithTagsPdfPath)).Items.First();
        Assert.Single(firstItem.TagDetails);
        Assert.DoesNotContain("TAG #", firstItem.Description);
    }

    [Fact]
    public async Task Parse_Sample2_ParsesTagDetailCorrectly()
    {
        var tag = (await ParseFile(_sampleWithTagsPdfPath)).Items.First().TagDetails.First();
        Assert.Equal("39331071", tag.TagNo);
        Assert.Equal("N/A", tag.HeatNo);
        Assert.Equal("LD2250227101F-1", tag.MillRef);
        Assert.Equal(2240, tag.Qty);
        Assert.Equal(0.04000m, tag.ThicknessIn);
        Assert.Equal("48\"", tag.Size);
        Assert.Equal("U51", tag.Location);
    }
}
