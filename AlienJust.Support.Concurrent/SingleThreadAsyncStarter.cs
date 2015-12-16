using System;
using System.Threading;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent {
	/// <summary>
	/// Запускает асинхронные задачи в отдельном потоке, позволяя контролировать максимальное число одновременно выполняемых асинхронных задач
	/// </summary>
	public sealed class SingleThreadAsyncStarterWithFlowControl {
		private readonly string _name;
		private readonly int _maxFlow;
		private readonly ILogger _debugLogger;
		private readonly SingleThreadedRelayQueueWorker<Action> _asyncActionQueueWorker;
		private readonly WaitableCounter _flowCounter;

		public SingleThreadAsyncStarterWithFlowControl(string name, int maxFlow, ThreadPriority threadPriority, bool markThreadAsBackground, ApartmentState? apartmentState, ILogger debugLogger) {
			if (_debugLogger == null) throw new ArgumentNullException("debugLogger");
			_name = name;
			_maxFlow = maxFlow;
			_debugLogger = debugLogger;

			_flowCounter = new WaitableCounter();
			_asyncActionQueueWorker = new SingleThreadedRelayQueueWorker<Action>(_name, a => a(), threadPriority, markThreadAsBackground, apartmentState, _debugLogger);
		}

		public void AddToQueueForExecution(Action asyncAction) {
			_asyncActionQueueWorker.AddWork(() => {
				_flowCounter.WaitForCounterChangeWhileNotPredecate(curCount => curCount < _maxFlow);

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
