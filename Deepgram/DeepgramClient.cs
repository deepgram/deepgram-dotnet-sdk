using Deepgram.Clients;

namespace Deepgram;

public class DeepgramClient
{
    private CleanCredentials _credentials;
    internal ApiRequest _apiRequest;
    public IKeyClient Keys { get; internal set; }
    public IProjectClient Projects { get; internal set; }
    public ITranscriptionClient Transcription { get; internal set; }
    public IUsageClient Usage { get; internal set; }
    public IBillingClient Billing { get; internal set; }
    public IInvitationClient Invitation { get; internal set; }
    public ILiveTranscriptionClient CreateLiveTranscriptionClient() => new LiveTranscriptionClient(_credentials);

    public DeepgramClient(Credentials credentials) => Initialize(credentials);

    /// <summary>
    /// Sets the Timeout of the HTTPClient used to send HTTP requests
    /// </summary>
    /// <param name="timeout">Timespan to wait before the request times out.</param>
    public void SetHttpClientTimeout(TimeSpan timeout) =>
        HttpClientUtil.SetTimeOut(timeout);

    private void Initialize(Credentials credentials)
    {
        _credentials = CredentialsUtil.Clean(credentials);
        InitializeClients();
    }

    internal void InitializeClients()
    {
        _apiRequest = new ApiRequest(HttpClientUtil.HttpClient, _credentials);
        Keys = new KeyClient(_apiRequest);
        Projects = new ProjectClient(_apiRequest);
        Transcription = new TranscriptionClient(_apiRequest);
        Usage = new UsageClient(_apiRequest);
        Billing = new BillingClient(_apiRequest);
        Invitation = new InvitationClient(_apiRequest);
    }
}
