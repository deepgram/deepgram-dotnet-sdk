using System.Text;

var API_KEY = "YOUR_DEEPGRAM_API_KEY";

var deepgram = new DeepgramClient(new Credentials(API_KEY));

var response = await deepgram.Transcription.GetPrerecordedTranscriptionAsync(
    new Deepgram.Transcription.UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    new Deepgram.Transcription.PrerecordedTranscriptionOptions()
    {
        Punctuate = true,
        Utterances = true
    });

Console.WriteLine(response.ToWebVTT());

Console.WriteLine("Stream Sample\n");

using (FileStream fs = File.OpenRead("path\\to\\file"))
{
    var sResponse = await deepgram.Transcription.GetPrerecordedTranscriptionAsync(
        new Deepgram.Transcription.StreamSource(
            fs,
            "audio/wav"),
        new Deepgram.Transcription.PrerecordedTranscriptionOptions()
        {
            Punctuate = true,
            Utterances = true,
        });

    Console.WriteLine(response.ToSRT());
}
