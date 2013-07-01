using System;

namespace AlienJust.Support.Concurrent
{
	sealed class AddressedItem<TKey, TItem>
	{
		public TKey Key { get; private set;}
		public TItem Item { get; private set; }
		public AddressedItem(TKey key, TItem item)
		{
			Key = key;
			Item = item;
		}
	}

	sealed class AddressedItemGuided<TKey, TItem>
	{
		public TKey Key { get; private set; }
		public TItem Item { get; private set; }
		public Guid Id { get; private set; }
		public AddressedItemGuided(TKey key, TItem item, Guid id) {
			Key = key;
			Item = item;
			Id = id;
		}
	}
}