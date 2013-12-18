namespace AlienJust.Support.Concurrent.Contracts {
	/// <summary>
	/// ������������� ���������
	/// </summary>
	/// <typeparam name="TKey">��� ��������� ���������</typeparam>
	public interface IItemsReleaser<in TKey> {

		/// <summary>
		/// ����������� ������� �� ���������� ������
		/// </summary>
		/// <param name="address"></param>
		void ReportSomeAddressedItemIsFree(TKey address);
	}
}