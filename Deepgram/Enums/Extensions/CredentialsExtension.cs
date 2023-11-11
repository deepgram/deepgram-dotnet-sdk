using Deepgram.Enums;

namespace Deepgram.Enums.Extensions;

public static class CredentialsExtension
{
    public static string GetStringValue(Credentials value)
    {
        var name = Enum.GetName(typeof(Credentials), value);
        return name switch
        {
            "Include" => "include",
            "SameOrigin" => "same-origin",
            "Omit" => "omit",
            _ => null,
        };
    }
}
