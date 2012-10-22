using System.ComponentModel;

namespace AlienJust.Support.ModelViewViewModel
{
	/// <summary>
	/// Следит за изменением свойств в контексте вида модели
	/// </summary>
	public class PropertyListener
	{
		public INotifyPropertyChanged ViewModel { get; private set; }
		public RelayCommand Command { get; private set; }
		public string PropertyName { get; private set; }
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