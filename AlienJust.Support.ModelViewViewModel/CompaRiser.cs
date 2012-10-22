using System;
using System.Linq.Expressions;
using AlienJust.Support.Reflection;

namespace AlienJust.Support.ModelViewViewModel
{
	internal static class CompaRiser
	{
		/// <summary>
		/// ���������� ������� ��������� � � ����������� �� ���������� (��)���������� ��������
		/// </summary>
		/// <param name="comparer">������� ���������</param>
		/// <param name="assignAction">������ �������� � ������ ������</param>
		/// <param name="notifyPropertyChanged">������ �������� � ������ ������</param>
		/// <param name="property">��������� ��������</param>
		/// <returns>����������� ������� ����� �������� comparer</returns>
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