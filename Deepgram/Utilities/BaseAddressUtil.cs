using System.Text.RegularExpressions;

namespace Deepgram.Utilities;
internal static class BaseAddressUtil
{
    internal static DeepgramClientOptions GetWss(DeepgramClientOptions deepgramClientOptions)
    {
        var baseAddress = deepgramClientOptions.BaseAddress;
        //checks for ws:// wss:// ws wss - wss:// is include to ensure it is all stripped out and correctly formatted
        Regex regex = new Regex(@"\b(ws:\/\/|wss:\/\/|ws|wss)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(baseAddress))
            deepgramClientOptions.BaseAddress = regex.Replace(baseAddress, "wss://");
        else
            //if no protocol in the address then https:// is added
            deepgramClientOptions.BaseAddress = $"wss://{baseAddress}";
        return deepgramClientOptions;
    }
    internal static DeepgramClientOptions GetHttps(DeepgramClientOptions deepgramClientOptions)
    {
        var baseAddress = deepgramClientOptions.BaseAddress;
        //checks for http:// https:// http https - https:// is include to ensure it is all stripped out and correctly formatted 
        Regex regex = new Regex(@"\b(http:\/\/|https:\/\/|http|https)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(baseAddress))
            deepgramClientOptions.BaseAddress = regex.Replace(baseAddress, "https://");
        else
            //if no protocol in the address then https:// is added
            deepgramClientOptions.BaseAddress = $"https://{baseAddress}";
        return deepgramClientOptions;
    }



}

