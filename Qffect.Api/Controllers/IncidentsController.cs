using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Qffect.Application.Commands.IncidentCommand;

namespace Qffect.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IncidentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIncidentCommand cmd)
        {
            var id = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(Get), new { id }, new { Id = id });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get([FromServices] Qffect.Infrastructure.Persistence.QffectDbContext ctx, Guid id)
        {
            var incident = await ctx.Set<Qffect.Domain.ADM.IncidentRecord>().FindAsync(id);
            if (incident == null) return NotFound();
            return Ok(incident);
        }
    }
}
