using System;

namespace Deepgram.Utillities
{
    internal static class UriUtil
    {
        internal static Uri ResolveUri(string apiUrl, string uriSegment, string protocol, object queryParameters = null)
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
}
