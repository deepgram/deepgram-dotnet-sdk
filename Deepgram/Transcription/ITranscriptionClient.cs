using System;
using System.Threading.Tasks;

namespace Deepgram.Transcription
{
    public interface ITranscriptionClient
    {
        IPrerecordedTranscriptionClient Prerecorded { get; }
    }
}
