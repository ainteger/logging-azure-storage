# Logging in Azure Blob Storage

Use Azure Blob Storage to log application logs

## Notes

Options at registration is mandatory, StorageAccountName, StorageKey must be set. Container needs to be created in Blob Storage Account to be able to use the package. 

## nuget

https://www.nuget.org/packages/Ainteger.Logging.Azure.Storage/

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
