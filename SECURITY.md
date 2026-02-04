# Política de Segurança

## Versões Suportadas

As seguintes versões do Law Assist recebem atualizações de segurança:

| Versão | Suportada          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Relatório de Vulnerabilidades

### Como Reportar

Se você descobrir uma vulnerabilidade de segurança, por favor:

1. **NÃO** abra uma issue pública
2. Envie um email para: **security@reallife-law-assist.com**
3. Inclua as seguintes informações:
   - Descrição detalhada da vulnerabilidade
   - Passos para reproduzir o problema
   - Versão afetada do software
   - Impacto potencial
   - Sugestões de correção (se houver)

### Processo de Resposta

- **Confirmação:** Responderemos em até 48 horas
- **Avaliação:** Análise inicial em até 5 dias úteis
- **Correção:** Patch de segurança em até 30 dias (dependendo da severidade)
- **Divulgação:** Coordenada após correção implementada

### Classificação de Severidade

| Nível | Descrição | Tempo de Resposta |
|-------|-----------|------------------|
| **Crítica** | Execução remota de código, vazamento de dados sensíveis | 24h |
| **Alta** | Escalação de privilégios, bypass de autenticação | 72h |
| **Média** | Denial of Service, exposição de informações | 1 semana |
| **Baixa** | Problemas menores de configuração | 2 semanas |

## Considerações de Segurança

### Dados Sensíveis
- **API Keys:** Nunca commite chaves de API no código
- **Logs:** Não registre informações confidenciais dos documentos
- **Armazenamento:** PDFs processados são temporários e devem ser removidos

### Configuração Segura
- Use variáveis de ambiente para credenciais
- Configure HTTPS para todas as comunicações
- Mantenha dependências atualizadas
- Execute com privilégios mínimos necessários

### Auditoria
- Logs de acesso são mantidos por 90 dias
- Monitoramento de uso da API Gemini
- Verificação regular de dependências vulneráveis

## Contato

- **Email de Segurança:** security@reallife-law-assist.com
- **Maintainer:** @seu-usuario
- **PGP Key:** [Disponível aqui](https://keybase.io/seu-usuario)

---

**Nota:** Este projeto lida com documentos legais sensíveis. A segurança é nossa prioridade máxima.
