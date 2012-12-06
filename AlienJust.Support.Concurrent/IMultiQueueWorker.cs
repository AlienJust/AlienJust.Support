namespace AlienJust.Support.Concurrent
{
	public interface IMultiQueueWorker<in TItem>
	{
		void AddToExecutionQueue(TItem item, int queueNumber);
	}

	public interface IAddressedMultiQueueWorker<in TKey, in TItem>
	{
		void AddToExecutionQueue(TKey address, TItem item, int queueNumber);
		void ReportItemIsFree(TKey address);
	}
}