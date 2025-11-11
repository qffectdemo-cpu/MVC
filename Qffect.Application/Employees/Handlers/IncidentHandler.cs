using MediatR;
using Qffect.Application.Commands;
using Qffect.Application.Interfaces;
using Qffect.Domain.ADM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Qffect.Application.Commands.IncidentCommand;

namespace Qffect.Application.Employees.Handlers
{
    public class IncidentHandler : IRequestHandler<CreateIncidentCommand, Guid>
    {
        private readonly IIncidentRepository _repo;

        public IncidentHandler(IIncidentRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
        {
            var incident = new IncidentRecord(request.Title, request.Description, request.DateOccurred);
            incident.SetAuditInfo(request.UserId); // Fill audit info

            await _repo.AddAsync(incident);
            return incident.Id;
        }
    }
}
