using Qffect.Application.Interfaces;
using Qffect.Domain.ADM;
using Qffect.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qffect.Infrastructure.Repositories
{
    public class IncidentRepository:IIncidentRepository
    {
        private readonly QffectDbContext _ctx;
        public IncidentRepository(QffectDbContext ctx) => _ctx = ctx;

        public async Task AddAsync(IncidentRecord incident)
        {
            _ctx.Set<IncidentRecord>().Add(incident);
            await _ctx.SaveChangesAsync();
        }
    }
}
