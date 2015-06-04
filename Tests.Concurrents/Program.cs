using System;
using System.Threading;
using AlienJust.Support.Concurrent;

namespace Tests.Concurrents
{
	class Program
	{
		static void Main(string[] args) {
			Console.WriteLine("Press any key to start");
			Console.ReadKey();
			var counter = new WaitableCounter();
			var starter = new SingleThreadPriorityAddressedAsyncStarterExceptionless<int>(100, 1, 5);
			Console.WriteLine("Async starter was created, press any key to begin test");
			Console.ReadKey();

			for (int k = 0; k < 5; ++k) {
				var producer = new Thread(() => {
					for (int i = 0; i < 100000; ++i) {
						for (int j = 0; j < 5; ++j) {
							int i1 = i;
							int j1 = j;
							counter.IncrementCount();
							starter.AddToQueueForExecution(complete => LongOperationAsync(i1, i2 => {
								Console.WriteLine("Async operation complete for arg = " + i1 + ", priority = " + j1 + ", result = " + i2);
								complete();
								counter.DecrementCount();
							}), j, i);
							Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " > Long operation added with arg = " + i + ", priority = " + j);
						}
					}
				});
				producer.Start();
			}

			Thread.Sleep(1000);
			counter.WaitForCounterChangeWhileNotPredecate(count => count == 0);
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
	}
}
