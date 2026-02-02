using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using RealLifeLawAssist.Models;
using System.Globalization;

namespace RealLifeLawAssist.Services
{
    public class HtmlConsolidatedWriterService
    {
        // Normaliza texto (remover acentos e minúsculas)
        private static string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "";
            var normalized = texto.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                    != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().ToLowerInvariant();
        }

        public void CreateConsolidatedHtml(string outputPath, List<AnaliseConsolidadaItem> itens)
        {
            try
            {
                var htmlContent = new StringBuilder();
                string dataProcessamento = DateTime.Now.ToString("dd/MM/yyyy HH:mm", new CultureInfo("pt-PT"));

                int riscosAltos = 0;
                int riscosMedios = 0;
                int riscosBaixos = 0;

                string consolidadoDir = Path.GetDirectoryName(outputPath) ?? ".";

                // Contagem de riscos
                foreach (var item in itens ?? new List<AnaliseConsolidadaItem>())
                {
                    foreach (var r in item.Riscos ?? new List<RiscoItem>())
                    {
                        var tipo = Normalizar(r.Tipo ?? "");
                        if (tipo.Contains("alto")) riscosAltos++;
                        else if (tipo.Contains("medio")) riscosMedios++;
                        else if (tipo.Contains("baixo")) riscosBaixos++;
                    }
                }

                // HTML inicial
                htmlContent.AppendLine("<!DOCTYPE html>");
                htmlContent.AppendLine("<html lang='pt-pt'>");
                htmlContent.AppendLine("<head>");
                htmlContent.AppendLine("  <meta charset='UTF-8'>");
                htmlContent.AppendLine("  <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
                htmlContent.AppendLine("  <title>Relatório Consolidado de Risco</title>");
                htmlContent.AppendLine("  <script src='https://cdn.tailwindcss.com'></script>");
                htmlContent.AppendLine("  <script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r121/three.min.js'></script>");
                htmlContent.AppendLine("  <script src='https://cdn.jsdelivr.net/npm/vanta@latest/dist/vanta.waves.min.js'></script>");
                htmlContent.AppendLine("  <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
                htmlContent.AppendLine("  <style>");
                htmlContent.AppendLine("    @import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;600;700&display=swap');");
                htmlContent.AppendLine("    body { font-family: 'Inter', sans-serif; background-color: #f8fafc; }");
                htmlContent.AppendLine("    .card { transition: transform 0.2s; }");
                htmlContent.AppendLine("    .card:hover { transform: translateY(-4px); }");
                htmlContent.AppendLine("  </style>");
                htmlContent.AppendLine("</head>");
                htmlContent.AppendLine("<body>");

                // HEADER com VANTA
                htmlContent.AppendLine("<header id='vanta-bg' class='text-white py-16 px-6 relative overflow-hidden'>");
                htmlContent.AppendLine("  <div class='max-w-7xl mx-auto flex items-center justify-between mb-6'>");
                htmlContent.AppendLine("    <div class='flex items-center space-x-4'>");
                htmlContent.AppendLine("      <img src='../img/logo.png' class='h-12' alt='RealLife Law Assist' />");
                htmlContent.AppendLine("      <span class='font-bold text-lg'>RealLife Law Assist</span>");
                htmlContent.AppendLine("    </div>");
                htmlContent.AppendLine($"    <div class='text-sm text-slate-300'>Processado em {dataProcessamento}</div>");
                htmlContent.AppendLine("  </div>");
                htmlContent.AppendLine("  <div class='text-center'>");
                htmlContent.AppendLine("    <h1 class='text-3xl md:text-4xl font-bold tracking-tight'>Relatório Consolidado de Riscos</h1>");
                htmlContent.AppendLine("  </div>");
                htmlContent.AppendLine("</header>");

                // MAIN
                htmlContent.AppendLine("<main class='max-w-7xl mx-auto px-6 py-12 space-y-12'>");

                // GRÁFICO
                htmlContent.AppendLine("<section class='bg-white rounded-xl shadow-sm border border-gray-200 p-8'>");
                htmlContent.AppendLine("  <h2 class='text-xl font-bold mb-6 text-slate-800'>Distribuição dos Riscos Identificados</h2>");
                htmlContent.AppendLine("  <div class='max-w-md mx-auto'>");
                htmlContent.AppendLine("    <canvas id='riskChart'></canvas>");
                htmlContent.AppendLine("  </div>");
                htmlContent.AppendLine("</section>");

                // FILTRO
                htmlContent.AppendLine("<section class='mb-6'>");
                htmlContent.AppendLine("  <input id='filtro' placeholder='Filtrar por texto...' ");
                htmlContent.AppendLine("         class='w-full md:w-1/2 p-3 border border-gray-300 rounded-lg shadow-sm' ");
                htmlContent.AppendLine("         onkeyup='filtrar()' />");
                htmlContent.AppendLine("</section>");

                // CARDS
                htmlContent.AppendLine("<section class='grid md:grid-cols-2 lg:grid-cols-3 gap-6'>");

                foreach (var item in itens ?? new List<AnaliseConsolidadaItem>())
                {
                    string nivelClasse =
                        item.ScoreRisco >= 8 ? "border-red-500 bg-red-50" :
                        item.ScoreRisco >= 4 ? "border-yellow-400 bg-yellow-50" :
                        "border-green-500 bg-green-50";

                    htmlContent.AppendLine($"<div class='card bg-white rounded-xl shadow-sm border-l-4 {nivelClasse} p-6'>");

                    htmlContent.AppendLine("  <div class='flex justify-between items-start mb-2'>");
                    htmlContent.AppendLine($"    <h3 class='font-bold text-slate-800 text-lg'>{WebUtility.HtmlEncode(item.Titulo ?? "")}</h3>");
                    htmlContent.AppendLine($"    <span class='text-xl font-bold text-slate-700'>{item.ScoreRisco}</span>");
                    htmlContent.AppendLine("  </div>");

                    // BADGES
                    htmlContent.AppendLine("  <div class='flex flex-wrap gap-2 mb-3'>");
                    foreach (var badge in item.Badges ?? new List<string>())
                    {
                        htmlContent.AppendLine(
                            $"    <span class='text-xs font-bold px-2 py-1 rounded bg-blue-100 text-blue-800'>{WebUtility.HtmlEncode(badge)}</span>");
                    }
                    htmlContent.AppendLine("  </div>");

                    htmlContent.AppendLine($"  <p class='text-sm text-gray-600 mb-4'>{WebUtility.HtmlEncode(item.Descricao ?? "")}</p>");

                    htmlContent.AppendLine("  <div class='space-y-1 text-xs text-gray-700'>");
                    foreach (var r in item.Riscos ?? new List<RiscoItem>())
                    {
                        htmlContent.AppendLine(
                            $"    <div>• <strong>{WebUtility.HtmlEncode(r.Tipo ?? "")}</strong> – {WebUtility.HtmlEncode(r.Titulo ?? "")}</div>");
                    }
                    htmlContent.AppendLine("  </div>");

                    // LINK PARA HTML INDIVIDUAL (sempre visível)
                    string relativePath = !string.IsNullOrWhiteSpace(item.HtmlPath)
                        ? Path.GetRelativePath(consolidadoDir, item.HtmlPath).Replace("\\", "/")
                        : "#";

                    string linkClass = !string.IsNullOrWhiteSpace(item.HtmlPath)
                        ? "text-blue-700 hover:underline"
                        : "text-gray-400 cursor-not-allowed";

                    htmlContent.AppendLine(
                        $"  <a href='{WebUtility.HtmlEncode(relativePath)}' target='_blank' class='inline-block mt-4 text-sm font-semibold {linkClass}'>Ver análise completa →</a>");

                    htmlContent.AppendLine("</div>");
                }

                htmlContent.AppendLine("</section>");
                htmlContent.AppendLine("</main>");

                // FOOTER
                htmlContent.AppendLine("<footer class='bg-white border-t border-gray-200 py-6 text-center text-gray-400 text-xs'>");
                htmlContent.AppendLine($"Relatório consolidado gerado automaticamente em {dataProcessamento} | RealLife Law Assist");
                htmlContent.AppendLine("</footer>");

                // SCRIPT FILTRO E GRÁFICO
                htmlContent.AppendLine("<script>");
                htmlContent.AppendLine("function filtrar() {");
                htmlContent.AppendLine("  const q = document.getElementById('filtro').value.toLowerCase();");
                htmlContent.AppendLine("  document.querySelectorAll('.card').forEach(c => {");
                htmlContent.AppendLine("    c.style.display = c.innerText.toLowerCase().includes(q) ? 'block' : 'none';");
                htmlContent.AppendLine("  });");
                htmlContent.AppendLine("}");
                htmlContent.AppendLine("new Chart(document.getElementById('riskChart'), {");
                htmlContent.AppendLine("  type: 'doughnut',");
                htmlContent.AppendLine("  data: {");
                htmlContent.AppendLine("    labels: ['Risco Alto', 'Risco Médio', 'Risco Baixo'],");
                htmlContent.AppendLine($"    datasets: [{{ data: [{riscosAltos}, {riscosMedios}, {riscosBaixos}], backgroundColor: ['#dc2626','#facc15','#16a34a'] }}]");
                htmlContent.AppendLine("  },");
                htmlContent.AppendLine("  options: { plugins: { legend: { position: 'bottom' } } }");
                htmlContent.AppendLine("});");

                // VANTA HEADER
                htmlContent.AppendLine(@"
VANTA.WAVES({
  el: '#vanta-bg',
  mouseControls: false,
  touchControls: false,
  gyroControls: false,
  minHeight: 300.0,
  minWidth: 200.0,
  scale: 1.0,
  scaleMobile: 1.0,
  color: 0x1e3a8a,
  shininess: 35,
  waveHeight: 20,
  waveSpeed: 0.6,
  zoom: 0.85
});
");

                htmlContent.AppendLine("</script>");
                htmlContent.AppendLine("</body>");
                htmlContent.AppendLine("</html>");

                File.WriteAllText(outputPath, htmlContent.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao gerar HTML consolidado: {ex.Message}");
            }
        }
    }
}
