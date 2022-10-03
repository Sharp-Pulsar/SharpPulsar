using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;

namespace SharpPulsar.Admin
{
    public class NonPersistentTopics
    {
        private Uri _uri;
        private HttpClient _httpClient;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public NonPersistentTopics(string brokerwebserviceurl, HttpClient httpClient)
        {
            _uri = new Uri($"{brokerwebserviceurl.TrimEnd('/')}/admin/v2");
            _httpClient = httpClient;
            if (_uri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
        }

        public PartitionedTopicMetadata GetPartitionedMetadata(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? checkAllowAutoCreation = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetPartitionedMetadataAsync(tenant, namespaceParameter, topic, authoritative, checkAllowAutoCreation, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get partitioned topic metadata.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='checkAllowAutoCreation'>
        /// Is check configuration required to automatically create topic
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
        public async ValueTask<PartitionedTopicMetadata> GetPartitionedMetadataAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? checkAllowAutoCreation = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }
           
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{topic}/partitions").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", Uri.EscapeDataString(JsonSerializer.Serialize(authoritative, _jsonSerializerOptions).Trim('"'))));
            }
            if (checkAllowAutoCreation != null)
            {
                _queryParameters.Add(string.Format("checkAllowAutoCreation={0}", Uri.EscapeDataString(JsonSerializer.Serialize(checkAllowAutoCreation, _jsonSerializerOptions).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
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
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 401 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500 && (int)_statusCode != 503)
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
            PartitionedTopicMetadata _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<PartitionedTopicMetadata>(_responseContent);
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

        public PersistentTopicInternalStats GetInternalStats(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? metadata = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetInternalStatsAsync(tenant, namespaceParameter, topic, authoritative, metadata, customHeaders).GetAwaiter().GetResult();  
        }
        /// <summary>
        /// Get the internal stats for the topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='metadata'>
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
        public async ValueTask<PersistentTopicInternalStats> GetInternalStatsAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? metadata = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{topic}/internalStats").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", Uri.EscapeDataString(JsonSerializer.Serialize(authoritative, _jsonSerializerOptions).Trim('"'))));
            }
            if (metadata != null)
            {
                _queryParameters.Add(string.Format("metadata={0}", Uri.EscapeDataString(JsonSerializer.Serialize(metadata, _jsonSerializerOptions).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
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
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 401 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
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
            PersistentTopicInternalStats _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<PersistentTopicInternalStats>(_responseContent);
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

        public string CreatePartitionedTopic(string tenant, string namespaceParameter, string topic, int body, Dictionary<string, List<string>> customHeaders = null)
        {
           return CreatePartitionedTopicAsync(tenant, namespaceParameter, topic, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Create a partitioned topic.
        /// </summary>
        /// <remarks>
        /// It needs to be called before creating a producer on a partitioned topic.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='body'>
        /// The number of partitions for the topic
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
        public async ValueTask<string> CreatePartitionedTopicAsync(string tenant, string namespaceParameter, string topic, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{topic}/partitions").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("PUT");
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

            // Serialize Request
            string _requestContent = null;
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 401 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 406 && (int)_statusCode != 409 && (int)_statusCode != 412 && (int)_statusCode != 500 && (int)_statusCode != 503)
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
            
            return _httpResponse.ReasonPhrase;
        }

        public string GetPartitionedStats(string tenant, string namespaceParameter, string topic, bool? perPartition = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, bool? getEarliestTimeInBacklog = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetPartitionedStatsAsync(tenant, namespaceParameter, topic, perPartition, authoritative, getPreciseBacklog, subscriptionBacklogSize, getEarliestTimeInBacklog).GetAwaiter().GetResult();  
        }
        /// <summary>
        /// Get the stats for the partitioned topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='perPartition'>
        /// Get per partition stats
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='getPreciseBacklog'>
        /// If return precise backlog or imprecise backlog
        /// </param>
        /// <param name='subscriptionBacklogSize'>
        /// If return backlog size for each subscription, require locking on ledger so
        /// be careful not to use when there's heavy traffic.
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
        public async ValueTask<string> GetPartitionedStatsAsync(string tenant, string namespaceParameter, string topic, bool? perPartition = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, bool? getEarliestTimeInBacklog = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }
           
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{topic}/partitioned-stats").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            List<string> _queryParameters = new List<string>();
            if (perPartition != null)
            {
                _queryParameters.Add(string.Format("perPartition={0}", Uri.EscapeDataString(JsonSerializer.Serialize(perPartition, _jsonSerializerOptions).Trim('"'))));
            }
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", Uri.EscapeDataString(JsonSerializer.Serialize(authoritative, _jsonSerializerOptions).Trim('"'))));
            }
            if (getPreciseBacklog != null)
            {
                _queryParameters.Add(string.Format("getPreciseBacklog={0}", Uri.EscapeDataString(JsonSerializer.Serialize(getPreciseBacklog, _jsonSerializerOptions).Trim('"'))));
            }
            if (subscriptionBacklogSize != null)
            {
                _queryParameters.Add(string.Format("subscriptionBacklogSize={0}", Uri.EscapeDataString(JsonSerializer.Serialize(subscriptionBacklogSize, _jsonSerializerOptions).Trim('"'))));
            }
            if (getEarliestTimeInBacklog != null)
            {
                _queryParameters.Add(string.Format("getEarliestTimeInBacklog={0}", Uri.EscapeDataString(JsonSerializer.Serialize(getEarliestTimeInBacklog, _jsonSerializerOptions).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
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
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 401 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500 && (int)_statusCode != 503)
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
            
            return _httpResponse.ReasonPhrase;
        }

        public string UnloadTopic(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return UnloadTopicAsync(tenant, namespaceParameter, topic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Unload a topic
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
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
        public async ValueTask<string> UnloadTopicAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{topic}/unload").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", Uri.EscapeDataString(JsonSerializer.Serialize(authoritative, _jsonSerializerOptions).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("PUT");
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
           
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 401 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500 && (int)_statusCode != 503)
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
           
            return _httpResponse.ReasonPhrase;
        }

        public IList<string> GetList(string tenant, string namespaceParameter, string? bundle = null, bool? includeSystemTopic = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetListAsync(tenant, namespaceParameter, bundle, includeSystemTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of non-persistent topics under a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
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
        public async ValueTask<IList<string>> GetListAsync(string tenant, string namespaceParameter, string? bundle = null, bool? includeSystemTopic = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (bundle != null)
            {
                _queryParameters.Add(string.Format("bundle={0}", Uri.EscapeDataString(bundle)));
            }
            if (includeSystemTopic != null)
            {
                _queryParameters.Add(string.Format("includeSystemTopic={0}", Uri.EscapeDataString(JsonSerializer.Serialize(includeSystemTopic, _jsonSerializerOptions).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
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
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 401 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
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
            IList<string> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IList<string>>(_responseContent);
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

        public IList<string> GetListFromBundle(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetListFromBundleAsync(tenant, namespaceParameter, bundle, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of non-persistent topics under a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='bundle'>
        /// Bundle range of a topic
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
        public async ValueTask<IList<string>> GetListFromBundleAsync(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (bundle == null)
            {
                throw new InvalidOperationException("bundle");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{bundle}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", Uri.EscapeDataString(bundle));
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
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 401 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500 && (int)_statusCode != 503)
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
            IList<string> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IList<string>>(_responseContent);
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

        public string TruncateTopic(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return TruncateTopicAsync(tenant, namespaceParameter, topic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Truncate a topic.
        /// </summary>
        /// <remarks>
        /// NonPersistentTopic does not support truncate.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
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
        public async ValueTask<string> TruncateTopicAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{topic}/truncate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", Uri.EscapeDataString(JsonSerializer.Serialize(authoritative, _jsonSerializerOptions).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("DELETE");
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
           
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 412)
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
            
            return _httpResponse.ReasonPhrase;
        }

        public EntryFilters GetEntryFilters(string tenant, string namespaceParameter, string topic, bool? applied = false, bool? isGlobal = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetEntryFiltersAsync(tenant, namespaceParameter, topic, applied, isGlobal, authoritative, customHeaders).GetAwaiter().GetResult();
        }

        public async ValueTask<EntryFilters> GetEntryFiltersAsync(string tenant, string namespaceParameter, string topic, bool? applied = false, bool? isGlobal = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }

            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{topic}/entryFilters").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            List<string> _queryParameters = new List<string>();
            
            if (applied != null)
            {
                _queryParameters.Add(string.Format("applied={0}", Uri.EscapeDataString(JsonSerializer.Serialize(applied, _jsonSerializerOptions).Trim('"'))));
            }
            if (isGlobal != null)
            {
                _queryParameters.Add(string.Format("isGlobal={0}", Uri.EscapeDataString(JsonSerializer.Serialize(isGlobal, _jsonSerializerOptions).Trim('"'))));
            }
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", Uri.EscapeDataString(JsonSerializer.Serialize(authoritative, _jsonSerializerOptions).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
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

            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            EntryFilters _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<EntryFilters>(_responseContent);
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

        public string SetEntryFilters(string tenant, string namespaceParameter, string topic, bool? isGlobal = false, bool? authoritative = false, EntryFilters body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return  SetEntryFiltersAsync(tenant, namespaceParameter, topic, isGlobal, authoritative, body).GetAwaiter().GetResult();
        }

        public async ValueTask<string> SetEntryFiltersAsync(string tenant, string namespaceParameter, string topic, bool? isGlobal = false, bool? authoritative = false, EntryFilters body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }

            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{topic}/entryFilters").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            List<string> _queryParameters = new List<string>();

            if (isGlobal != null)
            {
                _queryParameters.Add(string.Format("isGlobal={0}", Uri.EscapeDataString(JsonSerializer.Serialize(isGlobal, _jsonSerializerOptions).Trim('"'))));
            }
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", Uri.EscapeDataString(JsonSerializer.Serialize(authoritative, _jsonSerializerOptions).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("POST");
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

            // Serialize Request
            string _requestContent = null;
            if (body != null)
            {
                _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
                _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }

            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);

            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            return _httpResponse.ReasonPhrase;
        }

        public string RemoveEntryFilters(string tenant, string namespaceParameter, string topic, bool? isGlobal = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveEntryFiltersAsync(tenant, namespaceParameter, topic, isGlobal, authoritative, customHeaders).GetAwaiter().GetResult();
        }

        public async ValueTask<string> RemoveEntryFiltersAsync(string tenant, string namespaceParameter, string topic, bool? isGlobal = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }

            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{topic}/entryFilters").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            List<string> _queryParameters = new List<string>();

            if (isGlobal != null)
            {
                _queryParameters.Add(string.Format("isGlobal={0}", Uri.EscapeDataString(JsonSerializer.Serialize(isGlobal, _jsonSerializerOptions).Trim('"'))));
            }
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", Uri.EscapeDataString(JsonSerializer.Serialize(authoritative, _jsonSerializerOptions).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("DELETE");
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

            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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

            return _httpResponse.ReasonPhrase;
        }

        /// <summary>
        /// Get the stats for the topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='getPreciseBacklog'>
        /// If return precise backlog or imprecise backlog
        /// </param>
        /// <param name='subscriptionBacklogSize'>
        /// If return backlog size for each subscription, require locking on ledger so
        /// be careful not to use when there's heavy traffic.
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async System.Threading.Tasks.Task<Microsoft.Rest.HttpOperationResponse<NonPersistentTopicStats>> GetStatsAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, bool? getEarliestTimeInBacklog = false, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (topic == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "topic");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                System.Collections.Generic.Dictionary<string, object> tracingParameters = new System.Collections.Generic.Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("topic", topic);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("getPreciseBacklog", getPreciseBacklog);
                tracingParameters.Add("subscriptionBacklogSize", subscriptionBacklogSize);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetStats", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new System.Uri(new System.Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "non-persistent/{tenant}/{namespace}/{topic}/stats").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", System.Uri.EscapeDataString(topic));
            System.Collections.Generic.List<string> _queryParameters = new System.Collections.Generic.List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
            }
            if (getPreciseBacklog != null)
            {
                _queryParameters.Add(string.Format("getPreciseBacklog={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(getPreciseBacklog, this.SerializationSettings).Trim('"'))));
            }
            if (subscriptionBacklogSize != null)
            {
                _queryParameters.Add(string.Format("subscriptionBacklogSize={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(subscriptionBacklogSize, this.SerializationSettings).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
            // Create HTTP transport objects
            var _httpRequest = new System.Net.Http.HttpRequestMessage();
            System.Net.Http.HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new System.Net.Http.HttpMethod("GET");
            _httpRequest.RequestUri = new System.Uri(_url);
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

            // Serialize Request
            string _requestContent = null;
            // Send Request
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.SendRequest(_invocationId, _httpRequest);
            }
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await this.HttpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.ReceiveResponse(_invocationId, _httpResponse);
            }
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 401 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
            {
                var ex = new Microsoft.Rest.HttpOperationException(string.Format("Operation returned an invalid status code '{0}'", _statusCode));
                if (_httpResponse.Content != null)
                {
                    _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    _responseContent = string.Empty;
                }
                ex.Request = new Microsoft.Rest.HttpRequestMessageWrapper(_httpRequest, _requestContent);
                ex.Response = new Microsoft.Rest.HttpResponseMessageWrapper(_httpResponse, _responseContent);
                if (_shouldTrace)
                {
                    Microsoft.Rest.ServiceClientTracing.Error(_invocationId, ex);
                }
                _httpRequest.Dispose();
                if (_httpResponse != null)
                {
                    _httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            var _result = new Microsoft.Rest.HttpOperationResponse<NonPersistentTopicStats>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<NonPersistentTopicStats>(_responseContent, this.DeserializationSettings);
                }
                catch (Newtonsoft.Json.JsonException ex)
                {
                    _httpRequest.Dispose();
                    if (_httpResponse != null)
                    {
                        _httpResponse.Dispose();
                    }
                    throw new Microsoft.Rest.SerializationException("Unable to deserialize the response.", _responseContent, ex);
                }
            }
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
        }

    }
}
