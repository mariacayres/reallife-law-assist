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
        private readonly XFont TitleFont   = new XFont("Arial", 16, XFontStyle.Bold);
        private readonly XFont SectionFont = new XFont("Arial", 12, XFontStyle.Bold);
        private readonly XFont BodyFont    = new XFont("Arial", 10, XFontStyle.Regular);
        private readonly XFont SmallFont   = new XFont("Arial", 9, XFontStyle.Regular);

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
                new XSolidBrush(XColors.DarkBlue),
                0, 0, page.Width, 90
            );

            gfx.DrawString(
                "Dashboard Consolidado de Risco Contratual",
                TitleFont,
                XBrushes.White,
                new XRect(0, 35, page.Width, 30),
                XStringFormats.Center
            );

            gfx.DrawString(
                $"{DateTime.Now:dd/MM/yyyy HH:mm} · {totalDocs} documentos analisados",
                SmallFont,
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
                i.Riscos?.Count(r => r.Tipo?.ToLower().Contains("alto") == true) ?? 0);

            int medio = itens.Sum(i =>
                i.Riscos?.Count(r => r.Tipo?.ToLower().Contains("medio") == true) ?? 0);

            int baixo = itens.Sum(i =>
                i.Riscos?.Count(r => r.Tipo?.ToLower().Contains("baixo") == true) ?? 0);

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
            gfx.DrawString(label, SmallFont, XBrushes.Gray, x + 8, y + 18);
            gfx.DrawString(value, SectionFont, valueBrush, x + 8, y + 40);
        }

        // ================= SEÇÃO =================
        private void DrawSectionTitle(
            XGraphics gfx,
            double margin,
            ref double y,
            string title)
        {
            gfx.DrawString(title, SectionFont, XBrushes.DarkBlue, margin, y);
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

            if (y + cardHeight > page.Height - 80)
            {
                gfx.Dispose();
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = 60;
            }

            XColor border =
                item.ScoreRisco >= 8 ? XColors.DarkRed :
                item.ScoreRisco >= 4 ? XColors.DarkOrange :
                XColors.DarkGreen;

            gfx.DrawRectangle(
                new XPen(border, 3),
                margin,
                y,
                page.Width - margin * 2,
                cardHeight
            );

            gfx.DrawString(
                item.Titulo ?? "Documento sem título",
                SectionFont,
                XBrushes.Black,
                margin + 10,
                y + 22
            );

            gfx.DrawString(
                $"Score de Risco: {item.ScoreRisco}",
                BodyFont,
                new XSolidBrush(border),
                margin + 10,
                y + 40
            );

            double ry = y + 58;

            if (item.Riscos != null)
            {
                foreach (var r in item.Riscos)
                {
                    gfx.DrawString(
                        $"• {r.Titulo}",
                        SmallFont,
                        XBrushes.Black,
                        margin + 20,
                        ry
                    );
                    ry += 14;
                }
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

                gfx.DrawLine(
                    XPens.LightGray,
                    50,
                    page.Height - 45,
                    page.Width - 50,
                    page.Height - 45
                );

                gfx.DrawString(
                    $"RealLife Law Assist © {DateTime.Now.Year}",
                    SmallFont,
                    XBrushes.Gray,
                    50,
                    page.Height - 30
                );

                gfx.DrawString(
                    $"Página {i + 1} de {document.PageCount}",
                    SmallFont,
                    XBrushes.Gray,
                    page.Width - 150,
                    page.Height - 30
                );
            }
        }
    }
}
