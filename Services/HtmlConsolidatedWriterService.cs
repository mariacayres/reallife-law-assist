using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RealLifeLawAssist.Models;

namespace RealLifeLawAssist.Services
{
    public class HtmlConsolidatedWriterService
    {
        public void CreateConsolidatedHtml(
            string outputPath,
            List<AnaliseConsolidadaItem> itens
        )
        {
            var htmlContent = new StringBuilder();

            htmlContent.AppendLine("<!DOCTYPE html>");
            htmlContent.AppendLine("<html lang='pt'>");
            htmlContent.AppendLine("<head>");
            htmlContent.AppendLine("    <meta charset='utf-8' />");
            htmlContent.AppendLine("    <title>Relatório Consolidado de Riscos</title>");
            htmlContent.AppendLine("    <style>");
            htmlContent.AppendLine("        body { font-family: Arial, sans-serif; background: #f8fafc; padding: 30px; }");
            htmlContent.AppendLine("        h1 { margin-bottom: 20px; }");
            htmlContent.AppendLine("        table { border-collapse: collapse; width: 100%; background: white; }");
            htmlContent.AppendLine("        th, td { border: 1px solid #ddd; padding: 10px; vertical-align: top; }");
            htmlContent.AppendLine("        th { background: #f1f5f9; text-align: left; }");
            htmlContent.AppendLine("        .alto { color: #b91c1c; font-weight: bold; }");
            htmlContent.AppendLine("        .medio { color: #d97706; font-weight: bold; }");
            htmlContent.AppendLine("        .baixo { color: #15803d; font-weight: bold; }");
            htmlContent.AppendLine("    </style>");
            htmlContent.AppendLine("</head>");
            htmlContent.AppendLine("<body>");
            htmlContent.AppendLine("<h1>Relatório Consolidado de Riscos</h1>");
            htmlContent.AppendLine("<table>");
            htmlContent.AppendLine("    <tr>");
            htmlContent.AppendLine("        <th>Arquivo</th>");
            htmlContent.AppendLine("        <th>Título</th>");
            htmlContent.AppendLine("        <th>Score de Risco</th>");
            htmlContent.AppendLine("        <th>Riscos Identificados</th>");
            htmlContent.AppendLine("    </tr>");

            foreach (var item in itens)
            {
                htmlContent.AppendLine("    <tr>");
                htmlContent.AppendLine($"        <td>{item.Arquivo}</td>");
                htmlContent.AppendLine($"        <td>{item.Titulo}</td>");
                htmlContent.AppendLine($"        <td><strong>{item.ScoreRisco}</strong></td>");
                htmlContent.AppendLine("        <td>");

                foreach (var risco in item.Riscos)
                {
                    var css = risco.Tipo?.ToLower() ?? "baixo";
                    htmlContent.AppendLine($"            <div class='{css}'>{risco.Tipo} - {risco.Titulo}</div>");
                }

                htmlContent.AppendLine("        </td>");
                htmlContent.AppendLine("    </tr>");
            }

            htmlContent.AppendLine("</table>");
            htmlContent.AppendLine("</body>");
            htmlContent.AppendLine("</html>");

            File.WriteAllText(outputPath, htmlContent.ToString(), Encoding.UTF8);
        }
    }
}
