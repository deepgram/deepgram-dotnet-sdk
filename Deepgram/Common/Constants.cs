namespace Deepgram.Common
{
    public static class Constants
    {

        public const string DEFAULT_URI = "https://api.deepgram.com";
        public const string API_VERSION = "v1";
        public const string APIKEY_ENVIRONMENT_NAME = "DEEPGRAM_API_KEY";
        public const string HTTPCLIENT_NAME = "DEEPGRAM_HTTP_CLIENT";
        public const string DEFAULT_CONTENT_TYPE = "application/json";
        public const string DEEPGRAM_CONTENT_TYPE = "deepgram/audio+video";

        #region URISegments
        //using constants instead of inline value(magic strings) make consistence
        //across SDK And Test Projects Simpler and Easier to change
        public const string PROJECTS = "projects";
        public const string BILLING = "billing";
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
        #endregion
    }
}
