using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AlienJust.Support.ModelViewViewModel
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public void SetProp<T>(Func<bool> comparer, Action assignAction, Expression<Func<T>> property)
		{
			CompaRiser.CompaRise(comparer, assignAction, RaisePropertyChanged, property);
		}

		public string GetPropName<T>(Expression<Func<T>> property) {
			return CompaRiser.GetPropName(property);
		}
	}
}
