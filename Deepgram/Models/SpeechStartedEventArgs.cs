using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deepgram.Models
{
    public class SpeechStartedEventArgs : EventArgs
    {
        public SpeechStartedEventArgs(LiveTranscriptionSpeechStarted speechStarted)
        {
            SpeechStarted = speechStarted;
        }

        public LiveTranscriptionSpeechStarted SpeechStarted { get; set; }
    }
}
