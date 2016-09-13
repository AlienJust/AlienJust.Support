using System;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent {

	/// <summary>
	/// Однопоточный обработчик приоритетно-адресной очереди
	/// </summary>
	/// <typeparam name="TKey">Тип адресов очереди</typeparam>
	/// <typeparam name="TItem">Тип элементов очереди</typeparam>
	public sealed class SingleThreadedRelayAddressedMultiQueueWorker<TKey, TItem> : IAddressedMultiQueueWorker<TKey, TItem>, IItemsReleaser<TKey>, IStoppableWorker {

		private readonly ConcurrentQueueWithPriorityAndAddressUsageControlGuided<TKey, TItem> _items;

		private readonly Action<TItem, IItemsReleaser<TKey>> _relayUserAction; // Пользовательское действие, которое будет совершаться над каждым элементом в порядке очереди
		private readonly AutoResetEvent _threadNotifyAboutQueueItemsCountChanged;
		private readonly Thread _workThread;

		private readonly string _name; // TODO: implement interface INamedObject
		private readonly ILogger _debugLogger;

		private readonly object _syncRunFlags;
		private readonly object _syncUserActions;

		private bool _isRunning;
		private bool _mustBeStopped; // Флаг, подающий фоновому потоку сигнал о необходимости завершения (обращение идет через потокобезопасное свойство MustBeStopped)

		public SingleThreadedRelayAddressedMultiQueueWorker(
			string name,
			Action<TItem, IItemsReleaser<TKey>> relayUserAction,
			ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState, ILogger debugLogger,
			int maxPriority, uint maxParallelUsingItemsCount, uint maxTotalOnetimeItemsUsages) {

			if (relayUserAction == null) throw new ArgumentNullException(nameof(relayUserAction));
			if (debugLogger == null) throw new ArgumentNullException(nameof(debugLogger));

			_syncRunFlags = new object();
			_syncUserActions = new object();

			_name = name;
			_relayUserAction = relayUserAction;
			_debugLogger = debugLogger;

			_threadNotifyAboutQueueItemsCountChanged = new AutoResetEvent(false);
			_items = new ConcurrentQueueWithPriorityAndAddressUsageControlGuided<TKey, TItem>(maxPriority, maxParallelUsingItemsCount, maxTotalOnetimeItemsUsages);

			_isRunning = true;
			_mustBeStopped = false;

			_workThread = new Thread(WorkingThreadStart) {Priority = threadPriority, IsBackground = markThreadAsBackground, Name = name};
			if (apartmentState.HasValue) _workThread.SetApartmentState(apartmentState.Value);
			_workThread.Start();
		}


		public Guid AddWork(TKey key, TItem item, int queueNumber) {
			lock (_syncUserActions) {
				lock (_syncRunFlags) {
					if (!_mustBeStopped) {
						Guid result = _items.Enqueue(key, item, queueNumber);
						_threadNotifyAboutQueueItemsCountChanged.Set();
						return result;
					}
					var ex = new Exception("Cannot handle items any more, worker has been stopped or stopping now");
					_debugLogger.Log(ex);
					throw ex;
				}
			}
		}

		public void ReportSomeAddressedItemIsFree(TKey address) {
			_items.ReportDecrementItemUsages(address);
			lock (_syncUserActions) {
				_threadNotifyAboutQueueItemsCountChanged.Set();
			}
		}

		public bool RemoveItem(Guid id) {
			var result = _items.RemoveItem(id);
			lock (_syncUserActions) {
				_threadNotifyAboutQueueItemsCountChanged.Set();
			}
			return result;
		}


		private void WorkingThreadStart() {
			IsRunning = true;
			try {
				while (true) {
					try {
						var item = _items.Dequeue(); // выбрасывает исключение, если очередь пуста, и поток переходит к ожиданию сигнала
						//var releaser = new ItemReleaserRelayWithExecutionCountControl<TKey>((IItemsReleaser<TKey>) this);
						try {
							_relayUserAction(item, (IItemsReleaser<TKey>) this); // TODO: Warning! Если в пользовательсоком действии произойдет ошибка, то счетчик элементов застрянет!
						}
						catch (Exception ex) {
							// Даже если действие над элементом очереди не получилось, нужно проверить, не осталось ли еще чего нибудь в очереди
							// НО, я не знаю адреса:
							// if (!releaser.SomeItemWasReleased)
							//releaser.ReportSomeAddressedItemIsFree( TODO );
							_debugLogger.Log(ex);
						}
					}
					catch (Exception ex) {
						_debugLogger.Log(ex);
						_debugLogger.Log("All actions from queue were executed, waiting for new ones");
						_threadNotifyAboutQueueItemsCountChanged.WaitOne(); // Итемы кончились, начинаем ждать (основное время проводится здесь в ожидании :-))

						if (MustBeStopped) throw new Exception("MustBeStopped is true, this is the end of thread");
						_debugLogger.Log("MustBeStopped was false, so continue dequeueing");

						_debugLogger.Log("New action was enqueued, or stop is required!");
					}
				}
			}
			catch (Exception ex) {
				_debugLogger.Log(ex);
			}
			IsRunning = false;
		}

		public void StopAsync() {
			_debugLogger.Log("Stop called");
			lock (_syncUserActions) {
				MustBeStopped = true;
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

		public uint MaxTotalOnetimeItemsUsages {
			// Thread safity is guaranted by queue
			get { return _items.MaxTotalUsingItemsCount; }
			set {
				lock (_syncUserActions) {
					bool isDequeueNeeded = value > _items.MaxTotalUsingItemsCount;
					_items.MaxTotalUsingItemsCount = value;
					if (isDequeueNeeded) _threadNotifyAboutQueueItemsCountChanged.Set();
				}
			}
		}
	}
}

