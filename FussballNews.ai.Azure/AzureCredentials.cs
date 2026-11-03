using Azure.Core;
using Azure.Identity;

namespace FussballNews.ai.Azure;
public class AzureCredentials
{
    public static TokenCredential GetAzureCredentials()
    {
#if DEBUG
        var azureCredentials = new AzureCliCredential();
#else
            var azureCredentials = new ManagedIdentityCredential(Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"));
#endif
        return azureCredentials;
    }
}
