using Logging.Azure.Storage.Internal;
using Logging.Azure.Storage.Internal.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Logging.Azure.Storage
{
	/// <summary>
	/// An <see cref="ILoggerProvider" /> that writes logs to a file
	/// </summary>
	[ProviderAlias("AzureBlobStorage")]
	public class AzureStorageLoggerProvider : BatchingLoggerProvider
	{
		private string ContainerName { get; }
		private string BlobNamePrefix { get; }
		private int MaxBlobSize { get; }
		private int RetainedBlobCountLimit { get; }
		private IBlobHandler BlobHandler { get; }

		/// <summary>
		/// Creates an instance of the <see cref="AzureStorageLoggerProvider" /> 
		/// </summary>
		/// <param name="options">The options object controlling the logger</param>
		public AzureStorageLoggerProvider(IOptions<AzureStorageLoggerOptions> options, IBlobHandler blobHandler) : base(options)
		{
			var loggerOptions = options.Value;
			ContainerName = loggerOptions.ContainerName;
			BlobNamePrefix = loggerOptions.BlobNamePrefix;
			MaxBlobSize = loggerOptions.BlobSizeLimit;
			RetainedBlobCountLimit = loggerOptions.RetainedBlobCountLimit;
			BlobHandler = blobHandler;
		}

		/// <inheritdoc />
		protected override async Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken cancellationToken)
		{
			foreach (var group in messages.GroupBy(GetGrouping))
			{
				var fullName = GetFullName(group.Key);
				var blob = await BlobHandler.GetBlobOrDefaultAsync(ContainerName, fullName);

				if (MaxBlobSize > 0 && blob != null && blob.Length > MaxBlobSize)
				{
					return;
				}

				using (var streamWriter = new MemoryStream())
				{
					await streamWriter.WriteAsync(blob);

					foreach (var item in group)
					{
						await streamWriter.WriteAsync(Encoding.ASCII.GetBytes(item.Message));
					}

					var data = streamWriter.ToArray();
					await BlobHandler.PutBlobAsync(ContainerName, fullName, data);
				}

			}

			await RollFilesAsync();
		}

		private string GetFullName((int Year, int Month, int Day) group)
		{
			return $"{group.Year:0000}/{group.Month:00}/{BlobNamePrefix}{group.Year:0000}{group.Month:00}{group.Day:00}.log";
		}

		private (int Year, int Month, int Day) GetGrouping(LogMessage message)
		{
			return (message.Timestamp.Year, message.Timestamp.Month, message.Timestamp.Day);
		}

		/// <summary>
		/// Deletes old log files, keeping a number of files defined by <see cref="AzureStorageLoggerOptions.RetainedBlobCountLimit" />
		/// </summary>
		protected async Task RollFilesAsync()
		{
			if (RetainedBlobCountLimit > 0)
			{
				var blobInfos = await BlobHandler.ListBlobsAsync(ContainerName);

				var blobs = blobInfos
					.OrderByDescending(f => f.Name)
					.Skip(RetainedBlobCountLimit);

				foreach (var blob in blobs)
				{
					await BlobHandler.DeleteBlobAsync(ContainerName, blob.Name);
				}
			}
		}
	}
}
