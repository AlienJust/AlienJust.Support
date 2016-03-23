using System;

namespace AlienJust.Support.Concurrent.Contracts {
	/// <summary>
	/// Интерфейс обработчика приоритетно-адресной очереди
	/// </summary>
	/// <typeparam name="TKey">Тип адреса очереди</typeparam>
	/// <typeparam name="TItem">Тип элемента очереди</typeparam>
	public interface IAddressedMultiQueueWorker<in TKey, in TItem>
	{
		/// <summary>
		/// Добавляет элемент в очередь по нужному адресу и с нужными приоритетом
		/// </summary>
		/// <param name="address">Адрес элемента</param>
		/// <param name="item">Элемент</param>
		/// <param name="queueNumber">Номер очреди (обычно обратен приоритету выполнения)</param>
		/// <returns>Уникальный идентификатор элемента в очереди</returns>
		Guid AddWork(TKey address, TItem item, int queueNumber);

		/// <summary>
		/// Удаляет элемент из очереди
		/// </summary>
		/// <param name="id">Уникальный идентификатор элемента</param>
		/// <returns>Истина, если удаление было произведено</returns>
		bool RemoveItem(Guid id);
	}
}