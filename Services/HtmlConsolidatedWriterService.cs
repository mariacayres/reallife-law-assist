using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using RealLifeLawAssist.Models;

namespace RealLifeLawAssist.Services
{
    public class HtmlConsolidatedWriterService
    {
        public void CreateConsolidatedHtml(string outputPath, List<AnaliseConsolidadaItem> itens)
        {
            var sb = new StringBuilder();

            string dataHoraGeracao = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='pt-pt'>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='UTF-8'>");
            sb.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.AppendLine("    <title>Relatório Consolidado de Riscos</title>");
            sb.AppendLine("    <script src='https://cdn.tailwindcss.com'></script>");
            sb.AppendLine("    <script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r121/three.min.js'></script>");
            sb.AppendLine("    <script src='https://cdn.jsdelivr.net/npm/vanta@latest/dist/vanta.waves.min.js'></script>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; background: #f8fafc; }");
            sb.AppendLine("        .alto { color: red; font-weight: bold; }");
            sb.AppendLine("        .medio { color: orange; }");
            sb.AppendLine("        .baixo { color: green; }");
            sb.AppendLine("        table { border-collapse: collapse; width: 100%; margin-top: 1rem; }");
            sb.AppendLine("        th, td { border: 1px solid #ccc; padding: 8px; }");
            sb.AppendLine("        th { background: #eee; }");
            sb.AppendLine("        header h1 { text-shadow: 1px 1px 4px rgba(0,0,0,0.3); }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header moderno com flexbox
            sb.AppendLine("    <header id='vanta-bg' class='relative overflow-hidden'>");
            sb.AppendLine("        <div class='max-w-6xl mx-auto flex items-center justify-between py-6 px-6'>");
            sb.AppendLine("            <!-- Logo à esquerda -->");
            sb.AppendLine("            <div class='flex items-center space-x-4'>");
            sb.AppendLine("                <img src='../img/logo.png' alt='Logo' class='h-12' />");
            sb.AppendLine("                <span class='text-white font-bold text-lg'>RealLife Law Assist</span>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <!-- Data/hora à direita -->");
            sb.AppendLine($"            <div class='text-white text-sm'>Gerado em: {dataHoraGeracao}</div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <!-- Título centralizado abaixo do header principal -->");
            sb.AppendLine("        <div class='text-center py-8'>");
            sb.AppendLine("            <h1 class='text-3xl md:text-4xl font-bold text-white'>Relatório Consolidado de Riscos</h1>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </header>");

            // Tabela de riscos
            sb.AppendLine("    <main class='max-w-6xl mx-auto py-10 px-6'>");
            sb.AppendLine("        <table class='shadow-lg'>");
            sb.AppendLine("            <tr class='bg-gray-200'>");
            sb.AppendLine("                <th>Arquivo</th>");
            sb.AppendLine("                <th>Título</th>");
            sb.AppendLine("                <th>Score de Risco</th>");
            sb.AppendLine("                <th>Riscos Identificados</th>");
            sb.AppendLine("            </tr>");

            foreach (var item in itens)
            {
                sb.AppendLine("            <tr>");
                sb.AppendLine($"                <td>{WebUtility.HtmlEncode(item.Arquivo)}</td>");
                sb.AppendLine($"                <td>{WebUtility.HtmlEncode(item.Titulo)}</td>");
                sb.AppendLine($"                <td>{item.ScoreRisco}</td>");
                sb.AppendLine("                <td>");
                foreach (var risco in item.Riscos)
                {
                    string css = risco.Tipo?.ToLower() switch
                    {
                        "alto" => "alto",
                        "medio" => "medio",
                        "baixo" => "baixo",
                        _ => "baixo"
                    };
                    sb.AppendLine($"<div class='{css}'>{WebUtility.HtmlEncode(risco.Tipo)} - {WebUtility.HtmlEncode(risco.Titulo)}</div>");
                }
                sb.AppendLine("                </td>");
                sb.AppendLine("            </tr>");
            }

            sb.AppendLine("        </table>");
            sb.AppendLine("    </main>");

            // Footer
            sb.AppendLine("    <footer class='text-center mt-10 mb-6 text-gray-600 text-sm'>");
            sb.AppendLine("        Relatório gerado automaticamente. Todos os direitos reservados © 2026");
            sb.AppendLine("    </footer>");

            // Script Vanta
            sb.AppendLine("<script>");
            sb.AppendLine("VANTA.WAVES({ el: '#vanta-bg', mouseControls: false, touchControls: false, gyroControls: false, minHeight: 200.0, minWidth: 200.0, scale: 1.0, scaleMobile: 1.0, color: 0x1e3a8a, shininess: 35, waveHeight: 20, waveSpeed: 0.6, zoom: 0.85 });");
            sb.AppendLine("</script>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }
    }
}
