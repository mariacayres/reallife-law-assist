⚖️ Law Assist – Analista de Prevenção da Corrupção

O Law Assist é uma ferramenta de auditoria inteligente desenvolvida em C# (.NET) que utiliza IA generativa (Google Gemini) para analisar documentos PDF de contratação pública, como editais e cadernos de encargos.

O sistema identifica automaticamente riscos de corrupção e restrições à concorrência, produzindo relatórios técnicos em HTML (individual e consolidado), preparados para consulta, arquivo e partilha.

🚀 Funcionalidades Principais

Análise direta de documentos PDF
Processamento automático de Cadernos de Encargos / Editais em formato PDF.

Deteção de riscos e cláusulas restritivas
Identificação de exigências técnicas excessivas, direcionamento de fornecedores e vícios concorrenciais.

Pontuação de risco contratual
Atribuição de um nível de risco (Baixo / Médio / Alto) com fundamentação objetiva.

📄 Relatórios Gerados

Relatório técnico detalhado por documento, contendo:

Identificação do procedimento

Pontuação de risco

Lista de cláusulas críticas

Fundamentação jurídica e técnica

📸 [Imagem – Relatório individual em HTML]


Dashboard agregador de múltiplos documentos, incluindo:

Visão global de riscos

Gráficos de distribuição

Filtros por texto e nível de risco

Links diretos para cada relatório individual

📸 [Imagem – Dashboard consolidado]
📸 [Imagem – Gráfico de riscos]

🔄 Fluxo de Funcionamento

Entrada
O utilizador fornece o caminho para um ficheiro PDF.

![Dashboard consolidado](docs/images/pdf1.png)

Análise por IA
O documento é analisado com base em critérios de transparência e concorrência.

Geração de Relatórios
O sistema produz automaticamente:

🧾 HTML Individual

📊 HTML Consolidado

📸 [Imagem – Estrutura de pastas com HTML gerados]

🧠 Critérios de Avaliação de Risco
Nível de Risco	Indicadores
🚩 Alto	Especificações “à medida”, prazos impossíveis, bloqueio de equivalências
⚠️ Médio	Ambiguidade técnica, critérios pouco claros
✅ Baixo	Descrições funcionais e concorrência efetiva
🔧 Tecnologias Utilizadas

C# (.NET)

Google Vertex AI – Gemini (multimodal)

PDF como input

HTML como output

📸 [Imagem – Arquitetura geral do sistema]

⚖️ Aviso Legal

O Law Assist é uma ferramenta de apoio à decisão e não substitui parecer jurídico ou auditoria oficial.

📄 Licença

Licença MIT – ver ficheiro LICENSE.
