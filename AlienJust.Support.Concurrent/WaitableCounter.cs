using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Concurrent
{
	public sealed class WaitableCounter
	{
		private int _count;
		private readonly AutoResetEvent _incrementSignal = new AutoResetEvent(false);
		private readonly AutoResetEvent _decrementSignal = new AutoResetEvent(false);

		public void IncrementCount()
		{
			Interlocked.Increment(ref _count);
			_incrementSignal.Set();
		}
		public void DecrementCount()
		{
			Interlocked.Decrement(ref _count);
			_decrementSignal.Set();
		}

		/// <summary>
		/// Проверяет равенство счётчик с аргументом
		/// </summary>
		/// <returns>Истина, если счётчик равен аргументу</returns>
		public bool CompareCount(int compareTo)
		{
			return Thread.VolatileRead(ref _count) == compareTo;
		}

		public int GetCount()
		{
			return Thread.VolatileRead(ref _count);
			//return _count;
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
