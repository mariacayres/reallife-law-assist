using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using RealLifeLawAssist.Services;
using RealLifeLawAssist.Models;

namespace RealLifeLawAssist.Tests
{
    public class HtmlConsolidatedWriterServiceTests
    {
        [Fact]
        public void CreateConsolidatedHtml_DadosValidos_DeveCriarHtml()
        {
            // Arrange
            var service = new HtmlConsolidatedWriterService();

            var outputPath = Path.Combine(
                Path.GetTempPath(),
                $"consolidado_{Guid.NewGuid()}.html"
            );

            var itens = new List<AnaliseConsolidadaItem>
            {
                new AnaliseConsolidadaItem
                {
                    Titulo = "Contrato Teste",
                    ScoreRisco = 6,
                    OutputHtmlPath = "teste.html",
                    Riscos = new List<RiscoItem> // ✅ TIPO CERTO
                    {
                        new RiscoItem
                        {
                            Titulo = "Risco financeiro",
                            Tipo = "Médio"
                        }
                    }
                }
            };

            // Act
            service.CreateConsolidatedHtml(outputPath, itens);

            // Assert
            Assert.True(File.Exists(outputPath));

            var html = File.ReadAllText(outputPath);
            Assert.Contains("Dashboard Consolidado de Risco", html);
            Assert.Contains("Contrato Teste", html);

            // Cleanup
            File.Delete(outputPath);
        }
    }
}
