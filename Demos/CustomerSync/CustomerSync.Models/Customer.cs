using MobileSync.Models;
using Newtonsoft.Json;
using CustomerSync.Models;

[assembly:Preserve]

namespace CustomerSync.Models
{
    public class Customer : SyncObject
    {
        [JsonConstructor]
        public Customer () : base ()
        {
            name = string.Empty;
        }

		public string Name {
			get { return name; }
			set {
				if (name != value) {
					name = value;
					RaisePropertyChanged ();
				}
			}
		}

		public string Company {
			get { return company; }
			set {
				if (company != value) {
					company = value;
					RaisePropertyChanged ();
				}
			}
		}

		public string Title {
			get { return title; }
			set {
				if (title != value) {
					title = value;
					RaisePropertyChanged ();
				}
			}
		}

       

		public string Email {
			get { return email; }
			set {
				if (email != value) {
					email = value;
					RaisePropertyChanged ();
				}
			}
		}

		public string Phone {
			get { return phone; }
			set {
				if (phone != value) {
					phone = value;
					RaisePropertyChanged ();
				}
			}
		}


		public string Notes {
			get { return note; }
			set {
				if (note != value) {
					note = value;
					RaisePropertyChanged ();
				}
			}
		}

        public override string ToString()
        {
            return Name + ", " + Company;
        }

        string name;
        string company;
        string title;
        string email;
        string phone;
        string note;

        public Customer Clone()
        {
            return new Customer()
            {
                Company = this.Company,
                CorrelationId = this.CorrelationId,
                CreateDateTime = this.CreateDateTime,
                DeletedDateTime = this.DeletedDateTime,
                Email = this.Email,
                Id = this.Id,
                IsDeleted = this.IsDeleted,
                LastUpdateDateTime = this.LastUpdateDateTime,
                Name = this.Name,
                Notes = this.Notes,
                Phone = this.Phone,
                Title = this.Title,
                VersionNumber = this.VersionNumber
            };
        }
    }
}
