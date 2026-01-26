using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

namespace RealLifeLawAssist.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public GeminiService(HttpClient httpClient, string apiKey, string model)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _model = model;

            // Retry simples com Polly: 3 tentativas em caso de falha de rede
            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));
        }

        public async Task<string> GenerateContentAsync(string text, string prompt)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/{_model}:generateContent?key={_apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = $"{prompt}\n\n{text}" } } }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine("=== ENVIANDO REQUISIÇÃO ===");
            Console.WriteLine($"URL: {url}");
            Console.WriteLine($"Payload JSON: {json}");

            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.PostAsync(url, content));
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("=== RESPOSTA DA API ===");
            Console.WriteLine($"StatusCode: {response.StatusCode}");
            Console.WriteLine($"Body: {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Erro ao chamar a API: " + responseBody);
            }

            using var document = JsonDocument.Parse(responseBody);
            var textResult = document
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return textResult;
        }
    }
}
