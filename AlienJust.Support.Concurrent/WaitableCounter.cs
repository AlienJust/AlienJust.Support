using System;
using System.Threading;

namespace AlienJust.Support.Concurrent {
	public sealed class WaitableCounter {
		private readonly object _sync = new object();
		private int _count;
		private readonly AutoResetEvent _incrementSignal;
		private readonly AutoResetEvent _decrementSignal;
		private readonly AutoResetEvent _changeSignal;

		public WaitableCounter() {
			_incrementSignal = new AutoResetEvent(false);
			_decrementSignal = new AutoResetEvent(false);
			_changeSignal = new AutoResetEvent(false);
		}

		public void IncrementCount() {
			Interlocked.Increment(ref _count);
			lock (_sync) {
				_changeSignal.Set();
				_incrementSignal.Set();
			}
		}

		public void DecrementCount() {
			Interlocked.Decrement(ref _count);
			lock (_sync) {
				_changeSignal.Set();
				_decrementSignal.Set();
			}
		}

		/// <summary>
		/// Проверяет равенство счётчик с аргументом
		/// </summary>
		/// <returns>Истина, если счётчик равен аргументу</returns>
		public bool CompareCount(int compareTo) {
			bool compareResult;
			lock (_sync) compareResult = Thread.VolatileRead(ref _count) == compareTo;
			return compareResult;
		}

		public int Count {
			get {
				int count;
				lock (_sync) count = Thread.VolatileRead(ref _count);
				return count;
			}
		}

		public void WaitForIncrement() {
			_incrementSignal.WaitOne();
		}

		public void WaitForDecrement() {
			_decrementSignal.WaitOne();
		}

		public void WaitForCounterChangeWhileNotPredecate(Func<int, bool> predecate) {
			while (true) {
				bool exit;
				// lock для того чтобы не пропустить ни одного .Set(), они тоже лочатся на _sync
				lock (_sync) {
					exit = predecate(Count);
				}
				if (exit) break;
				_changeSignal.WaitOne();
			}
		}
	}
}
