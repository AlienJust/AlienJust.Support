﻿using System;
using AlienJust.Support.Loggers.Contracts;
using AlienJust.Support.Text.Contracts;

namespace AlienJust.Support.Loggers {
	public sealed class RelayActionLogger : ILogger
	{
		private readonly Action<string> _relayLoggerAction;
		private readonly ITextFormatter _textFormatter;
		private readonly Action<string> _selectedLogAction;

		public RelayActionLogger(Action<string> relayLoggerAction)
		{
			_relayLoggerAction = relayLoggerAction;
			_textFormatter = null;
			_selectedLogAction = _relayLoggerAction == null ? (Action<string>)LogNothing : LogSimple;
		}

		public RelayActionLogger(Action<string> relayLoggerAction, ITextFormatter textFormatter)
		{
			_relayLoggerAction = relayLoggerAction;
			_textFormatter = textFormatter;
			_selectedLogAction = _relayLoggerAction == null ? LogNothing : _textFormatter == null ? (Action<string>)LogSimple : LogAdvanced;
		}

		public void Log(string text)
		{
			_selectedLogAction(text);
		}

		public void Log(object obj)
		{
			Log(obj.ToString());
		}


		private void LogNothing(string text)
		{
		}

		private void LogSimple(string text)
		{
			_relayLoggerAction(text);
		}

		private void LogAdvanced(string text)
		{
			_relayLoggerAction(_textFormatter.Format(text));
		}
	}
}