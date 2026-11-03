using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;

namespace FussballNews.ai.Azure;

public class AzureAppConfigMapper 
{
    private Dictionary<string, string> _configKeys;
    private const string APPSETTINGS_PREFIX = "AzureAppConfigRef";
    public AzureAppConfigMapper(IConfigurationManager configuration)
    {
        _configKeys = GetAppConfigReferences(configuration);
    }

    public ValueTask<ConfigurationSetting> Map(ConfigurationSetting configurationSetting)
    {
        if (_configKeys.ContainsKey(configurationSetting.Key))
        {
            var configSetting = new ConfigurationSetting(
                key: _configKeys[configurationSetting.Key],
                value: configurationSetting.Value,
                label: configurationSetting.Label
            )
            {
                ContentType = configurationSetting.ContentType
            };
            return ValueTask.FromResult(configSetting);
        }
        return ValueTask.FromResult(configurationSetting);
    }

    /// <summary>
    /// Get all keys that are prefixed with AzureAppConfigRef
    /// Returns a map with the name of the app config property as key and the location it should be insterted in the configuration as value
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    private Dictionary<string, string> GetAppConfigReferences(IConfigurationManager configuration)
    {
        var valuePrefix = $"{APPSETTINGS_PREFIX}:";
        var keys = new Dictionary<string, string>();
        var configKeyValues = configuration.AsEnumerable();
        foreach (var kvp in configKeyValues)
        {
            if (kvp.Value != null && kvp.Value.StartsWith(valuePrefix))
            {
                var appConfigPropertyKey = kvp.Value.Split(valuePrefix)[1];
                keys[appConfigPropertyKey] = kvp.Key;
            }
        }

        return keys;
    }
}