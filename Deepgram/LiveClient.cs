using System.Net.WebSockets;
using Deepgram.Logger;

namespace Deepgram;

public class LiveClient(string? apiKey, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractWebSocketClient(apiKey, deepgramClientOptions)
{


    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task StartConnectionAsync(LiveSchema options)
    {
        _clientWebSocket?.Dispose();
        _clientWebSocket = new ClientWebSocket();
        _clientWebSocket = WssClientUtil.SetHeaders(_apiKey, _deepgramClientOptions, _clientWebSocket);
        _tokenSource = new CancellationTokenSource();

        _deepgramClientOptions = BaseAddressUtil.GetWss(_deepgramClientOptions);
        try
        {
            await _clientWebSocket.ConnectAsync(
                GetUri(options),
                CancellationToken.None).ConfigureAwait(false);
            StartSenderBackgroundThread();
            StartReceiverBackgroundThread();
            InvokeConnectionOpen(this);
            _disposed = false;
        }
        catch (Exception ex)
        {
            Log.WebSocketStartError(logger, ex);
            InvokeConnectionError(ex, this);
        }

        void StartSenderBackgroundThread() => _ = Task.Factory.StartNew(
            _ => ProcessSenderQueue(),
                TaskCreationOptions.LongRunning,
                _tokenSource.Token);

        void StartReceiverBackgroundThread() => _ = Task.Factory.StartNew(
                _ => Receive(),
            TaskCreationOptions.LongRunning,
            _tokenSource.Token);
    }


    /// <summary>
    /// Sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data">The data to be sent over the WebSocket.</param>
    public virtual void SendData(byte[] data) =>
        EnqueueForSending(new MessageToSend(data, WebSocketMessageType.Binary));


    // <summary>
    /// Signals to Deepgram that the audio has completed so it can return the final transcription output
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task FinishAsync()
    {
        if (_clientWebSocket!.State != WebSocketState.Open)
            return;

        await _clientWebSocket.SendAsync(
            new ArraySegment<byte>([]),
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None)
            .ConfigureAwait(false);
    }

    public void Dispose() => DisposeResources();

}
