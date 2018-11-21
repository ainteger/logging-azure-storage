using System.Net.Http;

namespace Logging.Azure.Storage.Internal.Storage
{
	public interface IAzureStorageHandler
	{
		HttpRequestMessage GetRequest(HttpMethod method, string resource, byte[] requestBody = null);
	}
}
