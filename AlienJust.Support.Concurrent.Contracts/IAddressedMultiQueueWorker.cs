using System;

namespace AlienJust.Support.Concurrent.Contracts {
	/// <summary>
	/// ��������� ����������� �����������-�������� �������
	/// </summary>
	/// <typeparam name="TKey">��� ������ �������</typeparam>
	/// <typeparam name="TItem">��� �������� �������</typeparam>
	public interface IAddressedMultiQueueWorker<in TKey, in TItem> : IBackThreaded
	{
		/// <summary>
		/// ��������� ������� � ������� �� ������� ������ � � ������� �����������
		/// </summary>
		/// <param name="address">����� ��������</param>
		/// <param name="item">�������</param>
		/// <param name="queueNumber">����� ������ (������ ������� ���������� ����������)</param>
		/// <returns>���������� ������������� �������� � �������</returns>
		Guid AddToExecutionQueue(TKey address, TItem item, int queueNumber);

		/// <summary>
		/// ������� ������� �� �������
		/// </summary>
		/// <param name="id">���������� ������������� ��������</param>
		/// <returns>������, ���� �������� ���� �����������</returns>
		bool RemoveItem(Guid id);

		
	}

	/// <summary>
	/// ������, ��������� ������� ������� (��������, �� �����)
	/// </summary>
	public interface IBackThreaded {
		
		/// <summary>
		/// ����� ��� ������ ������� �����������
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// ��������� ������ ������ �������
		/// </summary>
		void StopWorker();
	}
}