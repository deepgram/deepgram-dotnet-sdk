namespace Deepgram.Models;
public class SendProjectInviteSchema
{
    public SendProjectInviteSchema(string email, string scope)
    {
        Email = email;
        Scope = scope;
    }
    /// <summary>
    /// email of the person being invited
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; }

    /// <summary>
    /// scopes to add for the invited person
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; }

}
