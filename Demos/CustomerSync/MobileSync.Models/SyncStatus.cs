namespace MobileSync.Models
{
    public enum SyncStatus : int
    {
        Success = 1,
        Failed = 2,
        PartialSuccessWithConflict = 3
    }
}