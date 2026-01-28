﻿using System;
using System.IO;
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
    var pdfReaderService = new PdfReaderService();
    var pdfOutputService = new PdfService();
    var config = new ConfigEnv();

    // Pasta onde os PDFs de saída serão salvos
    var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "consolidado");
    Directory.CreateDirectory(outputDir);

    // --- 2. Leitura dos PDFs ---
    var pdfFiles = pdfReaderService.GetPdfFiles()?.ToList() ?? new List<string>();
    if (pdfFiles.Count == 0)
    {
        Console.WriteLine("Nenhum PDF encontrado.");
        return;
    }

    string jsonInstruction = @"
    Analise o documento e responda ESTRITAMENTE com um JSON válido (sem markdown ```json) seguindo esta estrutura:
    {
    ""titulo"": ""Titulo do Relatório"",
    ""descricao"": ""Resumo executivo curto"",
    ""objeto"": ""Descrição do objeto do contrato"",
    ""localizacao"": ""Locais de execução"",
    ""totalLinhas"": ""Ex: 12 circuitos"",
    ""precoBase"": 0.0,
    ""custoKm"": 0.0,
    ""kmMax"": 0.0,
    ""vigencia"": ""Ex: 60 dias"",
    ""caucao"": ""Ex: 5%"",
    ""pagamento"": ""Ex: 30 dias"",
    ""conclusao"": ""Texto da conclusão final"",
    ""clausulasFixas"": [ { ""area"": ""Ex: Frota"", ""clausula"": ""Art. 5"", ""requisito"": ""Descrição"" } ],
    ""aspetosVariaveis"": [ { ""titulo"": ""Ex: Preço"", ""descricao"": ""Critério"" } ],
    ""penalidades"": [ { ""nivel"": ""Leve/Grave"", ""exemplos"": ""Atraso"", ""coima"": ""Valor"", ""compulsoria"": ""Valor"" } ],
    ""riscos"": [ { ""tipo"": ""Alto/Baixo"", ""titulo"": ""Titulo"", ""descricao"": ""Descrição"" } ]
    }";

    string prompt;

    if (config.Validating)
    {
        prompt =
            "1. Estrutura Obrigatória: Definição clara do objeto e fixação do Preço Base (limite máximo). " +
            "Distinção entre cláusulas fixas (adesão obrigatória) e aspetos variáveis (sujeitos à concorrência/avaliação). " +
            "Definição das regras de execução: prazos, garantias e penalidades por incumprimento. " +
            "2. Critérios de Avaliação de Risco: " +
            "Risco Alto (Nota 1-2) - Indícios de Favorecimento. " +
            "Risco Baixo (Nota 4-5) - Boas Práticas.";
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

    string fullPrompt = $"{prompt}\n\n{jsonInstruction}";

    // --- 3. Processamento ---
    foreach (var pdfPath in pdfFiles)
    {
        try
        {
            Console.WriteLine($"\n=== Processando PDF: {pdfPath} ===");

            var pdfText = pdfReaderService.ExtractTextFromPdf(pdfPath);
            if (string.IsNullOrWhiteSpace(pdfText))
            {
                Console.WriteLine("PDF vazio. Pulando...");
                continue;
            }

            var analysis = await geminiService.GenerateContentAsync(pdfText, fullPrompt);

            Console.WriteLine("\n--- RESPOSTA DO GEMINI ---\n");
            Console.WriteLine(analysis);

            // --- 4. GERAR PDF DE SAÍDA ---
            var fileName = Path.GetFileNameWithoutExtension(pdfPath);
            var outputPdfPath = Path.Combine(
                outputDir,
                $"{fileName}_analise.pdf"
            );

            pdfOutputService.CreateAnalysisPdf(
                outputPdfPath,
                fileName,
                analysis
            );

            Console.WriteLine($"PDF de análise gerado: {outputPdfPath}");
        }
        catch (Exception exPdf)
        {
            Console.WriteLine($"Erro ao processar {pdfPath}: {exPdf.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro geral: {ex.Message}");
}
