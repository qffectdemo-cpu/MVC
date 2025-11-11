using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qffect.Application.Employees.DTOs
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }          // Unique identifier
        public string Name { get; set; }
        public string Email { get; set; } // Employee name
        public string Department { get; set; } // Department name
        public string Status { get; set; }
    }
}
