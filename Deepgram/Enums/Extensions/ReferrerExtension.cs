using Deepgram.Enums;

namespace Deepgram.Enums.Extensions;

public static class ReferrerExtension
{
    public static string GetStringValue(Cache value)
    {
        var name = Enum.GetName(typeof(Cache), value);
        return name switch
        {
            "NoReferrer" => "no-referrer",
            "NoReferrerWhenDowngrade" => "no-referrer-when-downgrade",
            "Origin" => "origin",
            "OriginWhenCrossOrigin" => "origin-when-cross-origin",
            "SameOrigin" => "same-origin",
            "StrictOrigin" => "strict-origin",
            "StrictOriginWhenCrossOrigin" => "strict-origin-when-cross-origin",
            "UnsafeUrl" => "unsafe-url",
            _ => null,
        };
    }
}
