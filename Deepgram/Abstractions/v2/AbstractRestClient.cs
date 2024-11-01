// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Encapsulations;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;

namespace Deepgram.Abstractions.v2;

public abstract class AbstractRestClient
{
    /// <summary>
    ///  HttpClient created by the factory
    internal HttpClient _httpClient;

    /// <summary>
    /// Copy of the options for the client
    /// </summary>
    internal IDeepgramClientOptions _options;

    /// <summary>
    /// Constructor that take the options and a httpClient
    /// </summary>
    /// <param name="deepgramClientOptions"><see cref="_deepgramClientOptions"/>Options for the Deepgram client</param>

    internal AbstractRestClient(string? apiKey = null, IDeepgramClientOptions? options = null, string? httpId = null)
    {
        Log.Verbose("AbstractRestClient", "ENTER");

        if (options == null)
        {
            options = (IDeepgramClientOptions) new DeepgramHttpClientOptions(apiKey);
        }
        _httpClient = HttpClientFactory.Create(httpId);
        _httpClient = HttpClientFactory.ConfigureDeepgram(_httpClient, options);
        _options = options;

        Log.Debug("AbstractRestClient", $"APIVersion: {options.APIVersion}");
        Log.Debug("AbstractRestClient", $"BaseAddress: {options.BaseAddress}");
        Log.Debug("AbstractRestClient", $"options: {options.OnPrem}");
        Log.Verbose("AbstractRestClient", "LEAVE");
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
        Log.Verbose("AbstractRestClient.GetAsync<T>", "ENTER");
        Log.Debug("GetAsync<T>", $"uriSegment: {uriSegment}");
        Log.Debug("GetAsync<T>", $"addons: {addons}");

        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                Log.Information("GetAsync", $"Using default timeout: {Constants.DefaultRESTTimeout}");
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
                    Log.Debug("GetAsync<T>", $"Add Header {header.Key}={header.Value}");
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            Log.Verbose("GetAsync<T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("GetAsync<T>", response, resultStr);
            }

            Log.Verbose("GetAsync<T>", $"Response:\n{resultStr}");
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            Log.Debug("GetAsync<T>", "Succeeded");
            Log.Verbose("AbstractRestClient.GetAsync<T>", "LEAVE");
            return result;
        }
        catch (OperationCanceledException ex)
        {
            Log.Information("GetAsync<T>", "Task was cancelled.");
            Log.Verbose("GetAsync<T>", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractRestClient.GetAsync<T>", "LEAVE");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("GetAsync<T>", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("GetAsync<T>", $"Excepton: {ex}");
            Log.Verbose("AbstractRestClient.GetAsync<T>", "LEAVE");
            throw;
        }
    }

