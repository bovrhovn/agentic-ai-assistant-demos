using AAI.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AAI.Rest.Services.Controllers;

[ApiController, AllowAnonymous, Route(GeneralRoutes.GeneralRoute)]
public class GeneralController(ILogger<GeneralController> logger) : ControllerBase
{
    [HttpGet]
    [Route(GeneralRoutes.ErrorRoute)]
    [EndpointSummary("This is a health check for the chat controller.")]
    [EndpointDescription(
        "This is a health check for the AAI chat controller to see if it is online and all system are workings.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ErrorInformation()
    {
        logger.LogInformation("Called error endpoint for services controller at {DateCalled}", DateTime.UtcNow);
        return new ContentResult
        {
            StatusCode = 200,
            Content = $"Error happened {DateTime.Now} on {Environment.MachineName}. We are investigating errors."
        };
    }
}