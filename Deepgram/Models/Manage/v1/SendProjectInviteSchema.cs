namespace Deepgram.Models.Manage.v1;
public class SendProjectInviteSchema(string email, string scope)
{
    /// <summary>
    /// email of the person being invited
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = email;

    /// <summary>
    /// scopes to add for the invited person
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = scope;

}
