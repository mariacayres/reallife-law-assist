﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using RealLifeLawAssist.Services;
using RealLifeLawAssist.Configuration;

try
{
    // --- 1. Inicialização dos Serviços ---
    using var geminiService = new GeminiService();
    var pdfService = new PdfReaderService();
    var config = new ConfigEnv(); // ✅ variável local, não campo

    // --- 2. Leitura e Validação dos PDFs ---
    var pdfFiles = pdfService.GetPdfFiles()?.ToList() ?? new List<string>();
    if (pdfFiles.Count == 0)
    {
        Console.WriteLine("Nenhum PDF encontrado.");
        return;
    }

    string prompt;

    if (config.Validating)
    {
        prompt =
            "1. Estrutura Obrigatória: Definição clara do objeto e fixação do Preço Base (limite máximo). " +
            "Distinção entre cláusulas fixas (adesão obrigatória) e aspetos variáveis (sujeitos à concorrência/avaliação). " +
            "Definição das regras de execução: prazos, garantias e penalidades por incumprimento. " +
            "2. Critérios de Avaliação de Risco: " +
            "Risco Alto (Nota 1-2) - Indícios de Favorecimento: Especificações à medida, prazos impossíveis, bloqueio de marca. " +
            "Risco Baixo (Nota 4-5) - Boas Práticas: Descritivo funcional, realismo de mercado, rigor na execução.";
    }
    else
    {
        Console.WriteLine("Digite o prompt para validação do conteúdo:");
        prompt = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(prompt))
        {
            Console.WriteLine("Prompt vazio. Saindo...");
            return;
        }
    }

    // --- 3. Processamento dos PDFs ---
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
    Console.WriteLine($"Error: {ex.Message}");
}
