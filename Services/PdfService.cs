using System;
using System.Collections.Generic;
using System.IO;
using UglyToad.PdfPig;

namespace RealLifeLawAssist.Services
{
    public class PdfReaderService
    {
        private readonly string _pdfFolder;

        public PdfReaderService()
        {
            // Pasta "pdfs" na raiz do projeto
            var projectRoot = Directory.GetParent(AppContext.BaseDirectory)
                           .Parent?.Parent?.Parent?.FullName; // sobe 3 níveis
            _pdfFolder = Path.Combine(projectRoot, "pdfs");

            if (string.IsNullOrEmpty(projectRoot))
                throw new Exception("Não foi possível determinar a raiz do projeto.");

            Console.WriteLine($"Procurando PDFs na pasta: {_pdfFolder}");
        }

        // Retorna todos os arquivos PDF da pasta
        public IEnumerable<string> GetPdfFiles()
        {
            if (!Directory.Exists(_pdfFolder))
            {
                Console.WriteLine($"Pasta {_pdfFolder} não encontrada.");
                return Array.Empty<string>();
            }

            var pdfFiles = Directory.GetFiles(_pdfFolder, "*.pdf");
            if (pdfFiles.Length == 0)
            {
                Console.WriteLine("Nenhum PDF encontrado. Saindo...");
            }

            return pdfFiles;
        }

        // Extrai o texto de um PDF específico
        public string ExtractTextFromPdf(string pdfPath)
        {
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException("PDF não encontrado.", pdfPath);

            using var pdf = PdfDocument.Open(pdfPath);
            var text = "";

            foreach (var page in pdf.GetPages())
            {
                text += page.Text + "\n";
            }

            return text;
        }
    }
}
