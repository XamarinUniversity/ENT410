using SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;
using CustomerSync.Models;
using System;
using System.Linq;
using MobileSync.Models;

namespace CustomerSync
{
    public class DataManager
    {
        SQLiteAsyncConnection db;

        public DataManager(string filename)
        {
            db = new SQLiteAsyncConnection(filename);
            SetupDatabaseAsync();
        }

        public async Task SetupDatabaseAsync ()
        {
            await db.CreateTableAsync<StoredCustomer>().ConfigureAwait (false);
            await db.CreateTableAsync<LastSyncDetails>();
        }

        public async Task<List<StoredCustomer>> GetStoredCustomersAsync ()
        {
            return await db.Table<StoredCustomer> ().ToListAsync ();
        }

        public async Task RemoveCustomersAsync()
        {
            await db.DropTableAsync<StoredCustomer>().ConfigureAwait(false);
            await db.CreateTableAsync<StoredCustomer>().ConfigureAwait (false);
        }

        StoredCustomer CustomerToStoredCustomer(Customer c)
        {
            return new StoredCustomer {
                Name = c.Name,
                Company = c.Company,
                Title = c.Title,
                Email = c.Email,
                Phone = c.Phone,
                Notes = c.Notes,
                Id = c.Id,
                VersionNumber = c.VersionNumber,
                CreateDateTime = c.CreateDateTime,
                LastUpdateDateTime = c.LastUpdateDateTime,
                DeletedDateTime = c.DeletedDateTime,
                IsDeleted = c.IsDeleted,
                CorrelationId = c.CorrelationId
            };
        }

        Customer StoredCustomerToCustomer(StoredCustomer c)
        {
            return new Customer {
                Name = c.Name,
                Company = c.Company,
                Title = c.Title,
                Email = c.Email,
                Phone = c.Phone,
                Notes = c.Notes,
                Id = c.Id,
                VersionNumber = c.VersionNumber,
                CreateDateTime = c.CreateDateTime,
                LastUpdateDateTime = c.LastUpdateDateTime,
                DeletedDateTime = c.DeletedDateTime,
                IsDeleted = c.IsDeleted,
                CorrelationId = c.CorrelationId
            };
        }

        public async Task UpdateCustomersAsync(IEnumerable<Customer> customers)
        {
            // We need to delete all the items, well the ones that have not been updated since the last sync
            var lastSyncDate = await GetLastSyncDateTimeAsync ();

            // We should only delete those objects that have expired
            //          var itemsToRemove = await Table<StoredCustomer> ().Where (c => c.LastUpdateDateTime == lastSyncDate)
            //              .ToListAsync()
            //              .ConfigureAwait(false);
            //
            //          // Delete the items that should be removed
            //          foreach (var item in itemsToRemove)
            //              await DeleteAsync (item).ConfigureAwait(false);

            var storedCustomersFromServer = customers.Select(c => CustomerToStoredCustomer(c)).ToList();

            foreach (var sc in storedCustomersFromServer) 
            {
                var matchingLocalCustomer = 
                    await GetCustomerAsync(sc.Id).ConfigureAwait(false);

                sc.HasChanges = false;
                if (matchingLocalCustomer == null) 
                {
                    await db.InsertAsync (sc);
                } else if (matchingLocalCustomer.HasChanges == false) 
                {
                    await db.DeleteAsync(matchingLocalCustomer);
                    await db.InsertAsync(sc);
                }
            }
        }

        public async Task<StoredCustomer> GetCustomerAsync (string correlationId)
        {
            return await db.Table<StoredCustomer>()
                .Where(c => (c.CorrelationId == correlationId))
                .FirstOrDefaultAsync();
        }

        public async Task<StoredCustomer> GetCustomerAsync (int id)
        {
            return await db.Table<StoredCustomer>()
                .Where(c => (c.Id == id))
                .FirstOrDefaultAsync();
        }

        public async Task DeleteCustomer(Customer customer)
        {
            var c = await GetCustomerAsync(customer.Id).ConfigureAwait(false);

            if (c == null)
                return;

            c.DeletedDateTime = DateTime.UtcNow;
            c.IsDeleted = true;
            c.HasChanges = true;

            await db.UpdateAsync(c);

            // Update the local reference
            customer.DeletedDateTime = DateTime.UtcNow;
            customer.IsDeleted = true;
        }

