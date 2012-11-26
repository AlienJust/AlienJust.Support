using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent
{
	public sealed class SingleThreadedRelayQueueWorker<TItem> : IQueueWorker<TItem>
	{
		private readonly ConcurrentQueue<TItem> _items;
		private readonly Action<TItem> _action;
		private readonly AutoResetEvent _threadNotify;
		private readonly Thread _workThread;
		private readonly ILogger _logger;

		public SingleThreadedRelayQueueWorker(Action<TItem> action, ILogger logger)
		{
			_items = new ConcurrentQueue<TItem>();
			_action = action;
			_logger = logger;

			_threadNotify = new AutoResetEvent(false);
			_workThread = new Thread(WorkingThreadStart) { IsBackground = true };
			_workThread.Start();
		}


		public void AddToExecutionQueue(TItem item)
		{
			try
			{
				_items.Enqueue(item);
				_logger.Log("item added");
				_threadNotify.Set();
			}
			catch (Exception ex)
			{
				_logger.Log(ex.ToString());
			}
		}


		private void WorkingThreadStart()
		{
			try
			{
				while (true)
				{
					_threadNotify.WaitOne();
					TItem dequeuedItem;
					while (_items.TryDequeue(out dequeuedItem))
					{
						try
						{
							_action(dequeuedItem);
							_logger.Log("Item dequeued and action executed");
						}
						catch (Exception ex)
						{
							_logger.Log(ex.ToString());
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Log(ex.ToString());
			}
			finally
			{
				_logger.Log("Exiting background thread (abnormal)");
			}
		}
	}

	public interface IQueueWorker<in TItem>
	{
		void AddToExecutionQueue(TItem item);
	}
}
