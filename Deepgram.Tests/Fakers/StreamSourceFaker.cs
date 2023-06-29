using System.IO;
using System.Text;
using AutoBogus;
using Deepgram.Models;
namespace Deepgram.Tests.Fakers
{
    public class StreamSourceFaker : AutoFaker<StreamSource>
    {
        public StreamSourceFaker()
        {
            RuleFor(p => p.Stream, (_, p) =>
             {
                 return new MemoryStream(Encoding.UTF8.GetBytes("TestStreamData"));
             });
            RuleFor(p => p.MimeType, f => f.System.MimeType());


        }
    }
}
