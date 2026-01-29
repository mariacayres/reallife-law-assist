using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // ✅ ESSENCIAL
using UglyToad.PdfPig;

namespace RealLifeLawAssist.Services
{
    public class PdfReaderService
    {
        public IEnumerable<string> GetPdfFiles(string folder = null)
        {
            // Se não especificado, usa pasta pdfs na raiz do projeto
            if (string.IsNullOrEmpty(folder))
            {
                var projectRoot = Directory.GetParent(AppContext.BaseDirectory)?
                               .Parent?.Parent?.Parent?.FullName;
                folder = Path.Combine(projectRoot ?? "", "pdfs");
                
                // Cria a pasta se não existir
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    Console.WriteLine($"Pasta criada: {folder}");
                }
            }
            
            Console.WriteLine($"Procurando PDFs na pasta: {folder}");
            
            if (!Directory.Exists(folder))
                return Array.Empty<string>();

            var files = Directory.GetFiles(folder, "*.pdf");
            Console.WriteLine($"Encontrados {files.Length} arquivos PDF.");
            return files;
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
