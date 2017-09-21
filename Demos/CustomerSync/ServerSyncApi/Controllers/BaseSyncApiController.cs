using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using MobileSync.Server;
using MobileSync.Models;
using ServiceStack.Text;

namespace ServerSyncApi.Controllers
{
    /// <summary>
    /// A generic implementation of an ApiController that is designed to synchonize with a back-end
    /// data store
    /// </summary>
    /// <typeparam name="T">The type of item that is being synchronized</typeparam>
    /// <typeparam name="DS">An implementation of the DataSync that is being used</typeparam>
    public abstract class BaseSyncApiController<T, DS> : ApiController 
        where DS : BaseServerSync<T>, new() 
        where T : SyncObject
    {
        protected DS _sync = new DS();
        protected readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<IEnumerable<T>> Get()
        {
            this.Dump();
            logger.Debug("Get Items");
            return await _sync.GetItemsAsync(GetUserToken(), true);
        }

        public async Task<T> Get(int id)
        {
            logger.Debug(String.Format("Get Item {0}", id));
            return await _sync.GetItemAsync(id, GetUserToken(), true);
        }

        protected string GetUserToken()
        {
            if (!this.Request.Headers.TryGetValues("userToken", out IEnumerable<string> keys))
                throw new ArgumentException("No User Token Specified");

            return keys.First();
        }

        public async Task<SyncResult<T>> Post([FromBody] T[] items)
        {
            logger.Debug(String.Format("Post {0}", items.Dump()));
            // http://localhost/ServerSyncApi/Properties/
            return await _sync.ProcessAsync(items, GetUserToken());
        }
    }

    public class BaseConflictAllowedSyncApiController<T, DS> : BaseSyncApiController<T, DS>
        where DS : BaseServerSync<T>, new()
        where T : SyncObject
    {
        // Update the elements from the list and ensure that they forced to be updated
        public async Task<SyncResult<T>> Put([FromBody] T[] items)
        {
            logger.Debug(String.Format("Put {0}", items.Dump()));
            return await _sync.ProcessAsync(items, GetUserToken(), true);
        }
    }
}
