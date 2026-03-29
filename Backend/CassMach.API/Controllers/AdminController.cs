using CassMach.Application.Features.Admin.Commands.GiftTokens;
using CassMach.Application.Features.Admin.Commands.TopUpTokens;
using CassMach.Application.Features.Admin.Commands.UpdateSetting;
using CassMach.Application.Features.Admin.Commands.RevokeUserSessions;
using CassMach.Application.Features.Admin.Dtos;
using CassMach.Application.Features.Admin.Queries.GetActiveUserCount;
using CassMach.Application.Features.Admin.Queries.GetActiveUsersSnapshot;
using CassMach.Application.Features.Admin.Queries.GetAllSettings;
using CassMach.Application.Features.Admin.Queries.GetAllUsersAdmin;
using CassMach.Application.Features.Admin.Queries.GetDashboard;
using CassMach.Application.Features.Admin.Queries.GetRevokableUsers;
using CassMach.Application.Features.Admin.Queries.GetUserUsage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CassMach.API.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AdminController : BaseController
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get count of users with at least one active session.
        /// SuperAdmin: pass tenantId or omit for all tenants. Admin: returns count for own tenant only.
        /// </summary>
        /// <param name="tenantId">Optional. When null, SuperAdmin gets total across all tenants.</param>
        /// <returns>Active user count</returns>
        [HttpGet("active-users-count")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetActiveUserCount([FromQuery] int? tenantId = null)
        {
            var query = new GetActiveUserCountQuery { TenantId = tenantId };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Users with at least one active refresh token (session), as a table row per user.
        /// Admin: own tenant only. SuperAdmin: all tenants, optional tenantId filter.
        /// </summary>
        [HttpGet("active-users")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetActiveUsersSnapshot([FromQuery] int? tenantId = null)
        {
            var query = new GetActiveUsersSnapshotQuery { TenantId = tenantId };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get users that can be revoked (active sessions, excluding SuperAdmin).
        /// For dropdown in remote logout UI.
        /// </summary>
        [HttpGet("revokable-users")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetRevokableUsers([FromQuery] int? tenantId = null)
        {
            var query = new GetRevokableUsersQuery { TenantId = tenantId };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Revoke all sessions for a user (admin logout from all devices).
        /// Admin can only revoke users in their tenant. SuperAdmin can revoke any user.
        /// </summary>
        /// <param name="userId">User ID to revoke sessions for</param>
        /// <param name="reason">Optional reason for revocation</param>
        /// <returns>Success message</returns>
        [HttpPost("users/{userId:int}/revoke-sessions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RevokeUserSessions(int userId, [FromQuery] string? reason = null)
        {
            var command = new RevokeUserSessionsCommand { UserId = userId, Reason = reason };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet("ai/users")]
        [Authorize(Policy = "adminpanel.read")]
        public async Task<IActionResult> GetAllUsersAdmin([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string searchTerm = null)
        {
            var query = new GetAllUsersAdminQuery { Page = page, PageSize = pageSize, SearchTerm = searchTerm };
            var result = await _mediator.Send(query);
            return HandlePagedResult(result);
        }

        [HttpGet("ai/users/{userId:int}/usage")]
        [Authorize(Policy = "adminpanel.read")]
        public async Task<IActionResult> GetUserUsage(int userId)
        {
            var query = new GetUserUsageQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        [HttpPost("ai/users/{userId:int}/topup")]
        [Authorize(Policy = "adminpanel.create")]
        public async Task<IActionResult> TopUp(int userId, [FromBody] TopUpDto dto)
        {
            var command = new TopUpTokensCommand
            {
                UserId = userId,
                CreditAmount = dto.CreditAmount,
                Description = dto.Description,
                AdminUserId = GetCurrentUserId()
            };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPost("ai/users/{userId:int}/gift")]
        [Authorize(Policy = "adminpanel.create")]
        public async Task<IActionResult> GiftTokens(int userId, [FromBody] GiftTokensDto dto)
        {
            var command = new GiftTokensCommand
            {
                UserId = userId,
                CreditAmount = dto.CreditAmount,
                Description = dto.Description,
                AdminUserId = GetCurrentUserId()
            };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet("ai/settings")]
        [Authorize(Policy = "adminpanel.read")]
        public async Task<IActionResult> GetSettings()
        {
            var query = new GetAllSettingsQuery();
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        [HttpPatch("ai/settings/{key}")]
        [Authorize(Policy = "adminpanel.update")]
        public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingDto dto)
        {
            var command = new UpdateSettingCommand
            {
                Key = key,
                Value = dto.Value,
                AdminUserId = GetCurrentUserId()
            };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet("ai/dashboard")]
        [Authorize(Policy = "adminpanel.read")]
        public async Task<IActionResult> GetDashboard()
        {
            var query = new GetDashboardQuery();
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }
    }
}
