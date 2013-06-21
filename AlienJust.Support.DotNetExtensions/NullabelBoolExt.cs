using System;

namespace AlienJust.Support.DotNetExtensions {
	public static class NullabelBoolExt {
		public static bool IsNotNullAndTrue(this bool? check) {
			return check.HasValue && check.Value;
		}

		public static bool IsNotNullAndFalse(this bool? check) {
			return check.HasValue && !check.Value;
		}

		public static bool IsNullOrFalse(this bool? check) {
			return !check.HasValue || check.Value == false;
		}
	}

	public static class ObjectExt {
		public static bool IsNotNullAndPredecate(this object obj, Func<bool> predecate) {
			return obj != null && predecate();
		}
	}
}
