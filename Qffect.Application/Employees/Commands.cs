using MediatR;
using Qffect.Domain.ADM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qffect.Application.Employees
{
    public  class Commands
    {
        public record CreateEmployeeCommand(string Name, string Email, string Department) : IRequest<int>;
        public record GetEmployeeCommand(int Id) : IRequest<Employee?>;


    }
}
