using Qffect.SharedKernel;

namespace Qffect.Domain.ADM;

public class TaskItem : Entity<int>
{
    public string TaskTitle { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;

    public string? RefModuleCode { get; set; } // 'QPS','EHS',...
    public string? RefEntity { get; set; }     // 'Incident','Ticket',...
    public string? RefId { get; set; }         // external id (INT/GUID as text)

    public string? OwnerEmpId { get; set; }
    public int StatusId { get; set; }
    public int PriorityId { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ClosedAt { get; set; }

    public bool IsDeleted { get; set; }
}
