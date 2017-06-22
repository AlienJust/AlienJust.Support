using System;
using System.Globalization;

namespace AlienJust.Support.Collections {
	/// <summary>
	/// Пара байтов
	/// </summary>
	public struct BytesPair {
		/// <summary>
		/// Первый байт
		/// </summary>
		public byte First { get; }

		/// <summary>
		/// Второй байт
		/// </summary>
		public byte Second { get; }

		public BytesPair(byte first, byte second) {
			First = first;
			Second = second;
		}

		/// <summary>
		/// Возвращяет массив байт согласно формату хранения данных на архитектуре данного процессора (Ст-мл или Мл-Ст) для дальнейшего конвертирования при помощи BitConverter
		/// </summary>
		/// <param name="first">Первый байт</param>
		/// <param name="second">Второй байт</param>
		/// <returns>Массив для дальнейшей работы с BitConverter</returns>
		public static byte[] GetArrayForBitConverterAccodringToCurrentArchitectureEndian(byte first, byte second) {
			if (BitConverter.IsLittleEndian) {
				return new[] { second, first };
			}
			return new[] { first, second };
		}

		/// <summary>
		/// Возвращает значение структуры как беззнаковое двухбайтное число считая первый байт старшим
		/// </summary>
		public ushort HighFirstUnsignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(First, Second);
				return BitConverter.ToUInt16(tempByteArray, 0);
			}
		}

		/// <summary>
		/// Возвращает значение структуры как знаковое двухбайтное число считая первый байт старшим
		/// </summary>
		public short HighFirstSignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(First, Second);
				return BitConverter.ToInt16(tempByteArray, 0);
			}
		}

		/// <summary>
		/// Возвращает значение структуры как беззнаковое двухбайтное число считая первый байт младшим
		/// </summary>
		public ushort LowFirstUnsignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(Second, First);
				return BitConverter.ToUInt16(tempByteArray, 0);
			}
		}

		/// <summary>
		/// Возвращает значение структуры как знаковое двухбайтное число считая первый байт младшим
		/// </summary>
		public short LowFirstSignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(Second, First);
				return BitConverter.ToInt16(tempByteArray, 0);
			}
		}

		public override bool Equals(object obj) {
			return obj is BytesPair && this == (BytesPair)obj;
		}
		public override int GetHashCode() {
			return First.GetHashCode() ^ Second.GetHashCode();
		}
		public static bool operator ==(BytesPair x, BytesPair y) {
			return x.First == y.First && x.Second == y.Second;
		}
		public static bool operator !=(BytesPair x, BytesPair y) {
			return !(x == y);
		}

		public static BytesPair FromSignedShortHighFirst(short value) {
			byte hi = (byte)((value & 0xFF00) >> 8);
			byte lo = (byte) (value & 0xFF);
			return new BytesPair(hi, lo);
		}

		public static BytesPair FromSignedShortLowFirst(short value) {
			byte hi = (byte)((value & 0xFF00) >> 8);
			byte lo = (byte)(value & 0xFF);
			return new BytesPair(lo, hi);
		}

		public static BytesPair FromUnsignedShortHighFirst(ushort value) {
			byte hi = (byte)((value & 0xFF00) >> 8);
			byte lo = (byte)(value & 0xFF);
			return new BytesPair(hi, lo);
		}

		public static BytesPair FromUnsignedShortLowFirst(ushort value) {
			byte hi = (byte)((value & 0xFF00) >> 8);
			byte lo = (byte)(value & 0xFF);
			return new BytesPair(lo, hi);
		}

		/// <summary>
		/// Создаёт структуру из BCD числа считая первый байт старшим
		/// </summary>
		/// <param name="bcdValueHf">BCD значение</param>
		/// <returns>Новая структура</returns>
		public static BytesPair FromBcdHighFirst(int bcdValueHf) {
			if (bcdValueHf < 0 || bcdValueHf > 9999) throw new ArgumentException();
			int bcd = 0;
			for (int digit = 0; digit < 4; ++digit) {
				int nibble = bcdValueHf % 10;
				bcd |= nibble << (digit * 4);
				bcdValueHf /= 10;
			}
			return new BytesPair((byte)((bcd >> 8) & 0xff), (byte)(bcd & 0xff) );
		}

		/// <summary>
		/// Создаёт структуру из BCD числа считая первый байт младшим
		/// </summary>
		/// <param name="bcdValueLf">BCD значение</param>
		/// <returns>Новая структура</returns>
		public static BytesPair FromBcdLowFirst(int bcdValueLf) {
			if (bcdValueLf < 0 || bcdValueLf > 9999) throw new ArgumentException();
			int bcd = 0;
			for (int digit = 0; digit < 4; ++digit) {
				int nibble = bcdValueLf % 10;
				bcd |= nibble << (digit * 4);
				bcdValueLf /= 10;
			}
			return new BytesPair((byte)(bcd & 0xff), (byte)((bcd >> 8) & 0xff));
		}

		public override string ToString()
		{
			return First.ToString("X2") + Second.ToString("X2");
		}

		public static BytesPair Parse(string value)
		{
			if (value == null) throw new NullReferenceException("Input string must be not null");
			if (value.Length != 4) throw new Exception("Supported length of the string is 4");
			var first = byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber);
			var second = byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber);
			return new BytesPair(first, second);
		}

		public int HighFirstBcd => First.ToBcdInteger() * 1000 + Second.ToBcdInteger();
		public int LowFirstBcd => Second.ToBcdInteger() * 1000 + First.ToBcdInteger();
	}

	public static class ByteExtensions
	{
		public static int ToBcdInteger(this byte currentByte)
		{
			int high = currentByte >> 4;
			int low = currentByte & 0xF;
			return 10 * high + low;
		}
	}
}