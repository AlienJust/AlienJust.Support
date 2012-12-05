namespace AlienJust.Support.Concurrent
{
	public interface IMultiQueueWorker<in TItem>
	{
		void AddToExecutionQueue(TItem item, int queueNumber);
	}

	public interface IAddressedMultiQueueWorker<in TItem>
	{
		void AddToExecutionQueue(int address, TItem item, int queueNumber);
	}
}