using System.ComponentModel.DataAnnotations;

namespace AAI.Core;

public class AuthApiOptions
{
    public const string AuthOptionsSectionName = "AuthApiOptions";
    public const string ApiKeyHeaderName = "X-Api-Key";
    [Required(ErrorMessage = "Api key must be defined")] 
    public required string ApiKey { get; init; }
    public required string BaseUrl { get; init; }
}