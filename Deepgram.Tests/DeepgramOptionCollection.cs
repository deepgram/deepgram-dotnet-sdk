using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Deepgram.Tests
{
    /// <summary>
    /// Represents a collection of common options that can be passed to the Deepgram API.
    /// </summary>
    public record DeepgramOptionCollection : IDictionary<string, string>
    {
        protected readonly Dictionary<string, string> _options = [];

        /// <summary>
        /// AI model used to process submitted audio. Default: <c>general</c>. Learn More: <see href="https://developers.deepgram.com/docs/model" />
        /// </summary>
        public string Model { get => Uri.UnescapeDataString(_options[nameof(Model)]); set => _options[nameof(Model)] = Uri.EscapeDataString(value); }

        /// <summary>
        /// Level of model you would like to use in your request. Default: <c>base</c>. Learn More: <see href="https://developers.deepgram.com/docs/tier" />
        /// </summary>
        public string Tier { get => Uri.UnescapeDataString(_options[nameof(Tier)]); set => _options[nameof(Tier)] = Uri.EscapeDataString(value); }

        /// <summary>
        /// Version of the model to use. Default: <c>latest</c>. Learn more: <see href="https://developers.deepgram.com/docs/version" />
        /// </summary>
        public string Version { get => Uri.UnescapeDataString(_options[nameof(Version)]); set => _options[nameof(Version)] = Uri.EscapeDataString(value); }

        /// <summary>
        /// The <c>BCP-47</c> language tag that hints at the primary spoken language. Default: <c>en</c>. Learn more: <see href="https://developers.deepgram.com/docs/language" />
        /// </summary>
        /// <remarks>
        /// The definition of <c>BCP-47</c> is available at <see href="https://tools.ietf.org/html/bcp47" />.
        /// </remarks>
        public CultureInfo Language { get => CultureInfo.GetCultureInfoByIetfLanguageTag(_options[nameof(Language)]); set => _options[nameof(Language)] = value.IetfLanguageTag; }

        /// <summary>
        /// Add punctuation and capitalization to the transcript. Default: <see cref="false"/>. Learn more: <see href="https://developers.deepgram.com/docs/punctuation" />
        /// </summary>
        public bool Punctuate { get => bool.Parse(_options[nameof(Punctuate)]); set => _options[nameof(Punctuate)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Remove profanity from the transcript. Default: <see cref="false"/>. Learn more: <see href="https://developers.deepgram.com/docs/profanity-filter" />
        /// </summary>
        public bool ProfanityFilter { get => bool.Parse(_options[nameof(ProfanityFilter)]); set => _options[nameof(ProfanityFilter)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Redact sensitive information, replacing redacted content with asterisks (*). Learn more: <see href="https://developers.deepgram.com/docs/redaction" />
        /// </summary>
        public IEnumerable<string> Redact { get => _options[nameof(Redact)].Split("&redact").Select(Uri.UnescapeDataString); set => _options[nameof(Redact)] = value.Aggregate((str1, str2) => $"{Uri.EscapeDataString(str1)}&redact={Uri.EscapeDataString(str2)}"); }

        /// <summary>
        /// Recognize speaker changes. Each word in the transcript will be assigned a speaker number starting at <c>0</c>. Learn more: <see href="https://developers.deepgram.com/docs/diarization" />
        /// </summary>
        public bool Diarize { get => bool.Parse(_options[nameof(Diarize)]); set => _options[nameof(Diarize)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Version of the diarization feature to use. Only used when <see cref="Diarize"/> is <see cref="true"/>. Default: <c>latest</c>. Learn more: <see href="https://developers.deepgram.com/docs/diarization#enable-feature" />
        /// </summary>
        public string DiarizeVersion { get => Uri.UnescapeDataString(_options[nameof(DiarizeVersion)]); set => _options[nameof(DiarizeVersion)] = Uri.EscapeDataString(value); }

        /// <summary>
        /// Apply formatting to transcript output. When set to true, additional formatting will be applied to transcripts to improve readability. Default: <see cref="false"/>. Learn more: <see href="https://developers.deepgram.com/docs/smart-format" />
        /// </summary>
        public bool SmartFormat { get => bool.Parse(_options[nameof(SmartFormat)]); set => _options[nameof(SmartFormat)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Whether to include words like "uh" and "um" in transcription output. Default: <see cref="false"/>. Learn more: <see href="https://developers.deepgram.com/docs/filler-words" />
        /// </summary>
        public bool FillerWords { get => bool.Parse(_options[nameof(FillerWords)]); set => _options[nameof(FillerWords)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Transcribe each audio channel independently. Default: <see cref="false"/>. Learn more: <see href="https://developers.deepgram.com/docs/multichannel" />
        /// </summary>
        public bool MultiChannel { get => bool.Parse(_options[nameof(MultiChannel)]); set => _options[nameof(MultiChannel)] = value.ToString().ToLowerInvariant(); }

        /// <summary>
        /// Number of transcripts to return per request. Default: <c>1</c>.
        /// </summary>
        public int Alternatives { get => int.Parse(_options[nameof(Alternatives)]); set => _options[nameof(Alternatives)] = value.ToString(CultureInfo.InvariantCulture); }

        /// <summary>
        /// Terms or phrases to search for in the submitted audio. Learn more: <see href="https://developers.deepgram.com/docs/search" />
        /// </summary>
        public IEnumerable<string> Search { get => _options[nameof(Redact)].Split("&search").Select(Uri.UnescapeDataString); set => _options[nameof(Redact)] = value.Aggregate((str1, str2) => $"{Uri.EscapeDataString(str1)}&search={Uri.EscapeDataString(str2)}"); }

        /// <summary>
        /// Terms or phrases to search for in the submitted audio and replace. Learn more: <see href="https://developers.deepgram.com/docs/find-and-replace" />
        /// </summary>
        public IDictionary<string, string> Replace { get => _options[nameof(Redact)].Split("&replace").ToDictionary(str => Uri.UnescapeDataString(str.Split(':')[0]), str => Uri.UnescapeDataString(str.Split(':')[1])); set => _options[nameof(Redact)] = value.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}:{Uri.EscapeDataString(kvp.Value)}").Aggregate((str1, str2) => $"{str1}&replace={str2}"); }

        /// <summary>
        /// Callback URL to provide if you would like your submitted audio to be processed asynchronously. Learn more: <see href="https://developers.deepgram.com/docs/callback" />
        /// </summary>
        public Uri Callback { get => new(_options[nameof(Callback)]); set => _options[nameof(Callback)] = value.ToString(); }

        /// <summary>
        /// Uncommon proper nouns or other words to transcribe that are not a part of the model's vocabulary. Learn more: <see href="https://developers.deepgram.com/docs/keywords" />
        /// </summary>
        public IDictionary<string, double> Keywords { get => _options[nameof(Redact)].Split("&keywords").ToDictionary(str => Uri.UnescapeDataString(str.Split(':')[0]), str => double.Parse(str.Split(':')[1], CultureInfo.InvariantCulture)); set => _options[nameof(Redact)] = value.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}:{kvp.Value.ToString(CultureInfo.InvariantCulture)}").Aggregate((str1, str2) => $"{str1}&keywords={str2}"); }

        /// <summary>
        /// Tag to associate with the request. Learn more: <see href="https://developers.deepgram.com/docs/tagging" />
        /// </summary>
        public string Tag { get => Uri.UnescapeDataString(_options[nameof(Tag)]); set => _options[nameof(Tag)] = Uri.EscapeDataString(value); }

        /// <summary>
        /// Converts the specified options to a query string.
        /// </summary>
        /// <returns>The query string.</returns>
        public string ToQueryString() => string.Join("&", _options.Select(x => $"{ToSnakeCase(x.Key)}={x.Value}"));

        /// <summary>
        /// Converts a string to snake case.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>The snake cased string.</returns>
        protected static string ToSnakeCase(string str)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(char.ToLowerInvariant(str[0]));
            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]))
                {
                    stringBuilder.Append('_');
                }

                stringBuilder.Append(char.ToLowerInvariant(str[i]));
            }

            return stringBuilder.ToString();
        }

        /// <inheritdoc />
        public string this[string key] { get => _options[key]; set => _options[key] = value; }

        /// <inheritdoc />
        public ICollection<string> Keys => _options.Keys;

        /// <inheritdoc />
        public ICollection<string> Values => _options.Values;

        /// <inheritdoc />
        public int Count => _options.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(string key, string value) => _options.Add(key, value);

        /// <inheritdoc />
        public void Add(KeyValuePair<string, string> item) => _options.Add(item.Key, item.Value);

        /// <inheritdoc />
        public void Clear() => _options.Clear();

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, string> item) => _options.ContainsKey(item.Key) && _options.ContainsValue(item.Value);

        /// <inheritdoc />
        public bool ContainsKey(string key) => _options.ContainsKey(key);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, string>>)_options).CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _options.GetEnumerator();

        /// <inheritdoc />
        public bool Remove(string key) => _options.Remove(key);

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)_options).Remove(item);

        /// <inheritdoc />
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => _options.TryGetValue(key, out value);

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_options).GetEnumerator();
    }
}
