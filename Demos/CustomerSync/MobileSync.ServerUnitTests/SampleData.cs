using System;
using CustomerSync.Models;

namespace MobileSync.ServerUnitTests
{
    public class SampleData
    {
        public const string userToken1 = "User1";
        public const string userToken2 = "User2";

        public static Customer JohnSmith
        {
            get
            {
                return new Customer()
                {
                    Company = "Some Organisation",
                    CreateDateTime = DateTime.Now,
                    Email = "John.Smith@someorganisation.com",
                    IsDeleted = false,
                    LastUpdateDateTime = DateTime.Now,
                    Id = 0,
                    Name = "John Smith",
                    Notes = "",
                    Phone = "555 1234",
                    Title = "Some Generic Title",
                    VersionNumber = 1,
                    CorrelationId = Guid.NewGuid().ToString()
                };
            }
        }

        public static Customer JaneSmyth
        {
            get
            {
                return new Customer()
                {
                    Company = "Another Organisation",
                    CreateDateTime = DateTime.Now,
                    Email = "jsmyth@another.org",
                    IsDeleted = false,
                    LastUpdateDateTime = DateTime.Now,
                    Id = 0,
                    Name = "Jane Doe",
                    Notes = "",
                    Phone = "555 9876",
                    Title = "Another Role",
                    VersionNumber = 1,
                    CorrelationId = Guid.NewGuid().ToString()
                };
            }
        }
    }
}
