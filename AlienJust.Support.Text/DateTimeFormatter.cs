using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AlienJust.Support.Text.Contracts;

namespace AlienJust.Support.Text {
	public class DateTimeFormatter : ITextFormatter {
		private readonly string _seporator;

		public DateTimeFormatter(string seporator) {
			_seporator = seporator;
		}

		public string Format(string text) {
			if (text == string.Empty) return text;
			return DateTime.Now.ToString("HH:mm:ss.fff") + _seporator + text;
		}
	}

	public class ChainedFormatter : ITextFormatter {
		private readonly IEnumerable<ITextFormatter> _formatters;
		public ChainedFormatter(IEnumerable<ITextFormatter> formatters)
		{
			_formatters = formatters;
		}
		public string Format(string text) {
			return _formatters.Aggregate(text, (current, f) => f.Format(current));
		}
	}
}
