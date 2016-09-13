using System;
using System.Threading;
using AlienJust.Support.Concurrent;
using AlienJust.Support.Loggers;

namespace Tests.Concurrents
{
	class Program
	{
		static void Main(string[] args) {
			TestStarter();

			//counter.WaitForCounterChangeWhileNotPredecate(count => count == 0);
			Console.WriteLine("--------------------- Tests complete ----------------------");
		}

		private static void TestAddrStarter() {
			Console.WriteLine("Press any key to start");
			Console.ReadKey();
			var counter = new WaitableCounter();
			var starter = new SingleThreadPriorityAddressedAsyncStarterExceptionless<int>("SingleThreadPriorityAddressedAsyncStarterExceptionless", ThreadPriority.Normal, false, null, new RelayActionLogger(Console.WriteLine), 100, 1, 5, false);
			Console.WriteLine("Async starter was created, press any key to begin test");
			Console.ReadKey();

			for (int k = 0; k < 5; ++k) {
				var producer = new Thread(() => {
					for (int i = 0; i < 100000; ++i) {
						try {
							for (int j = 0; j < 5; ++j) {
								int i1 = i;
								int j1 = j;
								counter.IncrementCount();
								starter.AddWork(complete => LongOperationAsync(i1, i2 => {
									Console.WriteLine("Async operation complete for arg = " + i1 + ", priority = " + j1 + ", result = " + i2);
									complete();
									counter.DecrementCount();
								}), j, i);
								Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " > Long operation added with arg = " + i + ", priority = " + j);
							}
						}
						catch (Exception ex) {
							Console.WriteLine(ex);
							break;
						}
					}
				}) {Name = "producer #" + k, IsBackground = true};
				producer.Start();
			}

			Thread.Sleep(2000);
			Console.WriteLine("Stopping starter             ,,,,,,,,,,,,");
			starter.StopAsync();
			starter.WaitStopComplete();
			Console.WriteLine("Starter stopped              ............");
		}


		private static void TestStarter() {
			Console.WriteLine("Press any key to start");
			Console.ReadKey();
			var counter = new WaitableCounter();
			var starter = new SingleThreadPriorityAsyncStarter("SingleThreadPriorityAsyncStarter", ThreadPriority.Normal, false, null, new RelayLoggerWithStackTrace(new RelayActionLogger(Console.WriteLine), new StackTraceFormatterWithNullSuport(" > ", null)), 5, 5, true);
			Console.WriteLine("Async starter was created, press any key to begin test");
			Console.ReadKey();

			for (int k = 0; k < 5; ++k) {
				var producer = new Thread(() => {
					for (int i = 0; i < 100000; ++i) {
						try {
							for (int j = 0; j < 5; ++j) {
								int i1 = i;
								int j1 = j;
								counter.IncrementCount();
								starter.AddWork(() => LongOperation2Async(() => {
									counter.DecrementCount();
									starter.NotifyStarterAboutQueuedOperationComplete();
								}), j1);
								Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " > Long operation added with arg = " + i + ", priority = " + j);
							}
						}
						catch (Exception ex) {
							Console.WriteLine(ex);
							break;
						}
					}
				}) { Name = "producer #" + k, IsBackground = true };
				producer.Start();
			}

			Thread.Sleep(2000);
			Console.WriteLine("Stopping starter             ,,,,,,,,,,,,");
			starter.StopAsync();
			starter.WaitStopComplete();
			Console.WriteLine("Starter stopped              ............");
		}


		static int LongOperation(int arg) {
			Thread.Sleep(100);
			var result = arg;
			return result;
		}


		static void LongOperationAsync(int arg, Action<int> callback) {
			var thread = new Thread(() => callback(LongOperation(arg)));
			thread.Start();
		}


		static void LongOperation2() {
			Thread.Sleep(100);
		}


		static void LongOperation2Async(Action callback) {
			var thread = new Thread(() => {
				LongOperation2();
				callback();
			});
			thread.Start();
		}
	}
}
