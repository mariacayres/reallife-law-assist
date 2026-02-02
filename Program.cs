﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RealLifeLawAssist.Configuration;
using RealLifeLawAssist.Models;
using RealLifeLawAssist.Services;

try
{
    // --- 1. Inicialização dos Serviços ---
    using var geminiService = new GeminiService();
    var pdfReaderService = new PdfReaderService();
    var pdfOutputService = new PdfService();
    var htmlWriterService = new HtmlWriterService();
    var config = new ConfigEnv();

    // Pasta onde os outputs serão salvos
    var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "consolidado");
    Directory.CreateDirectory(outputDir);

    // --- 2. Leitura dos PDFs ---
    var pdfFiles = pdfReaderService.GetPdfFiles()?.ToList() ?? new List<string>();
    if (pdfFiles.Count == 0)
    {
        Console.WriteLine("Nenhum PDF encontrado.");
        return;
    }

    // Lista para o relatório consolidado
    var consolidado = new List<AnaliseConsolidadaItem>();

    string jsonInstruction =
        @"
Analise o documento e responda ESTRITAMENTE com um JSON válido (sem markdown) seguindo esta estrutura:
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
            "Estrutura obrigatória do procedimento, distinção entre cláusulas fixas e aspetos variáveis, " +
            "regras de execução, penalidades e avaliação de risco (alto vs baixo).";
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

    // --- 3. Processamento dos PDFs ---
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

            var analysisJson = await geminiService.GenerateContentAsync(pdfText, fullPrompt);

            AnaliseDados dados;
            try
            {
                dados = JsonSerializer.Deserialize<AnaliseDados>(
                    analysisJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                )!;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao converter JSON para AnaliseDados:");
                Console.WriteLine(ex.Message);
                continue;
            }

            // 🔹 cálculo do score de risco
            var scoreRisco = pdfOutputService.CalcularScoreRisco(dados);

            // 🔹 adiciona ao consolidado
            consolidado.Add(new AnaliseConsolidadaItem
            {
                Arquivo = Path.GetFileName(pdfPath),
                Titulo = dados.Titulo,
                Descricao = dados.Descricao,
                ScoreRisco = scoreRisco,
                Riscos = dados.Riscos ?? new List<Risco>()
            });

            // --- 4. Geração dos Outputs individuais ---
            var fileName = Path.GetFileNameWithoutExtension(pdfPath);

            var outputPdfPath = Path.Combine(outputDir, $"{fileName}_analise.pdf");
            var outputHtmlPath = Path.Combine(outputDir, $"{fileName}_analise.html");

            pdfOutputService.CreateAnalysisPdf(
                outputPdfPath,
                Path.GetFileName(pdfPath),
                dados
            );

            htmlWriterService.CreateAnalysisHtml(
                outputHtmlPath,
                Path.GetFileName(pdfPath),
                dados
            );

            Console.WriteLine($"✔ PDF gerado:  {outputPdfPath}");
            Console.WriteLine($"✔ HTML gerado: {outputHtmlPath}");
        }
        catch (Exception exPdf)
        {
            Console.WriteLine($"Erro ao processar {pdfPath}: {exPdf.Message}");
        }
    }

    // --- 5. Ordenar e gerar HTML consolidado ---
    if (consolidado.Any())
    {
        var consolidadoOrdenado = consolidado
            .OrderByDescending(c => c.ScoreRisco)
            .ToList();

        var consolidatedHtmlPath = Path.Combine(
            outputDir,
            "relatorio_consolidado.html"
        );

        var htmlConsolidatedWriterService = new HtmlConsolidatedWriterService();
htmlConsolidatedWriterService.CreateConsolidatedHtml(consolidatedHtmlPath, consolidadoOrdenado);

        Console.WriteLine($"✔ HTML consolidado gerado: {consolidatedHtmlPath}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro geral: {ex.Message}");
}
