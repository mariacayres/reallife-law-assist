using System;

namespace RealLifeLawAssist.Models
{
    // Estas classes (DTOs - Data Transfer Objects) modelam a estrutura do corpo da requisição JSON
    // que é enviada para a API do Gemini. Usar classes fortemente tipadas em vez de
    // construir a string JSON manualmente torna o código mais limpo, seguro e fácil de manter.

    /// <summary>
    /// Representa uma parte do conteúdo, que é essencialmente o texto do prompt.
    /// </summary>
    public class ContentPart
    {
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Representa o conteúdo da requisição, que contém uma ou mais partes.
    /// </summary>
    public class Content
    {
        public ContentPart[] Parts { get; set; } = Array.Empty<ContentPart>();
    }

    /// <summary>
    /// O objeto de nível superior para a requisição de geração de conteúdo.
    /// </summary>
    public class GenerateContentRequest
    {
        public Content[] Contents { get; set; } = Array.Empty<Content>();
    }
}
