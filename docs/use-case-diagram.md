# Diagrama de Casos de Uso - Law Assist

```mermaid
graph TB
    subgraph "Sistema Law Assist"
        UC1[Analisar Documento PDF]
        UC2[Extrair Texto de PDF]
        UC3[Identificar Riscos de Corrupção]
        UC4[Calcular Score de Risco]
        UC5[Gerar Relatório Individual HTML]
        UC6[Gerar Relatório Individual PDF]
        UC7[Gerar Relatório Consolidado]
        UC8[Configurar API Gemini]
        UC9[Validar Cláusulas Restritivas]
        UC10[Detectar Direcionamento]
        UC11[Analisar Especificações Técnicas]
        UC12[Processar Múltiplos Documentos]
    end
    
    subgraph "Atores"
        A1[Auditor Público]
        A2[Analista Jurídico]
        A3[Gestor de Contratação]
        A4[Sistema Gemini API]
    end
    
    A1 --> UC1
    A1 --> UC7
    A1 --> UC12
    
    A2 --> UC3
    A2 --> UC9
    A2 --> UC10
    A2 --> UC11
    
    A3 --> UC5
    A3 --> UC6
    A3 --> UC7
    
    UC1 --> UC2
    UC1 --> UC3
    UC3 --> UC4
    UC3 --> UC9
    UC3 --> UC10
    UC3 --> UC11
    UC4 --> UC5
    UC4 --> UC6
    
    UC1 -.-> A4
    UC8 -.-> A4
    
    style A1 fill:#e3f2fd
    style A2 fill:#f3e5f5
    style A3 fill:#e8f5e8
    style A4 fill:#fff3e0
    style UC1 fill:#ffebee
    style UC3 fill:#ffebee
    style UC7 fill:#ffebee
```

## Descrição dos Casos de Uso

### Atores Principais

- **Auditor Público**: Responsável pela auditoria de processos de contratação pública
- **Analista Jurídico**: Especialista em análise de documentos legais e identificação de riscos
- **Gestor de Contratação**: Responsável por supervisionar processos de contratação
- **Sistema Gemini API**: API externa para análise de documentos com IA

### Casos de Uso Principais

1. **UC1 - Analisar Documento PDF**
   - **Ator**: Auditor Público, Analista Jurídico
   - **Descrição**: Processo principal de análise de um documento PDF
   - **Pré-condições**: Documento PDF válido disponível
   - **Pós-condições**: Análise completa gerada

2. **UC3 - Identificar Riscos de Corrupção**
   - **Ator**: Analista Jurídico
   - **Descrição**: Identifica potenciais riscos de corrupção no documento
   - **Critérios**: Cláusulas restritivas, especificações direcionadas, prazos irreais

3. **UC7 - Gerar Relatório Consolidado**
   - **Ator**: Auditor Público, Gestor de Contratação
   - **Descrição**: Cria relatório consolidado de múltiplas análises
   - **Formatos**: HTML e PDF

4. **UC9 - Validar Cláusulas Restritivas**
   - **Ator**: Analista Jurídico
   - **Descrição**: Verifica se cláusulas limitam a concorrência
   - **Indicadores**: Especificações "à medida", bloqueio de marcas

5. **UC10 - Detectar Direcionamento**
   - **Ator**: Analista Jurídico
   - **Descrição**: Identifica sinais de direcionamento para fornecedor específico
   - **Sinais**: Requisitos únicos, prazos impossíveis

### Fluxos de Extensão

- **UC12 - Processar Múltiplos Documentos**: Extensão do UC1 para análise em lote
- **UC8 - Configurar API Gemini**: Configuração necessária para funcionamento do sistema