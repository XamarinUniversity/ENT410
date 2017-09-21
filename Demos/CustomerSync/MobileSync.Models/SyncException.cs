using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileSync.Models
{
	public class SyncException : Exception
	{
		public SyncException(string message) : base(message) 
		{
		}

		public SyncException(string message, Exception inner) : base(message, inner)
		{
		}
	}

	public class CouldNotConnectException : SyncException
	{
		public CouldNotConnectException() : base("Could not connect to perform the sync") 
		{
		}

		public CouldNotConnectException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}
