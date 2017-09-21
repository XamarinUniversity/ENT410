using System;
using CustomerSync.Models;
using MobileSync.Server;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Text;
using MobileSync.Models;
using System.Threading.Tasks;

namespace CustomerSync.Server
{
    /// <summary>
    /// An implementation of the class that works directly with a back-end database. It is using MSSQL in this case.
    /// The type of back-end storage is irrelivant,  you could have code using EF, you could have legacy support. 
    /// This is just one example to demonstrate that there are existing options
    /// </summary>
    public class DatabaseCustomerDataSync : BaseServerSync<Customer>
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        RemoteCustomerContext db;

        protected async override Task SetupAsync()
        {
            db = new RemoteCustomerContext();
        }

        protected async override Task CommitAsync()
        {
            await db.SaveChangesAsync();

            // Update the correlations. For EF we need to wait for the Save Changes to have
            // access to the created Ids
            foreach (var correlationId in insertedItems.Keys)
            {
                correlationIds[correlationId] = insertedItems[correlationId].Id;
            }

            db.Dispose();
        }

        protected async override Task RollbackAsync()
        {
            db.Dispose();
        }

        Customer RemoteCustomerToCustomer(RemoteCustomer rc)
        {
            return new Customer()
            {
                Id = rc.Id,
                Company = rc.Company,
                CreateDateTime = rc.CreatedDateTime,
                DeletedDateTime = rc.DeletedDateTime,
                Email = rc.Email,
                IsDeleted = rc.IsDeleted,
                LastUpdateDateTime = rc.LastUpdateDateTime,
                Name = rc.Name,
                Notes = rc.Notes,
                Phone = rc.Phone,
                Title = rc.Title,
                VersionNumber = rc.VersionNumber
            };
        }

        RemoteCustomer CustomerToRemoteCustomer(Customer c)
        {
            return new RemoteCustomer()
            {
                Id = c.Id,
                Company = c.Company,
                CreatedDateTime = c.CreateDateTime,
                DeletedDateTime = c.DeletedDateTime,
                Email = c.Email,
                IsDeleted = c.IsDeleted,
                LastUpdateDateTime = c.LastUpdateDateTime,
                Name = c.Name,
                Notes = c.Notes,
                Phone = c.Phone,
                Title = c.Title,
                VersionNumber = c.VersionNumber
            };
        }

        public async override Task<IList<Customer>> GetItemsAsync(string userToken, bool markAsViewed)
        {
            using (db = new RemoteCustomerContext())
            {
                var customers = db.Customers
                    .Select(rc => RemoteCustomerToCustomer(rc))
                    .ToList();
                logger.Debug(String.Format("GetCustomers: {0}", customers.Dump()));

                foreach (var c in customers)
                {
                    await this.MarkAsViewedAsync(c, userToken);
                }

                return customers;
            }
        }

        public async override Task<Customer> GetItemAsync(int id, string userToken, bool markAsViewed)
        {
            logger.Debug(String.Format("GetItem {0}", id));

            var customer = db.Customers
                .Where(rc => rc.Id == id)
                .Select(rc => RemoteCustomerToCustomer(rc))
                .FirstOrDefault();
            logger.Debug(String.Format("GetItem: {0}", customer.Dump()));

            await MarkAsViewedAsync(customer, userToken);

            return customer;
        }

        Dictionary<string, RemoteCustomer> insertedItems = new Dictionary<string, RemoteCustomer>();

        public async override Task<int> InsertAsync(Customer item)
        {         
            var rc = CustomerToRemoteCustomer(item);
            db.Add(rc);
            insertedItems[item.CorrelationId] = rc;
            logger.Debug(String.Format("Insert {0}", item.Dump()));
            return rc.Id;
        }

        public async override Task UpdateAsync(Customer item)
        {
            var rc = CustomerToRemoteCustomer(item);
            db.Update(rc);
            logger.Debug(String.Format("Update {0}", item.Id));
        }

