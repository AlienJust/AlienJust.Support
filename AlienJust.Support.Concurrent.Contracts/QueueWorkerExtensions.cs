using System;
using System.Threading;

namespace AlienJust.Support.Concurrent.Contracts {
	public static class QueueWorkerExtensions
	{
		public static void AddToQueueAndWaitExecution(this IQueueWorker<Action> queueWorker, Action a)
		{
			var signal = new ManualResetEvent(false);
			Exception exception = null;
			queueWorker.AddToExecutionQueue(() =>
			                                {
														  try
														  {
															  a();
														  }
														  catch (Exception ex)
														  {
															  exception = ex;
														  }
														  finally {
														  	signal.Set();
														  }
			                                });
			signal.WaitOne();
			if (exception != null) throw exception;
		}

		public static void AddToQueueAndWaitExecution(this IQueueWorker<Action> queueWorker, Action a, TimeSpan timeout) {
			var sync = new object();
			var signal = new ManualResetEvent(false);
			bool wasExecuted = false;
			Exception exception = null;
			queueWorker.AddToExecutionQueue(() =>
			                                {
														  try
														  {
															  a();
															  lock (sync) {
															  	wasExecuted = true;
															  }
														  }
														  catch (Exception ex)
														  {
															  exception = ex;
														  }
														  finally
														  {
															  signal.Set();
														  }
			                                });
			signal.WaitOne(timeout);
			bool hasBeenExecuted;
			lock (sync) {
				hasBeenExecuted = wasExecuted;
			}
			if (!hasBeenExecuted) throw new Exception("������� ��������");
			if (exception != null) throw exception;
		}
	}
}