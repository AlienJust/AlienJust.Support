using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AlienJust.Support.Concurrent
{
	/// <summary>
	/// Потокобезопасная очередь с приоритетом и адресацией
	/// </summary>
	/// <typeparam name="TKey">Тип адреса</typeparam>
	/// <typeparam name="TItem">Тип элемента</typeparam>
	public sealed class ConcurrentQueueWithPriorityAndAddressUsageControlGuided<TKey, TItem>
	{
		private readonly object _syncRoot = new object();
		private readonly List<List<AddressedItemGuided<TKey, TItem>>> _itemCollections;
		private readonly int _maxParallelUsingItemsCount;
		private readonly int _maxTotalUsingItemsCount;
		private readonly WaitableMultiCounter<TKey> _itemsInUseCounters;

		/// <summary>
		/// Создает новую очередь
		/// </summary>
		/// <param name="maxPriority">Максимальный приоритет</param>
		/// <param name="maxParallelUsingItemsCount">Максимальное количество одновременно разрешенных выборок элементов</param>
		/// <param name="maxTotalUsingItemsCount">Максимальное общее число выборок элементов</param>
		public ConcurrentQueueWithPriorityAndAddressUsageControlGuided(int maxPriority, int maxParallelUsingItemsCount, int maxTotalUsingItemsCount)
		{
			_maxParallelUsingItemsCount = maxParallelUsingItemsCount;
			_maxTotalUsingItemsCount = maxTotalUsingItemsCount;
			_itemCollections = new List<List<AddressedItemGuided<TKey, TItem>>>();
			
			for (int i = 0; i < maxPriority; ++i) {
				_itemCollections.Add(new List<AddressedItemGuided<TKey, TItem>>());
			}
			
			_itemsInUseCounters = new WaitableMultiCounter<TKey>();
		}

		/// <summary>
		/// Сообщает очереди о том, что адресованный элемент больше не используется
		/// (после того как число используемых элементов снизится до разрешенного значения, будет разрешена дальнейшая выборка элементов с таким адресом)
		/// </summary>
		/// <param name="address">Адрес элемента, который больше не используется</param>
		public void ReportDecrementItemUsages(TKey address)
		{
			_itemsInUseCounters.DecrementCount(address);
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
				TItem result;
				lock (_syncRoot) {
					result = DequeueItemsCycle();
				}
				return result;
			}
			catch (Exception ex) {
				throw new Exception("Cannot get item", ex);
			}
		}

        private TItem DequeueItemsCycle()
        {
            if (_itemsInUseCounters.TotalCount >= _maxTotalUsingItemsCount) throw new Exception("Cannot get item because max total limit riched");

            foreach (var items in _itemCollections)
            {
                for (int j = 0; j < items.Count; ++j)
                {
                    var item = items[j];
                    if (_itemsInUseCounters.GetCount(item.Key) < _maxParallelUsingItemsCount) // т.е. пропускаем итем в случае превышения использования итемов с таким ключем
                    {
                        items.RemoveAt(j);
                        _itemsInUseCounters.IncrementCount(item.Key);
                        return item.Item;
                    }
                }
            }
            throw new Exception("All queues passed, no more queues");
        }


	    public bool TryDequeue(out TItem result) {
	        lock (_syncRoot) {
	            return TryDequeueItemsCycle(out result);
	        }
	    }


	    private bool TryDequeueItemsCycle(out TItem result) {
		    if (_itemsInUseCounters.TotalCount >= _maxTotalUsingItemsCount) {
			    result = default(TItem);
				return false;
		    }
		    

	        foreach (var items in _itemCollections) {
	            for (int j = 0; j < items.Count; ++j) {
	                var item = items[j];
	                if (_itemsInUseCounters.GetCount(item.Key) < _maxParallelUsingItemsCount) // т.е. пропускаем итем в случае превышения использования итемов с таким ключем
	                {
	                    items.RemoveAt(j);
	                    _itemsInUseCounters.IncrementCount(item.Key);
	                    result = item.Item;
	                    return true;
	                }
	            }
	        }
	        result = default(TItem);
	        return false;
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
