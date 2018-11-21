using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace Logging.Azure.Storage.Internal
{
	public class BatchingLogger : ILogger
	{
		private BatchingLoggerProvider Provider { get; }
		private string Category { get; }

		public BatchingLogger(BatchingLoggerProvider loggerProvider, string categoryName)
		{
			Provider = loggerProvider;
			Category = categoryName;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			if (logLevel == LogLevel.None)
			{
				return false;
			}
			return true;
		}

		public void Log<TState>(DateTimeOffset timestamp, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			var builder = new StringBuilder();
			builder.Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
			builder.Append(" [");
			builder.Append(logLevel.ToString());
			builder.Append("] ");
			builder.Append(Category);
			builder.Append(": ");
			builder.AppendLine(formatter(state, exception));

			if (exception != null)
			{
				builder.AppendLine(exception.ToString());
			}

			Provider.AddMessage(timestamp, builder.ToString());
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			Log(DateTimeOffset.Now, logLevel, eventId, state, exception, formatter);
		}
	}
}
