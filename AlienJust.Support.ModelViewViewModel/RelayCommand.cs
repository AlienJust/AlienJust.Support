using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

#if WIN8
using Windows.UI.Xaml.Input;
using EventHandler = Windows.UI.Xaml.EventHandler;
#else

#endif

////using GalaSoft.Utilities.Attributes;

namespace AlienJust.Support.ModelViewViewModel
{
	public class RelayCommand : ICommand
	{
		private readonly Action _execute;

		private readonly Func<bool> _canExecute;

		public RelayCommand(Action execute): this(execute, null)
		{
		}

		public RelayCommand(Action execute, Func<bool> canExecute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException("execute");
			}

			_execute = execute;
			_canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged;

		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
		public void RaiseCanExecuteChanged()
		{
			var handler = CanExecuteChanged;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		[DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute();
		}

		public void Execute(object parameter)
		{
			if (CanExecute(parameter))
			{
				_execute();
			}
		}
	}
}