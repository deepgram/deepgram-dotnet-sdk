using System;
using Deepgram.Common;
using Deepgram.Models;

namespace Deepgram.Extensions
{
    internal static class UriExtension
    {
        internal static Uri ResolveUri(Credentials credentials, string uriSegment, string protocol, object queryParameters = null)
        {
            var startUri = $"{protocol}://{credentials.ApiUrl}/v1{uriSegment}";
            if (null != queryParameters)
            {
                var querystring = Helpers.GetParameters(queryParameters);
                return new Uri($"{startUri}?{querystring}");
            }
            return new Uri(startUri);
        }
    }
}
