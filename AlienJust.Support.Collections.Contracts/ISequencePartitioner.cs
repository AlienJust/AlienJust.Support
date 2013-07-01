using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlienJust.Support.Collections.Contracts
{
	public interface ISequencePartitioner<out T>
	{
		IEnumerable<T> GetNextPart();
	}
}
