using System;

namespace AlienJust.Support.Concurrent {
	/// <summary>
	/// Хранит значение переменной, доступ к которому осуществляется синхронизировано
	/// </summary>
	/// <typeparam name="T">Тип значения</typeparam>
	public sealed class SyncObjectContainer<T> {
		private readonly object _sync;
		private T _value;

		public SyncObjectContainer(T initialValue) {
			_sync = new object();
			_value = initialValue;
		}

		public T Value {
			// NOTE: Thread safe through monitor
			get {
				lock (_sync) {
					return _value;
				}
			}
			set {
				lock (_sync) {
					_value = value;
				}
			}
		}

		public void LockedAction(Action<T> action) {
			lock (_sync) {
				action(_value);
			}
		}
	}
}