using CassMach.Application.Features.UserMachines.Commands.AddUserMachine;
using CassMach.Application.Features.UserMachines.Commands.RemoveUserMachine;
using CassMach.Application.Features.UserMachines.Queries.GetMyMachines;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CassMach.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserMachinesController : BaseController
    {
        private readonly IMediator _mediator;

        public UserMachinesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Kullanıcının eşleştirdiği makineleri getirir
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "usermachines.read")]
        public async Task<IActionResult> GetMyMachines()
        {
            var query = new GetMyMachinesQuery { UserId = GetCurrentUserId() };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Katalogdan makine ekler (eşleştirir)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "usermachines.create")]
        public async Task<IActionResult> AddMachine([FromBody] AddUserMachineRequest request)
        {
            var command = new AddUserMachineCommand
            {
                UserId = GetCurrentUserId(),
                MachineId = request.MachineId,
                Name = request.Name
            };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Kullanıcının listesinden makine kaldırır
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "usermachines.delete")]
        public async Task<IActionResult> RemoveMachine(int id)
        {
            var command = new RemoveUserMachineCommand { Id = id, UserId = GetCurrentUserId() };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }

    public class AddUserMachineRequest
    {
        public int MachineId { get; set; }
        public string? Name { get; set; }
    }
}
