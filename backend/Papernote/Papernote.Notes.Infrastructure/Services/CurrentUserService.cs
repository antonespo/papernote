using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Papernote.Notes.Core.Application.Interfaces;

namespace Papernote.Notes.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthUserResolutionService _userResolutionService;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IAuthUserResolutionService userResolutionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userResolutionService = userResolutionService;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("User is not authenticated");

        var subClaim = user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(subClaim) || !Guid.TryParse(subClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user identifier in token");

        return userId;
    }
}