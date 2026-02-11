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
    System.Net.ServicePointManager.SecurityProtocol =
    System.Net.SecurityProtocolType.Tls12 |
    System.Net.SecurityProtocolType.Tls13;

    // ==========================================
    // ===========
    // 1. BOOTSTRAP DA APLICAÇÃO
    // =====================================================

    using var geminiService = new GeminiService();
    var pdfReaderService = new PdfReaderService();
    var pdfOutputService = new PdfService();
    var htmlWriterService = new HtmlWriterService();
    var csvService = new CsvService();
    var config = new ConfigEnv();

    // Diretório de saída
    var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "consolidado");
    Directory.CreateDirectory(outputDir);

    Console.WriteLine("A iniciar download do CSV...");
    var csvPath = await csvService.DownloadCsvAsync(new DateTime(2026, 1, 9));
    Console.WriteLine($"CSV guardado em: {csvPath}");

    var zipOutputDir = Path.Combine(Directory.GetCurrentDirectory(), "zipDownloads");
    int totalZips = await csvService.DownloadAndExtractZipFilesFromCsvAsync(csvPath, zipOutputDir);
    Console.WriteLine($"Total de ZIPs baixados: {totalZips}");

    string zipDownloadsDir = Path.Combine(Directory.GetCurrentDirectory(), "zipDownloads");
    string pdfsDir = Path.Combine(Directory.GetCurrentDirectory(), "pdfs");

    // Coletar todos os PDFs com "Caderno_de_Encargos"
    int totalPdfs = await csvService.CollectCadernoDeEncargosPdfsAsync(zipDownloadsDir, pdfsDir);

    Console.WriteLine($"Total de PDFs preparados para processamento: {totalPdfs}");



    // =====================================================
    // 2. DESCOBERTA DOS PDFs
    // =====================================================

//     var pdfFiles = pdfReaderService.GetPdfFiles()?.ToList() ?? new List<string>();

//     if (!pdfFiles.Any())
//     {
//         Console.WriteLine("Nenhum PDF encontrado.");
//         return;
//     }

//     // Lista usada no relatório consolidado
//     var consolidado = new List<AnaliseConsolidadaItem>();

//     // =====================================================
//     // 3. CONTRATO JSON PARA A IA
//     // =====================================================

//     string jsonInstruction = @"
// Analise o documento e responda ESTRITAMENTE com um JSON válido (sem markdown) seguindo esta estrutura:
// {
//     ""titulo"": ""Titulo do Relatório"",
//     ""descricao"": ""Resumo executivo curto"",
//     ""objeto"": ""Descrição do objeto do contrato"",
//     ""localizacao"": ""Locais de execução"",
//     ""totalLinhas"": ""Ex: 12 circuitos"",
//     ""precoBase"": 0.0,
//     ""custoKm"": 0.0,
//     ""kmMax"": 0.0,
//     ""vigencia"": ""Ex: 60 dias"",
//     ""caucao"": ""Ex: 5%"",
//     ""pagamento"": ""Ex: 30 dias"",
//     ""conclusao"": ""Texto da conclusão final"",
//     ""clausulasFixas"": [
//         { ""area"": ""Ex: Frota"", ""clausula"": ""Art. 5"", ""requisito"": ""Descrição"" }
//     ],
//     ""aspetosVariaveis"": [
//         { ""titulo"": ""Ex: Preço"", ""descricao"": ""Critério"" }
//     ],
//     ""penalidades"": [
//         { ""nivel"": ""Leve/Grave"", ""exemplos"": ""Atraso"", ""coima"": ""Valor"", ""compulsoria"": ""Valor"" }
//     ],
//     ""riscos"": [
//         { ""tipo"": ""Alto/Baixo"", ""titulo"": ""Titulo"", ""descricao"": ""Descrição"" }
//     ]
// }";

//     string prompt;

//     if (config.Validating)
//     {
//         prompt =
//             "Estrutura obrigatória do procedimento, distinção entre cláusulas fixas e aspetos variáveis, " +
//             "regras de execução, penalidades e avaliação de risco (alto vs baixo).";
//     }
//     else
//     {
//         Console.WriteLine("Digite o prompt para validação do conteúdo:");
//         prompt = Console.ReadLine() ?? string.Empty;

//         if (string.IsNullOrWhiteSpace(prompt))
//         {
//             Console.WriteLine("Prompt vazio. Saindo...");
//             return;
//         }
//     }

//     string fullPrompt = $"{prompt}\n\n{jsonInstruction}";

