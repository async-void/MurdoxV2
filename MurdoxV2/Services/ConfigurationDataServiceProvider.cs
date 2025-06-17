using MurdoxV2.Enums;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MurdoxV2.Services
{
    public class ConfigurationDataServiceProvider : IDataConfiguration
    {
        public async Task<Result<ConfigJson, SystemError<ConfigurationDataServiceProvider>>> GetConnectionStringsAsync()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "Config", "config.json");
            if (!File.Exists(path))
            {
                return Result<ConfigJson, SystemError<ConfigurationDataServiceProvider>>.Err(new SystemError<ConfigurationDataServiceProvider>()
                {
                    ErrorMessage = $"could not find part of the path: {path}",
                    ErrorType = ErrorType.WARNING,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = this
                });
            }

            var json = await File.ReadAllTextAsync(path);
            var conStr = JsonSerializer.Deserialize<ConfigJson>(json)!;
            return Result<ConfigJson, SystemError<ConfigurationDataServiceProvider>>.Ok(conStr);
        }

        public async Task<Result<string, SystemError<ConfigurationDataServiceProvider>>> GetDiscordTokenAsync()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "Config", "config.json");
            if (!File.Exists(path))
            {
                return Result<string, SystemError<ConfigurationDataServiceProvider>>.Err(new SystemError<ConfigurationDataServiceProvider>()
                {
                    ErrorMessage = $"could not find part of the path: {path}",
                    ErrorType = ErrorType.WARNING,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = this
                });
            }

            var json = await File.ReadAllTextAsync(path);
            var conStr = JsonSerializer.Deserialize<ConfigJson>(json)?.Token;
            return Result<string, SystemError<ConfigurationDataServiceProvider>>.Ok(conStr ?? "not found");
        }
    }
}
