
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.Admin
{
    [DebuggerStepThrough]
    public class HttpClientDiagnosticsHandler : DelegatingHandler
    {
        //private static readonly ILog Logger = LogProvider.For<HttpClientDiagnosticsHandler>();

        public HttpClientDiagnosticsHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        public HttpClientDiagnosticsHandler()
        {
        }

        /*protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (LogProvider.OpenNestedContext("SendAsync"))
            {
                var totalElapsedTime = Stopwatch.StartNew();

                Logger.Debug(string.Format("Request: {0}", request));
                if (request.Content != null)
                {
                    var content = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Logger.Debug(string.Format("Request Content: {0}", content));
                }

                var responseElapsedTime = Stopwatch.StartNew();
                var response = await base.SendAsync(request, cancellationToken);

                Logger.Debug(string.Format("Response: {0}", response));
                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Logger.Debug(string.Format("Response Content: {0}", content));
                }

                responseElapsedTime.Stop();
                Logger.Debug(string.Format("Response elapsed time: {0} ms", responseElapsedTime.ElapsedMilliseconds));

                totalElapsedTime.Stop();
                Logger.Debug(string.Format("Total elapsed time: {0} ms", totalElapsedTime.ElapsedMilliseconds));

                return response;
            }
        }
    */
    }
}
