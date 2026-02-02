using System;
using System.IO;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using RealLifeLawAssist.Models;
using System.Collections.Generic;
using System.Linq;

namespace RealLifeLawAssist.Services
{
    public class PdfService
    {
        // Cor usada no header (defina antes de usar)
        private XColor _realLifeBlue = XColors.DarkBlue;
        private XFont _footerFont = new XFont("Arial", 9, XFontStyle.Regular);

        public void CreateAnalysisPdf(string outputPath, string title, AnaliseDados data)
        {
            try
            {
                using var document = new PdfDocument();
                document.Info.Title = "Análise Jurídica - RealLife Law Assist";

                double margin = 50;
                double yPosition = 140;

                XFont titleFont = new XFont("Arial", 16, XFontStyle.Bold);
                XFont sectionFont = new XFont("Arial", 12, XFontStyle.Bold);
                XFont bodyFont = new XFont("Arial", 11, XFontStyle.Regular);
                XFont boldFont = new XFont("Arial", 11, XFontStyle.Bold);
                XFont italicFont = new XFont("Arial", 10, XFontStyle.Italic);

                PdfPage currentPage = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(currentPage);

                DrawHeader(gfx, currentPage);

                // Título e Metadados  
                gfx.DrawString(
                    "Relatório de Análise Técnica",
                    titleFont,
                    XBrushes.Black,
                    margin,
                    120
                );
                DrawWrappedText(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    data.Titulo ?? title,
                    boldFont,
                    XBrushes.DarkBlue
                );
                DrawWrappedText(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    data.Descricao,
                    italicFont,
                    XBrushes.DarkGray
                );
                yPosition += 10;

                // 1. Objeto e Preço
                DrawSectionHeader(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    "1. Objeto e Preço Base"
                );
                DrawWrappedText(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    $"Objeto: {data.Objeto}",
                    bodyFont,
                    XBrushes.Black
                );
                DrawWrappedText(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    $"Local: {data.Localizacao} | Linhas: {data.TotalLinhas}",
                    bodyFont,
                    XBrushes.Black
                );
                DrawWrappedText(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    $"Preço Base: {data.PrecoBase:C} | Custo/Km: {data.CustoKm:C}",
                    bodyFont,
                    XBrushes.Black
                );

                // 2. Matriz de Conformidade
                DrawSectionHeader(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    "2. Matriz de Conformidade"
                );
                foreach (var item in data.ClausulasFixas ?? new List<ClausulaFixa>())
                {
                    DrawWrappedText(
                        ref currentPage,
                        ref gfx,
                        document,
                        ref yPosition,
                        margin,
                        $"• [{item.Area}] {item.Clausula}: {item.Requisito}",
                        bodyFont,
                        XBrushes.Black
                    );
                }

                yPosition += 5;
                DrawWrappedText(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    "Aspetos Variáveis:",
                    boldFont,
                    XBrushes.Black
                );
                foreach (
                    var item in data.AspetosVariaveis ?? new List<AspetoVariavel>()
                )
                {
                    DrawWrappedText(
                        ref currentPage,
                        ref gfx,
                        document,
                        ref yPosition,
                        margin,
                        $"• {item.Titulo}: {item.Descricao}",
                        bodyFont,
                        XBrushes.Black
                    );
                }

                // 3. Prazos e Penalidades
                DrawSectionHeader(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    "3. Prazos e Penalidades"
                );
                DrawWrappedText(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    $"Vigência: {data.Vigencia} | Caução: {data.Caucao} | Pagamento: {data.Pagamento}",
                    bodyFont,
                    XBrushes.Black
                );
                yPosition += 5;
                foreach (var item in data.Penalidades ?? new List<Penalidade>())
                {
                    XBrush brush =
                        item.Nivel?.ToLower().Contains("grave") == true
                            ? XBrushes.DarkRed
                            : XBrushes.Black;
                    DrawWrappedText(
                        ref currentPage,
                        ref gfx,
                        document,
                        ref yPosition,
                        margin,
                        $"• [{item.Nivel}] {item.Exemplos} -> {item.Coima}",
                        bodyFont,
                        brush
                    );
                }

                // 4. Riscos
                DrawSectionHeader(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    "4. Matriz de Risco"
                );
                foreach (var item in data.Riscos ?? new List<Risco>())
                {
                    XBrush brush =
                        item.Tipo?.ToLower().Contains("alto") == true
                            ? XBrushes.Red
                            : XBrushes.Green;
                    DrawWrappedText(
                        ref currentPage,
                        ref gfx,
                        document,
                        ref yPosition,
                        margin,
                        $"[{item.Tipo}] {item.Titulo}",
                        boldFont,
                        brush
                    );
                    DrawWrappedText(
                        ref currentPage,
                        ref gfx,
                        document,
                        ref yPosition,
                        margin,
                        item.Descricao,
                        bodyFont,
                        XBrushes.Black
                    );
                    yPosition += 5;
                }

                // Conclusão
                DrawSectionHeader(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    "Conclusão Final"
                );
                DrawWrappedText(
                    ref currentPage,
                    ref gfx,
                    document,
                    ref yPosition,
                    margin,
                    data.Conclusao,
                    bodyFont,
                    XBrushes.Black
                );

                gfx.Dispose();

                // Rodapés
                int totalPages = document.PageCount;
                for (int i = 1; i <= totalPages; i++)
                {
                    using (XGraphics footerGfx = XGraphics.FromPdfPage(document.Pages[i - 1]))
                    {
                        DrawFooter(footerGfx, document.Pages[i - 1], i, totalPages);
                    }
                }

                document.Save(outputPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na geração do PDF: {ex.Message}");
            }
        }

        private void DrawSectionHeader(
            ref PdfPage page,
            ref XGraphics gfx,
            PdfDocument doc,
            ref double y,
            double margin,
            string title
        )
        {
            y += 15;
            CheckPageOverflow(ref page, ref gfx, doc, ref y, margin);
            gfx.DrawString(
                title,
                new XFont("Arial", 12, XFontStyle.Bold),
                XBrushes.DarkBlue,
                margin,
                y
            );
            y += 20;
        }

        private void DrawWrappedText(
            ref PdfPage page,
            ref XGraphics gfx,
            PdfDocument doc,
            ref double y,
            double margin,
            string text,
            XFont font,
            XBrush brush
        )
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var words = text.Split(' ');
            var line = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(line) ? word : line + " " + word;

                if (gfx.MeasureString(testLine, font).Width > page.Width - 2 * margin)
                {
                    CheckPageOverflow(ref page, ref gfx, doc, ref y, margin);
                    gfx.DrawString(line, font, brush, margin, y);
                    y += font.Height + 2;
                    line = word;
                }
                else
                {
                    line = testLine;
                }
            }

