using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AlienJust.Support.Loggers.Contracts;

namespace AlienJust.Support.Loggers
{
	public class ConsoleLogger : ILogger
	{
		private readonly int _stackFrame;
		public ConsoleLogger(int stackFrame)
		{
			_stackFrame = stackFrame;
		}
		public void Log(string text)
		{
			LogToConsole(text, _stackFrame);
		}

		private static void LogToConsole(string text, int stackFrameNumber)
		{
			var stackTrace = new System.Diagnostics.StackTrace();
			var frame = stackTrace.GetFrame(stackFrameNumber);
			string preffix = frame.GetMethod().DeclaringType.Name + "." + frame.GetMethod().Name;
			if (!string.IsNullOrEmpty(text))
				Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " > " + Thread.CurrentThread.ManagedThreadId + " > " + preffix + " > " + text);
			else
				Console.WriteLine();
		}

		public static void LogText(string text)
		{
			LogToConsole(text, 1);
		}
	}
}
