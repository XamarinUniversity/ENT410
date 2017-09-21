using System;

namespace MobileSync.Models
{
    /// <summary>
    /// Inidicates what has happened to the record for the user.  
    /// </summary>
    public enum ItemUpdatedHistoryType
    {
        NoChange = 0,
        Updated = 1,
        Deleted = 2
    }

    /// <summary>
    /// Represents an update from the server as to what changes have occurred, so that when data
    /// is being synchronised, we can also see what data has been modified by other users in the system
    /// If you are developing a system where each user has their own copy of the data per-device, this
    /// mechanism may not be needed. 
    /// 
    /// From an implementation point of view, you may need to have your own copy of this table per
    /// DTO. For example, with customers you would want to use something like:
    ///     
    ///     public class CustomerItemUpdateHistory : ItemUpdateHistory
    /// 
    /// to minimize the look up time for each of the entries. 
    /// </summary>
    public class ItemUpdateHistory
    {
        public int Id { get; set; }

        public string UserIdentifier { get; set; }

        public int RecordId { get; set; }

        public int VersionNumber { get; set; }

        public DateTime UpdateDateTime { get; set; }

        public ItemUpdatedHistoryType UpdateHistoryType { get; set; }
    }
}
