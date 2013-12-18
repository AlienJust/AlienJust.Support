namespace AlienJust.Support.Concurrent.Contracts {
	public interface IQueueWorker<in TItem>
	{
		void AddToExecutionQueue(TItem item);
	}
}