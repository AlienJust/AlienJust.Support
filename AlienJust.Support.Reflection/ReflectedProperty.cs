using System;
using System.Linq.Expressions;

namespace AlienJust.Support.Reflection
{
	/// <summary>
	/// Позволяет извлекать полезную информацию о свойствах классов
	/// </summary>
	public static class ReflectedProperty
	{
		public static string GetName<T>(Expression<Func<T>> property)
		{
			var expression = GetMemberInfo(property);
			return expression.Member.Name;
		}

		public static string GetFullName<T>(Expression<Func<T>> property)
		{
			var expression = GetMemberInfo(property);
			return string.Concat(expression.Member.DeclaringType.FullName, ".", expression.Member.Name);
		}

		private static MemberExpression GetMemberInfo(Expression method)
		{
			var lambda = method as LambdaExpression;
			if (lambda == null)
				throw new ArgumentNullException("method");

			MemberExpression memberExpr = null;

			if (lambda.Body.NodeType == ExpressionType.Convert)
			{
				memberExpr =
					((UnaryExpression)lambda.Body).Operand as MemberExpression;
			}
			else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
			{
				memberExpr = lambda.Body as MemberExpression;
			}

			if (memberExpr == null)
				throw new ArgumentException("method");

			return memberExpr;
		}
	}
}