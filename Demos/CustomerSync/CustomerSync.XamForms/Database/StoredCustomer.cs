using System;
using SQLite;

namespace CustomerSync
{
    public class StoredCustomer
    {
        [PrimaryKey, AutoIncrement]
        public int LocalId { get; set; }

        #region Information that is stored about the item

        [Indexed]
        public string Name { get; set; }

        public string Company { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Notes { get; set; }

        #endregion

        #region Information that is stored about the entity

        public int Id { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime LastUpdateDateTime { get; set; }
        public DateTime DeletedDateTime { get; set; }
        public bool IsDeleted { get; set; }
		public string CorrelationId { get; set; }
        public bool HasChanges { get; set; }

        #endregion
    }
}