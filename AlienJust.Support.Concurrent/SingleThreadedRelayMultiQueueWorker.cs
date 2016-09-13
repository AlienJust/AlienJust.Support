﻿using System;
using System.Diagnostics;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent {
	public sealed class SingleThreadedRelayMultiQueueWorker<TItem> : IMultiQueueWorker<TItem>, IStoppableWorker {
		private readonly ConcurrentQueueWithPriority<TItem> _items;
		private readonly Action<TItem> _action;
		private readonly AutoResetEvent _threadNotifyAboutQueueItemsCountChanged;
		private readonly Thread _workThread;

		private readonly ILoggerWithStackTrace _debugLogger;
		private readonly string _name; // TODO: implement interface INamedObject
		private readonly object _syncUserActions;
		private readonly object _syncRunFlags;

		private bool _isRunning;
		private bool _mustBeStopped; // Флаг, подающий фоновому потоку сигнал о необходимости завершения (обращение идет через потокобезопасное свойство MustBeStopped)

		public SingleThreadedRelayMultiQueueWorker(string name, Action<TItem> action, ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState, ILoggerWithStackTrace debugLogger, int queuesCount) {
			if (action == null) throw new ArgumentNullException(nameof(action));
			if (debugLogger == null) throw new ArgumentNullException(nameof(debugLogger));

			_syncRunFlags = new object();
			_syncUserActions = new object();

			_name = name;
			_action = action;
			_debugLogger = debugLogger;

			_threadNotifyAboutQueueItemsCountChanged = new AutoResetEvent(false);

			_items = new ConcurrentQueueWithPriority<TItem>(queuesCount);

			_isRunning = true;
			_mustBeStopped = false;

			_workThread = new Thread(WorkingThreadStart) { Priority = threadPriority, IsBackground = markThreadAsBackground, Name = name };
			if (apartmentState.HasValue) _workThread.SetApartmentState(apartmentState.Value);
			_workThread.Start();
		}


		public void AddWork(TItem workItem, int queueNumber) {
			lock (_syncUserActions) {
				lock (_syncRunFlags) {
					if (!_mustBeStopped) {
						_items.Enqueue(workItem, queueNumber);
						_threadNotifyAboutQueueItemsCountChanged.Set();
					}
					else {
						var ex = new Exception("Cannot handle items any more, worker has been stopped or stopping now");
						_debugLogger.Log(ex, new StackTrace());
						throw ex;
					}
				}
			}
		}

		public void ClearQueue() {
			_items.ClearQueue();
		}


		private void WorkingThreadStart() {
			IsRunning = true;
			try {
				while (true) {
					try {
						var item = _items.Dequeue();
						try {
							//GlobalLogger.Instance.Log("item received, producing action on it...");
							_action(item);
						}
						catch (Exception ex) {
							_debugLogger.Log(ex, new StackTrace());
						}
					}
					catch (Exception ex) {
						_debugLogger.Log("All actions from queue were executed, waiting for new ones", new StackTrace());
						_threadNotifyAboutQueueItemsCountChanged.WaitOne(); // Итемы кончились, начинаем ждать

						if (MustBeStopped) throw new Exception("MustBeStopped is true, this is the end of thread");
						_debugLogger.Log("MustBeStopped was false, so continue dequeueing", new StackTrace());

						_debugLogger.Log(ex, new StackTrace());
					}
				}
			}
			catch (Exception ex) {
				_debugLogger.Log(ex, new StackTrace());
			}
			IsRunning = false;
		}

		public void StopAsync() {
			_debugLogger.Log("Stop called", new StackTrace());
			lock (_syncUserActions) {
				_mustBeStopped = true;
				_threadNotifyAboutQueueItemsCountChanged.Set();
			}
		}

		public void WaitStopComplete() {
			_workThread.Join();
		}

		public bool IsRunning {
			get {
				bool result;
				lock (_syncRunFlags) {
					result = _isRunning;
				}
				return result;
			}

			private set {
				lock (_syncRunFlags) {
					_isRunning = value;
				}
			}
		}

		private bool MustBeStopped {
			get {
				bool result;
				lock (_syncRunFlags) {
					result = _mustBeStopped;
				}
				return result;
			}

			set {
				lock (_syncRunFlags) {
					_mustBeStopped = value;
				}
			}
		}
	}
}
