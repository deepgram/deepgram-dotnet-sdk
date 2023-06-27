using System;
using System.IO;
using Deepgram.Models;

namespace Deepgram.Tests
{
    public static class FakeModels
    {
        public static Credentials Credentials => new Credentials()
        {
            ApiKey = ApiKey,
            ApiUrl = CleanedUrl,
            RequireSSL = true
        };
        public static string ApiKey = Guid.NewGuid().ToString();
        public static string FullUrl = "http://test.com";
        public static string CleanedUrl = "test.com";
        public static string UriSegment = "test";

        public static PrerecordedTranscriptionOptions PrerecordedTranscriptionOptions = new PrerecordedTranscriptionOptions
        {
            Keywords = new[] { "key", "word" },
            Punctuate = true,
            Utterances = true,
            Redaction = new[] { "pci", "ssn" },
            UtteranceSplit = (decimal)2.223,
            Paragraphs = true,
            Model = "Model"
        };

        public static ListAllRequestsOptions ListAllRequestsOptions = new ListAllRequestsOptions()
        {
            Limit = 10,
            StartDateTime = new DateTime(2023, 5, 23)
        };

        public static UrlSource UrlSource = new UrlSource("https://test.com");

        public static UpdateScopeOptions UpdateScopeOptions = new UpdateScopeOptions() { Scope = "owner" };

        public static Project Project = new Project()
        {
            Company = "testCompany",
            Id = "testId",
            Name = "test"
        };

        public static StreamSource StreamSource = new StreamSource(new MemoryStream(new byte[] { 0b1, 0b10, 0b11, 0b100 }), "text/plain");

    }
}
