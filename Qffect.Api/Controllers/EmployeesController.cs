using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Qffect.Application.Employees.DTOs;
using static Qffect.Application.Employees.Commands;

namespace Qffect.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EmployeesController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeDto dto)
        {
            var id = await _mediator.Send(new CreateEmployeeCommand(dto.Name, dto.Email, dto.Department));
            return CreatedAtAction(nameof(Get), new { id }, new { Id = id });
        }

        [HttpGet("{id:int}")]

        public async Task<IActionResult> Get(int id)
        {
            var employee = await _mediator.Send(new GetEmployeeCommand(id));
            return Ok(employee);
        }
    }
}
