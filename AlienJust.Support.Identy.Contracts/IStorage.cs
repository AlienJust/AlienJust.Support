using System.Collections.Generic;

namespace AlienJust.Support.Identy.Contracts {
	public interface IStorage<TDataItem> where TDataItem : IObjectWithIdentifier
	{
		IEnumerable<TDataItem> StoredItems { get; }
		void Add(TDataItem item);
		void Remove(TDataItem item);
		void Update(TDataItem item);
	}
}