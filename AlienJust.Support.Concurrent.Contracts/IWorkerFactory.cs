using System;

namespace AlienJust.Support.Concurrent.Contracts {
	public interface IWorkerFactory {
		IAsyncWorker GetSimpleWorker(Action run, Action<int> progress, Action<Exception> complete);
	}
}