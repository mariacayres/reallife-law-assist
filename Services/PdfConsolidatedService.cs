using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using RealLifeLawAssist.Models;

namespace RealLifeLawAssist.Services
{
    public class PdfConsolidatedService
    {
        // ================= FONTS =================
        private readonly XFont _titleFont = new XFont("Arial", 16, XFontStyle.Bold);
        private readonly XFont _sectionFont = new XFont("Arial", 12, XFontStyle.Bold);
        private readonly XFont _bodyFont = new XFont("Arial", 10, XFontStyle.Regular);
        private readonly XFont _smallFont = new XFont("Arial", 9, XFontStyle.Regular);
        private readonly XColor _darkBlue = XColors.DarkBlue;

        // ================= PUBLIC API =================
        public void CreateConsolidatedPdf(
            string outputPath,
            List<AnaliseConsolidadaItem> itens)
        {
            if (itens == null || itens.Count == 0)
                throw new ArgumentException("Lista de itens consolidada vazia.");

            using var document = new PdfDocument();
            document.Info.Title = "Dashboard Consolidado de Risco";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            double margin = 50;
            double y = 120;

            DrawHeader(gfx, page, itens.Count);
            DrawKpis(gfx, margin, ref y, itens);
            DrawSectionTitle(gfx, margin, ref y, "Resumo Consolidado");

            foreach (var item in itens.OrderByDescending(i => i.ScoreRisco))
            {
                DrawCard(
                    document,
                    ref page,
                    ref gfx,
                    ref y,
                    margin,
                    item
                );
            }

            // Liberta o último graphics antes de salvar
            gfx.Dispose();

            DrawFooters(document);
            document.Save(outputPath);
        }

        // ================= HEADER =================
        private void DrawHeader(XGraphics gfx, PdfPage page, int totalDocs)
        {
            gfx.DrawRectangle(
                new XSolidBrush(_darkBlue),
                0, 0, page.Width, 90
            );

            gfx.DrawString(
                "Dashboard Consolidado de Risco Contratual",
                _titleFont,
                XBrushes.White,
                new XRect(0, 35, page.Width, 30),
                XStringFormats.Center
            );

            gfx.DrawString(
                $"{DateTime.Now:dd/MM/yyyy HH:mm} · {totalDocs} documentos analisados",
                _smallFont,
                XBrushes.LightGray,
                new XRect(0, 65, page.Width, 20),
                XStringFormats.Center
            );
        }

        // ================= KPIs =================
        private void DrawKpis(
            XGraphics gfx,
            double margin,
            ref double y,
            List<AnaliseConsolidadaItem> itens)
        {
            int alto = itens.Sum(i =>
                i.Riscos?.Count(r => 
                    r?.Tipo != null && r.Tipo.ToLower().Contains("alto")) ?? 0);

            int medio = itens.Sum(i =>
                i.Riscos?.Count(r => 
                    r?.Tipo != null && r.Tipo.ToLower().Contains("medio")) ?? 0);

            int baixo = itens.Sum(i =>
                i.Riscos?.Count(r => 
                    r?.Tipo != null && r.Tipo.ToLower().Contains("baixo")) ?? 0);

            DrawKpiBox(gfx, margin, y, "Documentos", itens.Count.ToString(), XBrushes.Black);
            DrawKpiBox(gfx, margin + 120, y, "Risco Alto", alto.ToString(), XBrushes.DarkRed);
            DrawKpiBox(gfx, margin + 240, y, "Risco Médio", medio.ToString(), XBrushes.DarkOrange);
            DrawKpiBox(gfx, margin + 360, y, "Risco Baixo", baixo.ToString(), XBrushes.DarkGreen);

            y += 70;
        }

        private void DrawKpiBox(
            XGraphics gfx,
            double x,
            double y,
            string label,
            string value,
            XBrush valueBrush)
        {
            gfx.DrawRectangle(XPens.LightGray, x, y, 100, 55);
            gfx.DrawString(label, _smallFont, XBrushes.Gray, x + 8, y + 18);
            gfx.DrawString(value, _sectionFont, valueBrush, x + 8, y + 40);
        }

        // ================= SEÇÃO =================
        private void DrawSectionTitle(
            XGraphics gfx,
            double margin,
            ref double y,
            string title)
        {
            gfx.DrawString(title, _sectionFont, XBrushes.DarkBlue, margin, y);
            y += 20;
        }

