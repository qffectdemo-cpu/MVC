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
    public class EmployeeRepository:IEmployeeRepository
    {
        private readonly QffectDbContext _db;
        public EmployeeRepository(QffectDbContext db) => _db = db;

        public async Task AddAsync(Employee employee)
        {
            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
            => await _db.Employees.FindAsync(id);

    }
}
