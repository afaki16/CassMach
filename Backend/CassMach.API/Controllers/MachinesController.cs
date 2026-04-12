using CassMach.Application.Features.Machines.Commands.CreateMachine;
using CassMach.Application.Features.Machines.Commands.DeleteMachine;
using CassMach.Application.Features.Machines.Commands.UpdateMachine;
using CassMach.Application.Features.Machines.Dtos;
using CassMach.Application.Features.Machines.Queries.GetUserMachines;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CassMach.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MachinesController : BaseController
    {
        private readonly IMediator _mediator;

        public MachinesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = "machines.read")]
        public async Task<IActionResult> GetMyMachines()
        {
            var query = new GetUserMachinesQuery { UserId = GetCurrentUserId() };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        [HttpPost]
        [Authorize(Policy = "machines.create")]
        public async Task<IActionResult> CreateMachine([FromBody] CreateMachineDto dto)
        {
            var command = new CreateMachineCommand
            {
                UserId = GetCurrentUserId(),
                Brand = dto.Brand,
                Model = dto.Model,
                Name = dto.Name
            };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "machines.update")]
        public async Task<IActionResult> UpdateMachine(int id, [FromBody] UpdateMachineDto dto)
        {
            var command = new UpdateMachineCommand
            {
                Id = id,
                UserId = GetCurrentUserId(),
                Brand = dto.Brand,
                Model = dto.Model,
                Name = dto.Name
            };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "machines.delete")]
        public async Task<IActionResult> DeleteMachine(int id)
        {
            var command = new DeleteMachineCommand { Id = id, UserId = GetCurrentUserId() };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}
