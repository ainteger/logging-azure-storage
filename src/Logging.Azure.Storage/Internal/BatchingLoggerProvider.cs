using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logging.Azure.Storage.Internal
{
	public abstract class BatchingLoggerProvider : ILoggerProvider
	{
		private List<LogMessage> CurrentBatch { get; } = new List<LogMessage>();
		private TimeSpan Interval { get; }
		private int? QueueSize { get; }
		private int? BatchSize { get; }

		private BlockingCollection<LogMessage> MessageQueue { get; set; }
		private Task OutputTask { get; set; }
		private CancellationTokenSource CancellationTokenSource { get; set; }

		protected BatchingLoggerProvider(IOptions<BatchingLoggerOptions> options)
		{
			// NOTE: Only IsEnabled is monitored

			var loggerOptions = options.Value;
			if (loggerOptions.BatchSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(loggerOptions.BatchSize), $"{nameof(loggerOptions.BatchSize)} must be a positive number.");
			}
			if (loggerOptions.FlushPeriod <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(loggerOptions.FlushPeriod), $"{nameof(loggerOptions.FlushPeriod)} must be longer than zero.");
			}

			Interval = loggerOptions.FlushPeriod;
			BatchSize = loggerOptions.BatchSize;
			QueueSize = loggerOptions.BackgroundQueueSize;

			Start();
		}

		protected abstract Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken token);

		private async Task ProcessLogQueue(object state)
		{
			while (!CancellationTokenSource.IsCancellationRequested)
			{
				var limit = BatchSize ?? int.MaxValue;

				while (limit > 0 && MessageQueue.TryTake(out var message))
				{
					CurrentBatch.Add(message);
					limit--;
				}

				if (CurrentBatch.Count > 0)
				{
					try
					{
						await WriteMessagesAsync(CurrentBatch, CancellationTokenSource.Token);
					}
					catch
					{
						// ignored
					}

					CurrentBatch.Clear();
				}

				await IntervalAsync(Interval, CancellationTokenSource.Token);
			}
		}

		protected virtual Task IntervalAsync(TimeSpan interval, CancellationToken cancellationToken)
		{
			return Task.Delay(interval, cancellationToken);
		}

		internal void AddMessage(DateTimeOffset timestamp, string message)
		{
			if (!MessageQueue.IsAddingCompleted)
			{
				try
				{
					MessageQueue.Add(new LogMessage { Message = message, Timestamp = timestamp }, CancellationTokenSource.Token);
				}
				catch
				{
					//cancellation token canceled or CompleteAdding called
				}
			}
		}

		private void Start()
		{
			MessageQueue = QueueSize == null ?
				new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>()) :
				new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>(), QueueSize.Value);

			CancellationTokenSource = new CancellationTokenSource();
			OutputTask = Task.Factory.StartNew<Task>(
				ProcessLogQueue,
				null,
				TaskCreationOptions.LongRunning);
		}

		private void Stop()
		{
			CancellationTokenSource.Cancel();
			MessageQueue.CompleteAdding();

			try
			{
				OutputTask.Wait(Interval);
			}
			catch (TaskCanceledException)
			{
			}
			catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
			{
			}
		}

		public void Dispose()
		{
			Stop();
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new BatchingLogger(this, categoryName);
		}
	}
}
