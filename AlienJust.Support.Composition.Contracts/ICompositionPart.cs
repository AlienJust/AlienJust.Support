namespace AlienJust.Support.Composition.Contracts {
	public interface ICompositionPart {
		string Name { get; }
		void SetCompositionRoot(ICompositionRoot root);
	}
}