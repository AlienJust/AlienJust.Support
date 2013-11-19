using System;
using System.Collections.Concurrent;
using AlienJust.Support.Concurrent.Contracts;

namespace AlienJust.Support.Concurrent {
	public sealed class ConcurrentGuidMemory<T> :IGuidMemory<T> {
		private readonly ConcurrentDictionary<Guid, T> _memory;
		public ConcurrentGuidMemory() {
			_memory = new ConcurrentDictionary<Guid, T>();
		}
		public Guid AddObject(T obj) {
			var guid = Guid.NewGuid();
			_memory.TryAdd(guid, obj);
			return guid;
		}

		public void AddObject(Guid guid, T obj) {
			_memory.TryAdd(guid, obj);
		}

		public void RemoveObject(Guid guid) {
			T obj;
			_memory.TryRemove(guid, out obj);
		}
	}
}