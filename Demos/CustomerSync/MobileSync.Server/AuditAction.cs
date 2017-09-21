namespace MobileSync.Server
{
    public enum AuditAction : int
    {
        Insert = 1,
        Update = 2,
        Delete = 3,
        ConflictOverride = 4
    }
}