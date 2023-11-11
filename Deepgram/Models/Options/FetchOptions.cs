using Deepgram.Enums;

namespace Deepgram.Models.Options;
public class FetchOptions
{
    public Method? Method { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public Cache? Cache { get; set; }

    public Credentials? Credentials { get; set; }
    public Redirect? Redirect { get; set; }
    public ReferrerPolicy? ReferrerPolicy { get; set; }


}
