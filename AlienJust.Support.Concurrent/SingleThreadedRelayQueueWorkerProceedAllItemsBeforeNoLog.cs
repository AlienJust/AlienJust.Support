using System;
using System.Collections.Concurrent;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;

namespace AlienJust.Support.Concurrent
{
	public sealed class SingleThreadedRelayQueueWorkerProceedAllItemsBeforeStopNoLog<TItem> : IWorker<TItem>, IStoppableWorker {
		private readonly object _syncUserActions;
		private readonly object _syncRunFlags;
		private readonly ConcurrentQueue<TItem> _items;
		private readonly string _name; // TODO: implement interface INamedObject
		private readonly Action<TItem> _action;
		private readonly Thread _workThread;
		//private readonly WaitableCounter _counter;

		private bool _isRunning;
		private bool _mustBeStopped; // Флаг, подающий фоновому потоку сигнал о необходимости завершения (обращение идет через потокобезопасное свойство MustBeStopped)

		private readonly AutoResetEvent _threadNotifyAboutQueueItemsCountChanged;

		public SingleThreadedRelayQueueWorkerProceedAllItemsBeforeStopNoLog(string name, Action<TItem> action, ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			_syncRunFlags = new object();
			_syncUserActions = new object();

			_items = new ConcurrentQueue<TItem>();
			_name = name;
			_action = action;

			//_counter = new WaitableCounter(); // свой счетчик с методами ожидания
			_threadNotifyAboutQueueItemsCountChanged = new AutoResetEvent(false);

			_isRunning = true;
			_mustBeStopped = false;

			_workThread = new Thread(WorkingThreadStart) { IsBackground = markThreadAsBackground, Priority = threadPriority, Name = name};
			if (apartmentState.HasValue) _workThread.SetApartmentState(apartmentState.Value);
			_workThread.Start();
		}

		public void AddWork(TItem workItem)
		{
			lock (_syncUserActions)
			{
				lock (_syncRunFlags) {
					if (!_mustBeStopped) {
						_items.Enqueue(workItem);
						_threadNotifyAboutQueueItemsCountChanged.Set();
					}
					else
					{
						var ex = new Exception("Cannot handle items any more, worker has been stopped or stopping now");
						throw ex;
					}
				}
			}
		}

		private void WorkingThreadStart()
		{
			IsRunning = true;
			try {
				while (true) {
					// в этом цикле опустошаем очередь
					TItem dequeuedItem;
					while (_items.TryDequeue(out dequeuedItem)) {
						try {
							_action(dequeuedItem);
						}
						catch
						{
							continue;
						}
					}
					_threadNotifyAboutQueueItemsCountChanged.WaitOne();

					if (MustBeStopped) throw new Exception("MustBeStopped is true, this is the end of thread");
				}
			}
			catch {
				IsRunning = false;
			}
		}

		public void StopAsync()
		{
			lock (_syncUserActions) {
				_mustBeStopped = true;
				_threadNotifyAboutQueueItemsCountChanged.Set();
			}
		}

		public void WaitStopComplete() {
			while (!_workThread.Join(100)) {
				_threadNotifyAboutQueueItemsCountChanged.Set();
			}
		}


		public bool IsRunning
		{
			get
			{
				bool result;
				lock (_syncRunFlags)
				{
					result = _isRunning;
				}
				return result;
			}

			private set
			{
				lock (_syncRunFlags)
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
				lock (_syncRunFlags)
				{
					result = _mustBeStopped;
				}
				return result;
			}
		}
	}
}
