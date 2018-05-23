namespace AlienJust.Support.Collections
{
	public static class ByteExtensions
	{
		public static int AsBcd(this byte bcdValueByte)
		{
			int high = bcdValueByte >> 4;
			int low = bcdValueByte & 0xF;
			return 10 * high + low;
		}
	}
}