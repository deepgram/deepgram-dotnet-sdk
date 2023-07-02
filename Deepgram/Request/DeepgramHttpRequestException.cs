using System;
using System.Net;
using System.Net.Http;

namespace Deepgram.Request;

public class DeepgramHttpRequestException : HttpRequestException
{
    public DeepgramHttpRequestException() { }
    public DeepgramHttpRequestException(string message) : base(message) { }
    public DeepgramHttpRequestException(string message, Exception inner) : base(message, inner) { }
    public string Json { get; set; }
    public HttpStatusCode HttpStatusCode { get; set; }
}
