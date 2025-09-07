using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PickingListApp.DTOs;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace PickingListApp.Services;

public class PdfPigPickingListParser : IPickingListParser
{
    // Regex
    private static readonly Regex SoNumberRegex = new(@"\bNo\.\s*(?<num>[0-9A-Z\-]+)", RegexOptions.Compiled);
    private static readonly Regex PrintDateTimeRegex = new(@"PRINT DATE/TIME:\s+(?<dt>.+)", RegexOptions.Compiled);
    private static readonly Regex DateRegex = new(@"\d{2}/\d{2}/\d{4}", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", RegexOptions.Compiled);
    private static readonly Regex PostalCodeRegex = new(@"\b[A-Z]\d[A-Z]\s?\d[A-Z]\d\b", RegexOptions.Compiled);
    private static readonly Regex ItemLineStartRegex = new(@"^\s*\d+\s+[\d,]+\s+(LBS|PCS)", RegexOptions.Compiled);
    private static readonly Regex TagDetailRowRegex = new(
        @"^\s*(?<tag>\S+)\s+(?<heat>\S+)\s+(?<mill>\S+)\s+(?<qty>[\d,]+)\s+LBS\s+(?<thk>[\d\.]+)" + "\"" + @"\s+(?<size>[\d\." + "\"" + @"\sX]+?)\s{2,}(?<loc>\S+)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);


    public Task<PickingListDto> Parse(Stream pdfStream)
    {
        var pickingList = new PickingListDto();
        var state = new ParserState();

        using (var document = PdfDocument.Open(pdfStream))
        {
            for (var i = 0; i < document.NumberOfPages; i++)
            {
                var page = document.GetPage(i + 1);
                var text = ContentOrderTextExtractor.GetText(page);
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    if (state.IsInItemSection) { ParseItemSectionLine(line, state, pickingList); continue; }
                    if (state.AddressLinesToRead > 0) { ParseAddressLine(line, state); continue; }
                    if (state.ExpectingBuyerAndShipDate) { ParseBuyerAndShipDateLine(line, pickingList); state.ExpectingBuyerAndShipDate = false; continue; }
                    if (state.ExpectingPoAndOrderDate) { ParsePoAndOrderDateLine(line, pickingList); state.ExpectingPoAndOrderDate = false; continue; }
                    if (state.ExpectingJobSalesRepShipVia) { ParseJobSalesRepShipViaLine(line, pickingList); state.ExpectingJobSalesRepShipVia = false; continue; }

                    if (TryParseHeader(line, pickingList)) continue;

                    if (line.Contains("LINE") && line.Contains("QUANTITY") && line.Contains("DESCRIPTION")) { state.IsInItemSection = true; }
                    else if (line.Contains("SOLD TO") && line.Contains("SHIP TO")) { state.AddressLinesToRead = 3; state.SoldToPos = line.IndexOf("SOLD TO"); state.ShipToPos = line.IndexOf("SHIP TO"); state.ShipViaPos = line.IndexOf("SHIP VIA"); }
                    else if (line.Contains("PICKING GROUP") && line.Contains("BUYER")) { state.ExpectingBuyerAndShipDate = true; }
                    else if (line.Contains("PURCHASE ORDER #") && line.Contains("ORDER DATE")) { state.ExpectingPoAndOrderDate = true; }
                    else if (line.Contains("JOB NAME") && line.Contains("SALES REP")) { state.ExpectingJobSalesRepShipVia = true; }
                }
            }
        }

        if (state.CurrentItem != null) pickingList.Items.Add(state.CurrentItem);

        pickingList.SoldTo = ParseParty(state.SoldToLines);
        pickingList.ShipTo = ParseParty(state.ShipToLines);

        if (!string.IsNullOrEmpty(pickingList.SoldTo.City) && string.IsNullOrEmpty(pickingList.ShipTo.City))
        {
            pickingList.ShipTo.City = pickingList.SoldTo.City;
            pickingList.ShipTo.Province = pickingList.SoldTo.Province;
            pickingList.ShipTo.PostalCode = pickingList.SoldTo.PostalCode;
        }

        return Task.FromResult(pickingList);
    }

    private void ParseItemSectionLine(string line, ParserState state, PickingListDto pickingList)
    {
        var trimmedLine = line.Trim();
        if (state.IsInTagTable)
        {
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("SOURCE:"))
            {
                state.IsInTagTable = false;
                return;
            }
            ParseTagDetailLine(trimmedLine, state);
            return;
        }

        if (ItemLineStartRegex.IsMatch(trimmedLine))
        {
            if (state.CurrentItem != null) pickingList.Items.Add(state.CurrentItem);
            state.CurrentItem = new PickingListItemDto();
            ParseNewItemLine(trimmedLine, state.CurrentItem);
        }
        else if (state.CurrentItem != null)
        {
            if (trimmedLine == "CTL -") { state.CurrentItem.ProcessType = "CTL"; }
            else if (trimmedLine.Contains("TAG #") && trimmedLine.Contains("HEAT #")) { state.IsInTagTable = true; }
            else { state.CurrentItem.Description = $"{state.CurrentItem.Description}\n{trimmedLine}".Trim(); }
        }
    }

    private void ParseTagDetailLine(string line, ParserState state)
    {
        if (state.CurrentItem == null) return;
        var match = TagDetailRowRegex.Match(line);
        if (!match.Success) return;

        var tag = new ItemTagDetailDto
        {
            TagNo = match.Groups["tag"].Value,
            HeatNo = match.Groups["heat"].Value,
            MillRef = match.Groups["mill"].Value,
            Size = match.Groups["size"].Value.Trim(),
            Location = match.Groups["loc"].Value
        };

        if (int.TryParse(match.Groups["qty"].Value.Replace(",", ""), out int qty)) tag.Qty = qty;
        if (decimal.TryParse(match.Groups["thk"].Value, out decimal thickness)) tag.ThicknessIn = thickness;

        state.CurrentItem.TagDetails.Add(tag);
    }