            if (!string.IsNullOrEmpty(line))
            {
                CheckPageOverflow(ref page, ref gfx, doc, ref y, margin);
                gfx.DrawString(line, font, brush, margin, y);
                y += font.Height + 2;
            }
        }

        private void CheckPageOverflow(
            ref PdfPage page,
            ref XGraphics gfx,
            PdfDocument doc,
            ref double y,
            double margin
        )
        {
            // Se a posição Y atingir o limite inferior (80px de margem de segurança para o rodapé)
            if (y > page.Height - 80)
            {
                gfx.Dispose();
                page = doc.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = 60; // Margem superior nas páginas subsequentes
            }
        }

        private void DrawHeader(XGraphics gfx, PdfPage page)
        {
            gfx.DrawRectangle(new XSolidBrush(_realLifeBlue), 0, 0, page.Width, 100);
            gfx.DrawString(
                "REALLIFE LAW ASSIST",
                new XFont("Arial", 14, XFontStyle.Bold),
                XBrushes.White,
                new XRect(0, 40, page.Width, 20),
                XStringFormats.Center
            );
        }

        private void DrawFooter(XGraphics gfx, PdfPage page, int pageNum, int totalPages)
        {
            string footerText = $"RealLife Law Assist © {DateTime.Now.Year}";
            string pageText = $"Página {pageNum} de {totalPages}";

            gfx.DrawLine(
                new XPen(XColors.LightGray, 0.5),
                50,
                page.Height - 45,
                page.Width - 50,
                page.Height - 45
            );
            gfx.DrawString(footerText, _footerFont, XBrushes.Gray, 50, page.Height - 30);

            var size = gfx.MeasureString(pageText, _footerFont);
            gfx.DrawString(
                pageText,
                _footerFont,
                XBrushes.Gray,
                page.Width - 50 - size.Width,
                page.Height - 30
            );
        }
        
        public int CalcularScoreRisco(AnaliseDados dados)
        {
            if (dados.Riscos == null) return 0;

            return dados.Riscos.Sum(r =>r.Tipo?.ToLower() switch
            {
            "alto" => 2,
            "medio" => 1,
            _ => 0
            });
        }
    }
}

