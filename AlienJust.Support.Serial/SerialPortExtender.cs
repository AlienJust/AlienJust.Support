using System;
using System.Diagnostics;
using System.Threading;
using System.IO.Ports;
using AlienJust.Support.Text;

namespace AlienJust.Support.Serial
{
	public sealed class SerialPortExtender {
		private readonly SerialPort _port;
		private readonly Action<string> _selectedLogAction;
		private readonly Stopwatch _readEplasedTimer = new Stopwatch();

		public SerialPortExtender(SerialPort port) {
			_port = port;
			_selectedLogAction = s => { };
		}

		public SerialPortExtender(SerialPort port, Action<string> logAction) {
			if (logAction == null) throw new NullReferenceException(".ctor parameter logAction cannot be null");
			_port = port;
			_selectedLogAction = logAction;
		}

		public void WriteBytes(byte[] bytes, int offset, int count) {
			Log("Удаление всех данных исходящего буфера последовательного порта...");
			_port.DiscardOutBuffer();
			Log("Очистка уже принятых байтов...");
			var discardedInBytes = ReadAllBytes();
			Log("Удалены следующие байты: " + discardedInBytes.ToText());
			_port.Write(bytes, 0, bytes.Length);
		}

		public byte[] ReadBytes(int bytesCount, int timeoutInSeconds) {
			var inBytes = new byte[bytesCount];
			int totalReadedBytesCount = 0;

			const int iterationsPerSecondCount = 20;
			TimeSpan maximumMillsecondsCountToWaitAfterEachIteration = TimeSpan.FromMilliseconds(1000.0/iterationsPerSecondCount);
			Log("Iteration period = " + maximumMillsecondsCountToWaitAfterEachIteration.TotalMilliseconds.ToString("f2") + " ms");
			
			for (int i = 0; i < timeoutInSeconds; ++i) {
				for (int j = 0; j < iterationsPerSecondCount; ++j) {
					_readEplasedTimer.Restart();

					var bytesToRead = _port.BytesToRead;

					if (bytesToRead != 0) {
						var currentReadedBytesCount = _port.Read(inBytes, totalReadedBytesCount, bytesCount - totalReadedBytesCount);
						Log("Incoming bytes now are = " + inBytes.ToText());
						totalReadedBytesCount += currentReadedBytesCount;
						Log("Total readed bytes count=" + totalReadedBytesCount);
						Log("Current readed bytes count=" + currentReadedBytesCount);

						if (totalReadedBytesCount == inBytes.Length) {
							Log("Result incoming bytes are = " + inBytes.ToText());
							Log("Discarding remaining bytes...");
							Log("Discarded bytes are: " + ReadAllBytes().ToText());
							return inBytes;
						}
					}
					_readEplasedTimer.Stop();
					var sleepTime = maximumMillsecondsCountToWaitAfterEachIteration - _readEplasedTimer.Elapsed;
					if (sleepTime.TotalMilliseconds > 0) Thread.Sleep(sleepTime);
				}
			}
			Log("Timeout, dropping all incoming bytes...");
			Log("Discarded bytes are: " + ReadAllBytes().ToText());
			Log("Rising timeout exception now");
			throw new TimeoutException("ReadFromPort timeout");
		}


		public byte[] ReadAllBytes()
		{
			var bytesToRead = _port.BytesToRead;
			var result = new byte[bytesToRead];
			_port.Read(result, 0, bytesToRead);
			return result;
		}

		private void Log(object obj) {
			_selectedLogAction(obj.ToString());
		}
	}
}
