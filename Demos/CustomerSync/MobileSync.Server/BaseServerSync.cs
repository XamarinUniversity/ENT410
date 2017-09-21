using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileSync.Models;

namespace MobileSync.Server
{
    /// <summary>
    /// Co-ordinates the activities required for synchronising details from a mobile app to a server
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseServerSync<T> where T : SyncObject
    {
        SyncResult<T> result = new SyncResult<T>();

        readonly List<ConflictItem<T>> conflicts = new List<ConflictItem<T>>();

        Dictionary<int, int> VersionChanges = new Dictionary<int, int>();
       
        /// <summary>
        /// Gets the current in-use version from the database. This is used in order to compare and check for conflict errors
        /// when uploading new results from the database
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async virtual Task<T> GetItemAsync(T item, string userToken, bool markAsViewed)
        {
            return await GetItemAsync(item.Id, userToken, markAsViewed);
        }

        /// <summary>
        /// Gets the current in-use version from the database. Often used when looking
        /// for an update or delete conflict
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<T> GetItemAsync(int id, string userToken, bool markAsViewed)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a list of all the items from the collection. 
        /// </summary>
        /// <returns>A IList of the specific type</returns>
        public async virtual Task<IList<T>> GetItemsAsync(string userToken, bool markAsViewed)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts an item into the local database
        /// </summary>
        /// <param name="item"></param>
        public async virtual Task<int> InsertAsync(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing entry in the database
        /// </summary>
        /// <param name="item"></param>
        public async virtual Task UpdateAsync(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes an entry in the database
        /// </summary>
        /// <param name="item"></param>
        public async virtual Task DeleteAsync(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Mark the record as being viewed so that if another user updates the record then they
        /// can download the updated version of that system
        /// </summary>
        /// <param name="item">The item that is being downloaded</param>
        /// <param name="userToken">The user token that identifies the user downloading the item</param>
        public async virtual Task MarkAsViewedAsync(T item, string userToken)
        {

        }

        /// <summary>
        /// Marks the current version of the record as updated, so those devices that
        /// have a copy of the record can be notified on their next sync
        /// </summary>
        /// <param name="item">The item that is being udpated</param>
        /// <param name="userToken">The user token that represents who has updated the record</param>
        public async virtual Task MarkAsUpdatedAsync(T item, string userToken)
        {

        }

        /// <summary>
        /// Marks the current version of the record as deleted, so those devices that
        /// have a copy of the recorded can be notified on their next sync
        /// </summary>
        /// <param name="id">The item that has been deleted</param>
        /// <param name="userToken">The user token that identifies the user deleting the item</param>
        public async virtual Task MarkAsDeletedAsync(T item, string userToken)
        {

        }

        public async virtual Task<List<ItemUpdateHistory>> GetRecordsUpdatedByOthersAsync(string userToken)
        {
            return new List<ItemUpdateHistory>();
        }

        /// <summary>
        /// Makes a log of an action that occurred if required
        /// </summary>
        /// <param name="action"></param>
        /// <param name="item"></param>
        public async virtual Task AuditAsync(AuditAction action, T item, string userToken)
        {
            
        }

        /// <summary>
        /// Initialization code. Useful if you want to apply all operations within a transaction and need to create the 
        /// transaction object
        /// </summary>
        protected async virtual Task SetupAsync()
        {

        }

        /// <summary>
        /// Commit the changes that were made to the database
        /// </summary>
        protected async virtual Task CommitAsync()
        {
            
        }

        /// <summary>
        /// Rollback the list of changes. This occurs when an error was raised when applying the updates
        /// </summary>
        protected async virtual Task RollbackAsync()
        {
            
        }

        protected void AddUpdateConflict(T currentItem, T requestedUpdateItem)
        {
            conflicts.Add(new ConflictItem<T>(currentItem, requestedUpdateItem));
        }

        protected void AddDeleteConflict(T item)
        {
            conflicts.Add(new ConflictItem<T>(item));
        }

        protected Dictionary<string, int> correlationIds;

        /// <summary>
        /// Perform the syncronization against the system. 
        /// </summary>
        /// <param name="items">The items to apply against the synchronisation</param>
        /// <param name="userIdentifier">This is something that uniquely identifies the user, a token of some sort</param>
        /// <param name="forceChanges">Should the changes be applied. The default is false which means that conflict resolution will be used. Otherwise all the changes
        /// that are sent will be applied against the system
        /// </param>
        /// <returns>The details of the sync operation</returns>
        public async virtual Task<SyncResult<T>> ProcessAsync(IEnumerable<T> items, 
            string userToken, 
            bool forceChanges = false)
        {
            result = new SyncResult<T>();

            var validator = GetTokenValidator();
            if (!validator.IsValid(userToken))
            {
                result.Status = SyncStatus.Failed;
                result.FailureReason = "There was an issue with your Authentication credentials";
                return result;
            }

            await SetupAsync();
            try
            {
                // Go through all the items and add them to the collection and sync them            
                var deletedItems = items.Where(item => item.IsDeleted).ToList();
                var updatedItems = items.Where(item => item.Id > 0 && item.IsDeleted == false).ToList();
                var insertedItems = items.Where(item => item.Id == 0).ToList();

				correlationIds = new Dictionary<string, int>();

                // Go through each of these items and process them
                foreach (var item in insertedItems)
                {
                    int id = await InsertAsync(item);
					correlationIds[item.CorrelationId] = id;
                    item.Id = id;
                    await MarkAsViewedAsync(item, userToken);
                    await AuditAsync(AuditAction.Insert, item, userToken);
                }

                foreach (var item in updatedItems)
                {
                    // Check for a conflict
                    var localVersion = await GetItemAsync(item, userToken, false);

                    if (localVersion == null && !forceChanges)
                    {
                        // The record has been removed. Add that to the conflict list
                        AddDeleteConflict(item);
                    }
                    else
                    {
                        if (localVersion.VersionNumber != item.VersionNumber && !forceChanges)
                            AddUpdateConflict(localVersion, item);
                        else
                        {
                            item.VersionNumber = localVersion.VersionNumber + 1;
                            await UpdateAsync(item);
                            await MarkAsUpdatedAsync(item, userToken);
                            VersionChanges[item.Id] = item.VersionNumber;
                            await AuditAsync(AuditAction.Update, item, userToken);                            
                        }
                    }
                }

                foreach (var item in deletedItems)
                {
                    // Check for a conflict
                    await DeleteAsync(item);
                    await MarkAsDeletedAsync(item, userToken);
                    await AuditAsync(AuditAction.Delete, item, userToken);

                    result.DeletedRecords.Add(item.Id);
                }

                await CommitAsync();

				result.CorrelationIds = correlationIds;
                result.Conflicts = conflicts.ToArray();
                result.VersionChanges = this.VersionChanges;

                result.UpdatedHistoryItems = await GetRecordsUpdatedByOthersAsync(userToken);

                if (conflicts.Count == 0)
                    result.Status = SyncStatus.Success;
                else
                    result.Status = SyncStatus.PartialSuccessWithConflict;
            }
            catch (Exception syncException)
            {
                result.Status = SyncStatus.Failed;

                await RollbackAsync();
            }

            return result;            
        }

        protected virtual TokenValidator GetTokenValidator()
        {
            return new AllTokensAreOk();
        }
    }
}