    private void ParseNewItemLine(string line, PickingListItemDto item)
    {
        var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0 && int.TryParse(parts[0], out int lineNo)) item.LineNo = lineNo;
        if (parts.Length > 1 && decimal.TryParse(parts[1].Replace(",", ""), out decimal qty)) item.Quantity = (int)qty;
        if (parts.Length > 2) item.QuantityUnit = parts[2];
        if (parts.Length > 4) item.ItemCode = parts[4];
        if (parts.Length > 5 && decimal.TryParse(parts[5].Replace("\"", ""), out decimal width)) item.WidthIn = width;
        if (parts.Length > 6 && decimal.TryParse(parts[6].Replace(",", ""), out decimal weight)) item.WeightLbs = weight;
    }

    private void ParseAddressLine(string line, ParserState state)
    {
        int shipToColEnd = state.ShipViaPos > state.ShipToPos ? state.ShipViaPos : line.Length;
        string soldToText = SafeSubstring(line, state.SoldToPos, state.ShipToPos);
        string shipToText = SafeSubstring(line, state.ShipToPos, shipToColEnd);
        var fobIndex = shipToText.IndexOf("FOB POINT");
        if (fobIndex > -1) shipToText = shipToText.Substring(0, fobIndex);
        state.SoldToLines.Add(soldToText.Trim());
        state.ShipToLines.Add(shipToText.Trim());
        state.AddressLinesToRead--;
    }

    private PartyDto ParseParty(List<string> lines)
    {
        var party = new PartyDto { Name = "", AddressLine = "", City = "", Province = "", PostalCode = "", Email = "" };
        var remainingLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        if (!remainingLines.Any()) return party;
        party.Name = remainingLines.FirstOrDefault() ?? "";
        if(!string.IsNullOrEmpty(party.Name)) remainingLines.RemoveAt(0);
        var emailLine = remainingLines.FirstOrDefault(l => EmailRegex.IsMatch(l));
        if (emailLine != null) { party.Email = EmailRegex.Match(emailLine).Value; remainingLines.Remove(emailLine); }
        var postalCodeLine = remainingLines.FirstOrDefault(l => PostalCodeRegex.IsMatch(l));
        if (postalCodeLine != null)
        {
            var postalMatch = PostalCodeRegex.Match(postalCodeLine);
            party.PostalCode = postalMatch.Value.Replace(" ", "");
            var cityProvText = postalCodeLine.Substring(0, postalMatch.Index).Trim();
            var cityProvParts = cityProvText.Split(',', StringSplitOptions.TrimEntries);
            party.City = cityProvParts.FirstOrDefault() ?? "";
            party.Province = cityProvParts.Length > 1 ? cityProvParts[1] : "";
            remainingLines.Remove(postalCodeLine);
        }
        party.AddressLine = string.Join(" ", remainingLines.Where(l => !string.IsNullOrWhiteSpace(l)));
        return party;
    }

    private bool TryParseHeader(string line, PickingListDto pickingList)
    {
        if (string.IsNullOrEmpty(pickingList.SalesOrderNumber)) { var m = SoNumberRegex.Match(line); if (m.Success) { pickingList.SalesOrderNumber = m.Groups["num"].Value.Trim(); return true; } }
        if (pickingList.PrintDateTime == null) { var m = PrintDateTimeRegex.Match(line); if (m.Success) { if (DateTime.TryParseExact(m.Groups["dt"].Value.Trim(), "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) pickingList.PrintDateTime = d; return true; } }
        return false;
    }

    private void ParseBuyerAndShipDateLine(string line, PickingListDto pickingList) { var m = DateRegex.Match(line); if (m.Success) { if (DateTime.TryParseExact(m.Value, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) pickingList.ShipDate = d; pickingList.Buyer = line.Substring(0, m.Index).Trim(); } }
    private void ParsePoAndOrderDateLine(string line, PickingListDto pickingList) { var m = DateRegex.Match(line); if (m.Success) { if (DateTime.TryParseExact(m.Value, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) pickingList.OrderDate = d; } var p = line.Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries); if(p.Length>0) pickingList.PurchaseOrderNumber=p[0]; }
    private void ParseJobSalesRepShipViaLine(string line, PickingListDto pickingList) { var p=line.Split(' ',StringSplitOptions.RemoveEmptyEntries); if(p.Length<2){pickingList.JobName=line.Trim();pickingList.SalesRep="";pickingList.ShipVia="";return;} pickingList.ShipVia=p.Last();pickingList.JobName=string.Join(" ",p.Take(p.Length-1));pickingList.SalesRep=""; }

    private string SafeSubstring(string text, int start, int? end = null)
    {
        if (start < 0 || start >= text.Length) return "";
        int endPos = end ?? text.Length;
        if (endPos > text.Length) endPos = text.Length;
        int length = endPos - start;
        if (length <= 0) return "";
        return text.Substring(start, length).Trim();
    }

    private class ParserState
    {
        public bool IsInItemSection { get; set; }
        public PickingListItemDto? CurrentItem { get; set; }
        public bool IsInTagTable { get; set; }
        public bool ExpectingBuyerAndShipDate { get; set; }
        public bool ExpectingPoAndOrderDate { get; set; }
        public bool ExpectingJobSalesRepShipVia { get; set; }
        public int AddressLinesToRead { get; set; }
        public int SoldToPos { get; set; }
        public int ShipToPos { get; set; }
        public int ShipViaPos { get; set; }
        public List<string> SoldToLines { get; } = new();
        public List<string> ShipToLines { get; } = new();
    }
}
