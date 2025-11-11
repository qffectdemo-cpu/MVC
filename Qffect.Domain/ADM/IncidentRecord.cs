using Qffect.SharedKernel.Audit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qffect.Domain.ADM
{
   
    public class IncidentRecord: AuditableEntity
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateTime DateOccurred { get; private set; }

        public IncidentRecord(string title, string description, DateTime dateOccurred)
        {
            Title = title;
            Description = description;
            DateOccurred = dateOccurred;
        }

        public void Update(string title, string description)
        {
            Title = title;
            Description = description;
        }
    }
}
