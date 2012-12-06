using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienJust.Support.Loggers;

namespace AlienJust.Support.Concurrent
{
	/// <summary>
	/// Запускает асинхронные задачи с разним приоритетом в отдельном потоке, 
	/// позволяя контролировать максимальное число одновременно выполняемых асинхронных задач
	/// и максимальное число одновременно выполняемых асинхронных задач для одного адреса
	/// </summary>
	public sealed class SingleThreadPriorityAddressedAsyncStarter<TAddressKey>
	{
		private readonly int _maxTotalFlow;
		
		private readonly SingleThreadedRelayAddressedMultiQueueWorker<TAddressKey, Action<Action<TAddressKey>>> _asyncActionQueueWorker;
		private readonly WaitableCounter _flowCounter;


		public SingleThreadPriorityAddressedAsyncStarter(int maxTotalFlow, int maxFlowPerAddress, int priorityGradation)
		{
			_maxTotalFlow = maxTotalFlow;
			_flowCounter = new WaitableCounter();
			_asyncActionQueueWorker = new SingleThreadedRelayAddressedMultiQueueWorker<TAddressKey, Action<Action<TAddressKey>>>(RunActionWthAsyncTailBack, priorityGradation, maxFlowPerAddress);
		}

		/// <summary>
		/// Запускает действие, завершающееся другим действием, передаваемым в качестве параметра запускаемому действию
		/// </summary>
		/// <param name="action">Действие, которое нужно запустить</param>
		/// <param name="callbackTail">Завершающее дейсвтие</param>
		private void RunActionWthAsyncTailBack(Action<Action<TAddressKey>> action, Action<TAddressKey> callbackTail)
		{
			action(callbackTail);
		}


		/// <summary>
		/// Добавляет действие в одну из очередей на выполнение
		/// </summary>
		/// <param name="asyncAction">Действие, которое будет выполнено асинхронно</param>
		/// <param name="priority">Приоритет</param>
		/// <param name="key"> </param>
		public void AddToQueueForExecution(Action<Action> asyncAction, int priority, TAddressKey key)
		{
			//GlobalLogger.Instance.Log("Adding action to queue=" + priority);

			_asyncActionQueueWorker.AddToExecutionQueue(key, notifyBackAction =>
			                                                 	{
			                                                 		//GlobalLogger.Instance.Log("Waiting for decrement, waitCount=" + _flowCounter.GetCount());
			                                                 		_flowCounter.WaitForDecrementWhileNotPredecate(curCount => curCount < _maxTotalFlow);
			                                                 		//GlobalLogger.Instance.Log("waiting complete, executing...");

			                                                 		_flowCounter.IncrementCount();
			                                                 		asyncAction(() =>
			                                                 		            	{
			                                                 		            		_asyncActionQueueWorker.ReportItemIsFree(key);
			                                                 		            		_flowCounter.DecrementCount();
			                                                 		            	});
			                                                 	},
			                                            priority);
		}
	}
}
