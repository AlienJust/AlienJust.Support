using System;
using System.Globalization;
using System.IO;
using System.Threading;
using AlienJust.Support.Collections;
using AlienJust.Support.Concurrent;
using AlienJust.Support.Loggers;
using AlienJust.Support.Loggers.Contracts;
using AlienJust.Support.Text;
using AlienJust.Support.Text.Contracts;

namespace TestApp {
	internal class Program {
		private static void Main(string[] args) {
			GlobalLogger.Setup(new RelayActionLogger(Console.WriteLine));

			long value = 7777_7777_7777_7777;
			Console.WriteLine(value.ToString("X16"));
			
			BytesOcta octa = BytesOcta.ToBcdLowFirst(value); // GENERATE HEX VALUE 0x7777 7777  7777 7777

			Console.WriteLine(octa);

			Console.WriteLine(octa.LowFirstBcd); // CONVERT HEX TO DEC
			//Console.WriteLine(octa.LowFirstBcd.ToString("X16"));
			//Console.WriteLine(octa.LowFirstSignedValue.ToString("X16"));
			//var array = new byte[] {20, 33, 0, 0, 0, 0, 0, 0};
			//MathExtensions.FillCrc16AtTheEndOfArrayHighLow(array);
			//GlobalLogger.Instance.Log(array.ToText());

			Console.ReadKey(true);
		}

		private static void TestFinallyCodeBlockOnThreadWorker() {
			_queueWorker = new SingleThreadedRelayQueueWorker<Action>("ololo", a => a(), ThreadPriority.Normal, true, ApartmentState.Unknown, new RelayLoggerWithStackTrace(new RelayActionLogger(Console.WriteLine), new StackTraceFormatterWithNullSuport(" > ", null)));
			_queueWorker.AddWork(() => { throw new Exception("oops"); });
			_queueWorker.AddWork(() => { throw new Exception("oops"); });
			_queueWorker.AddWork(() => { throw new Exception("oops"); });
		}

		private static void TestTextFormatter() {
			ITextFormatter f = new TraceTextFormatter(" > ", 1, Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture));
			Console.WriteLine(f.Format("Formatted text"));
		}

		private static SingleThreadedRelayQueueWorker<Action> _queueWorker;

		public static void SingleThreadRelayWorkerStressTest() {
			_queueWorker = new SingleThreadedRelayQueueWorker<Action>("axaxa", a => a(), ThreadPriority.Normal, true, null, new RelayLoggerWithStackTrace(new RelayActionLogger(Console.WriteLine), new StackTraceFormatterWithNullSuport(" > ", null)));
			Console.WriteLine("Worker created");
			var t1 = new Thread(ThreadStart) {IsBackground = true, Priority = ThreadPriority.BelowNormal};
			//var t2 = new Thread(ThreadStart) {IsBackground = true, Priority = ThreadPriority.Normal};
			var t3 = new Thread(ThreadStart) {IsBackground = true, Priority = ThreadPriority.Highest};

			t1.Start();
			//t2.Start();
			t3.Start();
			Console.WriteLine("All threads started");
		}

