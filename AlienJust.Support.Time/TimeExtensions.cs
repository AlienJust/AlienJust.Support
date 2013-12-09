using System;

namespace AlienJust.Support.Time {
	public static class TimeExtensions {
		public static DateTime RoundToLatestHalfAnHour(this DateTime time) {
			var minute = time.Minute > 30 ? 30 : 0;
			return new DateTime(time.Year, time.Month, time.Day, time.Hour, minute, 0);
		}
	}
}
