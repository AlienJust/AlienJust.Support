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
}