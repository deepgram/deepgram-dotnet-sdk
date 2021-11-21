# Deepgram .NET SDK

## Installation

To install the C# client library using NuGet:

Run the following command from your terminal in your projects directory:

```bash
dotnet add package Deepgram
```

## Targeted Frameworks

- 6.0.0
- .NET Standard 2.0 - supports everything 4.6.1 and above

## Configuration

To setup the configuration of the Deepgram Client you can do one of the following:

- Create a Deepgram Client instance and pass in credentials in the constructor.

```csharp
var credentials = new Credentials(YOUR_DEEPGRAM_API_KEY);
var deepgramClient = new DeepgramClient(credentials);
```

Or

- Provide the Deepgram API key and optional API Url in `appsettings.json`:

```js
{
  "appSettings": {
    "Deepgram.Api.Key": "YOUR_DEEPGRAM_API_KEY",
    "Deepgram.Api.Uri": "https://api.deepgram.com"
  }
}
```

> Note: In the event multiple configuration files are found, the order of precedence is as follows:

```
* ```appsettings.json``` which overrides
* ```settings.json```
```

Or

- Access the Configuration instance and set the appropriate key in your code for example:

```csharp
Configuration.Instance.Settings["appSettings:Deepgram.Api.Key"] = "YOUR_DEEPGRAM_API_KEY";
Configuration.Instance.Settings["appSettings:Deepgram.Api.Uri"] = "https://api.deepgram.com";

```

## Logging

The Library uses Microsoft.Extensions.Logging to preform all of it's logging tasks. To configure
logging for you app simply create a new ILoggerFactory and call the LogProvider.SetLogFactory()
method to tell the Deepgram library how to log. For example, to log to the console with serilog
you can do the following:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Deepgram.Logger;
using Serilog;

var log = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm} [{Level}]: {Message}\n")
    .CreateLogger();
var factory = new LoggerFactory();
factory.AddSerilog(log);
LogProvider.SetLogFactory(factory);
```

## Examples

### Sending a Remote File for Transcription

```csharp
var credentials = new Credentials(DEEPGRAM_API_KEY);

var deepgramClient = new DeepgramClient(credentials);

var response = await deepgramClient.Transcription.GetPrerecordedTranscriptionAsync(
    new Deepgram.Transcription.UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    new Deepgram.Transcription.PrerecordedTranscriptionOptions()
    {
        Punctuate = true
    });
```

### Sending a Local File for Transcription

```csharp
var credentials = new Credentials(DEEPGRAM_API_KEY);

var deepgramClient = new DeepgramClient(credentials);

using (FileStream fs = File.OpenRead("path\\to\\file"))
{
    var response = await deepgramClient.Transcription.GetPrerecordedTranscriptionAsync(
        new Deepgram.Transcription.StreamSource(
            fs,
            "audio/wav"),
        new Deepgram.Transcription.PrerecordedTranscriptionOptions()
        {
            Punctuate = true
        });
}
```

## Development and Contributing

Interested in contributing? We ❤️ pull requests!

To make sure our community is safe for all, be sure to review and agree to our
[Code of Conduct](./CODE_OF_CONDUCT.md). Then see the
[Contribution](./CONTRIBUTING.md) guidelines for more information.

## Getting Help

We love to hear from you so if you have questions, comments or find a bug in the
project, let us know! You can either:

- [Open an issue](https://github.com/deepgram-devs/deepgram-dotnet-sdk/issues/new) on this repository
- Tweet at us! We're [@DeepgramDevs on Twitter](https://twitter.com/DeepgramDevs)

## Further Reading

Check out the Developer Documentation at [https://developers.deepgram.com/](https://developers.deepgram.com/)
