using Deepgram.Models.Options;

namespace Deepgram.Tests.Fakes;
public class ClientOptionsFaker : AutoFaker<DeepgramClientOptions>
{
    public ClientOptionsFaker()
    {

        //RuleSet("All_Custom", rules =>
        //{
        //    RuleFor(o => o.Url, f => f.Internet.DomainName());
        //    RuleFor(o => o.Headers, f => FakeHeaders());
        //});

        RuleSet("Custom_Url_Only", rules =>
        {
            RuleFor(o => o.Url, f => "custom");
            RuleFor(o => o.Headers, f => null);
        });

        RuleSet("Custom_Headers_Only", rules =>
        {
            RuleFor(o => o.Url, f => null);
            RuleFor(o => o.Headers, f => null);
        });

        RuleSet("defaults", rules =>
        {
            RuleFor(o => o.Url, f => null);
            RuleFor(o => o.Headers, f => null);
        });
    }


}
