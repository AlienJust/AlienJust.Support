using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AlienJust.Support.ModelViewViewModel
{
	/// <summary>
	/// Следит за изменением свойств в контексте вида модели
	/// </summary>
	public class PropertyListener
	{
		public INotifyPropertyChanged ViewModel { get; }
		public RelayCommand Command { get; }
		public string PropertyName { get; }
		public PropertyListener(RelayCommand cmd, INotifyPropertyChanged vm, string propertyName)
		{
			ViewModel = vm;
			Command = cmd;
			if (ViewModel != null)
			{
				ViewModel.PropertyChanged += ViewModelPropertyChanged;
			}
			PropertyName = propertyName;
		}

		void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == PropertyName)
			{
				Command.RaiseCanExecuteChanged();
			}
		}
	}
}