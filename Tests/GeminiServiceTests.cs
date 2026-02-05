using Xunit;
using RealLifeLawAssist.Services;
using RealLifeLawAssist.Models;

namespace RealLifeLawAssist.Tests
{
    public class GeminiServiceTests
    {
        [Fact]
        public void ConverterParaObjeto_JsonValido_DeveRetornarAnaliseDados()
        {
            // Arrange
            var service = new GeminiService();

            var json = @"{
                ""titulo"": ""Análise de Contrato"",
                ""descricao"": ""Contrato de prestação de serviços"",
                ""objeto"": ""Serviços jurídicos"",
                ""conclusao"": ""Contrato aceitável""
            }";

            // Act
            var result = service.ConverterParaObjeto(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Análise de Contrato", result.Titulo);
            Assert.Equal("Serviços jurídicos", result.Objeto);
            Assert.Equal("Contrato aceitável", result.Conclusao);
        }

        [Fact]
        public void ConverterParaObjeto_ComMarkdownJson_DeveLimparMarkdown()
        {
            // Arrange
            var service = new GeminiService();

            var jsonMarkdown = @"```json
            {
                ""titulo"": ""Teste Markdown"",
                ""conclusao"": ""OK""
            }
            ```";

            // Act
            var result = service.ConverterParaObjeto(jsonMarkdown);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Teste Markdown", result.Titulo);
            Assert.Equal("OK", result.Conclusao);
        }

        [Fact]
        public void ConverterParaObjeto_TextoVazio_DeveRetornarObjetoVazio()
        {
            // Arrange
            var service = new GeminiService();

            // Act
            var result = service.ConverterParaObjeto("");

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Titulo);
        }
    }
}
