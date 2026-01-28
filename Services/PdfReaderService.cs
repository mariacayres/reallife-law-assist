using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // ✅ ESSENCIAL
using UglyToad.PdfPig;

namespace RealLifeLawAssist.Services
{
    public class PdfReaderService
    {
        public IEnumerable<string> GetPdfFiles(string folder = "pdfs")
        {
            if (!Directory.Exists(folder))
                return Array.Empty<string>();

            return Directory.GetFiles(folder, "*.pdf");
        }

        public string ExtractTextFromPdf(string pdfPath)
        {
            using var document = PdfDocument.Open(pdfPath);

            return string.Join(
                Environment.NewLine,
                document.GetPages().Select(p => p.Text)
            );
        }
    }
}
