namespace PdfParserTest.Parsing
{
    public sealed class ParsingProfile
    {
        public string[] PageCruftStarts { get; init; } =
        {
            "MAX SKID WEIGHT", "RECEIVING HOURS", "Coil to be packaged",
            "MUST WRITE DOWN LINEAL FOOTAGE", "-", "PG "
        };
        public string[] Anchors { get; init; } =
        {
            "PICKING LIST","PRINT DATE/TIME:","PULLED BY","TOTAL WT",
            "PICKING GROUP","BUYER","SHIP DATE","PURCHASE ORDER","ORDER DATE",
            "JOB NAME","SALES REP","SHIP VIA","SOLD TO","SHIP TO","FOB POINT","ROUTE","TERMS",
            "LINE ","TAG #","TAG:","SOURCE:","Other Reservations:"
        };
    }
}
