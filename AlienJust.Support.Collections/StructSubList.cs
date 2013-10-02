using System;
using System.Collections;
using System.Collections.Generic;

namespace AlienJust.Support.Collections {

	/// <summary>
	/// Формирует подсписок структур из большего списка
	/// </summary>
	/// <typeparam name="T">Тип структур данных</typeparam>
	public class StructSubList<T> : IList<T> where T : struct
	{
		#region Fields

		private readonly int _startIndex;
		private readonly int _endIndex;
		private readonly int _count;
		private readonly IList<T> _source;

		#endregion

		public StructSubList(IList<T> source, int startIndex, int count) {
			_source = source;
			_startIndex = startIndex;
			_count = count;
			_endIndex = _startIndex + _count - 1;
		}

		#region IList<T> Members

		public int IndexOf(T item) {
				for (int i = _startIndex; i <= _endIndex; i++) {
					if (item.Equals(_source[i]))
						return i;
				}
			return -1;
		}

		public void Insert(int index, T item) {
			throw new NotSupportedException();
		}

		public void RemoveAt(int index) {
			throw new NotSupportedException();
		}

		public T this[int index] {
			get {
				if (index >= 0 && index < _count)
					return _source[index + _startIndex];
				else
					throw new IndexOutOfRangeException("index");
			}
			set {
				if (index >= 0 && index < _count)
					_source[index + _startIndex] = value;
				else
					throw new IndexOutOfRangeException("index");
			}
		}

		#endregion

		#region ICollection<T> Members

		public void Add(T item) {
			throw new NotSupportedException();
		}

		public void Clear() {
			throw new NotSupportedException();
		}

		public bool Contains(T item) {
			return IndexOf(item) >= 0;
		}

		public void CopyTo(T[] array, int arrayIndex) {
			for (int i = 0; i < _count; i++) {
				array[arrayIndex + i] = _source[i + _startIndex];
			}
		}

		public int Count {
			get { return _count; }
		}

		public bool IsReadOnly {
			get { return true; }
		}

		public bool Remove(T item) {
			throw new NotSupportedException();
		}

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator() {
			for (int i = _startIndex; i < _endIndex; i++) {
				yield return _source[i];
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		#endregion
	}
}
