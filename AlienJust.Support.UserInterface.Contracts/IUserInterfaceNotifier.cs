using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlienJust.Support.UserInterface.Contracts
{
	public interface IUserInterfaceNotifier
	{
		void Notify(Action uiAction);
	}
}
