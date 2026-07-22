// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v2;
using Deepgram.Clients.Interfaces.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.Speak.REST;

namespace Deepgram.Clients.Flux.Speak.REST;

/// <summary>
/// (PREVIEW) Implements the Flux (v2 speak) batch text-to-speech REST Client (POST /v2/speak):
/// synthesize a complete block of text into a single audio response. Supply a callback URL to have
/// the request processed asynchronously.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public class Client(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null, string? httpId = null)
    : AbstractRestClient(apiKey, deepgramClientOptions, httpId), IFluxSpeakRESTClient
{
    #region NoneCallBacks
    /// <summary>
    /// Synthesizes the supplied text and returns the audio as a stream plus response metadata.
    /// </summary>
    /// <param name="source">The text to synthesize <see cref="TextSource"/></param>
    /// <param name="speakSchema">Query options for the request <see cref="SpeakSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public async Task<SyncResponse> ToStream(TextSource source, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("FluxSpeakRESTClient.ToStream", "ENTER");
        Log.Information("ToStream", $"source: {source}");
        Log.Information("ToStream", $"speakSchema:\n{speakSchema}");

        VerifyNoCallBack(nameof(ToStream), speakSchema);

        List<string> keys = new List<string> {
            Constants.ContentType,
            Constants.RequestId,
            Constants.ModelUUID,
            Constants.ModelName,
            Constants.CharCount,
            Constants.TransferEncoding,
            Constants.Date,
        };

        var uri = GetUri(_options);
        var localFileResult = await PostRetrieveLocalFileAsync<TextSource, SpeakSchema, LocalFileWithMetadata>(
            uri, speakSchema, source, keys, cancellationToken, addons, headers
            );

        SyncResponse response = new SyncResponse();

        // build up the response object from the returned headers
        foreach (var item in localFileResult.Metadata)
        {
            var key = item.Key.ToLower();

            switch (key)
            {
                case Constants.ContentType:
                    response.ContentType = item.Value;
                    break;
                case Constants.RequestId:
                    response.RequestId = item.Value;
                    break;
                case Constants.ModelUUID:
                    response.ModelUUID = item.Value;
                    break;
                case Constants.ModelName:
                    response.ModelName = item.Value;
                    break;
                case Constants.CharCount:
                    response.Characters = int.Parse(item.Value);
                    break;
                case Constants.TransferEncoding:
                    response.TransferEncoding = item.Value;
                    break;
                case Constants.Date:
                    response.Date = DateTime.Parse(item.Value);
                    break;
                default:
                    Log.Error("ToStream", $"Unknown metadata key: {key}");
                    break;
            }
        }

        Log.Information("ToStream", $"{uri} Succeeded");
        Log.Debug("ToStream", $"response: {response}");
        Log.Verbose("FluxSpeakRESTClient.ToStream", "LEAVE");

        // add stream to response
        response.Stream = localFileResult.Content;

        return response;
    }

    /// <summary>
    /// Synthesizes the supplied text and writes the audio to the given file.
    /// </summary>
    public async Task<SyncResponse> ToFile(TextSource source, string filename, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("FluxSpeakRESTClient.ToFile", "ENTER");
        Log.Information("ToFile", $"filename: {filename}");

        var response = await ToStream(source, speakSchema, cancellationToken, addons, headers);

        // save the file
        response.Filename = filename;
        using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
        {
            writer.Write(response.Stream.ToArray());
        }

        // clear the stream
        response.Stream = null;

        Log.Verbose("FluxSpeakRESTClient.ToFile", "LEAVE");

        return response;
    }
    #endregion

    #region CallBack Methods
    /// <summary>
    /// Synthesizes the supplied text asynchronously via a callback URL, returning an
    /// acknowledgement ({request_id}); the audio is delivered to the callback URL.
    /// </summary>
    /// <param name="source">The text to synthesize <see cref="TextSource"/></param>
    /// <param name="callBack">Callback URL</param>
    /// <param name="speakSchema">Query options for the request <see cref="SpeakSchema"/></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public async Task<AsyncResponse> StreamCallBack(TextSource source, string? callBack, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("FluxSpeakRESTClient.StreamCallBack", "ENTER");
        Log.Information("StreamCallBack", $"source: {source}");
        Log.Information("StreamCallBack", $"callBack: {callBack}");
        Log.Information("StreamCallBack", $"speakSchema:\n{speakSchema}");

        VerifyOneCallBackSet(nameof(StreamCallBack), callBack, speakSchema);
        if (callBack != null)
            speakSchema.CallBack = callBack;

        var uri = GetUri(_options);
        var result = await PostAsync<TextSource, SpeakSchema, AsyncResponse>(uri, speakSchema, source, cancellationToken, addons, headers);

        Log.Information("StreamCallBack", $"{uri} Succeeded");
        Log.Debug("StreamCallBack", $"result: {result}");
        Log.Verbose("FluxSpeakRESTClient.StreamCallBack", "LEAVE");

        return result;
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Builds the base URI for the Flux TTS batch endpoint. The SDK default appends /v1 to the
    /// configured BaseAddress; strip it and target /v2/speak. Query parameters from the schema are
    /// appended by the underlying POST helpers.
    /// </summary>
    internal static string GetUri(IDeepgramClientOptions options)
    {
        var baseAddress = options.BaseAddress;

        Regex regex = new Regex(@"\b(\/v[0-9]+)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(baseAddress))
        {
            Log.Information("GetUri", $"BaseAddress contains API version: {baseAddress}");
            baseAddress = regex.Replace(baseAddress, "");
            Log.Debug("GetUri", $"BaseAddress: {baseAddress}");
        }
        baseAddress = baseAddress.TrimEnd('/');

        return $"{baseAddress}/{UriSegments.SPEAK_V2}";
    }

    private void VerifyNoCallBack(string method, SpeakSchema? speakSchema)
    {
        Log.Debug("VerifyNoCallBack", $"method: {method}");

        if (speakSchema != null && speakSchema.CallBack != null)
        {
            var exStr = $"CallBack cannot be provided as a schema option to a synchronous request when calling {method}. Use {nameof(StreamCallBack)}";
            Log.Error("VerifyNoCallBack", $"Exception: {exStr}");
            throw new ArgumentException(exStr);
        }
    }

    private void VerifyOneCallBackSet(string method, string? callBack, SpeakSchema? speakSchema)
    {
        Log.Debug("VerifyOneCallBackSet", $"method: {method}");

        if (speakSchema.CallBack == null && callBack == null)
        {
            // no CallBack set in either the callBack parameter or the SpeakSchema
            var exStr = $"Either provide a CallBack url or set SpeakSchema.CallBack. Use {nameof(ToStream)}";
            Log.Error("VerifyOneCallBackSet", $"Exception: {exStr}");
            throw new ArgumentException(exStr);
        }
        else if (!string.IsNullOrEmpty(speakSchema.CallBack) && !string.IsNullOrEmpty(callBack))
        {
            // CallBack set in both the callBack parameter and the SpeakSchema
            var exStr = "CallBack should be set in either the CallBack parameter or SpeakSchema.CallBack, not in both.";
            Log.Error("VerifyOneCallBackSet", $"Exception: {exStr}");
            throw new ArgumentException(exStr);
        }
    }
    #endregion
}
