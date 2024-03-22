// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Speak.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Clients.Speak.v1;

/// <summary>
/// Implements version 1 of the Speak Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class Client(string apiKey, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, deepgramClientOptions)
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

        var localFileResult = await PostRetrieveLocalFileAsync<TextSource, SpeakSchema, LocalFileWithMetadata>(
            GetUri(_options, $"{UriSegments.SPEAK}"), speakSchema, source, keys, cancellationToken, addons, headers
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
                    // TODO log this
                    break;
            }
        }

        // add stream to response
        response.Stream = localFileResult.Content;

        return response;
    }

    public async Task<SyncResponse> ToFile(TextSource source, string filename, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null
        )
    {
        var response = await ToStream(source, speakSchema, cancellationToken, addons, headers);

        // save the file
        response.Filename = filename;
        using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
        {
            writer.Write(response.Stream.ToArray());
        }

        // clear the stream
        response.Stream = null;

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
        VerifyOneCallBackSet(nameof(StreamCallBack), callBack, speakSchema);
        if (callBack != null)
            speakSchema.CallBack = callBack;

        return await PostAsync<TextSource, SpeakSchema, AsyncResponse>(
            GetUri(_options, $"{UriSegments.SPEAK}"), speakSchema, source, cancellationToken, addons, headers);
    }
    #endregion

    #region CallbackChecks
    private void VerifyNoCallBack(string method, SpeakSchema? speakSchema)
    {
        // TODO: think about logging here based on coderabbit feedback
        if (speakSchema != null && speakSchema.CallBack != null)
            throw new ArgumentException($"CallBack cannot be provided as schema option to a synchronous transcription when calling {method}. Use {nameof(StreamCallBack)}");
    }

    private void VerifyOneCallBackSet(string callingMethod, string? callBack, SpeakSchema? speakSchema)
    {
        // TODO: think about logging here based on coderabbit feedback
        if (speakSchema.CallBack == null && callBack == null)
        { //check if no CallBack set in either callBack parameter or SpeakSchema
            var ex = new ArgumentException($"Either provide a CallBack url or set SpeakSchema.CallBack. If no CallBack needed either {nameof(Stream)}");
            Log.Exception(_logger, $"While calling {callingMethod} no callback set", ex);
            throw ex;
        }
        else if (!string.IsNullOrEmpty(speakSchema.CallBack) && !string.IsNullOrEmpty(callBack))
        {
            //check that only one CallBack is set in either callBack parameter or SpeakSchema
            var ex = new ArgumentException("CallBack should be set in either the CallBack parameter or SpeakSchema.CallBack not in both.");
            Log.Exception(_logger, $"While calling {callingMethod}, callback set in both parameter and property", ex);
            throw ex;
        }
    }
    #endregion 
}
