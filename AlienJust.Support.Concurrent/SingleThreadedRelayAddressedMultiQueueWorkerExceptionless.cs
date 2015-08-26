using System;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;

namespace AlienJust.Support.Concurrent {

	/// <summary>
	/// Однопоточный обработчик приоритетно-адресной очереди
	/// </summary>
	/// <typeparam name="TKey">Тип адресов очереди</typeparam>
	/// <typeparam name="TItem">Тип элементов очереди</typeparam>
	public sealed class SingleThreadedRelayAddressedMultiQueueWorkerExceptionless<TKey, TItem> : IAddressedMultiQueueWorker<TKey, TItem>, IItemsReleaser<TKey> {
		private readonly object _sync;
		private readonly ConcurrentQueueWithPriorityAndAddressUsageControlGuided<TKey, TItem> _queue;

		private readonly Action<TItem, IItemsReleaser<TKey>> _relayUserAction; // Пользовательское действие, которое будет совершаться над каждым элементом в порядке очереди
		private readonly AutoResetEvent _threadNotifyAboutQueueItemsCountChanged;
		private readonly Thread _workThread;

		private bool _isRunning;
		private bool _mustBeStopped; // Флаг, подающий фоновому потоку сигнал о необходимости завершения (обращение идет через потокобезопасное свойство MustBeStopped)

        public SingleThreadedRelayAddressedMultiQueueWorkerExceptionless(Action<TItem, IItemsReleaser<TKey>> relayUserAction, int maxPriority, int maxParallelUsingItemsCount, int maxTotalOnetimeItemsUsages)
		{
			_sync = new object();

			_queue = new ConcurrentQueueWithPriorityAndAddressUsageControlGuided<TKey, TItem>(maxPriority, maxParallelUsingItemsCount, maxTotalOnetimeItemsUsages);
			_relayUserAction = relayUserAction;

			_threadNotifyAboutQueueItemsCountChanged = new AutoResetEvent(false);

			_isRunning = true;
			_mustBeStopped = false;
			_workThread = new Thread(WorkingThreadStart) {IsBackground = true};
			_workThread.Start();
		}


		public Guid AddToExecutionQueue(TKey key, TItem item, int queueNumber) {
			if (IsRunning) {
				Guid result = _queue.Enqueue(key, item, queueNumber);
				_threadNotifyAboutQueueItemsCountChanged.Set();
				return result;
			}
			throw new Exception("Background thread was stopped, i will not add item to queue :-)");
		}

		public void ReportSomeAddressedItemIsFree(TKey address)
		{
			_queue.ReportDecrementItemUsages(address);
			_threadNotifyAboutQueueItemsCountChanged.Set();
		}

		public bool RemoveItem(Guid id) {
			var result = _queue.RemoveItem(id);
			_threadNotifyAboutQueueItemsCountChanged.Set();
			return result;
		}


	    private void WorkingThreadStart() {
	        try {
	            while (!MustBeStopped) {
	                TItem item;
	                bool isItemTaken = _queue.TryDequeue(out item); // выбрасывает исключение, если очередь пуста, и поток переходит к ожиданию сигнала
	                //var releaser = new ItemReleaserRelayWithExecutionCountControl<TKey>((IItemsReleaser<TKey>) this);
	                if (isItemTaken) {
	                    try {
	                        _relayUserAction(item, (IItemsReleaser<TKey>) this); // TODO: Warning! Если в пользовательсоком действии произойдет ошибка, то счетчик элементов застрянет!
	                    }
	                    catch {
	                        // Даже если действие над элементом очереди не получилось, нужно проверить, не осталось ли еще чего нибудь в очереди
	                        // НО, я не знаю адреса:
	                        // if (!releaser.SomeItemWasReleased)
	                        //releaser.ReportSomeAddressedItemIsFree( TODO );
	                    }
	                }
	                else {
	                    _threadNotifyAboutQueueItemsCountChanged.WaitOne(); // Итемы кончились, начинаем ждать (основное время проводится здесь в ожидании :-))    
	                }
	            }
	        }
	        catch {
	            //swallow all execptions
	        }
	    }

	    public void StopWorker() {
			if (IsRunning) {
				MustBeStopped = true;
				
				// На случай, если поток ждет итемов (очередь пуста). 
				// При взведении этого нотификатора поток продолжится и при следуюущей итерации цикла фактически завершится.
				_threadNotifyAboutQueueItemsCountChanged.Set(); 

				_workThread.Join();
				IsRunning = false;
			}
		}

		public bool IsRunning {
			get {
				bool result;
				lock (_sync) {
					result = _isRunning;
				}
				return result;
			}

			private set {
				lock (_sync) {
					_isRunning = value;
				}
			}
		}

		private bool MustBeStopped {
			get {
				bool result;
				lock (_sync) {
					result = _mustBeStopped;
				}
				return result;
			}

			set {
				lock (_sync) {
					_mustBeStopped = value;
				}
			}
		}


		public int MaxTotalOnetimeItemsUsages {
			// Thread safity is guaranted by queue
			get { return _queue.MaxTotalUsingItemsCount; }
			set { _queue.MaxTotalUsingItemsCount = value; }
		}
	}
}
