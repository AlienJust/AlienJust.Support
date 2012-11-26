using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Loggers
{
	public sealed class RelayLogger : ILogger
	{
		private readonly ILogger _relayLogger;
		public RelayLogger(ILogger relayLogger)
		{
			_relayLogger = relayLogger;
		}
		public void Log(string text)
		{
			if (_relayLogger != null) _relayLogger.Log(text);
		}
	}
}
