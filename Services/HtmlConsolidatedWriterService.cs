using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using RealLifeLawAssist.Models;

namespace RealLifeLawAssist.Services
{
    public class HtmlConsolidatedWriterService
    {
        // ================= NORMALIZAÇÃO =================
        // Método privado para normalizar texto: remove acentos e converte para minúsculas
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

        // ================= CLASSIFICAÇÃO DE RISCO =================
        // Método para classificar o tipo de risco baseado no título normalizado
        private static string ClassificarTipoRisco(string titulo)
        {
            var t = Normalizar(titulo);

            if (t.Contains("favorecimento")) return "Favorecimento";
            if (t.Contains("preco")) return "Preço";
            if (t.Contains("prazo")) return "Prazo";
            if (t.Contains("penal")) return "Penalidades";
            if (t.Contains("concorr")) return "Concorrência";

            return "Outros";
        }

        // ================= HTML CONSOLIDADO =================
        // Método público para criar um arquivo HTML consolidado com dashboard de riscos
        public void CreateConsolidatedHtml(
            string outputPath,
            List<AnaliseConsolidadaItem> itens)
        {
            var htmlContent = new StringBuilder();
            var dataProcessamento = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // ================= MÉTRICAS =================
            // Inicializa contadores para níveis de risco e dicionário para tipos de risco
            int riscoAlto = 0, riscoMedio = 0, riscoBaixo = 0;
            var tiposRisco = new Dictionary<string, int>();

            // Itera sobre os itens e riscos para calcular métricas
            foreach (var item in itens)
            {
                foreach (var r in item.Riscos)
                {
                    var tipo = Normalizar(r.Tipo);

                    if (tipo.Contains("alto")) riscoAlto++;
                    else if (tipo.Contains("medio")) riscoMedio++;
                    else if (tipo.Contains("baixo")) riscoBaixo++;

                    var categoria = ClassificarTipoRisco(r.Titulo);
                    tiposRisco[categoria] =
                        tiposRisco.ContainsKey(categoria)
                            ? tiposRisco[categoria] + 1
                            : 1;
                }
            }

            // ================= JS ARRAYS =================
            // Prepara arrays JavaScript para labels e valores dos tipos de risco
            var jsTipoLabels = string.Join(",",
                tiposRisco.Keys.Select(k => $"'{WebUtility.HtmlEncode(k)}'"));

            var jsTipoValues = string.Join(",",
                tiposRisco.Values);

            // ================= HTML =================
            // Inicia a construção do HTML
            htmlContent.AppendLine("<!DOCTYPE html>");
            htmlContent.AppendLine("<html lang='pt-pt'>");
            htmlContent.AppendLine("<head>");
            htmlContent.AppendLine("  <meta charset='UTF-8'>");
            htmlContent.AppendLine("  <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            htmlContent.AppendLine("  <title>Dashboard Consolidado de Risco</title>");
            htmlContent.AppendLine("  <script src='https://cdn.tailwindcss.com'></script>");
            htmlContent.AppendLine("  <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
            htmlContent.AppendLine("</head>");

            htmlContent.AppendLine("<body class='bg-gray-50 text-gray-900'>");

            // ================= HEADER =================
            // Adiciona cabeçalho com título e data de processamento
            htmlContent.AppendLine("<header class='bg-slate-900 text-white py-10 px-6'>");
            htmlContent.AppendLine("  <div class='max-w-7xl mx-auto'>");
            htmlContent.AppendLine("    <h1 class='text-3xl font-bold'>Dashboard Consolidado de Risco Contratual</h1>");
            htmlContent.AppendLine($"    <p class='text-slate-400 mt-2'>Processado em {dataProcessamento} · {itens.Count} documentos analisados</p>");
            htmlContent.AppendLine("  </div>");
            htmlContent.AppendLine("</header>");

            // ================= MAIN =================
            // Inicia seção principal
            htmlContent.AppendLine("<main class='max-w-7xl mx-auto px-6 py-10 space-y-10'>");

            // ================= KPIs =================
            // Adiciona cartões com KPIs: documentos, riscos alto/médio/baixo
            htmlContent.AppendLine("<div class='grid md:grid-cols-4 gap-4'>");
            htmlContent.AppendLine($"<div class='bg-white p-4 rounded-xl border'><p class='text-xs uppercase text-gray-500'>Documentos</p><p class='text-2xl font-bold'>{itens.Count}</p></div>");
            htmlContent.AppendLine($"<div class='bg-white p-4 rounded-xl border'><p class='text-xs uppercase text-gray-500'>Risco Alto</p><p class='text-2xl font-bold text-red-600'>{riscoAlto}</p></div>");
            htmlContent.AppendLine($"<div class='bg-white p-4 rounded-xl border'><p class='text-xs uppercase text-gray-500'>Risco Médio</p><p class='text-2xl font-bold text-amber-500'>{riscoMedio}</p></div>");
            htmlContent.AppendLine($"<div class='bg-white p-4 rounded-xl border'><p class='text-xs uppercase text-gray-500'>Risco Baixo</p><p class='text-2xl font-bold text-green-600'>{riscoBaixo}</p></div>");
            htmlContent.AppendLine("</div>");

            // ================= GRÁFICOS =================
            // Adiciona seção com gráficos (canvas para Chart.js)
            htmlContent.AppendLine("<div class='grid md:grid-cols-2 gap-6'>");
            htmlContent.AppendLine("<div class='bg-white p-6 rounded-xl border'><canvas id='graficoNivel'></canvas></div>");
            htmlContent.AppendLine("<div class='bg-white p-6 rounded-xl border'><canvas id='graficoTipo'></canvas></div>");
            htmlContent.AppendLine("</div>");

            // ================= FILTROS =================
            // Adiciona controles de filtro: texto e risco
            htmlContent.AppendLine("<div class='flex flex-wrap gap-4'>");
            htmlContent.AppendLine("<input id='filtroTexto' placeholder='Pesquisar...' onkeyup='aplicarFiltros()' class='p-2 border rounded-lg w-64' />");
            htmlContent.AppendLine("<select id='filtroRisco' onchange='aplicarFiltros()' class='p-2 border rounded-lg'>");
            htmlContent.AppendLine("<option value=''>Todos os riscos</option>");
            htmlContent.AppendLine("<option value='alto'>Risco Alto</option>");
            htmlContent.AppendLine("<option value='medio'>Risco Médio</option>");
            htmlContent.AppendLine("<option value='baixo'>Risco Baixo</option>");
            htmlContent.AppendLine("</select>");
            htmlContent.AppendLine("</div>");

            // ================= CARDS =================
            // Adiciona cartões para cada item analisado
            htmlContent.AppendLine("<div class='grid md:grid-cols-3 gap-6'>");

            foreach (var item in itens)
            {
                // Determina o nível de risco baseado no score
                var nivel =
                    item.ScoreRisco >= 8 ? "alto" :
                    item.ScoreRisco >= 4 ? "medio" : "baixo";

                // Verifica se há risco de favorecimento
                bool temFavorecimento = item.Riscos.Any(r =>
                    Normalizar(r.Titulo).Contains("favorecimento"));

                // Adiciona cartão com detalhes do item
                htmlContent.AppendLine(
                    $"<div class='bg-white p-6 rounded-xl border-l-4 {(nivel == "alto" ? "border-red-600" : nivel == "medio" ? "border-amber-500" : "border-green-600")} card' data-risco='{nivel}'>");

                htmlContent.AppendLine($"<h3 class='font-bold text-lg mb-1'>{WebUtility.HtmlEncode(item.Titulo)}</h3>");
                htmlContent.AppendLine($"<p class='text-sm text-gray-600 mb-2'>Score: <strong>{item.ScoreRisco}</strong></p>");

                if (temFavorecimento)
                    htmlContent.AppendLine("<span class='inline-block text-xs px-2 py-1 rounded bg-red-100 text-red-700 mr-1'>Possível Favorecimento</span>");

                htmlContent.AppendLine("<details class='mt-3'>");
                htmlContent.AppendLine("<summary class='cursor-pointer text-sm text-blue-600'>Ver riscos</summary>");
                htmlContent.AppendLine("<ul class='mt-2 text-sm text-gray-600'>");

                foreach (var r in item.Riscos)
                    htmlContent.AppendLine($"<li>• {WebUtility.HtmlEncode(r.Titulo)}</li>");

                htmlContent.AppendLine("</ul>");
                htmlContent.AppendLine("</details>");

                htmlContent.AppendLine(
                    $"<a href='{item.OutputHtmlPath}' target='_blank' class='inline-block mt-4 text-blue-700 text-sm font-semibold'>Ver análise completa →</a>");

                htmlContent.AppendLine("</div>");
            }

            htmlContent.AppendLine("</div>");
            htmlContent.AppendLine("</main>");

            // ================= FOOTER =================
            // Adiciona rodapé
            htmlContent.AppendLine("<footer class='bg-white border-t py-6 text-center text-xs text-gray-400'>Dashboard técnico gerado para análise jurídica · RealLife Law Assist</footer>");

            // ================= SCRIPTS =================
            // Adiciona scripts JavaScript para filtros e gráficos
            htmlContent.AppendLine("<script>");

            // Função para aplicar filtros nos cartões
            htmlContent.AppendLine(@"
function aplicarFiltros() {
  const t = document.getElementById('filtroTexto').value.toLowerCase();
  const r = document.getElementById('filtroRisco').value;

  document.querySelectorAll('.card').forEach(c => {
    const okT = c.innerText.toLowerCase().includes(t);
    const okR = !r || c.dataset.risco === r;
    c.style.display = okT && okR ? 'block' : 'none';
  });
}
");

            // Inicializa gráfico de níveis de risco
            htmlContent.AppendLine($@"
new Chart(document.getElementById('graficoNivel'), {{
  type: 'doughnut',
  data: {{
    labels: ['Alto', 'Médio', 'Baixo'],
    datasets: [{{
      data: [{riscoAlto}, {riscoMedio}, {riscoBaixo}],
      backgroundColor: ['#dc2626', '#f59e0b', '#16a34a']
    }}]
  }}
}});
");

            // Inicializa gráfico de tipos de risco
            htmlContent.AppendLine($@"
new Chart(document.getElementById('graficoTipo'), {{
  type: 'bar',
  data: {{
    labels: [{jsTipoLabels}],
    datasets: [{{
      data: [{jsTipoValues}],
      backgroundColor: '#2563eb'
    }}]
  }},
  options: {{
    plugins: {{ legend: {{ display: false }} }},
    scales: {{ y: {{ beginAtZero: true, ticks: {{ stepSize: 1 }} }} }}
  }}
}});
");

            htmlContent.AppendLine("</script>");
            htmlContent.AppendLine("</body>");
            htmlContent.AppendLine("</html>");

            // Escreve o conteúdo HTML no arquivo de saída
            File.WriteAllText(outputPath, htmlContent.ToString(), Encoding.UTF8);
        }
    }
}