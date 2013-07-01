using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienJust.Support.Loggers;

namespace AlienJust.Support.Concurrent
{
	/// <summary>
	/// Очередь с приоритетом и адресацией
	/// </summary>
	/// <typeparam name="TKey">Тип адреса</typeparam>
	/// <typeparam name="TItem">Тип элемента</typeparam>
	public sealed class ConcurrentQueueWithPriorityAndAddressUsageControlGuided<TKey, TItem>
	{
		private readonly object _syncRoot = new object();
		private readonly List<List<AddressedItemGuided<TKey, TItem>>> _itemCollections;
		private readonly int _maxParallelUsingItemsCount;
		private readonly WaitableMultiCounter<TKey> _itemCounters;

		/// <summary>
		/// Создает новую очередь
		/// </summary>
		/// <param name="maxPriority">Максимальный приоритет</param>
		/// <param name="maxParallelUsingItemsCount">Максимальное количество одновременно разрешенных выборок элементов</param>
		public ConcurrentQueueWithPriorityAndAddressUsageControlGuided(int maxPriority, int maxParallelUsingItemsCount)
		{
			_maxParallelUsingItemsCount = maxParallelUsingItemsCount;
			_itemCollections = new List<List<AddressedItemGuided<TKey, TItem>>>();
			
			for (int i = 0; i < maxPriority; ++i)
			{
				_itemCollections.Add(new List<AddressedItemGuided<TKey, TItem>>());
			}
			
			_itemCounters = new WaitableMultiCounter<TKey>();
		}

		/// <summary>
		/// Сообщяет очереди о том, что адресованный элемент больше не используется
		/// (после того как число используемых элементов снизится до разрешенного значения, будет разрешена дальнейшая выборка элементов с таким адресом)
		/// </summary>
		/// <param name="address">Адрес элемента, который больше не используется</param>
		public void ReportDecrementItemUsages(TKey address)
		{
			_itemCounters.DecrementCount(address);
		}

		/// <summary>
		/// Добавляет элемент в очередь
		/// </summary>
		/// <param name="key">Адрес элемента (ключ)</param>
		/// <param name="item">Элемент</param>
		/// <param name="priority">Приоритет (0 - наивысший приоритет)</param>
		public Guid Enqueue(TKey key, TItem item, int priority) {
			var guid = Guid.NewGuid();
			lock (_syncRoot) {
				_itemCollections[priority].Add(new AddressedItemGuided<TKey, TItem>(key, item, guid));
			}
			return guid;
		}

		/// <summary>
		/// Обходит очереди по приоритетам и выбирает элемент с наивысшим приоритетом из имеющихся
		/// </summary>
		/// <returns>Взятый из очереди элемент</returns>
		/// <exception cref="Exception">Исключение, итемов не найдено</exception>
		public TItem Dequeue() {
			try {
				lock (_syncRoot) {
					return DequeueItemsReqursively(0);
				}
			}
			catch (Exception ex) {
				throw new Exception("Cannot get item");
			}
		}

		/// <summary>
		/// Рекурсивно выбирает нужный итем
		/// </summary>
		/// <param name="currentQueueNumber">Номер очереди (приоритет)</param>
		/// <returns></returns>
		private TItem DequeueItemsReqursively(int currentQueueNumber)
		{
			//GlobalLogger.Instance.Log("currentQueueNumber=" + currentQueueNumber);
			int nextQueueNumber = currentQueueNumber + 1;
			if (currentQueueNumber >= _itemCollections.Count) throw new Exception("No more queues");

			var items = _itemCollections[currentQueueNumber];
			if (items.Count > 0)
			{
				for (int i = 0; i < items.Count; i++)
				{
					var item = items[i];
					if (_itemCounters.GetCount(item.Key) < _maxParallelUsingItemsCount) // т.е. пропускаем итем в случае превышения использования итемов с таким ключем
					{
						items.RemoveAt(i);
						_itemCounters.IncrementCount(item.Key);
						return item.Item;
					}
					//GlobalLogger.Instance.Log("item found, but it usages limited, skipping item with addr=" + item.Key.ToString());
				}
			}
			//GlobalLogger.Instance.Log("No items in queue=" + currentQueueNumber + " moving to newx queue...");
			return DequeueItemsReqursively(nextQueueNumber);
		}

		/// <summary>
		/// Удаляет элемент из коллекции
		/// </summary>
		/// <param name="id">Идентификатор итема</param>
		/// <returns>Истина, если элемент с таки идентификатором был и был удален :о</returns>
		public bool RemoveItem(Guid id) {
			var result = false;
			lock (_syncRoot) {
				List<AddressedItemGuided<TKey, TItem>> foundCollection = null;
				AddressedItemGuided<TKey, TItem> foundItem = null;
				foreach (var collection in _itemCollections) {
					foreach (var item in collection) {
						if (item.Id == id) {
							foundItem = item;
							break;
						}
					}
					if (foundItem != null) {
						foundCollection = collection;
						break;
					}
				}
				// if collection is not null, then found item is allways not null:
				if (foundCollection != null) {
					result = foundCollection.Remove(foundItem);
				}
			}
			return result;
		}
	}
}
