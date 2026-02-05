using System;
using System.IO;
using Xunit;
using RealLifeLawAssist.Services;
using RealLifeLawAssist.Models;
using System.Collections.Generic;

namespace RealLifeLawAssist.Tests
{
    public class PdfServiceTests
    {
        [Fact]
        public void CreateAnalysisPdf_DadosValidos_DeveCriarPdf()
        {
            // Arrange
            var service = new PdfService();

            var outputPath = Path.Combine(
                Path.GetTempPath(),
                $"analise_test_{Guid.NewGuid()}.pdf"
            );

            var dados = new AnaliseDados
            {
                Titulo = "Contrato Teste",
                Descricao = "Descrição do contrato",
                Objeto = "Objeto teste",
                Localizacao = "Lisboa",
                TotalLinhas = "10",
                PrecoBase = 1000,
                CustoKm = 5,
                Vigencia = "12 meses",
                Caucao = "5%",
                Pagamento = "30 dias",
                Conclusao = "Conclusão teste",
                ClausulasFixas = new List<ClausulaFixa>(),
                AspetosVariaveis = new List<AspetoVariavel>(),
                Penalidades = new List<Penalidade>(),
                Riscos = new List<Risco>()
            };

            // Act
            service.CreateAnalysisPdf(outputPath, "Contrato Teste", dados);

            // Assert
            Assert.True(File.Exists(outputPath));

            // Cleanup
            File.Delete(outputPath);
        }
    }
}
