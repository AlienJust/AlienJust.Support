namespace AlienJust.Support.Concurrent.Contracts
{
	public interface IMultiQueueWorker<in TItem>
	{
		void AddToExecutionQueue(TItem item, int queueNumber);
		void ClearQueue();
	}
}