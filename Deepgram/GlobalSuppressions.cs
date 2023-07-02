// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "namespace", Target = "~N:Deepgram")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~P:Deepgram.Clients.TranscriptionClient.Prerecorded")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~F:Deepgram.Common.AudioEncoding.OggSpeex")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.Request.DeepgramHttpRequestException.#ctor")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.Request.DeepgramHttpRequestException.#ctor(System.String)")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.Request.DeepgramHttpRequestException.#ctor(System.String,System.Exception)")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~P:Deepgram.Interfaces.ITranscriptionClient.Prerecorded")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "type", Target = "~T:Deepgram.Interfaces.IPrerecordedTranscriptionClient")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "type", Target = "~T:Deepgram.Request.DeepgramHttpRequestException")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~P:Deepgram.Models.GetUsageSummaryOptions.Diarization")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~P:Deepgram.Models.LiveTranscriptionOptions.DiarizationVersion")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~P:Deepgram.Models.LiveTranscriptionOptions.Diarize")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~P:Deepgram.Models.LiveTranscriptionOptions.Endpointing")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~P:Deepgram.Models.PrerecordedTranscriptionOptions.DiarizationVersion")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "member", Target = "~P:Deepgram.Models.UsageRequestResponseConfig.Diarize")]
[assembly: SuppressMessage("Spellchecker", "CRRSP09:A misspelled word has been found", Justification = "<Pending>", Scope = "type", Target = "~T:Deepgram.Models.DeepgramResponse")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "type", Target = "~T:Deepgram.Models.PrerecordedTranscription")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "type", Target = "~T:Deepgram.Models.PrerecordedTranscriptionOptions")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.Clients.LiveTranscriptionClient.EnqueueForSending(Deepgram.Models.MessageToSend)")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.Clients.LiveTranscriptionClient.FinishAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.Clients.LiveTranscriptionClient.ProcessSenderQueue~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.Clients.LiveTranscriptionClient.Send(System.ArraySegment{System.Byte},System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.Clients.LiveTranscriptionClient.StartConnectionAsync(Deepgram.Models.LiveTranscriptionOptions)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.Request.ApiRequest.ProcessResponse(System.Net.Http.HttpResponseMessage,System.String)~Deepgram.Models.DeepgramResponse")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "type", Target = "~T:Deepgram.Models.PrerecordedTranscriptionResult")]

/* Unmerged change from project 'Deepgram (net6.0)'
Added:
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.DeepgramClient.SetHttpClientTimeout(System.TimeSpan)")]
*/
[assembly: SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>", Scope = "member", Target = "~F:Deepgram.Logger.LogProvider._loggers")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:Deepgram.DeepgramClient.SetHttpClientTimeout(System.TimeSpan)")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "type", Target = "~T:Deepgram.DeepgramClient")]
[assembly: SuppressMessage("Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "<Pending>", Scope = "type", Target = "~T:Deepgram.Models.PrerecordedTranscriptionMetaData")]
