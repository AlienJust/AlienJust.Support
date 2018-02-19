using AlienJust.Support.Conversion.Contracts;

namespace AlienJust.Support.Conversion
{
	public sealed class RawAndConvertedValues<TRaw, TConverted> {
		private readonly IBuilderOneToOne<TRaw, TConverted> _builder;
		public TRaw RawValue { get; }
		public RawAndConvertedValues(TRaw rawValue, IBuilderOneToOne<TRaw, TConverted> builder) {
			_builder = builder;
			RawValue = rawValue;
		}
		public TConverted ConvertedValue => _builder.Build(RawValue);
	}
}
