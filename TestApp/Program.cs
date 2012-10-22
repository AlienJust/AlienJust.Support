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
		}
	}
}
