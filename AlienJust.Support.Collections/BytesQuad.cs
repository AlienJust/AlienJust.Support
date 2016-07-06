using System;

namespace AlienJust.Support.Collections {
	/// <summary>
	/// ���� ������
	/// </summary>
	public struct BytesQuad {
		/// <summary>
		/// ������ ����
		/// </summary>
		public byte First { get; }

		/// <summary>
		/// ������ ����
		/// </summary>
		public byte Second { get; }

		/// <summary>
		/// ������ ����
		/// </summary>
		public byte Third { get; }

		/// <summary>
		/// ��������� ����
		/// </summary>
		public byte Fourth { get; }


		public BytesQuad(byte first, byte second, byte third, byte fourth) {
			First = first;
			Second = second;
			Third = third;
			Fourth = fourth;
		}

		/// <summary>
		/// ���������� ������ ���� �������� ������� �������� ������ �� ����������� ������� ���������� (��-�� ��� ��-��) ��� ����������� ��������������� ��� ������ BitConverter
		/// </summary>
		/// <param name="first">������ ����</param>
		/// <param name="second">������ ����</param>
		/// <param name="third">������ ����</param>
		/// <param name="fourth">��������� ����</param>
		/// <returns>������ ��� ���������� ������ � BitConverter</returns>
		public static byte[] GetArrayForBitConverterAccodringToCurrentArchitectureEndian(byte first, byte second, byte third, byte fourth) {
			if (BitConverter.IsLittleEndian) {
				return new[] { fourth, third, second, first };
			}
			return new[] { first, second, third, fourth };
		}

		/// <summary>
		/// ���������� �������� ��������� ��� ����������� ����������� ����� ������ ������ ���� �������
		/// </summary>
		public uint HighFirstUnsignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(First, Second, Third, Fourth);
				return BitConverter.ToUInt32(tempByteArray, 0);
			}
		}

		/// <summary>
		/// ���������� �������� ��������� ��� �������� ����������� ����� ������ ������ ���� �������
		/// </summary>
		public int HighFirstSignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(First, Second, Third, Fourth);
				return BitConverter.ToInt32(tempByteArray, 0);
			}
		}


		/// <summary>
		/// ���������� �������� ��������� ��� ����������� ����������� ����� ������ ������ ���� �������
		/// </summary>
		public uint LowFirstUnsignedValue {
			get {
				var tempByteArray = GetArrayForBitConverterAccodringToCurrentArchitectureEndian(Fourth, Third, Second, First);
				return BitConverter.ToUInt32(tempByteArray, 0);
			}
		}

		/// <summary>
		/// ���������� �������� ��������� ��� �������� ����������� ����� ������ ������ ���� �������
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