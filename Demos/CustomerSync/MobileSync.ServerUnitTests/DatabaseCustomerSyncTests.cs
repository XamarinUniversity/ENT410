using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CustomerSync.Models;
using System.Linq;
using System.Threading.Tasks;
using CustomerSync.Server;

namespace MobileSync.ServerUnitTests
{
    [TestClass]
    public class DatabaseCustomerSyncTests 
    {
        public DatabaseCustomerDataSync GetDataSync()
        {
            return new DatabaseCustomerDataSync();
        }

        [TestMethod]
        public async Task ServerSideSync_DatabaseCustomerSync_CanInsertRows()
        {
            var conn = GetDataSync();

            var allCustomers = await conn.GetItemsAsync(SampleData.userToken1, false);
            var originalCustomerCount = allCustomers.Count();

            var customersToAdd = new Customer[] {
                SampleData.JohnSmith,
                SampleData.JaneSmyth
            };
            var insertResult = await conn.ProcessAsync(customersToAdd, SampleData.userToken1);
            Assert.IsTrue(insertResult.Status == Models.SyncStatus.Success);

            var allCustomersAgain = await conn.GetItemsAsync(SampleData.userToken1, false);

            Assert.AreEqual(originalCustomerCount + customersToAdd.Count(), allCustomersAgain.Count());

            // Check that the user has the record as one that they control
            var itemId = insertResult.CorrelationIds[customersToAdd[0].CorrelationId];
            var history = await conn.GetHistory(itemId, SampleData.userToken1);

            Assert.IsNotNull(history);
            Assert.AreEqual(history.RecordId, itemId);
            Assert.AreEqual(history.UpdateHistoryType, Models.ItemUpdatedHistoryType.NoChange);
            Assert.AreEqual(history.UserIdentifier, SampleData.userToken1);
            Assert.AreEqual(history.VersionNumber, 1);
        }

        [TestMethod]
        public async Task ServerSideSync_DatabaseCustomerSync_CanUpdateRows()
        {
            var conn = GetDataSync();

            var john = SampleData.JohnSmith;
            var customersToProcess = new Customer[] { john };

            // Insert the entry
            var result = await conn.ProcessAsync(customersToProcess, SampleData.userToken1);
            Assert.IsTrue(result.Status == Models.SyncStatus.Success);

            john.Id = result.CorrelationIds[john.CorrelationId];

            // Perform an update
            john.Email = "johnssmith@sirjohnofsmith.com";
            john.Notes = "Creator of Xamarin Apps";

            var updateResult = await conn.ProcessAsync(customersToProcess, SampleData.userToken1);
            Assert.IsTrue(result.Status == Models.SyncStatus.Success);
            john.VersionNumber = result.VersionChanges[john.Id];

            // Get the record from the server
            var item = await conn.GetItemAsync(john.Id, SampleData.userToken1, true);
            Assert.AreEqual(john.Email, item.Email);
            Assert.AreEqual(john.Notes, item.Notes);

            // Check that the version record is updated
            Assert.AreEqual(john.VersionNumber, item.VersionNumber);
        }

        [TestMethod]
        public async Task ServerSideSync_DatabaseCustomerSync_CanDeleteRows()
        {
            var conn = GetDataSync();

            var john = SampleData.JohnSmith;
            var customersToProcess = new Customer[] { john };

            // Insert the entry
            var result = await conn.ProcessAsync(customersToProcess, SampleData.userToken1);
            Assert.IsTrue(result.Status == Models.SyncStatus.Success);

            john.Id = result.CorrelationIds[john.CorrelationId];

            // Delete the entry from the system
            john.DeletedDateTime = DateTime.Now;
            john.IsDeleted = true;

            var deleteResult = await conn.ProcessAsync(customersToProcess, SampleData.userToken1);
            Assert.IsTrue(result.Status == Models.SyncStatus.Success);

            var searchForDeletedItem = await conn.GetItemAsync(john.Id, SampleData.userToken1, false);
            Assert.IsNull(searchForDeletedItem);
        }

        [TestMethod]
        public async Task ServerSideSync_DatabaseCustomerSync_AppropriateUpdateConflict_ShouldResultInConflict()
        {
            var conn = GetDataSync();

            var john = SampleData.JohnSmith;
            var customersToProcess = new Customer[] { john };

            // Insert the entry
            var result = await conn.ProcessAsync(customersToProcess, SampleData.userToken1);
            Assert.IsTrue(result.Status == Models.SyncStatus.Success);

            john.Id = result.CorrelationIds[john.CorrelationId];

            // Let's setup the two different clients
            var client1 = GetDataSync();
            var client2 = GetDataSync();

            // Both get the records
            var client1_item = await conn.GetItemAsync(john.Id, SampleData.userToken1, true);
            var client2_item = await conn.GetItemAsync(john.Id, SampleData.userToken2, true);

            // Have one client update the record
            client1_item.Email = "johnsmith@smithand.co";
            var client1_UpdateResult = await client1.ProcessAsync(new Customer[] { client1_item }, SampleData.userToken1);
            Assert.IsTrue(client1_UpdateResult.Status == Models.SyncStatus.Success);

            // Have the other client try and update the record
            client2_item.Notes = "This is an additional update";
            var client2_UpdateResult = await client2.ProcessAsync(new Customer[] { client2_item }, SampleData.userToken2);
            Assert.IsTrue(client2_UpdateResult.Status == Models.SyncStatus.PartialSuccessWithConflict);

            // We should have an update conflict on this record
            var conflict = client2_UpdateResult.Conflicts[0];
            Assert.IsTrue(conflict.IsAnUpdateConflict);

            // Check the particulars of the conflict
            Assert.AreNotEqual(conflict.RequestedUpdateItem.VersionNumber, conflict.CurrentItem.VersionNumber);
        }

        [TestMethod]
        public async Task ServerSideSync_DatabaseCustomerSync_AppropriateDeleteConflict_ShouldResultInConflict()
        {
            var conn = GetDataSync();

            var john = SampleData.JohnSmith;
            var customersToProcess = new Customer[] { john };

            // Insert the entry
            var result = await conn.ProcessAsync(customersToProcess, SampleData.userToken1);
            Assert.IsTrue(result.Status == Models.SyncStatus.Success);

            john.Id = result.CorrelationIds[john.CorrelationId];

            // Let's setup the two different clients
            var client1 = GetDataSync();
            var client2 = GetDataSync();

            // Both get the records
            var client1_item = await conn.GetItemAsync(john.Id, SampleData.userToken1, true);
            var client2_item = await conn.GetItemAsync(john.Id, SampleData.userToken2, true);

            // Have one client delete the record
            client1_item.DeletedDateTime = DateTime.Now;
            client1_item.IsDeleted = true;
            var client1_UpdateResult = await client1.ProcessAsync(new Customer[] { client1_item }, SampleData.userToken1);
            Assert.IsTrue(client1_UpdateResult.Status == Models.SyncStatus.Success);
            Assert.IsNull(await client1.GetItemAsync(client1_item.Id, SampleData.userToken1, true));

            // Have the other client try and update the record
            client2_item.Notes = "This is an additional update";
            var client2_UpdateResult = await client2.ProcessAsync(new Customer[] { client2_item }, SampleData.userToken2);
            Assert.IsTrue(client2_UpdateResult.Status == Models.SyncStatus.PartialSuccessWithConflict);

            // We should have an update conflict on this record
            var conflict = client2_UpdateResult.Conflicts[0];
            Assert.IsTrue(conflict.IsADeleteConflict);
        }
    }    
}
