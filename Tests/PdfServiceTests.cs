using System.Linq;
using Xunit;
using RealLifeLawAssist.Models;
using RealLifeLawAssist.Services;

namespace RealLifeLawAssist.Tests
{
    public class PdfServiceTests
    {
        private readonly PdfService _service;

        public PdfServiceTests()
        {
            _service = new PdfService();
        }

        [Fact]
        public void CalcularScoreRisco_DeveRetornarScoreCorreto()
        {
            var dados = new AnaliseDados
            {
                Riscos = new System.Collections.Generic.List<Risco>
                {
                    new() { Tipo = "alto" },
                    new() { Tipo = "alto" },
                    new() { Tipo = "medio" }
                }
            };

            var score = _service.CalcularScoreRisco(dados);

            Assert.Equal(5, score); // 2 + 2 + 1
        }

        [Fact]
        public void CalcularScoreRisco_ComListaVazia_DeveRetornarZero()
        {
            var dados = new AnaliseDados
            {
                Riscos = new System.Collections.Generic.List<Risco>()
            };

            var score = _service.CalcularScoreRisco(dados);

            Assert.Equal(0, score);
        }
    }
}
