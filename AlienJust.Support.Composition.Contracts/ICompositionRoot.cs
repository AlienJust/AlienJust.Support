using System.Collections.Generic;

namespace AlienJust.Support.Composition.Contracts
{
	public interface ICompositionRoot
	{
		IList<ICompositionPart> Compositions { get; }
	}
}
