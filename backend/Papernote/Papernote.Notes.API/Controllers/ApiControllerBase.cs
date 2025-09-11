using Microsoft.AspNetCore.Mvc;

namespace Papernote.Notes.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    // Add shared logic for all controllers here if needed
}
