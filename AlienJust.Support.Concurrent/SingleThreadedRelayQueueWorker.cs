using System;
using System.Collections.Concurrent;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;

namespace AlienJust.Support.Concurrent {
	public sealed class SingleThreadedRelayQueueWorker<TItem> : IQueueWorker<TItem> {
		private readonly ConcurrentQueue<TItem> _items;
		private readonly Action<TItem> _action;
		private readonly Thread _workThread;
		private readonly WaitableCounter _counter;

		public SingleThreadedRelayQueueWorker(Action<TItem> action, ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState) {
			_items = new ConcurrentQueue<TItem>();
			_action = action;

			_counter = new WaitableCounter(); // свой счетчик с методами ожидания

			_workThread = new Thread(WorkingThreadStart) {IsBackground = markThreadAsBackground, Priority = threadPriority};
			if (apartmentState.HasValue) _workThread.SetApartmentState(apartmentState.Value);
			_workThread.Start();
		}


		public void InsertAsFirstToExecutionQueue(TItem item) {
			throw new NotImplementedException("Not implemented!");
		}

		public void AddToExecutionQueue(TItem item) {
			_items.Enqueue(item);
			_counter.IncrementCount();
		}


		private void WorkingThreadStart() {
			try {
				while (true) {
					// В этом цикле ждем пополнения очереди:
					//_counter.WaitForIncrement();
					_counter.WaitForCounterChangeWhileNotPredecate(count => count > 0);
					while (true) {
						// в этом цикле опустошаем очередь
						TItem dequeuedItem;
						bool shouldProceed = _items.TryDequeue(out dequeuedItem);
						if (!shouldProceed) {
							break;
						}

						try {
							_action(dequeuedItem);
						}
						catch {
							continue;
						}
						finally {
							_counter.DecrementCount();
						}
					}
				}
			}
			catch {
				//swallow all exeptions
			}
		}
	}
}
