using MediatR;
using Qffect.Application.Interfaces;
using Qffect.Domain.ADM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Qffect.Application.Employees.Commands;

namespace Qffect.Application.Employees.Handlers
{
    public class EmployeeHandler: IRequestHandler<CreateEmployeeCommand, int>, IRequestHandler<GetEmployeeCommand, Employee>
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeHandler(IEmployeeRepository repo) => _repo = repo;

        public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            // Business rule example: name required
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required.");

            var employee = new Employee
            {
                // Assuming Employee uses settable properties as per your last entity sample.
                Name = request.Name,
                Email = request.Email,
                Department = request.Department,
                DateJoined = DateTime.UtcNow
            };

            await _repo.AddAsync(employee);

            // assuming Id is int and set by DB; EF will populate employee.Id after SaveChanges
            return employee.Id;
        }


        public async Task<Employee?> Handle(GetEmployeeCommand request, CancellationToken cancellationToken)
        {
            // Retrieve employee by ID from database
            var employee = await _repo.GetByIdAsync(request.Id);
            return employee; // Can be null if not found
        }
    }
}
