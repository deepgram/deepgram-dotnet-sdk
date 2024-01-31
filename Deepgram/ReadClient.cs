using Deepgram.Models.Shared.v1;

namespace Deepgram;

/// <summary>
///  Client containing methods for interacting with Read API's
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class ReadClient(string apiKey, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, deepgramClientOptions)
{
}
