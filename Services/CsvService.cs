using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;

namespace RealLifeLawAssist.Services
{
    public class CsvService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool _disposed;

        public CsvService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "RealLifeLawAssist/1.0 (+https://example.com)"
            );
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Baixa o CSV do BASE.gov.pt para a data especificada.
        /// </summary>
        public async Task<string> DownloadCsvAsync(DateTime dataPublicacao)
        {
            var url = MontarUrlBaseGov(dataPublicacao);

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var csvBytes = await response.Content.ReadAsByteArrayAsync();

                var fileName = $"anuncios_{dataPublicacao:yyyyMMdd}.csv";
                var filePath = Path.Combine(Environment.CurrentDirectory, fileName);

                await File.WriteAllBytesAsync(filePath, csvBytes);

                return filePath;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erro ao descarregar CSV do BASE.gov.pt: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"Timeout ao descarregar CSV do BASE.gov.pt: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lê o CSV, baixa os ZIPs da coluna "Ligação para Peças" e descompacta em pastas nomeadas pelo Número do Anúncio.
        /// Suporta URLs HTTP/HTTPS e arquivos locais (file://) para testes.
        /// </summary>
        public async Task<int> DownloadAndExtractZipFilesFromCsvAsync(string csvPath, string outputBaseDir)
        {
            if (!File.Exists(csvPath))
                throw new FileNotFoundException("CSV não encontrado.", csvPath);

            Directory.CreateDirectory(outputBaseDir);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                BadDataFound = null,
                MissingFieldFound = null,
                HeaderValidated = null,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim
            };

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<dynamic>();
            int count = 0;

            foreach (var record in records)
            {
                var dict = (IDictionary<string, object>)record;

                if (!dict.TryGetValue("Ligação para Peças", out var urlObj))
                    continue;

                string url = urlObj?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(url))
                    continue;

                string numeroAnuncio = dict.TryGetValue("Número do Anúncio", out var numObj)
                    ? numObj?.ToString()?.Trim() ?? $"anuncio_{count + 1}"
                    : $"anuncio_{count + 1}";

                // Remove caracteres inválidos de nome de pasta/arquivo
                numeroAnuncio = SanitizarNomeArquivo(numeroAnuncio);

                try
                {
                    await ProcessarAnuncioAsync(url, numeroAnuncio, outputBaseDir);
                    count++;
                    Console.WriteLine($"✔ Anúncio {numeroAnuncio}: ZIP baixado e extraído.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro no anúncio {numeroAnuncio}: {ex.Message}");
                }
            }

            Console.WriteLine($"✅ Download e extração concluídos. Total de anúncios processados: {count}");
            return count;
        }

        /// <summary>
        /// Processa um único anúncio: baixa o ZIP e extrai seu conteúdo
        /// Só cria a pasta após o download bem-sucedido
        /// </summary>
        private async Task ProcessarAnuncioAsync(string url, string numeroAnuncio, string outputBaseDir)
        {
            // PRIMEIRO: Baixa o ZIP
            byte[] zipBytes = await DownloadArquivoAsync(url);

            // SÓ DEPOIS: Cria a pasta e salva o arquivo
            string anuncioDir = Path.Combine(outputBaseDir, numeroAnuncio);
            Directory.CreateDirectory(anuncioDir);

            string zipPath = Path.Combine(anuncioDir, $"{numeroAnuncio}.zip");
            await File.WriteAllBytesAsync(zipPath, zipBytes);

            // Descompacta o ZIP
            ZipFile.ExtractToDirectory(zipPath, anuncioDir, overwriteFiles: true);
        }

        /// <summary>
        /// Baixa um arquivo de URL HTTP/HTTPS ou de caminho local (file://)
        /// </summary>
        private async Task<byte[]> DownloadArquivoAsync(string url)
        {
            // Suporte para arquivos locais (file://) - usado apenas em testes
            if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                string localPath = url.Substring(7);
                localPath = Uri.UnescapeDataString(localPath);
                
                if (!File.Exists(localPath))
                    throw new FileNotFoundException($"Arquivo local não encontrado: {localPath}");
                
                return await File.ReadAllBytesAsync(localPath);
            }
            
            // URLs HTTP/HTTPS - uso em produção
            try
            {
                return await _httpClient.GetByteArrayAsync(url);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erro ao baixar arquivo de {url}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Varre a pasta base, encontra PDFs com "Caderno_de_Encargos" no nome e copia para pasta central.
        /// </summary>
        public async Task<int> CollectCadernoDeEncargosPdfsAsync(string zipDownloadsDir, string pdfOutputDir)
        {
            if (!Directory.Exists(zipDownloadsDir))
                throw new DirectoryNotFoundException($"A pasta {zipDownloadsDir} não existe.");

            Directory.CreateDirectory(pdfOutputDir);

            int count = 0;
            var anuncioDirs = Directory.GetDirectories(zipDownloadsDir);

            foreach (var anuncioDir in anuncioDirs)
            {
                try
                {
                    var processoConcursoDirs = Directory.GetDirectories(anuncioDir, "Processo Concurso", SearchOption.AllDirectories);

                    foreach (var procDir in processoConcursoDirs)
                    {
                        var pdfFiles = Directory.GetFiles(procDir, "*Caderno_de_Encargos*.pdf", SearchOption.TopDirectoryOnly);

                        foreach (var pdfPath in pdfFiles)
                        {
                            string fileName = Path.GetFileName(pdfPath);
                            string destPath = Path.Combine(pdfOutputDir, fileName);

                            // Evitar sobrescrita
                            if (File.Exists(destPath))
                            {
                                string nomeSemExt = Path.GetFileNameWithoutExtension(fileName);
                                string extensao = Path.GetExtension(fileName);
                                destPath = Path.Combine(pdfOutputDir, $"{nomeSemExt}_{Guid.NewGuid():N}{extensao}");
                            }

                            File.Copy(pdfPath, destPath, overwrite: false);
                            Console.WriteLine($"✔ PDF coletado: {Path.GetFileName(destPath)}");
                            count++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao processar pasta {anuncioDir}: {ex.Message}");
                }
            }

            Console.WriteLine($"✅ Total de PDFs coletados: {count}");
            return count;
        }

        /// <summary>
        /// Remove caracteres inválidos para nomes de arquivo/pasta
        /// </summary>
        private string SanitizarNomeArquivo(string nome)
        {
            if (string.IsNullOrEmpty(nome))
                return "anuncio_sem_nome";

            var charsInvalids = Path.GetInvalidFileNameChars();
            foreach (var c in charsInvalids)
            {
                nome = nome.Replace(c, '_');
            }
            
            nome = nome.Trim();
            
            if (string.IsNullOrWhiteSpace(nome))
                return "anuncio_sanitizado";
                
            return nome;
        }

        /// <summary>
        /// Monta a URL do BASE.gov.pt para download do CSV
        /// </summary>
        private string MontarUrlBaseGov(DateTime dataPublicacao)
        {
            return $"https://www.base.gov.pt/Base4/pt/resultados/?" +
                   $"type=csv_anuncios" +
                   $"&tipocontrato=0" +
                   $"&desdedatapublicacao={dataPublicacao:yyyy-MM-dd}" +
                   $"&atedatapublicacao={dataPublicacao:yyyy-MM-dd}" +
                   $"&tipoacto=0" +
                   $"&tipomodelo=1" +
                   $"&sort(-drPublicationDate)";
        }

        public void Dispose()
        {
            if (_disposed) return;
            _httpClient?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}