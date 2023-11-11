namespace Deepgram.Models;

// node sdk assigns as - export type PrerecordedSource = UrlSource | Buffer | Readable;
//  c#  Equivalent of node buffer - byte[]
//  c#  Equivalent of node readable - Stream
public class PrerecordedSource
{
    public UrlSource? UrlSource { get; set; }

    public byte[]? Buffer { get; set; }

    public Stream Stream { get; set; }
}

