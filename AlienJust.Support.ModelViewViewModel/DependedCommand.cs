using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

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
		public List<PropertyListener> DependOnProperties { get; }

		/// <summary>
		/// Зависимые команды
		/// </summary>
		public List<RelayCommand> DependedCommands { get; }

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
		//
		public void AddDependOnProp<T>(INotifyPropertyChanged vm, Expression<Func<T>> property) {
			DependOnProperties.Add(new PropertyListener(this, vm, CompaRiser.GetPropName(property)));
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