        // ================= CARD =================
        private void DrawCard(
            PdfDocument document,
            ref PdfPage page,
            ref XGraphics gfx,
            ref double y,
            double margin,
            AnaliseConsolidadaItem item)
        {
            // Calcular altura dinamicamente baseado no conteúdo
            double currentY = y;
            double cardStartY = y;
            
            // TÍTULO EXATO: CADERNO DE ENCARGOS AQUISIÇÃO DE SERVIÇOS CONCURSO PÚBLICO- Manutenção de AVAC e Sistemas de Águas Quentes Sanitárias
            string tituloDocumento = string.IsNullOrEmpty(item.Titulo) ? "Documento sem título" : item.Titulo;

            // Determinar cor da borda baseado no score
            XColor border = item.ScoreRisco >= 8 ? XColors.DarkRed :
                            item.ScoreRisco >= 4 ? XColors.DarkOrange : 
                            XColors.DarkGreen;

            // ===== DESENHAR TÍTULO LONGO =====
            double titleY = currentY + 15;
            double maxTitleWidth = page.Width - margin * 2 - 20;
            
            // DESENHAR O TÍTULO COMPLETO COM QUEBRA - MÉTODO MELHORADO
            double linhaY = titleY;
            
            // Método mais robusto para títulos muito longos
            DesenharTituloComQuebraMelhorado(gfx, tituloDocumento, _sectionFont, XBrushes.Black, 
                margin + 10, ref linhaY, maxTitleWidth);
            
            // "Total de Riscos: X" - IGUAL NA IMAGEM
            double totalRiscosY = linhaY + 10;
            int totalRiscos = item.Riscos?.Count ?? 0;
            gfx.DrawString(
                $"Total de Riscos: {totalRiscos}",
                _bodyFont,
                XBrushes.DarkBlue,
                margin + 10,
                totalRiscosY
            );
            
            // Score de risco
            double scoreY = totalRiscosY + 15;
            
            gfx.DrawString(
                $"Score de Risco: {item.ScoreRisco}",
                _bodyFont,
                new XSolidBrush(border),
                margin + 10,
                scoreY
            );
            
            // Lista de riscos
            double risksY = scoreY + 20;
            double risksHeight = 0;

            if (item.Riscos != null && item.Riscos.Count > 0)
            {
                foreach (var risco in item.Riscos)
                {
                    if (risco == null) continue;
                    
                    XBrush riscoBrush = risco.Tipo?.ToLower().Contains("alto") == true ? 
                                       XBrushes.DarkRed : XBrushes.Black;

                    string tituloRisco = string.IsNullOrEmpty(risco.Titulo) ? "Risco sem título" : risco.Titulo;
                    gfx.DrawString(
                        $"• {tituloRisco}",
                        _smallFont,
                        riscoBrush,
                        margin + 20,
                        risksY + risksHeight
                    );
                    risksHeight += 14;
                }
            }
            else
            {
                gfx.DrawString(
                    "• Nenhum risco identificado",
                    _smallFont,
                    XBrushes.Gray,
                    margin + 20,
                    risksY
                );
                risksHeight += 14;
            }

            // Calcular altura total do card
            double cardHeight = (risksY + risksHeight) - cardStartY + 30;

            // Verificar se precisa de nova página
            if (y + cardHeight > page.Height - 80)
            {
                gfx.Dispose();
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                
                DrawHeader(gfx, page, 0);
                y = 120;
                cardStartY = y;
                
                DrawCard(document, ref page, ref gfx, ref y, margin, item);
                return;
            }

            // Desenhar borda do card
            gfx.DrawRectangle(
                new XPen(border, 2),
                margin,
                cardStartY,
                page.Width - margin * 2,
                cardHeight
            );

            y = cardStartY + cardHeight + 15;
        }

        // ================= MÉTODO MELHORADO PARA DESENHAR TÍTULO COM QUEBRA =================
        private void DesenharTituloComQuebraMelhorado(XGraphics gfx, string texto, XFont fonte, XBrush pincel,
            double x, ref double y, double larguraMaxima)
        {
            if (string.IsNullOrWhiteSpace(texto)) return;

            // Se o texto couber em uma linha, desenha direto
            XSize tamanhoTotal = gfx.MeasureString(texto, fonte);
            if (tamanhoTotal.Width <= larguraMaxima)
            {
                gfx.DrawString(texto, fonte, pincel, x, y);
                y += fonte.Height + 2;
                return;
            }
            
            // Se for MUITO longo, quebrar em partes
            string[] palavras = texto.Split(' ');
            string linhaAtual = "";
            
            for (int i = 0; i < palavras.Length; i++)
            {
                string palavra = palavras[i];
                string testeLinha = string.IsNullOrEmpty(linhaAtual) ? palavra : linhaAtual + " " + palavra;
                XSize tamanhoTeste = gfx.MeasureString(testeLinha, fonte);
                
                if (tamanhoTeste.Width > larguraMaxima)
                {
                    if (string.IsNullOrEmpty(linhaAtual))
                    {
                        // Palavra individual é muito longa - quebrar a palavra
                        QuebrarPalavraLonga(gfx, palavra, fonte, pincel, x, ref y, larguraMaxima);
                        linhaAtual = "";
                    }
                    else
                    {
                        // Desenha a linha atual
                        gfx.DrawString(linhaAtual, fonte, pincel, x, y);
                        y += fonte.Height + 2;
                        linhaAtual = palavra;
                    }
                }
                else
                {
                    linhaAtual = testeLinha;
                }
            }
            
            // Desenha a última linha
            if (!string.IsNullOrEmpty(linhaAtual))
            {
                gfx.DrawString(linhaAtual, fonte, pincel, x, y);
                y += fonte.Height + 2;
            }
        }

