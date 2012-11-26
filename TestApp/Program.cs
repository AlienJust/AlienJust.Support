using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienJust.Support.Loggers;
using AlienJust.Support.Loggers.Contracts;

namespace TestApp
{
	class Program
	{
		
		static void Main(string[] args)
		{
			GlobalLogger.Setup(new ConsoleLogger(1));
			GlobalLogger.Instance.Log("Hello world!");

			//GlobalLogger.Instance.Log(System.IO.Path.GetFullPath("C:\\\\Games"));
			//GlobalLogger.Instance.Log(System.IO.Path.GetDirectoryName("C:\\\\Games"));
			new HelloWorld().TestLogger();
		}
		
	}

	class HelloWorld
	{
		static readonly ILogger Logger = new SimpleLogger("123.txt", true, "\\");

		public void TestLogger()
		{
			Logger.Log("Hello");
			Logger.Log("world");
		}
	}
}
