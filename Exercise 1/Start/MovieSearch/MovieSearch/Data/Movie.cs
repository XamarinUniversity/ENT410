using SQLite;
using System;

namespace MovieSearch
{
	public class Movie
	{
		[PrimaryKey]
		public int ID { get; set; }

		public string Title { get; set; }
		public string ArtworkUri { get; set; }
		public string Genre { get; set; }
		public string ContentAdvisoryRating { get; set; }
		public string Description { get; set; }
	}
}
