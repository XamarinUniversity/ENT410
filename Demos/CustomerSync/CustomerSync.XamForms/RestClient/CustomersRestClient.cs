using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CustomerSync.Models;
using Newtonsoft.Json;
using System;
using MobileSync.Models;
using System.Text;

namespace CustomerSync
{
    public class CustomersRestClient
    {
        const string RestServiceBaseAddress = "https://xamuniversityent410.azurewebsites.net/api/Customers";
        // const string RestServiceBaseAddress = "http://localhost:4976/api/Customers";
        const string AcceptHeaderApplicationJson = "application/json";

        private HttpClient CreateRestClient()
        {
            var client = new HttpClient { 
				BaseAddress = new Uri(RestServiceBaseAddress),
				Timeout = new TimeSpan(0, 0, 0, 10, 0)
			};

            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(AcceptHeaderApplicationJson));
            client.DefaultRequestHeaders.Add("userToken", DeviceToken.Token);
            return client;
        }

        public async Task<List<Customer>> GetCustomers()
        {
            var result = new List<Customer>();

            var jsonResponse = string.Empty;

            using (var client = CreateRestClient())
            {
                var dataResp = await client.GetAsync("", HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                //If we do not get a successful status code, then return an empty set
                if (!dataResp.IsSuccessStatusCode)
					throw new SyncException ("Could not connect to the Server for synchronization");

                jsonResponse = await dataResp.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(jsonResponse))
                return null;

            var parsedResponse = await Task.Factory.StartNew(() => 
                JsonConvert.DeserializeObject<Customer[]>(jsonResponse)).ConfigureAwait(false);

            result.AddRange(parsedResponse);
            return result;
        }

		/// <summary>
		/// Send changes to the server so they can be processed
		/// </summary>
		/// <returns>Whether it synced and any conflicts if there were conflicts</returns>
		/// <param name="items">Items.</param>
        /// <param name = "forceChanges"></param>
		public async Task<SyncResult<Customer>> SyncData(List<Customer> items, bool forceChanges = false)
		{
			var jsonResponse = string.Empty;

			using (var client = CreateRestClient())
			{
                string postBody = await Task.Run(() => JsonConvert.SerializeObject(items.ToArray())).ConfigureAwait(false);

                HttpResponseMessage dataResp;

				if (!forceChanges) 
                {
					dataResp = await client.PostAsync ("", new StringContent (postBody, Encoding.UTF8, "application/json")); 
				} 
                else 
                {
					dataResp = await client.PutAsync ("", new StringContent (postBody, Encoding.UTF8, "application/json")); 
				}

				if (!dataResp.IsSuccessStatusCode)
					throw new CouldNotConnectException ();

				// Retrieve the JSON response
				jsonResponse = await dataResp.Content.ReadAsStringAsync();
			}

			return string.IsNullOrEmpty(jsonResponse) 
                ? null 
                : await Task.Run(() => JsonConvert.DeserializeObject<SyncResult<Customer>>(jsonResponse));

		}
    }
}
