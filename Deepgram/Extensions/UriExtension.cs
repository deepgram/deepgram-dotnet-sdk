using System;
using Deepgram.Common;

namespace Deepgram.Extensions
{
    internal static class UriExtension
    {
        internal static Uri ResolveUri(string apiUrl, string uriSegment, string protocol, object queryParameters = null)
        {
            var startUri = $@"{protocol}://{apiUrl}/v1/{uriSegment}";
            if (null != queryParameters)
            {
                var querystring = Helpers.GetParameters(queryParameters);

                return new Uri($"{startUri}?{querystring}");
            }
            return new Uri(startUri);
        }
    }
}