        // ================= MÉTODO PARA QUEBRAR PALAVRAS MUITO LONGAS =================
        private void QuebrarPalavraLonga(XGraphics gfx, string palavra, XFont fonte, XBrush pincel,
            double x, ref double y, double larguraMaxima)
        {
            if (string.IsNullOrEmpty(palavra)) return;
            
            // Se a palavra couber, desenha normal
            XSize tamanhoPalavra = gfx.MeasureString(palavra, fonte);
            if (tamanhoPalavra.Width <= larguraMaxima)
            {
                gfx.DrawString(palavra, fonte, pincel, x, y);
                y += fonte.Height + 2;
                return;
            }
            
            // Palavra muito longa - quebrar manualmente
            string parteAtual = "";
            for (int i = 0; i < palavra.Length; i++)
            {
                parteAtual += palavra[i];
                XSize tamanhoParte = gfx.MeasureString(parteAtual + "-", fonte);
                
                if (tamanhoParte.Width > larguraMaxima)
                {
                    // Desenha a parte atual com hífen
                    gfx.DrawString(parteAtual + "-", fonte, pincel, x, y);
                    y += fonte.Height + 2;
                    parteAtual = palavra[i].ToString(); // Começa nova linha com o caractere atual
                }
            }
            
            // Desenha o que sobrou
            if (!string.IsNullOrEmpty(parteAtual))
            {
                gfx.DrawString(parteAtual, fonte, pincel, x, y);
                y += fonte.Height + 2;
            }
        }

        // ================= FOOTER =================
        private void DrawFooters(PdfDocument document)
        {
            for (int i = 0; i < document.PageCount; i++)
            {
                var page = document.Pages[i];
                using var gfx = XGraphics.FromPdfPage(page);

                // Linha separadora
                gfx.DrawLine(
                    new XPen(XColors.LightGray, 0.5),
                    50,
                    page.Height - 45,
                    page.Width - 50,
                    page.Height - 45
                );

                // Texto do rodapé esquerdo
                gfx.DrawString(
                    $"RealLife Law Assist © {DateTime.Now.Year}",
                    _smallFont,
                    XBrushes.Gray,
                    50,
                    page.Height - 30
                );

                // Número da página
                gfx.DrawString(
                    $"Página {i + 1} de {document.PageCount}",
                    _smallFont,
                    XBrushes.Gray,
                    page.Width - 100,
                    page.Height - 30
                );
            }
        }

        // ================= MÉTODO AUXILIAR PARA CÁLCULO DE SCORE =================
        public int CalcularScoreRisco(AnaliseConsolidadaItem item)
        {
            if (item?.Riscos == null) return 0;
            
            return item.Riscos.Sum(r => 
            {
                if (r?.Tipo == null) return 0;
                
                return r.Tipo.ToLower() switch
                {
                    "alto" => 2,
                    "medio" or "médio" => 1,
                    _ => 0
                };
            });
        }

        // ================= MÉTODO PARA GERAR DADOS DE TESTE =================
        public List<AnaliseConsolidadaItem> GerarDadosTeste()
        {
            return new List<AnaliseConsolidadaItem>
            {
                new AnaliseConsolidadaItem
                {
                    Arquivo = "caderno_encargos.pdf",
                    // TÍTULO EXATO QUE VOCÊ MANDOU
                    Titulo = "CADERNO DE ENCARGOS AQUISIÇÃO DE SERVIÇOS CONCURSO PÚBLICO- Manutenção de AVAC e Sistemas de Águas Quentes Sanitárias",
                    Descricao = "Documento para aquisição de serviços de manutenção via concurso público",
                    ScoreRisco = 6,
                    OutputHtmlPath = "/analises/encargos.html",
                    Riscos = new List<RiscoItem>
                    {
                        new RiscoItem { Tipo = "Alto", Titulo = "Incumprimento de Tempo de Resposta (Urgência)" },
                        new RiscoItem { Tipo = "Alto", Titulo = "Resolução Contratual por Falha Crítica" },
                        new RiscoItem { Tipo = "Médio", Titulo = "Risco Financeiro por Revisão de Preços" },
                        new RiscoItem { Tipo = "Baixo", Titulo = "Risco de Não Pagamento" }
                    },
                    Badges = new List<string> { "Concurso Público", "AVAC", "Manutenção" }
                }
            };
        }
    }
}