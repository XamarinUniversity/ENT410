using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CustomerSync.Models;
using log4net.Repository.Hierarchy;
using MobileSync.Server;
using MobileSync.Models;
using CustomerSync.Server;

namespace ServerSyncApi.Controllers
{
    /// <summary>
    ///  An example instance of how the sync could potentially occur. It uses the BaseSyncApiController
    ///  so that subsequent integrations use the same mechanism. You may want to partition the 
    ///  Apis so that different items have different scopes
    /// </summary>
    public class CustomersController : BaseSyncApiController<Customer, DatabaseCustomerDataSync>
    {

    }
}
