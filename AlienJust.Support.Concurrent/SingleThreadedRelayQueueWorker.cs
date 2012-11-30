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

		public SingleThreadedRelayQueueWorker(Action<TItem> action)
		{
			_items = new ConcurrentQueue<TItem>();
			_action = action;

			_threadNotify = new AutoResetEvent(false);
			_workThread = new Thread(WorkingThreadStart) { IsBackground = true };
			_workThread.Start();
		}


		public void InsertAsFirstToExecutionQueue(TItem item)
		{
			try
			{
				//_items.(item);
				_threadNotify.Set();
			}
			catch (Exception ex)
			{

			}
		}

		public void AddToExecutionQueue(TItem item)
		{
			try
			{
				_items.Enqueue(item);
				_threadNotify.Set();
			}
			catch (Exception ex)
			{

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
						}
						catch (Exception ex)
						{
						}
					}
				}
			}
			catch (Exception ex)
			{
			}
			finally
			{
			}
		}
	}

	public interface IQueueWorker<in TItem>
	{
		void AddToExecutionQueue(TItem item);
	}
}
