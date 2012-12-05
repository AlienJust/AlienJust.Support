using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AlienJust.Support.Concurrent;
using AlienJust.Support.Loggers;
using AlienJust.Support.Loggers.Contracts;
using AlienJust.Support.Text;
using AlienJust.Support.Text.Contracts;

namespace TestApp
{
	class Program
	{
		
		static void Main(string[] args)
		{
			GlobalLogger.Setup(new ConsoleLogger(2));
			//GlobalLogger.Instance.Log("Hello world!");
			//GlobalLogger.Instance.Log(Path.PathSeparator.ToString(CultureInfo.InvariantCulture));
			//TestTextFormatter();
			//GlobalLogger.Instance.Log(System.IO.Path.GetFullPath("C:\\\\Games"));
			//GlobalLogger.Instance.Log(System.IO.Path.GetDirectoryName("C:\\\\Games"));
			//new HelloWorld().TestLogger();
			//TestMultiQueueWorker();
			//TestMultiQueueStarter();
			TestMultiQueueAddrStarter();
			Console.ReadKey(true);
		}

		private static void TestTextFormatter()
		{
			ITextFormatter f = new TraceTextFormatter(" > ", 1, Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture));
			Console.WriteLine(f.Format("Formatted text"));
		}


		public static void TestMultiQueueWorker()
		{
			var starter = new SingleThreadedRelayMultyQueueWorker<int>(i =>
			                                                           	{
			                                                           		Thread.Sleep(300);
																																		GlobalLogger.Instance.Log("Consumed item " + i); }
																																		, 2); // Две очереди
			var producerThread1 = new Thread(() =>
			                                	{
																					for(int i = 0; i < 10; ++i)
																					{
																						Thread.Sleep(200); // each 200 ms nonpriority item added to queue
																						GlobalLogger.Instance.Log("Produced background item: " + i);
																						starter.AddToExecutionQueue(i, 1);
																					}
			                                	});
			var producerThread2 = new Thread(() =>
			                                 	{
																					for (int i = 1000; i < 1010; ++i)
																					{
																						Thread.Sleep(30); // each 600 ms priority item added to queue
																						GlobalLogger.Instance.Log("Produced pioritized item: " + i);
																						starter.AddToExecutionQueue(i, 0);
																					}
			                                 	});
			
			producerThread1.Start();
			Thread.Sleep(1000);
			producerThread2.Start();
		}

		public static void TestMultiQueueStarter()
		{
			var starter = new SingleThreadPriorityAsyncStarter(2, 2);
			for (int i = 0; i < 10; ++i)
			{
				GlobalLogger.Instance.Log("Starting async action... i = " + i);
				int i1 = i;
				starter.AddToQueueForExecution(() => AsyncAction(i1, () =>
				                                                 	{
				                                                 		GlobalLogger.Instance.Log("Callback ok for i = " + i1);
				                                                 		starter.NotifyStarterAboutQueuedOperationComplete();
				                                                 	}), 1);
			}
		}

		public static void TestMultiQueueAddrStarter()
		{
			var starter = new SingleThreadPriorityAddressedAsyncStarter<int>(5, 2);
			for (int i = 0; i < 10; ++i)
			{
				for (int j = 0; j < 2; ++j)
				{
					GlobalLogger.Instance.Log("Starting async action... i = " + i);
					int i1 = i;
					starter.AddToQueueForExecution(() => AsyncAction(i1, () =>
					                                                 	{
					                                                 		GlobalLogger.Instance.Log("Callback ok for i = " + i1);
					                                                 		starter.NotifyStarterAboutQueuedOperationComplete(i1);
					                                                 	}), 1, i1);
				}
			}
		}

		static void AsyncAction(int actNumber, Action completeCallback)
		{
			var t = new Thread(() =>
			                   	{
														GlobalLogger.Instance.Log("Async action i=" + actNumber +" in progress");
			                   		Thread.Sleep(1000);
			                   		completeCallback.Invoke();
			                   	}) {IsBackground = true};
			t.Start();
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
