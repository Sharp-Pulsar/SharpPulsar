using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.Admin.Models;
using SharpPulsar.Admin.interfaces;
using SharpPulsar.Admin.Model;
using BookieInfo = SharpPulsar.Admin.Model.BookieInfo;
using BookiesClusterInfo = SharpPulsar.Admin.Model.BookiesClusterInfo;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SharpPulsar.Admin
{
    public class Bookies:IBookies
    {
        private Uri _uri;
        private HttpClient _httpClient;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public Bookies(string brokerwebserviceurl, HttpClient httpClient)
        {
            _uri = new Uri($"{brokerwebserviceurl.TrimEnd('/')}/admin/v2");
            _httpClient = httpClient;
            if (_uri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
        }
        public BookiesClusterInfo GetBookies(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBookiesAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets raw information for all the bookies in the cluster
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
        public async ValueTask<BookiesClusterInfo> GetBookiesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Construct URL
            var baseUrl = _uri.AbsoluteUri;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/")), "bookies/all");
            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = url;
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
            BookiesClusterInfo result = null;
            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<BookiesClusterInfo>(responseContent); ;
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

        public IDictionary<string, IDictionary<string, BookieInfo>> GetBookiesRackInfo(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBookiesRackInfoAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets the rack placement information for all the bookies in the cluster
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
        public async ValueTask<IDictionary<string, IDictionary<string, BookieInfo>>> GetBookiesRackInfoAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _baseUrl = _uri.AbsoluteUri; ;
            var url = new Uri(new Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "bookies/racks-info");
            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = url;
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
            //string requestContent = null;
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
            IDictionary<string, IDictionary<string, BookieInfo>> result = null;
            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, IDictionary<string, BookieInfo>>>(responseContent);
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
        public BookieInfo GetBookieRackInfo(string bookie, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBookieRackInfoAsync(bookie, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets the rack placement information for a specific bookie in the cluster
        /// </summary>
        /// <param name='bookie'>
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
        public async ValueTask<BookieInfo> GetBookieRackInfoAsync(string bookie, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (bookie == null)
            {
                throw new InvalidOperationException("bookie");
            }

            // Construct URL
            var baseUrl = _uri.AbsoluteUri; ;
            var url = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/")), "bookies/racks-info/{bookie}".
                Replace("{bookie}", Uri.EscapeDataString(bookie)));


            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("GET");
            httpRequest.RequestUri = url;
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

            // Send Request
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
            BookieInfo result = null;
            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<BookieInfo>(responseContent);
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
        public void UpdateBookieRackInfo(string bookie, string group = default(string), BookieInfo bookieInfo = default(BookieInfo), Dictionary<string, List<string>> customHeaders = null)
        {
            UpdateBookieRackInfoAsync(bookie, group, bookieInfo, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Updates the rack placement information for a specific bookie in the cluster
        /// (note. bookie address format:`address:port`)
        /// </summary>
        /// <param name='bookie'>
        /// </param>
        /// <param name='group'>
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
        public async ValueTask UpdateBookieRackInfoAsync(string bookie, string group = default(string), BookieInfo bookieInfo = default(BookieInfo), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (bookie == null)
            {
                throw new InvalidOperationException("bookie");
            }
            var baseUrl = _uri.AbsoluteUri; ;
            var uri = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/")), "bookies/racks-info/{bookie}").ToString();
            uri = uri.Replace("{bookie}", Uri.EscapeDataString(bookie));
            List<string> queryParameters = new List<string>();
            if (group != null)
            {
                queryParameters.Add(string.Format("group={0}", Uri.EscapeDataString(JsonSerializer.Serialize(group, _jsonSerializerOptions))));
            }
            if (queryParameters.Count > 0)
            {
                uri += "?" + string.Join("&", queryParameters);
            }
            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("POST");
            httpRequest.RequestUri = new Uri(uri);
            // Set Headers


            if (customHeaders != null)
            {
                foreach (var _header in customHeaders)
                {
                    if (httpRequest.Headers.Contains(_header.Key))
                    {
                        httpRequest.Headers.Remove(_header.Key);
                    }
                    httpRequest.Headers.TryAddWithoutValidation(_header.Key, _header.Value);
                }
            }

            // Serialize Request
            string requestContent = null;
            if (bookieInfo != null)
            {
                requestContent = System.Text.Json.JsonSerializer.Serialize(bookieInfo, _jsonSerializerOptions);
                httpRequest.Content = new StringContent(requestContent, Encoding.UTF8);
                httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }

            cancellationToken.ThrowIfCancellationRequested();
            httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            

            System.Net.HttpStatusCode statusCode = httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string responseContent = null;
            if ((int)statusCode != 403)
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
           
            return;
        }
        public void DeleteBookieRackInfo(string bookie, Dictionary<string, List<string>> customHeaders = null)
        {
            DeleteBookieRackInfoAsync(bookie, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Removed the rack placement information for a specific bookie in the cluster
        /// </summary>
        /// <param name='bookie'>
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
        public async ValueTask DeleteBookieRackInfoAsync(string bookie, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (bookie == null)
            {
                throw new InvalidOperationException("bookie");
            }

            var baseUrl = _uri.AbsoluteUri; ;
            var uri = new Uri(new Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/")), "bookies/racks-info/{bookie}").ToString();
            uri = uri.Replace("{bookie}", Uri.EscapeDataString(bookie));
            
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("DELETE");
            httpRequest.RequestUri = new Uri(uri);
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
            if ((int)statusCode != 403)
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
            return;
        }

    }
}
