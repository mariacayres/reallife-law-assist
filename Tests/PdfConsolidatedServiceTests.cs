using System.Collections.Generic;
using Xunit;
using RealLifeLawAssist.Models;
using RealLifeLawAssist.Services;

namespace RealLifeLawAssist.Tests
{
    public class PdfConsolidatedServiceTests
    {
        private readonly PdfConsolidatedService _service;

        public PdfConsolidatedServiceTests()
        {
            _service = new PdfConsolidatedService();
        }

        [Fact]
        public void CalcularScoreRisco_DeveRetornarScoreCorreto()
        {
            var item = new AnaliseConsolidadaItem
            {
                Riscos = new List<RiscoItem>
                {
                    new() { Tipo = "Alto" },
                    new() { Tipo = "Alto" },
                    new() { Tipo = "Médio" }
                }
            };

            var score = _service.CalcularScoreRisco(item);

            Assert.Equal(5, score); // 2 + 2 + 1
        }

        [Fact]
        public void CalcularScoreRisco_ComListaVazia_DeveRetornarZero()
        {
            var item = new AnaliseConsolidadaItem
            {
                Riscos = new List<RiscoItem>()
            };

            var score = _service.CalcularScoreRisco(item);

            Assert.Equal(0, score);
        }

        public static List<AnaliseConsolidadaItem> GerarDadosTeste()
        {
            return new List<AnaliseConsolidadaItem>
            {
                new()
                {
                    Titulo = "Contrato de Prestação de Serviços TI",
                    ScoreRisco = 8,
                    Riscos = new List<RiscoItem>
                    {
                        new() { Tipo = "Alto", Titulo = "Cláusula de Renovação Automática", Descricao = "Renovação automática sem aviso prévio" },
                        new() { Tipo = "Alto", Titulo = "Limitação de Responsabilidade", Descricao = "Limitação excessiva em caso de falhas" },
                        new() { Tipo = "Médio", Titulo = "Prazo de Pagamento", Descricao = "Prazo de 60 dias pode afetar cashflow" }
                    }
                },
                new()
                {
                    Titulo = "Contrato de Aluguer de Equipamentos",
                    ScoreRisco = 3,
                    Riscos = new List<RiscoItem>
                    {
                        new() { Tipo = "Médio", Titulo = "Multas por Atraso", Descricao = "Multas acumulativas podem ser pesadas" },
                        new() { Tipo = "Baixo", Titulo = "Manutenção", Descricao = "Responsabilidade do arrendatário" }
                    }
                },
                new()
                {
                    Titulo = "Acordo de Confidencialidade",
                    ScoreRisco = 1,
                    Riscos = new List<RiscoItem>
                    {
                        new() { Tipo = "Baixo", Titulo = "Prazo de Vigência", Descricao = "5 anos pode ser extenso para algumas informações" }
                    }
                }
            };
        }
    }
}
