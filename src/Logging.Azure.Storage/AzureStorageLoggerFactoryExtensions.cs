using Logging.Azure.Storage;
using Logging.Azure.Storage.Internal.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Extensions.Logging
{
	/// <summary>
	/// Extensions for adding the <see cref="AzureStorageLoggerProvider" /> to the <see cref="ILoggingBuilder" />
	/// </summary>
	public static class AzureStorageLoggerFactoryExtensions
	{
		/// <summary>
		/// Adds a blob logger named 'AzureBlobStorage' to the factory.
		/// </summary>
		/// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
		/// <param name="configure">Configure an instance of the <see cref="AzureStorageLoggerOptions" /> to set logging options</param>
		public static ILoggingBuilder AddBlobStorage(this ILoggingBuilder builder, Action<AzureStorageLoggerOptions> configure)
		{
			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			builder.Services.AddSingleton<IAzureStorageHandler, AzureStorageHandler>();
			builder.Services.AddSingleton<IBlobHandler, BlobHandler>();
			builder.Services.AddSingleton<ILoggerProvider, AzureStorageLoggerProvider>();

			builder.Services.Configure(configure);

			return builder;
		}
	}
}
