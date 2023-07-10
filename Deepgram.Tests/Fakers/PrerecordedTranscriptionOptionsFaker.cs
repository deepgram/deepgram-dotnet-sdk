using System.Linq;
using AutoBogus;
using Deepgram.Transcription;

namespace Deepgram.Tests.Fakers
{

    public class PrerecordedTranscriptionOptionsFaker : AutoFaker<PrerecordedTranscriptionOptions>
    {
        public PrerecordedTranscriptionOptionsFaker()
        {
            RuleFor(p => p.Model, f => f.Lorem.Word());
            RuleFor(p => p.Version, f => f.Lorem.Word());
            RuleFor(p => p.Language, f => f.Lorem.Word());
            RuleFor(p => p.Tier, f => f.Lorem.Word());
            RuleFor(p => p.Punctuate, f => f.Random.Bool());
            RuleFor(p => p.ProfanityFilter, f => f.Random.Bool());
            var redactionDataset = new string[] { "pci", "number", "ssn" };
            RuleFor(p => p.Redaction, f => f.PickRandom(redactionDataset, 2).ToArray());
            RuleFor(p => p.Diarize, f => f.Random.Bool());
            RuleFor(p => p.DiarizationVersion, f => f.Random.Int().ToString());
            RuleFor(p => p.MultiChannel, f => f.Random.Bool());
            RuleFor(p => p.Alternatives, f => f.Random.Int());
            RuleFor(p => p.Numerals, f => f.Random.Bool());
            RuleFor(p => p.Numbers, f => f.Random.Bool());
            RuleFor(p => p.NumbersSpaces, f => f.Random.Bool());
            RuleFor(p => p.Dates, f => f.Random.Bool());
            RuleFor(p => p.DateFormat, f => f.Lorem.Word());
            RuleFor(p => p.Times, f => f.Random.Bool());
            RuleFor(p => p.Dictation, f => f.Random.Bool());
            RuleFor(p => p.Measurements, f => f.Random.Bool());
            RuleFor(p => p.SmartFormat, f => f.Random.Bool());
            RuleFor(p => p.SearchTerms, f => f.Random.WordsArray(1, 3));
            RuleFor(p => p.Replace, f => f.Random.WordsArray(1, 3));
            RuleFor(p => p.Callback, f => f.Internet.Url());
            RuleFor(p => p.Keywords, f => f.Random.WordsArray(1, 3));
            RuleFor(p => p.KeywordBoost, f => f.Lorem.Word());
            RuleFor(p => p.Utterances, f => f.Random.Bool());
            RuleFor(p => p.DetectLanguage, f => f.Random.Bool());
            RuleFor(p => p.Paragraphs, f => f.Random.Bool());
            RuleFor(p => p.UtteranceSplit, f => f.Random.Decimal(2, 4));
            var summarizeDataset = new object[] { "v2", true, false };
            RuleFor(p => p.Summarize, f => f.PickRandom(summarizeDataset));

            RuleFor(p => p.DetectEntities, f => f.Random.Bool());
            RuleFor(p => p.Translate, f => f.Random.WordsArray(1, 3));
            RuleFor(p => p.DetectTopics, f => f.Random.Bool());
            RuleFor(p => p.AnalyzeSentiment, f => f.Random.Bool());
            RuleFor(p => p.Sentiment, f => f.Random.Bool());
            RuleFor(p => p.SentimentThreshold, f => f.Random.Decimal((decimal)0.1, (decimal)3.0));

        }
    }
}