namespace AlienJust.Support.Identy.Contracts {
	/// <summary>
	/// Idenstifier, allows to identy something (now only  by string comparision)
	/// </summary>
	public interface IIdentifier
	{
		string IdentyString { get; }
	}
}