		private static void ThreadStart() {
			var threadId = Thread.CurrentThread.ManagedThreadId;
			Console.WriteLine("Thread " + threadId + " started, priority = " + Thread.CurrentThread.Priority);

			while (true) {
				_queueWorker.AddWork(() => Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " > Hello from thread " + threadId));

				Thread.Sleep(500 - Thread.CurrentThread.Priority.ToInt()*100);
			}
		}

		public static void TestSingleThreadedRelayMultiQueueWorker() {
			var worker = new SingleThreadedRelayMultiQueueWorker<int>("SingleThreadedRelayMultiQueueWorker", i => {
				Thread.Sleep(250);
				GlobalLogger.Instance.Log(" << Consumed item " + i);
			}, ThreadPriority.Normal, false, null, new RelayLoggerWithStackTrace(new RelayActionLogger(Console.WriteLine), new StackTraceFormatterWithNullSuport(" > ", null)), 2); // Две очереди
			
			var producerThread1 = new Thread(() => {
				for (int i = 0; i < 10; ++i) {
					Thread.Sleep(200); // each 200 ms nonpriority item added to queue
					GlobalLogger.Instance.Log(" > Produced background item: " + i);
					worker.AddWork(i, 1);
				}
			});
			var producerThread2 = new Thread(() => {
				for (int i = 1000; i < 1010; ++i) {
					Thread.Sleep(30);
					GlobalLogger.Instance.Log(" >> Produced prioritized item: " + i);
					worker.AddWork(i, 0);
				}
			});

			producerThread1.Start();
			Thread.Sleep(600);
			producerThread2.Start();

			Thread.Sleep(800);
			worker.StopAsync();
			worker.WaitStopComplete();
			Console.WriteLine("SingleThreadedRelayMultiQueueWorker stopped.............");
		}

		public static void TestSingleThreadedRelayMultiQueueWorkerExceptionless() {
			var worker = new SingleThreadedRelayMultiQueueWorkerExceptionless<int>("SingleThreadedRelayMultiQueueWorkerExceptionless", i => {
				Thread.Sleep(250);
				GlobalLogger.Instance.Log(" << ex Consumed item " + i);
			}, ThreadPriority.Normal, false, null, new RelayLoggerWithStackTrace(new RelayActionLogger(Console.WriteLine), new StackTraceFormatterWithNullSuport(" > ", null)), 2); // Две очереди

			var producerThread1 = new Thread(() => {
				for (int i = 0; i < 10; ++i) {
					Thread.Sleep(200); // each 200 ms nonpriority item added to queue
					GlobalLogger.Instance.Log(" > ex Produced background item: " + i);
					worker.AddWork(i, 1);
				}
			});
			var producerThread2 = new Thread(() => {
				for (int i = 1000; i < 1010; ++i) {
					Thread.Sleep(30);
					GlobalLogger.Instance.Log(" >> ex Produced prioritized item: " + i);
					worker.AddWork(i, 0);
				}
			});

			producerThread1.Start();
			Thread.Sleep(600);
			producerThread2.Start();

			Thread.Sleep(800);
			worker.StopAsync();
			worker.WaitStopComplete();
			Console.WriteLine("SingleThreadedRelayMultiQueueWorker stopped.............");
		}

		public static void TestSingleThreadedRelayAddressedMultiQueueWorker() {
			var starter = new SingleThreadedRelayAddressedMultiQueueWorker<int, int>("SingleThreadedRelayAddressedMultiQueueWorker", (i, itemsReleaser) => {
				Thread.Sleep(1000);
				Console.WriteLine("Consumed item " + i);
				itemsReleaser.ReportSomeAddressedItemIsFree(i);
			}, ThreadPriority.Normal, false, null, new RelayActionLogger(Console.WriteLine), 2, 1, 2);

			var producerThread1 = new Thread(() => {
				for (int i = 0; i < 10; ++i) {
					Thread.Sleep(200); // each 200 ms nonpriority item added to queue
					Console.WriteLine("Produced background item: " + i);
					starter.AddWork(i, i, 1);
					starter.AddWork(i, i, 1);
					starter.AddWork(i, i, 1);
				}
			});
			var producerThread2 = new Thread(() => {
				for (int i = 1000; i < 1010; ++i) {
					Thread.Sleep(30); // each 600 ms priority item added to queue
					Console.WriteLine("Produced pioritized item: " + i);
					starter.AddWork(i, i, 0);
					starter.AddWork(i, i, 0);
					starter.AddWork(i, i, 0);
					starter.AddWork(i, i, 0);
				}
			});

			producerThread1.Start();
			Thread.Sleep(1000);
			producerThread2.Start();
		}

		public static void TestSingleThreadedRelayAddressedMultiQueueWorkerExceptionless() {
			var starter = new SingleThreadedRelayAddressedMultiQueueWorkerExceptionless<int, int>(
				"SingleThreadedRelayAddressedMultiQueueWorkerExceptionless", 
				(i, itemsReleaser) => {
					Thread.Sleep(1000);
					Console.WriteLine("Consumed item " + i);
					itemsReleaser.ReportSomeAddressedItemIsFree(i);
				}, 
				ThreadPriority.Normal, 
				false, 
				null, 
				new RelayActionLogger(Console.WriteLine),
				2,
				1, 
				2);
			var producerThread1 = new Thread(() => {
				for (int i = 0; i < 10; ++i) {
					Thread.Sleep(200); // each 200 ms nonpriority item added to queue
					Console.WriteLine("Produced background item: " + i);
					starter.AddWork(i, i, 1);
					starter.AddWork(i, i, 1);
					starter.AddWork(i, i, 1);
				}
			});
			var producerThread2 = new Thread(() => {
				for (int i = 1000; i < 1010; ++i) {
					Thread.Sleep(30); // each 600 ms priority item added to queue
					Console.WriteLine("Produced pioritized item: " + i);
					starter.AddWork(i, i, 0);
					starter.AddWork(i, i, 0);
					starter.AddWork(i, i, 0);
					starter.AddWork(i, i, 0);
				}
			});

			producerThread1.Start();
			Thread.Sleep(1000);
			producerThread2.Start();
		}

		public static void TestMultiQueueStarter() {
			var starter = new SingleThreadPriorityAsyncStarter("TestMultiQueueStarter", ThreadPriority.Normal, false, null, new RelayLoggerWithStackTrace(new RelayActionLogger(Console.WriteLine), new StackTraceFormatterWithNullSuport(" > ", null)), 2, 2, true);
			for (int i = 0; i < 10; ++i) {
				GlobalLogger.Instance.Log("Starting async action... i = " + i);
				int i1 = i;
				starter.AddWork(() => AsyncAction(i1, () => {
					GlobalLogger.Instance.Log("Callback ok for i = " + i1);
					starter.NotifyStarterAboutQueuedOperationComplete();
				}), 1);
			}
		}

		public static void TestMultiQueueAddrStarter() {
			var random = new Random();
			const int totalMaxFlow = 5;
			const int perAddressFlow = 1;
			int priorityGraduation = Colors.Length;
			var starter = new SingleThreadPriorityAddressedAsyncStarter<int>("SingleThreadPriorityAddressedAsyncStarter", ThreadPriority.Normal,
				false,
				null,
				new RelayActionLogger(Console.WriteLine), totalMaxFlow, perAddressFlow, priorityGraduation, true);
			for (int i = 0; i < Colors.Length; ++i) {
				for (int j = 0; j < 3; ++j) {
					Thread.Sleep(random.Next(50, 100));
					int priority = Colors.Length - i - 1;
					ConsoleWriteLineColored(DateTime.Now.ToString("HH:mm:ss.fff") + " > Adding async action " + i + "." + j + " to queue, priority=" + priority, Colors[i]);
					int i1 = i;
					int j1 = j;
					starter.AddWork(notifyAboutAsyncOperationComplete => {
						var t = new Thread(
							() => {
								try {
									ConsoleWriteLineColored(DateTime.Now.ToString("HH:mm:ss.fff") + " > Async action i=" + i1 + "." + j1 + " in progress", Colors[i1]);
									Thread.Sleep(5000);
									ConsoleWriteLineColored(DateTime.Now.ToString("HH:mm:ss.fff") + " > Async action i=" + i1 + "." + j1 + " completed", Colors[i1]);
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

		private static void AsyncAction(int actNumber, Action completeCallback) {
			var t = new Thread(
				() => {
					Console.WriteLine("Async action i=" + actNumber + " in progress");
					Thread.Sleep(2000 - actNumber*10);
					completeCallback.Invoke();
				}) {IsBackground = true};
			t.Start();
		}

		private static readonly ConsoleColor[] Colors = {ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Magenta, ConsoleColor.White};
		private static readonly object SyncConsole = new object();

		private static void ConsoleWriteLineColored(string text, ConsoleColor color) {
			lock (SyncConsole) {
				var wasColor = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.WriteLine(text);
				Console.ForegroundColor = wasColor;
			}
		}

		private static void Log(string text, ConsoleColor color) {
			lock (SyncConsole) {
				var wasColor = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " > " + text);
				Console.ForegroundColor = wasColor;
			}
		}
	}

	internal class HelloWorld {
		private static readonly ILogger Logger = new RelayLogger(null);

		public void TestLogger() {
			Logger.Log("Hello");
			Logger.Log("world");
		}
	}

	internal static class Ext {
		public static int ToInt(this ThreadPriority priority) {
			switch (priority) {
				case ThreadPriority.Lowest:
					return 0;
				case ThreadPriority.BelowNormal:
					return 1;
				case ThreadPriority.Normal:
					return 2;
				case ThreadPriority.AboveNormal:
					return 3;
				case ThreadPriority.Highest:
					return 4;
				default:
					throw new Exception("Unknown thread priority, cannot convert it to " + typeof (int).FullName);
			}
		}
	}
}
