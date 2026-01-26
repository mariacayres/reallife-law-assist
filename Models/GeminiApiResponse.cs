using System.Text.Json.Serialization;
using System;

namespace RealLifeLawAssist.Models
{
    // Estas classes (DTOs) modelam a estrutura da resposta JSON recebida da API Gemini.
    // A deserialização para estas classes permite um acesso seguro e tipado aos dados da resposta,
    // evitando a análise manual de strings que é frágil e propensa a erros.

    /// <summary>
    /// Representa a estrutura de nível superior da resposta da API Gemini.
    /// </summary>
    public class GeminiApiResponse
    {
        [JsonPropertyName("candidates")]
        public ApiCandidate[] Candidates { get; set; } = Array.Empty<ApiCandidate>();
    }

    /// <summary>
    /// Representa um "candidato" de resposta gerado pela IA. Normalmente, apenas o primeiro é usado.
    /// </summary>
    public class ApiCandidate
    {
        [JsonPropertyName("content")]
        public ApiContent Content { get; set; } = new();
    }

    /// <summary>
    /// Contém as partes da resposta.
    /// </summary>
    public class ApiContent
    {
        [JsonPropertyName("parts")]
        public ApiPart[] Parts { get; set; } = Array.Empty<ApiPart>();
    }

    /// <summary>
    /// Representa a parte final que contém o texto da resposta gerada.
    /// </summary>
    public class ApiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}