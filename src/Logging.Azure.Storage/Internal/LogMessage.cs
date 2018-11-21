using System;

namespace Logging.Azure.Storage.Internal
{
	public struct LogMessage
	{
		public DateTimeOffset Timestamp { get; set; }
		public string Message { get; set; }
	}
}
