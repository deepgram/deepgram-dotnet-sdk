namespace Deepgram.Models;

// node sdk assigns as - export type FileSource =  Buffer | Readable;
//  c#  Equivalent of node buffer - byte[]
//  c#  Equivalent of node readable - Stream
public class FileSource
{

    public byte[]? Buffer { get; set; }

    public Stream? Stream { get; set; }
}

