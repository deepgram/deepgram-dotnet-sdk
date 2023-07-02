namespace Deepgram.Tests.Fakers;

public class CleanCredentialsFaker : AutoFaker<CleanCredentials>
{
    public CleanCredentialsFaker()
    {
        RuleFor(c => c.ApiKey, f => f.Random.Guid().ToString());
        RuleFor(c => c.ApiUrl, f => f.Internet.DomainName());
        RuleFor(c => c.RequireSSL, f => f.Random.Bool());
    }
}
