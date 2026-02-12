using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using RealLifeLawAssist.Services;

namespace RealLifeLawAssist.Tests.Services
{
    public class CsvServiceTests : IDisposable
    {
        private readonly CsvService _csvService;
        private readonly string _testDirectory;
        private readonly string _csvDirectory;
        private readonly string _zipDownloadsDirectory;
        private readonly string _pdfsDirectory;
        private bool _disposed;

        public CsvServiceTests()
        {
            _csvService = new CsvService();
            
            _testDirectory = Path.Combine(Path.GetTempPath(), "CsvServiceTests_" + Guid.NewGuid());
            _csvDirectory = Path.Combine(_testDirectory, "csvs");
            _zipDownloadsDirectory = Path.Combine(_testDirectory, "zipDownloads");
            _pdfsDirectory = Path.Combine(_testDirectory, "pdfs");
            
            Directory.CreateDirectory(_csvDirectory);
            Directory.CreateDirectory(_zipDownloadsDirectory);
            Directory.CreateDirectory(_pdfsDirectory);
        }

        #region Testes de DownloadAndExtractZipFilesFromCsvAsync

        [Fact]
        public async Task DownloadAndExtractZipFilesFromCsvAsync_DeveLancarExcecao_QuandoCsvNaoExiste()
        {
            var csvInexistente = Path.Combine(_csvDirectory, "arquivo_inexistente.csv");
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                _csvService.DownloadAndExtractZipFilesFromCsvAsync(csvInexistente, _zipDownloadsDirectory));
        }

        [Fact]
        public async Task DownloadAndExtractZipFilesFromCsvAsync_DeveProcessarCSV_ComDadosValidos()
        {
            var csvPath = CriarCsvTesteValido();
            var zip1Path = CriarZipTeste("ZIP 1");
            var zip2Path = CriarZipTeste("ZIP 2");
            
            AtualizarLinksCsv(csvPath, new[] 
            { 
                ("http://exemplo.com/zip1.zip", zip1Path),
                ("http://exemplo.com/zip2.zip", zip2Path)
            });

            var result = await _csvService.DownloadAndExtractZipFilesFromCsvAsync(csvPath, _zipDownloadsDirectory);

            Assert.Equal(2, result);
            Assert.True(Directory.Exists(Path.Combine(_zipDownloadsDirectory, "12345")));
            Assert.True(Directory.Exists(Path.Combine(_zipDownloadsDirectory, "67890")));
            Assert.True(File.Exists(Path.Combine(_zipDownloadsDirectory, "12345", "12345.zip")));
            Assert.True(File.Exists(Path.Combine(_zipDownloadsDirectory, "67890", "67890.zip")));
            Assert.True(File.Exists(Path.Combine(_zipDownloadsDirectory, "12345", "demo.txt")));
            Assert.True(File.Exists(Path.Combine(_zipDownloadsDirectory, "67890", "demo.txt")));
            Assert.Equal("ZIP 1", File.ReadAllText(Path.Combine(_zipDownloadsDirectory, "12345", "demo.txt")));
            Assert.Equal("ZIP 2", File.ReadAllText(Path.Combine(_zipDownloadsDirectory, "67890", "demo.txt")));
        }

        [Fact]
        public async Task DownloadAndExtractZipFilesFromCsvAsync_DeveLidarComCaracteresInvalidos_NoNumeroAnuncio()
        {
            var csvPath = CriarCsvTesteCaracteresInvalidos();
            var zipPath = CriarZipTeste("Conteúdo de teste");
            
            AtualizarLinksCsv(csvPath, new[] 
            { 
                ("http://exemplo.com/zip1.zip", zipPath)
            });

            var result = await _csvService.DownloadAndExtractZipFilesFromCsvAsync(csvPath, _zipDownloadsDirectory);

            Assert.Equal(1, result);
            
            var pastaCriada = Directory.GetDirectories(_zipDownloadsDirectory).First();
            var nomePasta = Path.GetFileName(pastaCriada);
            
            Assert.DoesNotContain("/", nomePasta);
            Assert.DoesNotContain(":", nomePasta);
            Assert.DoesNotContain("*", nomePasta);
            Assert.DoesNotContain("?", nomePasta);
            Assert.Contains("123", nomePasta);
            Assert.Contains("45", nomePasta);
            Assert.Contains("ABC", nomePasta);
        }

