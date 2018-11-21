using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Logging.Azure.Storage.Internal.Storage
{
	public class AzureStorageHandler : IAzureStorageHandler
	{
		private string StorageAccount { get; }
		private string StorageKey { get; }

		public AzureStorageHandler(IOptions<AzureStorageLoggerOptions> options)
		{
			StorageAccount = options.Value.StorageAccountName;
			StorageKey = options.Value.StorageKey;

			if (string.IsNullOrWhiteSpace(StorageAccount) || string.IsNullOrWhiteSpace(StorageKey))
			{
				throw new ArgumentException($"{nameof(StorageAccount)} and {nameof(StorageKey)} must have a value");
			}
		}

		public HttpRequestMessage GetRequest(HttpMethod method, string resource, byte[] requestBody = null)
		{
			var now = DateTime.UtcNow;

			var request = new HttpRequestMessage(method, $"https://{StorageAccount}.blob.core.windows.net/{resource}");

			request.Headers.Add("x-ms-date", now.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
			request.Headers.Add("x-ms-version", "2018-03-28");
			request.Headers.Add("x-ms-blob-type", "BlockBlob");

			if (requestBody != null)
			{
				request.Content = new ByteArrayContent(requestBody);
				request.Content.Headers.Add("Content-Length", requestBody.Length.ToString());
			}

			request.Headers.Add("Authorization", GetAuthorizationHeader(method, now, request));

			return request;
		}

		private string GetAuthorizationHeader(HttpMethod method, DateTime now, HttpRequestMessage request)
		{
			var messageSignature = string.Format("{0}\n\n\n{1}\n\n\n\n\n\n\n\n\n{2}{3}",
					method,
					(method == HttpMethod.Get || method == HttpMethod.Head) ? string.Empty : request.Content?.Headers?.FirstOrDefault(x => x.Key == "Content-Length").Value.FirstOrDefault() ?? string.Empty,
					GetCanonicalizedHeaders(request),
					GetCanonicalizedResource(request.RequestUri, StorageAccount)
					);

			return $"SharedKey {StorageAccount}:{GetSignature(messageSignature)}";
		}

		private string GetSignature(string messageSignature)
		{
			var signatureBytes = Encoding.UTF8.GetBytes(messageSignature);
			var SHA256 = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(StorageKey));
			return Convert.ToBase64String(SHA256.ComputeHash(signatureBytes));
		}

		private string GetCanonicalizedHeaders(HttpRequestMessage request)
		{
			var sortedHeaders = request.Headers.Where(x => x.Key.ToLowerInvariant().StartsWith("x-ms-", StringComparison.Ordinal))
				.OrderBy(x => x.Key);

			return string.Join(string.Empty,
				sortedHeaders.Select(x =>
					$"{x.Key}:{string.Join(",", x.Value.Select(v => v.Replace("\r\n", string.Empty)))}\n"));
		}

		private string GetCanonicalizedResource(Uri address, string accountName)
		{
			var queryString = new Dictionary<string, string>();

			var queryStringValues = QueryHelpers.ParseQuery(address.Query);

			foreach (string key in queryStringValues.Keys)
			{
				queryString.Add(key?.ToLowerInvariant(), string.Join(",", queryStringValues[key].OrderBy(x => x)));
			}

			return $"/{accountName}{address.AbsolutePath}{string.Join(string.Empty, queryString.OrderBy(x => x.Key).Select(x => $"\n{x.Key}:{x.Value}"))}";
		}
	}
}
