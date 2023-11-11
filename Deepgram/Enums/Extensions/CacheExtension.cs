using Deepgram.Enums;

namespace Deepgram.Enums.Extensions;

public static class CacheExtension
{
    public static string GetStringValue(Cache value)
    {
        var name = Enum.GetName(typeof(Cache), value);
        return name switch
        {
            "Default" => "default",
            "NoCache" => "no-cache",
            "Reload" => "reload",
            "ForceCache" => "force-cache",
            "OnlyIfCached" => "only-if-cached",
            _ => null,
        };
    }
}
