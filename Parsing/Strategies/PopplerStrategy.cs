using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace PdfParserTest.Parsing.Strategies
{
    public sealed class PopplerStrategy : IPdfParseStrategy
    {
        private readonly string _popplerPath;
        public PopplerStrategy(IConfiguration cfg)
            => _popplerPath = cfg["Parsing:PopplerPath"] ?? "";

        public bool CanHandle()
            => !string.IsNullOrWhiteSpace(FindExe("pdftotext"));

        public string? TryGetText(Stream pdf)
        {
            var exe = FindExe("pdftotext");
            if (exe is null) return null;

            var tmpPdf = Path.GetTempFileName() + ".pdf";
            var tmpTxt = Path.ChangeExtension(tmpPdf, ".txt");
            try
            {
                using (var fs = File.Create(tmpPdf)) { pdf.Position = 0; pdf.CopyTo(fs); }
                var psi = new ProcessStartInfo(exe, $"-layout -nopgbrk \"{tmpPdf}\" \"{tmpTxt}\"")
                { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
                using var p = Process.Start(psi)!;
                p.WaitForExit(20000);
                if (!File.Exists(tmpTxt)) return null;
                var txt = File.ReadAllText(tmpTxt);
                return string.IsNullOrWhiteSpace(txt) ? null : txt;
            }
            finally { TryDelete(tmpPdf); TryDelete(tmpTxt); }
        }

        private string? FindExe(string name)
        {
            if (!string.IsNullOrWhiteSpace(_popplerPath))
            {
                var path = Path.Combine(_popplerPath, name + (OperatingSystem.IsWindows() ? ".exe" : ""));
                if (File.Exists(path)) return path;
            }
            return name;
        }
        private static void TryDelete(string p) { try { if (File.Exists(p)) File.Delete(p); } catch { } }
    }
}
