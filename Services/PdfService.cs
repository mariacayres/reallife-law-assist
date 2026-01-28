using System;
using System.IO;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace RealLifeLawAssist.Services
{
    public class PdfService
    {
        public void CreateAnalysisPdf(string outputPath, string title, string content)
        {
            try
            {
                Console.WriteLine($"Iniciando criação do PDF: {outputPath}");

                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
                var headerFont = new XFont("Arial", 14, XFontStyle.Bold);
                var bodyFont = new XFont("Arial", 11, XFontStyle.Regular);
                var footerFont = new XFont("Arial", 9, XFontStyle.Regular);

                double yPosition = 50;

                // Título
                gfx.DrawString(
                    "Relatório de Análise Jurídica AI",
                    titleFont,
                    XBrushes.Black,
                    new XRect(50, yPosition, page.Width, 30),
                    XStringFormats.TopLeft
                );
                yPosition += 40;

                // Documento original
                gfx.DrawString(
                    $"Documento Original: {title}",
                    bodyFont,
                    XBrushes.Black,
                    50,
                    yPosition
                );
                yPosition += 40;

                // Cabeçalho da análise
                gfx.DrawString("Análise:", headerFont, XBrushes.Black, 50, yPosition);
                yPosition += 30;

                double maxLineWidth = page.Width - 100;

                var lines = content.Split('\n');

                foreach (var line in lines)
                {
                    if (yPosition > page.Height - 100)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yPosition = 50;
                    }

                    var words = line.Split(' ');
                    var currentLine = "";

                    foreach (var word in words)
                    {
                        var testLine = string.IsNullOrEmpty(currentLine)
                            ? word
                            : currentLine + " " + word;

                        var size = gfx.MeasureString(testLine, bodyFont);

                        if (size.Width > maxLineWidth && !string.IsNullOrEmpty(currentLine))
                        {
                            gfx.DrawString(currentLine, bodyFont, XBrushes.Black, 50, yPosition);
                            yPosition += 20;
                            currentLine = word;
                        }
                        else
                        {
                            currentLine = testLine;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(currentLine))
                    {
                        gfx.DrawString(currentLine, bodyFont, XBrushes.Black, 50, yPosition);
                        yPosition += 20;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        yPosition += 10;
                    }
                }

                yPosition += 20;
                gfx.DrawString(
                    $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                    footerFont,
                    XBrushes.Gray,
                    50,
                    yPosition
                );

                document.Save(outputPath);
                document.Close();

                Console.WriteLine($"PDF criado com sucesso: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar PDF: {ex.Message}");
                throw;
            }
        }
    }
}
