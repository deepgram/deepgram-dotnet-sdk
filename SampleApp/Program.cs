
var API_KEY = "61290eebdcaf7f458f8adaf8bbc77fc700afb6e6";

var deepgram = new DeepgramClient();

var projectList = await deepgram.Projects.ListProjectsAsync();

if (projectList.Projects.Count > 0)
{

    var firstProject = projectList.Projects[0];

    Console.WriteLine(firstProject.Name);

    firstProject.Name = "A different name";

    var result = await deepgram.Projects.UpdateProjectAsync(firstProject);

    var newkey = await deepgram.Keys.CreateKeyAsync(firstProject.Id, "test key dotnet", new List<string>() { "member" });

    var result2 = await deepgram.Keys.DeleteKeyAsync(firstProject.Id, newkey.Id);


}