using System;
using AlienJust.Support.Concurrent.Contracts;

namespace AlienJust.Support.Concurrent {
	public sealed class AsyncMemoriedWorkerFactory : IWorkerFactory {
		private readonly ConcurrentGuidMemory<IAsyncWorker> _memory;

		public AsyncMemoriedWorkerFactory() {
			_memory = new ConcurrentGuidMemory<IAsyncWorker>();
		}

		public IAsyncWorker GetSimpleWorker(Action<IAsyncWorkerProgressHandler> run, Action<int> progress, Action<Exception> complete)
		{
			var guid = Guid.NewGuid();
			var worker = new RelayAsyncWorker(
				run,
				progress,
				e => {
					try {
						complete(e);
					}
					finally {
						_memory.RemoveObject(guid);
					}
				});
			_memory.AddObject(guid, worker);
			return worker;
		}

	}
}