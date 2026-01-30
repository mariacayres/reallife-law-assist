using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using RealLifeLawAssist.Models;

namespace RealLifeLawAssist.Services
{
    public class HtmlWriterService
    {
        public void CreateAnalysisHtml(
            string outputPath,
            string originalFileName,
            AnaliseDados data
        )
        {
            try
            {
                var htmlContent = new StringBuilder();
                var culture = new CultureInfo("pt-PT");

                htmlContent.AppendLine("<!DOCTYPE html>");
                htmlContent.AppendLine("<html lang='pt-pt'>");
                htmlContent.AppendLine("<head>");
                htmlContent.AppendLine("    <meta charset='UTF-8'>");
                htmlContent.AppendLine(
                    "    <meta name='viewport' content='width=device-width, initial-scale=1.0'>"
                );
                htmlContent.AppendLine($"    <title>Análise Técnica - {originalFileName}</title>");
                htmlContent.AppendLine("    <script src='https://cdn.tailwindcss.com'></script>");
                htmlContent.AppendLine(
                    "    <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css'>"
                );
                htmlContent.AppendLine("    <style>");
                htmlContent.AppendLine(
                    "        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;600;700&display=swap');"
                );
                htmlContent.AppendLine("        body { font-family: 'Inter', sans-serif; }");
                htmlContent.AppendLine(
                    "        .clause-tag { @apply bg-blue-100 text-blue-800 text-xs font-semibold px-2.5 py-0.5 rounded border border-blue-400; }"
                );
                htmlContent.AppendLine(
                    "        .risk-high { @apply border-l-4 border-red-500 bg-red-50; }"
                );
                htmlContent.AppendLine(
                    "        .risk-low { @apply border-l-4 border-green-500 bg-green-50; }"
                );
                htmlContent.AppendLine("    </style>");
                htmlContent.AppendLine("</head>");

                htmlContent.AppendLine("<body class='bg-gray-50 text-gray-900 leading-relaxed'>");

                // Header
                htmlContent.AppendLine(
                    "    <header class='bg-slate-900 text-white py-12 px-6 shadow-lg'>"
                );
                htmlContent.AppendLine("        <div class='max-w-5xl mx-auto'>");
                htmlContent.AppendLine(
                    "            <div class='flex items-center space-x-4 mb-4'>"
                );
                htmlContent.AppendLine(
                    "                <img src='../img/logo.png' alt='Law Assist Logo' class='h-12' />"
                );
                htmlContent.AppendLine(
                    $"                <h1 class='text-3xl font-bold tracking-tight'>{WebUtility.HtmlEncode(data.Titulo ?? "Relatório de Análise")}</h1>"
                );
                htmlContent.AppendLine("            </div>");
                htmlContent.AppendLine(
                    $"            <p class='text-slate-400 max-w-4xl'>{WebUtility.HtmlEncode(data.Descricao ?? "")}</p>"
                );
                htmlContent.AppendLine("        </div>");
                htmlContent.AppendLine("    </header>");

                htmlContent.AppendLine(
                    "    <main class='max-w-5xl mx-auto py-10 px-6 space-y-12'>"
                );

                // Secção 1: Objeto e Preço
                htmlContent.AppendLine("        <section>");
                htmlContent.AppendLine(
                    "            <div class='flex items-center space-x-2 mb-6'>"
                );
                htmlContent.AppendLine(
                    "                <span class='bg-blue-600 text-white w-8 h-8 rounded-full flex items-center justify-center font-bold'>1</span>"
                );
                htmlContent.AppendLine(
                    "                <h2 class='text-3xl font-bold text-slate-800'>Objeto e Fixação do Preço Base</h2>"
                );
                htmlContent.AppendLine("            </div>");
                htmlContent.AppendLine("            <div class='grid md:grid-cols-2 gap-6'>");

                // Coluna Objeto
                htmlContent.AppendLine(
                    "                <div class='bg-white p-6 rounded-xl shadow-sm border border-gray-200'>"
                );
                htmlContent.AppendLine(
                    "                    <h3 class='font-semibold text-lg mb-3 flex items-center text-blue-700'><i class='fas fa-globe mr-2'></i> Definição do Objeto</h3>"
                );
                htmlContent.AppendLine(
                    $"                    <p class='text-sm text-gray-600 mb-4'>{WebUtility.HtmlEncode(data.Objeto ?? "N/A")}</p>"
                );
                htmlContent.AppendLine("                    <div class='space-y-2'>");
                htmlContent.AppendLine(
                    $"                        <div class='flex justify-between text-xs font-medium'><span class='text-gray-500'>Localização</span><span>{WebUtility.HtmlEncode(data.Localizacao)}</span></div>"
                );
                htmlContent.AppendLine(
                    $"                        <div class='flex justify-between text-xs font-medium'><span class='text-gray-500'>Oferta</span><span>{WebUtility.HtmlEncode(data.TotalLinhas)}</span></div>"
                );
                htmlContent.AppendLine("                    </div>");
                htmlContent.AppendLine("                </div>");

                // Coluna Financeira
                htmlContent.AppendLine(
                    "                <div class='bg-blue-900 text-white p-6 rounded-xl shadow-md'>"
                );
                htmlContent.AppendLine(
                    "                    <h3 class='font-semibold text-lg mb-3 flex items-center'><i class='fas fa-coins mr-2 text-yellow-400'></i> Resumo Financeiro</h3>"
                );
                htmlContent.AppendLine("                    <div class='space-y-4'>");
                htmlContent.AppendLine(
                    $"                        <div><p class='text-xs text-blue-300 uppercase font-bold tracking-wider'>Preço Base (S/ IVA)</p><p class='text-5xl font-bold'>{data.PrecoBase.ToString("C", culture)}</p></div>"
                );
                htmlContent.AppendLine(
                    "                        <div class='grid grid-cols-2 gap-4 border-t border-blue-800 pt-4 text-sm'>"
                );
                htmlContent.AppendLine("                    </div>");
                htmlContent.AppendLine("                </div>");
                htmlContent.AppendLine("            </div>");
                htmlContent.AppendLine("        </section>");

                // Secção 2: Cláusulas
                htmlContent.AppendLine("        <section>");
                htmlContent.AppendLine(
                    "            <div class='flex items-center space-x-2 mb-6'>"
                );
                htmlContent.AppendLine(
                    "                <span class='bg-blue-600 text-white w-8 h-8 rounded-full flex items-center justify-center font-bold'>2</span>"
                );
                htmlContent.AppendLine(
                    "                <h2 class='text-2xl font-bold text-slate-800'>Matriz de Conformidade</h2>"
                );
                htmlContent.AppendLine("            </div>");

                // Tabela Fixas
                htmlContent.AppendLine(
                    "            <div class='mb-8 overflow-hidden rounded-xl border border-gray-200 shadow-sm'>"
                );
                htmlContent.AppendLine(
                    "                <div class='bg-gray-100 px-4 py-3 border-b border-gray-200'><h3 class='font-bold text-gray-700 text-sm uppercase'>Cláusulas Fixas (Adesão Obrigatória)</h3></div>"
                );
                htmlContent.AppendLine("                <table class='w-full text-left text-sm'>");
                htmlContent.AppendLine(
                    "                    <thead class='bg-gray-50 text-gray-500'><tr><th class='px-4 py-3 font-semibold'>Área</th><th class='px-4 py-3 font-semibold text-center'>Cláusula</th><th class='px-4 py-3 font-semibold'>Requisito Obrigatório</th></tr></thead>"
                );
                htmlContent.AppendLine(
                    "                    <tbody class='divide-y divide-gray-200 bg-white'>"
                );

                foreach (var item in data.ClausulasFixas ?? new List<ClausulaFixa>())
                {
                    htmlContent.AppendLine("                        <tr>");
                    htmlContent.AppendLine(
                        $"                            <td class='px-4 py-3 font-semibold text-blue-600'>{WebUtility.HtmlEncode(item.Area)}</td>"
                    );
                    htmlContent.AppendLine(
                        $"                            <td class='px-4 py-3 text-center'><span class='clause-tag'>{WebUtility.HtmlEncode(item.Clausula)}</span></td>"
                    );
                    htmlContent.AppendLine(
                        $"                            <td class='px-4 py-3'>{WebUtility.HtmlEncode(item.Requisito)}</td>"
                    );
                    htmlContent.AppendLine("                        </tr>");
                }
                htmlContent.AppendLine("                    </tbody>");
                htmlContent.AppendLine("                </table>");
                htmlContent.AppendLine("            </div>");

                // Aspetos Variáveis
                htmlContent.AppendLine(
                    "            <div class='bg-amber-50 border border-amber-200 rounded-xl p-6'>"
                );
                htmlContent.AppendLine(
                    "                <h3 class='font-bold text-amber-800 mb-4 flex items-center'><i class='fas fa-trophy mr-2'></i> Aspetos Variáveis (Fatores de Avaliação)</h3>"
                );
                htmlContent.AppendLine(
                    "                <div class='grid md:grid-cols-2 lg:grid-cols-4 gap-3'>"
                );
                foreach (var item in data.AspetosVariaveis ?? new List<AspetoVariavel>())
                {
                    htmlContent.AppendLine(
                        "                    <div class='bg-white p-3 rounded-lg border border-amber-200 shadow-sm'>"
                    );
                    htmlContent.AppendLine(
                        $"                        <p class='text-xs text-amber-600 font-bold uppercase'>{WebUtility.HtmlEncode(item.Titulo)}</p>"
                    );
                    htmlContent.AppendLine(
                        $"                        <p class='text-sm'>{WebUtility.HtmlEncode(item.Descricao)}</p>"
                    );
                    htmlContent.AppendLine("                    </div>");
                }
                htmlContent.AppendLine("                </div>");
                htmlContent.AppendLine("            </div>");
                htmlContent.AppendLine("        </section>");

                // Secção 3: Execução e Penalidades
                htmlContent.AppendLine("        <section>");
                htmlContent.AppendLine(
                    "            <div class='flex items-center space-x-2 mb-6'>"
                );
                htmlContent.AppendLine(
                    "                <span class='bg-blue-600 text-white w-8 h-8 rounded-full flex items-center justify-center font-bold'>3</span>"
                );
                htmlContent.AppendLine(
                    "                <h2 class='text-2xl font-bold text-slate-800'>Prazos e Regime Sancionatório</h2>"
                );
                htmlContent.AppendLine("            </div>");

                htmlContent.AppendLine(
                    "            <div class='grid md:grid-cols-3 gap-6 mb-8 text-center'>"
                );
                htmlContent.AppendLine(
                    $"                <div class='bg-white p-6 rounded-xl border border-gray-200 shadow-sm'><i class='fas fa-calendar-alt text-gray-400 text-2xl mb-2'></i><p class='text-xs text-gray-500 uppercase font-bold'>Vigência Base</p><p class='text-xl font-bold'>{WebUtility.HtmlEncode(data.Vigencia)}</p></div>"
                );
                htmlContent.AppendLine(
                    $"                <div class='bg-white p-6 rounded-xl border border-gray-200 shadow-sm'><i class='fas fa-shield-alt text-gray-400 text-2xl mb-2'></i><p class='text-xs text-gray-500 uppercase font-bold'>Caução</p><p class='text-xl font-bold'>{WebUtility.HtmlEncode(data.Caucao)}</p></div>"
                );
                htmlContent.AppendLine(
                    $"                <div class='bg-white p-6 rounded-xl border border-gray-200 shadow-sm'><i class='fas fa-file-invoice text-gray-400 text-2xl mb-2'></i><p class='text-xs text-gray-500 uppercase font-bold'>Pagamento</p><p class='text-xl font-bold'>{WebUtility.HtmlEncode(data.Pagamento)}</p></div>"
                );
                htmlContent.AppendLine("            </div>");

                // Tabela Penalidades
                htmlContent.AppendLine(
                    "            <div class='overflow-hidden rounded-xl border border-red-200 shadow-sm'>"
                );
                htmlContent.AppendLine(
                    "                <div class='bg-red-600 px-4 py-3 text-white'><h3 class='font-bold text-sm uppercase'>Escalonamento de Penalidades</h3></div>"
                );
                htmlContent.AppendLine("                <table class='w-full text-left text-sm'>");
                htmlContent.AppendLine(
                    "                    <thead class='bg-red-50 text-red-800'><tr><th class='px-4 py-3 font-semibold'>Gravidade</th><th class='px-4 py-3 font-semibold'>Exemplos</th><th class='px-4 py-3 font-semibold'>Coima Direta</th><th class='px-4 py-3 font-semibold'>Compulsória (Dia)</th></tr></thead>"
                );
                htmlContent.AppendLine(
                    "                    <tbody class='divide-y divide-gray-200 bg-white'>"
                );
                foreach (var item in data.Penalidades ?? new List<Penalidade>())
                {
                    string badgeClass = item.Nivel.ToLower().Contains("grave")
                        ? "bg-red-600 text-white"
                        : "bg-yellow-100 text-yellow-800";
                    htmlContent.AppendLine("                        <tr>");
                    htmlContent.AppendLine(
                        $"                            <td class='px-4 py-3'><span class='{badgeClass} px-2 py-1 rounded text-xs font-bold uppercase'>{WebUtility.HtmlEncode(item.Nivel)}</span></td>"
                    );
                    htmlContent.AppendLine(
                        $"                            <td class='px-4 py-3 text-gray-600'>{WebUtility.HtmlEncode(item.Exemplos)}</td>"
                    );
                    htmlContent.AppendLine(
                        $"                            <td class='px-4 py-3 font-semibold'>{WebUtility.HtmlEncode(item.Coima)}</td>"
                    );
                    htmlContent.AppendLine(
                        $"                            <td class='px-4 py-3 text-gray-400'>{WebUtility.HtmlEncode(item.Compulsoria)}</td>"
                    );
                    htmlContent.AppendLine("                        </tr>");
                }
                htmlContent.AppendLine("                    </tbody>");
                htmlContent.AppendLine("                </table>");
                htmlContent.AppendLine("            </div>");
                htmlContent.AppendLine("        </section>");

                // Secção 4: Riscos
                htmlContent.AppendLine("        <section class='pb-20'>");
                htmlContent.AppendLine(
                    "            <div class='flex items-center space-x-2 mb-6'>"
                );
                htmlContent.AppendLine(
                    "                <span class='bg-blue-600 text-white w-8 h-8 rounded-full flex items-center justify-center font-bold'>4</span>"
                );
                htmlContent.AppendLine(
                    "                <h2 class='text-2xl font-bold text-slate-800'>Matriz de Risco do Procedimento</h2>"
                );
                htmlContent.AppendLine("            </div>");
                htmlContent.AppendLine("            <div class='space-y-4'>");

                foreach (var item in data.Riscos ?? new List<Risco>())
                {
                    bool isHighRisk = item.Tipo?.ToLower().Contains("alto") == true;
                    string divClass = isHighRisk ? "risk-high" : "risk-low";
                    string icon = isHighRisk
                        ? "fa-exclamation-triangle text-red-600"
                        : "fa-check-circle text-green-600";
                    string titleColor = isHighRisk ? "text-red-800" : "text-green-800";

                    htmlContent.AppendLine(
                        $"                <div class='{divClass} p-6 rounded-r-xl'>"
                    );
                    htmlContent.AppendLine(
                        "                    <div class='flex items-center mb-3'>"
                    );
                    htmlContent.AppendLine(
                        $"                        <i class='fas {icon} mr-2'></i>"
                    );
                    htmlContent.AppendLine(
                        $"                        <h3 class='font-bold {titleColor} uppercase text-sm tracking-wide'>{WebUtility.HtmlEncode(item.Titulo)}</h3>"
                    );
                    htmlContent.AppendLine("                    </div>");
                    htmlContent.AppendLine(
                        $"                    <p class='text-sm text-gray-700'>{WebUtility.HtmlEncode(item.Descricao)}</p>"
                    );
                    htmlContent.AppendLine("                </div>");
                }
                htmlContent.AppendLine("            </div>");

                // Conclusão
                htmlContent.AppendLine(
                    "            <div class='mt-8 bg-slate-100 p-8 rounded-2xl border border-slate-200'>"
                );
                htmlContent.AppendLine(
                    "                <h3 class='text-lg font-bold mb-3'>Conclusão Final</h3>"
                );
                htmlContent.AppendLine(
                    $"                <p class='text-sm text-gray-600'>{WebUtility.HtmlEncode(data.Conclusao)}</p>"
                );
                htmlContent.AppendLine("            </div>");
                htmlContent.AppendLine("        </section>");

                htmlContent.AppendLine("    </main>");
                htmlContent.AppendLine(
                    "    <footer class='bg-white border-t border-gray-200 py-6 text-center text-gray-400 text-xs'>Análise gerada para uso técnico e consultoria jurídica. Todos os direitos reservados.</footer>"
                );
                htmlContent.AppendLine("</body>");
                htmlContent.AppendLine("</html>");

                File.WriteAllText(outputPath, htmlContent.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao gerar relatório HTML: {ex.Message}");
            }
        }
    }
}
