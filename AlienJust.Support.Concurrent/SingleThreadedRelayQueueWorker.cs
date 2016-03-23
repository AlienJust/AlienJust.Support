using System;
using System.Collections.Concurrent;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent
{
	public sealed class SingleThreadedRelayQueueWorker<TItem> : IWorker<TItem>, IStoppableWorker {
		private readonly ILogger _debugLogger;
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

		public SingleThreadedRelayQueueWorker(string name, Action<TItem> action, ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState, ILogger debugLogger)
		{
			if (action == null) throw new ArgumentNullException("action");
			if (debugLogger == null) throw new ArgumentNullException("debugLogger");

			_syncRunFlags = new object();
			_syncUserActions = new object();

			_items = new ConcurrentQueue<TItem>();
			_name = name;
			_action = action;
			_debugLogger = debugLogger;
			

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
						_debugLogger.Log(ex);
						throw ex;
					}
				}
			}
		}

		private void WorkingThreadStart()
		{
			IsRunning = true;
			try {
				while (true)
				{
					if (MustBeStopped) throw new Exception("MustBeStopped is true, this is the end of thread");
					_debugLogger.Log("MustBeStopped was false, so continue dequeueing");
					// в этом цикле опустошаем очередь
					TItem dequeuedItem;
					bool shouldProceed = _items.TryDequeue(out dequeuedItem);
					if (shouldProceed)
					{
						try
						{
							_action(dequeuedItem);
						}
						catch (Exception ex)
						{
							_debugLogger.Log(ex);
						}
					}
					else
					{
						_debugLogger.Log("All actions from queue were executed, waiting for new ones");
						_threadNotifyAboutQueueItemsCountChanged.WaitOne();
						_debugLogger.Log("New action was enqueued, or stop is required!");
					}
				}
			}
			catch (Exception ex)
			{
				_debugLogger.Log(ex);
			}
			IsRunning = false;
		}

		public void StopAsync()
		{
			_debugLogger.Log("Stop called");
			lock (_syncUserActions) {
				_mustBeStopped = true;
				_threadNotifyAboutQueueItemsCountChanged.Set();
			}
		}

		public void WaitStopComplete() {
			_workThread.Join();
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

			set
			{
				lock (_syncRunFlags)
				{
					_mustBeStopped = value;
				}
			}
		}
	}
}
