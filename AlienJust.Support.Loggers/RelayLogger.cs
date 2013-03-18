using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienJust.Support.Loggers.Contracts;
using AlienJust.Support.Text.Contracts;

namespace AlienJust.Support.Loggers {
	public sealed class RelayLogger : ILogger {
		private readonly ILogger _relayLogger;
		private readonly ITextFormatter _textFormatter;
		private readonly Action<string> _logAction;

		public RelayLogger(ILogger relayLogger) {
			_relayLogger = relayLogger;
			_textFormatter = null;
			_logAction = _relayLogger == null ? (Action<string>)LogNothing : LogSimple;
		}

		public RelayLogger(ILogger relayLogger, ITextFormatter textFormatter) {
			_relayLogger = relayLogger;
			_textFormatter = textFormatter;
			_logAction = _relayLogger == null ? LogNothing : _textFormatter == null ? (Action<string>) LogSimple : LogAdvanced;
		}

		public void Log(string text) {
			_logAction(text);
		}

		
		private void LogNothing(string text) {
		}

		private void LogSimple(string text) {
			_relayLogger.Log(text);
		}

		private void LogAdvanced(string text) {
			_relayLogger.Log(_textFormatter.Format(text));
		}
	}
}
