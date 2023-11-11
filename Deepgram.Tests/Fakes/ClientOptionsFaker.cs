using Bogus;
using Deepgram.Models.Options;

namespace Deepgram.Tests.Fakes;
public class ClientOptionsFaker : AutoFaker<DeepgramClientOptions>
{
    public ClientOptionsFaker()
    {

        RuleSet("All", rules =>
        {
            RuleFor(o => o.Url, f => f.Internet.DomainName());
            RuleFor(o => o.Headers, f => FakeHeaders());
        });

        RuleSet("defaults", rules =>
        {
            RuleFor(o => o.Url, f => null);
            RuleFor(o => o.Headers, f => null);
        });
    }

    private static Dictionary<string, string> FakeHeaders()
    {
        var faker = new Faker();
        var headers = new Dictionary<string, string>();
        var headersCount = new Random().Next(1, 3);
        for (var i = 0; i < headersCount; i++)
        {
            headers.Add(faker.Random.Word(), faker.Random.Word());
        }

        return headers;
    }
}
