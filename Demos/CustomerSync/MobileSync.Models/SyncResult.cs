using System.Collections.Generic;

namespace MobileSync.Models
{
    /// <summary>
    /// Defines the result to send back to the 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SyncResult<T> where T : SyncObject
    {
        public SyncStatus Status { get; set; }

        public string FailureReason { get; set; }

        public ConflictItem<T>[] Conflicts { get; set; }

        public Dictionary<string, int> CorrelationIds { get; set; }

        public Dictionary<int, int> VersionChanges { get; set; } = new Dictionary<int, int>();

        public IList<ItemUpdateHistory> UpdatedHistoryItems { get; set; } = new List<ItemUpdateHistory>();

        public IList<int> DeletedRecords { get; set; } = new List<int>();
    }
}