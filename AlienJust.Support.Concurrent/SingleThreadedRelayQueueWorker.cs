﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;

namespace AlienJust.Support.Concurrent {
	public sealed class SingleThreadedRelayQueueWorker<TItem> : IWorker<TItem> {
		private readonly object _sync;
		private readonly ConcurrentQueue<TItem> _items;
		private readonly Action<TItem> _action;
		private readonly Thread _workThread;
		private readonly WaitableCounter _counter;

		private bool _isRunning;
		private bool _mustBeStopped; // Флаг, подающий фоновому потоку сигнал о необходимости завершения (обращение идет через потокобезопасное свойство MustBeStopped)

		public SingleThreadedRelayQueueWorker(Action<TItem> action, ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState) {
			_sync = new object();
			_items = new ConcurrentQueue<TItem>();
			_action = action;

			_counter = new WaitableCounter(); // свой счетчик с методами ожидания

			_isRunning = true;
			_mustBeStopped = false;

			_workThread = new Thread(WorkingThreadStart) {IsBackground = markThreadAsBackground, Priority = threadPriority};
			if (apartmentState.HasValue) _workThread.SetApartmentState(apartmentState.Value);
			_workThread.Start();
		}


		public void InsertAsFirstToExecutionQueue(TItem item) {
			throw new NotImplementedException("Not implemented!");
		}

		public void AddWork(TItem workItem) {
			if (!MustBeStopped) {
				_items.Enqueue(workItem);
				_counter.IncrementCount();
			}
			else throw new Exception("Cannot handle items any more, worker has been stopped or stopping now");
		}

		private void WorkingThreadStart() {
			try {
				while (!MustBeStopped)
				{
					// В этом цикле ждем пополнения очереди:
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

		public void StopSynchronously()
		{
			if (IsRunning)
			{
				MustBeStopped = true;
				_counter.IncrementCount(); // счетчик очереди сбивается, но это не важно, потому что после этого метода поток уничтожается

				_workThread.Join();
				IsRunning = false;
			}
		}

		public bool IsRunning
		{
			get
			{
				bool result;
				lock (_sync)
				{
					result = _isRunning;
				}
				return result;
			}

			private set
			{
				lock (_sync)
				{
					_isRunning = value;
				}
			}
		}

		private bool MustBeStopped
		{
			get
			{
				bool result;
				lock (_sync)
				{
					result = _mustBeStopped;
				}
				return result;
			}

			set
			{
				lock (_sync)
				{
					_mustBeStopped = value;
				}
			}
		}
	}
}
