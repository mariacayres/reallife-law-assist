using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RealLifeLawAssist.Configuration
{
    public class ConfigEnv
    {
        public string ApiKey { get; private set; } = string.Empty;
        public string Model { get; private set; } = string.Empty;
        public string Url { get; private set; } = string.Empty;
        public bool Validating { get; private set; } = false;
        public ConfigEnv()
        {
            string configFilePath = "config.yml";
            LoadFromYaml(configFilePath);
        }

        private void LoadFromYaml(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new System.IO.FileNotFoundException($"Arquivo de configuração não encontrado: {filePath}");
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties() // Evita erros se houver campos extras no YAML
                .Build();

            var yamlContent = System.IO.File.ReadAllText(filePath);
            
            // Deserializa para a estrutura esperada no seu config.yml
            // Deserializa para um dicionário genérico para lidar com chaves em diferentes níveis
            var configData = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

            if (configData.TryGetValue("googleApi", out var googleApiObj) && googleApiObj is Dictionary<object, object> googleApiRaw)
            {
                // Converte o dicionário aninhado para <string, string> para facilitar o uso
                var googleApi = googleApiRaw.ToDictionary(kvp => kvp.Key.ToString() ?? string.Empty, kvp => kvp.Value.ToString() ?? string.Empty);

                ApiKey = googleApi.GetValueOrDefault("apiKey") ?? string.Empty;
                Model = googleApi.GetValueOrDefault("model") ?? string.Empty;
                Url = googleApi.GetValueOrDefault("url") ?? string.Empty;
            }

            // A chave 'validating' agora está na raiz do YAML, então a lemos a partir do dicionário principal
            if (configData.TryGetValue("validating", out var validatingValue))
            {
                Validating = bool.TryParse(validatingValue?.ToString(), out var validating) ? validating : false;
            }
            Validate();
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new Exception("A API Key do Google não foi encontrada no arquivo de configuração.");
            
            if (string.IsNullOrWhiteSpace(Model))
                Model = "gemini-pro"; // Valor default caso não exista no YAML

            if (string.IsNullOrWhiteSpace(Url))
                throw new Exception("A URL da API do Google não foi encontrada no arquivo de configuração.");
            
            if (string.IsNullOrWhiteSpace(Validating.ToString()))
                throw new Exception("A opção de validação do prompt não foi encontrada no arquivo de configuração.");
        }
    }
}