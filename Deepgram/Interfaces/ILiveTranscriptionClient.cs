using Deepgram.CustomEventArgs;
using Deepgram.Models;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Deepgram.Interfaces
{
    public interface ILiveTranscriptionClient : IDisposable
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
        /// Fires if vad_events set to true and speech detected
        /// </summary>
        event EventHandler<SpeechStartedEventArgs> SpeechStarted;

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
        /// Signals to Deepgram that the audio has completed so it can return
        /// the final transcription output
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task FinishAsync();

        /// <summary>
        /// Closes the Web Socket connection to the Deepgram API
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task StopConnectionAsync();

        /// <summary>
        /// Keeps the connection alive even if no audio has been sent before the timeout
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        void KeepAlive();

        /// <summary>
        /// Sends audio data to the Deepgram API for transcription
        /// </summary>
        /// <param name="data">Byte array of the audio to be sent.</param>
        void SendData(byte[] data);
    }
}
