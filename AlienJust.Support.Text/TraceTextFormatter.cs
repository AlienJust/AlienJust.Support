using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using AlienJust.Support.Text.Contracts;

namespace AlienJust.Support.Text
{
	public class TraceTextFormatter : ITextFormatter
	{
		private readonly string _seporator;
		private readonly int _frameIndex;
		private readonly string _fileNameLimiter;


		public TraceTextFormatter(string seporator, int stackFrameOffset, string fileNameLimiter)
		{
			_seporator = seporator;
			_frameIndex = stackFrameOffset;
			_fileNameLimiter = fileNameLimiter;
		}


		public string Format(string text)
		{
			if (text == string.Empty) return text;

			var outStr = Thread.CurrentThread.ManagedThreadId + " > ";
			var stackTrace = new StackTrace(true);
			var stackDeep = stackTrace.FrameCount;
			for (int i = stackDeep - 1; i > _frameIndex; --i)
			{
				outStr += FrameToString(stackTrace.GetFrame(i), _seporator);
			}

			var lastFrame = stackTrace.GetFrame(_frameIndex);
			outStr += FrameToString(lastFrame, _seporator) + text + " (" + GetShortenFileName(lastFrame.GetFileName(), _fileNameLimiter) + ":" + lastFrame.GetFileLineNumber() + ")";

			return DateTime.Now.ToString("HH:mm:ss.fff") + _seporator + outStr;
		}


		private static string FrameToString(StackFrame frame, string seporator)
		{
			var result = string.Empty;
			var m = frame.GetMethod();
			var t = m.DeclaringType;
			if (t != null)
				result += t.Name + ".";
			result += m.Name + seporator;
			return result;
		}


		private static string GetShortenFileName(string fileName, string limiter)
		{
			try
			{
				var lastFoundLimiterPos = fileName.LastIndexOf(limiter);
				if (lastFoundLimiterPos < 0) return fileName;

				return fileName.Substring(lastFoundLimiterPos + limiter.Length);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return fileName;
		}
	}
}
