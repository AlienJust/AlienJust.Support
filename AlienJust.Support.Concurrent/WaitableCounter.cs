using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent
{
	public sealed class WaitableCounter {
		private readonly object _sync = new object();
		private int _count;
		private readonly AutoResetEvent _incrementSignal = new AutoResetEvent(false);
		private readonly AutoResetEvent _decrementSignal = new AutoResetEvent(false);

		public void IncrementCount()
		{
			Interlocked.Increment(ref _count);
			lock(_sync) _incrementSignal.Set();
			Console.WriteLine("Count = " + GetCount());
		}
		public void DecrementCount()
		{
			Interlocked.Decrement(ref _count);
			lock(_sync) _decrementSignal.Set();
		}

		/// <summary>
		/// Проверяет равенство счётчик с аргументом
		/// </summary>
		/// <returns>Истина, если счётчик равен аргументу</returns>
		public bool CompareCount(int compareTo) {
			bool compareResult;
			lock(_sync) compareResult = Thread.VolatileRead(ref _count) == compareTo;
			return compareResult;
		}

		public int GetCount() {
			int count;
			lock(_sync) count = Thread.VolatileRead(ref _count);
			return count;
		}

		public void WaitForIncrement()
		{
			_incrementSignal.WaitOne();
		}

		public void WaitForDecrement()
		{
			_decrementSignal.WaitOne();
		}

		/// <summary>
		/// Ожидает снижение счётчика до нуля
		/// </summary>
		//public void WaitForDowncount(int waitForValue)
		//{
		//while (!CompareCount(waitForValue)) _decrementSignal.WaitOne();
		//}

		public void WaitForDecrementWhileNotPredecate(Func<int, bool> predecate)
		{
			while (!predecate(GetCount())) _decrementSignal.WaitOne();
		}
	}
}
