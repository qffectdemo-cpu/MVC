using Microsoft.AspNetCore.Identity;
using System;

namespace Qffect.Domain.ADM;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
