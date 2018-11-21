using System.Collections.Generic;
using System.Threading.Tasks;

namespace Logging.Azure.Storage.Internal.Storage
{
	public interface IBlobHandler
	{
		Task<bool> PutBlobAsync(string container, string contentName, byte[] content);
		Task<byte[]> GetBlobOrDefaultAsync(string container, string contentName);
		Task<bool> DeleteBlobAsync(string container, string contentName);
		Task<IEnumerable<BlobData>> ListBlobsAsync(string container);
	}
}
