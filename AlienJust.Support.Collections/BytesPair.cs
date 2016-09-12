using System;

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
	}
}