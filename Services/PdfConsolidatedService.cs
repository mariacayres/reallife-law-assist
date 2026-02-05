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
            int riscosCount = item.Riscos?.Count ?? 0;
            double cardHeight = 80 + (riscosCount * 14);

            // Verificar se precisa de nova página
            if (y + cardHeight > page.Height - 80)
            {
                gfx.Dispose();
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                
                // Redesenhar cabeçalho na nova página
                DrawHeader(gfx, page, 0); // 0 porque não queremos mostrar número de docs
                y = 120; // Resetar posição Y após cabeçalho
            }

            // Determinar cor da borda baseado no score
            XColor border = item.ScoreRisco >= 8 ? XColors.DarkRed :
                            item.ScoreRisco >= 4 ? XColors.DarkOrange : 
                            XColors.DarkGreen;

            // Desenhar card
            gfx.DrawRectangle(
                new XPen(border, 2), // Reduzi de 3 para 2 para melhor visualização
                margin,
                y,
                page.Width - margin * 2,
                cardHeight
            );

            // Título do documento
            gfx.DrawString(
                item.Titulo ?? "Documento sem título",
                _sectionFont,
                XBrushes.Black,
                margin + 10,
                y + 22
            );

            // Score de risco
            gfx.DrawString(
                $"Score de Risco: {item.ScoreRisco}",
                _bodyFont,
                new XSolidBrush(border),
                margin + 10,
                y + 40
            );

            // Lista de riscos
            double ry = y + 58;

            if (item.Riscos != null && item.Riscos.Count > 0)
            {
                foreach (var risco in item.Riscos)
                {
                    if (risco == null) continue;
                    
                    // Usar cor diferente baseado no tipo de risco
                    XBrush riscoBrush = risco.Tipo?.ToLower().Contains("alto") == true ? 
                                       XBrushes.DarkRed : XBrushes.Black;

                    gfx.DrawString(
                        $"• {risco.Titulo ?? "Risco sem título"}",
                        _smallFont,
                        riscoBrush,
                        margin + 20,
                        ry
                    );
                    ry += 14;
                }
            }
            else
            {
                // Se não há riscos
                gfx.DrawString(
                    "• Nenhum risco identificado",
                    _smallFont,
                    XBrushes.Gray,
                    margin + 20,
                    ry
                );
                ry += 14;
            }

            y += cardHeight + 15;
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
    }
}