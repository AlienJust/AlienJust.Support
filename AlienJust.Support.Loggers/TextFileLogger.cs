using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Loggers
{
	public class SimpleLogger : ILogger
	{
		private readonly string _logFileName = string.Empty;
		private bool _append = false;
		private readonly int _frameIndex = 1;
		public SimpleLogger(string path)
		{
			_logFileName = path;
		}
		public SimpleLogger(string path, bool append)
		{
			_logFileName = path;
			_append = append;
		}
		public SimpleLogger(string path, bool append, int frameIndex)
		{
			_logFileName = path;
			_append = append;
			_frameIndex = frameIndex;
		}

		public void Log(string messageText)
		{
			try
			{
				using (var sw = new StreamWriter(_logFileName, _append))
				{
					var stackTrace = new System.Diagnostics.StackTrace();
					var frame = stackTrace.GetFrame(_frameIndex);
					string preffix = Thread.CurrentThread.ManagedThreadId + " > " + frame.GetMethod().DeclaringType.Name + "." + frame.GetMethod().Name;

					if (!string.IsNullOrEmpty(messageText))
						sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " > " + preffix + " > " + messageText);
					else
						sw.WriteLine();

					sw.Close();
				}
				if (!_append)
				{
					_append = true;
				}
			}
			catch
			{
			}
		}
	}
}
