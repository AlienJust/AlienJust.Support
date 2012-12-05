using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AlienJust.Support.Concurrent
{
	public sealed class WaitableMultiCounter<TKey>
	{
		private int _count;
		private readonly AutoResetEvent _incrementSignal = new AutoResetEvent(false);
		private readonly AutoResetEvent _decrementSignal = new AutoResetEvent(false);

		private readonly ConcurrentDictionary<TKey, WaitableCounter> _counters = new ConcurrentDictionary<TKey,WaitableCounter>();

		private WaitableCounter GetCounter(TKey key)
		{
			return _counters.GetOrAdd(key, k => new WaitableCounter());
		}

		public void IncrementCount(TKey key)
		{
			GetCounter(key).IncrementCount();
			Interlocked.Increment(ref _count);
			_incrementSignal.Set();
		}
		public void DecrementCount(TKey key)
		{
			GetCounter(key).DecrementCount();
			Interlocked.Decrement(ref _count);
			_decrementSignal.Set();
		}

		/// <summary>
		/// Проверяет равенство счётчика с аргументом
		/// </summary>
		/// <returns>Истина, если счётчик равен аргументу</returns>
		public bool CompareCount(int compareTo)
		{
			return Thread.VolatileRead(ref _count) == compareTo;
		}

		public int GetTotalCount()
		{
			return Thread.VolatileRead(ref _count);
			//return _count;
		}
		public int GetCount(TKey key)
		{
			return GetCounter(key).GetCount();
			//return _count;
		}

		public void WaitForAnyIncrement()
		{
			_incrementSignal.WaitOne();
		}
		public void WaitForIncrement(TKey key)
		{
			GetCounter(key).WaitForIncrement();
		}

		public void WaitForAnyDecrement()
		{
			_decrementSignal.WaitOne();
		}
		public void WaitForDecrement(TKey key)
		{
			GetCounter(key).WaitForDecrement();
		}


		/// <summary>
		/// Ожидает снижение счётчика до нуля
		/// </summary>
		//public void WaitForDowncount(int waitForValue)
		//{
		//while (!CompareCount(waitForValue)) _decrementSignal.WaitOne();
		//}

		public void WaitForAnyDecrementWhileNotPredecate(Func<int, bool> predecate)
		{
			while (!predecate(GetTotalCount())) WaitForAnyDecrement();
		}

		public void WaitForDecrementWhileNotPredecate(TKey key, Func<int, bool> predecate)
		{
			while (!predecate(GetCount(key))) WaitForDecrement(key);
		}

		public int GetNotNullCountersCount()
		{
			return _counters.Sum(wc => wc.Value.GetCount() > 0 ? 1 : 0);
		}
	}
}
