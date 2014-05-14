using System;
using System.Collections.Generic;
using System.Text;

namespace AlienJust.Support.Loggers.Contracts
{
	public interface ILogger
	{
		void Log(string text);
		void Log(object obj);
	}
}
