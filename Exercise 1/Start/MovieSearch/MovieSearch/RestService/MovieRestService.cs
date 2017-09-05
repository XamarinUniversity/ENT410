using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Net;

namespace MovieSearch
{
	/// <summary>
	/// Uses the iTunes Search REST API. 
	/// Further details at:
	///   https://affiliate.itunes.apple.com/resources/documentation/itunes-store-web-service-search-api/
	/// </summary>
	public class MovieRestService
	{
		public int NumberOfMoviesPerRequest = 25;
		public string HostName = "itunes.apple.com";

		string GetPath (string search, int pageNo)
		{
			var searchTerm = WebUtility.UrlEncode (search.Trim ());
			var offset = (pageNo - 1) * NumberOfMoviesPerRequest;
			return string.Format ("https://itunes.apple.com/search?term={0}&entity=movie&limit={1}&offset={2}",
				searchTerm, NumberOfMoviesPerRequest, offset);
		}

		public async Task<IList<Movie>> GetMoviesForSearchAsync (string search, int pageNo = 1)
		{
			// Load the data from the remote service
			using (var client = new HttpClient ()) {
				client.BaseAddress = new Uri (GetPath (search, pageNo));
				client.DefaultRequestHeaders.Accept.Clear ();
				client.DefaultRequestHeaders.Accept.Add (
					new MediaTypeWithQualityHeaderValue ("application/json"));

				var response = await client.GetAsync ("");

				response.EnsureSuccessStatusCode ();

				var allMovies = new List<Movie> ();

				var content = await response.Content.ReadAsStringAsync ();
				await Task.Run (() => {
					var results = JsonConvert.DeserializeObject<MovieSearchResponse> (content);

					if (results.ResultCount > 0) {
						allMovies.AddRange (results.Results.Select (item =>
                            new Movie {
							ID = item.TrackId,
							Title = item.TrackName,
							Genre = item.PrimaryGenreName,
							ContentAdvisoryRating = item.ContentAdvisoryRating,
							Description = item.LongDescription,
								ArtworkUri = item.ArtworkUrl100
						}));
					}
				});

				return allMovies;
			}
		}
	}
}
