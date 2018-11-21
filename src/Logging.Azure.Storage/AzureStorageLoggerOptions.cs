using System;
using Logging.Azure.Storage.Internal;

namespace Logging.Azure.Storage
{
	/// <summary>
	/// Options for file logging.
	/// </summary>
	public class AzureStorageLoggerOptions : BatchingLoggerOptions
	{
		private int blobSizeLimit = 10 * 1024 * 1024;
		private int retainedBlobCountLimit = 2;
		private string blobNamePrefix = "logs-";

		/// <summary>
		/// Gets or sets Azure Storage Account Name
		/// </summary>
		public string StorageAccountName { get; set; }

		/// <summary>
		/// Gets or sets Azure Storage Key
		/// </summary>
		public string StorageKey { get; set; }

		/// <summary>
		/// Gets or sets a strictly positive value representing the maximum log size in bytes or null for no limit.
		/// Once the log is full, no more messages will be appended.
		/// Defaults to <c>10MB</c>.
		/// </summary>
		public int BlobSizeLimit
		{
			get { return blobSizeLimit; }
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(BlobSizeLimit)} must be positive.");
				}
				blobSizeLimit = value;
			}
		}

		/// <summary>
		/// Gets or sets a strictly positive value representing the maximum retained file count or null for no limit.
		/// Defaults to <c>2</c>.
		/// </summary>
		public int RetainedBlobCountLimit
		{
			get { return retainedBlobCountLimit; }
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(RetainedBlobCountLimit)} must be positive.");
				}
				retainedBlobCountLimit = value;
			}
		}

		/// <summary>
		/// Gets or sets the filename prefix to use for log files.
		/// Defaults to <c>logs-</c>.
		/// </summary>
		public string BlobNamePrefix
		{
			get { return blobNamePrefix; }
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentException(nameof(value));
				}
				blobNamePrefix = value;
			}
		}

		/// <summary>
		/// The path in which log files will be written, relative to the app process.
		/// Default to <c>logs</c>
		/// </summary>
		/// <returns></returns>
		public string ContainerName { get; set; } = "logs";
	}
}
