using System;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent {
	/// <summary>
	/// Запускает асинхронные задачи с разним приоритетом в отдельном потоке, 
	/// позволяя контролировать максимальное число одновременно выполняемых асинхронных задач
	/// и максимальное число одновременно выполняемых асинхронных задач для одного адреса
	/// </summary>
	public sealed class SingleThreadPriorityAddressedAsyncStarter<TAddressKey> : IPriorKeyedAsyncStarter<TAddressKey>, IStoppableWorker {
		private readonly string _name; // TODO: implement interface INamedObject
		private readonly ILogger _debugLogger;
		private readonly bool _isWaitAllTasksCompleteNeededOnStop;

		private readonly WaitableCounter _totalFlowCounter; // счетчик текущего количества запущенных задач
		private readonly SingleThreadedRelayAddressedMultiQueueWorker<TAddressKey, Action<IItemsReleaser<TAddressKey>>> _asyncActionQueueWorker;

		public SingleThreadPriorityAddressedAsyncStarter(
			string name,
			ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState, ILogger debugLogger,
			uint maxTotalFlow, uint maxFlowPerAddress, int priorityGradation, bool isWaitAllTasksCompleteNeededOnStop) {

			if (debugLogger == null) throw new ArgumentNullException(nameof(debugLogger));
			_name = name;
			_debugLogger = debugLogger;
			_isWaitAllTasksCompleteNeededOnStop = isWaitAllTasksCompleteNeededOnStop;

			_totalFlowCounter = new WaitableCounter(0);
			_asyncActionQueueWorker = new SingleThreadedRelayAddressedMultiQueueWorker<TAddressKey, Action<IItemsReleaser<TAddressKey>>>
				(
				_name, RunActionWithAsyncTailBack, threadPriority, markThreadAsBackground, apartmentState, debugLogger,
				priorityGradation,
				maxFlowPerAddress,
				maxTotalFlow);
		}

		/// <summary>
		/// Запускает завершение асинхронной операции, передавая на вход заершения входной освободитель итемов
		/// </summary>
		/// <param name="asyncOperationBeginAction">Действие, знаменующее завершение асинхронной операции</param>
		/// <param name="itemsReleaser">Освободитель итемов</param>
		private void RunActionWithAsyncTailBack(Action<IItemsReleaser<TAddressKey>> asyncOperationBeginAction, IItemsReleaser<TAddressKey> itemsReleaser) {
			try {
				_totalFlowCounter.IncrementCount();
				_debugLogger.Log("_totalFlowCounter.Count = " + _totalFlowCounter.Count);
				asyncOperationBeginAction(itemsReleaser);
			}
			catch (Exception ex) {
				_debugLogger.Log(ex);
			}
		}


		/// <summary>
		/// Добавляет асинхронную операцию в очередь на выполнение
		/// </summary>
		/// <param name="asyncAction">Действие, которое будет протекать асинхронно</param>
		/// <param name="priority">Приоритет очереди</param>
		/// <param name="key">Ключ-адрес</param>
		/// <returns>Идентификатор задания</returns>
		public Guid AddWork(Action<Action> asyncAction, int priority, TAddressKey key) {
			var id = _asyncActionQueueWorker.AddWork
				(
					key,
					itemsReleaser => asyncAction(() => {
						itemsReleaser.ReportSomeAddressedItemIsFree(key);
						_totalFlowCounter.DecrementCount();
						_debugLogger.Log("_totalFlowCounter.Count = " + _totalFlowCounter.Count);
					}),
					priority
				);
			return id;
		}

		public bool RemoveExecution(Guid id) {
			return _asyncActionQueueWorker.RemoveItem(id);
		}

		public uint MaxTotalFlow {
			// Thread safety is guaranteed by worker
			get => _asyncActionQueueWorker.MaxTotalOnetimeItemsUsages;
			set => _asyncActionQueueWorker.MaxTotalOnetimeItemsUsages = value;
		}

		public void StopAsync() {
			_asyncActionQueueWorker.StopAsync();
		}

		public void WaitStopComplete() {
			_asyncActionQueueWorker.WaitStopComplete();
			_debugLogger.Log("Background worker has been stopped");
			if (_isWaitAllTasksCompleteNeededOnStop) {
				_totalFlowCounter.WaitForCounterChangeWhileNotPredecate(count => count == 0);
				_debugLogger.Log("Total tasks count is now 0");
			}
		}
	}
}
