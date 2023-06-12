

using Deepgram;
using Deepgram.Models;

var client = new DeepgramClient(new Deepgram.Credentials { ApiKey = "4e82b9876d99e35f11fc1bff5d79bda00f10e897" }); ;
//var projects = await client.Projects.ListProjectsAsync();
//var projId = projects.Projects[0].Id;
//var project = await client.Projects.GetProjectAsync(projId);
//project.Name = $"{project.Name}_d";
//var updateProj = await client.Projects.UpdateProjectAsync(project);
//var members = await client.Projects.GetMembersAsync(projId);
//var memid = members.Members[1].Id;
//var memebrScope = await client.Projects.GetMemberScopesAsync(projId, memid);
//var updated = await client.Projects.UpdateScopeAsync(projId, memid, new Deepgram.Models.UpdateScopeOptions() { Scope = "member" });

var response = await client.Transcription.Prerecorded.GetTranscriptionAsync(
          new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
          new PrerecordedTranscriptionOptions()
          {
              Punctuate = true,
              Utterances = true,
              Redaction = new[] { "pci", "ssn" }
          });
var output = "";
foreach (var utt in response.Results.Utterances)
{
    foreach (var word in utt.Words)
    {

        output = $"{output} {word.Word} ";
    }

}

Console.WriteLine(output);
Console.ReadLine();