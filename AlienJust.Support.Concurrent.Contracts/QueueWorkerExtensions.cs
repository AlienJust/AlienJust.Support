using System;
using System.Threading;

namespace AlienJust.Support.Concurrent.Contracts {
	public static class QueueWorkerExtensions
	{
		public static void AddToQueueAndWaitExecution(this IQueueWorker<Action> queueWorker, Action a)
		{
			var signal = new ManualResetEvent(false);
			queueWorker.AddToExecutionQueue(() =>
			                                {
			                                	a();
			                                	signal.Set();
			                                });
			signal.WaitOne();
		}

		public static void AddToQueueAndWaitExecution(this IQueueWorker<Action> queueWorker, Action a, TimeSpan timeout)
		{
			var signal = new ManualResetEvent(false);
			queueWorker.AddToExecutionQueue(() =>
			                                {
			                                	a();
			                                	signal.Set();
			                                });
			signal.WaitOne(timeout);
		}
	}
}