using System.Globalization;
using AlienJust.Support.Identy.Contracts;

namespace AlienJust.Support.Identy {
	public sealed class IdentifierStringBased : IIdentifier
	{
		private readonly string _identyString;
		public IdentifierStringBased(string identyString)
		{
			_identyString = identyString;
		}

		public string IdentyString
		{
			get { return _identyString; }
		}

		public override string ToString()
		{
			return _identyString;
		}
	}

	public sealed class IdentifierStringToLowerBased : IIdentifier {
		private readonly string _identyString;
		public IdentifierStringToLowerBased(string identyString) {
			_identyString = identyString.ToLower(CultureInfo.InvariantCulture);
		}

		public string IdentyString {
			get { return _identyString; }
		}

		public override string ToString() {
			return _identyString;
		}
	}
}