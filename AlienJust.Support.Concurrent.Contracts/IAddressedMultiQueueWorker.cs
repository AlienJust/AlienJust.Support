using System;

namespace AlienJust.Support.Concurrent.Contracts {
	/// <summary>
	/// ��������� ����������� �����������-�������� �������
	/// </summary>
	/// <typeparam name="TKey">��� ������ �������</typeparam>
	/// <typeparam name="TItem">��� �������� �������</typeparam>
	public interface IAddressedMultiQueueWorker<in TKey, in TItem>
	{
		/// <summary>
		/// ��������� ������� � ������� �� ������� ������ � � ������� �����������
		/// </summary>
		/// <param name="address">����� ��������</param>
		/// <param name="item">�������</param>
		/// <param name="queueNumber">����� ������ (������ ������� ���������� ����������)</param>
		/// <returns>���������� ������������� �������� � �������</returns>
		Guid AddWork(TKey address, TItem item, int queueNumber);

		/// <summary>
		/// ������� ������� �� �������
		/// </summary>
		/// <param name="id">���������� ������������� ��������</param>
		/// <returns>������, ���� �������� ���� �����������</returns>
		bool RemoveItem(Guid id);
	}
}