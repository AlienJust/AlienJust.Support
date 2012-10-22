using System;
using System.Collections.Generic;

namespace AlienJust.Support.ModelViewViewModel
{
	public class ViewModelProperty<TT> where TT : struct
	{
		public string Name { get; private set; }
		private readonly Action<string> _propChangeAction;

		public Dictionary<string, Action<string>> DependedProperties { get; private set; }

		private TT _value;
		public TT Value
		{
			get { return _value; }
			set
			{
				if (_value.Equals(value))
				{
					_value = value;
					if (_propChangeAction != null) _propChangeAction(this.Name);
					NotifyAllDepended();
				}
			}
		}

		public ViewModelProperty(string propertyName, TT defaultValue, Action<string> propertyChangeAction)
		{
			_value = defaultValue;
			Name = propertyName;
			_propChangeAction = propertyChangeAction;
			DependedProperties = new Dictionary<string, Action<string>>();
		}



		public void NotifyAllDepended()
		{
			foreach (var dependedProp in DependedProperties)
			{
				if (dependedProp.Value != null)
					dependedProp.Value(dependedProp.Key);
			}
		}
	}
}
