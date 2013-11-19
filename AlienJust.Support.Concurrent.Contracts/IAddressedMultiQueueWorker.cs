using System;

namespace AlienJust.Support.Concurrent.Contracts {
	public interface IAddressedMultiQueueWorker<in TKey, in TItem>
	{
		Guid AddToExecutionQueue(TKey address, TItem item, int queueNumber);
		void ReportItemIsFree(TKey address);
		bool RemoveItem(Guid id);
	}
}