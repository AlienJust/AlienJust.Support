﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlienJust.Support.Concurrent
{
	/// <summary>
	/// Запускает асинхронные задачи с разним приоритетом в отдельном потоке, позволяя контролировать максимальное число одновременно выполняемых асинхронных задач
	/// </summary>
	public sealed class SingleThreadPriorityAsyncStarter
	{
		private readonly int _maxFlow;
		//private readonly int _queuesCount;
		private readonly SingleThreadedRelayMultyQueueWorker<Action> _asyncActionQueueWorker;
		private readonly WaitableCounter _flowCounter;

		public SingleThreadPriorityAsyncStarter(int maxFlow, int queuesCount)
		{
			_maxFlow = maxFlow;
			//_queuesCount = queuesCount;
			_flowCounter = new WaitableCounter();
			_asyncActionQueueWorker = new SingleThreadedRelayMultyQueueWorker<Action>(a => a(), queuesCount);
		}
		
		/// <summary>
		/// Добавляет действие в одну из очередей на выполнение
		/// </summary>
		/// <param name="asyncAction">Действие, которое будет выполнено асинхронно</param>
		/// <param name="queueNumber">Номер очереди (определяет приоритет), в которую будет добавлено действие</param>
		public void AddToQueueForExecution(Action asyncAction, int queueNumber)
		{
			_flowCounter.IncrementCount();
			_asyncActionQueueWorker.AddToExecutionQueue(()=>
			                                            	{
																											_flowCounter.WaitForDecrementWhileNotPredecate(curCount => curCount < _maxFlow);
			                                            		asyncAction();
			                                            	}, queueNumber);
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
