using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qffect.Application.Commands
{
    public class IncidentCommand
    {
        public record CreateIncidentCommand(string Title, string Description, DateTime DateOccurred, string UserId)
       : IRequest<Guid>;
    }
}
