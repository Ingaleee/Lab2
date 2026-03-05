using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OrderTracking.Presentation.Api.Controllers;

/// <summary>
/// Error controller for exception handling.
/// </summary>
[ApiController]
[Route("[controller]")]
public sealed class ErrorController : ControllerBase
{
    /// <summary>
    /// Handles errors and returns problem details.
    /// </summary>
    /// <returns>Problem details.</returns>
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpPatch]
    public IActionResult Error()
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        return Problem();
    }
}
