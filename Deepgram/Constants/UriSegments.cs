namespace Deepgram.Constants;
public static class UriSegments
{

    //using constants instead of inline value(magic strings) make consistence
    //across SDK And Test Projects Simpler and Easier to change
    public const string PROJECTS = "projects";
    public const string BALANCES = "balances";
    public const string USAGE = "usage";
    public const string MEMBERS = "members";
    public const string KEYS = "keys";
    public const string INVITES = "invites";
    public const string SCOPES = "scopes";
    public const string REQUESTS = "requests";
    public const string LISTEN = "listen";
    public const string ONPREM = "onprem/distribution/credentials";
    public const string TRANSCRIPTION = "listen";
}
