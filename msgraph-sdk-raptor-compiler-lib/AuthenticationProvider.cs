using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;

namespace MsGraphSDKSnippetsCompiler
{
    public class AuthenticationProvider
    {
        private static readonly IConfigurationRoot configuration = AppSettings.Config();

        private static readonly string clientId = configuration.GetSection("Azure").GetSection("ClientId").Value;
        private static readonly string scopes = configuration.GetSection("Azure").GetSection("Scopes").Value;

        private static readonly string[] scopesArray = scopes.Split(",");

        private static IAuthenticationProvider authProvider = null;

        // Get an access token for the given context and resourceId. 
        // An attempt is first made to acquire the token silently. 
        // If that fails, then we try to acquire the token by prompting the user.
        public static IAuthenticationProvider GetIAuthenticationProvider()
        {
            if (authProvider == null)
            {
                try
                {
                    IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
                      .Create(clientId)
                      .Build();

                    authProvider = new InteractiveAuthenticationProvider(publicClientApplication, scopesArray);
                    return authProvider;
                }

                catch (ServiceException ex)
                {
                    throw new Exception("Could not create an IAuthenticationProvider: " + ex.Message);
                }
            }
            return authProvider;
        }
    }
}