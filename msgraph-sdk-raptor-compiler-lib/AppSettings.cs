using System;
using Microsoft.Extensions.Configuration;

namespace MsGraphSDKSnippetsCompiler
{
    public static class AppSettings
    {
        public static IConfigurationRoot Config()
        {
            var builder = new ConfigurationBuilder().SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            return configuration;
        }

        /// <summary>
        /// Extracts the configuration value, throws if empty string
        /// </summary>
        /// <param name="config">configuration</param>
        /// <param name="key">lookup key</param>
        /// <returns>non-empty configuration value if found</returns>
        public static string GetNonEmptyValue(this IConfigurationRoot config, string key)
        {
            var value = config.GetSection(key).Value;
            if (value == string.Empty)
            {
                throw new Exception($"Value for {key} is not found in appsettings.json");
            }

            return value;
        }
    }
}
