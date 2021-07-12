using Microsoft.Extensions.Configuration;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public class RaptorConfig
    {
        public RaptorConfig(IConfigurationRoot config)
        {
            ClientID = config.GetNonEmptyValue(nameof(ClientID));
            Authority = config.GetNonEmptyValue(nameof(Authority));
            Username = config.GetNonEmptyValue(nameof(Username));
            Password = config.GetNonEmptyValue(nameof(Password));
            // application permissions
            TenantID = config.GetNonEmptyValue(nameof(TenantID));
            ClientSecret = config.GetNonEmptyValue(nameof(ClientSecret));
            CertificateThumbprint = config.GetNonEmptyValue(nameof(CertificateThumbprint));
            DocsRepoCheckoutDirectory = config.GetNonEmptyValue(nameof(DocsRepoCheckoutDirectory));
            RaptorStorageConnectionString = config.GetNonEmptyValue(nameof(RaptorStorageConnectionString));
            SASUrl = config.GetNonEmptyValue(nameof(SASUrl));
            IsLocalRun = bool.Parse(config.GetNonEmptyValue(nameof(IsLocalRun)));
        }
        public string ClientID { get; }
        public string Authority { get; }
        public string Username { get; }
        public string Password { get; }
        public string TenantID { get; }
        public string ClientSecret { get; }
        public string CertificateThumbprint { get; }
        public string DocsRepoCheckoutDirectory { get; }
        public string RaptorStorageConnectionString { get; }
        public string SASUrl { get; }
        public bool IsLocalRun { get; }
    }
}
