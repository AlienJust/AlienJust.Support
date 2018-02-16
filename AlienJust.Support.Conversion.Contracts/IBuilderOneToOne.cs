namespace AlienJust.Support.Conversion.Contracts
{
	/// <summary>
	/// Converts single source objects to result one
	/// </summary>
	/// <typeparam name="TSource">Source object type</typeparam>
	/// <typeparam name="TResult">Result object type</typeparam>
	public interface IBuilderOneToOne<in TSource, out TResult> {
		TResult Build(TSource source);
	}
}
