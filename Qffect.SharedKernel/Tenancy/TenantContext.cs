namespace Qffect.SharedKernel.Tenancy;

public sealed class TenantContext
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = "";
    public string Edition { get; set; } = "Standard";
    public HashSet<string> EnabledModules { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
