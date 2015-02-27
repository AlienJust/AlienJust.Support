using System;
using AlienJust.Support.Concurrent.Contracts;

namespace AlienJust.Support.Concurrent
{
	/// <summary>
	/// Запускает асинхронные задачи с разним приоритетом в отдельном потоке, 
	/// позволяя контролировать максимальное число одновременно выполняемых асинхронных задач
	/// и максимальное число одновременно выполняемых асинхронных задач для одного адреса
	/// </summary>
	public sealed class SingleThreadPriorityAddressedAsyncStarter<TAddressKey> : IPriorKeyedAsyncStarter<TAddressKey>
	{
		//private readonly int _maxTotalFlow; // максимальное число одновремменно запущенных задач

		private readonly SingleThreadedRelayAddressedMultiQueueWorker<TAddressKey, Action<IItemsReleaser<TAddressKey>>> _asyncActionQueueWorker;
		//private readonly WaitableCounter _totalFlowCounter; // счетчик текущего количества запущенных задач


		public SingleThreadPriorityAddressedAsyncStarter(int maxTotalFlow, int maxFlowPerAddress, int priorityGradation) {
			//_maxTotalFlow = maxTotalFlow;
			//_totalFlowCounter = new WaitableCounter();
			_asyncActionQueueWorker = new SingleThreadedRelayAddressedMultiQueueWorker<TAddressKey, Action<IItemsReleaser<TAddressKey>>>
				(
				RunActionWithAsyncTailBack,
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
				asyncOperationBeginAction(itemsReleaser);
			}
			catch (Exception) {
				// TODO: do something with exception :-)
				// TODO: thats bad, cause items can be not released
			}
		}


		/// <summary>
		/// Добавляет асинхронную операцию в очередь на выполнение
		/// </summary>
		/// <param name="asyncAction">Действие, которое будет протекать асинхронно</param>
		/// <param name="priority">Приоритет очереди</param>
		/// <param name="key">Ключ-адрес</param>
		/// <returns>Идентификатор задания</returns>
		public Guid AddToQueueForExecution(Action<Action> asyncAction, int priority, TAddressKey key)
		{
			return _asyncActionQueueWorker.AddToExecutionQueue
				(
					key,
					itemsReleaser => asyncAction(() => itemsReleaser.ReportSomeAddressedItemIsFree(key)),
					priority
				);
		}

		public bool RemoveExecution(Guid id) {
			return _asyncActionQueueWorker.RemoveItem(id);
		}
	}
}
