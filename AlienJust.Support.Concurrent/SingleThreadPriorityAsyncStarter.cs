using System;
using System.Diagnostics;
using System.Security.Authentication.ExtendedProtection.Configuration;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent
{
	/// <summary>
	/// Запускает асинхронные задачи с разним приоритетом в отдельном потоке, позволяя контролировать максимальное число одновременно выполняемых асинхронных задач
	/// </summary>
	public sealed class SingleThreadPriorityAsyncStarter : IStoppableWorker
	{
		private readonly int _maxFlow;
		private readonly bool _isWaitAllTasksCompleteNeededOnStop;
		private readonly string _name; // TODO: implement interface INamedObject
		private readonly ILoggerWithStackTrace _debugLogger;
		private readonly SingleThreadedRelayMultiQueueWorker<Action> _asyncActionQueueWorker;
		private readonly WaitableCounter _flowCounter;

		public SingleThreadPriorityAsyncStarter(string name, ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState, ILoggerWithStackTrace debugLogger, int maxFlow, int queuesCount, bool isWaitAllTasksCompleteNeededOnStop)
		{
			if (debugLogger == null) throw new ArgumentNullException(nameof(debugLogger));

			_name = name;
			_debugLogger = debugLogger;
			_maxFlow = maxFlow;
			_isWaitAllTasksCompleteNeededOnStop = isWaitAllTasksCompleteNeededOnStop;

			_flowCounter = new WaitableCounter();
			_asyncActionQueueWorker = new SingleThreadedRelayMultiQueueWorker<Action>(_name, a => a(), threadPriority, markThreadAsBackground, apartmentState, debugLogger, queuesCount);
		}
		
		/// <summary>
		/// Добавляет действие в одну из очередей на выполнение
		/// </summary>
		/// <param name="asyncAction">Действие, которое будет выполнено асинхронно</param>
		/// <param name="queueNumber">Номер очереди (номер обратен приоритету), в которую будет добавлено действие</param>
		public void AddWork(Action asyncAction, int queueNumber) {
			_asyncActionQueueWorker.AddWork
				(
					() => {
						_flowCounter.WaitForCounterChangeWhileNotPredecate(curCount => curCount < _maxFlow);
						_flowCounter.IncrementCount();
						_debugLogger.Log("_flowCounter.Count = " + _flowCounter.Count, new StackTrace());
						asyncAction();
					},
					queueNumber
				);
		}

		/// <summary>
		/// Вызывается клиентом при выполнении асинхронной задачи,
		/// таким образом сообщяя, что асинхронная задача выполнена
		/// </summary>
		public void NotifyStarterAboutQueuedOperationComplete()
		{
			_flowCounter.DecrementCount();
			_debugLogger.Log("_flowCounter.Count = " + _flowCounter.Count, new StackTrace());
		}

		public void StopAsync() {
			_asyncActionQueueWorker.StopAsync();
		}

		public void WaitStopComplete() {
			_asyncActionQueueWorker.WaitStopComplete();
			_debugLogger.Log("Background worke has been stopped            ,,,,,,,,,,,,,,", new StackTrace());
			if (_isWaitAllTasksCompleteNeededOnStop) {
				_flowCounter.WaitForCounterChangeWhileNotPredecate(count => count == 0);
				_debugLogger.Log("Total tasks count is now 0                   ..............", new StackTrace());
			}
		}
	}
}
