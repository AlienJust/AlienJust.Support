using System;

namespace AlienJust.Support.Collections {
	/// <summary>
	/// Пара байтов
	/// </summary>
	public struct BytesQuad {
		/// <summary>
		/// Первый байт
		/// </summary>
		public byte First { get; }

		/// <summary>
		/// Второй байт
		/// </summary>
		public byte Second { get; }

		/// <summary>
		/// Третий байт
		/// </summary>
		public byte Third { get; }

		/// <summary>
		/// Четвертый байт
		/// </summary>
		public byte Fourth { get; }


		public BytesQuad(byte first, byte second, byte third, byte fourth) {
			First = first;
			Second = second;
			Third = third;
			Fourth = fourth;
		}

		/// <summary>
		/// Возвращяет массив байт согласно формату хранения данных на архитектуре данного процессора (Ст-мл или Мл-Ст) для дальнейшего конвертирования при помощи BitConverter
		/// </summary>
		/// <param name="first">Первый байт</param>
		/// <param name="second">Второй байт</param>
		/// <param name="third">Третий байт</param>
		/// <param name="fourth">Четвертый байт</param>
		/// <returns>Массив для дальнейшей работы с BitConverter</returns>
		public static byte[] GetArrayForBitConverterAccodringToCurrentArchitectureEndian(byte first, byte second, byte third, byte fourth) {
			if (BitConverter.IsLittleEndian) {
				return new[] { fourth, third, second, first };
			}
			return new[] { first, second, third, fourth };
		}

		/// <summary>
		/// Возвращает значение структуры как беззнаковое двухбайтное число считая первый байт старшим
		/// </summary>
		public uint HighFirstUnsignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(First, Second, Third, Fourth);
				return BitConverter.ToUInt32(tempByteArray, 0);
			}
		}

		/// <summary>
		/// Возвращает значение структуры как знаковое двухбайтное число считая первый байт старшим
		/// </summary>
		public int HighFirstSignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(First, Second, Third, Fourth);
				return BitConverter.ToInt32(tempByteArray, 0);
			}
		}


		/// <summary>
		/// Возвращает значение структуры как беззнаковое двухбайтное число считая первый байт младшим
		/// </summary>
		public uint LowFirstUnsignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(Fourth, Third, Second, First);
				return BitConverter.ToUInt32(tempByteArray, 0);
			}
		}

		/// <summary>
		/// Возвращает значение структуры как знаковое двухбайтное число считая первый байт младшим
		/// </summary>
		public int LowFirstSignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(Fourth, Third, Second, First);
				return BitConverter.ToInt32(tempByteArray, 0);
			}
		}

		public override bool Equals(object obj) {
			return obj is BytesQuad && this == (BytesQuad)obj;
		}
		public override int GetHashCode() {
			return First.GetHashCode() ^ Second.GetHashCode();
		}
		public static bool operator ==(BytesQuad x, BytesQuad y) {
			return x.First == y.First && x.Second == y.Second && x.Third == y.Third && x.Fourth == y.Fourth;
		}
		public static bool operator !=(BytesQuad x, BytesQuad y) {
			return !(x == y);
		}
	}
}