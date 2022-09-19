using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.Admin.Models;
using SharpPulsar.Admin.Model;

namespace SharpPulsar.Admin
{
    public class BrokerStats
    {
        private Uri _uri;
        private HttpClient _httpClient;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public BrokerStats(string brokerwebserviceurl, HttpClient httpClient)
        {
            _uri = new Uri($"{brokerwebserviceurl.TrimEnd('/')}/admin/v2");
            _httpClient = httpClient;
            if (_uri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
        }
        public object GetTopics(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTopicsAsync(customHeaders).GetAwaiter().GetResult();    
        }
        /// <summary>
        /// Get all the topic stats by namespace
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async ValueTask<object> GetTopicsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var baseUrl = _uri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/")), "broker-stats/topics").ToString();
            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = new Uri(url);
            // Set Headers


            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    if (httpRequest.Headers.Contains(header.Key))
                    {
                        httpRequest.Headers.Remove(header.Key);
                    }
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            
            System.Net.HttpStatusCode statusCode = httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string responseContent = null;
            if ((int)statusCode != 200 && (int)statusCode != 403)
            {
                if (httpResponse.Content != null)
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    responseContent = string.Empty;
                }

                var ex = new Exception(string.Format("Operation returned an invalid status code '{0}': Content {1}", statusCode, responseContent));

                httpRequest.Dispose();
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            object result = null;
            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<object>(responseContent);
                }
                catch (JsonException ex)
                {
                    httpRequest.Dispose();
                    if (httpResponse != null)
                    {
                        httpResponse.Dispose();
                    }
                    throw new Exception($"Unable to deserialize the response. {responseContent} {ex}");
                }
            }
            return result;
        }

        public object GetTopics2(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTopics2Async(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get all the topic stats by namespace
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async ValueTask<object> GetTopics2Async(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var baseUrl = _uri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/")), "broker-stats/destinations").ToString();
            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = new Uri(url);
            // Set Headers


            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    if (httpRequest.Headers.Contains(header.Key))
                    {
                        httpRequest.Headers.Remove(header.Key);
                    }
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            System.Net.HttpStatusCode statusCode = httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string responseContent = null;
            if ((int)statusCode != 200 && (int)statusCode != 403)
            {
                if (httpResponse.Content != null)
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    responseContent = string.Empty;
                }

                var ex = new Exception(string.Format("Operation returned an invalid status code '{0}': Content {1}", statusCode, responseContent));

                httpRequest.Dispose();
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            object result = null;
            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<object>(responseContent);
                }
                catch (JsonException ex)
                {
                    httpRequest.Dispose();
                    if (httpResponse != null)
                    {
                        httpResponse.Dispose();
                    }
                    throw new Exception($"Unable to deserialize the response. {responseContent} {ex}");
                }
            }
            return result;
        }

        public IDictionary<string, ResourceUnit> GetBrokerResourceAvailability(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBrokerResourceAvailabilityAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Broker availability report
        /// </summary>
        /// <remarks>
        /// This API gives the current broker availability in percent, each resource
        /// percentage usage is calculated and thensum of all of the resource usage
        /// percent is called broker-resource-availability&lt;br/&gt;&lt;br/&gt;THIS
        /// API IS ONLY FOR USE BY TESTING FOR CONFIRMING NAMESPACE ALLOCATION
        /// ALGORITHM
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async ValueTask<IDictionary<string, ResourceUnit>> GetBrokerResourceAvailabilityAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
           
            // Construct URL
            var baseUrl = _uri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/")), "broker-stats/broker-resource-availability/{tenant}/{namespace}").ToString();
            url = url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            url = url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = new Uri(url);
            // Set Headers


            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    if (httpRequest.Headers.Contains(header.Key))
                    {
                        httpRequest.Headers.Remove(header.Key);
                    }
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            
            System.Net.HttpStatusCode statusCode = httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string responseContent = null;
            if ((int)statusCode != 200 && (int)statusCode != 403 && (int)statusCode != 409)
            {
                if (httpResponse.Content != null)
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    responseContent = string.Empty;
                }

                var ex = new Exception(string.Format("Operation returned an invalid status code '{0}': Content {1}", statusCode, responseContent));

                httpRequest.Dispose();
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            IDictionary<string, ResourceUnit> result = null;
            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, ResourceUnit>>(responseContent);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    httpRequest.Dispose();
                    if (httpResponse != null)
                    {
                        httpResponse.Dispose();
                    }
                    throw new Exception($"Unable to deserialize the response. {responseContent} {ex}");
                }
            }
            return result;
        }
        public LoadReport GetLoadReport(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetLoadReportAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get Load for this broker
        /// </summary>
        /// <remarks>
        /// consists of topics stats &amp; systemResourceUsage
        /// </remarks>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async ValueTask<LoadReport> GetLoadReportAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "broker-stats/load-report").ToString();
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("GET");
            _httpRequest.RequestUri = new Uri(_url);
            // Set Headers


            if (customHeaders != null)
            {
                foreach (var _header in customHeaders)
                {
                    if (_httpRequest.Headers.Contains(_header.Key))
                    {
                        _httpRequest.Headers.Remove(_header.Key);
                    }
                    _httpRequest.Headers.TryAddWithoutValidation(_header.Key, _header.Value);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403)
            {

                if (_httpResponse.Content != null)
                {
                    _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    _responseContent = string.Empty;
                }

                var ex = new Exception(string.Format("Operation returned an invalid status code '{0}': Content {1}", _statusCode, _responseContent));

                _httpRequest.Dispose();
                if (_httpResponse != null)
                {
                    _httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            LoadReport _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = System.Text.Json.JsonSerializer.Deserialize<LoadReport>(_responseContent);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    _httpRequest.Dispose();
                    if (_httpResponse != null)
                    {
                        _httpResponse.Dispose();
                    }
                    throw new Exception($"Unable to deserialize the response. {_responseContent} {ex}"); 
                }
            }
            return _result;
        }
        public IList<Metrics> GetMBeans(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMetricsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get all the mbean details of this broker JVM
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async ValueTask<IList<Metrics>> GetMBeansAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "broker-stats/mbeans").ToString();
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("GET");
            _httpRequest.RequestUri = new Uri(_url);
            // Set Headers


            if (customHeaders != null)
            {
                foreach (var _header in customHeaders)
                {
                    if (_httpRequest.Headers.Contains(_header.Key))
                    {
                        _httpRequest.Headers.Remove(_header.Key);
                    }
                    _httpRequest.Headers.TryAddWithoutValidation(_header.Key, _header.Value);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403)
            {

                if (_httpResponse.Content != null)
                {
                    _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    _responseContent = string.Empty;
                }

                var ex = new Exception(string.Format("Operation returned an invalid status code '{0}': Content {1}", _statusCode, _responseContent));

                _httpRequest.Dispose();
                if (_httpResponse != null)
                {
                    _httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            IList<Metrics> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = System.Text.Json.JsonSerializer.Deserialize<IList<Metrics>>(_responseContent);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    _httpRequest.Dispose();
                    if (_httpResponse != null)
                    {
                        _httpResponse.Dispose();
                    }
                    throw new Exception($"Unable to deserialize the response. {_responseContent} {ex}");
                }
            }
            return _result;
        }
        public IList<Metrics> GetMetrics(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMetricsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets the metrics for Monitoring
        /// </summary>
        /// <remarks>
        /// Requested should be executed by Monitoring agent on each broker to fetch
        /// the metrics
        /// </remarks>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async ValueTask<IList<Metrics>> GetMetricsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "broker-stats/metrics").ToString();
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("GET");
            _httpRequest.RequestUri = new Uri(_url);
            // Set Headers


            if (customHeaders != null)
            {
                foreach (var _header in customHeaders)
                {
                    if (_httpRequest.Headers.Contains(_header.Key))
                    {
                        _httpRequest.Headers.Remove(_header.Key);
                    }
                    _httpRequest.Headers.TryAddWithoutValidation(_header.Key, _header.Value);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403)
            {
                if (_httpResponse.Content != null)
                {
                    _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    _responseContent = string.Empty;
                }

                var ex = new Exception(string.Format("Operation returned an invalid status code '{0}': Content {1}", _statusCode, _responseContent));

                _httpRequest.Dispose();
                if (_httpResponse != null)
                {
                    _httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            IList<Metrics> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = System.Text.Json.JsonSerializer.Deserialize<IList<Metrics>>(_responseContent);
                }

                catch (System.Text.Json.JsonException ex)
                {
                    _httpRequest.Dispose();
                    if (_httpResponse != null)
                    {
                        _httpResponse.Dispose();
                    }
                    throw new Exception($"Unable to deserialize the response. {_responseContent} {ex}");
                }
            }
            return _result;
        }
        public AllocatorStats GetAllocatorStats(string allocator, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAllocatorStatsAsync(allocator, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the stats for the Netty allocator. Available allocators are 'default'
        /// and 'ml-cache'
        /// </summary>
        /// <param name='allocator'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async ValueTask<AllocatorStats> GetAllocatorStatsAsync(string allocator, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (allocator == null)
            {
                throw new InvalidOperationException("allocator");
            }
           
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "broker-stats/allocator-stats/{allocator}").ToString();
            _url = _url.Replace("{allocator}", Uri.EscapeDataString(allocator));
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("GET");
            _httpRequest.RequestUri = new Uri(_url);
            // Set Headers


            if (customHeaders != null)
            {
                foreach (var _header in customHeaders)
                {
                    if (_httpRequest.Headers.Contains(_header.Key))
                    {
                        _httpRequest.Headers.Remove(_header.Key);
                    }
                    _httpRequest.Headers.TryAddWithoutValidation(_header.Key, _header.Value);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403)
            {
                if (_httpResponse.Content != null)
                {
                    _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    _responseContent = string.Empty;
                }

                var ex = new Exception(string.Format("Operation returned an invalid status code '{0}': Content {1}", _statusCode, _responseContent));

                _httpRequest.Dispose();
                if (_httpResponse != null)
                {
                    _httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            AllocatorStats _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = System.Text.Json.JsonSerializer.Deserialize<AllocatorStats>(_responseContent);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    _httpRequest.Dispose();
                    if (_httpResponse != null)
                    {
                        _httpResponse.Dispose();
                    }
                    throw new Exception($"Unable to deserialize the response. {_responseContent} {ex}");
                }
            }
            return _result;
        }
        public IDictionary<string, PendingBookieOpsStats> GetPendingBookieOpsStats(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPendingBookieOpsStatsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get pending bookie client op stats by namesapce
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async ValueTask<IDictionary<string, PendingBookieOpsStats>> GetPendingBookieOpsStatsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "broker-stats/bookieops").ToString();
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("GET");
            _httpRequest.RequestUri = new Uri(_url);
            // Set Headers


            if (customHeaders != null)
            {
                foreach (var _header in customHeaders)
                {
                    if (_httpRequest.Headers.Contains(_header.Key))
                    {
                        _httpRequest.Headers.Remove(_header.Key);
                    }
                    _httpRequest.Headers.TryAddWithoutValidation(_header.Key, _header.Value);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403)
            {
                if (_httpResponse.Content != null)
                {
                    _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    _responseContent = string.Empty;
                }

                var ex = new Exception(string.Format("Operation returned an invalid status code '{0}': Content {1}", _statusCode, _responseContent));

                _httpRequest.Dispose();
                if (_httpResponse != null)
                {
                    _httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            IDictionary<string, PendingBookieOpsStats> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, PendingBookieOpsStats>>(_responseContent);
                }

                catch (JsonException ex)
                {
                    _httpRequest.Dispose();
                    if (_httpResponse != null)
                    {
                        _httpResponse.Dispose();
                    }
                    throw new Exception($"Unable to deserialize the response. {_responseContent} {ex}");
                }
            }
            return _result;
        }

    }
}
