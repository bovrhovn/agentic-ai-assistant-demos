using System.ComponentModel.DataAnnotations;

namespace AAI.GenericChatInterface.Options;

public class GeneralOptions
{
    public const string GeneralWebName = "GeneralWeb";
    [Required(ErrorMessage = "ApiBaseUrl is required.")]
    public required string ApiBaseUrl { get; init; }
    public int MessageLengthLimit { get; set; } = 400;
}