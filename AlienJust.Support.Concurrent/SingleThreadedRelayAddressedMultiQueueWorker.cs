﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AlienJust.Support.Loggers;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent
{
	public sealed class SingleThreadedRelayAddressedMultiQueueWorker<TKey, TItem> : IAddressedMultiQueueWorker<TKey, TItem>
	{
		private readonly ConcurrentQueueWithPriorityAndAddressUsageControl<TKey, TItem> _queue;
		//private readonly ConcurrentQueue<TItem> _itemsFirst;
		private readonly Action<TItem, Action<TKey>> _action;
		private readonly AutoResetEvent _threadNotify;
		private readonly Thread _workThread;


		public SingleThreadedRelayAddressedMultiQueueWorker(Action<TItem, Action<TKey>> action, int maxPriority, int maxParallelUsingItemsCount)
		{
			_queue = new ConcurrentQueueWithPriorityAndAddressUsageControl<TKey, TItem>(maxPriority, maxParallelUsingItemsCount);
			_action = action;

			_threadNotify = new AutoResetEvent(false);
			_workThread = new Thread(WorkingThreadStart) { IsBackground = true };
			_workThread.Start();
		}


		public void AddToExecutionQueue(TKey key, TItem item, int queueNumber)
		{
			try
			{
				_queue.Enqueue(key, item, queueNumber);
				_threadNotify.Set();
			}
			catch (Exception ex)
			{
				//Console.WriteLine(ex);
			}
		}

		public void ReportItemIsFree(TKey address)
		{
			_queue.ReportDecrementItemUsages(address);
		}


		private void WorkingThreadStart()
		{
			try
			{
				while (true)
				{
					try
					{
						var item = _queue.Dequeue();
						try
						{
							//GlobalLogger.Instance.Log("item received, producing action on it...");
							_action(item, ReportItemIsFree);
						}
						catch(Exception ex)
						{
							//GlobalLogger.Instance.Log(ex.ToString());
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

		
	}
}
