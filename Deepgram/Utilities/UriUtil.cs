namespace Deepgram.Utilities;

internal static class UriUtil
{
    /// <summary>
    /// create the Uri require for calling the api
    /// </summary>
    /// <param name="apiUrl"></param>
    /// <param name="uriSegment"></param>
    /// <param name="protocol"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    internal static Uri ResolveUri(string apiUrl, string uriSegment, string protocol, object? queryParameters = null)
    {
        var startUri = $@"{protocol}://{apiUrl}/v1/{uriSegment}";
        if (null != queryParameters)
        {
            var querystring = QueryParameterUtil.GetParameters(queryParameters);

            return new Uri($"{startUri}?{querystring}");
        }
        return new Uri(startUri);
    }
}
