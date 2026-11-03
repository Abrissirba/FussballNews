using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace FussballNews.ai.Azure;


public static class AddAppConfigurationExtension
{
    public static void AddAzureAppConfiguration(this IConfigurationManager configurationManager, string? appConfigEndpoint, string? label)
    {
        if (!string.IsNullOrEmpty(appConfigEndpoint))
        {
            var appConfigMapper = new AzureAppConfigMapper(configurationManager);
            configurationManager.AddAzureAppConfiguration(options =>
            {
                options.Connect(new Uri(appConfigEndpoint), AzureCredentials.GetAzureCredentials())
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(AzureCredentials.GetAzureCredentials());
                    })
                    .Select(KeyFilter.Any, LabelFilter.Null);
                if (!string.IsNullOrEmpty(label))
                {
                    options.Select(KeyFilter.Any, label);
                }

                options.Map(configurationSetting => appConfigMapper.Map(configurationSetting));
            });
        }
    }

    //public static void AddAzureAppConfiguration(this IConfigurationBuilder configurationBuilder, string? appConfigEndpoint, string? label)
    //{
    //    if (!string.IsNullOrEmpty(appConfigEndpoint))
    //    {
    //        var appConfigMapper = new AzureAppConfigMapper(configurationBuilder.Build());
    //        configurationBuilder.AddAzureAppConfiguration(options =>
    //        {
    //            options.Connect(new Uri(appConfigEndpoint), AzureCredentials.GetAzureCredentials())
    //                .ConfigureKeyVault(kv =>
    //                {
    //                    kv.SetCredential(AzureCredentials.GetAzureCredentials());
    //                })
    //                .Select(KeyFilter.Any, LabelFilter.Null);
    //            if (!string.IsNullOrEmpty(label))
    //            {
    //                options.Select(KeyFilter.Any, label);
    //            }

    //            options.Map(configurationSetting => appConfigMapper.Map(configurationSetting));
    //        });
    //    }
    //}
}