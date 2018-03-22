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

		public static BytesQuad FromSignedIntHighFirst(int value) {
			byte uhi = (byte)((value & 0xFF000000) >> 24);
			byte ulo = (byte)((value & 0xFF0000) >> 16);
			byte hi = (byte)((value & 0xFF00) >> 8);
			byte lo = (byte)(value & 0xFF);
			return new BytesQuad(uhi, ulo, hi, lo);
		}

		public static BytesQuad FromSignedIntLowFirst(int value) {
			byte uhi = (byte)((value & 0xFF000000) >> 24);
			byte ulo = (byte)((value & 0xFF0000) >> 16);
			byte hi = (byte)((value & 0xFF00) >> 8);
			byte lo = (byte)(value & 0xFF);
			return new BytesQuad(lo, hi, ulo, uhi);
		}

		public static BytesQuad FromUnsignedIntHighFirst(uint value) {
			byte uhi = (byte)((value & 0xFF000000) >> 24);
			byte ulo = (byte)((value & 0xFF0000) >> 16);
			byte hi = (byte)((value & 0xFF00) >> 8);
			byte lo = (byte)(value & 0xFF);
			return new BytesQuad(uhi, ulo, hi, lo);
		}

		public static BytesQuad FromUnsignedIntLowFirst(uint value) {
			byte uhi = (byte)((value & 0xFF000000) >> 24);
			byte ulo = (byte)((value & 0xFF0000) >> 16);
			byte hi = (byte)((value & 0xFF00) >> 8);
			byte lo = (byte)(value & 0xFF);
			return new BytesQuad(lo, hi, ulo, uhi);
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

		/// <summary>
		/// Возвращает BCD значение структуры  считая первый байт старшим
		/// </summary>
		public int HighFirstBcd => First.ToBcdInteger() * 1000000 + Second.ToBcdInteger() * 10000 + Third.ToBcdInteger() * 100 + Fourth.ToBcdInteger();

		/// <summary>
		/// Возвращает BCD значение структуры  считая первый байт младшим
		/// </summary>
		public int LowFirstBcd => Fourth.ToBcdInteger() * 1000000 + Third.ToBcdInteger() * 10000 + Second.ToBcdInteger() * 100 + First.ToBcdInteger();

		/// <summary>
		/// Создаёт структуру из BCD числа считая первый байт старшим
		/// </summary>
		/// <param name="bcdValueHf">BCD значение</param>
		/// <returns>Новая структура</returns>
		public static BytesQuad ToBcdHighFirst(int bcdValueHf) {
			if (bcdValueHf < 0 || bcdValueHf > 99999999) throw new ArgumentException();
			int bcd = 0;
			for (int digit = 0; digit < 8; ++digit) {
				int nibble = bcdValueHf % 10;
				bcd |= nibble << (digit * 4);
				bcdValueHf /= 10;
			}
			return new BytesQuad((byte)((bcd >> 24) & 0xff), (byte)((bcd >> 16) & 0xff), (byte)((bcd >> 8) & 0xff), (byte)(bcd & 0xff));
		}

		/// <summary>
		/// Создаёт структуру из BCD числа считая первый байт младшим
		/// </summary>
		/// <param name="bcdValueLf">BCD значение</param>
		/// <returns>Новая структура</returns>
		public static BytesQuad ToBcdLowFirst(int bcdValueLf) {
			if (bcdValueLf < 0 || bcdValueLf > 99999999) throw new ArgumentException();
			int bcd = 0;
			for (int digit = 0; digit < 8; ++digit) {
				int nibble = bcdValueLf % 10;
				bcd |= nibble << (digit * 4);
				bcdValueLf /= 10;
			}
			return new BytesQuad((byte)(bcd & 0xff), (byte)((bcd >> 8) & 0xff), (byte)((bcd >> 16) & 0xff), (byte)((bcd >> 24) & 0xff));
		}
	}
}