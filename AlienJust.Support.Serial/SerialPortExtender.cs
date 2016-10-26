using System;
using System.Diagnostics;
using System.Threading;
using System.IO.Ports;
using AlienJust.Support.Text;

namespace AlienJust.Support.Serial {
	public sealed class SerialPortExtender : ISerialPortExtender {
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
			_port.Write(bytes, offset, count);
			Log("В порт отправлены байты буфера: " + bytes.ToText() + " начиная с " + offset + " в количестве " + count);
		}

		public byte[] ReadBytes(int bytesCount, TimeSpan timeout, bool discardRemainingBytesAfterSuccessRead) {
			var inBytes = new byte[bytesCount];
			int totalReadedBytesCount = 0;

			TimeSpan beetweenIterationPause = TimeSpan.FromMilliseconds(25);
			var totalIterationsCount = (int)(timeout.TotalMilliseconds / beetweenIterationPause.TotalMilliseconds);

			Log("Iteration period = " + beetweenIterationPause.TotalMilliseconds.ToString("f2") + " ms, max iterations count = " + totalIterationsCount);

			for (int i = 0; i < totalIterationsCount; ++i) {
				Log("Iteration number = " + i);
				_readEplasedTimer.Restart();
				var bytesToRead = _port.BytesToRead;
				if (bytesToRead != 0) {
					var currentReadedBytesCount = _port.Read(inBytes, totalReadedBytesCount, bytesCount - totalReadedBytesCount);
					Log("Incoming bytes now are = " + inBytes.ToText());
					totalReadedBytesCount += currentReadedBytesCount;
					Log("Total readed bytes count=" + totalReadedBytesCount + ", current readed bytes count=" + currentReadedBytesCount);

					if (totalReadedBytesCount == inBytes.Length) {
						Log("Result incoming bytes are = " + inBytes.ToText());
						if (discardRemainingBytesAfterSuccessRead) {
							Log("Discarding remaining bytes, discarded bytes are: " + ReadAllBytes().ToText());
						}
						return inBytes;
					}
				}
				_readEplasedTimer.Stop();
				var sleepTime = beetweenIterationPause - _readEplasedTimer.Elapsed;
				if (sleepTime.TotalMilliseconds > 0) Thread.Sleep(sleepTime);
			}
			Log("Timeout, dropping all incoming bytes...");
			Log("Discarded bytes are: " + ReadAllBytes().ToText());
			Log("Rising timeout exception now");
			throw new TimeoutException("ReadFromPort timeout");
		}

		public byte[] ReadAllBytes() {
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
