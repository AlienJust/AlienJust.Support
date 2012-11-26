using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Loggers
{
	public sealed class SimpleLogger : ILogger
	{
		public const string Seporator = " > ";
		public int FrameIndex { get; private set; }

		private readonly object _sync = new object();

		private readonly string _logFileName = string.Empty;
		private bool _append;
		private readonly string _fileNameLimiter;

		public SimpleLogger(string path, string fileNameLimiter)
		{
			_logFileName = path;
			_append = false;
			_fileNameLimiter = fileNameLimiter;
			FrameIndex = 1;
		}


		public SimpleLogger(string path, bool append, string fileNameLimiter)
		{
			_logFileName = path;
			_append = append;
			_fileNameLimiter = fileNameLimiter;
			FrameIndex = 1;
		}


		public SimpleLogger(string path, bool append, string fileNameLimiter, int frameIndex)
		{
			_logFileName = path;
			_append = append;
			_fileNameLimiter = fileNameLimiter;
			FrameIndex = frameIndex;
			
		}


		public void Log(string messageText)
		{
			lock (_sync)
			{
				using (var sw = new StreamWriter(_logFileName, _append))
				{
					var outStr = Thread.CurrentThread.ManagedThreadId + " > ";
					var stackTrace = new StackTrace(true);
					var stackDeep = stackTrace.FrameCount;
					for (int i = stackDeep - 1; i > FrameIndex; --i)
					{
						outStr += FrameToString(stackTrace.GetFrame(i));
					}

					var lastFrame = stackTrace.GetFrame(FrameIndex);
					outStr += FrameToString(lastFrame) + messageText + " (" + GetShortenFileName(lastFrame.GetFileName(), _fileNameLimiter) + ":" + lastFrame.GetFileLineNumber() + ")";

					if (!string.IsNullOrEmpty(messageText))
						sw.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " > " + outStr);
					else
						sw.WriteLine();

					sw.Close();
				}
				if (!_append)
				{
					_append = true;
				}
			}
		}


		private static string FrameToString(StackFrame frame)
		{
			var result = string.Empty;
			var m = frame.GetMethod();
			var t = m.DeclaringType;
			if (t != null)
				result += t.Name + ".";
			result += m.Name + Seporator;
			return result;
		}


		private static string GetShortenFileName(string fileName, string limiter)
		{
			/*var lastFoundLimiterPos = fileName.LastIndexOf(limiter, StringComparison.Ordinal);
			if (lastFoundLimiterPos < 0) return fileName;

			return fileName.Substring(lastFoundLimiterPos + limiter.Length);
			 */
			return fileName;
		}
	}
}
