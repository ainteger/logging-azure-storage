# Logging in Azure Blob Storage

Use Azure Blob Storage to log application logs

## nuget

Project will be uploaded soon

## Sample code

Program.cs

	public class Program {
		public static void Main(string[] args) {
			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.ConfigureLogging((context, builder) => {
					builder.AddBlobStorage(opts => {
						context.Configuration.GetSection("AzureBlobStorageLoggingOptions").Bind(opts);
					});
				})
				.UseStartup<Startup>()
				.Build();
	}

appsettings.json

	"AzureBlobStorageLoggingOptions": {
		"BlobNamePrefix": "app-logs-",
		"BlobSizeLimit": 1048576,
		"RetainedBlobCountLimit": 10,
		"ContainerName": "logs",
		"StorageAccountName": "ACCOUNT-NAME",
		"StorageKey": "STORAGE-KEY"
	}
