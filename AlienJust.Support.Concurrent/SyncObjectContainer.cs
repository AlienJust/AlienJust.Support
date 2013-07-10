namespace AlienJust.Support.Concurrent {
	/// <summary>
	/// Хранит значение переменной, доступ к которому осуществляется синхронизованно
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
				T result;
				lock (_sync) {
					result = _value;
				}
				return result;
			}
			set {
				lock (_sync) {
					_value = value;
				}
			}
		}
	}
}