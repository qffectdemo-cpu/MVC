namespace Qffect.SharedKernel.Security;

public static class Permissions
{
    public static class QPS
    {
        public const string IncidentRead = "QPS.Incidents.Read";
        public const string IncidentWrite = "QPS.Incidents.Write";
        public const string RiskRead = "QPS.Risks.Read";
        public const string RiskWrite = "QPS.Risks.Write";
    }

    public static class ADM
    {
        public const string TaskRead = "ADM.Tasks.Read";
        public const string TaskWrite = "ADM.Tasks.Write";
    }

    // Add EHS/MMS/CRM/PDM/SCM/FBM/OPS similarly
}
