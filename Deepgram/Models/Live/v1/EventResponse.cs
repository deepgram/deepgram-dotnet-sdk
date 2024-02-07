// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Live.v1;

public class EventResponse
{
    /// <summary>
    /// MetaData response from the live transcription service
    /// </summary>
    public MetadataResponse? MetaData { get; set; }

    /// <summary>
    /// Transcription response from the live transcription service
    /// </summary>
    public TranscriptionResponse? Transcription { get; set; }

    /// <summary>
    /// UtterancEnd response from the live transcription service
    /// </summary>
    public UtteranceEndResponse? UtteranceEnd { get; set; }

    /// <summary>
    /// Error response from the live transcription service
    /// </summary>
    public Exception? Error { get; set; }
}
