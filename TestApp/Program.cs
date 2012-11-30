using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AlienJust.Support.Concurrent;
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
			TestMultiQueueStarter();
			Console.ReadKey(true);
		}
		

		public static void TestMultiQueueStarter()
		{
			var starter = new SingleThreadedRelayMultyQueueWorker<int>(i =>
			                                                           	{
			                                                           		Thread.Sleep(300);
																																		Console.WriteLine("Consumed item " + i); }
																																		, 2); // Две очереди
			var producerThread1 = new Thread(() =>
			                                	{
																					for(int i = 0; i < 20; ++i)
																					{
																						Thread.Sleep(200); // each 200 ms nonpriority item added to queue
																						Console.WriteLine("Produced background item: " + i);
																						starter.AddToExecutionQueue(i, 1);
																					}
			                                	});
			var producerThread2 = new Thread(() =>
			                                 	{
																					for (int i = 1000; i < 1020; ++i)
																					{
																						Thread.Sleep(30); // each 600 ms priority item added to queue
																						Console.WriteLine("Produced pioritized item: " + i);
																						starter.AddToExecutionQueue(i, 0);
																					}
			                                 	});
			
			producerThread1.Start();
			Thread.Sleep(3000);
			producerThread2.Start();
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
