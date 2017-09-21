using MobileSync.Models;
using Newtonsoft.Json;

namespace MobileSync.Models
{
    public class ConflictItem<T> where T : SyncObject
    {
        public T CurrentItem { get; private set; }
        public T RequestedUpdateItem { get; private set; }

        /// <summary>
        /// Used for Conflicting entries due to an update operation
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="requestedUpdateItem"></param>
        [JsonConstructor]
        public ConflictItem(T currentItem, T requestedUpdateItem)
        {
            this.CurrentItem = currentItem;
            this.RequestedUpdateItem = requestedUpdateItem;
        }

        public ConflictItem(T requestedUpdateItem)
        {
            this.CurrentItem = null;
            this.RequestedUpdateItem = requestedUpdateItem;
        }

        public bool IsAnUpdateConflict
        {
            get { return CurrentItem != null; }
        }

		public bool IsADeleteConflict
		{
			get { return CurrentItem == null; }
		}

		public string ConflictMessage
		{
			get 
            {
                return IsAnUpdateConflict 
                    ? "A newer version of this record has been written on the server" 
                        : IsADeleteConflict 
                            ? "The record you are updating has been deleted from the server" 
                            : string.Empty;

            }
		}
    }
}