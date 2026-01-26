using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using RealLifeLawAssist.Models;
using RealLifeLawAssist.Configuration;

namespace RealLifeLawAssist.Services
{
    /// <summary>
    /// Serviço responsável por toda a comunicação com a API Google Gemini.
    /// Encapsula a lógica de construção de requisições, tratamento de respostas e políticas de repetição.
    /// </summary>
    public class GeminiService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ConfigEnv _config;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        /// <summary>
        /// Inicializa uma nova instância do <see cref="GeminiService"/>.
        /// Carrega a configuração, inicializa o HttpClient e configura a política de repetição.
        /// </summary>
        public GeminiService()
        {
            // Carrega as configurações (API Key, Modelo) a partir do arquivo config.yml.
            _config = new ConfigEnv();
            _httpClient = new HttpClient();
            _apiKey = _config.ApiKey;
            _model = _config.Model;

            // Configura uma política de repetição (retry) usando a biblioteca Polly.
            // Se a chamada à API falhar (retornar um status code não-sucesso),
            // a política espera 2 segundos e tenta novamente, até um máximo de 3 vezes.
            // Isso aumenta a resiliência da aplicação a falhas de rede temporárias.
            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));
        }

        /// <summary>
        /// Envia um texto e um prompt para a API Gemini para gerar conteúdo.
        /// </summary>
        /// <param name="text">O texto principal a ser analisado (extraído do PDF).</param>
        /// <param name="prompt">A instrução/pergunta para a IA sobre o que fazer com o texto.</param>
        /// <returns>A resposta em texto gerada pela IA.</returns>
        public async Task<string> GenerateContentAsync(string text, string prompt)
        {
            // Constrói a URL do endpoint da API, inserindo o modelo e a chave de API.
            var url = $"https://generativelanguage.googleapis.com/v1beta/{_model}:generateContent?key={_apiKey}";

            var request = new GenerateContentRequest
            {
                Contents = new[]
                {
                    new Content
                    {
                        Parts = new[] { new ContentPart { Text = $"{prompt}\n\n{text}" } }
                    }
                }
            };

            // Serializa o objeto de requisição C# para uma string JSON, seguindo o padrão camelCase esperado pela API.
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine("=== ENVIANDO REQUISIÇÃO ===");
            Console.WriteLine($"URL: {url}");
            Console.WriteLine($"Payload JSON: {json}");
            
            // Executa a chamada POST à API usando a política de repetição.
            // O Polly irá gerir as tentativas em caso de falha.
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.PostAsync(url, content));
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("=== RESPOSTA DA API ===");
            Console.WriteLine($"StatusCode: {response.StatusCode}");
            Console.WriteLine($"Body: {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao chamar a API. Status: {response.StatusCode}, Body: {responseBody}");
            }

            // Deserializa a resposta JSON para os nossos objetos C# (DTOs).
            // Usar classes fortemente tipadas é mais seguro e limpo do que analisar o JSON manualmente.
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseBody, options);

            // Navega pela estrutura do objeto de resposta de forma segura para extrair o texto.
            // O operador '?' (null-conditional) previne NullReferenceException se alguma parte da resposta estiver em falta.
            // O operador '??' (null-coalescing) garante que retornamos uma string vazia em vez de null.
            return apiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
        }

        /// <summary>
        /// Liberta os recursos não geridos, especificamente o HttpClient.
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
