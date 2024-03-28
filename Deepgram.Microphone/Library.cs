
namespace Deepgram.Microphone;

public class Library
{
    public static void Initialize()
    {
        // TODO: logging
        Console.WriteLine("Portaudio Version: {0}\n\n", PortAudio.VersionInfo.versionText);
        PortAudio.Initialize();
    }

    public static void Terminate()
    {
        // TODO: logging
        // Terminate PortAudio
        PortAudio.Terminate();
    }
}
