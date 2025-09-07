using PdfParserTest.Models;
using PdfParserTest.Parsing.Strategies;
using System;
using System.Collections.Generic;
using System.IO;

namespace PdfParserTest.Parsing
{
    public sealed class ParsingEngine
    {
        private readonly IEnumerable<IPdfParseStrategy> _strategies;
        private readonly ITextParser _textParser;
        public ParsingEngine(IEnumerable<IPdfParseStrategy> strategies, ITextParser textParser)
        { _strategies = strategies; _textParser = textParser; }

        public PickingListDto Parse(Stream pdf)
        {
            foreach (var strat in _strategies)
            {
                if (!strat.CanHandle()) continue;
                var text = strat.TryGetText(pdf);
                if (!string.IsNullOrWhiteSpace(text))
                    return _textParser.ParseFromPlainText(text!);
            }
            throw new InvalidOperationException("Unable to extract text via PdfPig, Poppler, or OCR.");
        }
    }
}
