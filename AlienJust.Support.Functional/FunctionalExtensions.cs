using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlienJust.Support.Functional
{
	public static class FunctionalExtensions
	{
		public static void CheckCondition(this Func<bool> condition, Action onTrue, Action onFalse)
		{
			if (condition())
				onTrue();
			else 
				onFalse();
		}
	}
}
