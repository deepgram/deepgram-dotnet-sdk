//using Bogus;
//using Deepgram.Models;

//namespace Deepgram.Tests.Fakers
//{
//    public class ApiKeyFaker : Faker<ApiKey>
//    {
//        public ApiKeyFaker()
//        {
//            RuleFor(a => a.Id, f => f.Random.Guid().ToString());
//            RuleFor(a => a.Comment, f => f.Lorem.Sentence());
//            RuleFor(a => a.Created, f => f.Date.Recent());
//            RuleFor(a => a.Scopes, f => f.Random.WordsArray(1, 4));
//            RuleFor(a => a.Tags, f => f.Random.WordsArray(1, 4));
//            RuleFor(a => a.ExpirationDate, f => f.Date.Recent());
//        }
//    }
//}
