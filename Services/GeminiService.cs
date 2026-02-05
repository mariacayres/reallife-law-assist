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
        private readonly string? _apiKey;
        private readonly string? _model;
        private readonly string? _url;
        private readonly ConfigEnv? _config;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly bool _isTestEnvironment;

        public GeminiService()
        {
            _isTestEnvironment = false;
            
            try
            {
                // Tentar carregar a configuração
                _config = new ConfigEnv();
                _apiKey = _config.ApiKey;
                _model = _config.Model;
                _url = _config.Url;
            }
            catch (System.IO.FileNotFoundException)
            {
                // Se não encontrar o arquivo de configuração, definimos valores padrão
                // Isso permite que os testes funcionem sem config.yml
                _config = null;
                _apiKey = null;
                _model = null;
                _url = null;
                _isTestEnvironment = true;
                
                Console.WriteLine("⚠️ Modo teste ativado - config.yml não encontrado");
            }
            catch (Exception ex)
            {
                // Outros erros de configuração
                _config = null;
                _apiKey = null;
                _model = null;
                _url = null;
                _isTestEnvironment = true;
                
                Console.WriteLine($"⚠️ Erro ao carregar configuração: {ex.Message}");
            }

            _httpClient = new HttpClient();

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));
        }

        /// <summary>
        /// Envia texto + prompt para o Gemini e devolve a resposta em TEXTO.
        /// </summary>
        public async Task<string> GenerateContentAsync(string text, string prompt)
        {
            // Se estiver em ambiente de teste e não tiver configuração,
            // lançar uma exceção mais clara
            if (_isTestEnvironment && string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException(
                    "GeminiService não está configurado para chamadas à API. " +
                    "Este método só funciona com configuração válida."
                );
            }

            // Verificar se temos configuração
            if (string.IsNullOrEmpty(_url) || string.IsNullOrEmpty(_model) || string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException(
                    "Configuração do Gemini não está definida. " +
                    "Certifique-se de que o arquivo config.yml existe e contém as chaves necessárias."
                );
            }

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
        /// Este método funciona mesmo em ambiente de teste.
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

    // Classes auxiliares para deserialização
    public class GenerateContentRequest
    {
        public Content[]? Contents { get; set; }
    }

    public class Content
    {
        public ContentPart[]? Parts { get; set; }
    }

    public class ContentPart
    {
        public string? Text { get; set; }
    }

    public class GeminiApiResponse
    {
        public Candidate[]? Candidates { get; set; }
    }

    public class Candidate
    {
        public Content? Content { get; set; }
    }
}