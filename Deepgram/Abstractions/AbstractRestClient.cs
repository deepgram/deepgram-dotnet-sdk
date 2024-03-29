// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Encapsulations;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Abstractions;

public abstract class AbstractRestClient
{
    /// <summary>
    ///  HttpClient created by the factory
    internal HttpClient _httpClient;

    /// <summary>
    /// get the logger of category type of _clientName
    /// </summary>
    internal ILogger _logger => LogProvider.GetLogger(this.GetType().Name);

    /// <summary>
    /// Copy of the options for the client
    /// </summary>
    internal DeepgramHttpClientOptions _options;

    /// <summary>
    /// Constructor that take the options and a httpClient
    /// </summary>
    /// <param name="deepgramClientOptions"><see cref="_deepgramClientOptions"/>Options for the Deepgram client</param>

    internal AbstractRestClient(string? apiKey = null, DeepgramHttpClientOptions? options = null)
    {
        options ??= new DeepgramHttpClientOptions(apiKey);
        _httpClient = HttpClientFactory.Create();
        _httpClient = HttpClientFactory.ConfigureDeepgram(_httpClient, options);
        _options = options;
    }

    /// <summary>
    /// GET Rest Request
    /// </summary>
    /// <typeparam name="T">Type of class of response expected</typeparam>
    /// <param name="uriSegment">request uri Endpoint</param>
    /// <returns>Instance of T</returns>
    public virtual async Task<T> GetAsync<T>(string uriSegment, CancellationTokenSource? cancellationToken = null,
    Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(Constants.DefaultRESTTimeout);
            }

            // create request message and add custom query parameters
            NoopSchema? parameter = null;
            var request = new HttpRequestMessage(HttpMethod.Get, QueryParameterUtil.FormatURL(uriSegment, parameter, addons));

            // add custom headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);
            response.EnsureSuccessStatusCode();
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            return result;
        }
        catch (Exception ex)
        {
            Log.Exception(_logger, "GET", ex);
            throw;
        }
    }

    public virtual async Task<T> GetAsync<S, T>(string uriSegment, S? parameter, CancellationTokenSource? cancellationToken = null,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(Constants.DefaultRESTTimeout);
            }

            // create request message and add custom query parameters
            var request = new HttpRequestMessage(HttpMethod.Get, QueryParameterUtil.FormatURL(uriSegment, parameter, addons));

            // add custom headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);
            response.EnsureSuccessStatusCode();
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            return result;
        }
        catch (Exception ex)
        {
            Log.Exception(_logger, "GET", ex);
            throw;
        }
    }

    /// <summary>
    /// Post method 
    /// </summary>
    /// <typeparam name="T">Class type of what return type is expected</typeparam>
    /// <param name="uriSegment">Uri for the api including the query parameters</param> 
    /// <param name="content">HttpContent as content for HttpRequestMessage</param>  
    /// <returns>Instance of T</returns>
    public virtual async Task<LocalFileWithMetadata> PostRetrieveLocalFileAsync<R, S, T>(string uriSegment, S? parameter, R? content,
        List<string>? keys = null, CancellationTokenSource? cancellationToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null
        )
    {
        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(Constants.DefaultRESTTimeout);
            }

            // create request message and add custom query parameters
            var request = new HttpRequestMessage(HttpMethod.Post, QueryParameterUtil.FormatURL(uriSegment, parameter, addons))
            {
                Content = HttpRequestUtil.CreatePayload(content)
            };

            // add custom headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);
            response.EnsureSuccessStatusCode();

            var result = new Dictionary<string, string>();

            if (keys != null)
            {
                for (int i = 0; i < response.Headers.Count(); i++)
                {
                    var key = response.Headers.ElementAt(i).Key.ToLower();
                    var value = response.Headers.GetValues(key).FirstOrDefault() ?? "";

                    var index = key.IndexOf("x-dg-");
                    if (index == 0)
                    {
                        var newKey = key.Substring(5);
                        if (keys.Contains(newKey))
                        {
                            result.Add(newKey, value);
                            continue;
                        }
                    }
                    index = key.IndexOf("dg-");
                    if (index == 0)
                    {
                        var newKey = key.Substring(3);
                        if (keys.Contains(newKey))
                        {
                            result.Add(newKey, value);
                            continue;
                        }
                    }
                    if (keys.Contains(key))
                    {
                        result.Add(key, value);
                    }
                }

                if (keys.Contains("content-type"))
                {
                    result.Add("content-type", response.Content.Headers.ContentType?.MediaType ?? "");
                }
            }

            MemoryStream stream = new MemoryStream();
            await response.Content.CopyToAsync(stream);

            return new LocalFileWithMetadata()
                        {
                            Metadata = result,
                            Content = stream,
                        };

        }
        catch (Exception ex)
        {
            Log.Exception(_logger, "POST", ex);
            throw;
        }
    }

    /// <summary>
    /// Post method 
    /// </summary>
    /// <typeparam name="T">Class type of what return type is expected</typeparam>
    /// <param name="uriSegment">Uri for the api including the query parameters</param> 
    /// <param name="content">HttpContent as content for HttpRequestMessage</param>  
    /// <returns>Instance of T</returns>
    public virtual async Task<T> PostAsync<S, T>(string uriSegment, S? parameter, CancellationTokenSource? cancellationToken = null,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(Constants.DefaultRESTTimeout);
            }

            // create request message and add custom query parameters
            var request = new HttpRequestMessage(HttpMethod.Post, QueryParameterUtil.FormatURL(uriSegment, parameter, addons))
            {
                Content = HttpRequestUtil.CreatePayload(parameter)
            };

            // add custom headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);
            response.EnsureSuccessStatusCode();
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            return result;
        }
        catch (Exception ex)
        {
            Log.Exception(_logger, "POST", ex);
            throw;
        }
    }

    public virtual async Task<T> PostAsync<R, S, T>(string uriSegment, S? parameter, R? content, CancellationTokenSource? cancellationToken = null,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(Constants.DefaultRESTTimeout);
            }

            // create request message and add custom query parameters
            var request = new HttpRequestMessage(HttpMethod.Post, QueryParameterUtil.FormatURL(uriSegment, parameter, addons));
            if (typeof(R) == typeof(Stream))
            {
                Stream? stream = content as Stream;
                if (stream == null)
                {
                    stream = new MemoryStream();
                }
                request.Content = HttpRequestUtil.CreateStreamPayload(stream);
            }
            else
            {
                request.Content = HttpRequestUtil.CreatePayload(content);
            }

            // add custom headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);
            response.EnsureSuccessStatusCode();
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            return result;
        }
        catch (Exception ex)
        {
            Log.Exception(_logger, "POST", ex);
            throw;
        }
    }

    /// <summary>
    /// Patch method call that takes a body object
    /// </summary>
    /// <typeparam name="T">Class type of what return type is expected</typeparam>
    /// <param name="uriSegment">Uri for the api including the query parameters</param>  
    /// <returns>Instance of T</returns>
    public virtual async Task<T> PatchAsync<S, T>(string uriSegment, S? parameter, CancellationTokenSource? cancellationToken = null,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(Constants.DefaultRESTTimeout);
            }

            // create request message and add custom query parameters