    public virtual async Task<T> GetAsync<S, T>(string uriSegment, S? parameter, CancellationTokenSource? cancellationToken = null,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AbstractRestClient.GetAsync<S, T>", "ENTER");
        Log.Debug("GetAsync<S, T>", $"uriSegment: {uriSegment}");
        Log.Debug("GetAsync<S, T>", $"addons: {addons}");

        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                Log.Information("GetAsync<S, T>", $"Using default timeout: {Constants.DefaultRESTTimeout}");
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
                    Log.Debug("GetAsync<S, T>", $"Add Header {header.Key}={header.Value}");
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            Log.Verbose("GetAsync<S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("GetAsync<S, T>", response, resultStr);
            }

            Log.Verbose("GetAsync<S, T>", $"Response:\n{resultStr}");
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            Log.Debug("GetAsync<S, T>", "Succeeded");
            Log.Verbose("AbstractRestClient.GetAsync<S, T>", "LEAVE");

            return result;
        }
        catch (OperationCanceledException ex)
        {
            Log.Information("GetAsync<S, T>", "Task was cancelled.");
            Log.Verbose("GetAsync<S, T>", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractRestClient.GetAsync<S, T>", "LEAVE");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("GetAsync<S, T>", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("GetAsync<S, T>", $"Excepton: {ex}");
            Log.Verbose("AbstractRestClient.GetAsync<S, T>", "LEAVE");
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
        Log.Verbose("AbstractRestClient.PostRetrieveLocalFileAsync<R, S, T>", "ENTER");
        Log.Debug("PostRetrieveLocalFileAsync<R, S, T>", $"uriSegment: {uriSegment}");
        Log.Debug("PostRetrieveLocalFileAsync<R, S, T>", $"keys: {keys}");
        Log.Debug("PostRetrieveLocalFileAsync<R, S, T>", $"addons: {addons}");

        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                Log.Information("PostRetrieveLocalFileAsync<R, S, T>", $"Using default timeout: {Constants.DefaultRESTTimeout}");
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
                    Log.Debug("PostRetrieveLocalFileAsync<R, S, T>", $"Add Header {header.Key}={header.Value}");
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            Log.Verbose("PostRetrieveLocalFileAsync<R, S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            if (!response.IsSuccessStatusCode)
            {
                var resultStr = await response.Content.ReadAsStringAsync();
                await ThrowException("PostRetrieveLocalFileAsync<R, S, T>", response, resultStr);
            }

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

            Log.Verbose("PostRetrieveLocalFileAsync<R, S, T>", "Succeeded");
            Log.Verbose("AbstractRestClient.PostRetrieveLocalFileAsync<R, S, T>", "LEAVE");

            return new LocalFileWithMetadata()
                        {
                            Metadata = result,
                            Content = stream,
                        };

        }
        catch (OperationCanceledException ex)
        {
            Log.Information("PostRetrieveLocalFileAsync<R, S, T>", "Task was cancelled.");
            Log.Verbose("PostRetrieveLocalFileAsync<R, S, T>", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractRestClient.PostRetrieveLocalFileAsync<R, S, T>", "LEAVE");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("PostRetrieveLocalFileAsync<R, S, T>", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("PostRetrieveLocalFileAsync<R, S, T>", $"Excepton: {ex}");
            Log.Verbose("AbstractRestClient.PostRetrieveLocalFileAsync<R, S, T>", "LEAVE");
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
        Log.Verbose("AbstractRestClient.PostAsync<S, T>", "ENTER");
        Log.Debug("PostAsync<S, T>", $"uriSegment: {uriSegment}");
        Log.Debug("PostAsync<S, T>", $"addons: {addons}");

        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                Log.Information("PostAsync<S, T>", $"Using default timeout: {Constants.DefaultRESTTimeout}");
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
                    Log.Debug("PostAsync<S, T>", $"Add Header {header.Key}={header.Value}");
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            Log.Verbose("PostAsync<S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("PostAsync<S, T>", response, resultStr);
            }

            Log.Verbose("PostAsync<S, T>", $"Response:\n{resultStr}");
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            Log.Debug("PostAsync<S, T>", $"Succeeded");
            Log.Verbose("AbstractRestClient.PostAsync<S, T>", "LEAVE");

            return result;
        }
        catch (OperationCanceledException ex)
        {
            Log.Information("PostAsync<S, T>", "Task was cancelled.");
            Log.Verbose("PostAsync<S, T>", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractRestClient.PostAsync<S, T>", "LEAVE");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("PostAsync<S, T>", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("PostAsync<S, T>", $"Excepton: {ex}");
            Log.Verbose("AbstractRestClient.PostAsync<S, T>", "LEAVE");
            throw;
        }
    }

    public virtual async Task<T> PostAsync<R, S, T>(string uriSegment, S? parameter, R? content, CancellationTokenSource? cancellationToken = null,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AbstractRestClient.PostAsync<R, S, T>", "ENTER");
        Log.Debug("PostAsync<S, T>", $"uriSegment: {uriSegment}");
        Log.Debug("PostAsync<R, S, T>", $"addons: {addons}");

        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                Log.Information("PostAsync<R, S, T>", $"Using default timeout: {Constants.DefaultRESTTimeout}");
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
                    Log.Debug("PostAsync<R, S, T>", $"Add Header {header.Key}={header.Value}");
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            Log.Verbose("PostAsync<R, S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("PostAsync<R, S, T>", response, resultStr);
            }

            Log.Verbose("PostAsync<R, S, T>", $"Response:\n{resultStr}");
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            Log.Debug("PostAsync<R, S, T>", $"Succeeded");
            Log.Verbose("AbstractRestClient.PostAsync<R, S, T>", "LEAVE");

            return result;
        }
        catch (OperationCanceledException ex)
        {
            Log.Information("PostAsync<R, S, T>", "Task was cancelled.");
            Log.Verbose("PostAsync<R, S, T>", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractRestClient.PostAsync<R, S, T>", "LEAVE");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("PostAsync<R, S, T>", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("PostAsync<R, S, T>", $"Excepton: {ex}");
            Log.Verbose("AbstractRestClient.PostAsync<R, S, T>", "LEAVE");
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
        Log.Verbose("AbstractRestClient.PatchAsync<S, T>", "ENTER");
        Log.Debug("PatchAsync<S, T>", $"uriSegment: {uriSegment}");
        Log.Debug("PatchAsync<S, T>", $"addons: {addons}");

        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                Log.Information("PatchAsync<S, T>", $"Using default timeout: {Constants.DefaultRESTTimeout}");
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
                    Log.Debug("PatchAsync<S, T>", $"Add Header {header.Key}={header.Value}");
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            Log.Verbose("PatchAsync<S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("PatchAsync<S, T>", response, resultStr);
            }

            Log.Verbose("PatchAsync<S, T>", $"Response:\n{resultStr}");
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            Log.Debug("PatchAsync<S, T>", $"Succeeded");
            Log.Verbose("AbstractRestClient.PatchAsync<S, T>", "LEAVE");

            return result;

        }
        catch (OperationCanceledException ex)
        {
            Log.Information("PatchAsync<S, T>", "Task was cancelled.");
            Log.Verbose("PatchAsync<S, T>", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractRestClient.PatchAsync<S, T>", "LEAVE");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("PatchAsync<S, T>", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("PatchAsync<S, T>", $"Excepton: {ex}");
            Log.Verbose("AbstractRestClient.PatchAsync<S, T>", "LEAVE");
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
        Log.Verbose("AbstractRestClient.PutAsync<S, T>", "ENTER");
        Log.Debug("PutAsync<S, T>", $"uriSegment: {uriSegment}");
        Log.Debug("PutAsync<S, T>", $"addons: {addons}");

        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                Log.Information("PutAsync<S, T>", $"Using default timeout: {Constants.DefaultRESTTimeout}");
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
                    var tmp = header.Key.ToLower();
                    if ( !(tmp.Contains("password") || tmp.Contains("token") || tmp.Contains("authorization") || tmp.Contains("auth")) )
                    {
                        Log.Debug("PutAsync<S, T>", $"Add Header {header.Key}={header.Value}");
                    }
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            Log.Verbose("PutAsync<S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("PutAsync<S, T>", response, resultStr);
            }

            Log.Verbose("PutAsync<S, T>", $"Response:\n{resultStr}");
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            Log.Debug("PutAsync<S, T>", $"Succeeded");
            Log.Verbose("AbstractRestClient.PutAsync<S, T>", "LEAVE");

            return result;
        }
        catch (OperationCanceledException ex)
        {
            Log.Information("PutAsync<S, T>", "Task was cancelled.");
            Log.Verbose("PutAsync<S, T>", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractRestClient.PutAsync<S, T>", "LEAVE");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("PutAsync<S, T>", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("PutAsync<S, T>", $"Excepton: {ex}");
            Log.Verbose("AbstractRestClient.PutAsync<S, T>", "LEAVE");
            throw;
        }
    }

    /// <summary>
    /// Delete Method for use with calls that do not expect a response
    /// </summary>
    /// <param name="uriSegment">Uri for the api including the query parameters</param> 
    public virtual async Task<T> DeleteAsync<T>(string uriSegment, CancellationTokenSource? cancellationToken = null,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AbstractRestClient.DeleteAsync<T>", "ENTER");
        Log.Debug("DeleteAsync<T>", $"uriSegment: {uriSegment}");
        Log.Debug("DeleteAsync<T>", $"addons: {addons}");

        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                Log.Information("DeleteAsync<T>", $"Using default timeout: {Constants.DefaultRESTTimeout}");
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
                    Log.Debug("DeleteAsync<T>", $"Add Header {header.Key}={header.Value}");
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            Log.Verbose("DeleteAsync<T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("DeleteAsync<T>", response, resultStr);
            }

            Log.Verbose("DeleteAsync<T>", $"Response:\n{resultStr}");
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            Log.Debug("DeleteAsync<T>", $"Succeeded");
            Log.Verbose("AbstractRestClient.DeleteAsync<T>", "LEAVE");

            return result;
        }
        catch (OperationCanceledException ex)
        {
            Log.Information("DeleteAsync<T>", "Task was cancelled.");
            Log.Verbose("DeleteAsync<T>", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractRestClient.DeleteAsync<T>", "LEAVE");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("DeleteAsync<T>", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("DeleteAsync<T>", $"Excepton: {ex}");
            Log.Verbose("AbstractRestClient.DeleteAsync<T>", "LEAVE");
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
        Log.Verbose("AbstractRestClient.DeleteAsync<S, T>", "ENTER");
        Log.Debug("DeleteAsync<S, T>", $"uriSegment: {uriSegment}");
        Log.Debug("DeleteAsync<S, T>", $"addons: {addons}");

        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                Log.Information("DeleteAsync<S, T>", $"Using default timeout: {Constants.DefaultRESTTimeout}");
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
                    Log.Debug("DeleteAsync<S, T>", $"Add Header {header.Key}={header.Value}");
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // do the request
            Log.Verbose("DeleteAsync<S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("DeleteAsync<S, T>", response, resultStr);
            }

            Log.Verbose("DeleteAsync<S, T>", $"Response:\n{resultStr}");
            var result = await HttpRequestUtil.DeserializeAsync<T>(response);

            Log.Debug("DeleteAsync<S, T>", $"Succeeded");
            Log.Verbose("AbstractRestClient.DeleteAsync<S, T>", "LEAVE");

            return result;
        }
        catch (OperationCanceledException ex)
        {
            Log.Information("DeleteAsync<S, T>", "Task was cancelled.");
            Log.Verbose("DeleteAsync<S, T>", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractRestClient.DeleteAsync<S, T>", "LEAVE");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("DeleteAsync<S, T>", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("DeleteAsync<S, T>", $"Excepton: {ex}");
            Log.Verbose("AbstractRestClient.DeleteAsync<S, T>", "LEAVE");
            throw;
        }
    }

    private static async Task ThrowException(string module, HttpResponseMessage response, string errMsg)
    {
        if (errMsg == null || errMsg.Length == 0)
        {
            Log.Verbose(module, $"HTTP/REST Exception thrown");
            response.EnsureSuccessStatusCode(); // this throws the exception
        }

        Log.Verbose(module, $"Deepgram Exception: {errMsg}");
        DeepgramRESTException? resException = null;
        try
        {
            resException = await HttpRequestUtil.DeserializeAsync<DeepgramRESTException>(response);
        }
        catch (Exception ex)
        {
            Log.Verbose(module, $"DeserializeAsync Error Exception: {ex}");
        }

        if (resException != null)
        {
            Log.Verbose(module, "DeepgramRESTException thrown");
            throw resException;
        }

        Log.Verbose(module, $"Deepgram Generic Exception thrown");
        throw new DeepgramException(errMsg);
    }

    internal static string GetUri(IDeepgramClientOptions options, string path)
    {
        return $"{options.BaseAddress}/{path}";
    }
}

