using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RealLifeLawAssist.Configuration
{
    public class ConfigEnv
    {
        public string ApiKey { get; private set; } = string.Empty;
        public string Model { get; private set; } = string.Empty;
        public string Url { get; private set; } = string.Empty;

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
            var configData = deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(yamlContent);

            if (configData.TryGetValue("googleApi", out var googleApi))
            {
                ApiKey = googleApi.GetValueOrDefault("apiKey");
                Model = googleApi.GetValueOrDefault("model");
                Url = googleApi.GetValueOrDefault("url");
            }

            Validate();
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new Exception("A API Key do Google não foi encontrada no arquivo de configuração.");
            
            if (string.IsNullOrWhiteSpace(Model))
                Model = "gemini-pro"; // Valor default caso não exista no YAML
        }
    }
}