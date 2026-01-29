using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
    /// </summary>
    public class GeminiService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _url;
        private readonly ConfigEnv _config;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public GeminiService()
        {
            _config = new ConfigEnv();
            _httpClient = new HttpClient();

            _apiKey = _config.ApiKey;
            _model = _config.Model;
            _url = _config.Url;

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));
        }

        /// <summary>
        /// Envia texto + prompt para o Gemini e devolve a resposta em TEXTO.
        /// </summary>
        public async Task<string> GenerateContentAsync(string text, string prompt)
        {
            var url = $"{_url}{_model}:generateContent?key={_apiKey}";

            var request = new GenerateContentRequest
            {
                Contents = new[]
                {
                    new Content
                    {
                        Parts = new[]
                        {
                            new ContentPart
                            {
                                Text = $"{prompt}\n\n{text}"
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(
                request,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine("=== ENVIANDO REQUISIÇÃO AO GEMINI ===");
            Console.WriteLine(json);

            var response = await _retryPolicy.ExecuteAsync(
                () => _httpClient.PostAsync(url, content)
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("=== RESPOSTA DO GEMINI ===");
            Console.WriteLine(responseBody);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Erro Gemini | Status: {response.StatusCode} | Body: {responseBody}"
                );
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseBody, options);

            return apiResponse?
                       .Candidates?
                       .FirstOrDefault()?
                       .Content?
                       .Parts?
                       .FirstOrDefault()?
                       .Text
                   ?? string.Empty;
        }

        /// <summary>
        /// Converte a resposta em texto do Gemini (JSON) para o objeto AnaliseDados.
        /// </summary>
        public AnaliseDados ConverterParaObjeto(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return new AnaliseDados();

            try
            {
                // Remove ```json e ``` se o Gemini devolver markdown
                string jsonLimpo = Regex.Replace(
                    rawText,
                    @"^```json\s*|```$",
                    "",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline
                ).Trim();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<AnaliseDados>(jsonLimpo, options)
                       ?? new AnaliseDados();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ Erro ao converter JSON do Gemini: {ex.Message}");

                return new AnaliseDados
                {
                    Titulo = "Erro ao processar análise"
                };
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
