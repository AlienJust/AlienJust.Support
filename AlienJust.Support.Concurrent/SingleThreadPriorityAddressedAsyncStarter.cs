using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienJust.Support.Loggers;

namespace AlienJust.Support.Concurrent
{
	/// <summary>
	/// Запускает асинхронные задачи с разним приоритетом в отдельном потоке, позволяя контролировать максимальное число одновременно выполняемых асинхронных задач
	/// </summary>
	public sealed class SingleThreadPriorityAddressedAsyncStarter<TAddressKey>
	{
		private readonly int _maxFlow;
		private readonly int _maxAddressFlow = 1;
		private readonly SingleThreadedRelayMultyQueueWorker<Action> _asyncActionQueueWorker;
		private readonly WaitableMultiCounter<TAddressKey> _flowCounters;
		private AddressedConcurrentQueueWithPriority<TAddressKey, Action> _queueForAddresses;
		public SingleThreadPriorityAddressedAsyncStarter(int maxFlow, int queuesCount)
		{
			_maxFlow = maxFlow;
			
			_flowCounters = new WaitableMultiCounter<TAddressKey>();
			_asyncActionQueueWorker = new SingleThreadedRelayMultyQueueWorker<Action>(a => a(), queuesCount);
			
		}

		/// <summary>
		/// Добавляет действие в одну из очередей на выполнение
		/// </summary>
		/// <param name="asyncAction">Действие, которое будет выполнено асинхронно</param>
		/// <param name="queueNumber">Номер очереди (номер обратен приоритету), в которую будет добавлено действие</param>
		/// <param name="key"> </param>
		public void AddToQueueForExecution(Action asyncAction, int queueNumber, TAddressKey key)
		{
			//GlobalLogger.Instance.Log("Adding action to queue=" + queueNumber);
			
			_asyncActionQueueWorker.AddToExecutionQueue(() =>
			                                            	{
			                                            		//GlobalLogger.Instance.Log("Waiting for shared flow...");
			                                            		while (_flowCounters.GetNotNullCountersCount() >= _maxFlow) _flowCounters.WaitForAnyDecrement(); // Ждем пока число занятых адресов не снизится меньше общего потока
			                                            		
																											if (_flowCounters.GetCount(key) < _maxAddressFlow)
			                                            		{
			                                            			_flowCounters.IncrementCount(key);
			                                            			asyncAction();
			                                            		}
			                                            		else
			                                            		{
			                                            			_queueForAddresses.Enqueue(key, () => AddToQueueForExecution(asyncAction, queueNumber, key), 0);
			                                            		}
			                                            	},
			                                            queueNumber);
		}

		/*private void AddToexecutionOrWaitingQueue(Action asyncAction, int queueNumber, TAddressKey key)
		{
			
		}*/

		/// <summary>
		/// Вызывается клиентом при выполнении асинхронной задачи,
		/// таким образом сообщяя, что асинхронная задача выполнена
		/// </summary>
		public void NotifyStarterAboutQueuedOperationComplete(TAddressKey key)
		{
			_flowCounters.DecrementCount(key);
			try
			{
				if (_flowCounters.GetCount(key) < _maxAddressFlow) // Если адрес свободен для получения команд
				{
					_queueForAddresses.Dequeue(key).Invoke();
				}
			}
			catch (Exception)
			{
				
			}
		}
	}

	
}
