using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlienJust.Support.Concurrent.Contracts
{
	public interface IThreadNotifier {
		void Notify(Action notifyAction);
	}
}
