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
	}
}