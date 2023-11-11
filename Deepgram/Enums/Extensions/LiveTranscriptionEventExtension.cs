namespace Deepgram.Enums.Extensions;

public static class LiveTranscriptionExtension
{
    public static string GetStringValue(LiveTranscriptionEvent value)
    {
        var name = Enum.GetName(typeof(LiveTranscriptionEvent), value);
        return name switch
        {
            "Open" => "open",
            "Close" => "close",
            "Transcript" => "Results", // exact match to data type from API
            "Metadata" => "Metadata", // exact match to data type from API
            "Error" => "error",
            "Warning" => "warning",
            _ => null,
        };
    }
}
