using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using RealLifeLawAssist.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // --- Carregar configuração YAML ---
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = System.IO.File.ReadAllText("config.yml");
            var config = deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(yaml);

            var apiKey = config["googleApi"]["apiKey"];
            var model = config["googleApi"]["model"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("Erro: API Key não definida no config.yml.");
                return;
            }

            // --- Inicializar serviços ---
            using var httpClient = new HttpClient();
            var geminiService = new GeminiService(httpClient, apiKey, model);
            var pdfService = new PdfReaderService();

            // --- Ler PDFs ---
            var pdfFiles = pdfService.GetPdfFiles()?.ToList() ?? new List<string>();
            if (pdfFiles.Count == 0)
            {
                Console.WriteLine("Nenhum PDF encontrado. Saindo...");
                return;
            }

            Console.WriteLine("Digite o prompt para validação do conteúdo:");
            var prompt = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                Console.WriteLine("Prompt vazio. Saindo...");
                return;
            }

            // --- Processar cada PDF ---
            foreach (var pdfPath in pdfFiles)
            {
                try
                {
                    Console.WriteLine($"\n=== Processando PDF: {pdfPath} ===");
                    var pdfText = pdfService.ExtractTextFromPdf(pdfPath);

                    if (string.IsNullOrWhiteSpace(pdfText))
                    {
                        Console.WriteLine("PDF vazio ou sem texto extraível. Pulando...");
                        continue;
                    }

                    var result = await geminiService.GenerateContentAsync(pdfText, prompt);

                    Console.WriteLine("\n--- RESPOSTA DO GEMINI ---\n");
                    Console.WriteLine(result);
                }
                catch (Exception exPdf)
                {
                    Console.WriteLine($"Erro ao processar PDF {pdfPath}: {exPdf.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro geral: " + ex.Message);
        }
    }
}
