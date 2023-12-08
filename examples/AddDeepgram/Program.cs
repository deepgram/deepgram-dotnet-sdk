// Using deepgram from a nuget package and not a project reference

using Deepgram;
using Deepgram.Common;
using Deepgram.Extensions;
using Deepgram.Models;
using Microsoft.Extensions.DependencyInjection;
var ApiKey = "9500112ed69a4d8d13e940b99399ae2db16e2b97";












var serviceCollection = new ServiceCollection();

//serviceCollection.AddDeepgram(ApiKey);
var deepgramClientOptions = new DeepgramClientOptions(ApiKey)
{
    BaseAddress = "acme.com",
    HttpTimeout = TimeSpan.FromSeconds(300),
};
serviceCollection.AddDeepgram(deepgramClientOptions);


var serviceProvider = serviceCollection.BuildServiceProvider();

var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

var httpClient = httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME);


//var manageClient = new ManageClient(null, null);
//var manageClient = new ManageClient(null, httpClient);
//var manageClient = new ManageClient(new DeepgramClientOptions(ApiKey), null);
var manageClient = new ManageClient(new DeepgramClientOptions(ApiKey), httpClient);
//var manageClient = new ManageClient(deepgramClientOptions, httpClient);


var result = await manageClient.GetProjectsAsync();

Console.WriteLine(result.Projects.First().Name);


Console.ReadLine();




//creating a client - you have to pass in  deepgramClientOptions regardless of which way you do  .AddDeepgram()
// Seem odd to have to pass the deepgramClientOptions Twice?


// the above works great if the application is a single user


//MultiTenanted application



// Register the SDK 

//var serviceCollection = new ServiceCollection();
//serviceCollection.AddDeepgram(ApiKey);


//var serviceProvider = serviceCollection.BuildServiceProvider();
//var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
//var httpClient = httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME);



////var deepgramClientOptions = new DeepgramClientOptions(ApiKey);
//// add to service collection with client options


////Passing Client Options
//var ApiKey = "asbfvadsdbfasbdsvadbdpvbiadfivbadif";
////var deepgramClientOptions = new DeepgramClientOptions(ApiKey)
////{   
////    HttpTimeout = TimeSpan.FromSeconds(300),
////};
////serviceCollection.AddDeepgram(deepgramClientOptions);


//// Create a key for the support departments
//var manageClient = new ManageClient(deepgramClientOptions, httpClient);



//var roadRunnerSupport = await manageClient.CreateProjectKeyAsync("acmesolutionproject",
//    new CreateProjectKeySchema()
//    {
//        Comment = "wiley coyote support key",
//        Scopes = new List<string>() { "support" }
//    });
//var roadRunnerSupportKey = roadRunnerSupport.Key;

//var deepgramClientOptions = new DeepgramClientOptions(roadRunnerSupportKey);

//var wileysPrerecordedClient = new PrerecordedClient(deepgramClientOptions, httpClient);




//var martianSupport = await manageClient.CreateProjectKeyAsync("acmesolutionproject",
//    new CreateProjectKeySchema()
//    {
//        Comment = "daffy duck support key",
//        Scopes = new List<string>() { "support" }
//    });
//var martianSupportKey = roadRunnerSupport.Key;

