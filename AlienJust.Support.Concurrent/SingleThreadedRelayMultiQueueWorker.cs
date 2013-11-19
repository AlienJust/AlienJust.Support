using System;
using System.Threading;
using AlienJust.Support.Concurrent.Contracts;

namespace AlienJust.Support.Concurrent {
	public sealed class SingleThreadedRelayMultiQueueWorker<TItem> : IMultiQueueWorker<TItem> {
		private readonly ConcurrentQueueWithPriority<TItem> _cpQueue;
		private readonly Action<TItem> _action;
		private readonly AutoResetEvent _threadNotify;
		private readonly Thread _workThread;


		public SingleThreadedRelayMultiQueueWorker(Action<TItem> action, int queuesCount) {
			_cpQueue = new ConcurrentQueueWithPriority<TItem>(queuesCount);
			_action = action;

			_threadNotify = new AutoResetEvent(false);
			_workThread = new Thread(WorkingThreadStart) {IsBackground = true};
			_workThread.Start();
		}


		public void AddToExecutionQueue(TItem item, int queueNumber) {
			try {
				_cpQueue.Enqueue(item, queueNumber);
				_threadNotify.Set();
			}
			catch (Exception ex) {

			}
		}

		public void ClearQueue() {
			_cpQueue.ClearQueue();
		}


		private void WorkingThreadStart() {
			try {
				while (true) {
					try {
						var item = _cpQueue.Dequeue();
						try {
							//GlobalLogger.Instance.Log("item received, producing action on it...");
							_action(item);
						}
						catch {
							// cannot execute action...
						}
					}
					catch (Exception ex) {
						//GlobalLogger.Instance.Log("No more items, waiting for new one...");
						_threadNotify.WaitOne(); // Итемы кончились, начинаем ждать
					}
				}
			}
			catch (Exception ex) {
				//throw ex;
			}
			finally {
				//Console.WriteLine("Background thread ending...");
			}
		}
	}
}
