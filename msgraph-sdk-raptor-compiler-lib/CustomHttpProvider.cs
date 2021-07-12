using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace MsGraphSDKSnippetsCompiler
{
    /// <summary>
    ///     Provides interception to catch errors thrown during execution.
    /// </summary>
    public class CustomHttpProvider : HttpProvider, IHttpProvider
    {
        async Task<HttpResponseMessage> IHttpProvider.SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri;
            var headers = request.Headers;
            
            var stopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();
                var response = await base.SendAsync(request, completionOption, cancellationToken);
                stopWatch.Stop();
                Console.WriteLine($"\nRequest Uri:\t{uri}");
                Console.WriteLine($"\nResponse Headers:\n{response.Headers}");
                Console.WriteLine($"\nRequest Duration: \n{stopWatch.Elapsed}");
                return response;
            }
            catch (Exception e)
            {
                throw new Exception($"Request URI: {uri}{Environment.NewLine}Request Headers:{Environment.NewLine}{headers}", e);
            }
        }
    }
}
