namespace AlienJust.Support.Composition.Contracts {
	public interface ICompositionPart : IUnknownLike {
		string Name { get; }
		void SetCompositionRoot(ICompositionRoot root);
	}

	public interface IUnknownLike {
		void Release();
		void AddRef();
		int RefsCount { get; }
	}
}