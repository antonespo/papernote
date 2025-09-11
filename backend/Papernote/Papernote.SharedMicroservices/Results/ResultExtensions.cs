using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Papernote.SharedMicroservices.Results;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();
        
        return new ObjectResult(new ProblemDetails
        {
            Title = result.ErrorCode ?? "Error",
            Detail = result.Error,
            Status = StatusCodes.Status400BadRequest
        }) { StatusCode = StatusCodes.Status400BadRequest };
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess && result.Value is not null)
            return new OkObjectResult(result.Value);
            
        return new ObjectResult(new ProblemDetails
        {
            Title = result.ErrorCode ?? "Error",
            Detail = result.Error,
            Status = StatusCodes.Status400BadRequest
        }) { StatusCode = StatusCodes.Status400BadRequest };
    }
}