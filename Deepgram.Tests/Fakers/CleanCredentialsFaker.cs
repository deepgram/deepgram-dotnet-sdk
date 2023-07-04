using AutoBogus;
using Deepgram.Request;

namespace Deepgram.Tests.Fakers
{

    internal class CleanCredentialsFaker : AutoFaker<CleanCredentials>
    {
        public CleanCredentialsFaker()
        {
            RuleFor(c => c.ApiKey, f => f.Random.Guid().ToString());
            RuleFor(c => c.ApiUrl, f => f.Internet.DomainName());
            RuleFor(c => c.RequireSSL, f => f.Random.Bool());
        }
    }
}
