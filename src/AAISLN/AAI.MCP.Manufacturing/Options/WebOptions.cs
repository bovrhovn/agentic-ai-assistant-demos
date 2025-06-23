using System.ComponentModel.DataAnnotations;

namespace AAI.MCP.Manufacturing.Options;

public class WebOptions
{
    public const string WebSettingsName = "WebOptions";
    [Required(ErrorMessage = "Base URL is required.")]
    public required string BaseUrl { get; init; }
}