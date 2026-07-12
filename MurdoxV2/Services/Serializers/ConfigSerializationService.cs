using MurdoxV2.Models;
using System.Text.Json;

namespace MurdoxV2.Services.Serializers
{
    public sealed class ConfigSerializationService
    {
        private readonly string _path;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
        };

        public ConfigJson Config { get; private set; }

        public ConfigSerializationService(string configFilePath)
        {
            _path = configFilePath;
            var json = File.ReadAllText(_path);

            Config = JsonSerializer.Deserialize<ConfigJson>(json, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize config.");
        }

        public async Task Save()
        {
            var json = JsonSerializer.Serialize(Config, _jsonOptions);
            await Task.Delay(300);
            await File.WriteAllTextAsync(_path, json);
        }
    }
}
