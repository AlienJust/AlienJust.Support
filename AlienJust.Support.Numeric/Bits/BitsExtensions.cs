namespace AlienJust.Support.Numeric.Bits {
	public static class BitsExtensions {
		public static bool GetBit(this byte b, int bitNumber) {
			return (b & (1 << bitNumber)) != 0;
		}

		public static bool GetBit(this sbyte b, int bitNumber) {
			return (b & (1 << bitNumber)) != 0;
		}

		public static bool GetBit(this short b, int bitNumber) {
			return (b & (1 << bitNumber)) != 0;
		}

		public static bool GetBit(this ushort b, int bitNumber) {
			return (b & (1 << bitNumber)) != 0;
		}

		public static bool GetBit(this int b, int bitNumber) {
			return (b & (1 << bitNumber)) != 0;
		}

		public static bool GetBit(this uint b, int bitNumber) {
			return (b & (1 << bitNumber)) != 0;
		}

		public static bool GetBit(this long b, int bitNumber) {
			return (b & (1 << bitNumber)) != 0;
		}
	}
}