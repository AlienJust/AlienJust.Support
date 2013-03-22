using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AlienJust.Support.Concurrent {
	/// <summary>
	/// Запускает асинхронные задачи в отдельном потоке, позволяя контролировать максимальное число одновременно выполняемых асинхронных задач
	/// </summary>
	public sealed class SingleThreadAsyncStarterWithFlowControl {
		private readonly int _maxFlow;
		private readonly SingleThreadedRelayQueueWorker<Action> _asyncActionQueueWorker;
		private readonly WaitableCounter _flowCounter;

		public SingleThreadAsyncStarterWithFlowControl(int maxFlow, ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState) {
			_maxFlow = maxFlow;
			_flowCounter = new WaitableCounter();
			_asyncActionQueueWorker = new SingleThreadedRelayQueueWorker<Action>(a => a(), threadPriority, markThreadAsBackground, apartmentState);
		}

		public void AddToQueueForExecution(Action asyncAction) {
			_asyncActionQueueWorker.AddToExecutionQueue(() => {
			                                            	_flowCounter.WaitForDecrementWhileNotPredecate(curCount => curCount < _maxFlow);

			                                            	_flowCounter.IncrementCount();
			                                            	asyncAction();
			                                            });
		}


		/// <summary>
		/// Вызывается клиентом при выполнении асинхронной задачи,
		/// таким образом сообщяя, что асинхронная задача выполнена
		/// </summary>
		public void NotifyStarterAboutQueuedOperationComplete() {
			_flowCounter.DecrementCount();
		}
	}
}
