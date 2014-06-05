using System;
using System.Threading;

namespace AlienJust.Support.Concurrent {
	public sealed class WaitableCounter {
		private readonly object _sync = new object();
		private int _count;

		private readonly AutoResetEvent _incrementSignal;
		private readonly AutoResetEvent _decrementSignal;
		private readonly AutoResetEvent _changeSignal;

		public WaitableCounter(int count) {
			_count = count;
			_incrementSignal = new AutoResetEvent(false);
			_decrementSignal = new AutoResetEvent(false);
			_changeSignal = new AutoResetEvent(false);
		}

		public WaitableCounter() {
			_incrementSignal = new AutoResetEvent(false);
			_decrementSignal = new AutoResetEvent(false);
			_changeSignal = new AutoResetEvent(false);
		}

		public void IncrementCount() {
			lock (_sync) {
				_count += 1;
				_changeSignal.Set();
				_incrementSignal.Set();
			}
		}

		public void DecrementCount() {
			lock (_sync) {
				_count -= 1;
				_changeSignal.Set();
				_decrementSignal.Set();
			}
		}

		/// <summary>
		/// Проверяет равенство счётчик с аргументом
		/// </summary>
		/// <returns>Истина, если счётчик равен аргументу</returns>
		public bool CompareCount(int compareTo) {
			lock (_sync) {
				return _count == compareTo;
			}
		}

		public int Count {
			get {
				lock (_sync) {
					return _count;
				}
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
				// сперва проверяем, затем ждем (мгновенная проверка предиката при вызове)
				// блокировка нужна для того чтобы не пропустить ни одного вызова .Set(), они тоже блокируются на _sync
				lock (_sync) {
					exit = predecate(Count);
				}
				if (exit) break;

				_changeSignal.WaitOne();
			}
		}

		public void ActOnLockedCounterAndIncrementCount(Action<int> actionOnLockedCount) {
			lock(_sync) {
				actionOnLockedCount(_count);
				_count += 1;
				_changeSignal.Set();
				_incrementSignal.Set();
			}
		}

		public void ActOnLockedCounterAndDecrementCount(Action<int> actionOnLockedCount)
		{
			lock (_sync)
			{
				actionOnLockedCount(_count);
				_count -= 1;
				_changeSignal.Set();
				_decrementSignal.Set();
			}
		}
	}
}
