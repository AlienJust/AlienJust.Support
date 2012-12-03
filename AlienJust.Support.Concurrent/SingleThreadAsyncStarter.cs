using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlienJust.Support.Concurrent
{
	/// <summary>
	/// Запускает асинхронные задачи в отдельном потоке, позволяя контролировать максимальное число одновременно выполняемых асинхронных задач
	/// </summary>
	public sealed class SingleThreadAsyncStarterWithFlowControl
	{
		private readonly int _maxFlow;
		private readonly SingleThreadedRelayQueueWorker<Action> _asyncActionQueueWorker;
		private readonly WaitableCounter _flowCounter;
		
		public SingleThreadAsyncStarterWithFlowControl(int maxFlow)
		{
			_maxFlow = maxFlow;
			_flowCounter = new WaitableCounter();
			_asyncActionQueueWorker = new SingleThreadedRelayQueueWorker<Action>(a=>a());
		}
		
		public void AddToQueueForExecution(Action asyncAction)
		{
			_asyncActionQueueWorker.AddToExecutionQueue(()=>
			                                            	{
																											_flowCounter.WaitForDecrementWhileNotPredecate(curCount => curCount < _maxFlow);
																											
																											_flowCounter.IncrementCount();
			                                            		asyncAction();
			                                            	});
		}


		/// <summary>
		/// Вызывается клиентом при выполнении асинхронной задачи,
		/// таким образом сообщяя, что асинхронная задача выполнена
		/// </summary>
		public void NotifyStarterAboutQueuedOperationComplete()
		{
			_flowCounter.DecrementCount();
		}
	}

	
}
