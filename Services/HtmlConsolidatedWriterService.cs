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

        // ================= HTML CONSOLIDADO =================
        public void CreateConsolidatedHtml(
            string outputPath,
            List<AnaliseConsolidadaItem> itens)
        {
            var htmlContent = new StringBuilder();
            var dataProcessamento = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // ================= MÉTRICAS =================
            int riscoAlto = 0, riscoMedio = 0, riscoBaixo = 0;

            foreach (var item in itens)
            {
                foreach (var r in item.Riscos)
                {
                    var tipo = Normalizar(r.Tipo);

                    if (tipo.Contains("alto")) riscoAlto++;
                    else if (tipo.Contains("medio")) riscoMedio++;
                    else if (tipo.Contains("baixo")) riscoBaixo++;
                }
            }

            // ================= HTML =================
            htmlContent.AppendLine("<!DOCTYPE html>");
            htmlContent.AppendLine("<html lang='pt-pt'>");
            htmlContent.AppendLine("<head>");
            htmlContent.AppendLine("  <meta charset='UTF-8'>");
            htmlContent.AppendLine("  <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            htmlContent.AppendLine("  <title>Dashboard Consolidado de Risco Contratual</title>");
            htmlContent.AppendLine("  <script src='https://cdn.tailwindcss.com'></script>");
            htmlContent.AppendLine("  <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
            htmlContent.AppendLine("  <script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r121/three.min.js'></script>");
            htmlContent.AppendLine("  <script src='https://cdn.jsdelivr.net/npm/vanta@latest/dist/vanta.waves.min.js'></script>");
            htmlContent.AppendLine("  <style>");
            htmlContent.AppendLine("    #vanta-bg { min-height: 220px; }");
            htmlContent.AppendLine("    @media (max-width: 768px) {");
            htmlContent.AppendLine("      #vanta-bg { min-height: 180px; padding: 2rem 1rem; }");
            htmlContent.AppendLine("      #vanta-bg h1 { font-size: 1.5rem; }");
            htmlContent.AppendLine("      #vanta-bg p { font-size: 0.875rem; }");
            htmlContent.AppendLine("    }");
            htmlContent.AppendLine("  </style>");
            htmlContent.AppendLine("</head>");

            htmlContent.AppendLine("<body class='bg-gray-50 text-gray-900'>");

            // ================= HEADER CORRIGIDO =================
            // Aumentei a altura da caixa azul e ajustei o padding
            htmlContent.AppendLine(
                "<header id='vanta-bg' style='background-color:#1e3a8a' class='text-white py-16 px-6 relative overflow-hidden'>");
            htmlContent.AppendLine("  <div class='max-w-7xl mx-auto relative z-10 px-4'>"); // Added px-4 for mobile
            htmlContent.AppendLine("    <h1 class='text-4xl font-bold mb-3 leading-tight'>Dashboard Consolidado de Risco Contratual</h1>");
            htmlContent.AppendLine($"    <p class='text-blue-200 text-lg font-light'>Processado em {dataProcessamento} · {itens.Count} documentos analisados</p>");
            htmlContent.AppendLine("  </div>");
            htmlContent.AppendLine("</header>");

            // ================= MAIN =================
            htmlContent.AppendLine("<main class='max-w-7xl mx-auto px-4 md:px-6 py-10 space-y-10'>");

            // ================= KPIs =================
            htmlContent.AppendLine("<div class='grid md:grid-cols-4 gap-4'>");
            htmlContent.AppendLine($"<div class='bg-white p-4 rounded-xl border shadow-sm'><p class='text-xs uppercase text-gray-500'>Documentos</p><p class='text-2xl font-bold'>{itens.Count}</p></div>");
            htmlContent.AppendLine($"<div class='bg-white p-4 rounded-xl border shadow-sm'><p class='text-xs uppercase text-gray-500'>Risco Alto</p><p class='text-2xl font-bold text-red-600'>{riscoAlto}</p></div>");
            htmlContent.AppendLine($"<div class='bg-white p-4 rounded-xl border shadow-sm'><p class='text-xs uppercase text-gray-500'>Risco Médio</p><p class='text-2xl font-bold text-amber-500'>{riscoMedio}</p></div>");
            htmlContent.AppendLine($"<div class='bg-white p-4 rounded-xl border shadow-sm'><p class='text-xs uppercase text-gray-500'>Risco Baixo</p><p class='text-2xl font-bold text-green-600'>{riscoBaixo}</p></div>");
            htmlContent.AppendLine("</div>");

            // ================= GRÁFICO =================
            htmlContent.AppendLine("<div class='bg-white p-6 rounded-xl border shadow-sm h-[360px]'>");
            htmlContent.AppendLine("  <canvas id='graficoNivel' class='w-full h-full'></canvas>");
            htmlContent.AppendLine("</div>");

            // ================= FILTROS =================
            htmlContent.AppendLine("<div class='flex flex-wrap gap-4 items-center'>");
            htmlContent.AppendLine("  <div class='flex-1 min-w-[250px]'>");
            htmlContent.AppendLine("    <input id='filtroTexto' placeholder='Pesquisar por título, descrição...' onkeyup='aplicarFiltros()' class='p-3 border rounded-lg w-full focus:ring-2 focus:ring-blue-500 focus:border-blue-500' />");
            htmlContent.AppendLine("  </div>");
            htmlContent.AppendLine("  <div class='flex-shrink-0'>");
            htmlContent.AppendLine("    <select id='filtroRisco' onchange='aplicarFiltros()' class='p-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500'>");
            htmlContent.AppendLine("      <option value=''>Todos os riscos</option>");
            htmlContent.AppendLine("      <option value='alto'>Risco Alto</option>");
            htmlContent.AppendLine("      <option value='medio'>Risco Médio</option>");
            htmlContent.AppendLine("      <option value='baixo'>Risco Baixo</option>");
            htmlContent.AppendLine("    </select>");
            htmlContent.AppendLine("  </div>");
            htmlContent.AppendLine("</div>");

            // ================= CARDS =================
            htmlContent.AppendLine("<div id='cards-container' class='grid md:grid-cols-2 lg:grid-cols-3 gap-6'>");

            foreach (var item in itens)
            {
                var nivel =
                    item.ScoreRisco >= 8 ? "alto" :
                    item.ScoreRisco >= 4 ? "medio" : "baixo";

                bool temFavorecimento = item.Riscos.Any(r =>
                    Normalizar(r.Titulo).Contains("favorecimento"));

                // Badges do item
                var badgesHtml = new StringBuilder();
                if (item.Badges != null && item.Badges.Count > 0)
                {
                    foreach (var badge in item.Badges.Take(3)) // Limita a 3 badges
                    {
                        badgesHtml.Append($"<span class='inline-block text-xs px-2 py-1 rounded bg-gray-100 text-gray-700 mr-1 mb-1'>{WebUtility.HtmlEncode(badge)}</span>");
                    }
                }

                htmlContent.AppendLine(
                    $"<div class='bg-white p-6 rounded-xl border-l-4 {(nivel == "alto" ? "border-red-600" : nivel == "medio" ? "border-amber-500" : "border-green-600")} shadow-sm hover:shadow-md transition-shadow duration-300 card' data-risco='{nivel}'>");

                // Título e badges
                htmlContent.AppendLine($"<h3 class='font-bold text-lg mb-2 text-gray-800'>{WebUtility.HtmlEncode(item.Titulo)}</h3>");
                
                if (badgesHtml.Length > 0)
                {
                    htmlContent.AppendLine($"<div class='mb-3'>{badgesHtml}</div>");
                }

                // Score e descrição
                htmlContent.AppendLine($"<p class='text-sm text-gray-600 mb-1'>Score de Risco: <strong class='{(nivel == "alto" ? "text-red-600" : nivel == "medio" ? "text-amber-500" : "text-green-600")}'>{item.ScoreRisco}</strong></p>");
                
                if (!string.IsNullOrEmpty(item.Descricao))
                {
                    htmlContent.AppendLine($"<p class='text-sm text-gray-500 mb-3 line-clamp-2'>{WebUtility.HtmlEncode(item.Descricao)}</p>");
                }

                // Badge de favorecimento
                if (temFavorecimento)
                    htmlContent.AppendLine("<span class='inline-block text-xs px-2 py-1 rounded bg-red-100 text-red-700 font-semibold mb-3'>⚠️ Possível Favorecimento</span>");

                // Riscos (collapse)
                htmlContent.AppendLine("<details class='mt-3'>");
                htmlContent.AppendLine("<summary class='cursor-pointer text-sm font-medium text-blue-600 hover:text-blue-800 flex items-center'>");
                htmlContent.AppendLine($"<span>Ver {item.Riscos.Count} risco(s)</span>");
                htmlContent.AppendLine("<svg class='w-4 h-4 ml-1 transition-transform duration-300' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M19 9l-7 7-7-7'></path></svg>");
                htmlContent.AppendLine("</summary>");
                htmlContent.AppendLine("<ul class='mt-3 text-sm text-gray-600 space-y-2'>");

                foreach (var r in item.Riscos)
                {
                    var riscoCor = r.Tipo?.ToLower() switch
                    {
                        "alto" => "text-red-600",
                        "medio" or "médio" => "text-amber-600",
                        _ => "text-green-600"
                    };
                    htmlContent.AppendLine($"<li class='flex items-start'>");
                    htmlContent.AppendLine($"  <span class='{riscoCor} mr-2'>•</span>");
                    htmlContent.AppendLine($"  <span>{WebUtility.HtmlEncode(r.Titulo)}</span>");
                    htmlContent.AppendLine("</li>");
                }

                htmlContent.AppendLine("</ul>");
                htmlContent.AppendLine("</details>");

                // Link para análise completa
                if (!string.IsNullOrEmpty(item.OutputHtmlPath))
                {
                    htmlContent.AppendLine(
                        $"<a href='{item.OutputHtmlPath}' target='_blank' class='inline-block mt-4 text-blue-700 text-sm font-semibold hover:text-blue-900 transition-colors duration-200'>");
                    htmlContent.AppendLine("  Ver análise completa →");
                    htmlContent.AppendLine("</a>");
                }

                htmlContent.AppendLine("</div>");
            }

            htmlContent.AppendLine("</div>");
            
            // Mensagem se não houver itens após filtro
            htmlContent.AppendLine("<div id='sem-resultados' class='hidden text-center py-12'>");
            htmlContent.AppendLine("  <p class='text-gray-500 text-lg'>Nenhum documento encontrado com os filtros atuais.</p>");
            htmlContent.AppendLine("  <button onclick='limparFiltros()' class='mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors'>Limpar filtros</button>");
            htmlContent.AppendLine("</div>");
            
            htmlContent.AppendLine("</main>");

            // ================= FOOTER =================
            htmlContent.AppendLine("<footer class='bg-white border-t py-8 text-center'>");
            htmlContent.AppendLine("  <div class='max-w-7xl mx-auto px-4'>");
            htmlContent.AppendLine("    <p class='text-xs text-gray-400 mb-2'>Dashboard técnico gerado para análise jurídica</p>");
            htmlContent.AppendLine("    <p class='text-xs text-gray-400'>RealLife Law Assist © " + DateTime.Now.Year + "</p>");
            htmlContent.AppendLine("  </div>");
            htmlContent.AppendLine("</footer>");

            // ================= SCRIPTS =================
            htmlContent.AppendLine("<script>");

            htmlContent.AppendLine(@"
                function aplicarFiltros() {
                    const texto = document.getElementById('filtroTexto').value.toLowerCase();
                    const risco = document.getElementById('filtroRisco').value;
                    const cards = document.querySelectorAll('.card');
                    let visiveis = 0;

                    cards.forEach(card => {
                        const textoCard = card.innerText.toLowerCase();
                        const nivelCard = card.dataset.risco;
                        const correspondeTexto = texto === '' || textoCard.includes(texto);
                        const correspondeRisco = risco === '' || nivelCard === risco;
                        
                        if (correspondeTexto && correspondeRisco) {
                            card.style.display = 'block';
                            visiveis++;
                        } else {
                            card.style.display = 'none';
                        }
                    });

                    // Mostra/oculta mensagem de sem resultados
                    const semResultados = document.getElementById('sem-resultados');
                    if (visiveis === 0) {
                        semResultados.classList.remove('hidden');
                    } else {
                        semResultados.classList.add('hidden');
                    }
                }

                function limparFiltros() {
                    document.getElementById('filtroTexto').value = '';
                    document.getElementById('filtroRisco').value = '';
                    aplicarFiltros();
                }

                // Anima os detalhes (expandir/recolher)
                document.querySelectorAll('details').forEach(details => {
                    const summary = details.querySelector('summary');
                    const svg = summary.querySelector('svg');
                    
                    details.addEventListener('toggle', () => {
                        if (details.open) {
                            svg.style.transform = 'rotate(180deg)';
                        } else {
                            svg.style.transform = 'rotate(0deg)';
                        }
                    });
                });
            ");

            htmlContent.AppendLine($@"
                // Gráfico de pizza/doughnut
                const ctx = document.getElementById('graficoNivel').getContext('2d');
                new Chart(ctx, {{
                    type: 'doughnut',
                    data: {{
                        labels: ['Alto', 'Médio', 'Baixo'],
                        datasets: [{{
                            data: [{riscoAlto}, {riscoMedio}, {riscoBaixo}],
                            backgroundColor: ['#dc2626', '#f59e0b', '#16a34a'],
                            borderWidth: 2,
                            borderColor: '#ffffff'
                        }}]
                    }},
                    options: {{
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {{
                            legend: {{
                                position: 'bottom',
                                labels: {{
                                    padding: 20,
                                    font: {{
                                        size: 14
                                    }}
                                }}
                            }},
                            tooltip: {{
                                callbacks: {{
                                    label: function(context) {{
                                        const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                        const value = context.raw;
                                        const percentage = total > 0 ? Math.round((value / total) * 100) : 0;
                                        return `${{context.label}}: ${{value}} (${{percentage}}%)`;
                                    }}
                                }}
                            }}
                        }}
                    }}
                }});
            ");

            // ================= VANTA BACKGROUND =================
            htmlContent.AppendLine(@"
                // Efeito de fundo animado
                if (typeof VANTA !== 'undefined') {
                    VANTA.WAVES({
                        el: '#vanta-bg',
                        mouseControls: false,
                        touchControls: false,
                        gyroControls: false,
                        minHeight: 220.0,
                        scale: 1.0,
                        scaleMobile: 1.0,
                        color: 0x1e3a8a,
                        shininess: 35,
                        waveHeight: 18,
                        waveSpeed: 0.6,
                        zoom: 0.85
                    });
                }

                // Inicializar filtros
                document.addEventListener('DOMContentLoaded', function() {
                    aplicarFiltros();
                });
            ");

            htmlContent.AppendLine("</script>");
            htmlContent.AppendLine("</body>");
            htmlContent.AppendLine("</html>");

            File.WriteAllText(outputPath, htmlContent.ToString(), Encoding.UTF8);
        }
    }
}