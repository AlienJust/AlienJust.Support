namespace AlienJust.Support.Concurrent.Contracts {
	public interface IWorker<in TItem> : IStoppable
	{
		void AddWork(TItem workItem);
	}

	public interface IStoppable {
		void StopSynchronously();
	}
}