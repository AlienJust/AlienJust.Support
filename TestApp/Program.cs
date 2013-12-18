﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AlienJust.Support.Concurrent;
using AlienJust.Support.Concurrent.Contracts;
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
			//TestTextFormatter();
			//GlobalLogger.Instance.Log(System.IO.Path.GetFullPath("C:\\\\Games"));
			//GlobalLogger.Instance.Log(System.IO.Path.GetDirectoryName("C:\\\\Games"));
			//new HelloWorld().TestLogger();
			//TestMultiQueueWorker();
			//TestMultiQueueStarter();
			//TestAddressedMultiQueueWorker();
			TestMultiQueueAddrStarter();
			
			//SingleThreadRelayWorkerStressTest();
			//Thread.Sleep(9000000); // 9000 seconds, it is about 150 minutes
			
			//TestAsyncWorkers();
			Console.ReadKey(true);
		}

		private static void TestTextFormatter()
		{
			ITextFormatter f = new TraceTextFormatter(" > ", 1, Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture));
			Console.WriteLine(f.Format("Formatted text"));
		}

		private static SingleThreadedRelayQueueWorker<Action> _worker;
		public static void SingleThreadRelayWorkerStressTest() {
			_worker = new SingleThreadedRelayQueueWorker<Action>(a => a(), ThreadPriority.Normal, true, null);
			
			var t1 = new Thread(ThreadStart) { IsBackground = true, Priority = ThreadPriority.BelowNormal};
			var t2 = new Thread(ThreadStart) { IsBackground = true };
			var t3 = new Thread(ThreadStart) { IsBackground = true };

			t1.Start();
			t2.Start();
			t3.Start();
		}

		private static void ThreadStart() {
			var threadId = Thread.CurrentThread.ManagedThreadId;
			Console.WriteLine("Thread " + threadId + " started");
			
			while (true) {
				_worker.AddToExecutionQueue(() => Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " > Hello from thread " + threadId));
				//Thread.Sleep(1);
			}
		}

		public static void TestMultiQueueWorker()
		{
			var starter = new SingleThreadedRelayMultiQueueWorker<int>(i =>
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

		public static void TestAddressedMultiQueueWorker()
		{
			var starter = new SingleThreadedRelayAddressedMultiQueueWorker<int, int>((i, itemsReleaser) =>
			                                                                         	{
			                                                                         		Thread.Sleep(1000);
																													Console.WriteLine("Consumed item " + i);
			                                                                         		itemsReleaser.ReportSomeAddressedItemIsFree(i);
			                                                                         	},
			                                                                         2,
			                                                                         1, 2);
			var producerThread1 = new Thread(() =>
			{
				for (int i = 0; i < 10; ++i)
				{
					Thread.Sleep(200); // each 200 ms nonpriority item added to queue
					Console.WriteLine("Produced background item: " + i);
					starter.AddToExecutionQueue(i, i, 1);
					starter.AddToExecutionQueue(i, i, 1);
					starter.AddToExecutionQueue(i, i, 1);
				}
			});
			var producerThread2 = new Thread(() =>
			{
				for (int i = 1000; i < 1010; ++i)
				{
					Thread.Sleep(30); // each 600 ms priority item added to queue
					Console.WriteLine("Produced pioritized item: " + i);
					starter.AddToExecutionQueue(i, i, 0);
					starter.AddToExecutionQueue(i, i, 0);
					starter.AddToExecutionQueue(i, i, 0);
					starter.AddToExecutionQueue(i, i, 0);
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
			var random = new Random();
			const int totalMaxFlow = 5;
			const int perAddressFlow = 1;
			int priorityGraduation = _colors.Length;
			var starter = new SingleThreadPriorityAddressedAsyncStarter<int>(totalMaxFlow, perAddressFlow, priorityGraduation);
			for (int i = 0; i < _colors.Length; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					Thread.Sleep(random.Next(50, 100));
					int priority = _colors.Length - i - 1;
					ConsoleWriteLineColored(DateTime.Now.ToString("HH:mm:ss.fff") + " > Adding async action " + i + "." + j + " to queue, priority=" + priority, _colors[i]);
					int i1 = i;
					int j1 = j;
					starter.AddToQueueForExecution(notifyAboutAsyncOperationComplete => {
					                               	var t = new Thread(
					                               		() => {
					                               			try {
																			ConsoleWriteLineColored(DateTime.Now.ToString("HH:mm:ss.fff") + " > Async action i=" + i1 + "." + j1 + " in progress", _colors[i1]);
					                               				Thread.Sleep(5000);
																			ConsoleWriteLineColored(DateTime.Now.ToString("HH:mm:ss.fff") + " > Async action i=" + i1 + "." + j1 + " completed", _colors[i1]);
					                               			}
					                               			finally {
					                               				notifyAboutAsyncOperationComplete();
					                               			}
					                               		}) {IsBackground = true};
					                               	t.Start();
					}, priority, i1);
				}
			}
		}

		static void AsyncAction(int actNumber, Action completeCallback) {
			var t = new Thread(
				() => {
					Console.WriteLine("Async action i=" + actNumber + " in progress");
					Thread.Sleep(2000 - actNumber*10);
					completeCallback.Invoke();
				}) {IsBackground = true};
			t.Start();
		}

		static readonly IWorkerFactory WorkersFactory = new AsyncMemoriedWorkerFactory();
		static void TestAsyncWorkers() {
			Console.WriteLine("Current thread id = " + Thread.CurrentThread.ManagedThreadId);
			
			var worker = WorkersFactory.GetSimpleWorker(
				progressHandler => {
					for (int i = 0; i < 10; ++i) {
						Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " > long background task...");
						Thread.Sleep(1000);
						progressHandler.NotifyProgrssChanged(i + 1 * 10);
					}
					//progressHandler.NotifyProgrssChanged(100);
				},
				progressPercent => Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " > Progress " + progressPercent),
				ex => Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " > all task complete")
				);
			worker.Run();
		}

		private static ConsoleColor[] _colors = new ConsoleColor[] {ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Magenta, ConsoleColor.White};
		private static readonly object SyncConsole = new object();
		static void ConsoleWriteLineColored(string text, ConsoleColor color) {
			lock (SyncConsole) {
				var wasColor = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.WriteLine(text);
				Console.ForegroundColor = wasColor;
			}
		}
	}

	class HelloWorld
	{
		static readonly ILogger Logger = new RelayLogger(null);

		public void TestLogger()
		{
			Logger.Log("Hello");
			Logger.Log("world");
		}
	}

	
}
