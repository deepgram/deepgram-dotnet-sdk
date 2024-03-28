namespace Deepgram.Microphone;

public static class Defaults
{
    // Default sample rate
    public const int RATE = 16000;

    // Number of channels
    public const int CHANNELS = 1;

    // Default chunk size
    public const int CHUNK_SIZE = 8194;

    // Default input device index
    public const SampleFormat SAMPLE_FORMAT = SampleFormat.Int16;

    // Default input device index
    public const int DEVICE_INDEX = PortAudio.NoDevice;
}

