internal static class RequestContentUtil
{
    static ILogger logger => LogProvider.GetLogger(nameof(RequestContentUtil));
    static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    /// <summary>
    /// Create the body payload of a HttpRequestMessage
    /// </summary>
    /// <typeparam name="T">Type of the body to be sent</typeparam>
    /// <param name="body">instance value for the body</param>
    /// <param name="contentType">What type of content is being sent default is : application/json</param>
    /// <returns></returns>
    internal static StringContent CreatePayload<T>(T body) => new StringContent(
                                JsonSerializer.Serialize(body, _jsonSerializerOptions),
                               Encoding.UTF8,
                               Constants.DEFAULT_CONTENT_TYPE);


    /// <summary>
    /// Create the stream payload of a HttpRequestMessage
    /// </summary>
    /// <param name="body">of type stream</param>
    /// <returns>HttpContent</returns>
    internal static HttpContent CreateStreamPayload(Stream body)
    {
        body.Seek(0, SeekOrigin.Begin);
        HttpContent httpContent = new StreamContent(body);
        httpContent.Headers.Add("Content-Type", Constants.DEEPGRAM_CONTENT_TYPE);
        httpContent.Headers.Add("Content-Length", body.Length.ToString());
        return httpContent;
    }


    /// <summary>
    /// method that deserializes DeepgramResponse and performs null checks on values
    /// </summary>
    /// <typeparam name="TResponse">Class Type of expected response</typeparam>
    /// <param name="httpResponseMessage">Http Response to be deserialized</param>       
    /// <returns>instance of TResponse or a Exception</returns>
    internal static async Task<TResponse> DeserializeAsync<TResponse>(HttpResponseMessage httpResponseMessage)
    {
        var content = await httpResponseMessage.Content.ReadAsStringAsync();
        var deepgramResponse = JsonSerializer.Deserialize<TResponse>(content);
        return deepgramResponse;
    }


    /// <summary>
    /// method that deserializes DeepgramResponse and performs null checks on values
    /// </summary>
    /// <typeparam name="TResponse">Class Type of expected response</typeparam>
    /// <param name="httpResponseMessage">Http Response to be deserialized</param>       
    /// <returns>instance of TResponse or a Exception</returns>
    internal static TResponse Deserialize<TResponse>(string content) =>
        JsonSerializer.Deserialize<TResponse>(content, _jsonSerializerOptions);
}
