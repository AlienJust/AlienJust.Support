using System;
using System.Linq.Expressions;
using AlienJust.Support.Reflection;

namespace AlienJust.Support.ModelViewViewModel
{
	internal static class CompaRiser
	{
		/// <summary>
		/// Производит внешнее сравнение и в зависимости от результата (не)производит действия
		/// </summary>
		/// <param name="comparer">Функтор сравнения</param>
		/// <param name="assignAction">Первое действие в случае успеха</param>
		/// <param name="notifyPropertyChanged">Второе действие в случае успеха</param>
		/// <param name="property">Выражение свойства</param>
		/// <returns>Результатом явлется выход функтора comparer</returns>
		public static bool CompaRise<T>(Func<bool> comparer, Action assignAction, Action<string> notifyPropertyChanged, Expression<Func<T>> property)
		{
			var result = comparer.Invoke();
			if (result)
			{
				assignAction.Invoke();
				notifyPropertyChanged.Invoke(ReflectedProperty.GetName(property));
			}
			return result;
		}
	}
}