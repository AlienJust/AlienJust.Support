using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AlienJust.Support.Concurrent {
	public sealed class WaitableMultiCounter<TKey> {
		private readonly ConcurrentDictionary<TKey, WaitableCounter> _counters = new ConcurrentDictionary<TKey, WaitableCounter>();
		private readonly WaitableCounter _totalCounter = new WaitableCounter();


		private WaitableCounter GetCounter(TKey key) {
			return _counters.GetOrAdd(key, k => new WaitableCounter());
		}

		public void IncrementCount(TKey key) {
			GetCounter(key).IncrementCount();
			_totalCounter.IncrementCount();
		}

		public void DecrementCount(TKey key) {
			GetCounter(key).DecrementCount();
			_totalCounter.DecrementCount();
		}

		/// <summary>
		/// Проверяет равенство счётчика с аргументом
		/// </summary>
		/// <returns>Истина, если счётчик равен аргументу</returns>
		public bool CompareCount(int compareTo) {
			return _totalCounter.CompareCount(compareTo);
		}

		public int TotalCount {
			get { return _totalCounter.Count; }
		}

		public int GetCount(TKey key) {
			return GetCounter(key).Count;
		}

		public void WaitForAnyIncrement() {
			_totalCounter.WaitForIncrement();
		}

		public void WaitForIncrement(TKey key) {
			GetCounter(key).WaitForIncrement();
		}

		public void WaitForAnyDecrement() {
			_totalCounter.WaitForDecrement();
		}

		public void WaitForDecrement(TKey key) {
			GetCounter(key).WaitForDecrement();
		}

		public void WaitForAnyCounterChangeWhileNotPredecate(Func<int, bool> predecate) {
			_totalCounter.WaitForCounterChangeWhileNotPredecate(predecate);
		}

		public void WaitForCounterChangeWhileNotPredecate(TKey key, Func<int, bool> predecate) {
			GetCounter(key).WaitForCounterChangeWhileNotPredecate(predecate);
		}

		public int GetNotZeroCountersCount() {
			return _counters.Sum(wc => wc.Value.Count > 0 ? 1 : 0);
		}
	}
}
