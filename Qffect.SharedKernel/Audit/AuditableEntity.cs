using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qffect.SharedKernel.Audit
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; protected set; }
        public string? CreatedBy { get; protected set; }
        public DateTime? ModifiedAt { get; protected set; }
        public string? ModifiedBy { get; protected set; }
        public string RowHmac { get; protected set; } = "";

        // Call this whenever you create or update
        public void SetAuditInfo(string userId)
        {
            if (CreatedAt == default)
                CreatedAt = DateTime.UtcNow;

            CreatedBy = string.IsNullOrWhiteSpace(CreatedBy) ? userId : CreatedBy;
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = userId;

            // Simple RowHmac example
            RowHmac = ComputeHmac();
        }

        private string ComputeHmac()
        {
            // Simple string hash based on property values (for demo)
            var raw = $"{CreatedAt}-{CreatedBy}-{ModifiedAt}-{ModifiedBy}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));
        }
    }
}
