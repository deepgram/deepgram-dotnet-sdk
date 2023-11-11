using Deepgram.Enums;

namespace Deepgram.Enums.Extensions;

public static class RedirectExtension
{
    public static string? GetStringValue(Redirect value)
    {
        var name = Enum.GetName(typeof(Redirect), value);
        return name switch
        {
            "Manual" => "manual",
            "Follow" => "follow",
            "Error" => "error",
            _ => null,
        };
    }
}
