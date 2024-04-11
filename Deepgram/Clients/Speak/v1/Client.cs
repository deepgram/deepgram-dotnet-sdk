// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Speak.v1;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Clients.Interfaces.v1;

namespace Deepgram.Clients.Speak.v1;

/// <summary>
/// Implements version 1 of the Speak Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public class Client(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null, string? httpId = null)
    : AbstractRestClient(apiKey, deepgramClientOptions, httpId), ISpeakClient
{
    #region NoneCallBacks
    /// <summary>
    /// Speaks a file using the provided stream
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"/></param>
    /// <param name="speakSchema">Options for the transcription <see cref="SpeakSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public async Task<SyncResponse> ToStream(TextSource source, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("SpeakClient.ToStream", "ENTER");
        Log.Information("ToStream", $"source: {source}");
        Log.Information("ToStream", $"analyzeSchema:\n{JsonSerializer.Serialize(speakSchema, JsonSerializeOptions.DefaultOptions)}");

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

        var uri = GetUri(_options, $"{UriSegments.SPEAK}");
        var localFileResult = await PostRetrieveLocalFileAsync<TextSource, SpeakSchema, LocalFileWithMetadata>(
            uri, speakSchema, source, keys, cancellationToken, addons, headers
            );

        SyncResponse response = new SyncResponse();

        // build up the response object
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
        Log.Verbose("Client.ToStream", "LEAVE");

        // add stream to response
        response.Stream = localFileResult.Content;

        return response;
    }

    public async Task<SyncResponse> ToFile(TextSource source, string filename, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("Client.ToFile", "ENTER");
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

        Log.Verbose("Client.ToFile", "LEAVE");

        return response;
    }
    #endregion

    #region  CallBack Methods
    /// <summary>
    /// Speaks a file using the provided byte array and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="speakSchema">Options for the transcription<see cref="SpeakSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public async Task<AsyncResponse> StreamCallBack(TextSource source, string? callBack, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("SpeakClient.StreamCallBack", "ENTER");
        Log.Information("StreamCallBack", $"source: {source}");
        Log.Information("StreamCallBack", $"callBack: {callBack}");
        Log.Information("StreamCallBack", $"speakSchema:\n{JsonSerializer.Serialize(speakSchema, JsonSerializeOptions.DefaultOptions)}");

        VerifyOneCallBackSet(nameof(StreamCallBack), callBack, speakSchema);
        if (callBack != null)
            speakSchema.CallBack = callBack;

        var uri = GetUri(_options, $"{UriSegments.SPEAK}");
        var result = await PostAsync<TextSource, SpeakSchema, AsyncResponse>(uri, speakSchema, source, cancellationToken, addons, headers);

        Log.Information("StreamCallBack", $"{uri} Succeeded");
        Log.Debug("StreamCallBack", $"result: {result}");
        Log.Verbose("Client.StreamCallBack", "LEAVE");

        return result;
    }
    #endregion

    #region CallbackChecks
    private void VerifyNoCallBack(string method, SpeakSchema? speakSchema)
    {
        Log.Debug("VerifyNoCallBack", $"method: {method}");

        if (speakSchema != null && speakSchema.CallBack != null)
        {
            var exStr = $"CallBack cannot be provided as schema option to a synchronous transcription when calling {method}. Use {nameof(StreamCallBack)}";
            Log.Error("VerifyNoCallBack", $"Exception: {exStr}");
            throw new ArgumentException(exStr);
        }
    }

    private void VerifyOneCallBackSet(string method, string? callBack, SpeakSchema? speakSchema)
    {
        Log.Debug("VerifyOneCallBackSet", $"method: {method}");

        if (speakSchema.CallBack == null && callBack == null)
        {
            //check if no CallBack set in either callBack parameter or AnalyzeSchema
            var exStr = $"Either provide a CallBack url or set SpeakSchema.CallBack. Use {nameof(ToStream)}";
            Log.Error("VerifyNoCallBack", $"Exception: {exStr}");
            throw new ArgumentException(exStr);
        }
        else if (!string.IsNullOrEmpty(speakSchema.CallBack) && !string.IsNullOrEmpty(callBack))
        {
            //check that only one CallBack is set in either callBack parameter or AnalyzeSchema
            var exStr = "CallBack should be set in either the CallBack parameter or SpeakSchema.CallBack not in both.";
            Log.Error("VerifyNoCallBack", $"Exceptions: {exStr}");
            throw new ArgumentException(exStr);
        }
    }
    #endregion 
}