        public async Task SaveCustomer(Customer customer)
        {
            StoredCustomer c;
            if (customer.Id > 0)
                c = await GetCustomerAsync(customer.Id).ConfigureAwait(false);
            else
                c = await GetCustomerAsync(customer.CorrelationId).ConfigureAwait(false);

            if (c == null) 
            {
                // We have a new entry, just add that to the item to sync, unless we are editing locally
                c = CustomerToStoredCustomer(customer);
                c.HasChanges = true;
                await db.InsertAsync (c);
            } 
            else 
            {
                // Update the entry in the system
                c.Name = customer.Name;
                c.Company = customer.Company;
                c.Title = customer.Title;
                c.Email = customer.Email;
                c.Phone = customer.Phone;
                c.Notes = customer.Notes;
                c.LastUpdateDateTime = DateTime.UtcNow;
                c.HasChanges = true;
                await db.UpdateAsync (c);
            }
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            var customers = await db.Table<StoredCustomer>().ToListAsync().ConfigureAwait(false);
            return await Task.Run(() => 
                customers.Select(c => StoredCustomerToCustomer(c)).ToList());
        }

        private async Task<DateTime> GetLastSyncDateTimeAsync()
        {
            var item = await db.Table<LastSyncDetails> ().FirstOrDefaultAsync ().ConfigureAwait(false);
            if (item == null)
                return DateTime.MinValue;
            else
                return item.LastSyncDateTime;
        }

        public async Task UpdateSyncDateTimeAsync(DateTime value)
        {
            var item = await db.Table<LastSyncDetails>()
                .FirstOrDefaultAsync ()
                .ConfigureAwait(false);

            if (item == null) 
            {
                var details = new LastSyncDetails 
                {
                    Id = 1,
                    LastSyncDateTime = value
                };
                await db.InsertAsync (details);
            } 
            else 
            {
                item.LastSyncDateTime = value;
                await db.UpdateAsync (item);
            }
        }

        public async Task<List<Customer>> GetSyncableCustomersAsync()
        {
            // Need to include a list of the customers who are inserted, or who have been deleted or updated since the last sync date/time
            var lastSyncDate = await GetLastSyncDateTimeAsync ();

            // Get the customers that we are using 
            var matchingItems = await db.Table<StoredCustomer> ().Where (
                c => 
                (c.Id == 0) || // Get the insert operations 
                (c.Id > 0 && c.HasChanges) || // Get the update operations
                (c.IsDeleted && c.Id > 0 && c.HasChanges) // Get the delete operation
            ).ToListAsync().ConfigureAwait(false);

            var asCustomers = matchingItems
                .Select (sc => StoredCustomerToCustomer (sc));

            return asCustomers.ToList();
        }

        public async Task UpdateCorrelationIds(Dictionary<string, int> items)
        {
            foreach (var key in items.Keys) 
            {
                var item = await GetCustomerAsync(key);
                if (item == null)
                    throw new SyncException("A correlation id can no longer be found");

                item.Id = items[key];
                item.HasChanges = false;

                await db.UpdateAsync(item).ConfigureAwait(false);
            }
        }

        public async Task UpdateVersionHistory(Dictionary<int, int> versionChanges)
        {
            foreach (var key in versionChanges.Keys)
            {
                var item = await GetCustomerAsync(key);
                if (item == null)
                    throw new SyncException("A correlation id can no longer be found");

                item.VersionNumber = versionChanges[key];
                item.HasChanges = false;

                await db.UpdateAsync(item).ConfigureAwait(false);       
            }
        }

        public async Task ResetAllData()
        {
            await db.ExecuteAsync("delete from StoredCustomer");
            await db.ExecuteAsync("delete from LastSyncDetails");
        }

        public async Task DeleteCustomers(IList<int> ids)
        {
            foreach (var id in ids)
                await db.ExecuteAsync("delete from StoredCustomer where Id = ?", id);
        }
    }
}