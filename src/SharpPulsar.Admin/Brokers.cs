using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.interfaces;
using SharpPulsar.Admin.Model;

namespace SharpPulsar.Admin
{
    public class Brokers: IBrokers
    {
        private Uri _uri;
        private HttpClient _httpClient;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public Brokers(string brokerwebserviceurl, HttpClient httpClient)
        {
            _uri = new Uri($"{brokerwebserviceurl.TrimEnd('/')}/admin/v2");
            _httpClient = httpClient;
            if (_uri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
        }
        public string BacklogQuotaCheck(Dictionary<string, List<string>> customHeaders = null)
        {
            return BacklogQuotaCheckAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async ValueTask<string> BacklogQuotaCheckAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/backlog-quota-check").ToString();
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 500)
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

        public IList<string> GetDynamicConfigurationNames(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDynamicConfigurationNamesAsync(customHeaders).GetAwaiter().GetResult(); 
        }
        /// <summary>
        /// Get all updatable dynamic configurations's name
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
        public async ValueTask<IList<string>> GetDynamicConfigurationNamesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/configuration").ToString();
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
            IList<string> _result = null;

            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IList<string>>(_responseContent);
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

        public IDictionary<string, string> GetRuntimeConfigurations(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetRuntimeConfigurationsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get all runtime configurations. This operation requires Pulsar super-user
        /// privileges.
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
        public async ValueTask<IDictionary<string, string>> GetRuntimeConfigurationsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/configuration/runtime").ToString();
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
            IDictionary<string, string> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IDictionary<string, string>>(_responseContent);
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

        public IDictionary<string, string> GetAllDynamicConfigurations(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAllDynamicConfigurationsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get value of all dynamic configurations' value overridden on local config
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
        public async ValueTask<IDictionary<string, string>> GetAllDynamicConfigurationsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/configuration/values").ToString();
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 500)
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
            IDictionary<string, string> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IDictionary<string, string>>(_responseContent);
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

        public string DeleteDynamicConfiguration(string configName, Dictionary<string, List<string>> customHeaders = null)
        {
           return DeleteDynamicConfigurationAsync(configName, customHeaders).GetAwaiter().GetResult();    
        }
        /// <summary>
        /// Delete dynamic serviceconfiguration into zk only. This operation requires
        /// Pulsar super-user privileges.
        /// </summary>
        /// <param name='configName'>
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
        public async ValueTask<string> DeleteDynamicConfigurationAsync(string configName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (configName == null)
            {
                throw new InvalidOperationException("configName");
            }
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/configuration/{configName}").ToString();
            _url = _url.Replace("{configName}", Uri.EscapeDataString(configName));
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
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 204 && (int)_statusCode != 403 && (int)_statusCode != 412 && (int)_statusCode != 500)
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
        public string UpdateDynamicConfiguration(string configName, string configValue, Dictionary<string, List<string>> customHeaders = null)
        {
           return UpdateDynamicConfigurationAsync(configName, configValue, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update dynamic serviceconfiguration into zk only. This operation requires
        /// Pulsar super-user privileges.
        /// </summary>
        /// <param name='configName'>
        /// </param>
        /// <param name='configValue'>
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
        public async ValueTask<string> UpdateDynamicConfigurationAsync(string configName, string configValue, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (configName == null)
            {
                throw new InvalidOperationException("configName");
            }
            if (configValue == null)
            {
                throw new InvalidOperationException("configValue");
            }
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/configuration/{configName}/{configValue}").ToString();
            _url = _url.Replace("{configName}", Uri.EscapeDataString(configName));
            _url = _url.Replace("{configValue}", Uri.EscapeDataString(configValue));
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

            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            

            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 204 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
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
        public string Healthcheck(TopicVersion? topicVersion, Dictionary<string, List<string>> customHeaders = null)
        {
           return HealthcheckAsync(topicVersion, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Run a healthcheck against the broker
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
        public async ValueTask<string> HealthcheckAsync(TopicVersion? topicVersion, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/health").ToString();
            List<string> _queryParameters = new List<string>();
            if (topicVersion != null)
            {
                _queryParameters.Add(string.Format("topicVersion={0}", Uri.EscapeDataString(JsonSerializer.Serialize(topicVersion, _jsonSerializerOptions).Trim('"'))));
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
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 500)
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
        public InternalConfigurationData GetInternalConfigurationData(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetInternalConfigurationDataAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the internal configuration data
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
        public async ValueTask<InternalConfigurationData> GetInternalConfigurationDataAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/internal-configuration").ToString();
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
            InternalConfigurationData _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<InternalConfigurationData>(_responseContent);
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
        public BrokerInfo GetLeaderBroker(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetLeaderBrokerAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the information of the leader broker.
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
        public async ValueTask<BrokerInfo> GetLeaderBrokerAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/leaderBroker").ToString();
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
            if ((int)_statusCode != 200 && (int)_statusCode != 401 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            BrokerInfo _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<BrokerInfo>(_responseContent);
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
        public string IsReady(Dictionary<string, List<string>> customHeaders = null)
        {
            return IsReadyAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Check if the broker is fully initialized
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
        public async ValueTask<string> IsReadyAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/ready").ToString();
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
            if ((int)_statusCode != 200 && (int)_statusCode != 500)
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
        public string Version(Dictionary<string, List<string>> customHeaders = null)
        {
            return VersionAsync(customHeaders).GetAwaiter().GetResult();    
        }
        /// <summary>
        /// Get version of current broker
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
        public async ValueTask<string> VersionAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/version").ToString();
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
            if ((int)_statusCode != 200 && (int)_statusCode != 500)
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
            string _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<string>(_responseContent);
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
        public IDictionary<string, NamespaceOwnershipStatus> GetOwnedNamespaces(string clusterName, string brokerWebserviceurl, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOwnedNamespacesAsync(clusterName, brokerWebserviceurl, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of namespaces served by the specific broker
        /// </summary>
        /// <param name='clusterName'>
        /// </param>
        /// <param name='brokerWebserviceurl'>
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
        public async ValueTask<IDictionary<string, NamespaceOwnershipStatus>> GetOwnedNamespacesAsync(string clusterName, string brokerWebserviceurl, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (clusterName == null)
            {
                throw new InvalidOperationException("clusterName");
            }
            if (brokerWebserviceurl == null)
            {
                throw new InvalidOperationException("brokerWebserviceurl");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/{clusterName}/{broker-webserviceurl}/ownedNamespaces").ToString();
            _url = _url.Replace("{clusterName}", Uri.EscapeDataString(clusterName));
            _url = _url.Replace("{broker-webserviceurl}", Uri.EscapeDataString(brokerWebserviceurl));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            IDictionary<string, NamespaceOwnershipStatus> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IDictionary<string, NamespaceOwnershipStatus>>(_responseContent);
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
        public IList<string> GetActiveBrokers(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetActiveBrokersAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of active brokers (web service addresses) in the cluster.If
        /// authorization is not enabled, any cluster name is valid.
        /// </summary>
        /// <param name='cluster'>
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
        public async ValueTask<IList<string>> GetActiveBrokersAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }

            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/{cluster}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 401 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
        public string ShutDownBrokerGracefully(int? maxConcurrentUnloadPerSec, bool? forcedTerminateTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
           return ShutDownBrokerGracefullyAsync(maxConcurrentUnloadPerSec, forcedTerminateTopic, customHeaders).GetAwaiter().GetResult();
        }
        public async ValueTask<string> ShutDownBrokerGracefullyAsync(int? maxConcurrentUnloadPerSec, bool? forcedTerminateTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "brokers/shutdown").ToString();
            // Create HTTP transport objects
            List<string> _queryParameters = new List<string>();
            if (maxConcurrentUnloadPerSec != null)
            {
                _queryParameters.Add(string.Format("maxConcurrentUnloadPerSec={0}", Uri.EscapeDataString(JsonSerializer.Serialize(maxConcurrentUnloadPerSec, _jsonSerializerOptions).Trim('"'))));
            }
            if (forcedTerminateTopic != null)
            {
                _queryParameters.Add(string.Format("forcedTerminateTopic={0}", Uri.EscapeDataString(JsonSerializer.Serialize(forcedTerminateTopic, _jsonSerializerOptions).Trim('"'))));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
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
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);

            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 204 && (int)_statusCode != 403 && (int)_statusCode != 500)
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

    }
}
