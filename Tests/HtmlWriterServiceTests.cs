using System;
using System.IO;
using Xunit;
using RealLifeLawAssist.Services;
using RealLifeLawAssist.Models;
using System.Collections.Generic;

namespace RealLifeLawAssist.Tests
{
    public class HtmlWriterServiceTests
    {
        [Fact]
        public void CreateAnalysisHtml_DadosValidos_DeveCriarFicheiroHtml()
        {
            // Arrange
            var service = new HtmlWriterService();

            var outputPath = Path.Combine(
                Path.GetTempPath(),
                $"analise_test_{Guid.NewGuid()}.html"
            );

            var dados = new AnaliseDados
            {
                Titulo = "Teste HTML",
                Descricao = "Descrição de teste",
                Objeto = "Objeto de teste",
                Localizacao = "Porto",
                TotalLinhas = "5",
                PrecoBase = 1500,
                CustoKm = 3,
                Vigencia = "6 meses",
                Caucao = "10%",
                Pagamento = "60 dias",
                Conclusao = "Conclusão final",
                ClausulasFixas = new List<ClausulaFixa>(),
                AspetosVariaveis = new List<AspetoVariavel>(),
                Penalidades = new List<Penalidade>(),
                Riscos = new List<Risco>()
            };

            // Act
            service.CreateAnalysisHtml(outputPath, "ficheiro_original.pdf", dados);

            // Assert
            Assert.True(File.Exists(outputPath));

            var content = File.ReadAllText(outputPath);
            Assert.Contains("<html", content);
            Assert.Contains("Teste HTML", content);

            // Cleanup
            File.Delete(outputPath);
        }
    }
}
