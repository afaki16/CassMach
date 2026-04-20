using CassMach.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using CassMach.Application.Common.Results;

namespace CassMach.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (result.Value == null)
                return NotFound();
            return Ok(new { success = true, data = result.Value });
        }

        return StatusCode(result.Error!.Status, new { success = false, error = result.Error });
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(new { success = true, message = "Operation completed successfully" });

        return StatusCode(result.Error!.Status, new { success = false, error = result.Error });
    }

    protected IActionResult HandlePagedResult<T>(PagedResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new
            {
                success = true,
                data = new
                {
                    items = result.Items,
                    totalCount = result.TotalItems,
                    totalPages = result.TotalPages,
                    pageNumber = result.PageNumber
                }
            });
        }

        return StatusCode(result.Error!.Status, new { success = false, error = result.Error });
    }

    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User ID not found in claims");
        return userId;
    }

    protected string GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    protected string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }
}
