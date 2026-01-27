﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using RealLifeLawAssist.Services;
using RealLifeLawAssist.Configuration;

// O ponto de entrada da aplicação agora usa "top-level statements".
// O código é executado diretamente, e o compilador gera a classe Program e o método Main nos bastidores.
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
    bool validating = false;
    string answer= "";
    var prompt = "";
    do
    {

    Console.WriteLine("deseja usar um prompt determinado? (s/n)");
    answer= Console.ReadLine();
    if (answer== "s")
    {
    prompt = "1. Estrutura Obrigatória: Definição clara do objeto e fixação do Preço Base (limite máximo).Distinção entre cláusulas fixas (adesão obrigatória) e aspetos variáveis (sujeitos à concorrência/avaliação).Definição das regras de execução: prazos, garantias e penalidades por incumprimento.2. Critérios de Avaliação de Risco:Risco Alto (Nota 1-2) - Indícios de Favorecimento: Especificações À Medida: Dimensões exatas (ex: 1024mm) ou técnicas sem justificação funcional. Prazos Impossíveis: Cronogramas de execução viáveis apenas para quem tenha informação privilegiada.Bloqueio de Marca: Referência a marcas, modelos ou patentes sem a menção expressa ou equivalente. Risco Baixo (Nota 4-5) - Boas Práticas:Descritivo Funcional: Especificações focadas no desempenho/resultado e não no método de fabrico.Realismo de Mercado: Prazos alinhados com os standards da indústria.Rigor na Execução: Regime de penalizações (multas) claro e dissuasor para atrasos ou falhas.";
    validating= true;
    }
    else if (answer == "n  " )
    {
    Console.WriteLine("Digite o prompt para validação do conteúdo:");
    prompt = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(prompt))
    {
        Console.WriteLine("Prompt vazio. Saindo...");
        return;
    }    validating= true;
    }
    else
    {
        Console.WriteLine("tente novamente");
    }
    }while(validating==false);
    // --- 3. Interação com o Utilizador ---
    // Pede ao utilizador para inserir o comando (prompt) que será usado para analisar os PDFs.
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
