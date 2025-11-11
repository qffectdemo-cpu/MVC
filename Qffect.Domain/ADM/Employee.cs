using Qffect.SharedKernel.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qffect.Domain.ADM
{
    [Auditable]
    public class Employee:AuditableEntity
    {
        public int Id { get; set; }             // Primary Key
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
    }
}
