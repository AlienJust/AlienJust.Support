using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienJust.Support.Loggers;

namespace TestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			GlobalLogger.Setup(new ConsoleLogger(1));
			GlobalLogger.Instance.Log("Hello world!");

			GlobalLogger.Instance.Log(System.IO.Path.GetFullPath("C:\\\\Games"));
			GlobalLogger.Instance.Log(System.IO.Path.GetDirectoryName("C:\\\\Games"));
		}
	}
}
