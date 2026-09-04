// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Encapsulations;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;

namespace Deepgram.Abstractions.v1;

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
            options = new DeepgramHttpClientOptions(apiKey);
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

            // add custom headers (names only are logged; values may be credentials)
            AddHeaders("GetAsync<T>", request, headers);

            // do the request
            Log.Verbose("GetAsync<T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("GetAsync<T>", response, resultStr);
            }

            LogResponse("GetAsync<T>", resultStr);
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
            LogException("GetAsync<T>", ex);
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

            // add custom headers (names only are logged; values may be credentials)
            AddHeaders("GetAsync<S, T>", request, headers);

            // do the request
            Log.Verbose("GetAsync<S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("GetAsync<S, T>", response, resultStr);
            }

            LogResponse("GetAsync<S, T>", resultStr);
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
            LogException("GetAsync<S, T>", ex);
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

            // add custom headers (names only are logged; values may be credentials)
            AddHeaders("PostRetrieveLocalFileAsync<R, S, T>", request, headers);

            // do the request
            Log.Verbose("PostRetrieveLocalFileAsync<R, S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("PostRetrieveLocalFileAsync<R, S, T>", response, response.Content.ReadAsStringAsync().Result);
            }

            var result = new Dictionary<string, string>();

            if (keys != null)
            {
                for (var i = 0; i < response.Headers.Count(); i++)
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

            var stream = new MemoryStream();
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
            LogException("PostRetrieveLocalFileAsync<R, S, T>", ex);
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

            // add custom headers (names only are logged; values may be credentials)
            AddHeaders("PostAsync<S, T>", request, headers);

            // do the request
            Log.Verbose("PostAsync<S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("PostAsync<S, T>", response, resultStr);
            }

            LogResponse("PostAsync<S, T>", resultStr);
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
            LogException("PostAsync<S, T>", ex);
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
                var stream = content as Stream;
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

            // add custom headers (names only are logged; values may be credentials)
            AddHeaders("PostAsync<R, S, T>", request, headers);

            // do the request
            Log.Verbose("PostAsync<R, S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("PostAsync<R, S, T>", response, resultStr);
            }

            LogResponse("PostAsync<R, S, T>", resultStr);
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
            LogException("PostAsync<R, S, T>", ex);
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

            // add custom headers (names only are logged; values may be credentials)
            AddHeaders("PatchAsync<S, T>", request, headers);

            // do the request
            Log.Verbose("PatchAsync<S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("PatchAsync<S, T>", response, resultStr);
            }

            LogResponse("PatchAsync<S, T>", resultStr);
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
            LogException("PatchAsync<S, T>", ex);
            Log.Verbose("AbstractRestClient.PatchAsync<S, T>", "LEAVE");
            throw;
        }
    }

    /// <summary>
    /// Patch method call with separate query-parameter and body objects. Unlike
    /// PatchAsync&lt;S, T&gt;, the body content is not repeated in the query string — use this
    /// when the body carries values that do not belong in (or are too large for) a URL.
    /// </summary>
    /// <typeparam name="R">Class type of the request body</typeparam>
    /// <typeparam name="S">Class type of the query-parameter schema</typeparam>
    /// <typeparam name="T">Class type of what return type is expected</typeparam>
    /// <param name="uriSegment">Uri for the api</param>
    /// <returns>Instance of T</returns>
    public virtual async Task<T> PatchAsync<R, S, T>(string uriSegment, S? parameter, R? content, CancellationTokenSource? cancellationToken = null,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
        => await PatchCoreAsync<R, S, T>(uriSegment, parameter, content, allowEmptyResponseBody: false, cancellationToken, addons, headers);

    /// <summary>
    /// PATCH for endpoints whose success contract permits an empty response body (currently only
    /// the Agent management variable update). An empty 200 yields the type's default value
    /// instead of a <see cref="JsonException"/>. Deliberately NOT an overload of
    /// <see cref="PatchAsync{R, S, T}"/>: a same-named overload with a <c>bool</c> in the
    /// cancellation slot makes existing positional-<c>default</c> calls ambiguous (CS0121).
    /// </summary>
    protected internal virtual Task<T> PatchAllowingEmptyResponseAsync<R, S, T>(string uriSegment, S? parameter, R? content,
        CancellationTokenSource? cancellationToken = null, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
        => PatchCoreAsync<R, S, T>(uriSegment, parameter, content, allowEmptyResponseBody: true, cancellationToken, addons, headers);

    private async Task<T> PatchCoreAsync<R, S, T>(string uriSegment, S? parameter, R? content, bool allowEmptyResponseBody,
        CancellationTokenSource? cancellationToken, Dictionary<string, string>? addons, Dictionary<string, string>? headers)
    {
        Log.Verbose("AbstractRestClient.PatchAsync<R, S, T>", "ENTER");
        Log.Debug("PatchAsync<R, S, T>", $"uriSegment: {uriSegment}");
        Log.Debug("PatchAsync<R, S, T>", $"addons: {addons}");

        try
        {
            // if not defined, use default timeout
            if (cancellationToken == null)
            {
                Log.Information("PatchAsync<R, S, T>", $"Using default timeout: {Constants.DefaultRESTTimeout}");
                cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(Constants.DefaultRESTTimeout);
            }

            // create request message and add custom query parameters
#if NETSTANDARD2_0
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), QueryParameterUtil.FormatURL(uriSegment, parameter, addons))
            {
                Content = HttpRequestUtil.CreatePayload(content)
            };
#else
            var request = new HttpRequestMessage(HttpMethod.Patch, QueryParameterUtil.FormatURL(uriSegment, parameter, addons))
            {
                Content = HttpRequestUtil.CreatePayload(content)
            };
#endif

            // add custom headers (names only are logged; values may be credentials)
            AddHeaders("PatchAsync<R, S, T>", request, headers);

            // do the request
            Log.Verbose("PatchAsync<R, S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("PatchAsync<R, S, T>", response, resultStr);
            }

            LogResponse("PatchAsync<R, S, T>", resultStr);
            var result = await HttpRequestUtil.DeserializeAsync<T>(response, allowEmptyResponseBody);

            Log.Debug("PatchAsync<R, S, T>", $"Succeeded");
            Log.Verbose("AbstractRestClient.PatchAsync<R, S, T>", "LEAVE");

            return result;

        }
        catch (OperationCanceledException ex)
        {
            Log.Information("PatchAsync<R, S, T>", "Task was cancelled.");
            Log.Verbose("PatchAsync<R, S, T>", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractRestClient.PatchAsync<R, S, T>", "LEAVE");
            throw;
        }
        catch (Exception ex)
        {
            LogException("PatchAsync<R, S, T>", ex);
            Log.Verbose("AbstractRestClient.PatchAsync<R, S, T>", "LEAVE");
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
        => await PutCoreAsync<S, T>(uriSegment, parameter, allowEmptyResponseBody: false, cancellationToken, addons, headers);

    /// <summary>
    /// PUT for endpoints whose success contract permits an empty response body (currently only
    /// the Agent management metadata update). An empty 200 yields the type's default value
    /// instead of a <see cref="JsonException"/>. Deliberately NOT an overload of
    /// <see cref="PutAsync{S, T}"/>: a same-named overload with a <c>bool</c> in the
    /// cancellation slot makes existing positional-<c>default</c> calls ambiguous (CS0121).
    /// </summary>
    protected internal virtual Task<T> PutAllowingEmptyResponseAsync<S, T>(string uriSegment, S? parameter,
        CancellationTokenSource? cancellationToken = null, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
        => PutCoreAsync<S, T>(uriSegment, parameter, allowEmptyResponseBody: true, cancellationToken, addons, headers);

    private async Task<T> PutCoreAsync<S, T>(string uriSegment, S? parameter, bool allowEmptyResponseBody,
        CancellationTokenSource? cancellationToken, Dictionary<string, string>? addons, Dictionary<string, string>? headers)
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

            // add custom headers (names only are logged; values may be credentials)
            AddHeaders("PutAsync<S, T>", request, headers);

            // do the request
            Log.Verbose("PutAsync<S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("PutAsync<S, T>", response, resultStr);
            }

            LogResponse("PutAsync<S, T>", resultStr);
            var result = await HttpRequestUtil.DeserializeAsync<T>(response, allowEmptyResponseBody);

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
            LogException("PutAsync<S, T>", ex);
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
        => await DeleteCoreAsync<T>(uriSegment, allowEmptyResponseBody: false, cancellationToken, addons, headers);

    /// <summary>
    /// DELETE for endpoints whose success contract permits an empty response body (currently
    /// only the Agent management delete operations). An empty 200 yields the type's default
    /// value instead of a <see cref="JsonException"/>. Deliberately NOT an overload of
    /// <see cref="DeleteAsync{T}"/>: a same-named overload with a <c>bool</c> in the
    /// cancellation slot makes existing positional-<c>default</c> calls ambiguous (CS0121).
    /// </summary>
    protected internal virtual Task<T> DeleteAllowingEmptyResponseAsync<T>(string uriSegment,
        CancellationTokenSource? cancellationToken = null, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
        => DeleteCoreAsync<T>(uriSegment, allowEmptyResponseBody: true, cancellationToken, addons, headers);

    private async Task<T> DeleteCoreAsync<T>(string uriSegment, bool allowEmptyResponseBody,
        CancellationTokenSource? cancellationToken, Dictionary<string, string>? addons, Dictionary<string, string>? headers)
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

            // add custom headers (names only are logged; values may be credentials)
            AddHeaders("DeleteAsync<T>", request, headers);

            // do the request
            Log.Verbose("DeleteAsync<T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("DeleteAsync<T>", response, resultStr);
            }

            LogResponse("DeleteAsync<T>", resultStr);
            var result = await HttpRequestUtil.DeserializeAsync<T>(response, allowEmptyResponseBody);

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
            LogException("DeleteAsync<T>", ex);
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

            // add custom headers (names only are logged; values may be credentials)
            AddHeaders("DeleteAsync<S, T>", request, headers);

            // do the request
            Log.Verbose("DeleteAsync<S, T>", "Calling _httpClient.SendAsync...");
            var response = await _httpClient.SendAsync(request, cancellationToken.Token);

            var resultStr = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                await ThrowException("DeleteAsync<S, T>", response, resultStr);
            }

            LogResponse("DeleteAsync<S, T>", resultStr);
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
            LogException("DeleteAsync<S, T>", ex);
            Log.Verbose("AbstractRestClient.DeleteAsync<S, T>", "LEAVE");
            throw;
        }
    }

    /// <summary>
    /// Whether successful response bodies are written to the log at Verbose. The default keeps
    /// the long-standing diagnostic behaviour for the transcription, synthesis, analysis and
    /// management clients. Clients whose responses carry customer data (e.g. Agent management:
    /// prompts, metadata, function-endpoint headers, variable values) override this to false so
    /// that data never reaches any SDK log level.
    /// </summary>
    protected virtual bool LogResponseBodies => true;

    /// <summary>
    /// Adds caller-supplied headers to the request and logs only the header NAMES at Debug.
    /// Header values are never logged at any level: they can carry Authorization tokens or
    /// other credentials.
    /// </summary>
    protected static void AddHeaders(string module, HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var header in headers)
        {
            Log.Debug(module, $"Add Header {header.Key}");
            request.Headers.Add(header.Key, header.Value);
        }
    }

    /// <summary>
    /// Logs a successful response body at Verbose when <see cref="LogResponseBodies"/> is
    /// enabled; otherwise logs only its size so the trace still shows the call completed.
    /// </summary>
    protected void LogResponse(string module, string body)
    {
        if (LogResponseBodies)
        {
            Log.Verbose(module, $"Response:\n{body}");
        }
        else
        {
            Log.Verbose(module, $"Response: {body.Length} bytes (body logging disabled for this client)");
        }
    }

    /// <summary>
    /// Logs a non-success response body at Verbose when <see cref="LogResponseBodies"/> is
    /// enabled; otherwise only its size. Error bodies can echo back submitted data (validation
    /// messages quote the offending field), so they follow the same gate as success bodies.
    /// </summary>
    protected void LogErrorResponse(string module, string body)
    {
        if (LogResponseBodies)
        {
            Log.Verbose(module, $"Deepgram Exception: {body}");
        }
        else
        {
            Log.Verbose(module, $"Deepgram Exception: {body.Length} bytes (body logging disabled for this client)");
        }
    }

    /// <summary>
    /// Logs a failed request. With body logging enabled this is the long-standing behaviour: type
    /// and message at Error, the full exception at Verbose. With it disabled, only the exception
    /// type plus the API error code and request id are logged — never the message, which for
    /// <see cref="DeepgramException"/> carries the response body.
    /// </summary>
    protected void LogException(string module, Exception ex)
    {
        if (LogResponseBodies)
        {
            Log.Error(module, $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose(module, $"Exception: {ex}");
            return;
        }

        var detail = ex is DeepgramException dg ? $" (err_code: {dg.ErrCode}, request_id: {dg.RequestId})" : "";
        Log.Error(module, $"{ex.GetType()} thrown{detail}; message suppressed (body logging disabled for this client)");
    }

    private async Task ThrowException(string module, HttpResponseMessage response, string errMsg)
    {
        if (errMsg == null || errMsg.Length == 0)
        {
            Log.Verbose(module, $"HTTP/REST Exception thrown");
            response.EnsureSuccessStatusCode(); // this throws the exception
        }

        LogErrorResponse(module, errMsg);
        DeepgramRESTException? resException = null;
        try
        {
            resException = await HttpRequestUtil.DeserializeAsync<DeepgramRESTException>(response);
        }
        catch (Exception ex)
        {
            Log.Verbose(module, LogResponseBodies ? $"DeserializeAsync Error Exception: {ex}" : $"DeserializeAsync Error Exception: {ex.GetType().Name} (details suppressed for this client)");
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

