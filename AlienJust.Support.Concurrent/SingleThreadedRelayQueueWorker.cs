﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent
{
	public sealed class SingleThreadedRelayQueueWorker<TItem> : IWorker<TItem>, IStoppableWorker {
		private readonly ILoggerWithStackTrace _debugLogger;
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

		public SingleThreadedRelayQueueWorker(string name, Action<TItem> action, ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState, ILoggerWithStackTrace debugLogger)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));
			if (debugLogger == null) throw new ArgumentNullException(nameof(debugLogger));

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
						_debugLogger.Log(ex, new StackTrace());
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
					// в этом цикле опустошаем очередь
					TItem dequeuedItem;
					bool shouldProceed = _items.TryDequeue(out dequeuedItem);
					if (shouldProceed)
					{
						try
						{
							_debugLogger.Log("Before user action", new StackTrace(Thread.CurrentThread, true));
							_action(dequeuedItem);
							_debugLogger.Log("After user action", new StackTrace(Thread.CurrentThread, true));
						}
						catch (Exception ex)
						{
							_debugLogger.Log(ex, new StackTrace(Thread.CurrentThread, true));
						}
					}
					else
					{
						_debugLogger.Log("All actions from queue were executed, waiting for new ones", new StackTrace(Thread.CurrentThread, true));
						_threadNotifyAboutQueueItemsCountChanged.WaitOne();

						if (MustBeStopped) throw new Exception("MustBeStopped is true, this is the end of thread");
						_debugLogger.Log("MustBeStopped was false, so continue dequeueing", new StackTrace(Thread.CurrentThread, true));

						_debugLogger.Log("New action was enqueued, or stop is required!", new StackTrace(Thread.CurrentThread, true));
					}
				}
			}
			catch (Exception ex)
			{
				_debugLogger.Log(ex, new StackTrace(Thread.CurrentThread, true));
			}
			IsRunning = false;
		}

		public void StopAsync()
		{
			_debugLogger.Log("Stop called", new StackTrace(Thread.CurrentThread, true));
			lock (_syncUserActions) {
				_mustBeStopped = true;
				_threadNotifyAboutQueueItemsCountChanged.Set();
			}
		}

		public void WaitStopComplete() {
			_debugLogger.Log("Waiting for thread exit begans...", new StackTrace(Thread.CurrentThread, true));
			while (!_workThread.Join(100)) {
				_debugLogger.Log("Waiting for thread exit...", new StackTrace(Thread.CurrentThread, true));
				_threadNotifyAboutQueueItemsCountChanged.Set();
			}
			_debugLogger.Log("Waiting for thread exit complete", new StackTrace(Thread.CurrentThread, true));
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