//     // =====================================================
//     // 4. PROCESSAMENTO DE CADA PDF
//     // =====================================================

//     foreach (var pdfPath in pdfFiles)
//     {
//         try
//         {
//             Console.WriteLine($"\n=== Processando PDF: {pdfPath} ===");

//             var pdfText = pdfReaderService.ExtractTextFromPdf(pdfPath);

//             if (string.IsNullOrWhiteSpace(pdfText))
//             {
//                 Console.WriteLine("PDF vazio. Ignorado.");
//                 continue;
//             }

//             var analysisJson = await geminiService.GenerateContentAsync(
//                 pdfText,
//                 fullPrompt
//             );

//             // Limpeza de blocos ```json
//             analysisJson = analysisJson.Trim();
//             if (analysisJson.StartsWith("```"))
//             {
//                 int end = analysisJson.LastIndexOf("```");
//                 if (end > 0)
//                     analysisJson = analysisJson.Substring(3, end - 3).Trim();
//             }

//             AnaliseDados dados;
//             try
//             {
//                 dados = JsonSerializer.Deserialize<AnaliseDados>(
//                     analysisJson,
//                     new JsonSerializerOptions
//                     {
//                         PropertyNameCaseInsensitive = true
//                     }
//                 )!;
//             }
//             catch (JsonException)
//             {
//                 Console.WriteLine("JSON inválido. Ignorado.");
//                 Console.WriteLine(analysisJson);
//                 continue;
//             }

//             // Cálculo do score de risco
//             var scoreRisco = pdfOutputService.CalcularScoreRisco(dados);

//             // Caminhos de saída
//             var fileName = Path.GetFileNameWithoutExtension(pdfPath);
//             var outputPdfPath = Path.Combine(outputDir, $"{fileName}_analise.pdf");
//             var outputHtmlPath = Path.Combine(outputDir, $"{fileName}_analise.html");

//             // Dados para o consolidado
//             consolidado.Add(new AnaliseConsolidadaItem
//             {
//                 Arquivo = Path.GetFileName(pdfPath),
//                 Titulo = dados.Titulo,
//                 Descricao = dados.Descricao,
//                 ScoreRisco = scoreRisco,
//                 Riscos = (dados.Riscos ?? new List<Risco>())
//                     .Select(r => new RiscoItem
//                     {
//                         Titulo = r.Titulo,
//                         Tipo = r.Tipo,
//                         Descricao = r.Descricao
//                     })
//                     .ToList(),
//                 OutputHtmlPath = outputHtmlPath
//             });

//             // Outputs individuais
//             pdfOutputService.CreateAnalysisPdf(
//                 outputPdfPath,
//                 Path.GetFileName(pdfPath),
//                 dados
//             );

//             htmlWriterService.CreateAnalysisHtml(
//                 outputHtmlPath,
//                 Path.GetFileName(pdfPath),
//                 dados
//             );

//             Console.WriteLine($"✔ PDF gerado:  {outputPdfPath}");
//             Console.WriteLine($"✔ HTML gerado: {outputHtmlPath}");
//         }
//         catch (Exception exPdf)
//         {
//             Console.WriteLine($"Erro ao processar {pdfPath}: {exPdf.Message}");
//         }
//     }

//     // =====================================================
//     // 5. RELATÓRIOS CONSOLIDADOS (HTML + PDF)
//     // =====================================================

//     if (consolidado.Any())
//     {
//         var consolidadoOrdenado = consolidado
//             .OrderByDescending(c => c.ScoreRisco)
//             .ToList();

//         // HTML consolidado
//         var consolidatedHtmlPath = Path.Combine(
//             outputDir,
//             "relatorio_consolidado.html"
//         );

//         var htmlConsolidatedWriterService = new HtmlConsolidatedWriterService();
//         htmlConsolidatedWriterService.CreateConsolidatedHtml(
//             consolidatedHtmlPath,
//             consolidadoOrdenado
//         );

//         Console.WriteLine($"✔ HTML consolidado gerado: {consolidatedHtmlPath}");

//         // PDF consolidado
//         var consolidatedPdfPath = Path.Combine(
//             outputDir,
//             "relatorio_consolidado.pdf"
//         );

//         var pdfConsolidatedService = new RealLifeLawAssist.Services.PdfConsolidatedService();

//         pdfConsolidatedService.CreateConsolidatedPdf(
//             consolidatedPdfPath,
//             consolidadoOrdenado
//         );

//         Console.WriteLine($"✔ PDF consolidado gerado: {consolidatedPdfPath}");
//     }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro geral: {ex.Message}");
}
