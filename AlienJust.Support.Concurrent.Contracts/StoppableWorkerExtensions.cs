using System;
using System.Threading;

namespace AlienJust.Support.Concurrent.Contracts {
	public static class StoppableWorkerExtensions {
		public static void AddLastWorkAndWaitExecution(this IStoppableWorker<Action> worker, Action a) {
			var signal = new ManualResetEvent(false);
			Exception exception = null;
			worker.AddLastWork(() => {
				try {
					a();
				}
				catch (Exception ex) {
					exception = ex;
				}
				finally {
					signal.Set();
				}
			});
			signal.WaitOne();
			if (exception != null) throw exception;
		}
	}
}