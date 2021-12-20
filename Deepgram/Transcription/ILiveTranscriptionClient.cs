using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Deepgram.Transcription
{
    public interface ILiveTranscriptionClient: IDisposable
    {
        /// <summary>
        /// Fires when the WebSocket connection to Deepgram has been opened
        /// </summary>
        event EventHandler<ConnectionOpenEventArgs> ConnectionOpened;

        /// <summary>
        /// Fires on any error in the connection, sending or receiving
        /// </summary>
        event EventHandler<ConnectionErrorEventArgs> ConnectionError;

        /// <summary>
        /// Fires when the WebSocket connection is closed
        /// </summary>
        event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;

        /// <summary>
        /// Fires when a transcript is received from the Deepgram API
        /// </summary>
        event EventHandler<TranscriptReceivedEventArgs> TranscriptReceived;

        /// <summary>
        /// Retrieves the connection state of the WebSocket
        /// </summary>
        /// <returns>Returns the connection state of the WebSocket</returns>
        WebSocketState State();

        /// <summary>
        /// Connect to a Deepgram API Web Socket to begin transcribing audio
        /// </summary>
        /// <param name="options">Options to use when transcribing audio</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task StartConnectionAsync(LiveTranscriptionOptions options);

        /// <summary>
        /// Closes the Web Socket connection to the Deepgram API
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task StopConnectionAsync();

        /// <summary>
        /// Sends audio data to the Deepgram API for transcription
        /// </summary>
        /// <param name="data">Byte array of the audio to be sent.</param>
        void SendData(byte[] data);
    }
}