        [Fact]
        public async Task DownloadAndExtractZipFilesFromCsvAsync_DeveIgnorarLinhas_SemLinkParaPecas()
        {
            var csvPath = CriarCsvTesteComLinkVazio();
            var zipPath = CriarZipTeste("Conteúdo de teste");
            
            AtualizarLinksCsv(csvPath, new[] 
            { 
                ("http://exemplo.com/zip2.zip", zipPath)
            });

            var result = await _csvService.DownloadAndExtractZipFilesFromCsvAsync(csvPath, _zipDownloadsDirectory);

            Assert.Equal(1, result);
            Assert.True(Directory.Exists(Path.Combine(_zipDownloadsDirectory, "67890")));
            Assert.False(Directory.Exists(Path.Combine(_zipDownloadsDirectory, "12345")));
        }

        [Fact]
        public async Task DownloadAndExtractZipFilesFromCsvAsync_DeveContinuarProcessamento_QuandoUmZipFalha()
        {
            var csvPath = CriarCsvTesteValido();
            var zipPath = CriarZipTeste("ZIP 2");
            var caminhoInexistente = Path.Combine(_testDirectory, "pasta_inexistente", "arquivo_inexistente.zip");
            
            AtualizarLinksCsv(csvPath, new[] 
            { 
                ("http://exemplo.com/zip1.zip", caminhoInexistente),
                ("http://exemplo.com/zip2.zip", zipPath)
            });

            var result = await _csvService.DownloadAndExtractZipFilesFromCsvAsync(csvPath, _zipDownloadsDirectory);

            Assert.Equal(1, result);
            Assert.True(Directory.Exists(Path.Combine(_zipDownloadsDirectory, "67890")));
            Assert.False(Directory.Exists(Path.Combine(_zipDownloadsDirectory, "12345")));
        }

        [Fact]
        public async Task DownloadAndExtractZipFilesFromCsvAsync_DeveProcessarCSV_SemColunaNumeroAnuncio()
        {
            var csvPath = CriarCsvTesteSemNumeroAnuncio();
            var zipPath = CriarZipTeste("Conteúdo de teste");
            
            AtualizarLinksCsv(csvPath, new[] 
            { 
                ("http://exemplo.com/zip1.zip", zipPath)
            });

            var result = await _csvService.DownloadAndExtractZipFilesFromCsvAsync(csvPath, _zipDownloadsDirectory);

            Assert.Equal(1, result);
            
            var pastaCriada = Directory.GetDirectories(_zipDownloadsDirectory).First();
            Assert.Contains("anuncio_1", Path.GetFileName(pastaCriada));
        }

        #endregion

        #region Testes de CollectCadernoDeEncargosPdfsAsync

        [Fact]
        public async Task CollectCadernoDeEncargosPdfsAsync_DeveColetarPDFs_Corretamente()
        {
            CriarEstruturaDePastasParaTeste();

            var result = await _csvService.CollectCadernoDeEncargosPdfsAsync(_zipDownloadsDirectory, _pdfsDirectory);

            Assert.Equal(3, result);
            
            var pdfsColetados = Directory.GetFiles(_pdfsDirectory, "*.pdf");
            Assert.Equal(3, pdfsColetados.Length);
            
            Assert.Contains(pdfsColetados, p => p.Contains("Caderno_de_Encargos_1.pdf"));
            Assert.Contains(pdfsColetados, p => p.Contains("Caderno_de_Encargos_2.pdf"));
            Assert.Contains(pdfsColetados, p => p.Contains("Caderno_de_Encargos_3.pdf"));
            Assert.DoesNotContain(pdfsColetados, p => p.Contains("Outro_Documento.pdf"));
        }

