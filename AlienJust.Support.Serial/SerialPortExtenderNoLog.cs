using System;
using System.IO.Ports;

namespace AlienJust.Support.Serial
{
	public sealed class SerialPortExtenderNoLog : ISerialPortExtender {
		private readonly SerialPort _port;

		public SerialPortExtenderNoLog(SerialPort port) {
			_port = port;
		}

		public void WriteBytes(byte[] bytes, int offset, int count) {
			_port.DiscardOutBuffer();
			ReadAllBytes();
			_port.Write(bytes, offset, count);
		}

		public byte[] ReadBytes(int bytesCount, TimeSpan timeout, bool discardRemainingBytesAfterSuccessRead) {
			var inBytes = new byte[bytesCount];
			_port.ReadTimeout = (int)timeout.TotalMilliseconds;
			try {
				_port.Read(inBytes, 0, bytesCount);
				if (discardRemainingBytesAfterSuccessRead) {
					_port.DiscardInBuffer();
					ReadAllBytes();
				}
				return inBytes;
			}
			catch (TimeoutException) {
				_port.DiscardInBuffer(); // TODO: any reason to do it? It must be empty on error or what?
				ReadAllBytes();
				throw;
			}
		}

		public byte[] ReadAllBytes() {
			var bytesToRead = _port.BytesToRead;
			var result = new byte[bytesToRead];
			_port.Read(result, 0, bytesToRead);
			return result;
		}
	}
}