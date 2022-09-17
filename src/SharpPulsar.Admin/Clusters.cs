using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.interfaces;
using SharpPulsar.Admin.Model;

namespace SharpPulsar.Admin
{
    public class Clusters : IClusters
    {
        private Uri _uri;
        private HttpClient _httpClient;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public Clusters(string brokerwebserviceurl, HttpClient httpClient)
        {
            _uri = new Uri($"{brokerwebserviceurl.TrimEnd('/')}/admin/v2");
            _httpClient = httpClient;
            if (_uri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
        }
        public IList<string> GetClusters(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetClustersAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of all the Pulsar clusters.
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
        public async ValueTask<IList<string>> GetClustersAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters").ToString();
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

        public ClusterData GetCluster(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetClusterAsync(cluster, customHeaders).GetAwaiter().GetResult();    
        }
        /// <summary>
        /// Get the configuration for the specified cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
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
        public async ValueTask<ClusterData> GetClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}").ToString();
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
            ClusterData _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<ClusterData>(_responseContent);
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

        public string UpdateCluster(string cluster, ClusterData body, Dictionary<string, List<string>> customHeaders = null)
        {
            return UpdateClusterAsync(cluster, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update the configuration for a cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='body'>
        /// The cluster data
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
        public async ValueTask<string> UpdateClusterAsync(string cluster, ClusterData body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
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
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 204 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 500)
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
        public string CreateCluster(string cluster, ClusterData body, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateClusterAsync(cluster, body, customHeaders).GetAwaiter().GetResult();   
        }
      
        /// <summary>
        /// Create a new cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges, and the name cannot
        /// contain the '/' characters.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='body'>
        /// The cluster data
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
        public async ValueTask<string> CreateClusterAsync(string cluster, ClusterData body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
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
            if (body != null)
            {
                _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
                _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 204 && (int)_statusCode != 403 && (int)_statusCode != 409 && (int)_statusCode != 412 && (int)_statusCode != 500)
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

        public string DeleteCluster(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteClusterAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete an existing cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
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
        public async ValueTask<string> DeleteClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
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

        public IDictionary<string, FailureDomain> GetFailureDomains(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetFailureDomainsAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the cluster failure domains.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
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
        public async ValueTask<IDictionary<string, FailureDomain>> GetFailureDomainsAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
           
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/failureDomains").ToString();
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
            // Create Result
            IDictionary<string, FailureDomain> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IDictionary<string, FailureDomain>>(_responseContent);
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

        public FailureDomain GetDomain(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDomainAsync(cluster, domainName, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get a domain in a cluster
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='domainName'>
        /// The failure domain name
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
        public async ValueTask<FailureDomain> GetDomainAsync(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (domainName == null)
            {
                throw new InvalidOperationException("domainName");
            }
            // Construct URL
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/failureDomains/{domainName}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
            _url = _url.Replace("{domainName}", Uri.EscapeDataString(domainName));
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

            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
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
            FailureDomain _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<FailureDomain>(_responseContent);
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
        public string SetFailureDomain(string cluster, string domainName, FailureDomain body, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetFailureDomainAsync(cluster, domainName, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set the failure domain of the cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='domainName'>
        /// The failure domain name
        /// </param>
        /// <param name='body'>
        /// The configuration data of a failure domain
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
        public async ValueTask<string> SetFailureDomainAsync(string cluster, string domainName, FailureDomain body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (domainName == null)
            {
                throw new InvalidOperationException("domainName");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/failureDomains/{domainName}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
            _url = _url.Replace("{domainName}", Uri.EscapeDataString(domainName));
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
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412 && (int)_statusCode != 500)
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

        public string DeleteFailureDomain(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteFailureDomainAsync(cluster, domainName, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete the failure domain of the cluster
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='domainName'>
        /// The failure domain name
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
        public async ValueTask<string> DeleteFailureDomainAsync(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (domainName == null)
            {
                throw new InvalidOperationException("domainName");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/failureDomains/{domainName}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
            _url = _url.Replace("{domainName}", Uri.EscapeDataString(domainName));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
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

        public IDictionary<string, NamespaceIsolationData> GetNamespaceIsolationPolicies(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceIsolationPoliciesAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the namespace isolation policies assigned to the cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
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
        public async ValueTask<IDictionary<string, NamespaceIsolationData>> GetNamespaceIsolationPoliciesAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/namespaceIsolationPolicies").ToString();
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
            IDictionary<string, NamespaceIsolationData> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IDictionary<string, NamespaceIsolationData>>(_responseContent);
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

        public IList<BrokerNamespaceIsolationData> GetBrokersWithNamespaceIsolationPolicy(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBrokersWithNamespaceIsolationPolicyAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get list of brokers with namespace-isolation policies attached to them.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
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
        public async ValueTask<IList<BrokerNamespaceIsolationData>> GetBrokersWithNamespaceIsolationPolicyAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/namespaceIsolationPolicies/brokers").ToString();
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
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
            IList<BrokerNamespaceIsolationData> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IList<BrokerNamespaceIsolationData>>(_responseContent);
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

        public BrokerNamespaceIsolationData GetBrokerWithNamespaceIsolationPolicy(string cluster, string broker, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBrokerWithNamespaceIsolationPolicyAsync(cluster, broker, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get a broker with namespace-isolation policies attached to it.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='broker'>
        /// The broker name (&lt;broker-hostname&gt;:&lt;web-service-port&gt;)
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
        public async ValueTask<BrokerNamespaceIsolationData> GetBrokerWithNamespaceIsolationPolicyAsync(string cluster, string broker, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (broker == null)
            {
                throw new InvalidOperationException("broker");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/namespaceIsolationPolicies/brokers/{broker}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
            _url = _url.Replace("{broker}", Uri.EscapeDataString(broker));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
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
            BrokerNamespaceIsolationData _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<BrokerNamespaceIsolationData>(_responseContent);
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

        public NamespaceIsolationData GetNamespaceIsolationPolicy(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceIsolationPolicyAsync(cluster, policyName, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the single namespace isolation policy assigned to the cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='policyName'>
        /// The name of the namespace isolation policy
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
        public async ValueTask<NamespaceIsolationData> GetNamespaceIsolationPolicyAsync(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (policyName == null)
            {
                throw new InvalidOperationException("policyName");
            }
           
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/namespaceIsolationPolicies/{policyName}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
            _url = _url.Replace("{policyName}", Uri.EscapeDataString(policyName));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
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
            NamespaceIsolationData _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<NamespaceIsolationData>(_responseContent);
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

        public string SetNamespaceIsolationPolicy(string cluster, string policyName, NamespaceIsolationData body, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetNamespaceIsolationPolicyAsync(cluster, policyName, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set namespace isolation policy.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='policyName'>
        /// The namespace isolation policy name
        /// </param>
        /// <param name='body'>
        /// The namespace isolation policy data
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
        public async ValueTask<string> SetNamespaceIsolationPolicyAsync(string cluster, string policyName, NamespaceIsolationData body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (policyName == null)
            {
                throw new InvalidOperationException("policyName");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/namespaceIsolationPolicies/{policyName}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
            _url = _url.Replace("{policyName}", Uri.EscapeDataString(policyName));
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
            
            System.Net.HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 400 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
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
        public string DeleteNamespaceIsolationPolicy(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteNamespaceIsolationPolicyAsync(cluster, policyName, customHeaders).GetAwaiter().GetResult();    
        }
        /// <summary>
        /// Delete namespace isolation policy.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='policyName'>
        /// The namespace isolation policy name
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
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async ValueTask<string> DeleteNamespaceIsolationPolicyAsync(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (policyName == null)
            {
                throw new InvalidOperationException("policyName");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/namespaceIsolationPolicies/{policyName}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
            _url = _url.Replace("{policyName}", Uri.EscapeDataString(policyName));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412 && (int)_statusCode != 500)
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

        public IList<string> GetPeerCluster(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPeerClusterAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the peer-cluster data for the specified cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
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
        public async ValueTask<IList<string>> GetPeerClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/peers").ToString();
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

        public string SetPeerClusterNames(string cluster, IList<string> body, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetPeerClusterNamesAsync(cluster, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update peer-cluster-list for a cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='body'>
        /// The list of peer cluster names
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
        public async ValueTask<string> SetPeerClusterNamesAsync(string cluster, IList<string> body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "clusters/{cluster}/peers").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
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
    }
}
