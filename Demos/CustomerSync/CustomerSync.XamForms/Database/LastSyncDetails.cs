using System;
using SQLite;

namespace CustomerSync
{
    public class LastSyncDetails
    {
        [PrimaryKey]
        public int Id { get; set; }

        public DateTime LastSyncDateTime { get; set; }
    }    
}