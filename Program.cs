﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using RealLifeLawAssist.Services;
using RealLifeLawAssist.Configuration;

{
    // O método Main é o ponto de entrada da aplicação.
    // É declarado como 'async Task' para permitir o uso de chamadas assíncronas (await).
    static async Task Main(string[] args)
    {
        try
        {
            // --- 1. Inicialização dos Serviços ---
            // Instanciamos os serviços que a nossa aplicação irá usar.
            // O 'using' garante que o método 'Dispose' do GeminiService será chamado no final,
            // libertando recursos como a conexão HttpClient.
            using var geminiService = new GeminiService();
            var pdfService = new PdfReaderService();

            // --- 2. Leitura e Validação dos PDFs ---
            // Obtém a lista de todos os arquivos .pdf na pasta designada.
            var pdfFiles = pdfService.GetPdfFiles()?.ToList() ?? new List<string>();
            if (pdfFiles.Count == 0)
            {
                return;
            }

            // --- 3. Interação com o Utilizador ---
            // Pede ao utilizador para inserir o comando (prompt) que será usado para analisar os PDFs.
            Console.WriteLine("Digite o prompt para validação do conteúdo:");
            var prompt = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                Console.WriteLine("Prompt vazio. Saindo...");
                return;
            }

            // --- 4. Processamento em Loop ---
            // Itera sobre cada arquivo PDF encontrado.
            foreach (var pdfPath in pdfFiles)
            {
                try
                {
                    // Extrai o texto do PDF.
                    Console.WriteLine($"\n=== Processando PDF: {pdfPath} ===");
                    var pdfText = pdfService.ExtractTextFromPdf(pdfPath);

                    if (string.IsNullOrWhiteSpace(pdfText))
                    {
                        Console.WriteLine("PDF vazio ou sem texto extraível. Pulando...");
                        continue;
                    }

                    // Envia o texto extraído e o prompt para a API do Gemini.
                    var result = await geminiService.GenerateContentAsync(pdfText, prompt);

                    // Imprime o resultado retornado pela IA.
                    Console.WriteLine("\n--- RESPOSTA DO GEMINI ---\n");
                    Console.WriteLine(result);
                }
                catch (Exception exPdf)
                {
                    // Captura erros específicos do processamento de um único PDF,
                    // permitindo que a aplicação continue para o próximo.
                    Console.WriteLine($"Erro ao processar PDF {pdfPath}: {exPdf.Message}");
                }
            }
        }
        catch (Exception ex) // Captura erros gerais da aplicação (ex: falha ao carregar config, erro de rede irrecuperável).
        {
            Console.WriteLine("Erro geral: " + ex.Message);
        }
    }
}
