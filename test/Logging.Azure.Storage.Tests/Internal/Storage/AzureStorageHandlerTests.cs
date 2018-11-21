using Logging.Azure.Storage.Internal.Storage;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xunit;
using System.Linq;

namespace Logging.Azure.Storage.Tests.Internal.Storage
{
	public class AzureStorageHandlerTests
	{
		public AzureStorageHandler GetHandler()
		{
			var options = Options.Create(new AzureStorageLoggerOptions
			{
				StorageAccountName = "testaccount",
				StorageKey = "SSjwpaQxpuE39Ge1rtQSFG2d+SuyCdLfBavFFvWkgGuiJ3snVmHN7YNYsd1rQ6FVlco2GzCbfkwOplcrHJTgNA=="
			});
			return new AzureStorageHandler(options);
		}

		[Fact]
		public void GivenStorageKeyMissing_WhenInit_ExceptionShouldBeTriggered()
		{
			//Given
			var options = Options.Create(new AzureStorageLoggerOptions
			{
				StorageKey = "storagekey"
			});

			//When
			Action result = () => new AzureStorageHandler(options);

			//Then
			Assert.Throws<ArgumentException>(result);
		}

		[Fact]
		public void GivenStorageAccountMissing_WhenInit_ExceptionShouldBeTriggered()
		{
			//Given
			var options = Options.Create(new AzureStorageLoggerOptions
			{
				StorageAccountName = "storageaccountname"
			});

			//When
			Action result = () => new AzureStorageHandler(options);

			//Then
			Assert.Throws<ArgumentException>(result);
		}

		[Fact]
		public void GivenStorageHandlerInitialized_WhenGetRequest_ThenAllHeadersAndUrlsShouldBeCorrect()
		{
			//Given
			var handler = GetHandler();

			//When
			var result = handler.GetRequest(HttpMethod.Get, "container/contentName");

			//Then
			Assert.Contains("x-ms-version", result.Headers.Select(x => x.Key));
			Assert.Contains("x-ms-blob-type", result.Headers.Select(x => x.Key));
			Assert.Contains("x-ms-date", result.Headers.Select(x => x.Key));

			Assert.Equal("2018-03-28", result.Headers.First(x => x.Key == "x-ms-version").Value.First());
			Assert.Equal("BlockBlob", result.Headers.First(x => x.Key == "x-ms-blob-type").Value.First());
			Assert.StartsWith("testaccount:", result.Headers.Authorization.Parameter);

			Assert.Equal("https://testaccount.blob.core.windows.net/container/contentName", result.RequestUri.ToString());
			Assert.Equal(HttpMethod.Get, result.Method);
		}

		[Fact]
		public void GivenStorageHandlerInitialized_WhenGetSignature_ThenItShouldBeCorrect()
		{
			//Given
			var handler = GetHandler();
			var expected = "H/5kpHyEA6a0kf+YI8CMDgiaw12aCPBi04s7JRHdFpM=";

			//When
			var result = GetHandler().GetSignature("justtestingarandomvalueisgood");

			//Then
			Assert.Equal(expected, result);
		}

		[Fact]
		public void GivenStorageHandlerInitialized_WhenGetAuthorizationHeader_ThenItShouldBeCorrect()
		{
			//Given
			var method = HttpMethod.Get;
			var timestamp = new DateTime(2017, 1, 1, 12, 34, 2);
			var fakeRequest = new HttpRequestMessage(method, "https://testaccount.blob.core.windows.net/testblob/testblob");
			var expected = "SharedKey testaccount:VP1o60IMARJv3EcTExvm1NIJmoZUUmQ8QKzUwNxI+ic=";

			//When
			var actual = GetHandler().GetAuthorizationHeader(method, timestamp, fakeRequest);

			//Then
			Assert.Equal(expected, actual);
		}
	}
}
