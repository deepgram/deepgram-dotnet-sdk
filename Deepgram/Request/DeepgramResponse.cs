using System;
using System.Net;

namespace Deepgram.Request
{
    internal class DeepgramResponse
    {
        public HttpStatusCode Status { get; set; }
        public string JsonResponse { get; set; }
    }
}
