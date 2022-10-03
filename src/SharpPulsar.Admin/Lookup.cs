using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.Interfaces;
using SharpPulsar.Common.Naming;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Admin
{

    /// <summary>
    /// This is an interface class to allow using command line tool to quickly lookup the broker serving the topic.
    /// </summary>
    public class Lookup
	{
        private Uri _uri;
        private HttpClient _httpClient;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public Lookup(string brokerwebserviceurl, HttpClient httpClient)
        {
            _uri = new Uri($"{brokerwebserviceurl.TrimEnd('/')}/lookup/v2/topic");
            _httpClient = httpClient;
            if (_uri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
        }
        /// <summary>
        /// Lookup a topic.
        /// </summary>
        /// <param name="topic"> </param>
        /// <returns> the broker URL that serves the topic </returns>
        public string LookupTopic(string topicDomain, string tenant, string @namespace, string topic, bool? authoritative = false, string? listenerName = default, Dictionary<string, List<string>> customHeaders = null)
        {
            return LookupTopicAsync(topicDomain, tenant, @namespace, topic, authoritative, listenerName, customHeaders).GetAwaiter().GetResult();
        }

		/// <summary>
		/// Lookup a topic asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <returns> the broker URL that serves the topic </returns>
		public async ValueTask<string> LookupTopicAsync(string topicDomain, string tenant, string namespaceParameter,  string topic, bool? authoritative = false, string? listenerName = default, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (topicDomain == null)
            {
                throw new InvalidOperationException("topicDomain");
            }
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }

            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "{topic-domain}/{tenant}/{namespace}/{topic}").ToString();
            _url = _url.Replace("{topic-domain}", Uri.EscapeDataString(topicDomain));
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            List<string> _queryParameters = new List<string>();
            if (authoritative != null)
            {
                _queryParameters.Add(string.Format("authoritative={0}", Uri.EscapeDataString(JsonSerializer.Serialize(authoritative, _jsonSerializerOptions).Trim('"'))));
            }
            if (listenerName != null)
            {
                _queryParameters.Add(string.Format("listenerName={0}", Uri.EscapeDataString(listenerName)));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = new Uri(_url);
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

            // Serialize Request
            string requestContent = null;
            cancellationToken.ThrowIfCancellationRequested();
            httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            System.Net.HttpStatusCode statusCode = httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string responseContent = null;
            if ((int)statusCode != 200 && (int)statusCode != 307)
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
            string result = null;
            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result = JsonSerializer.Deserialize<string>(responseContent); ;
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

		/// <summary>
		/// Get a bundle range of a topic.
		/// </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		public string GetBundleRange(string topicDomain, string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBundleRangeAsync(topicDomain, tenant, namespaceParameter, topic, customHeaders).GetAwaiter().GetResult();
        }

		/// <summary>
		/// Get a bundle range of a topic asynchronously.
		/// </summary>
		/// <param name="topic">
		/// @return </param>
	    public async ValueTask<string> GetBundleRangeAsync(string topicDomain, string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {

            if (topicDomain == null)
            {
                throw new InvalidOperationException("topicDomain");
            }
            if (tenant == null)
            {
                throw new InvalidOperationException("tenant");
            }
            if (topic == null)
            {
                throw new InvalidOperationException("topic");
            }
            if (namespaceParameter == null)
            {
                throw new InvalidOperationException("namespaceParameter");
            }

            var _baseUrl = _uri.AbsoluteUri;
            var _url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "{topic-domain}/{tenant}/{namespace}/{topic}/bundle").ToString();
            _url = _url.Replace("{topic-domain}", Uri.EscapeDataString(topicDomain));
            _url = _url.Replace("{tenant}", Uri.EscapeDataString(tenant));
            _url = _url.Replace("{namespace}", Uri.EscapeDataString(namespaceParameter));
            _url = _url.Replace("{topic}", Uri.EscapeDataString(topic));
            
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = new Uri(_url);
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

            // Serialize Request
            string requestContent = null;
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
            string result = null;
            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result = JsonSerializer.Deserialize<string>(responseContent); ;
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
        
    }

}