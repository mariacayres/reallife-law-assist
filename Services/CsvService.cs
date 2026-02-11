using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;
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
        }

        /// <summary>
        /// Baixa o CSV do BASE.gov.pt para a data especificada.
        /// </summary>
        public async Task<string> DownloadCsvAsync(DateTime dataPublicacao)
        {
            var url =
                $"https://www.base.gov.pt/Base4/pt/resultados/?" +
                $"type=csv_anuncios" +
                $"&tipocontrato=0" +
                $"&desdedatapublicacao={dataPublicacao:yyyy-MM-dd}" +
                $"&atedatapublicacao={dataPublicacao:yyyy-MM-dd}" +
                $"&tipoacto=0" +
                $"&tipomodelo=1" +
                $"&sort(-drPublicationDate)";

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
        }

          /// <summary>
        /// Lê o CSV, baixa os ZIPs da coluna "Ligação para Peças" e descompacta em pastas nomeadas pelo Número do Anúncio.
        /// </summary>
        /// <param name="csvPath">Caminho do CSV baixado</param>
        /// <param name="outputBaseDir">Pasta base onde os ZIPs serão baixados e descompactados</param>
        public async Task<int> DownloadAndExtractZipFilesFromCsvAsync(string csvPath, string outputBaseDir)
        {
            if (!File.Exists(csvPath))
                throw new FileNotFoundException("CSV não encontrado.", csvPath);

            Directory.CreateDirectory(outputBaseDir);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                BadDataFound = null
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

                string url = urlObj?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(url))
                    continue;

                string numeroAnuncio = dict.TryGetValue("Número do Anúncio", out var numObj)
                    ? numObj?.ToString() ?? $"anuncio_{count + 1}"
                    : $"anuncio_{count + 1}";

                // Remove caracteres inválidos de nome de pasta/arquivo
                foreach (var c in Path.GetInvalidFileNameChars())
                    numeroAnuncio = numeroAnuncio.Replace(c, '_');

                try
                {
                    // Cria pasta para este anúncio
                    string anuncioDir = Path.Combine(outputBaseDir, numeroAnuncio);
                    Directory.CreateDirectory(anuncioDir);

                    // Baixa o ZIP
                    byte[] zipBytes = await _httpClient.GetByteArrayAsync(url);

                    string zipPath = Path.Combine(anuncioDir, $"{numeroAnuncio}.zip");
                    await File.WriteAllBytesAsync(zipPath, zipBytes);

                    // Descompacta o ZIP na mesma pasta
                    ZipFile.ExtractToDirectory(zipPath, anuncioDir, overwriteFiles: true);

                    Console.WriteLine($"✔ Anúncio {numeroAnuncio}: ZIP baixado e extraído.");

                    count++;
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
        /// Varre a pasta base (zipDownloads), encontra todos os PDFs que contenham "Caderno_de_Encargos" no nome
        /// e copia para uma pasta central (pdfs) para processamento.
        /// </summary>
        /// <param name="zipDownloadsDir">Pasta onde os ZIPs foram extraídos</param>
        /// <param name="pdfOutputDir">Pasta onde os PDFs serão copiados</param>
        public async Task<int> CollectCadernoDeEncargosPdfsAsync(string zipDownloadsDir, string pdfOutputDir)
        {
            if (!Directory.Exists(zipDownloadsDir))
                throw new DirectoryNotFoundException($"A pasta {zipDownloadsDir} não existe.");

            Directory.CreateDirectory(pdfOutputDir);

            int count = 0;

            // Varrer todas as subpastas da pasta zipDownloads
            var anuncioDirs = Directory.GetDirectories(zipDownloadsDir);

            foreach (var anuncioDir in anuncioDirs)
            {
                // A pasta "Processo Concurso" pode estar dentro do anúncio
                var processoConcursoDirs = Directory.GetDirectories(anuncioDir, "Processo Concurso", SearchOption.AllDirectories);

                foreach (var procDir in processoConcursoDirs)
                {
                    // Encontrar todos os PDFs com "Caderno_de_Encargos" no nome
                    var pdfFiles = Directory.GetFiles(procDir, "*Caderno_de_Encargos*.pdf", SearchOption.TopDirectoryOnly);

                    foreach (var pdfPath in pdfFiles)
                    {
                        string fileName = Path.GetFileName(pdfPath);
                        string destPath = Path.Combine(pdfOutputDir, fileName);

                        // Copiar o PDF para a pasta central (sobrescreve se já existir)
                        File.Copy(pdfPath, destPath, overwrite: true);

                        Console.WriteLine($"✔ PDF coletado: {fileName}");
                        count++;
                    }
                }
            }

            Console.WriteLine($"✅ Total de PDFs coletados: {count}");
            return count;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}
