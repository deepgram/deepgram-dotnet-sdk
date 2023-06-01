using System;
using System.Threading.Tasks;

namespace Deepgram.Interfaces
{
    public interface ITranscriptionClient
    {
        IPrerecordedTranscriptionClient Prerecorded { get; }
    }
}
