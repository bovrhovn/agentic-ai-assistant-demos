using System.Net.Mime;
using AAI.Core;
using AAI.Interfaces;
using AAI.Models;
using AAI.Rest.Services.DtoModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AAI.Rest.Services.Controllers;

[ApiController, Route(GeneralRoutes.ChatRoute), 
 AllowAnonymous, Produces(MediaTypeNames.Application.Json)]
public class ChatController(ILogger<ChatController> logger, IChatRepository chatRepository) : ControllerBase
{
    [HttpGet]
    [Route(DataRoutes.GenerateThreadNameRoute)]
    [EndpointSummary("Generate a new thread name.")]
    [EndpointDescription(
        "This endpoint is used to generate a new thread name for the chat. It is used by the AAI chat interface to create a new thread.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GenerateNewThreadName()
    {
        logger.LogInformation("Called save endpoint at {DateCalled}", DateTime.UtcNow);
        return Ok(StringHelper.GenerateUniqueName());
    }
    
    [HttpPost]
    [Route(DataRoutes.SaveChatRoute)]
    [EndpointSummary("Save chat to the repository.")]
    [EndpointDescription(
        "This endpoint is used to save a chat to the repository. It is used by the AAI chat interface to save the chat history.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveChatAsync([FromBody] ChatDto chatDto)
    {
        logger.LogInformation("Called save endpoint at {DateCalled}", DateTime.UtcNow);
        var chat = new Chat
        {
            ChatId = Guid.NewGuid().ToString(),
            UserId = chatDto.Email,
            ThreadName = chatDto.ThreadName,
            Text = chatDto.Text,
            ParentChat = new Chat
            {
                ChatId = chatDto.ParentId,
                UserId = chatDto.Email, 
                Text = "",
                ThreadName = chatDto.ThreadName
            },
            DatePosted = new DateTime()
        };
        if (!await chatRepository.SaveAsync(chat)) return BadRequest($"Failed to save chat at {DateTime.Now}.");

        logger.LogInformation("Chat saved successfully at {DateSaved}", DateTime.UtcNow);
        return Ok();
    }

    [HttpGet]
    [Route(GeneralRoutes.HealthRoute)]
    [EndpointSummary("This is a health check for the chat controller.")]
    [EndpointDescription(
        "This is a health check for the AAI chat controller to see if it is online and all system are workings.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult IsAlive()
    {
        logger.LogInformation("Called alive data endpoint fpr chat controller at {DateCalled}", DateTime.UtcNow);
        return new ContentResult
            { StatusCode = 200, Content = $"Chat controller is alive at {DateTime.Now} on {Environment.MachineName}" };
    }
}