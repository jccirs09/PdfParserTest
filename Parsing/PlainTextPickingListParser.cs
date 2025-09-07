using PdfParserTest.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using System.Linq;

namespace PdfParserTest.Parsing
{
    public sealed class PlainTextPickingListParser : ITextParser
    {
        // Header
        private static readonly Regex SoRx = new(@"PICKING\s*LIST\s*No\.\s*(?<so>\d+)", RegexOptions.IgnoreCase);
        private static readonly Regex PrintDtRx = new(@"PRINT\s*DATE/TIME:\s*(?<dt>\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2})", RegexOptions.IgnoreCase);
        private static readonly Regex ShipDateRx = new(@"\bSHIP\s*DATE\s*(?<d>\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase);
        private static readonly Regex OrderDateRx = new(@"\bORDER\s*DATE\s*(?<d>\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase);
        private static readonly Regex BuyerRx = new(@"\bBUYER\s+(?<buyer>.+?)\s+(SHIP\s*DATE|PURCHASE\s*ORDER|ORDER\s*DATE)", RegexOptions.IgnoreCase);
        private static readonly Regex PoRx = new(@"PURCHASE\s*ORDER\s*#\s*(?<po>[A-Za-z0-9\-]+)", RegexOptions.IgnoreCase);
        private static readonly Regex SalesRepRx = new(@"\bSALES\s*REP\s+(?<rep>.+?)\s+SHIP\s*VIA", RegexOptions.IgnoreCase);
        private static readonly Regex ShipViaRx = new(@"\bSHIP\s*VIA\s+(?<via>[A-Z][A-Z ]+)", RegexOptions.IgnoreCase);
        private static readonly Regex TermsRx = new(@"\bNET\s+\d+\s+DAYS\b", RegexOptions.IgnoreCase);

        // Items (PCS or LBS, dot in ItemCode allowed)
        private static readonly Regex LineHeaderRx = new(
            @"^\s*(?<no>\d+)\s+(?<qty>\d{1,3}(?:,\d{3})*|\d+)\s+(?<unit>PCS|LBS)\s+(?<staged>[_\d,]+)\s+(?<code>[A-Z0-9\-.]+)\s+(?<width>\d+(?:\.\d+)?)""(?:\s+(?<length>\d+(?:\.\d+)?)"")?\s+(?<weight>\d{1,3}(?:,\d{3})*(?:\.\d+)?)\b",
            RegexOptions.IgnoreCase);

        // TAG table
        private static readonly Regex TagHeaderRx = new(@"TAG\s*#\s+HEAT\s*#", RegexOptions.IgnoreCase);
        private static readonly Regex TagRowRx = new(
            @"^\s*(?<tag>\d+)\s+(?<heat>[A-Za-z0-9\-]+)\s+(?<mill>[A-Za-z0-9\-]+)\s+(?<qty>\d{1,3}(?:,\d{3})*(?:\.\d+)?)\s*(?<qunit>PCS|LBS)?\s+(?<thk>\d+(?:\.\d+)?)""?\s+(?<size>\d+""\s*X\s*\d+"")\s+(?<loc>[A-Za-z0-9\-]+)\s*$",
            RegexOptions.IgnoreCase);

        private static readonly Regex PageHeaderRx = new(@"^PICKING\s*LIST\s*No\.|^PRINT\s*DATE/TIME:|^\s*PG\s+\d+\s+OF\s+\d+", RegexOptions.IgnoreCase);
        private static readonly Regex OtherResHeaderRx = new(@"^Other Reservations:", RegexOptions.IgnoreCase);
        private static readonly Regex TrailerTotalRx = new(@"(?m)^\s*(?<total>\d{1,3}(?:,\d{3})*(?:\.\d+)?)\s*LBS\s*$", RegexOptions.IgnoreCase);

        public PickingListDto ParseFromPlainText(string raw)
        {
            var lines = Normalize(raw);

            var dto = new PickingListDto
            {
                SalesOrderNumber = SoRx.Match(raw).Groups["so"].Value.Trim()
            };
            if (TryDateTime(PrintDtRx, raw, out var pdt)) dto.PrintDateTime = pdt;
            if (TryDate(ShipDateRx, raw, out var sd)) dto.ShipDate = sd;
            if (TryDate(OrderDateRx, raw, out var od)) dto.OrderDate = od;
            dto.Buyer = BuyerRx.Match(raw).Groups["buyer"].Value.Trim();
            dto.PurchaseOrderNumber = PoRx.Match(raw).Groups["po"].Value.Trim();
            dto.SalesRep = SalesRepRx.Match(raw).Groups["rep"].Value.Trim();
            dto.ShipVia = ShipViaRx.Match(raw).Groups["via"].Value.Trim();
            dto.Terms = TermsRx.Match(raw).Value.Trim();
            dto.SoldTo = new(); dto.ShipTo = new();

            for (int i = 0; i < lines.Count; i++)
            {
                var m = LineHeaderRx.Match(lines[i]);
                if (!m.Success) continue;

                var item = new PickingListItemDto
                {
                    LineNo = ParseInt(m.Groups["no"].Value),
                    Quantity = ParseInt(m.Groups["qty"].Value),
                    QuantityUnit = m.Groups["unit"].Value.ToUpperInvariant(),
                    QuantityStaged = ParseIntOrNull(m.Groups["staged"].Value),
                    ItemCode = m.Groups["code"].Value.Trim(),
                    WidthIn = ParseDecOrNull(m.Groups["width"].Value),
                    LengthIn = ParseDecOrNull(m.Groups["length"].Value),
                    WeightLbs = ParseDecOrNull(m.Groups["weight"].Value),
                    TagDetails = new()
                };

                int j = i + 1;
                while (j < lines.Count && (IsCruft(lines[j]) || lines[j].Equals("CTL", StringComparison.OrdinalIgnoreCase)))
                {
                    if (lines[j].Equals("CTL", StringComparison.OrdinalIgnoreCase)) item.ProcessType ??= "CTL";
                    j++;
                }
                if (j < lines.Count && lines[j].StartsWith("TAG:", StringComparison.OrdinalIgnoreCase)) j++;
                if (j < lines.Count && OtherResHeaderRx.IsMatch(lines[j])) j = SkipOtherReservations(lines, j);

                var desc = new List<string>();
                for (; j < lines.Count; j++)
                {
                    if (LineHeaderRx.IsMatch(lines[j]) || TagHeaderRx.IsMatch(lines[j])
                        || lines[j].StartsWith("SOURCE:", StringComparison.OrdinalIgnoreCase) || PageHeaderRx.IsMatch(lines[j]))
                        break;

                    if (!lines[j].StartsWith("TAG:", StringComparison.OrdinalIgnoreCase)) desc.Add(lines[j].Trim());
                }
                if (desc.Count > 0) item.Description = string.Join(" ", desc);
                if (item.ProcessType is null)
                {
                    if (item.Description?.Contains("SHEET", StringComparison.OrdinalIgnoreCase) == true) item.ProcessType = "SHEET STOCK";
                    else if (item.Description?.Contains("CTL", StringComparison.OrdinalIgnoreCase) == true) item.ProcessType = "CTL";
                    else if (item.Description?.Contains("SLIT", StringComparison.OrdinalIgnoreCase) == true) item.ProcessType = "SLITTER";
                }

                if (j < lines.Count && TagHeaderRx.IsMatch(lines[j]))
                {
                    j++;
                    for (; j < lines.Count; j++)
                    {
                        if (LineHeaderRx.IsMatch(lines[j]) || lines[j].StartsWith("SOURCE:", StringComparison.OrdinalIgnoreCase) || PageHeaderRx.IsMatch(lines[j])) break;
                        var tr = TagRowRx.Match(lines[j]);
                        if (!tr.Success) { if (string.IsNullOrWhiteSpace(lines[j])) break; else continue; }
                        item.TagDetails.Add(new ItemTagDetailDto
                        {
                            TagNo = tr.Groups["tag"].Value.Trim(),
                            HeatNo = tr.Groups["heat"].Value.Trim(),
                            MillRef = tr.Groups["mill"].Value.Trim(),
                            Qty = ParseIntOrNull(tr.Groups["qty"].Value),
                            QtyUnit = tr.Groups["qunit"].Success ? tr.Groups["qunit"].Value.ToUpperInvariant() : null,
                            ThicknessIn = ParseDecOrNull(tr.Groups["thk"].Value),
                            Size = tr.Groups["size"].Value.Trim().Replace(" ", ""),
                            Location = tr.Groups["loc"].Value.Trim()
                        });
                    }
                }

                dto.Items.Add(item);
                i = j - 1;
            }

            var tm = TrailerTotalRx.Match(raw);
            if (tm.Success && ParseDecOrNull(tm.Groups["total"].Value) is decimal tot) dto.TotalWeightLbs = tot;

            return dto;
        }

        private static List<string> Normalize(string text)
        {
            text = text.Replace('\u201C', '"').Replace('\u201D', '"')
                       .Replace("\r\n", "\n").Replace("\r", "\n");
            text = Regex.Replace(text, @"[ \t]+", " ");
            text = Regex.Replace(text, @"(?<=\d)\s+(?=\d)", "");

            foreach (var a in new ParsingProfile().Anchors)
                text = Regex.Replace(text, @"(?=" + Regex.Escape(a) + @")", "\n");
            text = Regex.Replace(text, @"-{6,}|\*{6,}", "\n");

            var lines = text.Split('\n', StringSplitOptions.TrimEntries).ToList();
            lines.RemoveAll(IsCruft);
            return lines;
        }
        private static bool IsCruft(string l)
            => string.IsNullOrWhiteSpace(l) || PageHeaderRx.IsMatch(l)
               || l.StartsWith("MAX SKID WEIGHT", StringComparison.OrdinalIgnoreCase)
               || l.StartsWith("RECEIVING HOURS", StringComparison.OrdinalIgnoreCase)
               || l.StartsWith("Coil to be packaged", StringComparison.OrdinalIgnoreCase)
               || l.StartsWith("MUST WRITE DOWN LINEAL FOOTAGE", StringComparison.OrdinalIgnoreCase)
               || l.StartsWith("-");

        private static int SkipOtherReservations(List<string> lines, int start)
        {
            int i = start + 1;
            for (; i < lines.Count; i++)
                if (string.IsNullOrWhiteSpace(lines[i]) || lines[i].StartsWith("SOURCE:", StringComparison.OrdinalIgnoreCase)
                    || LineHeaderRx.IsMatch(lines[i]) || TagHeaderRx.IsMatch(lines[i]) || PageHeaderRx.IsMatch(lines[i]))
                    break;
            return i;
        }

        private static bool TryDate(Regex rx, string text, out DateTime? dt)
        {
            var m = rx.Match(text);
            if (m.Success && DateTime.TryParse(m.Groups["d"].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) { dt = d; return true; }
            dt = null; return false;
        }
        private static bool TryDateTime(Regex rx, string text, out DateTime? dt)
        {
            var m = rx.Match(text);
            if (m.Success && DateTime.TryParse(m.Groups["dt"].Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var d)) { dt = d; return true; }
            dt = null; return false;
        }
        private static int ParseInt(string s) => int.Parse(s.Replace(",", ""), NumberStyles.Integer, CultureInfo.InvariantCulture);
        private static int? ParseIntOrNull(string s)
        {
            s = s.Replace(",", "");
            if (string.IsNullOrWhiteSpace(s) || s.Contains("_")) return null;
            if (s.Contains('.')) return (int?)Math.Round(decimal.Parse(s, CultureInfo.InvariantCulture), MidpointRounding.AwayFromZero);
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;
        }
        private static decimal? ParseDecOrNull(string s)
        {
            s = s.Replace(" ", "").Replace(",", "").Replace("\"", "");
            return decimal.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var v) ? v : (decimal?)null;
        }
    }
}
