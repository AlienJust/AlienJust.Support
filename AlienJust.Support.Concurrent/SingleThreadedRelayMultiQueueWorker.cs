using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent
{
	public sealed class SingleThreadedRelayMultyQueueWorker<TItem> : IMultiQueueWorker<TItem>
	{
		private readonly List<ConcurrentQueue<TItem>> _itemsQueues;
		//private readonly ConcurrentQueue<TItem> _itemsFirst;
		private readonly Action<TItem> _action;
		private readonly AutoResetEvent _threadNotify;
		private readonly Thread _workThread;


		public SingleThreadedRelayMultyQueueWorker(Action<TItem> action, int queuesCount)
		{
			_itemsQueues = new List<ConcurrentQueue<TItem>>();
			for (int i = 0; i < queuesCount; ++i)
				_itemsQueues.Add(new ConcurrentQueue<TItem>());

				_action = action;

			_threadNotify = new AutoResetEvent(false);
			_workThread = new Thread(WorkingThreadStart) { IsBackground = true };
			_workThread.Start();
		}


		public void AddToExecutionQueue(TItem item, int queueNumber)
		{
			try
			{
				_itemsQueues[queueNumber].Enqueue(item);
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
					try
					{
						var item = DequeueItemsReqursively(0);
						try
						{
							_action(item);
						}
						catch
						{

						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message + ", waiting...");
						_threadNotify.WaitOne(); // Итемы кончились, начинаем ждать
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

		private TItem DequeueItemsReqursively(int currentQueueNumber)
		{
			int nextQueueNumber = currentQueueNumber + 1;
			if (currentQueueNumber >= _itemsQueues.Count) throw new Exception("No more queues");

			var items = _itemsQueues[currentQueueNumber];
			TItem dequeuedItem;
			if (items.TryDequeue(out dequeuedItem))
			{
				return dequeuedItem;
			}
			
			Console.WriteLine("No more items in queue No " + currentQueueNumber + " move to next queue");
			return DequeueItemsReqursively(nextQueueNumber);
		}
	}

	public interface IMultiQueueWorker<in TItem>
	{
		void AddToExecutionQueue(TItem item, int queueNumber);
	}
}
