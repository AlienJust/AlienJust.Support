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
		private readonly Thread _workThread;
		private readonly object _sync = new object();
		private readonly WaitableCounter _counter;
		private readonly ManualResetEvent _proceedNotify;
		public SingleThreadedRelayQueueWorker(Action<TItem> action, ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState)
		{
			_items = new ConcurrentQueue<TItem>();
			_action = action;

			_proceedNotify = new ManualResetEvent(false);
			_counter = new WaitableCounter();

			_workThread = new Thread(WorkingThreadStart) { IsBackground = markThreadAsBackground, Priority = threadPriority };
			if (apartmentState.HasValue) _workThread.SetApartmentState(apartmentState.Value);
			_workThread.Start();
		}


		public void InsertAsFirstToExecutionQueue(TItem item)
		{
			throw new NotImplementedException("Not implemented!");
		}

		public void AddToExecutionQueue(TItem item)
		{
			try
			{
				_items.Enqueue(item);
				_counter.IncrementCount();
				//_proceedNotify.Set();
				//lock (_sync)
				//{
					//Monitor.Pulse(_sync);
				//}
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
					Console.WriteLine("waiting for inrement...");
					//lock (_sync) Monitor.Wait(_sync);
					//_proceedNotify.WaitOne();
					_counter.WaitForIncrement();
					Console.WriteLine("increment received");
					while (true) {
						bool shouldProceed;
						TItem dequeuedItem;
						//lock (_sync) {
							shouldProceed = _items.TryDequeue(out dequeuedItem);
						//}
						if (!shouldProceed) {
							Console.WriteLine("no more items");
							break;
						}

						try
						{
							Console.WriteLine("executing action:");
							_action(dequeuedItem);
						}
						catch { continue;}
						finally 
						{
							_counter.DecrementCount();
							
						}
					}
					//_proceedNotify.Reset();
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
