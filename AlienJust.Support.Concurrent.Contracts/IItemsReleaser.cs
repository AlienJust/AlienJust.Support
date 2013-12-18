namespace AlienJust.Support.Concurrent.Contracts {
	/// <summary>
	/// Осовободитель элементов
	/// </summary>
	/// <typeparam name="TKey">Тип адресации элементов</typeparam>
	public interface IItemsReleaser<in TKey> {

		/// <summary>
		/// Освобождает элемент по указанному адресу
		/// </summary>
		/// <param name="address"></param>
		void ReportSomeAddressedItemIsFree(TKey address);
	}
}