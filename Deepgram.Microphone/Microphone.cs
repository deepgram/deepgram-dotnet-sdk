// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace Deepgram.Microphone;

/// <summary>
/// Implements a Microphone using PortAudio
/// </summary>
public class Microphone
{
    private Action<byte[]> _push_callback;

    private int _rate;
    private uint _chunk;
    private int _channels;
    private int _device_index;
    private SampleFormat _format;
    private bool _isMuted = false;

    private PortAudioSharp.Stream? _stream = null;
    private CancellationTokenSource? _exitToken = null;

    /// <summary>
    /// Constructor for Microphone
    /// </summary>
    public Microphone(
        Action<byte[]> push_callback,
        int rate = Defaults.RATE,
        uint chunkSize = Defaults.CHUNK_SIZE,
        int channels = Defaults.CHANNELS,
        int device_index = Defaults.DEVICE_INDEX,
        SampleFormat format = Defaults.SAMPLE_FORMAT
    )
    {
        _push_callback = push_callback;
        _rate = rate;
        _chunk = chunkSize;
        _channels = channels;
        _device_index = device_index;
        _format = format;
    }

    // Start begins the listening on the microphone
    public bool Start()
    {
        if (_stream != null)
        {
            return false;
        }

        // reset exit token
        _exitToken = new CancellationTokenSource();

        // Get the device info
        if (_device_index == Defaults.DEVICE_INDEX)
        {
            _device_index = PortAudio.DefaultInputDevice;
            if (_device_index == PortAudio.NoDevice)
            {
                return false;
            }
        }

        DeviceInfo info = PortAudio.GetDeviceInfo(_device_index);

        // Set the stream parameters
        StreamParameters param = new StreamParameters();
        param.device = _device_index;
        param.channelCount = _channels;
        param.sampleFormat = _format;
        param.suggestedLatency = info.defaultLowInputLatency;
        param.hostApiSpecificStreamInfo = IntPtr.Zero;

        // Set the callback
        PortAudioSharp.Stream.Callback callback = _callback;

        // Create the stream
        _stream = new PortAudioSharp.Stream(
            inParams: param,
            outParams: null,
            sampleRate: _rate,
            framesPerBuffer: _chunk,
            streamFlags: StreamFlags.ClipOff,
            callback: _callback,
            userData: IntPtr.Zero
            );

        // Start the stream
        _stream.Start();
        return true;
    }

    private StreamCallbackResult _callback(nint input, nint output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, nint userDataPtr)
    {
        // Check if the input is null
        if (input == IntPtr.Zero)
        {
            return StreamCallbackResult.Continue;
        }

        // Check if the exit token is set
        if (_exitToken != null && _exitToken.IsCancellationRequested)
        {
            return StreamCallbackResult.Abort;
        }

        // copy and send the data
        byte[] buf = new byte[frameCount * sizeof(Int16)];

        if (_isMuted)
        {
            buf = new byte[buf.Length];
        }
        else
        {
            Marshal.Copy(input, buf, 0, buf.Length);
        }

        // Push the data to the callback
        _push_callback(buf);

        return StreamCallbackResult.Continue;
    }

    public void Mute()
    {
        if (_stream != null)
        {
            return;
        }

        _isMuted = true;
    }

    public void Unmute()
    {
        if (_stream != null)
        {
            return;
        }

        _isMuted = false;
    }

    public void Stop()
    {
        // Check if we have a stream
        if (_stream == null)
        {
            return;
        }

        // signal stop
        if (_exitToken != null)
        {
            _exitToken.Cancel();
        }

        // Stop the stream
        _stream.Stop();
    }
}
