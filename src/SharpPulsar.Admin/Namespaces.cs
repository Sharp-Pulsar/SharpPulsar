using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;
using SharpPulsar.Admin.Admin.Models;

namespace SharpPulsar.Admin
{
    using SubscriptionType = org.apache.pulsar.client.api.SubscriptionType;
    using AuthAction = SharpPulsar.Admin.Model.AuthAction;
    using AutoSubscriptionCreationOverride = SharpPulsar.Admin.Model.AutoSubscriptionCreationOverride;
    using AutoTopicCreationOverride = SharpPulsar.Admin.Model.AutoTopicCreationOverride;
    using BacklogQuota = SharpPulsar.Admin.Model.BacklogQuota;
    using BookieAffinityGroupData = SharpPulsar.Admin.Model.BookieAffinityGroupData;
    using BundlesData = SharpPulsar.Admin.Model.BundlesData;
    using DelayedDeliveryPolicies = SharpPulsar.Admin.Model.DelayedDeliveryPolicies;
    using DispatchRate = SharpPulsar.Admin.Model.DispatchRate;
    using EntryFilters = SharpPulsar.Admin.Model.EntryFilters;
    using InactiveTopicPolicies = SharpPulsar.Admin.Model.InactiveTopicPolicies;
    using OffloadPolicies = SharpPulsar.Admin.Model.OffloadPolicies;
    using PersistencePolicies = SharpPulsar.Admin.Model.PersistencePolicies;
    using Policies = SharpPulsar.Admin.Model.Policies;
    using PublishRate = SharpPulsar.Admin.Model.PublishRate;
    using RetentionPolicies = SharpPulsar.Admin.Model.RetentionPolicies;
    using SchemaAutoUpdateCompatibilityStrategy = SharpPulsar.Admin.Model.SchemaAutoUpdateCompatibilityStrategy;
    using SchemaCompatibilityStrategy = SharpPulsar.Admin.Model.SchemaCompatibilityStrategy;
    using SubscribeRate = SharpPulsar.Admin.Model.SubscribeRate;
    using SubscriptionAuthMode = SharpPulsar.Admin.Model.SubscriptionAuthMode;
    using TopicHashPositions = SharpPulsar.Admin.Model.TopicHashPositions;


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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<IList<string>>> GetTenantNamespacesAsync(string tenant, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetTenantNamespaces", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<IList<string>>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<IList<string>>(_responseContent, this.DeserializationSettings);
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<IList<string>>> GetTopicsAsync(string tenant, string namespaceParameter, string mode = default(string), bool? includeSystemTopic = false, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("mode", mode);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetTopics", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/topics").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (mode != null)
            {
                _queryParameters.Add(string.Format("mode={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(mode, this.SerializationSettings).Trim('"'))));
            }
            if (includeSystemTopic != null)
            {
                _queryParameters.Add(string.Format("includeSystemTopic={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(mode, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<IList<string>>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<IList<string>>(_responseContent, this.DeserializationSettings);
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<Policies>> GetPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetPolicies", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<Policies>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<Policies>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> CreateNamespaceAsync(string tenant, string namespaceParameter, Policies body = default(Policies), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (body != null)
            {
                body.Validate();
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "CreateNamespace", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> DeleteNamespaceAsync(string tenant, string namespaceParameter, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("force", force);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "DeleteNamespace", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (force != null)
            {
                _queryParameters.Add(string.Format("force={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(force, this.SerializationSettings).Trim('"'))));
            }
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 405 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> DeleteNamespaceBundleWithHttpMessagesAsync(string tenant, string namespaceParameter, string bundle, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (bundle == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "bundle");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("bundle", bundle);
                tracingParameters.Add("force", force);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "DeleteNamespaceBundle", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", System.Uri.EscapeDataString(bundle));
            List<string> _queryParameters = new List<string>();
            if (force != null)
            {
                _queryParameters.Add(string.Format("force={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(force, this.SerializationSettings).Trim('"'))));
            }
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<IDictionary<string, IList<string>>>> GetPermissionsAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetPermissions", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/permissions").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<IDictionary<string, IList<string>>>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<IDictionary<string, IList<string>>>(_responseContent, this.DeserializationSettings);
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<IDictionary<string, IList<string>>>> GetPermissionOnSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetPermissions", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/permissions/subscription").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<IDictionary<string, IList<string>>>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<IDictionary<string, IList<string>>>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> GrantPermissionOnNamespaceAsync(string tenant, string namespaceParameter, string role, IList<string> body = default(IList<string>), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (role == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "role");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("role", role);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GrantPermissionOnNamespace", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/permissions/{role}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{role}", System.Uri.EscapeDataString(role));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 200 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 501)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> GrantPermissionOnSubscriptionAsync(string property, string namespaceParameter, string subscription, IList<string> body = default(IList<string>), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (property == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "property");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (subscription == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "subscription");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("property", property);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("subscription", subscription);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GrantPermissionOnNamespace", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/permissions/subscription/{subscription}").ToString();
            _url = _url.Replace("{property}", System.Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", System.Uri.EscapeDataString(subscription));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 200 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 501)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RevokePermissionsOnNamespaceAsync(string property, string namespaceParameter, string subscription, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (role == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "role");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("role", role);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RevokePermissionsOnNamespace", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/permissions/{role}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{role}", System.Uri.EscapeDataString(role));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RevokePermissionOnSubscriptionAsync(string property, string namespaceParameter, string subscription, string role, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (property == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "property");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (subscription == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "subscription");
            }
            if (role == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "role");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("role", role);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RevokePermissionsOnNamespace", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/permissions/{subscription}/{role}").ToString();
            _url = _url.Replace("{property}", System.Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", System.Uri.EscapeDataString(subscription));
            _url = _url.Replace("{role}", System.Uri.EscapeDataString(role));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<IList<string>>> GetNamespaceReplicationClustersAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetNamespaceReplicationClusters", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/replication").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<IList<string>>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<IList<string>>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetNamespaceReplicationClustersAsync(string tenant, string namespaceParameter, IList<string> body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (body == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "body");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetNamespaceReplicationClusters", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/replication").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<int?>> GetNamespaceMessageTTLAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetNamespaceMessageTTL", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/messageTTL").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<int?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<int?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetNamespaceMessageTTLAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetNamespaceMessageTTL", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/messageTTL").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveNamespaceMessageTTLAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveNamespaceMessageTTL", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/messageTTL").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<int?>> GetSubscriptionExpirationTimeAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetSubscriptionExpirationTime", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionExpirationTime").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<int?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<int?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetSubscriptionExpirationTimeAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSubscriptionExpirationTime", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionExpirationTime").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveSubscriptionExpirationTimeAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSubscriptionExpirationTime", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionExpirationTime").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<bool?>> GetDeduplicationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetDeduplication", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/deduplication").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<bool?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<bool?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> ModifyDeduplicationAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "ModifyDeduplication", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/deduplication").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveDeduplicationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveDeduplication", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/deduplication").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetAutoTopicCreationAsync(string tenant, string namespaceParameter, AutoTopicCreationOverride body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (body == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "body");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetAutoTopicCreation", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoTopicCreation").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 400 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 406)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveAutoTopicCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveAutoTopicCreation", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoTopicCreation").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse<bool?>> GetAutoTopicCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveAutoTopicCreation", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoTopicCreation").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<bool?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<bool?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetAutoSubscriptionCreationWAsync(string tenant, string namespaceParameter, AutoSubscriptionCreationOverride body = default(AutoSubscriptionCreationOverride), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetAutoSubscriptionCreation", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoSubscriptionCreation").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 400 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveAutoSubscriptionCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveAutoSubscriptionCreation", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoSubscriptionCreation").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse<bool?>> GetAutoSubscriptionCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveAutoSubscriptionCreation", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/autoSubscriptionCreation").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<bool?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<bool?>(_responseContent, this.DeserializationSettings);
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<BundlesData>> GetBundlesDataAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetBundlesData", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/bundles").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<BundlesData>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<BundlesData>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> UnloadNamespaceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "UnloadNamespace", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/unload").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> UnloadNamespaceBundleAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (bundle == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "bundle");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("bundle", bundle);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "UnloadNamespaceBundle", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/unload").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", System.Uri.EscapeDataString(bundle));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 307 && (int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SplitNamespaceBundleAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, bool? unload = false, string splitAlgorithmName = default(string), IList<long> body = default(IList<long>), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (bundle == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "bundle");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("bundle", bundle);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("unload", unload);
                tracingParameters.Add("splitAlgorithmName", splitAlgorithmName);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SplitNamespaceBundle", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/split").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", System.Uri.EscapeDataString(bundle));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
            }
            if (unload != null)
            {
                _queryParameters.Add(string.Format("unload={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(unload, this.SerializationSettings).Trim('"'))));
            }
            if (splitAlgorithmName != null)
            {
                _queryParameters.Add(string.Format("splitAlgorithmName={0}", System.Uri.EscapeDataString(splitAlgorithmName)));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 307 && (int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
        }

        public async Task<Microsoft.Rest.HttpOperationResponse<TopicHashPositions>> GetTopicHashPositionsAsync(string tenant, string namespaceParameter, string bundle, List<string> topics = default(List<string>), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (bundle == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "bundle");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("bundle", bundle);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SplitNamespaceBundle", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/topicHashPositions").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", System.Uri.EscapeDataString(bundle));
            List<string> _queryParameters = new List<string>();
            if (topics != null)
            {
                _queryParameters.Add(string.Format("topics={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(topics, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<TopicHashPositions>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<BundlesData>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse<PublishRate>> GetPublishRateAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (property == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "property");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("topic", topic);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetPublishRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/publishRate").ToString();
            _url = _url.Replace("{property}", System.Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 405 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<TopicHashPositions>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<BundlesData>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetPublishRateAsync(string property, string namespaceParameter, PublishRate body = default(PublishRate), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (property == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "property");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("topic", topic);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetPublishRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/publishRate").ToString();
            _url = _url.Replace("{property}", System.Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 405 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemovePublishRateAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (property == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "property");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("topic", topic);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemovePublishRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/publishRate").ToString();
            _url = _url.Replace("{property}", System.Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 405 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<DispatchRate>> GetDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetDispatchRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/dispatchRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<DispatchRate>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<DispatchRate>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetDispatchRateAsync(string tenant, string namespaceParameter, DispatchRateImpl body = default(DispatchRateImpl), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetDispatchRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/dispatchRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> DeleteDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "DeleteDispatchRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/dispatchRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<DispatchRate>> GetSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetSubscriptionDispatchRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionDispatchRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<DispatchRate>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<DispatchRate>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, DispatchRateImpl body = default(DispatchRateImpl), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSubscriptionDispatchRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionDispatchRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> DeleteSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "DeleteSubscriptionDispatchRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionDispatchRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<SubscribeRate>> GetSubscribeRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetSubscribeRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscribeRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<SubscribeRate>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<SubscribeRate>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetSubscribeRateAsync(string tenant, string namespaceParameter, SubscribeRate body = default(SubscribeRate), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSubscribeRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscribeRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> DeleteSubscribeRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "DeleteSubscribeRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscribeRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<DispatchRate>> GetReplicatorDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetReplicatorDispatchRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/replicatorDispatchRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<DispatchRate>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<DispatchRate>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetReplicatorDispatchRateAsync(string tenant, string namespaceParameter, DispatchRateImpl body = default(DispatchRateImpl), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetReplicatorDispatchRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/replicatorDispatchRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveReplicatorDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveReplicatorDispatchRate", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/replicatorDispatchRate").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetBacklogQuotaAsync(string tenant, string namespaceParameter, string backlogQuotaType = default(string), BacklogQuota body = default(BacklogQuota), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("backlogQuotaType", backlogQuotaType);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetBacklogQuota", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/backlogQuota").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (backlogQuotaType != null)
            {
                _queryParameters.Add(string.Format("backlogQuotaType={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(backlogQuotaType, this.SerializationSettings).Trim('"'))));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveBacklogQuotaAsync(string tenant, string namespaceParameter, string backlogQuotaType = default(string), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("backlogQuotaType", backlogQuotaType);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveBacklogQuota", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/backlogQuota").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (backlogQuotaType != null)
            {
                _queryParameters.Add(string.Format("backlogQuotaType={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(backlogQuotaType, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<IDictionary<string, BacklogQuota>>> GetBacklogQuotaMapAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetBacklogQuotaMap", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/backlogQuotaMap").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<IDictionary<string, BacklogQuota>>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<IDictionary<string, BacklogQuota>>(_responseContent, this.DeserializationSettings);
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<RetentionPolicies>> GetRetentionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetRetention", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/retention").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<RetentionPolicies>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<RetentionPolicies>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetRetentionAsync(string tenant, string namespaceParameter, RetentionPolicies body = default(RetentionPolicies), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetRetention", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/retention").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveRetentionAsync(string tenant, string namespaceParameter, RetentionPolicies body = default(RetentionPolicies), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveRetention", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/retention").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetPersistenceAsync(string tenant, string namespaceParameter, PersistencePolicies body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (body == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "body");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetPersistence", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/persistence").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 400 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> DeletePersistenceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "DeletePersistence", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/persistence").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<BookieAffinityGroupData>> GetBookieAffinityGroupAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (property == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "property");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("property", property);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetBookieAffinityGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/persistence/bookieAffinity").ToString();
            _url = _url.Replace("{property}", System.Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<BookieAffinityGroupData>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<BookieAffinityGroupData>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> DeleteBookieAffinityGroupAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (property == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "property");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("property", property);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "DeleteBookieAffinityGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{property}/{namespace}/persistence/bookieAffinity").ToString();
            _url = _url.Replace("{property}", System.Uri.EscapeDataString(property));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetBookieAffinityGroupAsync(string tenant, string namespaceParameter, BookieAffinityGroupData body = default(BookieAffinityGroupData), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetBookieAffinityGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/persistence/bookieAffinity").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<PersistencePolicies>> GetPersistenceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetPersistence", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/persistence").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<PersistencePolicies>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<PersistencePolicies>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> ClearNamespaceBacklogAsync(string tenant, string namespaceParameter, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "ClearNamespaceBacklog", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/clearBacklog").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> ClearNamespaceBacklogForSubscriptionAsync(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (subscription == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "subscription");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("subscription", subscription);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "ClearNamespaceBacklogForSubscription", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/clearBacklog/{subscription}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", System.Uri.EscapeDataString(subscription));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> ClearNamespaceBundleBacklogAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (bundle == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "bundle");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("bundle", bundle);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "ClearNamespaceBundleBacklog", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/clearBacklog").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{bundle}", System.Uri.EscapeDataString(bundle));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> ClearNamespaceBundleBacklogForSubscriptionAsync(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (subscription == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "subscription");
            }
            if (bundle == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "bundle");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("subscription", subscription);
                tracingParameters.Add("bundle", bundle);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "ClearNamespaceBundleBacklogForSubscription", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/clearBacklog/{subscription}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", System.Uri.EscapeDataString(subscription));
            _url = _url.Replace("{bundle}", System.Uri.EscapeDataString(bundle));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 307 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> UnsubscribeNamespaceAsync(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (subscription == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "subscription");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("subscription", subscription);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "UnsubscribeNamespace", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/unsubscribe/{subscription}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", System.Uri.EscapeDataString(subscription));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> UnsubscribeNamespaceBundleAsync(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (subscription == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "subscription");
            }
            if (bundle == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "bundle");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("subscription", subscription);
                tracingParameters.Add("bundle", bundle);
                tracingParameters.Add("authoritative", authoritative);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "UnsubscribeNamespaceBundle", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/{bundle}/unsubscribe/{subscription}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{subscription}", System.Uri.EscapeDataString(subscription));
            _url = _url.Replace("{bundle}", System.Uri.EscapeDataString(bundle));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", System.Uri.EscapeDataString(Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(authoritative, this.SerializationSettings).Trim('"'))));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetSubscriptionAuthModeAsync(string tenant, string namespaceParameter, string body = default(string), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSubscriptionAuthMode", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionAuthMode").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse<SubscriptionAuthMode>> GetSubscriptionAuthModeAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSubscriptionAuthMode", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionAuthMode").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<PersistencePolicies>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<PersistencePolicies>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> ModifyEncryptionRequiredAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "ModifyEncryptionRequired", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/encryptionRequired").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
        }

        public async Task<Microsoft.Rest.HttpOperationResponse<EncryptionRequired>> GetEncryptionRequiredAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSubscriptionAuthMode", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/encryptionRequired").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<PersistencePolicies>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<PersistencePolicies>(_responseContent, this.DeserializationSettings);
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<DelayedDeliveryPolicies>> GetDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetDelayedDeliveryPolicies", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/delayedDelivery").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<DelayedDeliveryPolicies>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<DelayedDeliveryPolicies>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, DelayedDeliveryPolicies body = default(DelayedDeliveryPolicies), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetDelayedDeliveryPolicies", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/delayedDelivery").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveDelayedDeliveryPolicies", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/delayedDelivery").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<InactiveTopicPolicies>> GetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetInactiveTopicPolicies", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/inactiveTopicPolicies").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<InactiveTopicPolicies>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<InactiveTopicPolicies>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, InactiveTopicPolicies body = default(InactiveTopicPolicies), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetInactiveTopicPolicies", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/inactiveTopicPolicies").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveInactiveTopicPolicies", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/inactiveTopicPolicies").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<int?>> GetMaxProducersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetMaxProducersPerTopic", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxProducersPerTopic").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<int?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<int?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetMaxProducersPerTopicAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetMaxProducersPerTopic", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxProducersPerTopic").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveMaxProducersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveMaxProducersPerTopic", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxProducersPerTopic").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<int?>> GetDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetDeduplicationSnapshotInterval", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/deduplicationSnapshotInterval").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<int?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<int?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetDeduplicationSnapshotInterval", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/deduplicationSnapshotInterval").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<int?>> GetMaxConsumersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetMaxConsumersPerTopic", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerTopic").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<int?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<int?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetMaxConsumersPerTopicAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetMaxConsumersPerTopic", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerTopic").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveMaxConsumersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveMaxConsumersPerTopic", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerTopic").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<int?>> GetMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetMaxConsumersPerSubscription", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerSubscription").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<int?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<int?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetMaxConsumersPerSubscription", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerSubscription").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveMaxConsumersPerSubscription", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxConsumersPerSubscription").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<int?>> GetMaxUnackedMessagesPerConsumerAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetMaxUnackedMessagesPerConsumer", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerConsumer").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<int?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<int?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetMaxUnackedMessagesPerConsumerAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetMaxUnackedMessagesPerConsumer", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerConsumer").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveMaxUnackedmessagesPerConsumerAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveMaxUnackedmessagesPerConsumer", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerConsumer").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<int?>> GetMaxUnackedmessagesPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetMaxUnackedmessagesPerSubscription", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerSubscription").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<int?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<int?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetMaxUnackedMessagesPerSubscriptionAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetMaxUnackedMessagesPerSubscription", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerSubscription").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveMaxUnackedmessagesPerSubscriptionMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveMaxUnackedmessagesPerSubscription", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxUnackedMessagesPerSubscription").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<int?>> GetMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetMaxSubscriptionsPerTopic", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxSubscriptionsPerTopic").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<int?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<int?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetMaxSubscriptionsPerTopic", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxSubscriptionsPerTopic").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveMaxSubscriptionsPerTopic", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxSubscriptionsPerTopic").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<string>> GetNamespaceAntiAffinityGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetNamespaceAntiAffinityGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/antiAffinity").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<string>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<string>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetNamespaceAntiAffinityGroupAsync(string tenant, string namespaceParameter, string body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (body == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "body");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetNamespaceAntiAffinityGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/antiAffinity").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveNamespaceAntiAffinityGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveNamespaceAntiAffinityGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/antiAffinity").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<IList<string>>> GetAntiAffinityNamespacesAsync(string cluster, string group, string tenant = default(string), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (cluster == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "cluster");
            }
            if (group == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "group");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("cluster", cluster);
                tracingParameters.Add("group", group);
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetAntiAffinityNamespaces", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{cluster}/antiAffinity/{group}").ToString();
            _url = _url.Replace("{cluster}", System.Uri.EscapeDataString(cluster));
            _url = _url.Replace("{group}", System.Uri.EscapeDataString(group));
            List<string> _queryParameters = new List<string>();
            if (tenant != null)
            {
                _queryParameters.Add(string.Format("tenant={0}", System.Uri.EscapeDataString(tenant)));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<IList<string>>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<IList<string>>(_responseContent, this.DeserializationSettings);
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<long?>> GetCompactionThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetCompactionThreshold", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/compactionThreshold").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<long?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<long?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetCompactionThresholdAsync(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetCompactionThreshold", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/compactionThreshold").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> DeleteCompactionThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "DeleteCompactionThreshold", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/compactionThreshold").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<long?>> GetOffloadThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetOffloadThreshold", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadThreshold").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<long?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<long?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetOffloadThresholdAsync(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetOffloadThreshold", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadThreshold").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<long?>> GetOffloadDeletionLagAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetOffloadDeletionLag", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadDeletionLagMs").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<long?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<long?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetOffloadDeletionLagAsync(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetOffloadDeletionLag", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadDeletionLagMs").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> ClearOffloadDeletionLagAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "ClearOffloadDeletionLag", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadDeletionLagMs").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<SchemaAutoUpdateCompatibilityStrategy>> GetSchemaAutoUpdateCompatibilityStrategyAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetSchemaAutoUpdateCompatibilityStrategy", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaAutoUpdateCompatibilityStrategy").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<string>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<string>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetSchemaAutoUpdateCompatibilityStrategyAsync(string tenant, string namespaceParameter, SchemaAutoUpdateCompatibilityStrategy body = default(SchemaAutoUpdateCompatibilityStrategy), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSchemaAutoUpdateCompatibilityStrategy", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaAutoUpdateCompatibilityStrategy").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<string>> GetSchemaCompatibilityStrategyAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetSchemaCompatibilityStrategy", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaCompatibilityStrategy").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<string>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<string>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetSchemaCompatibilityStrategyAsync(string tenant, string namespaceParameter, string body = default(string), Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSchemaCompatibilityStrategy", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaCompatibilityStrategy").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<bool?>> GetIsAllowAutoUpdateSchemaAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetIsAllowAutoUpdateSchema", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/isAllowAutoUpdateSchema").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<bool?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<bool?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetIsAllowAutoUpdateSchemaAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetIsAllowAutoUpdateSchema", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/isAllowAutoUpdateSchema").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<IList<string>>> GetSubscriptionTypesEnabledAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetSubscriptionTypesEnabled", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionTypesEnabled").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<IList<string>>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<IList<string>>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetSubscriptionTypesEnabledAsync(string tenant, string namespaceParameter, IList<string> body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (body == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "body");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSubscriptionTypesEnabled", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionTypesEnabled").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
        }
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveSubscriptionTypesEnabledAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveNamespaceMessageTTL", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/subscriptionTypesEnabled").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<bool?>> GetSchemaValidtionEnforcedAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetSchemaValidtionEnforced", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaValidationEnforced").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<bool?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<bool?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetSchemaValidtionEnforcedAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetSchemaValidtionEnforced", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/schemaValidationEnforced").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<OffloadPolicies>> GetOffloadPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetOffloadPolicies", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadPolicies").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<OffloadPoliciesImpl>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<OffloadPoliciesImpl>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetOffloadPoliciesAsync(string tenant, string namespaceParameter, OffloadPoliciesImpl body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (body == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "body");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetOffloadPolicies", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/offloadPolicies").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveOffloadPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveOffloadPolicies", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/removeOffloadPolicies").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 409 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<int?>> GetMaxTopicsPerNamespaceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetMaxTopicsPerNamespace", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxTopicsPerNamespace").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<int?>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<int?>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetInactiveTopicPolicies1", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxTopicsPerNamespace").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
            _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
            _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetInactiveTopicPolicies2", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/maxTopicsPerNamespace").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> GetPropertiesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetProperties", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/properties").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetPropertiesAsync(string tenant, string namespaceParameter, IDictionary<string, string> body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (body == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "body");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetProperties", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/properties").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> ClearPropertiesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "ClearProperties", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/properties").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> GetPropertyAsync(string tenant, string namespaceParameter, string key, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (key == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "key");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("key", key);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetProperty", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/property/{key}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{key}", System.Uri.EscapeDataString(key));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemovePropertyAsync(string tenant, string namespaceParameter, string key, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (key == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "key");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("key", key);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveProperty", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/property/{key}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{key}", System.Uri.EscapeDataString(key));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetPropertyAsync(string tenant, string namespaceParameter, string key, string value, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (key == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "key");
            }
            if (value == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "value");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("key", key);
                tracingParameters.Add("value", value);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetProperty", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/property/{key}/{value}").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{key}", System.Uri.EscapeDataString(key));
            _url = _url.Replace("{value}", System.Uri.EscapeDataString(value));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
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
        public async Task<Microsoft.Rest.HttpOperationResponse<string>> GetNamespaceResourceGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetNamespaceResourceGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/resourcegroup").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<string>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<string>(_responseContent, this.DeserializationSettings);
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
        public async Task<Microsoft.Rest.HttpOperationResponse> SetNamespaceResourceGroupAsync(string tenant, string namespaceParameter, string body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (body == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "body");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetNamespaceResourceGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/resourcegroup").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
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
        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveNamespaceResourceGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveNamespaceResourceGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/resourcegroup").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
        }

        public async Task<Microsoft.Rest.HttpOperationResponse<object>> GetScanOffloadedLedgersAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetNamespaceResourceGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/scanOffloadedLedgers").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<object>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<string>(_responseContent, this.DeserializationSettings);
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


        public async Task<Microsoft.Rest.HttpOperationResponse<EntryFilters>> GetEntryFiltersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "GetNamespaceResourceGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/entryFilters").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 200 && (int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse<EntryFilters>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = Microsoft.Rest.Serialization.SafeJsonConvert.DeserializeObject<string>(_responseContent, this.DeserializationSettings);
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

        public async Task<Microsoft.Rest.HttpOperationResponse> SetEntryFiltersPerTopicAsync(string tenant, string namespaceParameter, EntryFilters body, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            if (body == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "body");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("body", body);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "SetNamespaceResourceGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/entryFilters").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
                _requestContent = Microsoft.Rest.Serialization.SafeJsonConvert.SerializeObject(body, this.SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
        }

        public async Task<Microsoft.Rest.HttpOperationResponse> RemoveNamespaceEntryFiltersAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (tenant == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "tenant");
            }
            if (namespaceParameter == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Tracing
            bool _shouldTrace = Microsoft.Rest.ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = Microsoft.Rest.ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("tenant", tenant);
                tracingParameters.Add("namespaceParameter", namespaceParameter);
                tracingParameters.Add("cancellationToken", cancellationToken);
                Microsoft.Rest.ServiceClientTracing.Enter(_invocationId, this, "RemoveNamespaceResourceGroup", tracingParameters);
            }
            // Construct URL
            var _baseUrl = this.BaseUri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "namespaces/{tenant}/{namespace}/entryFilters").ToString();
            _url = _url.Replace("{tenant}", System.Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
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
            if ((int)_statusCode != 403 && (int)_statusCode != 404 && (int)_statusCode != 412)
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
            var _result = new Microsoft.Rest.HttpOperationResponse();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            if (_shouldTrace)
            {
                Microsoft.Rest.ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
        }

    }
}
