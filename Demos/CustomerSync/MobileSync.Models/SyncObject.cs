using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MobileSync.Models
{
    /// <summary>
    /// A base class for co-ordinating activities between a client and a server. 
    /// </summary>
	public class SyncObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor
        /// </summary>
		public SyncObject() 
        {
			id = 0;
			versionNumber = 1;
			createDateTime = DateTime.UtcNow;
			lastUpdateDateTime = CreateDateTime;
			deletedDateTime = CreateDateTime;
			isDeleted = false;
		}

		public void CreateCorrelationId()
		{
			correlationId = Guid.NewGuid ().ToString ();
		}

		/// <summary>
		/// The primary key of the item to synchronize
		/// </summary>
		public int Id
		{
			get { return id; }
			set {
				if (id != value) {
					id = value;
					RaisePropertyChanged ();
				}
			}
		}

        /// <summary>
        /// The version number of the record being updated. This is used to manage conflicts between updates
        /// </summary>
		public int VersionNumber {
			get { return versionNumber; }
			set {
				if (versionNumber != value) {
					versionNumber = value;
					RaisePropertyChanged ();
				}
			}
		}

        /// <summary>
        /// The date that the record was created in Universal Time
        /// </summary>
		public DateTime CreateDateTime {
			get { return createDateTime; }
			set {
				if (createDateTime != value) {
					createDateTime = value;
					RaisePropertyChanged ();
				}
			}
		}

        /// <summary>
        /// The date that the instance was last updated in Universal Time
        /// </summary>
		public DateTime LastUpdateDateTime {
			get { return lastUpdateDateTime; }
			set {
				if (lastUpdateDateTime != value) {
					lastUpdateDateTime = value;
					RaisePropertyChanged ("LastUpdateDateTime");
				}
			}
		}
        /// <summary>
        /// The date that the instance was last deleted in Universal time
        /// </summary>
		public DateTime DeletedDateTime {
			get { return deletedDateTime; }
			set {
				if (deletedDateTime != value) {
					deletedDateTime = value;
					RaisePropertyChanged ("DeletedDateTime");
				}
			}
		}
        /// <summary>
        /// Whether or not the record has been deleted or not
        /// </summary>
		public bool IsDeleted {
			get { return isDeleted; }
			set {
				if (isDeleted != value) {
					isDeleted = value;
					RaisePropertyChanged ("IsDeleted");
				}
			}
		}

        /// <summary>
        /// Correlation id
        /// </summary>
        /// <value>The correlation identifier.</value>
		public string CorrelationId {
			get { return correlationId; }
			set {
				if (correlationId != value) {
					correlationId = value;
					RaisePropertyChanged ("CorrelationId");
				}
			}
		}

        int id;
        int versionNumber;
        DateTime createDateTime;
        DateTime lastUpdateDateTime;
        DateTime deletedDateTime;
        bool isDeleted;
        string correlationId;

        public event PropertyChangedEventHandler PropertyChanged = delegate{};
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
