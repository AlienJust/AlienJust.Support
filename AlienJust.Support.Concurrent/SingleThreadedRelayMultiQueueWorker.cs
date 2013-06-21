using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AlienJust.Support.Loggers;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent
{
	public sealed class SingleThreadedRelayMultiQueueWorker<TItem> : IMultiQueueWorker<TItem>
	{
		private readonly List<ConcurrentQueue<TItem>> _itemsQueues;
		//private readonly ConcurrentQueue<TItem> _itemsFirst;
		private readonly Action<TItem> _action;
		private readonly AutoResetEvent _threadNotify;
		private readonly Thread _workThread;


		public SingleThreadedRelayMultiQueueWorker(Action<TItem> action, int queuesCount)
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
				//GlobalLogger.Instance.Log("Adding item to execution queue_number=" + queueNumber);
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
							//GlobalLogger.Instance.Log("item received, producing action on it...");
							_action(item);
						}
						catch
						{
							// cannot execute action...
						}
					}
					catch (Exception ex)
					{
						//GlobalLogger.Instance.Log("No more items, waiting for new one...");
						_threadNotify.WaitOne(); // Итемы кончились, начинаем ждать
					}
				}
			}
			catch (Exception ex)
			{
				//throw ex;
			}
			finally
			{
				//Console.WriteLine("Background thread ending...");
			}
		}

		private TItem DequeueItemsReqursively(int currentQueueNumber)
		{
			//GlobalLogger.Instance.Log("currentQueueNumber=" + currentQueueNumber);
			int nextQueueNumber = currentQueueNumber + 1;
			if (currentQueueNumber >= _itemsQueues.Count) throw new Exception("No more queues");

			var items = _itemsQueues[currentQueueNumber];
			TItem dequeuedItem;
			if (items.TryDequeue(out dequeuedItem))
			{
				//GlobalLogger.Instance.Log("Item found, returning...");
				return dequeuedItem;
			}
			
			//GlobalLogger.Instance.Log("No items in queue=" + currentQueueNumber + " moving to newx queue...");
			return DequeueItemsReqursively(nextQueueNumber);
		}
	}
}
