using Qffect.Domain.ADM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qffect.Application.Interfaces
{
    public interface IIncidentRepository
    {
        Task AddAsync(IncidentRecord incident);
    }
}
