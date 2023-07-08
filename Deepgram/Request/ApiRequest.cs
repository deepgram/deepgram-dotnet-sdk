namespace Deepgram.Request;

public class ApiRequest
{
    readonly HttpClient _httpClient;
    readonly CleanCredentials _cleanCredentials;
    readonly RequestMessageBuilder _messageBuilder;
    internal ApiRequest(HttpClient httpClient, CleanCredentials credentials)
    {
        _httpClient = httpClient;
        _cleanCredentials = credentials;
        _messageBuilder = new RequestMessageBuilder();
    }

    internal HttpRequestMessage GetHttpMessage(HttpMethod method, string uri, object? body = null, object? queryParameters = null)
    {
        var source = body as StreamSource;
        if (source is StreamSource)
        {
            return _messageBuilder.CreateStreamHttpRequestMessage(method, uri, _cleanCredentials, source, queryParameters = null);
        }
        return _messageBuilder.CreateHttpRequestMessage(method, uri, _cleanCredentials, body, queryParameters = null);
    }

    internal virtual async Task<T> SendHttpRequestAsync<T>(HttpMethod method, string uri, object? body = null, object? queryParameters = null)
    {
        var request = GetHttpMessage(method, uri, body, queryParameters);
        var response = await _httpClient.SendAsync(request);

        var stream = await response.Content.ReadAsStreamAsync();
        string json;
        using (var sr = new StreamReader(stream))
        {
            json = await sr.ReadToEndAsync();
        }

        var deepgramResponse = ProcessResponse(response, json);

        return JsonConvert.DeserializeObject<T>(deepgramResponse.JsonResponse);
    }

    private static DeepgramResponse ProcessResponse(HttpResponseMessage response, string json)
    {
        var logger = Logger.LogProvider.GetLogger(typeof(ApiRequest).Name);
        try
        {
            logger.LogDebug(json);
            response.EnsureSuccessStatusCode();
            return new DeepgramResponse
            {
                Status = response.StatusCode,
                JsonResponse = json
            };
        }
        catch (HttpRequestException exception)
        {
            logger.LogError($"FAIL: {response.StatusCode}");
            throw new DeepgramHttpRequestException(exception.Message) { HttpStatusCode = response.StatusCode, Json = json };
        }
    }

}