#if NETSTANDARD2_0
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), QueryParameterUtil.FormatURL(uriSegment, parameter, addons))
            {
                Content = HttpRequestUtil.CreatePayload(parameter)
            };
#else
            var request = new HttpRequestMessage(HttpMethod.Patch, QueryParameterUtil.FormatURL(uriSegment, parameter, addons))
            {
                Content = HttpRequestUtil.CreatePayload(parameter)
            };
#endif

            // add custom headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);
            response.EnsureSuccessStatusCode();
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            return result;

        }
        catch (Exception ex)
        {
            Log.Exception(_logger, "PATCH", ex);
            throw;
        }

    }

    /// <summary>
    /// Put method call that takes a body object
    /// </summary>
    /// <typeparam name="T">Class type of what return type is expected</typeparam>
    /// <param name="uriSegment">Uri for the api</param>   
    /// <returns>Instance of T</returns>
    public virtual async Task<T> PutAsync<S, T>(string uriSegment, S? parameter, CancellationTokenSource? cancellationToken = null,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(Constants.DefaultRESTTimeout);
            }

            // create request message and add custom query parameters
            var request = new HttpRequestMessage(HttpMethod.Put, QueryParameterUtil.FormatURL(uriSegment, parameter, addons))
            {
                Content = HttpRequestUtil.CreatePayload(parameter)
            };

            // add custom headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);
            response.EnsureSuccessStatusCode();
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            return result;
        }
        catch (Exception ex)
        {
            Log.Exception(_logger, "PUT", ex);
            throw;
        }
    }

    /// <summary>
    /// Delete Method for use with calls that do not expect a response
    /// </summary>
    /// <param name="uriSegment">Uri for the api including the query parameters</param> 
    public virtual async Task<T> DeleteAsync<T>(string uriSegment, CancellationTokenSource? cancellationToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(Constants.DefaultRESTTimeout);
            }

            // create request message and add custom query parameters
            var request = new HttpRequestMessage(HttpMethod.Delete, QueryParameterUtil.FormatURL(uriSegment, new NoopSchema(), addons));

            // add custom headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);
            response.EnsureSuccessStatusCode();
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            return result;
        }
        catch (Exception ex)
        {
            Log.Exception(_logger, "DELETE", ex);
            throw;
        }
    }

    /// <summary>
    /// Delete method that returns the type of response specified
    /// </summary>
    /// <typeparam name="T">Class Type of expected response</typeparam>
    /// <param name="uriSegment">Uri for the api including the query parameters</param>      
    /// <returns>Instance  of T or throws Exception</returns>
    public virtual async Task<T> DeleteAsync<S, T>(string uriSegment, S? parameter, CancellationTokenSource? cancellationToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(Constants.DefaultRESTTimeout);
            }

            // create request message and add custom query parameters
            var request = new HttpRequestMessage(HttpMethod.Delete, QueryParameterUtil.FormatURL(uriSegment, parameter, addons));

            // add custom headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);
            response.EnsureSuccessStatusCode();
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            return result;
        }
        catch (Exception ex)
        {
            Log.Exception(_logger, "DELETE ASYNC", ex);
            throw;
        }
    }

    internal static string GetUri(DeepgramHttpClientOptions options, string path)
    {
        return $"{options.BaseAddress}/{path}";
    }
}

