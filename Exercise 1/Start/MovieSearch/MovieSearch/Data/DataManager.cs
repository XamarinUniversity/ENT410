using System.Threading.Tasks;
using System.Collections.Generic;
using SQLite;
using System;

namespace MovieSearch
{
    public static class DataManager
    {
        static SQLiteAsyncConnection DB;

        public static async Task SetupDatabaseAsync(string filename)
        {
            DB = new SQLiteAsyncConnection(filename);
            await DB.CreateTableAsync<Movie>();
        }

		public static void CheckForExistingDatabase ()
		{
			if (DB == null)
				throw new Exception("Must call SetupDatabaseAsync first.");
		}

        public static Task<List<Movie>> GetMoviesAsync(string searchText)
        {
			CheckForExistingDatabase ();

            return DB.Table<Movie>()
				.Where(m => m.Title.Contains(searchText))
                .ToListAsync();
        }

        public async static Task StoreMoviesAsync (IList<Movie> movies)
        {
			// To implement
        }
    }
}
