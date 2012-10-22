using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlienJust.Support.ModelViewViewModel
{
	/// <summary>
	/// Based on RelayCommand
	/// </summary>
	public class DependedCommand : RelayCommand
	{
		/// <summary>
		/// Свойства от которых зависит состояние команды
		/// </summary>
		public List<PropertyListener> DependOnProperties { get; private set; }

		/// <summary>
		/// Зависимые команды
		/// </summary>
		public List<RelayCommand> DependedCommands { get; private set; }

		public DependedCommand(Action execute, Func<bool> canExecute)
			: base(execute, canExecute)
		{
			DependOnProperties = new List<PropertyListener>();
			DependedCommands = new List<RelayCommand>();
		}

		public void AddDependOnProp(INotifyPropertyChanged vm, string propertyName)
		{
			DependOnProperties.Add(new PropertyListener(this, vm, propertyName));
		}

		/// <summary>
		/// Проверяет зависимые команды на предмет изменения возможности выполнения + проверяет возможность своего исполнения
		/// </summary>
		public void NotifyCommands()
		{
			foreach (var cmd in DependedCommands)
			{
				cmd.RaiseCanExecuteChanged();
			}
			RaiseCanExecuteChanged();
		}
	}
}