        [Fact]
        public async Task CollectCadernoDeEncargosPdfsAsync_DeveLancarExcecao_QuandoDiretorioNaoExiste()
        {
            var dirInexistente = Path.Combine(_testDirectory, "diretorio_inexistente");
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => 
                _csvService.CollectCadernoDeEncargosPdfsAsync(dirInexistente, _pdfsDirectory));
        }

        [Fact]
        public async Task CollectCadernoDeEncargosPdfsAsync_DeveRetornarZero_QuandoNaoHaPDFs()
        {
            var pastaVazia = Path.Combine(_zipDownloadsDirectory, "pasta_vazia");
            Directory.CreateDirectory(pastaVazia);

            var result = await _csvService.CollectCadernoDeEncargosPdfsAsync(pastaVazia, _pdfsDirectory);

            Assert.Equal(0, result);
            Assert.Empty(Directory.GetFiles(_pdfsDirectory));
        }

        [Fact]
        public async Task CollectCadernoDeEncargosPdfsAsync_DeveCriarDiretorioSaida_QuandoNaoExiste()
        {
            CriarEstruturaDePastasParaTeste();
            var novoDiretorioSaida = Path.Combine(_testDirectory, "novos_pdfs");

            var result = await _csvService.CollectCadernoDeEncargosPdfsAsync(_zipDownloadsDirectory, novoDiretorioSaida);

            Assert.True(Directory.Exists(novoDiretorioSaida));
            Assert.Equal(3, result);
        }

        [Fact]
        public async Task CollectCadernoDeEncargosPdfsAsync_DeveEvitarSobrescrita_QuandoPDFsComMesmoNome()
        {
            var anuncioDir = Path.Combine(_zipDownloadsDirectory, "anuncio_1");
            var processoDir = Path.Combine(anuncioDir, "Processo Concurso");
            Directory.CreateDirectory(processoDir);
            File.WriteAllText(Path.Combine(processoDir, "Caderno_de_Encargos.pdf"), "PDF 1");
            
            var anuncioDir2 = Path.Combine(_zipDownloadsDirectory, "anuncio_2");
            var processoDir2 = Path.Combine(anuncioDir2, "Processo Concurso");
            Directory.CreateDirectory(processoDir2);
            File.WriteAllText(Path.Combine(processoDir2, "Caderno_de_Encargos.pdf"), "PDF 2");

            var result = await _csvService.CollectCadernoDeEncargosPdfsAsync(_zipDownloadsDirectory, _pdfsDirectory);

            Assert.Equal(2, result);
            
            var pdfs = Directory.GetFiles(_pdfsDirectory, "*.pdf");
            Assert.Equal(2, pdfs.Length);
            Assert.Contains(pdfs, p => p.Contains("Caderno_de_Encargos.pdf"));
            Assert.Contains(pdfs, p => p.Contains("Caderno_de_Encargos_"));
        }

        #endregion

        #region Testes de DownloadCsvAsync

        [Fact(Skip = "Teste de integração - requer conexão com internet")]
        public async Task DownloadCsvAsync_TesteIntegracao()
        {
            var data = DateTime.Now.AddDays(-30);
            var result = await _csvService.DownloadCsvAsync(data);
            Assert.True(File.Exists(result));
            Assert.Contains("anuncios_", result);
        }

        #endregion

        #region Métodos Auxiliares

        private string CriarCsvTesteValido()
        {
            var csvPath = Path.Combine(_csvDirectory, "anuncios_validos.csv");
            var csvContent = new StringBuilder();
            csvContent.AppendLine("Número do Anúncio;Ligação para Peças;Outro Campo");
            csvContent.AppendLine("12345;http://exemplo.com/zip1.zip;Valor 1");
            csvContent.AppendLine("67890;http://exemplo.com/zip2.zip;Valor 2");
            File.WriteAllText(csvPath, csvContent.ToString(), Encoding.UTF8);
            return csvPath;
        }

        private string CriarCsvTesteCaracteresInvalidos()
        {
            var csvPath = Path.Combine(_csvDirectory, "anuncios_invalidos.csv");
            var csvContent = new StringBuilder();
            csvContent.AppendLine("Número do Anúncio;Ligação para Peças;Outro Campo");
            csvContent.AppendLine("123/45:*ABC?;http://exemplo.com/zip1.zip;Valor 1");
            File.WriteAllText(csvPath, csvContent.ToString(), Encoding.UTF8);
            return csvPath;
        }

        private string CriarCsvTesteComLinkVazio()
        {
            var csvPath = Path.Combine(_csvDirectory, "anuncios_links_vazios.csv");
            var csvContent = new StringBuilder();
            csvContent.AppendLine("Número do Anúncio;Ligação para Peças;Outro Campo");
            csvContent.AppendLine("12345;;Valor 1");
            csvContent.AppendLine("67890;http://exemplo.com/zip2.zip;Valor 2");
            File.WriteAllText(csvPath, csvContent.ToString(), Encoding.UTF8);
            return csvPath;
        }

        private string CriarCsvTesteSemNumeroAnuncio()
        {
            var csvPath = Path.Combine(_csvDirectory, "anuncios_sem_numero.csv");
            var csvContent = new StringBuilder();
            csvContent.AppendLine("Outro Campo;Ligação para Peças");
            csvContent.AppendLine("Valor 1;http://exemplo.com/zip1.zip");
            File.WriteAllText(csvPath, csvContent.ToString(), Encoding.UTF8);
            return csvPath;
        }

        private string CriarZipTeste(string conteudo = "Conteúdo de teste")
        {
            var zipPath = Path.Combine(_testDirectory, $"{Guid.NewGuid()}.zip");
            
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("demo.txt");
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream);
                writer.Write(conteudo);
            }
            
            File.WriteAllBytes(zipPath, memoryStream.ToArray());
            return zipPath;
        }

        private void AtualizarLinksCsv(string csvPath, (string linkOriginal, string novoCaminho)[] substituicoes)
        {
            var csvContent = File.ReadAllText(csvPath);
            
            foreach (var (linkOriginal, novoCaminho) in substituicoes)
            {
                csvContent = csvContent.Replace(linkOriginal, $"file://{novoCaminho}");
            }
            
            File.WriteAllText(csvPath, csvContent, Encoding.UTF8);
        }

        private void CriarEstruturaDePastasParaTeste()
        {
            var anuncio1 = Path.Combine(_zipDownloadsDirectory, "anuncio_1");
            var processo1 = Path.Combine(anuncio1, "Processo Concurso");
            Directory.CreateDirectory(processo1);
            File.WriteAllText(Path.Combine(processo1, "Caderno_de_Encargos_1.pdf"), "PDF 1");
            File.WriteAllText(Path.Combine(processo1, "Outro_Documento.pdf"), "Outro");

            var anuncio2 = Path.Combine(_zipDownloadsDirectory, "anuncio_2");
            var processo2 = Path.Combine(anuncio2, "Subpasta", "Processo Concurso");
            Directory.CreateDirectory(processo2);
            File.WriteAllText(Path.Combine(processo2, "Caderno_de_Encargos_2.pdf"), "PDF 2");

            var anuncio3 = Path.Combine(_zipDownloadsDirectory, "anuncio_3");
            var processo3 = Path.Combine(anuncio3, "Processo Concurso");
            Directory.CreateDirectory(processo3);
            File.WriteAllText(Path.Combine(processo3, "Caderno_de_Encargos_3.pdf"), "PDF 3");
        }

        #endregion

        public void Dispose()
        {
            if (_disposed) return;
            
            _csvService?.Dispose();
            
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch { }
            
            _disposed = true;
        }
    }
}