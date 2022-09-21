using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;
namespace SharpPulsar.Admin
{
    public class Namespaces
    {
        private Uri _uri;
        private HttpClient _httpClient;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public Namespaces(string brokerwebserviceurl, HttpClient httpClient)
        {
            _uri = new Uri($"{brokerwebserviceurl.TrimEnd('/')}/admin/v2");
            _httpClient = httpClient;
            if (_uri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
        }
        public IList<string> GetTenantNamespaces(string tenant, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTenantNamespacesAsync(tenant, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of all the namespaces for a certain tenant.
        /// </summary>
        /// <param name='tenant'>
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
        public async ValueTask<IList<string>> GetTenantNamespacesAsync(string tenant, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            
            // Construct URL
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
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

        public IList<string> GetTopics(string tenant, string namespaceParameter, string mode = default, bool? includeSystemTopic = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTopicsAsync(tenant, namespaceParameter, mode, includeSystemTopic, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get the list of all the topics under a certain namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='mode'>
        /// Possible values include: 'PERSISTENT', 'NON_PERSISTENT', 'ALL'
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
        public async ValueTask<IList<string>> GetTopicsAsync(string tenant, string namespaceParameter, string mode = default, bool? includeSystemTopic = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/topics").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (mode != null)
            {
                _queryParameters.Add(string.Format("mode={0}", Uri.EscapeDataString(JsonSerializer.Serialize(mode, _jsonSerializerOptions).Trim('"'))));
            }
            if (includeSystemTopic != null)
            {
                _queryParameters.Add(string.Format("includeSystemTopic={0}", Uri.EscapeDataString(JsonSerializer.Serialize(mode, _jsonSerializerOptions).Trim('"'))));
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

        public Policies GetPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();  
        }
        /// <summary>
        /// Get the dump all the policies specified for a namespace.
        /// </summary>
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
        public async ValueTask<Policies> GetPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            Policies _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<Policies>(_responseContent);
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

        public string CreateNamespace(string tenant, string namespaceParameter, Policies body = default, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateNamespaceAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();    
        }
        /// <summary>
        /// Creates a new namespace with the specified policies
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Policies for the namespace
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
        public async ValueTask<string> CreateNamespaceAsync(string tenant, string namespaceParameter, Policies body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (body != null)
            {
                body.Validate();
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
                _httpRequest.Content.Headers.ContentType = System.System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);

            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string DeleteNamespace(string tenant, string namespaceParameter, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteNamespaceAsync(tenant, namespaceParameter, force, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete a namespace and all the topics under it.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='force'>
        /// </param>
        /// <param name='authoritative'>
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
        public async ValueTask<string> DeleteNamespaceAsync(string tenant, string namespaceParameter, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (force != null)
            {
                _queryParameters.Add(string.Format("force={0}", Uri.EscapeDataString(JsonSerializer.Serialize(force, _jsonSerializerOptions).Trim('"'))));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 405 && (int)_statusCode != 409)
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

        public string DeleteNamespaceBundle(string tenant, string namespaceParameter, string bundle, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteNamespaceBundleAsync(tenant, namespaceParameter, bundle, force, authoritative, customHeaders).GetAwaiter().GetResult();    
        }
        /// <summary>
        /// Delete a namespace bundle and all the topics under it.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='force'>
        /// </param>
        /// <param name='authoritative'>
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
        public async ValueTask<string> DeleteNamespaceBundleAsync(string tenant, string namespaceParameter, string bundle, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", Uri.EscapeDataString(bundle));
            List<string> _queryParameters = new List<string>();
            if (force != null)
            {
                _queryParameters.Add(string.Format("force={0}", Uri.EscapeDataString(JsonSerializer.Serialize(force, _jsonSerializerOptions).Trim('"'))));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public IDictionary<string, IList<string>> GetPermissions(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null) 
        {
            return GetPermissionsAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Retrieve the permissions for a namespace.
        /// </summary>
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
        public async ValueTask<IDictionary<string, IList<string>>> GetPermissionsAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/permissions").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            IDictionary<string, IList<string>> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IDictionary<string, IList<string>>>(_responseContent);
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

        public IDictionary<string, IList<string>> GetPermissionOnSubscription(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPermissionOnSubscriptionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
            /// Retrieve the permissions for a namespace.
            /// </summary>
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
        public async ValueTask<IDictionary<string, IList<string>>> GetPermissionOnSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/permissions/subscription").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            IDictionary<string, IList<string>> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IDictionary<string, IList<string>>>(_responseContent);
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

        public string GrantPermissionOnNamespace(string tenant, string namespaceParameter, string role, IList<string> body = default, Dictionary<string, List<string>> customHeaders = null) 
        {
            return GrantPermissionOnNamespaceAsync(tenant, namespaceParameter, role, body, customHeaders).GetAwaiter().GetResult();   
        }
        /// <summary>
        /// Grant a new permission to a role on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='role'>
        /// </param>
        /// <param name='body'>
        /// List of permissions for the specified role
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
        public async ValueTask<string> GrantPermissionOnNamespaceAsync(string tenant, string namespaceParameter, string role, IList<string> body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (role == null)
            {
                throw new InvalidOperationException("role");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/permissions/{role}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{role}", Uri.EscapeDataString(role));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 501)
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

        public string GrantPermissionOnSubscription(string property, string namespaceParameter, string subscription, IList<string> body = default, Dictionary<string, List<string>> customHeaders = null) 
        { 
          return GrantPermissionOnSubscriptionAsync(property, namespaceParameter, subscription, body, customHeaders).GetAwaiter().GetResult();    
        }
        /// <summary>
        /// Grant a new permission to a role on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='role'>
        /// </param>
        /// <param name='body'>
        /// List of permissions for the specified role
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
        public async ValueTask<string> GrantPermissionOnSubscriptionAsync(string property, string namespaceParameter, string subscription, IList<string> body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (property == null)
            {
                throw new InvalidOperationException("property");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (subscription == null)
            {
                throw new InvalidOperationException("subscription");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/permissions/subscription/{subscription}").ToString();
            _url = _url.Replace("{property}", Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", Uri.EscapeDataString(subscription));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 501)
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

        public string RevokePermissionsOnNamespace(string property, string namespaceParameter, string subscription, Dictionary<string, List<string>> customHeaders = null)
        {
            return RevokePermissionsOnNamespaceAsync(property, namespaceParameter, subscription, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Revoke all permissions to a role on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='role'>
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
        public async ValueTask<string> RevokePermissionsOnNamespaceAsync(string property, string namespaceParameter, string subscription, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (property == null)
            {
                throw new InvalidOperationException("property");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (subscription == null)
            {
                throw new InvalidOperationException("subscription");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/permissions/{subscription}").ToString();
            _url = _url.Replace("{property}", Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", Uri.EscapeDataString(subscription));
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

        public string RevokePermissionOnSubscription(string property, string namespaceParameter, string subscription, string role, Dictionary<string, List<string>> customHeaders = null)
        {
            return RevokePermissionOnSubscriptionAsync(property, namespaceParameter, subscription, role, customHeaders).GetAwaiter().GetResult(); 
        }
        /// <summary>
        /// Revoke all permissions to a role on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='role'>
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
        public async ValueTask<string> RevokePermissionOnSubscriptionAsync(string property, string namespaceParameter, string subscription, string role, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (property == null)
            {
                throw new InvalidOperationException("property");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (subscription == null)
            {
                throw new InvalidOperationException("subscription");
            }
            if (role == null)
            {
                throw new InvalidOperationException("role");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/permissions/{subscription}/{role}").ToString();
            _url = _url.Replace("{property}", Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", Uri.EscapeDataString(subscription));
            _url = _url.Replace("{role}", Uri.EscapeDataString(role));
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

        public IList<string> GetNamespaceReplicationClusters(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceReplicationClustersAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the replication clusters for a namespace.
        /// </summary>
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
        public async ValueTask<IList<string>> GetNamespaceReplicationClustersAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/replication").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public string SetNamespaceReplicationClusters(string tenant, string namespaceParameter, IList<string> body, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetNamespaceReplicationClustersAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();    
        }
        /// <summary>
        /// Set the replication clusters for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// List of replication clusters
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
        public async ValueTask<string> SetNamespaceReplicationClustersAsync(string tenant, string namespaceParameter, IList<string> body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/replication").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public int? GetNamespaceMessageTTL(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceMessageTTLAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();   
        }
        /// <summary>
        /// Get the message TTL for the namespace
        /// </summary>
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
        public async ValueTask<int?> GetNamespaceMessageTTLAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/messageTTL").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            int? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<int?>(_responseContent);
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

        public string SetNamespaceMessageTTL(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetNamespaceMessageTTLAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set message TTL in seconds for namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// TTL in seconds for the specified namespace
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
        public async ValueTask<string> SetNamespaceMessageTTLAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/messageTTL").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
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

        public string RemoveNamespaceMessageTTL(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveNamespaceMessageTTLAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set message TTL in seconds for namespace
        /// </summary>
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
        public async ValueTask<string> RemoveNamespaceMessageTTLAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/messageTTL").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public int? GetSubscriptionExpirationTime(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionExpirationTimeAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the subscription expiration time for the namespace
        /// </summary>
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
        public async ValueTask<int?> GetSubscriptionExpirationTimeAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionExpirationTime").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            int? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<int?>(_responseContent);
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

        public string SetSubscriptionExpirationTime(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSubscriptionExpirationTimeAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set subscription expiration time in minutes for namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Expiration time in minutes for the specified namespace
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
        public async ValueTask<string> SetSubscriptionExpirationTimeAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionExpirationTime").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
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

        public string RemoveSubscriptionExpirationTime(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveSubscriptionExpirationTimeAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set subscription expiration time in minutes for namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Expiration time in minutes for the specified namespace
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
        public async ValueTask<string> RemoveSubscriptionExpirationTimeAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionExpirationTime").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public bool? GetDeduplication(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDeduplicationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult(); 
        }
        /// <summary>
        /// Get broker side deduplication for all topics in a namespace
        /// </summary>
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
        public async ValueTask<bool?> GetDeduplicationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/deduplication").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            bool? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<bool?>(_responseContent);
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

        public string ModifyDeduplication(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null)
        {
           return  ModifyDeduplicationAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Enable or disable broker side deduplication for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Flag for disabling or enabling broker side deduplication for all topics in
        /// the specified namespace
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
        public async ValueTask<string> ModifyDeduplicationAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/deduplication").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
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

        public string RemoveDeduplication(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveDeduplicationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove broker side deduplication for all topics in a namespace
        /// </summary>
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
        public async ValueTask<string> RemoveDeduplicationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/deduplication").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public string SetAutoTopicCreation(string tenant, string namespaceParameter, AutoTopicCreationOverride body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetAutoTopicCreationAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Override broker's allowAutoTopicCreation setting for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Settings for automatic topic creation
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
        public async ValueTask<string> SetAutoTopicCreationAsync(string tenant, string namespaceParameter, AutoTopicCreationOverride body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoTopicCreation").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 400 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 406)
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

        public string RemoveAutoTopicCreation(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveAutoTopicCreationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove override of broker's allowAutoTopicCreation in a namespace
        /// </summary>
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
        public async ValueTask<string> RemoveAutoTopicCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoTopicCreation").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public bool? GetAutoTopicCreation(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetAutoTopicCreationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove override of broker's allowAutoTopicCreation in a namespace
        /// </summary>
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
        public async ValueTask<bool?> GetAutoTopicCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoTopicCreation").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            bool? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<bool?>(_responseContent);
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

        public string SetAutoSubscriptionCreation(string tenant, string namespaceParameter, AutoSubscriptionCreationOverride body = default, Dictionary<string, List<string>> customHeaders = null)
        {
          return  SetAutoSubscriptionCreationAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Override broker's allowAutoSubscriptionCreation setting for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Settings for automatic subscription creation
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
        public async ValueTask<string> SetAutoSubscriptionCreationAsync(string tenant, string namespaceParameter, AutoSubscriptionCreationOverride body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoSubscriptionCreation").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 400 && (int)_statusCode != 403 && (int)_statusCode != 404)
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

        public string RemoveAutoSubscriptionCreation(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveAutoSubscriptionCreationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove override of broker's allowAutoSubscriptionCreation in a namespace
        /// </summary>
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
        public async ValueTask<string> RemoveAutoSubscriptionCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoSubscriptionCreation").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public bool? GetAutoSubscriptionCreation(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetAutoSubscriptionCreationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove override of broker's allowAutoSubscriptionCreation in a namespace
        /// </summary>
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
        public async ValueTask<bool?> GetAutoSubscriptionCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoSubscriptionCreation").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            bool? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<bool?>(_responseContent);
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

        public BundlesData GetBundlesData(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return  GetBundlesDataAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the bundles split data.
        /// </summary>
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
        public async ValueTask<BundlesData> GetBundlesDataAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/bundles").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            // Create Result
            BundlesData _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<BundlesData>(_responseContent);
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

        public string UnloadNamespace(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return UnloadNamespaceAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Unload namespace
        /// </summary>
        /// <remarks>
        /// Unload an active namespace from the current broker serving it. Performing
        /// this operation will let the brokerremoves all producers, consumers, and
        /// connections using this namespace, and close all topics (includingtheir
        /// persistent store). During that operation, the namespace is marked as
        /// tentatively unavailable until thebroker completes the unloading action.
        /// This operation requires strictly super user privileges, since it
        /// wouldresult in non-persistent message loss and unexpected connection
        /// closure to the clients.
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
        public async ValueTask<string> UnloadNamespaceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/unload").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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

        public string UnloadNamespaceBundle(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return UnloadNamespaceBundleAsync(tenant, namespaceParameter, bundle, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Unload a namespace bundle
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
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
        public async ValueTask<string> UnloadNamespaceBundleAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/unload").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", Uri.EscapeDataString(bundle));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 403)
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

        public string SplitNamespaceBundle(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, bool? unload = false, string splitAlgorithmName = default, IList<long> body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SplitNamespaceBundleAsync(tenant, namespaceParameter, bundle, authoritative, unload, splitAlgorithmName, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Split a namespace bundle
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        /// <param name='unload'>
        /// </param>
        /// <param name='splitAlgorithmName'>
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
        public async ValueTask<string> SplitNamespaceBundleAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, bool? unload = false, string splitAlgorithmName = default, IList<long> body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/split").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", Uri.EscapeDataString(bundle));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", Uri.EscapeDataString(JsonSerializer.Serialize(authoritative, _jsonSerializerOptions).Trim('"'))));
            }
            if (unload != null)
            {
                _queryParameters.Add(string.Format("unload={0}", Uri.EscapeDataString(JsonSerializer.Serialize(unload, _jsonSerializerOptions).Trim('"'))));
            }
            if (splitAlgorithmName != null)
            {
                _queryParameters.Add(string.Format("splitAlgorithmName={0}", Uri.EscapeDataString(splitAlgorithmName)));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 403)
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

        public TopicHashPositions GetTopicHashPositions(string tenant, string namespaceParameter, string bundle, List<string> topics = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetTopicHashPositionsAsync(tenant, namespaceParameter, bundle, topics, customHeaders).GetAwaiter().GetResult();
        }
        public async ValueTask<TopicHashPositions> GetTopicHashPositionsAsync(string tenant, string namespaceParameter, string bundle, List<string> topics = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/topicHashPositions").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", Uri.EscapeDataString(bundle));
            List<string> _queryParameters = new List<string>();
            if (topics != null)
            {
                _queryParameters.Add(string.Format("topics={0}", Uri.EscapeDataString(JsonSerializer.Serialize(topics, _jsonSerializerOptions).Trim('"'))));
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
            TopicHashPositions _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<TopicHashPositions>(_responseContent);
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

        public PublishRate GetPublishRate(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetPublishRateAsync(property, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get publish rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
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
        public async ValueTask<PublishRate> GetPublishRateAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (property == null)
            {
                throw new InvalidOperationException("property");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/publishRate").ToString();
            _url = _url.Replace("{property}", Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 405 && (int)_statusCode != 409)
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
            PublishRate _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<PublishRate>(_responseContent);
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

        public string SetPublishRate(string property, string namespaceParameter, PublishRate body = default, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetPublishRateAsync(property, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set message publish rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='body'>
        /// Dispatch rate for the specified topic
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
        public async ValueTask<string> SetPublishRateAsync(string property, string namespaceParameter, PublishRate body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (property == null)
            {
                throw new InvalidOperationException("property");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/publishRate").ToString();
            _url = _url.Replace("{property}", Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 405 && (int)_statusCode != 409)
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

        public string RemovePublishRate(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemovePublishRateAsync(property, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove message publish rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
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
        public async ValueTask<string> RemovePublishRateAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (property == null)
            {
                throw new InvalidOperationException("property");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/publishRate").ToString();
            _url = _url.Replace("{property}", Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 405 && (int)_statusCode != 409)
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

        public DispatchRate GetDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get dispatch-rate configured for the namespace, -1 represents not
        /// configured yet
        /// </summary>
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
        public async ValueTask<DispatchRate> GetDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/dispatchRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            DispatchRate _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<DispatchRate>(_responseContent);
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

        public string SetDispatchRate(string tenant, string namespaceParameter, DispatchRate body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetDispatchRateAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Dispatch rate for all topics of the specified namespace
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
        public async ValueTask<string> SetDispatchRateAsync(string tenant, string namespaceParameter, DispatchRate body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/dispatchRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            return _httpResponse.ReasonPhrase;
        }
        public string DeleteDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return DeleteDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Delete dispatch-rate throttling for all topics of the namespace
        /// </summary>
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
        public async ValueTask<string> DeleteDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/dispatchRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            return _httpResponse.ReasonPhrase;
        }

        public DispatchRate GetSubscriptionDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetSubscriptionDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get Subscription dispatch-rate configured for the namespace, -1 represents
        /// not configured yet
        /// </summary>
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
        public async ValueTask<DispatchRate> GetSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionDispatchRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            DispatchRate _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<DispatchRate>(_responseContent);
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

        public string SetSubscriptionDispatchRate(string tenant, string namespaceParameter, DispatchRate body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetSubscriptionDispatchRateAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set Subscription dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Subscription dispatch rate for all topics of the specified namespace
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
        public async ValueTask<string> SetSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, DispatchRate body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionDispatchRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            return _httpResponse.ReasonPhrase;
        }

        public string DeleteSubscriptionDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteSubscriptionDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete Subscription dispatch-rate throttling for all topics of the
        /// namespace
        /// </summary>
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
        public async ValueTask<string> DeleteSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionDispatchRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            return _httpResponse.ReasonPhrase;
        }

        public SubscribeRate GetSubscribeRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetSubscribeRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get subscribe-rate configured for the namespace
        /// </summary>
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
        public async ValueTask<SubscribeRate> GetSubscribeRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscribeRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            SubscribeRate _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<SubscribeRate>(_responseContent);
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

        public string SetSubscribeRate(string tenant, string namespaceParameter, SubscribeRate body = default, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSubscribeRateAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set subscribe-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Subscribe rate for all topics of the specified namespace
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
        public async ValueTask<string> SetSubscribeRateAsync(string tenant, string namespaceParameter, SubscribeRate body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscribeRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
           
            return _httpResponse.ReasonPhrase;
        }

        public string DeleteSubscribeRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return DeleteSubscribeRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete subscribe-rate throttling for all topics of the namespace
        /// </summary>
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
        public async ValueTask<string> DeleteSubscribeRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscribeRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            return _httpResponse.ReasonPhrase;
        }

        public DispatchRate GetReplicatorDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetReplicatorDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get replicator dispatch-rate configured for the namespace, -1 represents
        /// not configured yet
        /// </summary>
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
        public async ValueTask<DispatchRate> GetReplicatorDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/replicatorDispatchRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            DispatchRate _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<DispatchRate>(_responseContent);
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

        public string SetReplicatorDispatchRate(string tenant, string namespaceParameter, DispatchRate body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetReplicatorDispatchRateAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set replicator dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Replicator dispatch rate for all topics of the specified namespace
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
        public async ValueTask<string> SetReplicatorDispatchRateAsync(string tenant, string namespaceParameter, DispatchRate body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/replicatorDispatchRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
           
            return _httpResponse.ReasonPhrase;
        }

        public string RemoveReplicatorDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveReplicatorDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove replicator dispatch-rate throttling for all topics of the namespace
        /// </summary>
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
        public async ValueTask<string> RemoveReplicatorDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/replicatorDispatchRate").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            return _httpResponse.ReasonPhrase;
        }

        public string SetBacklogQuota(string tenant, string namespaceParameter, string backlogQuotaType = default, BacklogQuota body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetBacklogQuotaAsync(tenant, namespaceParameter, backlogQuotaType, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set a backlog quota for all the topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='backlogQuotaType'>
        /// Possible values include: 'destination_storage', 'message_age'
        /// </param>
        /// <param name='body'>
        /// Backlog quota for all topics of the specified namespace
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
        public async ValueTask<string> SetBacklogQuotaAsync(string tenant, string namespaceParameter, string backlogQuotaType = default, BacklogQuota body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/backlogQuota").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (backlogQuotaType != null)
            {
                _queryParameters.Add(string.Format("backlogQuotaType={0}", Uri.EscapeDataString(JsonSerializer.Serialize(backlogQuotaType, _jsonSerializerOptions).Trim('"'))));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string RemoveBacklogQuota(string tenant, string namespaceParameter, string backlogQuotaType = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return  RemoveBacklogQuotaAsync(tenant, namespaceParameter, backlogQuotaType, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove a backlog quota policy from a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='backlogQuotaType'>
        /// Possible values include: 'destination_storage', 'message_age'
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
        public async ValueTask<string> RemoveBacklogQuotaAsync(string tenant, string namespaceParameter, string backlogQuotaType = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/backlogQuota").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (backlogQuotaType != null)
            {
                _queryParameters.Add(string.Format("backlogQuotaType={0}", Uri.EscapeDataString(JsonSerializer.Serialize(backlogQuotaType, _jsonSerializerOptions).Trim('"'))));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
        public IDictionary<string, BacklogQuota> GetBacklogQuotaMap(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBacklogQuotaMapAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get backlog quota map on a namespace.
        /// </summary>
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
        public async ValueTask<IDictionary<string, BacklogQuota>> GetBacklogQuotaMapAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/backlogQuotaMap").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            IDictionary<string, BacklogQuota> _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<IDictionary<string, BacklogQuota>>(_responseContent);
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

        public RetentionPolicies GetRetention(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetRetentionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get retention config on a namespace.
        /// </summary>
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
        public async ValueTask<RetentionPolicies> GetRetentionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/retention").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            RetentionPolicies _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<RetentionPolicies>(_responseContent);
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

        public string SetRetention(string tenant, string namespaceParameter, RetentionPolicies body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetRetentionAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set retention configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Retention policies for the specified namespace
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
        public async ValueTask<string> SetRetentionAsync(string tenant, string namespaceParameter, RetentionPolicies body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/retention").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string RemoveRetention(string tenant, string namespaceParameter, RetentionPolicies body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveRetentionAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove retention configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Retention policies for the specified namespace
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
        public async ValueTask<string> RemoveRetentionAsync(string tenant, string namespaceParameter, RetentionPolicies body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/retention").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string SetPersistence(string tenant, string namespaceParameter, PersistencePolicies body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetPersistenceAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set the persistence configuration for all the topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Persistence policies for the specified namespace
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
        public async ValueTask<string> SetPersistenceAsync(string tenant, string namespaceParameter, PersistencePolicies body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/persistence").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 400 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public string DeletePersistence(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return DeletePersistenceAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete the persistence configuration for all topics on a namespace
        /// </summary>
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
        public async ValueTask<string> DeletePersistenceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/persistence").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            return _httpResponse.ReasonPhrase;
        }

        public BookieAffinityGroupData GetBookieAffinityGroup(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetBookieAffinityGroupAsync(property, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the bookie-affinity-group from namespace-local policy.
        /// </summary>
        /// <param name='property'>
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
        public async ValueTask<BookieAffinityGroupData> GetBookieAffinityGroupAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (property == null)
            {
                throw new InvalidOperationException("property");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/persistence/bookieAffinity").ToString();
            _url = _url.Replace("{property}", Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            BookieAffinityGroupData _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<BookieAffinityGroupData>(_responseContent);
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

        public string DeleteBookieAffinityGroup(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return DeleteBookieAffinityGroupAsync(property, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete the bookie-affinity-group from namespace-local policy.
        /// </summary>
        /// <param name='property'>
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
        public async ValueTask<string> DeleteBookieAffinityGroupAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (property == null)
            {
                throw new InvalidOperationException("property");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/persistence/bookieAffinity").ToString();
            _url = _url.Replace("{property}", Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public string SetBookieAffinityGroup(string tenant, string namespaceParameter, BookieAffinityGroupData body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetBookieAffinityGroupAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set the bookie-affinity-group to namespace-persistent policy.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Bookie affinity group for the specified namespace
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
        public async ValueTask<string> SetBookieAffinityGroupAsync(string tenant, string namespaceParameter, BookieAffinityGroupData body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/persistence/bookieAffinity").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public PersistencePolicies GetPersistence(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetPersistenceAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the persistence configuration for a namespace.
        /// </summary>
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
        public async ValueTask<PersistencePolicies> GetPersistenceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/persistence").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            PersistencePolicies _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<PersistencePolicies>(_responseContent);
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

        public string ClearNamespaceBacklog(string tenant, string namespaceParameter, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return ClearNamespaceBacklogAsync(tenant, namespaceParameter, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear backlog for all topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='authoritative'>
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
        public async ValueTask<string> ClearNamespaceBacklogAsync(string tenant, string namespaceParameter, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/clearBacklog").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public string ClearNamespaceBacklogForSubscription(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return ClearNamespaceBacklogForSubscriptionAsync(tenant, namespaceParameter, subscription, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear backlog for a given subscription on all topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='authoritative'>
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
        public async ValueTask<string> ClearNamespaceBacklogForSubscriptionAsync(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (subscription == null)
            {
                throw new InvalidOperationException("subscription");
            }
           
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/clearBacklog/{subscription}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", Uri.EscapeDataString(subscription));
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

        public string ClearNamespaceBundleBacklog(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return ClearNamespaceBundleBacklogAsync(tenant, namespaceParameter, bundle, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear backlog for all topics on a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
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
        public async ValueTask<string> ClearNamespaceBundleBacklogAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/clearBacklog").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", Uri.EscapeDataString(bundle));
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
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
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
            
            return _httpResponse.ReasonPhrase;
        }

        public string ClearNamespaceBundleBacklogForSubscription(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return ClearNamespaceBundleBacklogForSubscriptionAsync(tenant, namespaceParameter, subscription, bundle, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear backlog for a given subscription on all topics on a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
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
        public async ValueTask<string> ClearNamespaceBundleBacklogForSubscriptionAsync(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (subscription == null)
            {
                throw new InvalidOperationException("subscription");
            }
            if (bundle == null)
            {
                throw new InvalidOperationException("bundle");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/clearBacklog/{subscription}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", Uri.EscapeDataString(subscription));
            _url = _url.Replace("{bundle}", Uri.EscapeDataString(bundle));
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
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
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
            
            return _httpResponse.ReasonPhrase;
        }

        public string UnsubscribeNamespace(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return UnsubscribeNamespaceAsync(tenant, namespaceParameter, subscription, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Unsubscribes the given subscription on all topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='authoritative'>
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
        public async ValueTask<string> UnsubscribeNamespaceAsync(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (subscription == null)
            {
                throw new InvalidOperationException("subscription");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/unsubscribe/{subscription}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", Uri.EscapeDataString(subscription));
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
        public string UnsubscribeNamespaceBundle(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
           return UnsubscribeNamespaceBundleAsync(tenant, namespaceParameter, subscription, bundle, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Unsubscribes the given subscription on all topics on a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
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
        public async ValueTask<string> UnsubscribeNamespaceBundleAsync(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (subscription == null)
            {
                throw new InvalidOperationException("subscription");
            }
            if (bundle == null)
            {
                throw new InvalidOperationException("bundle");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/unsubscribe/{subscription}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", Uri.EscapeDataString(subscription));
            _url = _url.Replace("{bundle}", Uri.EscapeDataString(bundle));
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

        public string SetSubscriptionAuthMode(string tenant, string namespaceParameter, string body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetSubscriptionAuthModeAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set a subscription auth mode for all the topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Subscription auth mode for all topics of the specified namespace
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
        public async ValueTask<string> SetSubscriptionAuthModeAsync(string tenant, string namespaceParameter, string body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionAuthMode").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public SubscriptionAuthMode GetSubscriptionAuthMode(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionAuthModeAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set a subscription auth mode for all the topics on a namespace.
        /// </summary>
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
        public async ValueTask<SubscriptionAuthMode> GetSubscriptionAuthModeAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionAuthMode").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            SubscriptionAuthMode? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<SubscriptionAuthMode>(_responseContent);
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
            return _result.Value;
        }

        public string ModifyEncryptionRequired(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null)
        {
           return ModifyEncryptionRequiredAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Message encryption is required or not for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Flag defining if message encryption is required
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
        public async ValueTask<string> ModifyEncryptionRequiredAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/encryptionRequired").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
        public bool? GetEncryptionRequired(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetEncryptionRequiredAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }

        public async ValueTask<bool?> GetEncryptionRequiredAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/encryptionRequired").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            bool? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<bool?>(_responseContent);
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

        public DelayedDeliveryPolicies GetDelayedDeliveryPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get delayed delivery messages config on a namespace.
        /// </summary>
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
        public async ValueTask<DelayedDeliveryPolicies> GetDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/delayedDelivery").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            DelayedDeliveryPolicies _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<DelayedDeliveryPolicies>(_responseContent);
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

        public string SetDelayedDeliveryPolicies(string tenant, string namespaceParameter, DelayedDeliveryPolicies body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set delayed delivery messages config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Delayed delivery policies for the specified namespace
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
        public async ValueTask<string> SetDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, DelayedDeliveryPolicies body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/delayedDelivery").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public string RemoveDelayedDeliveryPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete delayed delivery messages config on a namespace.
        /// </summary>
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
        public async ValueTask<string> RemoveDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/delayedDelivery").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public InactiveTopicPolicies GetInactiveTopicPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetInactiveTopicPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get inactive topic policies config on a namespace.
        /// </summary>
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
        public async ValueTask<InactiveTopicPolicies> GetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/inactiveTopicPolicies").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            InactiveTopicPolicies _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<InactiveTopicPolicies>(_responseContent);
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

        public string SetInactiveTopicPolicies(string tenant, string namespaceParameter, InactiveTopicPolicies body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetInactiveTopicPoliciesAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set inactive topic policies config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Inactive topic policies for the specified namespace
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
        public async ValueTask<string> SetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, InactiveTopicPolicies body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/inactiveTopicPolicies").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public string RemoveInactiveTopicPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return  RemoveInactiveTopicPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove inactive topic policies from a namespace.
        /// </summary>
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
        public async ValueTask<string> RemoveInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/inactiveTopicPolicies").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public int? GetMaxProducersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxProducersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxProducersPerTopic config on a namespace.
        /// </summary>
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
        public async ValueTask<int?> GetMaxProducersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxProducersPerTopic").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            int? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<int?>(_responseContent);
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

        public string SetMaxProducersPerTopic(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetMaxProducersPerTopicAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxProducersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Number of maximum producers per topic
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
        public async ValueTask<string> SetMaxProducersPerTopicAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxProducersPerTopic").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string RemoveMaxProducersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveMaxProducersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxProducersPerTopic configuration on a namespace.
        /// </summary>
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
        public async ValueTask<string> RemoveMaxProducersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxProducersPerTopic").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public int? GetDeduplicationSnapshotInterval(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetDeduplicationSnapshotIntervalAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get deduplicationSnapshotInterval config on a namespace.
        /// </summary>
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
        public async ValueTask<int?> GetDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/deduplicationSnapshotInterval").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            int? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<int?>(_responseContent);
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

        public string SetDeduplicationSnapshotInterval(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null)
        {
           return  SetDeduplicationSnapshotIntervalAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set deduplicationSnapshotInterval config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Interval to take deduplication snapshot per topic
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
        public async ValueTask<string> SetDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/deduplicationSnapshotInterval").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
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

        public int? GetMaxConsumersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return  GetMaxConsumersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxConsumersPerTopic config on a namespace.
        /// </summary>
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
        public async ValueTask<int?> GetMaxConsumersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerTopic").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            int? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<int?>(_responseContent);
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

        public string SetMaxConsumersPerTopic(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetMaxConsumersPerTopicAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxConsumersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Number of maximum consumers per topic
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
        public async ValueTask<string> SetMaxConsumersPerTopicAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerTopic").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string RemoveMaxConsumersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveMaxConsumersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxConsumersPerTopic configuration on a namespace.
        /// </summary>
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
        public async ValueTask<string> RemoveMaxConsumersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerTopic").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public int? GetMaxConsumersPerSubscription(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxConsumersPerSubscription config on a namespace.
        /// </summary>
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
        public async ValueTask<int?> GetMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerSubscription").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            int? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<int?>(_responseContent);
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

        public string SetMaxConsumersPerSubscription(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxConsumersPerSubscription configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Number of maximum consumers per subscription
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
        public async ValueTask<string> SetMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerSubscription").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string RemoveMaxConsumersPerSubscription(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxConsumersPerSubscription configuration on a namespace.
        /// </summary>
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
        public async ValueTask<string> RemoveMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerSubscription").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public int? GetMaxUnackedMessagesPerConsumer(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetMaxUnackedMessagesPerConsumerAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxUnackedMessagesPerConsumer config on a namespace.
        /// </summary>
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
        public async ValueTask<int?> GetMaxUnackedMessagesPerConsumerAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerConsumer").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            int? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<int?>(_responseContent);
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

        public string SetMaxUnackedMessagesPerConsumer(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetMaxUnackedMessagesPerConsumerAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxConsumersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Number of maximum unacked messages per consumer
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
        public async ValueTask<string> SetMaxUnackedMessagesPerConsumerAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerConsumer").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
           
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string RemoveMaxUnackedmessagesPerConsumer(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxUnackedmessagesPerConsumerAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxUnackedMessagesPerConsumer config on a namespace.
        /// </summary>
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
        public async ValueTask<string> RemoveMaxUnackedmessagesPerConsumerAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerConsumer").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public int? GetMaxUnackedmessagesPerSubscription(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetMaxUnackedmessagesPerSubscriptionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxUnackedMessagesPerSubscription config on a namespace.
        /// </summary>
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
        public async ValueTask<int?> GetMaxUnackedmessagesPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerSubscription").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            int? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<int?>(_responseContent);
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

        public string SetMaxUnackedMessagesPerSubscription(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetMaxUnackedMessagesPerSubscriptionAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxUnackedMessagesPerSubscription configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Number of maximum unacked messages per subscription
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
        public async ValueTask<string> SetMaxUnackedMessagesPerSubscriptionAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerSubscription").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string RemoveMaxUnackedmessagesPerSubscriptionMessages(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveMaxUnackedmessagesPerSubscriptionMessagesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxUnackedMessagesPerSubscription config on a namespace.
        /// </summary>
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
        public async ValueTask<string> RemoveMaxUnackedmessagesPerSubscriptionMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerSubscription").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public int? GetMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxSubscriptionsPerTopic config on a namespace.
        /// </summary>
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
        public async ValueTask<int?> GetMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxSubscriptionsPerTopic").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            int? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<int?>(_responseContent);
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

        public string SetMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxSubscriptionsPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Number of maximum subscriptions per topic
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
        public async ValueTask<string> SetMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxSubscriptionsPerTopic").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string RemoveMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxSubscriptionsPerTopic configuration on a namespace.
        /// </summary>
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
        public async ValueTask<string> RemoveMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxSubscriptionsPerTopic").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public string GetNamespaceAntiAffinityGroup(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetNamespaceAntiAffinityGroupAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get anti-affinity group of a namespace.
        /// </summary>
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
        public async ValueTask<string> GetNamespaceAntiAffinityGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/antiAffinity").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public string SetNamespaceAntiAffinityGroup(string tenant, string namespaceParameter, string body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetNamespaceAntiAffinityGroupAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set anti-affinity group for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Anti-affinity group for the specified namespace
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
        public async ValueTask<string> SetNamespaceAntiAffinityGroupAsync(string tenant, string namespaceParameter, string body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/antiAffinity").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public string RemoveNamespaceAntiAffinityGroup(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveNamespaceAntiAffinityGroupAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove anti-affinity group of a namespace.
        /// </summary>
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
        public async ValueTask<string> RemoveNamespaceAntiAffinityGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/antiAffinity").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public IList<string> GetAntiAffinityNamespaces(string cluster, string group, string tenant = default, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAntiAffinityNamespacesAsync(cluster, group, tenant, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get all namespaces that are grouped by given anti-affinity group in a given
        /// cluster. api can be only accessed by admin of any of the existing tenant
        /// </summary>
        /// <param name='cluster'>
        /// </param>
        /// <param name='group'>
        /// </param>
        /// <param name='tenant'>
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
        public async ValueTask<IList<string>> GetAntiAffinityNamespacesAsync(string cluster, string group, string tenant = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (cluster == null)
            {
                throw new InvalidOperationException("cluster");
            }
            if (group == null)
            {
                throw new InvalidOperationException("group");
            }
           
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{cluster}/antiAffinity/{group}").ToString();
            _url = _url.Replace("{cluster}", Uri.EscapeDataString(cluster));
            _url = _url.Replace("{group}", Uri.EscapeDataString(group));
            List<string> _queryParameters = new List<string>();
            if (tenant != null)
            {
                _queryParameters.Add(string.Format("tenant={0}", Uri.EscapeDataString(tenant)));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 412)
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

        public long? GetCompactionThreshold(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetCompactionThresholdAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Maximum number of uncompacted bytes in topics before compaction is
        /// triggered.
        /// </summary>
        /// <remarks>
        /// The backlog size is compared to the threshold periodically. A threshold of
        /// 0 disabled automatic compaction
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
        public async ValueTask<long?> GetCompactionThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/compactionThreshold").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            long? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<long?>(_responseContent);
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

        public string SetCompactionThreshold(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetCompactionThresholdAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maximum number of uncompacted bytes in a topic before compaction is
        /// triggered.
        /// </summary>
        /// <remarks>
        /// The backlog size is compared to the threshold periodically. A threshold of
        /// 0 disabled automatic compaction
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Maximum number of uncompacted bytes in a topic of the specified namespace
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
        public async ValueTask<string> SetCompactionThresholdAsync(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/compactionThreshold").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string DeleteCompactionThreshold(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteCompactionThresholdAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete maximum number of uncompacted bytes in a topic before compaction is
        /// triggered.
        /// </summary>
        /// <remarks>
        /// The backlog size is compared to the threshold periodically. A threshold of
        /// 0 disabled automatic compaction
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
        public async ValueTask<string> DeleteCompactionThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/compactionThreshold").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public long? GetOffloadThreshold(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadThresholdAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Maximum number of bytes stored on the pulsar cluster for a topic, before
        /// the broker will start offloading to longterm storage
        /// </summary>
        /// <remarks>
        /// A negative value disables automatic offloading
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
        public async ValueTask<long?> GetOffloadThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadThreshold").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            long? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<long?>(_responseContent);
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

        public string SetOffloadThreshold(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null)
        {
           return  SetOffloadThresholdAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maximum number of bytes stored on the pulsar cluster for a topic,
        /// before the broker will start offloading to longterm storage
        /// </summary>
        /// <remarks>
        /// -1 will revert to using the cluster default. A negative value disables
        /// automatic offloading.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Maximum number of bytes stored on the pulsar cluster for a topic of the
        /// specified namespace
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
        public async ValueTask<string> SetOffloadThresholdAsync(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadThreshold").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public long? GetOffloadDeletionLag(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadDeletionLagAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Number of milliseconds to wait before deleting a ledger segment which has
        /// been offloaded from the Pulsar cluster's local storage (i.e. BookKeeper)
        /// </summary>
        /// <remarks>
        /// A negative value denotes that deletion has been completely disabled. 'null'
        /// denotes that the topics in the namespace will fall back to the broker
        /// default for deletion lag.
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
        public async ValueTask<long?> GetOffloadDeletionLagAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadDeletionLagMs").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            long? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<long?>(_responseContent);
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

        public string SetOffloadDeletionLag(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetOffloadDeletionLagAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set number of milliseconds to wait before deleting a ledger segment which
        /// has been offloaded from the Pulsar cluster's local storage (i.e.
        /// BookKeeper)
        /// </summary>
        /// <remarks>
        /// A negative value disables the deletion completely.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// New number of milliseconds to wait before deleting a ledger segment which
        /// has been offloaded
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
        public async ValueTask<string> SetOffloadDeletionLagAsync(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadDeletionLagMs").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string ClearOffloadDeletionLag(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearOffloadDeletionLagAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear the namespace configured offload deletion lag. The topics in the
        /// namespace will fallback to using the default configured deletion lag for
        /// the broker
        /// </summary>
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
        public async ValueTask<string> ClearOffloadDeletionLagAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadDeletionLagMs").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public SchemaAutoUpdateCompatibilityStrategy GetSchemaAutoUpdateCompatibilityStrategy(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaAutoUpdateCompatibilityStrategyAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// The strategy used to check the compatibility of new schemas, provided by
        /// producers, before automatically updating the schema
        /// </summary>
        /// <remarks>
        /// The value AutoUpdateDisabled prevents producers from updating the schema.
        /// If set to AutoUpdateDisabled, schemas must be updated through the REST api
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
        public async ValueTask<SchemaAutoUpdateCompatibilityStrategy> GetSchemaAutoUpdateCompatibilityStrategyAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaAutoUpdateCompatibilityStrategy").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            SchemaAutoUpdateCompatibilityStrategy? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<SchemaAutoUpdateCompatibilityStrategy?>(_responseContent);
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
            
            return _result.Value;
        }

        public string SetSchemaAutoUpdateCompatibilityStrategy(string tenant, string namespaceParameter, SchemaAutoUpdateCompatibilityStrategy body = default, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetSchemaAutoUpdateCompatibilityStrategyAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update the strategy used to check the compatibility of new schemas,
        /// provided by producers, before automatically updating the schema
        /// </summary>
        /// <remarks>
        /// The value AutoUpdateDisabled prevents producers from updating the schema.
        /// If set to AutoUpdateDisabled, schemas must be updated through the REST api
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Strategy used to check the compatibility of new schemas
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
        public async ValueTask<string> SetSchemaAutoUpdateCompatibilityStrategyAsync(string tenant, string namespaceParameter, SchemaAutoUpdateCompatibilityStrategy body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaAutoUpdateCompatibilityStrategy").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public string GetSchemaCompatibilityStrategy(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaCompatibilityStrategyAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// The strategy of the namespace schema compatibility
        /// </summary>
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
        public async ValueTask<string> GetSchemaCompatibilityStrategyAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaCompatibilityStrategy").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public string SetSchemaCompatibilityStrategy(string tenant, string namespaceParameter, string body = default, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSchemaCompatibilityStrategyAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update the strategy used to check the compatibility of new schema
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Strategy used to check the compatibility of new schema
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
        public async ValueTask<string> SetSchemaCompatibilityStrategyAsync(string tenant, string namespaceParameter, string body = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaCompatibilityStrategy").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public bool? GetIsAllowAutoUpdateSchema(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetIsAllowAutoUpdateSchemaAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// The flag of whether allow auto update schema
        /// </summary>
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
        public async ValueTask<bool?> GetIsAllowAutoUpdateSchemaAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/isAllowAutoUpdateSchema").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            bool? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<bool?>(_responseContent);
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

        public string SetIsAllowAutoUpdateSchema(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetIsAllowAutoUpdateSchemaAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update flag of whether allow auto update schema
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Flag of whether to allow auto update schema
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
        public async ValueTask<string> SetIsAllowAutoUpdateSchemaAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/isAllowAutoUpdateSchema").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await _httpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public IList<string> GetSubscriptionTypesEnabled(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionTypesEnabledAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// The set of whether allow subscription types
        /// </summary>
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
        public async ValueTask<IList<string>> GetSubscriptionTypesEnabledAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionTypesEnabled").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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

        public string SetSubscriptionTypesEnabled(string tenant, string namespaceParameter, IList<string> body, Dictionary<string, List<string>> customHeaders = null)
        {
           return  SetSubscriptionTypesEnabledAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update set of whether allow share sub type
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Set of whether allow subscription types
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
        public async ValueTask<string> SetSubscriptionTypesEnabledAsync(string tenant, string namespaceParameter, IList<string> body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionTypesEnabled").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
        public string RemoveSubscriptionTypesEnabled(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return  RemoveSubscriptionTypesEnabledAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }

        public async ValueTask<string> RemoveSubscriptionTypesEnabledAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionTypesEnabled").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public bool? GetSchemaValidtionEnforced(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetSchemaValidtionEnforcedAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get schema validation enforced flag for namespace.
        /// </summary>
        /// <remarks>
        /// If the flag is set to true, when a producer without a schema attempts to
        /// produce to a topic with schema in this namespace, the producer will be
        /// failed to connect. PLEASE be carefully on using this, since non-java
        /// clients don't support schema.if you enable this setting, it will cause
        /// non-java clients failed to produce.
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
        public async ValueTask<bool?> GetSchemaValidtionEnforcedAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaValidationEnforced").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            bool? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<bool?>(_responseContent);
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

        public string SetSchemaValidtionEnforced(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSchemaValidtionEnforcedAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set schema validation enforced flag on namespace.
        /// </summary>
        /// <remarks>
        /// If the flag is set to true, when a producer without a schema attempts to
        /// produce to a topic with schema in this namespace, the producer will be
        /// failed to connect. PLEASE be carefully on using this, since non-java
        /// clients don't support schema.if you enable this setting, it will cause
        /// non-java clients failed to produce.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Flag of whether validation is enforced on the specified namespace
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
        public async ValueTask<string> SetSchemaValidtionEnforcedAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaValidationEnforced").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            
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

        public OffloadPolicies GetOffloadPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get offload configuration on a namespace.
        /// </summary>
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
        public async ValueTask<OffloadPolicies> GetOffloadPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadPolicies").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            OffloadPolicies? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<OffloadPolicies>(_responseContent);
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

        public string SetOffloadPolicies(string tenant, string namespaceParameter, OffloadPolicies body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetOffloadPoliciesAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set offload configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Offload policies for the specified namespace
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
        public async ValueTask<string> SetOffloadPoliciesAsync(string tenant, string namespaceParameter, OffloadPolicies body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadPolicies").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public string RemoveOffloadPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveOffloadPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set offload configuration on a namespace.
        /// </summary>
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
        public async ValueTask<string> RemoveOffloadPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/removeOffloadPolicies").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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

        public int? GetMaxTopicsPerNamespace(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetMaxTopicsPerNamespaceAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxTopicsPerNamespace config on a namespace.
        /// </summary>
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
        public async ValueTask<int?> GetMaxTopicsPerNamespaceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxTopicsPerNamespace").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            int? _result = null;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result = JsonSerializer.Deserialize<int?>(_responseContent);
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

        public string SetInactiveTopicPolicies(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetInactiveTopicPoliciesAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxTopicsPerNamespace config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Number of maximum topics for specific namespace
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
        public async ValueTask<string> SetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxTopicsPerNamespace").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = JsonSerializer.Serialize(body, _jsonSerializerOptions);
            _httpRequest.Content = new StringContent(_requestContent, Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            // Send Request
            
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

        public string SetInactiveTopicPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetInactiveTopicPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxTopicsPerNamespace config on a namespace.
        /// </summary>
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
        public async ValueTask<string> SetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxTopicsPerNamespace").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
        public string GetProperties(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPropertiesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get key value pair properties for a given namespace.
        /// </summary>
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
        public async ValueTask<string> GetPropertiesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/properties").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            
            return _httpResponse.ReasonPhrase;
        }
        public string SetProperties(string tenant, string namespaceParameter, IDictionary<string, string> body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetPropertiesAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Put key value pairs property on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Key value pair properties for the namespace
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
        public async ValueTask<string> SetPropertiesAsync(string tenant, string namespaceParameter, IDictionary<string, string> body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/properties").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public string ClearProperties(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return  ClearPropertiesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get property value for a given key on a namespace.
        /// </summary>
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
        public async ValueTask<string> ClearPropertiesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/properties").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
        public string GetProperty(string tenant, string namespaceParameter, string key, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetPropertyAsync(tenant, namespaceParameter, key, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get property value for a given key on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='key'>
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
        public async ValueTask<string> GetPropertyAsync(string tenant, string namespaceParameter, string key, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (key == null)
            {
                throw new InvalidOperationException("key");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/property/{key}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{key}", Uri.EscapeDataString(key));
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
            
            return _httpResponse.ReasonPhrase;
        }
        public string RemoveProperty(string tenant, string namespaceParameter, string key, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemovePropertyAsync(tenant, namespaceParameter, key, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get property value for a given key on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='key'>
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
        public async ValueTask<string> RemovePropertyAsync(string tenant, string namespaceParameter, string key, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (key == null)
            {
                throw new InvalidOperationException("key");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/property/{key}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{key}", Uri.EscapeDataString(key));
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

        public string SetProperty(string tenant, string namespaceParameter, string key, string value, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetPropertyAsync(tenant, namespaceParameter, key, value, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Put a key value pair property on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='key'>
        /// </param>
        /// <param name='value'>
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
        public async ValueTask<string> SetPropertyAsync(string tenant, string namespaceParameter, string key, string value, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (key == null)
            {
                throw new InvalidOperationException("key");
            }
            if (value == null)
            {
                throw new InvalidOperationException("value");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/property/{key}/{value}").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{key}", Uri.EscapeDataString(key));
            _url = _url.Replace("{value}", Uri.EscapeDataString(value));
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

        public string GetNamespaceResourceGroup(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetNamespaceResourceGroupAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the resourcegroup attached to the namespace
        /// </summary>
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
        public async ValueTask<string> GetNamespaceResourceGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/resourcegroup").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public string SetNamespaceResourceGroup(string tenant, string namespaceParameter, string body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetNamespaceResourceGroupAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set resourcegroup for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Name of resourcegroup
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
        public async ValueTask<string> SetNamespaceResourceGroupAsync(string tenant, string namespaceParameter, string body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/resourcegroup").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public string RemoveNamespaceResourceGroup(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveNamespaceResourceGroupAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete resourcegroup for a namespace
        /// </summary>
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
        public async ValueTask<string> RemoveNamespaceResourceGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/resourcegroup").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

        public object GetScanOffloadedLedgers(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetScanOffloadedLedgersAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async ValueTask<object> GetScanOffloadedLedgersAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/scanOffloadedLedgers").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
            object _result = null;
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

        public EntryFilters GetEntryFiltersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return GetEntryFiltersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async ValueTask<EntryFilters> GetEntryFiltersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/entryFilters").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
        public string SetEntryFiltersPerTopic(string tenant, string namespaceParameter, EntryFilters body, Dictionary<string, List<string>> customHeaders = null)
        {
           return SetEntryFiltersPerTopicAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        public async ValueTask<string> SetEntryFiltersPerTopicAsync(string tenant, string namespaceParameter, EntryFilters body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }
            if (body == null)
            {
                throw new InvalidOperationException("body");
            }
            
            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/entryFilters").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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
        public string RemoveNamespaceEntryFilters(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
           return RemoveNamespaceEntryFiltersAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async ValueTask<string> RemoveNamespaceEntryFiltersAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
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
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/entryFilters").ToString();
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
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

    }
}
