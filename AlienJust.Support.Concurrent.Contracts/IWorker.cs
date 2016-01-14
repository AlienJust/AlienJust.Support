namespace AlienJust.Support.Concurrent.Contracts {
	public interface IWorker<in TItem>
	{
		void AddWork(TItem workItem);
	}

	public interface IStoppableWorker<in TItem> : IWorker<TItem> {
		void AddLastWork(TItem workItem);
		void Stop();
	}
}