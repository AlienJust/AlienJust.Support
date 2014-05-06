using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using AlienJust.Support.Concurrent.Contracts;

namespace AlienJust.Support.Concurrent {
	public sealed class QueueBackWorker<TItem> : IQueueWorker<TItem>, IThreadNotifier  {
		private readonly ConcurrentQueue<TItem> _items;
		private readonly Action<TItem> _actionInBackThread;
		private readonly BackgroundWorker _workThread;
		private readonly WaitableCounter _counter;

		public QueueBackWorker(Action<TItem> actionInBackThread)
		{
			_items = new ConcurrentQueue<TItem>();
			_actionInBackThread = actionInBackThread;

			_counter = new WaitableCounter(); // свой счетчик с методами ожидания

			_workThread = new BackgroundWorker {WorkerReportsProgress = true};
			_workThread.DoWork += WorkingThreadStart;
			_workThread.RunWorkerAsync();
			_workThread.ProgressChanged += (sender, args) => ((Action) args.UserState).Invoke(); // если вылетает исключение - то оно будет в потоке GUI
		}


		public void InsertAsFirstToExecutionQueue(TItem item) {
			throw new NotImplementedException("Not implemented!");
		}

		public void AddToExecutionQueue(TItem item) {
			_items.Enqueue(item);
			_counter.IncrementCount();
		}


		private void WorkingThreadStart(object sender, EventArgs args) {
			try {
				while (true) {
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
							_actionInBackThread(dequeuedItem);
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

		public void Notify(Action notifyAction) {
			_workThread.ReportProgress(0, notifyAction);
		}
	}
}
