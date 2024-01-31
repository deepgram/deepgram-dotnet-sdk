namespace Deepgram.Models.Live.v1;

public record Hit
{
    /// <summary>
    /// 
    /// </summary>
    public int Confidence { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int End { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Snippet { get; set; }
}