        public async override Task DeleteAsync(Customer item)
        {
            var c = new RemoteCustomer() { Id = item.Id };
            db.Remove(c);
            logger.Debug(String.Format("Delete {0}", item.Id));
        }

        public async override Task<List<ItemUpdateHistory>> GetRecordsUpdatedByOthersAsync(string userIdentifier)
        {
            return null;
        }

        public async override Task MarkAsViewedAsync(Customer item, string userToken)
        {
            var customerHistory = db.CustomerItemHistory
                .Where(h => h.RecordId == item.Id && h.UserIdentifier == userToken)
                .FirstOrDefault();

            if (customerHistory == null)
            {
                customerHistory = new RemoteCustomerItemHistory()
                {
                    RecordId = item.Id,
                    UpdateHistoryType = ItemUpdatedHistoryType.NoChange,
                    UserIdentifier = userToken,
                    VersionNumber = item.VersionNumber,
                    UpdateDateTime = DateTime.Now
                };

                db.CustomerItemHistory.Add(customerHistory);
            }
            else
            {
                customerHistory.UpdateHistoryType = ItemUpdatedHistoryType.NoChange;
                customerHistory.UpdateDateTime = DateTime.Now;
                customerHistory.VersionNumber = item.VersionNumber;

                db.CustomerItemHistory.Update(customerHistory);
            }
        
            logger.Debug($"Logged Marked as Viewed {item.Id}");
        }

        public async override Task MarkAsDeletedAsync(Customer item, string userToken)
        {
            var customerHistory = db.CustomerItemHistory
                .Where(h => h.RecordId == item.Id && h.UserIdentifier == userToken)
                .FirstOrDefault();

            if (customerHistory == null)
            {
                customerHistory = new RemoteCustomerItemHistory()
                {
                    RecordId = item.Id,
                    UpdateHistoryType = ItemUpdatedHistoryType.Deleted,
                    UserIdentifier = userToken,
                    VersionNumber = 0,
                    UpdateDateTime = DateTime.Now                        
                };

                db.CustomerItemHistory.Add(customerHistory);
            }
            else
            {
                customerHistory.UpdateHistoryType = ItemUpdatedHistoryType.Deleted;
                customerHistory.UpdateDateTime = DateTime.Now;
                db.CustomerItemHistory.Update(customerHistory);
            }

            logger.Debug($"Logged Marked as Deleted {item.Id}");
        }

        public async override Task MarkAsUpdatedAsync(Customer item, string userToken)
        {
            var customerHistory = db.CustomerItemHistory
                .Where(h => h.RecordId == item.Id && h.UserIdentifier == userToken)
                .FirstOrDefault();

            if (customerHistory == null)
            {
                customerHistory = new RemoteCustomerItemHistory()
                {
                    RecordId = item.Id,
                    UpdateHistoryType = ItemUpdatedHistoryType.Updated,
                    UserIdentifier = userToken,
                    VersionNumber = item.VersionNumber,
                    UpdateDateTime = DateTime.Now
                };

                db.CustomerItemHistory.Add(customerHistory);
            }
            else
            {
                customerHistory.UpdateHistoryType = ItemUpdatedHistoryType.Updated;
                customerHistory.UpdateDateTime = DateTime.Now;
                customerHistory.VersionNumber = item.VersionNumber;

                db.CustomerItemHistory.Update(customerHistory);
            }

            logger.Debug($"Logged Marked as Updated {item.Id} ver: {item.VersionNumber}");
        }

        /// <summary>
        /// A method used to get the history entry for a record/user. Mainly used to 
        /// confirm appropriate logging in the unit tests
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userToken"></param>
        /// <returns></returns>
        public async Task<RemoteCustomerItemHistory> GetHistory(int id, string userToken)
        {
            return db.CustomerItemHistory
                .Where(h => h.RecordId == id && h.UserIdentifier == userToken)
                .FirstOrDefault();
        }
    }
}
