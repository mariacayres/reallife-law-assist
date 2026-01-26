using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UglyToad.PdfPig;

namespace RealLifeLawAssist.Services
{
    /// <summary>
    /// Serviço responsável por encontrar e extrair texto de arquivos PDF.
    /// </summary>
    public class PdfReaderService
    {
        private readonly string _pdfFolder;

        /// <summary>
        /// Inicializa uma nova instância do <see cref="PdfReaderService"/>.
        /// Determina o caminho para a pasta "pdfs" na raiz do projeto.
        /// </summary>
        public PdfReaderService()
        {
            // Este código determina o caminho da pasta "pdfs" de forma relativa à localização do executável.
            // Sobe 3 níveis a partir da pasta de compilação (ex: bin/Debug/netX.X) para chegar à raiz do projeto.
            var projectRoot = Directory.GetParent(AppContext.BaseDirectory)?
                           .Parent?.Parent?.Parent?.FullName; // sobe 3 níveis

            if (string.IsNullOrEmpty(projectRoot))
                // Lança uma exceção mais específica e antes de usar a variável nula.
                throw new DirectoryNotFoundException("Não foi possível determinar a raiz do projeto para encontrar a pasta de PDFs.");

            _pdfFolder = Path.Combine(projectRoot, "pdfs");
            Console.WriteLine($"Procurando PDFs na pasta: {_pdfFolder}");
        }

        /// <summary>
        /// Procura e retorna os caminhos de todos os arquivos .pdf na pasta configurada.
        /// </summary>
        /// <returns>Uma coleção de strings, onde cada string é o caminho completo para um arquivo PDF.</returns>
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

        /// <summary>
        /// Extrai todo o texto de um arquivo PDF especificado.
        /// </summary>
        /// <param name="pdfPath">O caminho completo para o arquivo PDF.</param>
        /// <returns>Uma string contendo todo o texto extraído do documento.</returns>
        /// <exception cref="FileNotFoundException">Lançada se o arquivo PDF não for encontrado no caminho especificado.</exception>
        public string ExtractTextFromPdf(string pdfPath)
        {
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException("PDF não encontrado.", pdfPath);

            using var pdf = PdfDocument.Open(pdfPath);
            // Usa StringBuilder para uma concatenação de strings eficiente dentro do loop.
            var textBuilder = new StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                textBuilder.Append(page.Text);
                textBuilder.AppendLine(); // Adiciona o texto da página e uma nova linha.
            }

            return textBuilder.ToString();
        }
    }
}
